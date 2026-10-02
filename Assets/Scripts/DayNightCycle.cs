using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Tooltip("Dauer eines kompletten Tag-Nacht-Zyklus in Sekunden")]
    public float SecondsPerDay = 360f;

    [Tooltip("Startzeit als Tagesanteil: 0 = Mitternacht, 0.25 = Morgen, 0.5 = Mittag, 0.75 = Abend")]
    [Range(0f, 1f)]
    public float StartTimeOfDay = 0.25f;

    [Tooltip("Maximale Sonnenhoehe ueber dem Horizont in Grad")]
    public float MaxElevation = 90f;

    [Tooltip("Lichtstaerke bei Sonnenhoechststand (HDRP-Lux)")]
    public float NoonIntensity = 100000f;

    [Tooltip("Lichtstaerke waehrend der Nacht (Mondschein)")]
    public float NightIntensity = 25f;

    [Tooltip("Farbe des Lichts in der Nacht")]
    public Color NightColor = new Color(0.35f, 0.45f, 0.7f);

    [Tooltip("Farbe des Lichts bei Sonnenauf-/-untergang")]
    public Color DawnColor = new Color(1f, 0.55f, 0.3f);

    [Tooltip("Farbe des Lichts am hohen Mittag")]
    public Color NoonColor = new Color(1f, 0.96f, 0.9f);

    [Tooltip("Zeitskala: 1 = Echtzeit, 2 = doppelt so schnell")]
    public float TimeScale = 1f;

    [Tooltip("Karten-Controller (wird automatisch gefunden, wenn leer)")]
    public MapController mapController;

    [Tooltip("Zyklus anhalten und auf Mittag setzen, solange ein Overlay per Knopf geoeffnet ist")]
    public bool PauseAtNoonInMapMode = true;

    private Light sunLight;
    private float startYaw;
    private float timeOfDay;
    private bool holdNoon;

    private void Awake()
    {
        sunLight = GetComponent<Light>();
        if (sunLight == null)
            Debug.LogWarning("DayNightCycle braucht ein Light-Component am gleichen GameObject");

        if (mapController == null)
            mapController = FindObjectOfType<MapController>();

        startYaw = transform.eulerAngles.y;
    }

    private void OnEnable()
    {
        if (mapController == null)
            mapController = FindObjectOfType<MapController>();
        if (mapController != null)
            mapController.MapModeChanged += OnMapModeChanged;
    }

    private void OnDisable()
    {
        if (mapController != null)
            mapController.MapModeChanged -= OnMapModeChanged;
    }

    private void OnMapModeChanged(bool overlayActive)
    {
        holdNoon = overlayActive;
    }

    private void Start()
    {
        timeOfDay = StartTimeOfDay;
    }

    private void Update()
    {
        if (sunLight == null)
            return;

        bool holdNoonActive = PauseAtNoonInMapMode && holdNoon;

        // Im Overlay einfrieren, sonst normal weiterlaufen lassen
        if (!holdNoonActive)
            timeOfDay = Mathf.Repeat(timeOfDay + Time.deltaTime * TimeScale / SecondsPerDay, 1f);

        // progress 0 = Mitternacht, 0.5 = Mittag; im Overlay wird er als Mittag gehalten
        float elevation = holdNoonActive
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