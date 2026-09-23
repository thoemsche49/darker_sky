using UnityEngine;

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

    public void ToggleHeight()
    {
        Vector3 before = targetCamera.transform.position;

        bool currentlyHigh =
            before.y > (lowHeight + highHeight) * 0.5f;

        float targetHeight = currentlyHigh
            ? lowHeight
            : highHeight;

        Vector3 position = before;
        position.y = targetHeight;

        targetCamera.transform.position = position;

        Debug.Log(
            "ToggleHeight | Objekt: " +
            gameObject.name +
            " | Instanz: " +
            GetInstanceID() +
            " | Vorher Y: " +
            before.y +
            " | Ziel Y: " +
            targetHeight
        );
    }
}