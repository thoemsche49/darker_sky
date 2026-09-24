using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class ZoomController : MonoBehaviour
{
    public ScreenTransformGesture ZoomGesture;

    public float MinHeight = 50f;
    public float MaxHeight = 5000f;
    public float GroundLevel = 0f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }

    private void OnEnable()
    {
        if (ZoomGesture != null)
            ZoomGesture.Transformed += OnTransformed;
    }

    private void OnDisable()
    {
        if (ZoomGesture != null)
            ZoomGesture.Transformed -= OnTransformed;
    }

    private void OnTransformed(object sender, System.EventArgs e)
    {
        if (cam == null)
            return;

        float deltaScale = ZoomGesture.DeltaScale;
        if (deltaScale <= 0.0001f)
            return;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector3 pivotBefore = GetGroundPoint(screenCenter);

        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y / deltaScale, MinHeight, MaxHeight);
        transform.position = position;

        Vector3 pivotAfter = GetGroundPoint(screenCenter);
        transform.position += pivotBefore - pivotAfter;
    }

    private Vector3 GetGroundPoint(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, GroundLevel, 0f));
        float enter;
        if (ground.Raycast(ray, out enter))
            return ray.GetPoint(enter);
        return transform.position;
    }
}