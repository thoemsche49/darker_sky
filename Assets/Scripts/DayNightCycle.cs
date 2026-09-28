using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Tooltip("Dauer eines kompletten Tag-Nacht-Zyklus in Sekunden")]
    public float SecondsPerDay = 60f;

    [Tooltip("Startzeit als Tagesanteil: 0 = Mitternacht, 0.25 = Morgen, 0.5 = Mittag, 0.75 = Abend")]
    [Range(0f, 1f)]
    public float StartTimeOfDay = 0.25f;

    [Tooltip("Maximale Sonnenhoehe ueber dem Horizont in Grad")]
    public float MaxElevation = 90f;

    [Tooltip("Lichtstaerke bei Sonnenhoechststand (HDRP-Lux)")]
    public float NoonIntensity = 100000f;

    [Tooltip("Lichtstaerke waehrend der Nacht (Mondschein)")]
    public float NightIntensity = 50f;

    [Tooltip("Farbe des Lichts in der Nacht")]
    public Color NightColor = new Color(0.35f, 0.45f, 0.7f);

    [Tooltip("Farbe des Lichts bei Sonnenauf-/-untergang")]
    public Color DawnColor = new Color(1f, 0.55f, 0.3f);

    [Tooltip("Farbe des Lichts am hohen Mittag")]
    public Color NoonColor = new Color(1f, 0.96f, 0.9f);

    [Tooltip("Zeitskala: 1 = Echtzeit, 2 = doppelt so schnell")]
    public float TimeScale = 1f;

    private Light sunLight;
    private float startYaw;

    private void Awake()
    {
        sunLight = GetComponent<Light>();
        if (sunLight == null)
            Debug.LogWarning("DayNightCycle braucht ein Light-Component am gleichen GameObject");

        startYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (sunLight == null)
            return;

        float progress = Mathf.Repeat(Time.time * TimeScale / SecondsPerDay + StartTimeOfDay, 1f);

        // progress 0 = Mitternacht, 0.5 = Mittag -> Sonnenhoehe laeuft sinusfoermig durch
        float elevation = Mathf.Sin((progress * 360f - 90f) * Mathf.Deg2Rad) * MaxElevation;

        transform.rotation = Quaternion.Euler(elevation, startYaw, 0f);

        float dayFactor = Mathf.Clamp01(elevation / MaxElevation);
        dayFactor = dayFactor * dayFactor * (3f - 2f * dayFactor);

        sunLight.intensity = Mathf.Lerp(NightIntensity, NoonIntensity, dayFactor);

        Color dayColor = Color.Lerp(DawnColor, NoonColor, dayFactor);
        sunLight.color = elevation > 0f ? dayColor : NightColor;
    }
}