using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A banner across the top of the screen while a map maker drop-in is placing the ball by hand.
//
// Built in code and owned by nothing. The gameplay scene's canvases are phase gated - most of them
// are switched off outside their own phase, and an inactive canvas also stops its children updating
// - so anything that has to be visible in any phase needs a canvas of its own. Building it here
// rather than adding one to the scene keeps drop-in tooling out of the assets the real game ships.
public class MapMakerDropInBanner : MonoBehaviour
{
    // Above the gameplay UI, so nothing the game turns on can cover it
    const int BannerSortingOrder = 500;
    const float BannerHeight = 70f;
    const float BannerFontSize = 26f;
    static readonly Vector2 ReferenceResolution = new Vector2(768f, 432f);
    static readonly Color BackingColor = new Color(0f, 0f, 0f, 0.6f);

    static MapMakerDropInBanner _instance;

    TextMeshProUGUI _text;

    public static void Show(string message)
    {
        if (!MapMakerDropInSession.IsActive)
            return;

        // A destroyed object compares equal to null, so leaving a scene quietly forces a rebuild.
        if (_instance == null)
            _instance = Build();

        _instance._text.text = message;
        _instance.gameObject.SetActive(true);
    }
    public static void Hide()
    {
        if (_instance == null)
            return;

        _instance.gameObject.SetActive(false);
    }
    static MapMakerDropInBanner Build()
    {
        GameObject root = new GameObject("MapMakerDropInBanner");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = BannerSortingOrder;

        // Fully qualified: the Football game defines its own global CanvasScaler, which otherwise
        // wins over Unity's here.
        UnityEngine.UI.CanvasScaler scaler = root.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;

        GameObject backing = new GameObject("Backing");
        backing.transform.SetParent(root.transform, false);
        Image backingImage = backing.AddComponent<Image>();
        backingImage.color = BackingColor;
        backingImage.raycastTarget = false;
        StretchAcrossTop(backingImage.rectTransform);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(root.transform, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = BannerFontSize;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        StretchAcrossTop(text.rectTransform);

        MapMakerDropInBanner banner = root.AddComponent<MapMakerDropInBanner>();
        banner._text = text;
        return banner;
    }
    static void StretchAcrossTop(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, BannerHeight);
    }
    void Update()
    {
        // The session can end while the banner is up, and nothing else owns this object.
        if (!MapMakerDropInSession.IsActive)
            Destroy(gameObject);
    }
}
