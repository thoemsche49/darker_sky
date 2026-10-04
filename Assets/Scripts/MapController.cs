using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Manages the light-pollution map overlay:
/// opening and closing the top-down map view, switching the pollution level
/// via the slider, and the credits / controls / burger-menu UI.
/// </summary>
public class MapController : MonoBehaviour
{
    [Header("Pollution Level Maps")]
    [FormerlySerializedAs("map1")] public GameObject map25;
    [FormerlySerializedAs("map2")] public GameObject map50;
    [FormerlySerializedAs("map3")] public GameObject map75;
    [FormerlySerializedAs("map4")] public GameObject map100;
    [FormerlySerializedAs("map5")] public GameObject map200;

    [Header("Map Visibility")]
    [FormerlySerializedAs("sichtbarY")]
    [Tooltip("World Y at which a map becomes visible.")]
    public float visibleMapHeight = 60f;

    [FormerlySerializedAs("unsichtbarY")]
    [Tooltip("World Y at which a map is hidden.")]
    public float hiddenMapHeight = -60f;

    [Header("Camera")]
    [FormerlySerializedAs("mainCamera")] public Camera sceneCamera;
    [FormerlySerializedAs("zoomHoehe")]
    [Tooltip("Height the camera rises to while the map view is open.")]
    public float mapViewHeight = 25000f;

    [FormerlySerializedAs("zoomDauer")]
    [Tooltip("Duration of the camera flight into/out of the map view.")]
    public float mapTransitionDuration = 2f;

    [Header("Map UI")]
    public Button mapButton;
    [FormerlySerializedAs("zurueckButton")] public Button backButton;
    public Slider mapSlider;
    public TextMeshProUGUI sliderLabel;
    public GameObject legend;

    [Header("Credits")]
    public Button creditsButton;
    [FormerlySerializedAs("creditsClose")] public Button creditsCloseButton;
    [FormerlySerializedAs("creditsText")] public GameObject creditsPanel;

    [Header("Burger Menu")]
    public Button menuButton;
    public Button quitButton;
    public Button resetButton;

    [Header("Controls (How To Use)")]
    [FormerlySerializedAs("steuerungButton")] public Button controlsButton;
    [FormerlySerializedAs("steuerungClose")] public Button controlsCloseButton;
    [FormerlySerializedAs("steuerungText")] public GameObject controlsPanel;
    [FormerlySerializedAs("steuerungText15Finger")]
    [Tooltip("Shown when no map overlay is open (1-5 finger gestures).")]
    public GameObject controlsFreeNavigationText;
    [FormerlySerializedAs("steuerungText12Finger")]
    [Tooltip("Shown while the map overlay is open (1-2 finger gestures).")]
    public GameObject controlsMapModeText;

    /// <summary>Pollution levels selectable via the slider, one per map.</summary>
    private readonly int[] pollutionLevels = { 25, 50, 75, 100, 200 };

    private GameObject[] maps;

    // Camera pose right before the map was opened; returned to when leaving the map view.
    private Vector3 previousPosition;
    private Quaternion previousRotation;

    // Real camera pose captured at app launch; target for the camera reset button.
    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool isMapOpen;
    private bool isTransitioning;
    private bool isMenuOpen;

    /// <summary>Camera position at app start; used by the camera reset button.</summary>
    public Vector3 StartPosition => startPosition;

    /// <summary>Camera rotation at app start; used by the camera reset button.</summary>
    public Quaternion StartRotation => startRotation;

    /// <summary>Whether the map overlay is currently open.</summary>
    public bool IsMapOpen => isMapOpen;

    /// <summary>
    /// Raised whenever the map overlay opens (<c>true</c>) or closes (<c>false</c>),
    /// e.g. used by <see cref="DayNightCycle"/> to pause the day/night cycle.
    /// </summary>
    public event Action<bool> MapViewChanged;

    private void Start()
    {
        maps = new GameObject[] { map25, map50, map75, map100, map200 };
        HideAllMaps();

        if (sceneCamera == null)
            sceneCamera = Camera.main;

        previousPosition = sceneCamera.transform.position;
        previousRotation = sceneCamera.transform.rotation;
        startPosition = sceneCamera.transform.position;
        startRotation = sceneCamera.transform.rotation;

        // The back arrow, slider and its label are only visible inside the map view.
        SetActive(backButton, false);
        SetActive(mapSlider, false);
        SetActive(sliderLabel, false);

        if (mapSlider != null)
            mapSlider.onValueChanged.AddListener(OnMapSliderChanged);

        SetActive(creditsPanel, false);
        SetActive(creditsCloseButton, false);

        mapButton?.onClick.AddListener(OpenMapView);
        backButton?.onClick.AddListener(ExitMapView);
        creditsButton?.onClick.AddListener(OpenCredits);
        creditsCloseButton?.onClick.AddListener(CloseCredits);

        SetActive(controlsPanel, false);
        SetActive(controlsCloseButton, false);
        SetActive(controlsFreeNavigationText, false);
        SetActive(controlsMapModeText, false);

        controlsButton?.onClick.AddListener(OpenControls);
        controlsCloseButton?.onClick.AddListener(CloseControls);

        UpdateSliderLabel(0);

        // In the burger menu only the burger button itself is visible at start.
        SetActive(creditsButton, false);
        SetActive(quitButton, false);
        SetActive(resetButton, false);
        SetActive(controlsButton, false);

        menuButton?.onClick.AddListener(ToggleMenu);

        // Selecting a menu item also closes the menu again.
        creditsButton?.onClick.AddListener(CloseMenu);
        quitButton?.onClick.AddListener(CloseMenu);
        resetButton?.onClick.AddListener(CloseMenu);
        controlsButton?.onClick.AddListener(CloseMenu);
    }

    /// <summary>Opens the light-pollution map view. Called by the map button.</summary>
    public void OpenMapView()
    {
        if (isTransitioning)
            return;

        if (!isMapOpen)
        {
            MapViewChanged?.Invoke(true);
            previousPosition = sceneCamera.transform.position;
            previousRotation = sceneCamera.transform.rotation;
            StartCoroutine(MoveToMapView());
        }
    }

    /// <summary>Returns the camera to its pre-map pose. Called by the back arrow.</summary>
    public void ExitMapView()
    {
        if (isTransitioning)
            return;

        MapViewChanged?.Invoke(false);
        StartCoroutine(ReturnToPreviousView());
    }

    /// <summary>Called while the slider moves; selects the pollution level.</summary>
    private void OnMapSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        UpdateSliderLabel(index);

        if (isMapOpen)
            ShowMap(maps[index]);
    }

    private void UpdateSliderLabel(int index)
    {
        if (sliderLabel != null)
            sliderLabel.text = pollutionLevels[index].ToString();
    }

    public void OpenCredits()
    {
        SetActive(creditsPanel, true);
        SetActive(creditsCloseButton, true);
    }

    public void CloseCredits()
    {
        SetActive(creditsPanel, false);
        SetActive(creditsCloseButton, false);
    }

    public void OpenControls()
    {
        SetActive(controlsPanel, true);
        SetActive(controlsCloseButton, true);

        // Show the appropriate gesture help: free navigation (1-5 fingers)
        // unless the map overlay is active (then 1-2 fingers).
        SetActive(controlsFreeNavigationText, !isMapOpen);
        SetActive(controlsMapModeText, isMapOpen);
    }

    public void CloseControls()
    {
        SetActive(controlsPanel, false);
        SetActive(controlsCloseButton, false);
        SetActive(controlsFreeNavigationText, false);
        SetActive(controlsMapModeText, false);
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        SetActive(creditsButton, isMenuOpen);
        SetActive(quitButton, isMenuOpen);
        SetActive(resetButton, isMenuOpen);
        SetActive(controlsButton, isMenuOpen);
    }

    public void CloseMenu()
    {
        isMenuOpen = false;

        SetActive(creditsButton, false);
        SetActive(quitButton, false);
        SetActive(resetButton, false);
        SetActive(controlsButton, false);
    }

    private IEnumerator MoveToMapView()
    {
        isTransitioning = true;

        if (mapButton != null)
            mapButton.interactable = false;

        // Fly straight up and look down at the map, staying above the current XZ position.
        Vector3 targetPosition = new Vector3(
            sceneCamera.transform.position.x,
            previousPosition.y + mapViewHeight,
            sceneCamera.transform.position.z);
        Quaternion targetRotation = Quaternion.Euler(90f, 0f, 0f);

        float elapsedTime = 0f;
        Vector3 startPosition = sceneCamera.transform.position;
        Quaternion startRotation = sceneCamera.transform.rotation;

        while (elapsedTime < mapTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / mapTransitionDuration);
            t = t * t * (3f - 2f * t); // Smoothstep

            sceneCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            sceneCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        isMapOpen = true;
        isTransitioning = false;

        // Show the map UI (back arrow, slider, label, legend) and hide the map button.
        SetActive(mapSlider, true);
        SetActive(sliderLabel, true);
        SetActive(legend, true);
        SetActive(backButton, true);
        if (backButton != null)
            backButton.interactable = true;
        mapButton?.gameObject.SetActive(false);

        // Start with the lowest pollution level.
        mapSlider.value = 0;
        ShowMap(maps[0]);
    }

    private IEnumerator ReturnToPreviousView()
    {
        isTransitioning = true;
        HideAllMaps();

        if (backButton != null)
            backButton.interactable = false;

        SetActive(mapSlider, false);
        SetActive(sliderLabel, false);
        SetActive(legend, false);

        float elapsedTime = 0f;
        Vector3 startPosition = sceneCamera.transform.position;
        Quaternion startRotation = sceneCamera.transform.rotation;

        while (elapsedTime < mapTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / mapTransitionDuration);
            t = t * t * (3f - 2f * t); // Smoothstep

            sceneCamera.transform.position = Vector3.Lerp(startPosition, previousPosition, t);
            sceneCamera.transform.rotation = Quaternion.Lerp(startRotation, previousRotation, t);
            yield return null;
        }

        isMapOpen = false;
        isTransitioning = false;

        SetActive(backButton, false);

        if (mapButton != null)
        {
            mapButton.gameObject.SetActive(true);
            mapButton.interactable = true;
        }
    }

    private void ShowMap(GameObject map)
    {
        HideAllMaps();

        Vector3 position = map.transform.position;
        position.y = visibleMapHeight;
        map.transform.position = position;
    }

    private void HideAllMaps()
    {
        if (maps == null)
            return;

        foreach (GameObject map in maps)
        {
            if (map != null)
            {
                Vector3 position = map.transform.position;
                position.y = hiddenMapHeight;
                map.transform.position = position;
            }
        }
    }

    private static void SetActive(Component component, bool active)
    {
        if (component != null)
            component.gameObject.SetActive(active);
    }

    private static void SetActive(GameObject gameObject, bool active)
    {
        if (gameObject != null)
            gameObject.SetActive(active);
    }
}