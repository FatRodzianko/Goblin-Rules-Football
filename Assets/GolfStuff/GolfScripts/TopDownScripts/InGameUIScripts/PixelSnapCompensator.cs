using UnityEngine;
using UnityEngine.Rendering;
using PixelPerfectCamera = UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera;

// Holds a world-space object that is parented to the camera still on screen. PixelPerfectCamera replaces
// the camera's view matrix every frame with one built from the camera's position rounded to the pixel
// grid. World content is then drawn pixel aligned, but anything parented to the camera keeps its exact
// fractional position, so it slides by whatever that frame's rounding threw away - up to half a pixel,
// changing every frame the camera moves. Adding the same offset back cancels it.
//
// Point filtered sprites hide that slide; anti-aliased text does not, which is what this was added for.
public class PixelSnapCompensator : MonoBehaviour
{
    private PixelPerfectCamera _pixelPerfectCamera;
    private Camera _camera;
    private Vector3 _restingLocalPosition;

    private void Awake()
    {
        _restingLocalPosition = transform.localPosition;
    }
    // Resolved here rather than in Awake because the club UI is only parented to the camera while it is
    // on screen, and goes back to the player object in between.
    private void OnEnable()
    {
        _pixelPerfectCamera = GetComponentInParent<PixelPerfectCamera>();
        _camera = _pixelPerfectCamera ? _pixelPerfectCamera.GetComponent<Camera>() : null;
        if (!_camera)
            return;

        RenderPipelineManager.beginCameraRendering += HandleBeginCameraRendering;
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= HandleBeginCameraRendering;
        transform.localPosition = _restingLocalPosition;
    }
    // Has to run after Cinemachine has placed the camera and before the camera draws, which is exactly
    // when this fires. PixelPerfectCamera reads the same camera position in its own callback, so the
    // order the two run in does not matter.
    void HandleBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera != _camera)
            return;

        Vector3 cameraPosition = _camera.transform.position;
        Vector3 roundingOffset = _pixelPerfectCamera.RoundToPixel(cameraPosition) - cameraPosition;
        transform.localPosition = _restingLocalPosition + new Vector3(roundingOffset.x, roundingOffset.y, 0f);
    }
}
