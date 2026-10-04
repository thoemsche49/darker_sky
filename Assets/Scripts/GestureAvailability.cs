using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Toggles TouchScript gestures depending on a UI button's availability.
/// Whenever the button is shown and interactive, the gestures are enabled.
/// Used to switch between the free-navigation gestures (>= 3 fingers) and a
/// single gesture while the map overlay is open.
/// </summary>
public class GestureAvailability : MonoBehaviour
{
    [FormerlySerializedAs("checkedButton")]
    [SerializeField] private Button availabilityButton;

    [Header("Object With Multiple Gestures")]
    [SerializeField] private GameObject multiGestureObject;

    [Header("Object With a Single Gesture")]
    [SerializeField] private GameObject singleGestureObject;

    private ScreenTransformGesture[] multiGestures;
    private ScreenTransformGesture singleGesture;
    private bool? lastButtonAvailable;

    private void Awake()
    {
        if (multiGestureObject != null)
            multiGestures = multiGestureObject.GetComponents<ScreenTransformGesture>();

        if (singleGestureObject != null)
            singleGesture = singleGestureObject.GetComponent<ScreenTransformGesture>();

        ApplyGestureState();
    }

    private void Update()
    {
        ApplyGestureState();
    }

    private void ApplyGestureState()
    {
        bool buttonIsAvailable =
            availabilityButton != null &&
            availabilityButton.gameObject.activeInHierarchy &&
            availabilityButton.enabled &&
            availabilityButton.interactable;

        if (lastButtonAvailable == buttonIsAvailable)
            return;

        lastButtonAvailable = buttonIsAvailable;

        // Toggle only the free-navigation gestures (those needing >= 3 fingers).
        if (multiGestures != null)
        {
            foreach (ScreenTransformGesture gesture in multiGestures)
            {
                if (gesture != null && gesture.MinPointers >= 3)
                    gesture.enabled = buttonIsAvailable;
            }
        }

        // Fully toggle the single gesture.
        if (singleGesture != null)
            singleGesture.enabled = buttonIsAvailable;
    }
}