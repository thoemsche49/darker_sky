using UnityEngine;
using UnityEngine.UI;
using TouchScript.Gestures.TransformGestures;

public class GestureAvailability : MonoBehaviour
{
    [SerializeField] private Button checkedButton;

    [Header("Objekt mit mehreren Gesten")]
    [SerializeField] private GameObject multiGestureObject;

    [Header("Objekt mit einzelner Gesture")]
    [SerializeField] private GameObject singleGestureObject;

    private ScreenTransformGesture[] multiGestures;
    private ScreenTransformGesture singleGesture;
    private bool? previousState;

    private void Awake()
    {
        if (multiGestureObject != null)
        {
            multiGestures =
                multiGestureObject.GetComponents<ScreenTransformGesture>();
        }

        if (singleGestureObject != null)
        {
            singleGesture =
                singleGestureObject.GetComponent<ScreenTransformGesture>();
        }

        UpdateGestureState();
    }

    private void Update()
    {
        UpdateGestureState();
    }

    private void UpdateGestureState()
    {
        bool buttonIsAvailable =
            checkedButton != null &&
            checkedButton.gameObject.activeInHierarchy &&
            checkedButton.enabled &&
            checkedButton.interactable;

        if (previousState == buttonIsAvailable)
            return;

        previousState = buttonIsAvailable;

        // Nur Gesten für mindestens drei Finger umschalten.
        if (multiGestures != null)
        {
            foreach (ScreenTransformGesture gesture in multiGestures)
            {
                if (gesture != null && gesture.MinPointers >= 3)
                    gesture.enabled = buttonIsAvailable;
            }
        }

        // Die einzelne Gesture vollständig umschalten.
        if (singleGesture != null)
            singleGesture.enabled = buttonIsAvailable;
    }
}