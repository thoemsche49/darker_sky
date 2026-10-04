using UnityEngine;
using UnityEngine.UI;

public class ScreenRotation : MonoBehaviour
{
    [Header("Kamera")]
    public Camera mainCamera;

    [Header("UI Elemente")]
    public RectTransform mapButton;
    public RectTransform menuButton;
    public RectTransform zurueckButton;
    public RectTransform mapSlider;
    public RectTransform legend;
    public RectTransform buttonSwitch;

    private bool istGedreht = false;

    // Positionen Normal
    private Vector2 mapButtonNormal;
    private Vector2 menuButtonNormal;
    private Vector2 zurueckButtonNormal;
    private Vector2 mapSliderNormal;
    private Vector2 legendNormal;
    private Vector2 buttonSwitchNormal;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Startpositionen merken
        if (mapButton != null) mapButtonNormal = mapButton.anchoredPosition;
        if (menuButton != null) menuButtonNormal = menuButton.anchoredPosition;
        if (zurueckButton != null) zurueckButtonNormal = zurueckButton.anchoredPosition;
        if (mapSlider != null) mapSliderNormal = mapSlider.anchoredPosition;
        if (legend != null) legendNormal = legend.anchoredPosition;
        if (buttonSwitch != null) buttonSwitchNormal = buttonSwitch.anchoredPosition;
    }

    public void Drehen()
    {
        istGedreht = !istGedreht;

        float zRotation = istGedreht ? 180f : 0f;

        // Kamera drehen
        Vector3 rot = mainCamera.transform.eulerAngles;
        rot.z = zRotation;
        mainCamera.transform.eulerAngles = rot;

        // Alle UI Elemente drehen und spiegeln
        DreheElement(mapButton, mapButtonNormal);
        DreheElement(menuButton, menuButtonNormal);
        DreheElement(zurueckButton, zurueckButtonNormal);
        DreheElement(mapSlider, mapSliderNormal);
        DreheElement(legend, legendNormal);
        DreheElement(buttonSwitch, buttonSwitchNormal);
    }

    void DreheElement(RectTransform element, Vector2 normalPosition)
    {
        if (element == null) return;

        if (istGedreht)
        {
            element.anchoredPosition = new Vector2(-normalPosition.x, -normalPosition.y);
            element.localEulerAngles = new Vector3(0, 0, 180f);
        }
        else
        {
            element.anchoredPosition = normalPosition;
            element.localEulerAngles = Vector3.zero;
        }
    }
}