using UnityEngine;
using UnityEngine.UI;

public class ResetButtonAvailability : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private Button blockingButton;

    private void Update()
    {
        if (resetButton == null || blockingButton == null)
            return;

        bool blockingButtonIsActive =
            blockingButton.gameObject.activeInHierarchy &&
            blockingButton.enabled &&
            blockingButton.interactable;

        resetButton.interactable =
            !blockingButtonIsActive;
    }
}