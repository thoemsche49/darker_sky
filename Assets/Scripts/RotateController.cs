using TouchScript.Gestures.TransformGestures;
using UnityEngine;

/// <summary>
/// Orbits the camera horizontally around the map point below the screen centre
/// while a three-finger rotation gesture is active. The camera always looks at
/// that pivot, so the map appears to rotate around the screen centre.
/// </summary>
public class RotateController : MonoBehaviour
{
    [Tooltip("Three-finger rotation gesture that drives the orbit.")]
    public ScreenTransformGesture RotateGesture;

    [Tooltip("World Y of the map plane around which the camera orbits.")]
    public float GroundLevel = 0f;

    private Camera cachedCamera;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        if (RotateGesture == null)
            return;

        RotateGesture.Transformed += OnTransformed;
    }

    private void OnDisable()
    {
        if (RotateGesture != null)
            RotateGesture.Transformed -= OnTransformed;
    }

    private void OnTransformed(object sender, System.EventArgs e)
    {
        // Only rotate with exactly three fingers; any other count aborts.
        if (RotateGesture.NumPointers != 3)
            return;

        float angle = RotateGesture.DeltaRotation;

        // Pivot: the map point directly below the screen centre.
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector3 pivot = GetGroundPoint(screenCenter);

        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 offset = rotation * (transform.position - pivot);
        transform.position = pivot + offset;
        transform.rotation = Quaternion.LookRotation((pivot - transform.position).normalized, Vector3.up);
    }

    private Vector3 GetGroundPoint(Vector2 screenPosition)
    {
        Ray ray = cachedCamera.ScreenPointToRay(screenPosition);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, GroundLevel, 0f));
        float distance;
        if (ground.Raycast(ray, out distance))
            return ray.GetPoint(distance);

        // Fallback if the ray misses the plane.
        return cachedCamera.transform.position + cachedCamera.transform.forward * 50f;
    }
}