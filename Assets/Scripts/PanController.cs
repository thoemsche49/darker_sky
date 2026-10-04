using TouchScript.Gestures.TransformGestures;
using UnityEngine;

/// <summary>
/// Pans the camera with a one-finger drag. The movement is aligned to the
/// camera orientation so the map follows the finger even after the camera has
/// been rotated. The pan speed scales with the camera height and the position
/// is clamped to the configured map bounds.
/// </summary>
public class PanController : MonoBehaviour
{
    [Tooltip("One-finger pan gesture that drives the panning.")]
    public ScreenTransformGesture OneFingerPanGesture;

    [Tooltip("Base movement per pixel of finger travel.")]
    public float PanSpeed = 0.1f;

    [Header("Dynamic Pan Speed")]
    [Tooltip("Camera height at which PanSpeed applies unchanged.")]
    public float ReferenceHeight = 200f;

    [Tooltip("Minimum pan speed (at very low camera heights).")]
    public float MinPanSpeed = 0.1f;

    [Tooltip("Maximum pan speed (at very high camera heights).")]
    public float MaxPanSpeed = 1.0f;

    [Header("Camera Bounds")]
    public float MinX = -1000f;
    public float MaxX = 1000f;
    public float MinZ = -1000f;
    public float MaxZ = 1000f;

    private void OnEnable()
    {
        if (OneFingerPanGesture != null)
            OneFingerPanGesture.Transformed += OnTransformed;
    }

    private void OnDisable()
    {
        if (OneFingerPanGesture != null)
            OneFingerPanGesture.Transformed -= OnTransformed;
    }

    private void OnTransformed(object sender, System.EventArgs e)
    {
        Vector2 delta = OneFingerPanGesture.DeltaPosition;

        // Pan along the camera orientation: right of the screen = world right.
        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        // The finger moves along the screen plane, so grab the camera's screen-up.
        Vector3 screenUp = transform.up;
        screenUp.y = 0f;
        if (screenUp.sqrMagnitude < 0.0001f)
            screenUp = -transform.forward;
        screenUp.Normalize();

        // Scale the pan speed with the camera height.
        float heightFactor = transform.position.y / ReferenceHeight;
        float currentSpeed =
            Mathf.Clamp(PanSpeed * heightFactor, MinPanSpeed, MaxPanSpeed);

        Vector3 movement =
            (-delta.x * right - delta.y * screenUp) * currentSpeed;
        transform.position += movement;

        // Keep the movement within the map bounds.
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, MinX, MaxX);
        position.z = Mathf.Clamp(position.z, MinZ, MaxZ);
        transform.position = position;
    }
}