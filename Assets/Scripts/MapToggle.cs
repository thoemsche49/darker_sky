using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [Header("Light Pollution Maps")]
    public GameObject map1;
    public GameObject map2;
    public GameObject map3;
    public GameObject map4;
    public GameObject map5;

    [Header("Positionen")]
    public float sichtbarY = 60f;
    public float unsichtbarY = -60f;

    [Header("Kamera Einstellungen")]
    public Camera mainCamera;
    public float zoomHoehe = 8000f;
    public float zoomDauer = 2f;

    [Header("UI")]
    public Button zurueckButton;  // Zurück Button hier reinziehen

    private int aktiveMap = -1;
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

        // Zurück Button am Anfang verstecken
        if (zurueckButton != null)
            zurueckButton.gameObject.SetActive(false);
    }

    // Haupt Button - macht alles
    public void NaechsteMap()
    {
        if (istAnimiert) return;

        if (!istGezoomt)
        {
            // Position JETZT speichern bevor gezoomt wird
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;

            StartCoroutine(RausZoomenDannMap());
        }
        else
        {
            // Schon gezoomt → nächste Map
            aktiveMap++;

            if (aktiveMap >= maps.Length)
            {
                // Letzte Map war aktiv → zurückzoomen
                StartCoroutine(ReinZoomenUndAusschalten());
            }
            else
            {
                MapAnzeigen(maps[aktiveMap]);
            }
        }
    }

    // Zurück Button
    public void ZurueckZoomen()
    {
        if (istAnimiert) return;
        StartCoroutine(ReinZoomenUndAusschalten());
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

        // Zurück Button einblenden
        if (zurueckButton != null)
            zurueckButton.gameObject.SetActive(true);

        // Erste Map anzeigen
        aktiveMap = 0;
        MapAnzeigen(maps[aktiveMap]);
    }

    IEnumerator ReinZoomenUndAusschalten()
    {
        istAnimiert = true;
        AlleAusschalten();
        aktiveMap = -1;

        // Zurück Button verstecken
        if (zurueckButton != null)
            zurueckButton.gameObject.SetActive(false);

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