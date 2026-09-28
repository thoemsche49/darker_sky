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

    // Strahlendichte Werte
    private int[] strahlenwerte = { 25, 50, 75, 100, 200 };
    private GameObject[] maps;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool istGezoomt = false;
    private bool istAnimiert = false;

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

        // Label initial setzen
        AktualisierLabel(0);
    }

    // Haupt Button
    public void NaechsteMap()
    {
        if (istAnimiert) return;

        if (!istGezoomt)
        {
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;
            StartCoroutine(RausZoomenDannMap());
        }
    }

    // Zurück Button
    public void ZurueckZoomen()
    {
        if (istAnimiert) return;
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
            sliderLabel.text = strahlenwerte[index] + " mcd/m²";
    }

    IEnumerator RausZoomenDannMap()
    {
        istAnimiert = true;

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

        // Slider + Label + Zurück Button einblenden
        if (zurueckButton != null)
            zurueckButton.gameObject.SetActive(true);
        if (mapSlider != null)
            mapSlider.gameObject.SetActive(true);
        if (sliderLabel != null)
            sliderLabel.gameObject.SetActive(true);
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

        // Slider + Label + Zurück Button verstecken
        if (zurueckButton != null)
            zurueckButton.gameObject.SetActive(false);
        if (mapSlider != null)
            mapSlider.gameObject.SetActive(false);
        if (sliderLabel != null)
            sliderLabel.gameObject.SetActive(false);
        // Map Button wieder einblenden
        if (mapButton != null)
            mapButton.gameObject.SetActive(true);

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