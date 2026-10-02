using UnityEngine;
using UnityEngine.UI;

public class ResetButtonAvailability : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private Button checkedButton;

    private void Update()
    {
        if (resetButton == null)
            return;

        bool checkedButtonIsActive =
            checkedButton != null &&
            checkedButton.gameObject.activeInHierarchy &&
            checkedButton.enabled &&
            checkedButton.interactable;

        resetButton.interactable = checkedButtonIsActive;
    }
}