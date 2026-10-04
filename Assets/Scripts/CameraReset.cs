using System.Collections;
using UnityEngine;

public class CameraReset : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float resetDuration = 1.5f;

    [Header("Während Reset deaktivieren")]
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

    public void ResetCamera()
    {
        if (targetCamera == null || mapController == null || isResetting)
            return;

        StartCoroutine(ResetCameraSmoothly());
    }

    private IEnumerator ResetCameraSmoothly()
    {
        isResetting = true;
        SetControlsEnabled(false);

        Vector3 startPosition = targetCamera.transform.position;
        Quaternion startRotation = targetCamera.transform.rotation;

        Vector3 resetPosition;
        Quaternion resetRotation;

        if (mapController.IsMapActive)
        {
            // In der Karte: auf die Default-"zoomed out"-Ansicht der
            // Lichtverschmutzungskarte ("uber dem Ursprung, Blick von oben);
            // der Kartenmodus bleibt dabei aktiv
            resetPosition = new Vector3(
                mapController.OriginPosition.x,
                mapController.OriginPosition.y + mapController.zoomHoehe,
                mapController.OriginPosition.z
            );
            resetRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            resetPosition = mapController.OriginPosition;
            resetRotation = mapController.OriginRotation;
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