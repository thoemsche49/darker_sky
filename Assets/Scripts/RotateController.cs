using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class RotateController : MonoBehaviour
{
    public ScreenTransformGesture RotateGesture;

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
        Debug.Log(
            "Rotation erkannt | Pointer: " +
            RotateGesture.NumPointers +
            " | Delta: " +
            RotateGesture.DeltaRotation
        );

        transform.Rotate(
            0f,
            -RotateGesture.DeltaRotation,
            0f,
            Space.World
        );
    }
}