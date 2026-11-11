using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lights the pitch from the flood lamps, by cutting a shape out of the night filter the same way a
/// lamp's own sprite mask does.
///
/// Each lamp throws a wide arc rather than a block, and the arcs are set wider than their spacing so
/// neighbours overlap and merge into a scalloped edge instead of butting together in squares. A lamp
/// going dark takes its arc with it at once, so the pitch flashes rather than draining away.
///
/// The band is drawn as a row of narrow sections, because a mask is a sprite and a sprite cannot be
/// bent. A section's height is the tallest arc reaching it, plus a slow travelling wave so the far
/// edge keeps breathing once the flickering is over.
///
/// Cutting the night filter open only takes the dark off the grass. Each section also draws a flat
/// warm pane in the same shape, which is what the lamp sprites are - one colour at half alpha, no
/// gradient - so the pitch is lit in the same light as the rest of the scene.
/// </summary>
public class FieldLightMask : MonoBehaviour
{
    [Header("The lit band, in world space")]
    [Tooltip("x, y is the bottom-left corner. Arcs rise from the bottom edge.")]
    [SerializeField] private Rect _fieldArea = new Rect(-24f, -13.5f, 48f, 2.44f);

    [Tooltip("How many slices the band is drawn in. More is a smoother arc and more masks to draw.")]
    [SerializeField] private int _sections = 96;

    [Header("Lamps")]
    [Tooltip("The lamps that light the pitch, one arc each, spread evenly left to right. Usually FloodLights.")]
    [SerializeField] private Transform _lamps;

    [Tooltip("Use only the lowest row of lamps. The rows above are aimed at the far half of the pitch, which is off screen.")]
    [SerializeField] private bool _lowestRowOnly = true;

    [Tooltip("How close in height two lamps must be to count as the same row.")]
    [SerializeField] private float _rowTolerance = 0.5f;

    [Header("Arc shape")]
    [Tooltip("Arc width as a multiple of the gap between lamps. 1 has them just touching; above that they overlap.")]
    [SerializeField] private float _arcWidth = 1.6f;

    [Tooltip("Height at the very edge of an arc, as a fraction of the band. Zero comes to a point; a little above keeps some spill between lamps.")]
    [Range(0f, 1f)]
    [SerializeField] private float _arcEdge = 0.15f;

    [Tooltip("Height at which an arc's crown starts to flatten. Below this the curve is untouched.")]
    [Range(0f, 1f)]
    [SerializeField] private float _crownKnee = 0.6f;

    [Tooltip("How much of the rise above the knee survives. 1 leaves the dome alone; lower settles the top into a shallower cap.")]
    [Range(0f, 1f)]
    [SerializeField] private float _crownRise = 0.35f;

    [Header("Edge pulse")]
    [Tooltip("World units the edge drifts above and below its resting height. One unit is 16 pixels, so keep this small.")]
    [SerializeField] private float _pulseHeight = 0.1f;

    [Tooltip("Radians per second. The edge is a travelling wave, so neighbouring sections move together instead of each jittering on its own.")]
    [SerializeField] private float _waveSpeed = 0.45f;

    [Tooltip("How many full waves fit across the band at once. More is a tighter ripple.")]
    [SerializeField] private float _waveCount = 3.5f;

    [Tooltip("A second, slower wave crossed with the first so the motion never repeats visibly.")]
    [SerializeField] private float _secondWaveScale = 0.55f;

    [Header("Mask")]
    [SerializeField] private Sprite _maskSprite;
    [SerializeField] private float _alphaCutoff = 0.1f;

    [Header("Glow")]
    [Tooltip("The light's own colour, drawn over the grass. Matches the lamp sprites, which are this flat colour at half alpha.")]
    [SerializeField] private Color _glowColour = new Color(1f, 1f, 0.784f, 0.498f);

    [Tooltip("Left empty, the mask's sprite is used. It only ever needs to be a plain square.")]
    [SerializeField] private Sprite _glowSprite;

    [Tooltip("Left empty, Unity's default sprite material is used. Set this to match the lamps.")]
    [SerializeField] private Material _glowMaterial;

    [Tooltip("Must put the glow above the night filter. The title panel sits higher still but never reaches the pitch.")]
    [SerializeField] private string _glowSortingLayer = "Default";
    [SerializeField] private int _glowSortingOrder = 2;

    private readonly List<SpriteMask> _masks = new List<SpriteMask>();
    private readonly List<SpriteRenderer> _glows = new List<SpriteRenderer>();
    private readonly List<TitleScreenLight> _zoneLamps = new List<TitleScreenLight>();
    private float _crossPhase;

    public int SectionCount { get { return _masks.Count; } }

    public int ZoneCount { get { return _zoneLamps.Count; } }

    /// <summary>Whether the lamp responsible for a zone is lit. A zone is on or off, never between.</summary>
    public float ZoneCoverage(int zone)
    {
        if (zone < 0 || zone >= _zoneLamps.Count)
            return 0f;
        return _zoneLamps[zone] != null && _zoneLamps[zone].IsLit ? 1f : 0f;
    }

    /// <summary>The tallest arc reaching a point, 0 to 1, ignoring the wave.</summary>
    public float HeightAt(float worldX)
    {
        float tallest = 0f;
        float radius = ArcRadius();
        for (int z = 0; z < _zoneLamps.Count; z++)
        {
            if (_zoneLamps[z] == null || !_zoneLamps[z].IsLit)
                continue;
            float distance = Mathf.Abs(worldX - ZoneCentre(z));
            if (distance >= radius)
                continue;
            float t = distance / radius;
            // An ellipse, so the light reads as an arc thrown across the grass rather than a slab.
            float arc = Mathf.Lerp(_arcEdge, 1f, Mathf.Sqrt(Mathf.Max(0f, 1f - t * t)));
            tallest = Mathf.Max(tallest, arc);
        }
        return FlattenCrown(tallest);
    }

    // An ellipse peaks in a dome, which reads as a mound of light rather than a wide arc thrown
    // across the grass. Above the knee the rise is eased off so the crown settles shallow while the
    // shoulders keep their curve. The quadratic leaves the slope continuous at the join; a straight
    // cut there shows up as a crease along the edge.
    private float FlattenCrown(float arc)
    {
        if (arc <= _crownKnee || _crownKnee >= 1f)
            return arc;
        float range = 1f - _crownKnee;
        float over = (arc - _crownKnee) / range;
        return _crownKnee + range * (over - (1f - _crownRise) * 0.5f * over * over);
    }

    private void Awake()
    {
        CollectLamps();
        BuildSections();
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    private float ArcRadius()
    {
        int zones = Mathf.Max(1, _zoneLamps.Count);
        return _fieldArea.width / zones * 0.5f * Mathf.Max(0.01f, _arcWidth);
    }

    // Lamps are spread evenly across the band rather than placed at their own x, because the rig is
    // two clusters at the far ends and using their real positions would leave the middle unlit.
    private float ZoneCentre(int zone)
    {
        int zones = Mathf.Max(1, _zoneLamps.Count);
        return _fieldArea.xMin + _fieldArea.width * (zone + 0.5f) / zones;
    }

    private void CollectLamps()
    {
        _zoneLamps.Clear();
        if (_lamps == null)
            return;
        _lamps.GetComponentsInChildren(true, _zoneLamps);

        if (_lowestRowOnly && _zoneLamps.Count > 0)
        {
            float lowest = float.MaxValue;
            for (int i = 0; i < _zoneLamps.Count; i++)
                lowest = Mathf.Min(lowest, _zoneLamps[i].transform.position.y);
            for (int i = _zoneLamps.Count - 1; i >= 0; i--)
                if (_zoneLamps[i].transform.position.y > lowest + _rowTolerance)
                    _zoneLamps.RemoveAt(i);
        }

        _zoneLamps.Sort(delegate (TitleScreenLight a, TitleScreenLight b)
        {
            return a.transform.position.x.CompareTo(b.transform.position.x);
        });
    }

    private void Apply()
    {
        float sectionWidth = _fieldArea.width / Mathf.Max(1, _masks.Count);
        for (int i = 0; i < _masks.Count; i++)
        {
            SpriteMask mask = _masks[i];
            float centreX = _fieldArea.xMin + sectionWidth * (i + 0.5f);
            float arc = HeightAt(centreX);
            if (arc <= 0.001f)
            {
                mask.enabled = false;
                _glows[i].enabled = false;
                continue;
            }

            // The band's bottom is pinned to the foot of the pitch; only the top edge moves.
            float height = Mathf.Max(0.01f, _fieldArea.height * arc + EdgeOffset(i));
            Transform t = mask.transform;
            t.localScale = new Vector3(sectionWidth, height, 1f);
            t.position = new Vector3(centreX, _fieldArea.yMin + height * 0.5f, 0f);
            mask.enabled = true;

            // The glow shares the section's transform, so it is already the right shape.
            _glows[i].enabled = _glowColour.a > 0f;
        }
    }

    // A section's offset comes from where it sits along the band, not from a private random phase.
    // That is what makes the edge ripple as one surface: a neighbour is always a small step along
    // the same wave, so the movement reads as travelling rather than as each section twitching.
    private float EdgeOffset(int index)
    {
        float along = _masks.Count > 1 ? (float)index / (_masks.Count - 1) : 0f;
        float tau = Mathf.PI * 2f;
        float first = Mathf.Sin(Time.time * _waveSpeed + along * tau * _waveCount);
        float second = Mathf.Sin(Time.time * _waveSpeed * _secondWaveScale
            + along * tau * _waveCount * _secondWaveScale + _crossPhase);
        return (first * 0.65f + second * 0.35f) * _pulseHeight;
    }

    private void BuildSections()
    {
        _crossPhase = Random.Range(0f, Mathf.PI * 2f);
        int wanted = Mathf.Max(1, _sections);
        for (int i = 0; i < wanted; i++)
        {
            var go = new GameObject("FieldLightSection");
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);

            var mask = go.AddComponent<SpriteMask>();
            mask.sprite = _maskSprite;
            mask.alphaCutoff = _alphaCutoff;
            mask.enabled = false;

            // The warm pane rides on the same object as the mask, so one transform drives both and
            // the colour can never drift out of the shape it is lighting. It draws unmasked: the
            // mask beneath it is there to lift the night filter, not to clip the light itself.
            var glow = go.AddComponent<SpriteRenderer>();
            glow.sprite = _glowSprite != null ? _glowSprite : _maskSprite;
            glow.color = _glowColour;
            glow.sortingLayerName = _glowSortingLayer;
            glow.sortingOrder = _glowSortingOrder;
            glow.maskInteraction = SpriteMaskInteraction.None;
            if (_glowMaterial != null)
                glow.sharedMaterial = _glowMaterial;
            glow.enabled = false;

            _masks.Add(mask);
            _glows.Add(glow);
        }
    }
}
