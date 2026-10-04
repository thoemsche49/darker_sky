using System.Collections;
using UnityEngine;

/// <summary>
/// Smoothly moves the camera to the reset target when the reset button is pressed:
/// the app-start pose, or - while the map overlay is open - the default top-down
/// map view above the start position (the overlay stays open).
/// </summary>
public class CameraReset : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float resetDuration = 1.5f;

    [Header("Disabled During Reset")]
    [Tooltip("Behaviours (gestures, controllers) disabled while the reset animation runs.")]
    [SerializeField] private Behaviour[] controlsToDisable;

    [SerializeField] private MapController mapController;

    private bool[] previousStates;
    private bool isResetting;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (mapController == null)
            mapController = FindObjectOfType<MapController>();
    }

    /// <summary>Called by the reset button (scene onClick).</summary>
    public void ResetCamera()
    {
        if (targetCamera == null || mapController == null || isResetting)
            return;

        StartCoroutine(AnimateCameraToTarget());
    }

    private IEnumerator AnimateCameraToTarget()
    {
        isResetting = true;
        SetControlsEnabled(false);

        Vector3 startPosition = targetCamera.transform.position;
        Quaternion startRotation = targetCamera.transform.rotation;

        Vector3 resetPosition;
        Quaternion resetRotation;

        if (mapController.IsMapOpen)
        {
            // In map view: go to the default "zoomed out" top-down view of the
            // light-pollution map (above the start position). The map stays open.
            resetPosition = new Vector3(
                mapController.StartPosition.x,
                mapController.StartPosition.y + mapController.mapViewHeight,
                mapController.StartPosition.z);
            resetRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            // Normal view: return to the app-start pose.
            resetPosition = mapController.StartPosition;
            resetRotation = mapController.StartRotation;
        }

        float elapsedTime = 0f;

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / resetDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            targetCamera.transform.position = Vector3.Lerp(startPosition, resetPosition, t);
            targetCamera.transform.rotation = Quaternion.Slerp(startRotation, resetRotation, t);

            yield return null;
        }

        targetCamera.transform.SetPositionAndRotation(resetPosition, resetRotation);

        SetControlsEnabled(true);
        isResetting = false;
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (!enabled)
            previousStates = new bool[controlsToDisable.Length];

        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            Behaviour control = controlsToDisable[i];

            if (control == null || control == this)
                continue;

            if (!enabled)
            {
                previousStates[i] = control.enabled;
                control.enabled = false;
            }
            else
            {
                control.enabled = previousStates[i];
            }
        }
    }
}