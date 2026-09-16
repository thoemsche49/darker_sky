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

        Vector3 movement = new Vector3(
            -delta.x * PanSpeed,
            0f,
            -delta.y * PanSpeed
        );

        transform.position += movement;

        // Kamera bleibt immer auf derselben Höhe
        Vector3 position = transform.position;
        position.y = fixedY;
        transform.position = position;
    }
}