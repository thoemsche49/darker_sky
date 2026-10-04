using UnityEngine;

/// <summary>
/// Animated a directional light through a day/night cycle. The cycle can be
/// paused at noon while the light-pollution map overlay is open.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Tooltip("Duration of a full day/night cycle in seconds.")]
    public float SecondsPerDay = 360f;

    [Tooltip("Start time as fraction of a day: 0 = midnight, 0.25 = morning, 0.5 = noon, 0.75 = evening.")]
    [Range(0f, 1f)]
    public float StartTimeOfDay = 0.25f;

    [Tooltip("Maximum sun elevation above the horizon in degrees.")]
    public float MaxElevation = 90f;

    [Tooltip("Light intensity at solar noon (HDRP lux).")]
    public float NoonIntensity = 100000f;

    [Tooltip("Light intensity at night (moonlight).")]
    public float NightIntensity = 25f;

    [Tooltip("Light color at night.")]
    public Color NightColor = new Color(0.35f, 0.45f, 0.7f);

    [Tooltip("Light color at sunrise and sunset.")]
    public Color DawnColor = new Color(1f, 0.55f, 0.3f);

    [Tooltip("Light color at high noon.")]
    public Color NoonColor = new Color(1f, 0.96f, 0.9f);

    [Tooltip("Time scale: 1 = real time, 2 = twice as fast.")]
    public float TimeScale = 1f;

    [Tooltip("Map controller (found automatically if empty).")]
    public MapController mapController;

    [Tooltip("Pause the cycle and hold it at noon while the map overlay is open.")]
    public bool PauseAtNoonInMapMode = true;

    private Light sunLight;
    private float startYaw;
    private float timeOfDay;
    private bool freezeAtNoon;

    private void Awake()
    {
        sunLight = GetComponent<Light>();
        if (sunLight == null)
            Debug.LogWarning("DayNightCycle requires a Light component on the same GameObject");

        if (mapController == null)
            mapController = FindObjectOfType<MapController>();

        startYaw = transform.eulerAngles.y;
    }

    private void OnEnable()
    {
        if (mapController == null)
            mapController = FindObjectOfType<MapController>();

        if (mapController != null)
            mapController.MapViewChanged += OnMapViewChanged;
    }

    private void OnDisable()
    {
        if (mapController != null)
            mapController.MapViewChanged -= OnMapViewChanged;
    }

    private void OnMapViewChanged(bool mapOpen)
    {
        freezeAtNoon = mapOpen;
    }

    private void Start()
    {
        timeOfDay = StartTimeOfDay;
    }

    private void Update()
    {
        if (sunLight == null)
            return;

        bool freezeActive = PauseAtNoonInMapMode && freezeAtNoon;

        // Advance the time unless it is frozen while the map overlay is open.
        if (!freezeActive)
            timeOfDay = Mathf.Repeat(timeOfDay + Time.deltaTime * TimeScale / SecondsPerDay, 1f);

        // Elevation: 0 = midnight, 0.5 = noon; held at MaxElevation while frozen.
        float elevation = freezeActive
            ? MaxElevation
            : Mathf.Sin((timeOfDay * 360f - 90f) * Mathf.Deg2Rad) * MaxElevation;

        transform.rotation = Quaternion.Euler(elevation, startYaw, 0f);

        float dayFactor = Mathf.Clamp01(elevation / MaxElevation);
        dayFactor = dayFactor * dayFactor * (3f - 2f * dayFactor);

        sunLight.intensity = Mathf.Lerp(NightIntensity, NoonIntensity, dayFactor);

        Color dayColor = Color.Lerp(DawnColor, NoonColor, dayFactor);
        sunLight.color = elevation > 0f ? dayColor : NightColor;
    }
}