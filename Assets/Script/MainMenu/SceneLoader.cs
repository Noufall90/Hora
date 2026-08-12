using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    // ============ SINGLETON ============
    public static SceneLoader Instance { get; private set; }

    // ============ VARIABLES ============
    [Header("Loading Screen References")]
    [Tooltip("Root GameObject of the loading screen, containing the background, progress text, and progress slider.")]
    [SerializeField] private GameObject loadingScreenRoot;

    [Tooltip("Slider that visually represents the scene loading progress.")]
    [SerializeField] private Slider progressSlider;

    [Tooltip("Text that shows the scene loading progress as a percentage.")]
    [SerializeField] private TMP_Text progressText;

    [Header("Loading Settings")]
    [Tooltip("Minimum time in seconds the loading screen stays visible, even if the scene loads faster than this.")]
    [SerializeField] private float minimumLoadDuration = 0.5f;

    private bool isLoading;

    // ============ UNITY EVENTS ============
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        loadingScreenRoot.SetActive(false);
    }

    // ============ PUBLIC METHODS ============
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.Log($"[SceneLoader] [{gameObject.name}] already loading a scene, ignoring request for {sceneName}.");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    // ============ PRIVATE METHODS ============
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;
        loadingScreenRoot.SetActive(true);
        progressSlider.value = 0f;

        float elapsedTime = 0f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"[SceneLoader] [{gameObject.name}] failed to start loading {sceneName}. Check the scene name spelling and make sure it is added and checked in File > Build Settings.");
            loadingScreenRoot.SetActive(false);
            isLoading = false;
            yield break;
        }

        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            progressSlider.value = loadProgress;
            progressText.text = $"{Mathf.RoundToInt(loadProgress * 100f)}%";

            bool sceneReady = operation.progress >= 0.89f;
            bool minimumTimePassed = elapsedTime >= minimumLoadDuration;

            if (sceneReady && minimumTimePassed)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        loadingScreenRoot.SetActive(false);
        isLoading = false;
        Debug.Log($"[SceneLoader] [{gameObject.name}] finished loading scene {sceneName}.");
    }
}