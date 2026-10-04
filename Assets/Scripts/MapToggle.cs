using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MapController : MonoBehaviour
{
    [Header("Light Pollution Maps")]
    public GameObject map1;  // 25
    public GameObject map2;  // 50
    public GameObject map3;  // 75
    public GameObject map4;  // 100
    public GameObject map5;  // 200

    [Header("Positionen")]
    public float sichtbarY = 60f;
    public float unsichtbarY = -60f;

    [Header("Kamera Einstellungen")]
    public Camera mainCamera;
    public float zoomHoehe = 25000f;
    public float zoomDauer = 2f;

    [Header("UI")]
    public Button mapButton;
    public Button zurueckButton;
    public Slider mapSlider;
    public TextMeshProUGUI sliderLabel;
    public GameObject legend; 
    public Button creditsButton;
    public Button creditsClose;
    public GameObject creditsText;
    public Button menuButton;
    public Button quitButton;
    public Button resetButton;
    public Button steuerungButton;
    public Button steuerungClose;
    public GameObject steuerungText;
    public GameObject steuerungText15Finger; // 1-5 Finger (kein Karten-Overlay)
    public GameObject steuerungText12Finger; // 1-2 Finger (Karten-Overlay aktiv)

    // Strahlendichte Werte
    private int[] strahlenwerte = { 25, 50, 75, 100, 200 };
    private GameObject[] maps;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool istGezoomt = false;
    private bool istAnimiert = false;

    // Wird beim Druecken der Karten-/Zuerueck-Tasten ausgeloest (true = Overlay offen)
    public System.Action<bool> MapModeChanged;

    void Start()
    {
        maps = new GameObject[] { map1, map2, map3, map4, map5 };
        AlleAusschalten();

        if (mainCamera == null)
            mainCamera = Camera.main;

        originalPosition = mainCamera.transform.position;
        originalRotation = mainCamera.transform.rotation;

        // Zurück Button und Slider am Anfang verstecken
        if (zurueckButton != null)
            zurueckButton.gameObject.SetActive(false);
        if (mapSlider != null)
            mapSlider.gameObject.SetActive(false);
        if (sliderLabel != null)
            sliderLabel.gameObject.SetActive(false);

        // Slider Listener
        if (mapSlider != null)
            mapSlider.onValueChanged.AddListener(SliderGeaendert);
        
        // Credits-Text und Schließen-Button am Anfang verstecken
        if (creditsText != null)
            creditsText.gameObject.SetActive(false);
        if (creditsClose != null)
            creditsClose.gameObject.SetActive(false);

        // Button-Listener Credits
        if (creditsButton != null)
            creditsButton.onClick.AddListener(CreditsOeffnen);
        if (creditsClose != null)
            creditsClose.onClick.AddListener(CreditsSchliessen);

        // Steuerung-Text und Schließen-Button am Anfang verstecken
        if (steuerungText != null)
            steuerungText.gameObject.SetActive(false);
        if (steuerungClose != null)
            steuerungClose.gameObject.SetActive(false);
        if (steuerungText15Finger != null)
            steuerungText15Finger.gameObject.SetActive(false);
        if (steuerungText12Finger != null)
            steuerungText12Finger.gameObject.SetActive(false);

        // Button-Listener Steuerung
        if (steuerungButton != null)
            steuerungButton.onClick.AddListener(SteuerungOeffnen);
        if (steuerungClose != null)
            steuerungClose.onClick.AddListener(SteuerungSchliessen);

        // Label initial setzen
        AktualisierLabel(0);

        // Burger-Menü-Buttons am Anfang verstecken
        if (creditsButton != null)
            creditsButton.gameObject.SetActive(false);
        if (quitButton != null)
            quitButton.gameObject.SetActive(false);
        if (resetButton != null)
            resetButton.gameObject.SetActive(false);
        if (steuerungButton != null)
            steuerungButton.gameObject.SetActive(false);

        // Burger-Button Listener
        if (menuButton != null)
            menuButton.onClick.AddListener(BurgerMenuToggle);

        // Zusätzliche Listener: Menü schließen, wenn ein Punkt gewählt wird
        if (creditsButton != null)
            creditsButton.onClick.AddListener(BurgerMenuSchliessen);
        if (quitButton != null)
            quitButton.onClick.AddListener(BurgerMenuSchliessen);
        if (resetButton != null)
            resetButton.onClick.AddListener(BurgerMenuSchliessen);
        if (steuerungButton != null)
            steuerungButton.onClick.AddListener(BurgerMenuSchliessen);
    }

    // Haupt Button
    public void NaechsteMap()
    {
        if (istAnimiert) return;

        if (!istGezoomt)
        {
            MapModeChanged?.Invoke(true);
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;
            StartCoroutine(RausZoomenDannMap());
        }
    }

    // Zurück Button
    public void ZurueckZoomen()
    {
        if (istAnimiert) return;
        MapModeChanged?.Invoke(false);
        StartCoroutine(ReinZoomenUndAusschalten());
    }

    // Wird automatisch aufgerufen wenn Slider bewegt wird
    void SliderGeaendert(float wert)
    {
        int index = Mathf.RoundToInt(wert);
        AktualisierLabel(index);

        if (istGezoomt)
            MapAnzeigen(maps[index]);
    }

    void AktualisierLabel(int index)
    {
        if (sliderLabel != null)
            sliderLabel.text = strahlenwerte[index] + "";
    }

    public void CreditsOeffnen()
    {
        if (creditsText != null)
            creditsText.gameObject.SetActive(true);
        if (creditsClose != null)
            creditsClose.gameObject.SetActive(true);
    }

    public void CreditsSchliessen()
    {
        if (creditsText != null)
            creditsText.gameObject.SetActive(false);
        if (creditsClose != null)
            creditsClose.gameObject.SetActive(false);
    }

    public void SteuerungOeffnen()
    {
        if (steuerungText != null)
            steuerungText.gameObject.SetActive(true);
        if (steuerungClose != null)
            steuerungClose.gameObject.SetActive(true);

        // Karten-Overlay aktiv? -> 1-2 Finger, sonst 1-5 Finger
        if (steuerungText15Finger != null)
            steuerungText15Finger.gameObject.SetActive(!istGezoomt);
        if (steuerungText12Finger != null)
            steuerungText12Finger.gameObject.SetActive(istGezoomt);
    }

    public void SteuerungSchliessen()
    {
        if (steuerungText != null)
            steuerungText.gameObject.SetActive(false);
        if (steuerungClose != null)
            steuerungClose.gameObject.SetActive(false);
        if (steuerungText15Finger != null)
            steuerungText15Finger.gameObject.SetActive(false);
        if (steuerungText12Finger != null)
            steuerungText12Finger.gameObject.SetActive(false);
    }

    private bool istMenuOffen = false;

    public void BurgerMenuToggle()
    {
        istMenuOffen = !istMenuOffen;

        if (creditsButton != null)
            creditsButton.gameObject.SetActive(istMenuOffen);
        if (quitButton != null)
            quitButton.gameObject.SetActive(istMenuOffen);
        if (resetButton != null)
            resetButton.gameObject.SetActive(istMenuOffen);
        if (steuerungButton != null)
            steuerungButton.gameObject.SetActive(istMenuOffen);
    }

    public void BurgerMenuSchliessen()
    {
        istMenuOffen = false;

        if (creditsButton != null)
            creditsButton.gameObject.SetActive(false);
        if (quitButton != null)
            quitButton.gameObject.SetActive(false);
        if (resetButton != null)
            resetButton.gameObject.SetActive(false);
        if (steuerungButton != null)
            steuerungButton.gameObject.SetActive(false);
    }

    IEnumerator RausZoomenDannMap()
    {
        istAnimiert = true;

        if (mapButton != null) {
            mapButton.interactable = false;
        }

        Vector3 zielPosition = new Vector3(
            mainCamera.transform.position.x,
            originalPosition.y + zoomHoehe,
            mainCamera.transform.position.z
        );
        Quaternion zielRotation = Quaternion.Euler(90f, 0f, 0f);

        float timer = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (timer < zoomDauer)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / zoomDauer);
            t = t * t * (3f - 2f * t);
            mainCamera.transform.position = Vector3.Lerp(startPos, zielPosition, t);
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, zielRotation, t);
            yield return null;
        }

        istGezoomt = true;
        istAnimiert = false;

        // Slider + Label + Zurück Button + Legende einblenden
        if (zurueckButton != null)
            zurueckButton.gameObject.SetActive(true);
            zurueckButton.interactable = true;
        if (mapSlider != null)
            mapSlider.gameObject.SetActive(true);
        if (sliderLabel != null)
            sliderLabel.gameObject.SetActive(true);
        if (legend != null)
            legend.gameObject.SetActive(true);
        // Map Button ausblenden
        if (mapButton != null)
            mapButton.gameObject.SetActive(false);

        // Erste Map anzeigen
        mapSlider.value = 0;
        MapAnzeigen(maps[0]);
    }

    IEnumerator ReinZoomenUndAusschalten()
    {
        istAnimiert = true;
        AlleAusschalten();

        if (zurueckButton != null) {
            zurueckButton.interactable = false;
        }
        // Slider + Label + Legende verstecken

        if (mapSlider != null)
            mapSlider.gameObject.SetActive(false);
        if (sliderLabel != null)
            sliderLabel.gameObject.SetActive(false);
        if (legend != null)
            legend.gameObject.SetActive(false);

        float timer = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (timer < zoomDauer)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / zoomDauer);
            t = t * t * (3f - 2f * t);
            mainCamera.transform.position = Vector3.Lerp(startPos, originalPosition, t);
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, originalRotation, t);
            yield return null;
        }

        istGezoomt = false;
        istAnimiert = false;

        if (zurueckButton != null)
            zurueckButton.gameObject.SetActive(false);

        if (mapButton != null)
        {
            mapButton.gameObject.SetActive(true);
            mapButton.interactable = true;
        }
    }

    void MapAnzeigen(GameObject map)
    {
        AlleAusschalten();
        Vector3 pos = map.transform.position;
        pos.y = sichtbarY;
        map.transform.position = pos;
    }

    void AlleAusschalten()
    {
        if (maps == null) return;
        foreach (GameObject map in maps)
        {
            if (map != null)
            {
                Vector3 pos = map.transform.position;
                pos.y = unsichtbarY;
                map.transform.position = pos;
            }
        }
    }
}