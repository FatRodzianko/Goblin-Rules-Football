using UnityEngine;

// Weather controls while testing a hole from the map maker. Clicking the wind icon points the wind
// at where you clicked, scrolling over it changes its strength, and clicking the weather icon
// cycles through the rain states.
//
// The two icons are SpriteRenderers parented to the Main Camera rather than UI, so there is nothing
// for a UGUI click to land on and no collider for the event system to find. Giving them colliders
// plus a Physics2DRaycaster would put trigger colliders that track the camera across the whole
// playfield, where any 2D query that didn't mask them out could hit them, so this tests the cursor
// against the icons' own bounds instead. Nothing about the icons changes.
//
// A drop-in is one player who is also the host, so this only ever runs on the server.
public class MapMakerDropInWeather : MonoBehaviour
{
    // Clockwise from east, matching how a click's angle is rounded to the nearest eighth
    static readonly Vector2[] CardinalDirections =
    {
        new Vector2(1f, 0f),
        new Vector2(1f, 1f),
        new Vector2(0f, 1f),
        new Vector2(-1f, 1f),
        new Vector2(-1f, 0f),
        new Vector2(-1f, -1f),
        new Vector2(0f, -1f),
        new Vector2(1f, -1f),
    };

    Camera _camera;
    SpriteRenderer _windArrow;
    SpriteRenderer _weatherIcon;

    public static void Create()
    {
        GameObject controls = new GameObject("MapMakerDropInWeather");
        controls.AddComponent<MapMakerDropInWeather>();
    }

    void Start()
    {
        // Both icons hang off the Main Camera, so they come from its own hierarchy. The player
        // prefab carries a second, inactive copy of the wind icon under the player instead, and
        // searching the camera can't reach it.
        _camera = Camera.main;
        if (_camera == null)
        {
            Debug.LogWarning("MapMakerDropInWeather: no Main Camera, so the weather controls are off.");
            enabled = false;
            return;
        }

        WindIconHolder windIcons = _camera.GetComponentInChildren<WindIconHolder>(true);
        WeatherIconHolder weatherIcons = _camera.GetComponentInChildren<WeatherIconHolder>(true);

        _windArrow = windIcons != null ? windIcons.WindDirectionArrow : null;
        _weatherIcon = weatherIcons != null ? weatherIcons.WeatherIcon : null;

        if (_windArrow == null)
            Debug.LogWarning("MapMakerDropInWeather: no wind icon under the Main Camera, so the wind can't be set.");
        if (_weatherIcon == null)
            Debug.LogWarning("MapMakerDropInWeather: no weather icon under the Main Camera, so the weather can't be cycled.");

        if (_windArrow == null && _weatherIcon == null)
            enabled = false;
    }
    void Update()
    {
        bool clicked = Input.GetMouseButtonDown(0);
        float scroll = Input.mouseScrollDelta.y;
        if (!clicked && Mathf.Approximately(scroll, 0f))
            return;
        if (!MapMakerDropInSession.IsActive)
            return;

        Vector3 pointer = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (IsPointerOverIcon(_windArrow, pointer))
        {
            if (clicked)
                SetWindDirectionFromPointer(pointer);
            else
                ChangeWindPower(scroll > 0f ? 1 : -1);
            return;
        }

        if (clicked && IsPointerOverIcon(_weatherIcon, pointer))
            CycleRainState();
    }
    static bool IsPointerOverIcon(SpriteRenderer icon, Vector3 pointer)
    {
        if (icon == null || !icon.enabled)
            return false;

        // The icon sits on a plane of its own, so only its width and height matter
        Bounds bounds = icon.bounds;
        pointer.z = bounds.center.z;
        return bounds.Contains(pointer);
    }
    // Where the click landed relative to the middle of the icon, rounded to the nearest of the
    // eight directions the game's own wind uses.
    void SetWindDirectionFromPointer(Vector3 pointer)
    {
        if (!CanChangeWind())
            return;

        Vector2 fromCentre = (Vector2)pointer - (Vector2)_windArrow.bounds.center;
        if (fromCentre.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(fromCentre.y, fromCentre.x) * Mathf.Rad2Deg;
        int eighth = Mathf.RoundToInt(angle / 45f);
        if (eighth < 0)
            eighth += 8;
        eighth %= 8;

        WindManager.instance.SetWindDirection(CardinalDirections[eighth]);
    }
    void ChangeWindPower(int change)
    {
        if (!CanChangeWind())
            return;

        WindManager.instance.SetWindPower(WindManager.instance.WindPower + change);
    }
    void CycleRainState()
    {
        if (!CanChangeRain())
            return;
        if (RainManager.instance.RainStates.Count == 0)
            return;

        int current = RainManager.instance.RainStates.IndexOf(RainManager.instance.RainState);
        int next = (current + 1) % RainManager.instance.RainStates.Count;

        RainManager.instance.SetWeatherState(RainManager.instance.RainStates[next]);
    }
    static bool CanChangeWind()
    {
        return WindManager.instance != null && WindManager.instance.IsServerInitialized;
    }
    static bool CanChangeRain()
    {
        return RainManager.instance != null && RainManager.instance.IsServerInitialized;
    }
}
