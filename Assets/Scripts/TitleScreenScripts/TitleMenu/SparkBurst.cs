using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Throws bright pixels out of a point in short spurts, as old wiring arcing before the power takes.
///
/// Sparks are plain pooled SpriteRenderers rather than a particle system, for two reasons: a sprite
/// can be held exactly on the pixel grid, which a particle cannot, and the whole effect is a few
/// dozen one-pixel quads for well under a second.
/// </summary>
public class SparkBurst : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private Color _colour = new Color(1f, 0.93f, 0.25f, 1f);

    [Tooltip("Left empty, a single white pixel is built at runtime and tinted.")]
    [SerializeField] private Sprite _sparkSprite;

    [Tooltip("Unlit, so a spark is its own light rather than something the scene lights up.")]
    [SerializeField] private Material _material;

    [Tooltip("Must put the sparks in front of the background and behind the title panel.")]
    [SerializeField] private int _sortingOrder = 1;
    [SerializeField] private string _sortingLayer = "Default";

    [Tooltip("The art's pixels per unit. Sparks are snapped to this grid so they never land between pixels.")]
    [SerializeField] private float _pixelsPerUnit = 16f;

    [Tooltip("How many pixels across one spark is. One is too small to read against a bright background; two carries.")]
    [SerializeField] private float _sparkPixels = 2f;

    [Header("Spurts")]
    [SerializeField] private Vector2Int _spurtCount = new Vector2Int(3, 5);
    [SerializeField] private Vector2 _spurtGap = new Vector2(0.05f, 0.16f);
    [SerializeField] private Vector2Int _sparksPerSpurt = new Vector2Int(6, 14);

    [Header("Each spark")]
    [SerializeField] private Vector2 _speed = new Vector2(2f, 7f);
    [SerializeField] private Vector2 _lifetime = new Vector2(0.12f, 0.4f);
    [SerializeField] private float _gravity = -16f;

    [Tooltip("Degrees the spray covers, with 0 to the right and 90 up. The default throws down and outwards.")]
    [SerializeField] private Vector2 _spreadDegrees = new Vector2(200f, 340f);

    private readonly List<Spark> _sparks = new List<Spark>();
    private Sprite _runtimeSprite;
    private Coroutine _routine;

    public bool IsPlaying { get { return _routine != null; } }

    /// <summary>Sparks still in the air.</summary>
    public int LiveSparkCount
    {
        get
        {
            int n = 0;
            for (int i = 0; i < _sparks.Count; i++)
                if (_sparks[i].Active) n++;
            return n;
        }
    }

    private class Spark
    {
        public Transform Transform;
        public SpriteRenderer Renderer;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Age;
        public float Lifetime;
        public bool Active;
    }

    /// <summary>Starts the spurts. Does nothing if they are already running.</summary>
    public void Play()
    {
        if (_routine != null)
            return;
        _routine = StartCoroutine(RunSpurts());
    }

    /// <summary>Stops emitting and clears anything still in the air.</summary>
    public void StopAndClear()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
        for (int i = 0; i < _sparks.Count; i++)
        {
            _sparks[i].Active = false;
            _sparks[i].Renderer.enabled = false;
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < _sparks.Count; i++)
        {
            Spark spark = _sparks[i];
            if (!spark.Active)
                continue;

            spark.Age += dt;
            if (spark.Age >= spark.Lifetime)
            {
                spark.Active = false;
                spark.Renderer.enabled = false;
                continue;
            }

            spark.Velocity += new Vector2(0f, _gravity) * dt;
            spark.Position += spark.Velocity * dt;

            // The true position carries on unsnapped; only what is drawn is pinned to the grid, so
            // the motion stays smooth while the pixel never straddles two.
            spark.Transform.localPosition = Snap(spark.Position);
        }
    }

    private IEnumerator RunSpurts()
    {
        int spurts = Random.Range(_spurtCount.x, _spurtCount.y + 1);
        for (int i = 0; i < spurts; i++)
        {
            int count = Random.Range(_sparksPerSpurt.x, _sparksPerSpurt.y + 1);
            for (int j = 0; j < count; j++)
                Emit();
            if (i < spurts - 1)
                yield return new WaitForSeconds(Random.Range(_spurtGap.x, _spurtGap.y));
        }
        _routine = null;
    }

    private void Emit()
    {
        Spark spark = Take();
        float angle = Random.Range(_spreadDegrees.x, _spreadDegrees.y) * Mathf.Deg2Rad;
        float speed = Random.Range(_speed.x, _speed.y);
        spark.Position = Vector2.zero;
        spark.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
        spark.Age = 0f;
        spark.Lifetime = Random.Range(_lifetime.x, _lifetime.y);
        spark.Active = true;
        spark.Transform.localPosition = Vector3.zero;
        spark.Renderer.enabled = true;
    }

    private Spark Take()
    {
        for (int i = 0; i < _sparks.Count; i++)
            if (!_sparks[i].Active)
                return _sparks[i];

        var go = new GameObject("Spark");
        go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);
        float size = _sparkPixels / _pixelsPerUnit;
        go.transform.localScale = new Vector3(size, size, 1f);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = _sparkSprite != null ? _sparkSprite : RuntimeSprite();
        renderer.color = _colour;
        renderer.sortingLayerName = _sortingLayer;
        renderer.sortingOrder = _sortingOrder;
        if (_material != null)
            renderer.sharedMaterial = _material;
        renderer.enabled = false;

        var spark = new Spark { Transform = go.transform, Renderer = renderer };
        _sparks.Add(spark);
        return spark;
    }

    // One white texel, so no art asset is needed for something this small.
    private Sprite RuntimeSprite()
    {
        if (_runtimeSprite != null)
            return _runtimeSprite;
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        _runtimeSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        _runtimeSprite.name = "SparkPixel";
        return _runtimeSprite;
    }

    private Vector3 Snap(Vector2 position)
    {
        return new Vector3(
            Mathf.Round(position.x * _pixelsPerUnit) / _pixelsPerUnit,
            Mathf.Round(position.y * _pixelsPerUnit) / _pixelsPerUnit,
            0f);
    }
}
