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
        if (RotateGesture == null) return;
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

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector3 pivot = GetGroundPoint(screenCenter);

        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 offset = rotation * (transform.position - pivot);
        transform.position = pivot + offset;

        // ← Z-Rotation merken
        float aktuellesZ = transform.eulerAngles.z;

        transform.rotation = Quaternion.LookRotation(
            (pivot - transform.position).normalized, Vector3.up);

        // ← Z-Rotation wiederherstellen
        Vector3 euler = transform.eulerAngles;
        euler.z = aktuellesZ;
        transform.eulerAngles = euler;
    }

    private Vector3 GetGroundPoint(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, GroundLevel, 0f));
        float d;
        if (ground.Raycast(ray, out d))
            return ray.GetPoint(d);
        return cam.transform.position + cam.transform.forward * 50f;
    }
}