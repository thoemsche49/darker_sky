using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Zooms the camera by moving it up/down while a two-finger pinch is active.
/// A pinch ratio of 1 keeps the current height, larger ratios move the camera
/// down (zoom in), smaller ratios move it up (zoom out). The world point below
/// the screen centre stays fixed while zooming.
/// </summary>
public class ZoomController : MonoBehaviour
{
    [Tooltip("Two-finger pinch gesture that drives the zoom.")]
    public ScreenTransformGesture ZoomGesture;

    [Header("Standard Zoom Range")]
    [Tooltip("Lowest camera height while zooming.")]
    public float MinHeight = 50f;

    [Tooltip("Highest camera height while zooming.")]
    public float MaxHeight = 5000f;

    [Header("Extended Zoom Range")]
    [FormerlySerializedAs("CheckedButton")]
    [Tooltip("If this button is not available (hidden/disabled), the extended range is used instead.")]
    public Button RangeToggleButton;

    [FormerlySerializedAs("DisabledMinHeight")]
    [Tooltip("Lowest camera height in the extended range.")]
    public float ExtendedMinHeight = 10000f;

    [FormerlySerializedAs("DisabledMaxHeight")]
    [Tooltip("Highest camera height in the extended range.")]
    public float ExtendedMaxHeight = 25400f;

    [Tooltip("World Y of the map plane used as zoom pivot.")]
    public float GroundLevel = 0f;

    [Tooltip("Zoom speed multiplier centred on 1.0: higher = faster zoom, 1 = unchanged behaviour.")]
    public float ZoomSpeed = 0.5f;

    private Camera cachedCamera;
    private float lastFingerDistance = -1f;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
        if (cachedCamera == null)
            cachedCamera = Camera.main;
    }

    private void Update()
    {
        if (ZoomGesture == null || cachedCamera == null)
            return;

        // Only a pinch with exactly two fingers zooms; any other count aborts.
        if (ZoomGesture.NumPointers != 2)
        {
            lastFingerDistance = -1f;
            return;
        }

        var pointers = ZoomGesture.ActivePointers;
        float distance = Vector2.Distance(pointers[0].Position, pointers[1].Position);

        if (lastFingerDistance <= 0f)
        {
            lastFingerDistance = distance;
            return;
        }

        float ratio;
        if (lastFingerDistance > 0.0001f && distance > 0.0001f)
        {
            ratio = distance / lastFingerDistance;
        }
        else
        {
            lastFingerDistance = distance;
            return;
        }

        lastFingerDistance = distance;

        if (Mathf.Abs(ratio - 1f) < 0.0005f)
            return;

        // Scale the ratio so that 1 (no movement) always stays 1.
        float zoomFactor = 1f + (ratio - 1f) * ZoomSpeed;
        if (zoomFactor < 0.01f)
            zoomFactor = 0.01f;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector3 pivotBefore = GetGroundPoint(screenCenter);

        // Use the standard or the extended zoom range depending on the button state.
        bool standardRangeActive =
            RangeToggleButton != null &&
            RangeToggleButton.gameObject.activeInHierarchy &&
            RangeToggleButton.enabled &&
            RangeToggleButton.interactable;

        float currentMinHeight = standardRangeActive ? MinHeight : ExtendedMinHeight;
        float currentMaxHeight = standardRangeActive ? MaxHeight : ExtendedMaxHeight;

        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y / zoomFactor, currentMinHeight, currentMaxHeight);
        transform.position = position;

        // Keep the world point below the screen centre fixed while zooming.
        Vector3 pivotAfter = GetGroundPoint(screenCenter);
        transform.position += pivotBefore - pivotAfter;
    }

    private Vector3 GetGroundPoint(Vector2 screenPosition)
    {
        Ray ray = cachedCamera.ScreenPointToRay(screenPosition);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, GroundLevel, 0f));
        float distance;
        if (ground.Raycast(ray, out distance))
            return ray.GetPoint(distance);
        return transform.position;
    }
}