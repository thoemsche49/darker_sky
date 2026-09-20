using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class PanController : MonoBehaviour
{
    public ScreenTransformGesture OneFingerPanGesture;

    [Tooltip("Bewegung pro Pixel Fingerbewegung")]
    public float PanSpeed = 0.02f;

    private float fixedY;

    private void Awake()
    {
        fixedY = transform.position.y;
    }

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

        // Pan in Kamera-Ausrichtung: Die Karte folgt der Fingerbewegung -
        // auch nach einer Rotation der Kamera (rechts = Bildrechts,
        // oben = Bildoberkante).
        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 screenUp = transform.up;
        screenUp.y = 0f;
        if (screenUp.sqrMagnitude < 0.0001f)
            screenUp = -transform.forward;
        screenUp.Normalize();

        Vector3 movement = (-delta.x * right - delta.y * screenUp) * PanSpeed;
        transform.position += movement;

        // Kamera bleibt immer auf derselben Höhe
        Vector3 position = transform.position;
        position.y = fixedY;
        transform.position = position;
    }
}