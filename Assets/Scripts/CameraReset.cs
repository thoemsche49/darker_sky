using System.Collections;
using UnityEngine;

public class CameraReset : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float resetDuration = 1.5f;

    [Header("Während Reset deaktivieren")]
    [SerializeField] private Behaviour[] controlsToDisable;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool[] previousStates;
    private bool isResetting;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        initialPosition = targetCamera.transform.position;
        initialRotation = targetCamera.transform.rotation;
    }

    public void ResetCamera()
    {
        if (targetCamera == null || isResetting)
            return;

        StartCoroutine(ResetCameraSmoothly());
    }

    private IEnumerator ResetCameraSmoothly()
    {
        isResetting = true;
        SetControlsEnabled(false);

        Vector3 startPosition =
            targetCamera.transform.position;

        Quaternion startRotation =
            targetCamera.transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsedTime / resetDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            targetCamera.transform.position =
                Vector3.Lerp(
                    startPosition,
                    initialPosition,
                    t
                );

            targetCamera.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    initialRotation,
                    t
                );

            yield return null;
        }

        targetCamera.transform.SetPositionAndRotation(
            initialPosition,
            initialRotation
        );

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