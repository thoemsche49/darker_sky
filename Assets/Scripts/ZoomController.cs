using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class ZoomController : MonoBehaviour
{
    public ScreenTransformGesture ZoomGesture;

    public float MinHeight = 50f;
    public float MaxHeight = 5000f;
    public float GroundLevel = 0f;

    private Camera cam;
    private float lastDistance = -1f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        Debug.Log(
            "ZoomController.Awake | Objekt: " +
            gameObject.name +
            " | Kamera: " +
            (cam != null ? cam.gameObject.name : "FEHLT")
        );
    }

    private void OnEnable()
    {
        Debug.Log(
            "ZoomController.OnEnable | ZoomGesture: " +
            (ZoomGesture != null ? ZoomGesture.gameObject.name : "NULL")
        );
    }

    private void Update()
    {
        if (ZoomGesture == null || cam == null)
            return;

        if (ZoomGesture.NumPointers < 2)
        {
            lastDistance = -1f;
            return;
        }

        var pointers = ZoomGesture.ActivePointers;
        float distance = Vector2.Distance(pointers[0].Position, pointers[1].Position);

        if (lastDistance <= 0f)
        {
            lastDistance = distance;
            return;
        }

        float ratio;
        if (lastDistance > 0.0001f && distance > 0.0001f)
            ratio = distance / lastDistance;
        else
        {
            lastDistance = distance;
            return;
        }

        lastDistance = distance;

        if (Mathf.Abs(ratio - 1f) < 0.0005f)
            return;

        Debug.Log(
            "ZoomController.Update | Abstand: " +
            distance +
            " | Verhaeltnis: " +
            ratio +
            " | Y vorher: " +
            transform.position.y
        );

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector3 pivotBefore = GetGroundPoint(screenCenter);

        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y / ratio, MinHeight, MaxHeight);
        transform.position = position;

        Vector3 pivotAfter = GetGroundPoint(screenCenter);
        transform.position += pivotBefore - pivotAfter;

        Debug.Log("ZoomController.Update | Y nachher: " + transform.position.y);
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