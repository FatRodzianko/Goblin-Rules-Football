using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws the game through the CRT effect, UI included.
///
/// The world camera and a second, static UI camera each render into their own full-window texture.
/// One full-screen image then composites the two and applies the effect. The UI therefore never
/// enters the world camera's pipeline, which is what keeps post-processing off it and keeps it out
/// of reach of the render pass events the effect used to depend on.
///
/// The textures are the size of the window on purpose. Each camera's Pixel Perfect Camera still does
/// its own internal render at an integer multiple of the reference resolution and its own stretch, so
/// each texture already holds exactly the picture that camera would have put on the screen. The
/// composite is then 1:1 and adds no resampling of its own. It also leaves both cameras' pixelRect in
/// screen space, so pointer coordinates need no translation anywhere.
///
/// Canvases are converted here at runtime rather than being authored converted, so scenes stay
/// exactly as they are to edit and a new canvas needs no setup.
/// </summary>
public class CRTPresenter : MonoBehaviour
{
    public static CRTPresenter Instance { get; private set; }

    [SerializeField] private Material _presentMaterial;
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private bool _effectEnabled = true;
    [Tooltip("Layer the UI camera draws, and the layer removed from the world camera.")]
    [SerializeField] private int _uiLayer = 5;
    [Tooltip("Sorting layer every converted canvas is put on, so order alone decides what is in front.")]
    [SerializeField] private string _uiSortingLayer = "ui-markers";
    [Tooltip("Seconds between checks for canvases that were spawned after startup.")]
    [SerializeField] private float _canvasScanInterval = 0.5f;

    private Camera _uiCamera;
#if UNITY_EDITOR
    private Camera _displayOwner;
#endif
    private Canvas _presentCanvas;
    private RawImage _presentImage;
    private Material _runtimeMaterial;
    private RenderTexture _worldTexture;
    private RenderTexture _uiTexture;
    private int _builtWidth;
    private int _builtHeight;
    private int _worldCullingMask;
    private bool _started;
    private int _overlayBaseOrder = -1;
    private float _nextCanvasScan;
    private readonly List<CanvasState> _convertedCanvases = new List<CanvasState>();
    private readonly Dictionary<Transform, int> _originalLayers = new Dictionary<Transform, int>();
    private readonly List<CanvasRenderer> _layerScratch = new List<CanvasRenderer>();

    private struct CanvasState
    {
        public Canvas Canvas;
        public RenderMode Mode;
        public Camera Camera;
        public float PlaneDistance;
        public int SortingLayerId;
        public int SortingOrder;
    }

    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int UITexId = Shader.PropertyToID("_UITex");
    private static readonly int EffectOnId = Shader.PropertyToID("_EffectOn");

    public bool EffectEnabled { get { return _effectEnabled; } }

    private void Awake()
    {
        Instance = this;
        // The camera is resolved in Build, not here: it has to come from this presenter's own
        // scene, and during a transition the other scene's camera is still around to be found.
    }

    private void Start()
    {
        _started = true;
        Build();
    }

    private void OnEnable()
    {
        // The first build has to wait for Start. A Pixel Perfect Camera works its size out in its
        // own Awake, and one component's OnEnable can run before another component's Awake has.
        if (_started)
            Build();
    }

    private void Build()
    {
        if (_worldCamera == null)
            _worldCamera = FindWorldCamera();
        if (_worldCamera == null)
        {
            Debug.LogWarning("CRTPresenter: no world camera, the effect will not run.");
            enabled = false;
            return;
        }

        _worldCullingMask = _worldCamera.cullingMask;
        try
        {
            BuildRig();
            ConvertCanvases();
            EnforceUILayers();
            AllocateTextures();
        }
        catch (System.Exception e)
        {
            // Half built is the worst outcome: the UI camera would clear the screen with nothing to
            // draw into, which reads as the game having hung. Undo it and draw without the effect.
            Debug.LogError("CRTPresenter: could not set up, drawing without the effect instead. " + e);
            Teardown();
            enabled = false;
            return;
        }

        // The setting is applied long before this scene loads, when there is no presenter to take
        // it, so the manager only remembered it. Re-applying it now that the rig exists puts it here.
        if (RenderFeaturesManager.instance != null)
            RenderFeaturesManager.instance.EnableRetroCRT(RenderFeaturesManager.instance.RetroCRTEnabled);
        else
            SetEffectEnabled(_effectEnabled);
    }

    /// <summary>Turns the effect on or off without changing how anything is rendered or composited.</summary>
    public void SetEffectEnabled(bool value)
    {
        _effectEnabled = value;
        if (_runtimeMaterial != null)
            _runtimeMaterial.SetFloat(EffectOnId, value ? 1f : 0f);
    }

    private void OnDisable()
    {
        Teardown();
    }

    private void Teardown()
    {
        // Leave the cameras able to draw to the screen on their own, so disabling this never blanks
        // the game.
        if (_worldCamera != null)
        {
            _worldCamera.targetTexture = null;
            _worldCamera.cullingMask = _worldCullingMask;
        }
        if (_uiCamera != null)
        {
            // Off, not just untargeted: a camera with no target draws over the screen and clears it.
            _uiCamera.targetTexture = null;
            _uiCamera.enabled = false;
        }
        if (_presentCanvas != null)
            _presentCanvas.gameObject.SetActive(false);
#if UNITY_EDITOR
        // Essential, not tidiness: teardown hands the display back to the world camera, and a clear
        // still running above it would paint over everything that camera draws.
        if (_displayOwner != null)
            _displayOwner.enabled = false;
#endif

        RestoreCanvases();
        RestoreLayers();
        ReleaseTextures();
    }

    /// <summary>
    /// Puts the canvases back exactly as the scene has them, so turning this off leaves a working
    /// game rather than UI pointed at a camera that is no longer drawing.
    /// </summary>
    private void RestoreCanvases()
    {
        for (int i = 0; i < _convertedCanvases.Count; i++)
        {
            CanvasState state = _convertedCanvases[i];
            if (state.Canvas == null)
                continue;
            // Sorting first: a Canvas ignores a sortingLayerID written while it is in Overlay mode,
            // so putting the render mode back before the layer would silently lose it.
            state.Canvas.sortingLayerID = state.SortingLayerId;
            state.Canvas.sortingOrder = state.SortingOrder;
            state.Canvas.renderMode = state.Mode;
            state.Canvas.worldCamera = state.Camera;
            state.Canvas.planeDistance = state.PlaneDistance;
        }
        _convertedCanvases.Clear();
    }

    /// <summary>
    /// Puts everything a converted canvas draws onto the UI layer, because the UI camera culls to
    /// that layer alone. A canvas graphic left on any other layer is invisible to it, and the world
    /// camera draws it instead - at whatever world position the canvas's camera-space placement
    /// implies, which is nowhere useful. A canvas authored on the Default layer therefore vanishes
    /// the moment it is converted, however correct it looked in Overlay.
    ///
    /// Only objects carrying a CanvasRenderer are touched. That is exactly the set the canvas draws,
    /// so this can never catch a sprite, a collider or a camera-following world object that happens
    /// to be parented under the UI and would break if its layer moved.
    /// </summary>
    private void EnforceUILayers()
    {
        for (int i = 0; i < _convertedCanvases.Count; i++)
        {
            Canvas canvas = _convertedCanvases[i].Canvas;
            if (canvas == null)
                continue;

            MoveToUILayer(canvas.transform);
            canvas.GetComponentsInChildren(true, _layerScratch);
            for (int j = 0; j < _layerScratch.Count; j++)
            {
                CanvasRenderer graphic = _layerScratch[j];
                if (graphic != null)
                    MoveToUILayer(graphic.transform);
            }
        }
        _layerScratch.Clear();
    }

    private void MoveToUILayer(Transform target)
    {
        if (target.gameObject.layer == _uiLayer)
            return;
        // First time only. Recording again on a later sweep would store the UI layer as the
        // original, and restoring would then leave everything converted.
        if (!_originalLayers.ContainsKey(target))
            _originalLayers.Add(target, target.gameObject.layer);
        target.gameObject.layer = _uiLayer;
    }

    private void RestoreLayers()
    {
        foreach (KeyValuePair<Transform, int> entry in _originalLayers)
        {
            if (entry.Key != null)
                entry.Key.gameObject.layer = entry.Value;
        }
        _originalLayers.Clear();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        SafeDestroy(_runtimeMaterial);
    }

    private void Update()
    {
        if (_builtWidth != Screen.width || _builtHeight != Screen.height)
            AllocateTextures();

        // Canvases that arrive later - a spawned player, a scoreboard, an escape menu built from a
        // prefab - would otherwise never be converted and would composite over the effect instead of
        // under it. Unscaled time, because the loading handshake pauses the game with timeScale 0.
        if (Time.unscaledTime >= _nextCanvasScan)
        {
            _nextCanvasScan = Time.unscaledTime + _canvasScanInterval;
            ConvertCanvases();
            // Separately from the conversion: a canvas that was converted long ago still gains
            // children at runtime, and each one arrives on whatever layer its prefab carries.
            EnforceUILayers();
        }
    }

    /// <summary>
    /// The camera this presenter is responsible for, which is the one in its own scene.
    ///
    /// Camera.main is not good enough. Scenes here load through FishNet, which brings the new scene
    /// up and only then unloads the old one, so for a moment two scenes - and two presenters - are
    /// live. Camera.main would hand the new scene's presenter the old scene's camera.
    /// </summary>
    private Camera FindWorldCamera()
    {
        foreach (Camera candidate in FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (candidate.gameObject.scene != gameObject.scene)
                continue;
            if (candidate.GetComponentInParent<CRTPresenter>(true) != null)
                continue;   // somebody's UI camera, not a world camera
            if (candidate.CompareTag("MainCamera"))
                return candidate;
        }
        return Camera.main;
    }

    private void BuildRig()
    {
        _worldCamera.cullingMask = _worldCullingMask & ~(1 << _uiLayer);

        if (_uiCamera == null)
        {
            var uiGo = new GameObject("CRT UI Camera");
            uiGo.transform.SetParent(transform, false);
            _uiCamera = uiGo.AddComponent<Camera>();
            _uiCamera.orthographic = true;
            _uiCamera.cullingMask = 1 << _uiLayer;
            _uiCamera.clearFlags = CameraClearFlags.SolidColor;
            _uiCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _uiCamera.nearClipPlane = _worldCamera.nearClipPlane;
            _uiCamera.farClipPlane = _worldCamera.farClipPlane;
            _uiCamera.depth = _worldCamera.depth + 1;
            _uiCamera.allowHDR = false;
            _uiCamera.allowMSAA = false;

            var uiData = _uiCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (uiData == null)
                uiData = uiGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            uiData.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;
            uiData.renderPostProcessing = false;

            // The UI camera gets its own Pixel Perfect so UI still rasterises at a whole multiple of
            // the reference resolution. Copied from the world camera so a scene with a different
            // reference resolution still matches itself.
            var worldPP = _worldCamera.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
            if (worldPP != null)
            {
                var uiPP = uiGo.AddComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
                uiPP.assetsPPU = worldPP.assetsPPU;
                uiPP.refResolutionX = worldPP.refResolutionX;
                uiPP.refResolutionY = worldPP.refResolutionY;
                uiPP.cropFrame = worldPP.cropFrame;
                uiPP.gridSnapping = worldPP.gridSnapping;

                // Pixel Perfect only writes orthographicSize when the camera renders, but a
                // Screen Space - Camera canvas sizes itself against the camera the moment it is
                // pointed at one, so the size has to be right before the canvases are converted.
                //
                // It has to come from the world camera's component. A Pixel Perfect Camera works
                // its size out in Awake, which has already run by the time these properties are
                // assigned, so the new one is still holding the size for its default 320x180 at
                // 100 PPU and will not recompute until it first renders.
                _uiCamera.orthographicSize = worldPP.orthographicSize;
            }
            else
            {
                _uiCamera.orthographicSize = _worldCamera.orthographicSize;
            }
        }

        if (_presentCanvas == null)
        {
            var canvasGo = new GameObject("CRT Present");
            canvasGo.transform.SetParent(transform, false);
            _presentCanvas = canvasGo.AddComponent<Canvas>();
            _presentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _presentCanvas.sortingOrder = short.MaxValue;

            var imageGo = new GameObject("CRT Present Image");
            imageGo.transform.SetParent(canvasGo.transform, false);
            _presentImage = imageGo.AddComponent<RawImage>();
            _presentImage.raycastTarget = false;
            var rect = _presentImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        _presentCanvas.gameObject.SetActive(true);
        _uiCamera.enabled = true;

        if (_runtimeMaterial == null && _presentMaterial != null)
        {
            _runtimeMaterial = new Material(_presentMaterial);
            _presentImage.material = _runtimeMaterial;
        }

#if UNITY_EDITOR
        BuildDisplayOwner();
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor only. The Game view prints "Display 1 - No cameras rendering" whenever no enabled
    /// camera targets the display, and with both of ours rendering into textures, none does. The
    /// picture is still right - it reaches the screen through the Overlay canvas, which needs no
    /// camera - but the notice sits over the middle of the Game view while play testing.
    ///
    /// This camera exists only to be the one that owns the display. It culls every layer, so it
    /// draws nothing; all it contributes is the clear. A player has no Game view, and nothing in a
    /// build depends on the clear - the composite covers all four corners and its shader pass is
    /// Blend Off, so every pixel is written whatever the backbuffer held - so it is compiled out.
    /// </summary>
    private void BuildDisplayOwner()
    {
        if (_displayOwner == null)
        {
            var ownerGo = new GameObject("CRT Display Owner");
            ownerGo.transform.SetParent(transform, false);
            _displayOwner = ownerGo.AddComponent<Camera>();
            _displayOwner.orthographic = true;
            _displayOwner.cullingMask = 0;
            _displayOwner.clearFlags = CameraClearFlags.SolidColor;
            _displayOwner.backgroundColor = Color.black;
            _displayOwner.allowHDR = false;
            _displayOwner.allowMSAA = false;
            _displayOwner.useOcclusionCulling = false;

            var ownerData = ownerGo.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (ownerData == null)
                ownerData = ownerGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            ownerData.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;
            ownerData.renderPostProcessing = false;
            ownerData.renderShadows = false;
        }

        // Last of the three. Nothing else draws to the display, so this decides nothing today; it
        // keeps the clear from ever landing on top of a camera that does.
        _displayOwner.depth = _uiCamera.depth + 1;
        _displayOwner.enabled = true;
    }
#endif

    /// <summary>
    /// Points every screen-space canvas at the UI camera. Overlay canvases composite after the
    /// pipeline has finished, so they can never be covered by the effect; Screen Space - Camera on a
    /// camera that renders to a texture can.
    /// </summary>
    private void ConvertCanvases()
    {
        for (int i = _convertedCanvases.Count - 1; i >= 0; i--)
            if (_convertedCanvases[i].Canvas == null)
                _convertedCanvases.RemoveAt(i);

        var toConvert = new List<Canvas>();
        var alreadyInCamera = new List<Canvas>();

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas == null)
                continue;
            // Never touch any presenter's own presentation canvas, not just this one's: during a
            // transition two presenters are live and would otherwise convert each other's.
            if (canvas.GetComponentInParent<CRTPresenter>(true) != null)
                continue;
            // Only this presenter's own scene. Two scenes are loaded at once during a transition,
            // and converting the other one's canvases means the departing presenter restores them
            // on its way out - leaving the arriving scene's UI pointed at a destroyed camera.
            if (canvas.gameObject.scene != gameObject.scene)
            {
                bool isPersistent = !canvas.gameObject.scene.IsValid()
                    || canvas.gameObject.scene.name == "DontDestroyOnLoad";
                if (!isPersistent || Instance != this)
                    continue;
            }
            // A nested canvas reports its root's render mode and is drawn by the root, so converting
            // one would only fight the root.
            if (!canvas.isRootCanvas)
                continue;
            if (canvas.renderMode == RenderMode.WorldSpace)
                continue;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == _uiCamera)
                continue;   // already ours

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                toConvert.Add(canvas);
            else
                alreadyInCamera.Add(canvas);
        }

        if (toConvert.Count == 0 && alreadyInCamera.Count == 0)
            return;

        // Overlay canvases composite after every camera has finished, so they sit above anything
        // drawn in a camera no matter what their sorting says. Once they move into a camera that is
        // no longer true, so they are re-based above the canvases that were already camera-space.
        // Their order relative to each other is left alone, which is what Overlay sorted them by.
        // The base is worked out once and kept, so canvases that appear later line up with the ones
        // converted at startup instead of climbing past them.
        if (_overlayBaseOrder < 0)
        {
            int baseOrder = 0;
            foreach (Canvas canvas in alreadyInCamera)
                baseOrder = Mathf.Max(baseOrder, canvas.sortingOrder);
            _overlayBaseOrder = baseOrder + 1;
        }

        foreach (Canvas canvas in alreadyInCamera)
            Retarget(canvas, canvas.sortingOrder);
        foreach (Canvas canvas in toConvert)
            Retarget(canvas, _overlayBaseOrder + canvas.sortingOrder);
    }

    private void Retarget(Canvas canvas, int sortingOrder)
    {
        // Only remember the first time. Re-adding on a later scan would record the converted state
        // as the original, so restoring would put back the conversion rather than undo it.
        bool alreadyTracked = false;
        for (int i = 0; i < _convertedCanvases.Count; i++)
        {
            if (_convertedCanvases[i].Canvas != canvas)
                continue;
            alreadyTracked = true;
            break;
        }
        if (!alreadyTracked)
        {
            var state = new CanvasState();
            state.Canvas = canvas;
            state.Mode = canvas.renderMode;
            state.Camera = canvas.worldCamera;
            state.PlaneDistance = canvas.planeDistance;
            state.SortingLayerId = canvas.sortingLayerID;
            state.SortingOrder = canvas.sortingOrder;
            _convertedCanvases.Add(state);
        }

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = _uiCamera;
        canvas.planeDistance = 10f;
        // Screen Space - Camera canvases sort by sorting layer first, where Overlay ignored it
        // entirely. One layer for all of them leaves order as the only thing that decides.
        if (!string.IsNullOrEmpty(_uiSortingLayer))
            canvas.sortingLayerName = _uiSortingLayer;
        canvas.sortingOrder = sortingOrder;
    }

    private void AllocateTextures()
    {
        ReleaseTextures();

        _builtWidth = Mathf.Max(1, Screen.width);
        _builtHeight = Mathf.Max(1, Screen.height);

        _worldTexture = new RenderTexture(_builtWidth, _builtHeight, 24, RenderTextureFormat.ARGB32);
        _worldTexture.name = "CRT World";
        _worldTexture.filterMode = FilterMode.Bilinear;

        _uiTexture = new RenderTexture(_builtWidth, _builtHeight, 24, RenderTextureFormat.ARGB32);
        _uiTexture.name = "CRT UI";
        _uiTexture.filterMode = FilterMode.Bilinear;

        _worldCamera.targetTexture = _worldTexture;
        if (_uiCamera != null)
            _uiCamera.targetTexture = _uiTexture;

        if (_presentImage != null)
            _presentImage.texture = _worldTexture;
        if (_runtimeMaterial != null)
        {
            _runtimeMaterial.SetTexture(MainTexId, _worldTexture);
            _runtimeMaterial.SetTexture(UITexId, _uiTexture);
        }
    }

    private void ReleaseTextures()
    {
        if (_worldCamera != null && _worldCamera.targetTexture == _worldTexture)
            _worldCamera.targetTexture = null;
        if (_uiCamera != null && _uiCamera.targetTexture == _uiTexture)
            _uiCamera.targetTexture = null;

        if (_worldTexture != null)
        {
            _worldTexture.Release();
            SafeDestroy(_worldTexture);
            _worldTexture = null;
        }
        if (_uiTexture != null)
        {
            _uiTexture.Release();
            SafeDestroy(_uiTexture);
            _uiTexture = null;
        }
    }

    // Play mode cannot use DestroyImmediate and edit mode cannot use Destroy, and this runs in both
    // once it is driven from a test or from the editor.
    private static void SafeDestroy(UnityEngine.Object target)
    {
        if (target == null)
            return;
        if (Application.isPlaying)
            Destroy(target);
        else
            DestroyImmediate(target);
    }
}
