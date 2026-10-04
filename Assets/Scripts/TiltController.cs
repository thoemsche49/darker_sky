using TouchScript.Gestures.TransformGestures;
using UnityEngine;

/// <summary>
/// Tilts the camera around the map point below the screen centre while a
/// two-finger vertical drag is active. Dragging up pitches the camera steeper,
/// dragging down returns it towards top-down (Google-Maps behaviour).
/// </summary>
public class TiltController : MonoBehaviour
{
    [Tooltip("Two-finger tilt gesture that drives the tilting.")]
    public ScreenTransformGesture TiltGesture;

    [Tooltip("World Y of the map plane around which the camera tilts.")]
    public float GroundLevel = 0f;

    [Tooltip("Minimum pitch (shallow) in degrees.")]
    public float MinPitch = 20f;

    [Tooltip("Maximum pitch (steep, close to top-down) in degrees.")]
    public float MaxPitch = 89f;

    [Tooltip("Sensitivity of the finger drag on the pitch angle.")]
    public float TiltSpeed = 0.2f;

    private Camera cachedCamera;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
        if (cachedCamera == null)
            cachedCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (TiltGesture == null)
        {
            Debug.LogError("TiltGesture is missing");
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
        if (cachedCamera == null)
            return;

        // Vertical finger drag; positive when the finger moves up (TouchScript).
        float verticalDrag = TiltGesture.DeltaPosition.y;
        if (Mathf.Approximately(verticalDrag, 0f))
            return;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector3 pivot = GetGroundPoint(screenCenter);

        Vector3 offset = transform.position - pivot;
        float distance = offset.magnitude;
        if (distance < 0.001f)
            return;

        // Derive the current pitch and yaw from the existing camera position.
        float currentPitch = Mathf.Asin(Mathf.Clamp(offset.y / distance, -1f, 1f)) * Mathf.Rad2Deg;
        float yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;

        // Drag up -> steeper (pitch towards horizontal); drag down -> top-down again.
        float newPitch = Mathf.Clamp(currentPitch - verticalDrag * TiltSpeed, MinPitch, MaxPitch);

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

    private Vector3 GetGroundPoint(Vector2 screenPosition)
    {
        Ray ray = cachedCamera.ScreenPointToRay(screenPosition);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, GroundLevel, 0f));
        float distance;
        if (ground.Raycast(ray, out distance))
            return ray.GetPoint(distance);

        return cachedCamera.transform.position + cachedCamera.transform.forward * 50f;
    }
}