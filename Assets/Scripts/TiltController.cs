using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class TiltController : MonoBehaviour
{
    public ScreenTransformGesture TiltGesture;

    [Tooltip("Hoehe der Kartenebene (Welt-Y), um die geneigt wird")]
    public float GroundLevel = 0f;

    [Tooltip("Minimaler Neigungswinkel in Grad (flach/Draufsicht, 90 = senkrecht von oben)")]
    public float MinPitch = 20f;

    [Tooltip("Maximaler Neigungswinkel in Grad (steil/fast Draufsicht)")]
    public float MaxPitch = 89f;

    [Tooltip("Empfindlichkeit der Fingerbewegung auf den Neigungswinkel")]
    public float TiltSpeed = 0.2f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }

    private void OnEnable()
    {
        if (TiltGesture == null)
        {
            Debug.LogError("TiltGesture fehlt");
            return;
        }

        TiltGesture.Transformed += OnTransformed;
    }

    private void OnDisable()
    {
        if (TiltGesture != null)
            TiltGesture.Transformed -= OnTransformed;
    }

    private void OnTransformed(object sender, System.EventArgs e)
    {
        if (cam == null)
            return;

        // Vertikale Fingerbewegung: nach oben ziehen = negative Y-Delta in Screen-Koordinaten? 
        // TouchScript: DeltaPosition.y ist positiv, wenn Finger nach oben bewegt werden.
        float dragY = TiltGesture.DeltaPosition.y;
        if (Mathf.Approximately(dragY, 0f))
            return;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector3 pivot = GetGroundPoint(screenCenter);

        Vector3 offset = transform.position - pivot;
        float distance = offset.magnitude;
        if (distance < 0.001f)
            return;

        // Aktuellen Yaw- und Pitch-Winkel aus der bestehenden Position ableiten
        float currentPitch = Mathf.Asin(Mathf.Clamp(offset.y / distance, -1f, 1f)) * Mathf.Rad2Deg;
        float yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;

        // Finger nach oben (Google-Maps-Verhalten) -> steiler neigen (Pitch sinkt Richtung horizontal)
        // Finger nach unten -> wieder Richtung Draufsicht
        float newPitch = Mathf.Clamp(currentPitch - dragY * TiltSpeed, MinPitch, MaxPitch);

        float pitchRad = newPitch * Mathf.Deg2Rad;
        float yawRad = yaw * Mathf.Deg2Rad;

        Vector3 newOffset = new Vector3(
            Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
        ) * distance;

        transform.position = pivot + newOffset;
        transform.rotation = Quaternion.LookRotation((pivot - transform.position).normalized, Vector3.up);
    }

    private Vector3 GetGroundPoint(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, GroundLevel, 0f));
        float d;
        if (ground.Raycast(ray, out d))
            return ray.GetPoint(d);

        return cam.transform.position + cam.transform.forward * 50f;
    }
}