using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class PanController : MonoBehaviour
{
    public ScreenTransformGesture OneFingerPanGesture;

    [Tooltip("Bewegung pro Pixel Fingerbewegung")]
    public float PanSpeed = 0.1f;

    [Header("Dynamische Pan-Geschwindigkeit")]
    public float ReferenceHeight = 200f;
    public float MinPanSpeed = 0.1f;
    public float MaxPanSpeed = 1.0f;

    [Header("Kamera-Grenzen")]
    public float MinX = -1000f;
    public float MaxX = 1000f;
    public float MinZ = -1000f;
    public float MaxZ = 1000f;

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

        float heightFactor =
            transform.position.y / ReferenceHeight;

        float currentPanSpeed =
            Mathf.Clamp(
                PanSpeed * heightFactor,
                MinPanSpeed,
                MaxPanSpeed
            );

        Vector3 movement =
            (-delta.x * right - delta.y * screenUp)
            * currentPanSpeed;      
        transform.position += movement;

        // Kamera bleibt immer auf derselben Höhe
        Vector3 position = transform.position;
        // position.y = fixedY;

        // Bewegung innerhalb der Karten-Grenzen halten
        position.x = Mathf.Clamp(position.x, MinX, MaxX);
        position.z = Mathf.Clamp(position.z, MinZ, MaxZ);

        transform.position = position;
    }
}