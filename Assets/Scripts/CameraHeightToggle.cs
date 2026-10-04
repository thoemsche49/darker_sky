using UnityEngine;

/// <summary>
/// Debug helper used by the test scene. Toggles the camera height between a
/// low and a high value and logs the result.
/// </summary>
public class CameraHeightToggle : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [SerializeField] private float lowHeight = 200f;
    [SerializeField] private float highHeight = 2000f;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    /// <summary>Called by the test-scene button (scene onClick).</summary>
    public void ToggleHeight()
    {
        Vector3 before = targetCamera.transform.position;

        bool isCurrentlyHigh = before.y > (lowHeight + highHeight) * 0.5f;
        float targetHeight = isCurrentlyHigh ? lowHeight : highHeight;

        Vector3 position = before;
        position.y = targetHeight;
        targetCamera.transform.position = position;

        Debug.Log(
            "ToggleHeight | Object: " + gameObject.name +
            " | Instance: " + GetInstanceID() +
            " | before Y: " + before.y +
            " | target Y: " + targetHeight);
    }
}