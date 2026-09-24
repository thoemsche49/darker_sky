using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class RotateController : MonoBehaviour
{
    public ScreenTransformGesture RotateGesture;

    [Tooltip("Hoehe der Kartenebene (Welt-Y), um die die Kamera kreist")]
    public float GroundLevel = 0f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        Debug.Log("RotateController gestartet");

        if (RotateGesture == null)
        {
            Debug.LogError("RotateGesture fehlt");
            return;
        }

        RotateGesture.Transformed += OnTransformed;
    }

    private void OnDisable()
    {
        if (RotateGesture != null)
            RotateGesture.Transformed -= OnTransformed;
    }

    private void OnTransformed(object sender, System.EventArgs e)
    {
        float angle = RotateGesture.DeltaRotation;

        // Pivot: Punkt der Kartenebene direkt unter der Bildschirmmitte.
        // Die Kamera kreist um diesen Punkt (Orbit) und schaut ihn dabei die
        // ganze Zeit an. Dadurch dreht sich die Karte um die Bildschirmmitte
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector3 pivot = GetGroundPoint(screenCenter);

        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 offset = rotation * (transform.position - pivot);
        transform.position = pivot + offset;
        transform.rotation = Quaternion.LookRotation((pivot - transform.position).normalized, Vector3.up);
    }

    private Vector3 GetGroundPoint(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, GroundLevel, 0f));
        float d;
        if (ground.Raycast(ray, out d))
            return ray.GetPoint(d);

        // Fallback, falls der Strahl die Ebene verfehlt
        return cam.transform.position + cam.transform.forward * 50f;

    }
}