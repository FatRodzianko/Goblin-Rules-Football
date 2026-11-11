using TMPro;
using UnityEngine;

/// <summary>
/// Makes the title read as a lit sign rather than text that happens to be yellow: its glow is
/// switched by a <see cref="TitleScreenLight"/>, so it strikes up stuttering alongside the stadium
/// lamps and sits dark until the power takes.
///
/// The light on this object carries no sprite of its own. A TextMeshPro graphic is neither a
/// SpriteRenderer nor an Image, so the light has nothing to switch and never hides the letters -
/// all it does here is keep time, which is what keeps the sign in character with the lamps without
/// a second copy of the strike-up timing.
///
/// What lit and dark look like is read from the material the text was authored with, so the whole
/// look stays tunable in the material inspector and nothing is duplicated as numbers in here.
/// </summary>
[RequireComponent(typeof(TitleScreenLight))]
public class TitleGlow : MonoBehaviour
{
    [Tooltip("The sign. Left empty, a TextMeshProUGUI on this object or under it is used.")]
    [SerializeField] private TextMeshProUGUI _text;

    [Tooltip("How much of the face colour is left while the sign is dark. Zero reads as missing text rather than an unlit sign.")]
    [Range(0f, 1f)]
    [SerializeField] private float _darkFace = 0.4f;

    [Tooltip("How much of the glow is left while the sign is dark. A trace of it stops the letters going flat between blinks.")]
    [Range(0f, 1f)]
    [SerializeField] private float _darkGlow;

    private TitleScreenLight _light;
    private Material _material;
    private Color _litFace;
    private Color _litGlow;
    private Color _litUnderlay;
    private bool _ready;

    private void Awake()
    {
        _light = GetComponent<TitleScreenLight>();
        if (_text == null)
            _text = GetComponentInChildren<TextMeshProUGUI>(true);
        if (_text == null)
        {
            Debug.LogWarning("TitleGlow on " + name + " has no text to light.", this);
            return;
        }

        // fontMaterial, not fontSharedMaterial: the shared one is the asset on disk, and a play
        // session would otherwise save the flicker into the preset.
        _material = _text.fontMaterial;
        _litFace = _text.color;
        if (_material.HasProperty(ShaderUtilities.ID_GlowColor))
            _litGlow = _material.GetColor(ShaderUtilities.ID_GlowColor);
        if (_material.HasProperty(ShaderUtilities.ID_UnderlayColor))
            _litUnderlay = _material.GetColor(ShaderUtilities.ID_UnderlayColor);
        _ready = true;

        _light.LitChanged += Apply;

        // The light sets itself dark in its own Awake, which may already have run.
        Apply(_light.IsLit);
    }

    private void OnDestroy()
    {
        if (_light != null)
            _light.LitChanged -= Apply;
    }

    private void Apply(bool lit)
    {
        if (!_ready)
            return;

        // Only the alphas move. Glow spread and underlay softness decide the mesh padding TMP
        // reserved when the text was laid out, so changing those would need the mesh rebuilt on
        // every blink; fading them is as convincing and costs nothing.
        float face = lit ? 1f : _darkFace;
        float glow = lit ? 1f : _darkGlow;

        _text.color = new Color(_litFace.r, _litFace.g, _litFace.b, _litFace.a * face);
        if (_material.HasProperty(ShaderUtilities.ID_GlowColor))
            _material.SetColor(ShaderUtilities.ID_GlowColor, Faded(_litGlow, glow));
        if (_material.HasProperty(ShaderUtilities.ID_UnderlayColor))
            _material.SetColor(ShaderUtilities.ID_UnderlayColor, Faded(_litUnderlay, glow));
    }

    private static Color Faded(Color colour, float amount)
    {
        return new Color(colour.r, colour.g, colour.b, colour.a * amount);
    }
}
