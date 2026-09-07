using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using EasyTransition;

public class CutScene : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public GameObject nextSceneButton;
    [SerializeField] private string targetSpawnID;

    public string sceneName = "Home";
    public TransitionSettings transition;
    public float startDelay = 0f;

    public bool autoLoadNextScene = false;

    [Header("Skip")]
    [Tooltip("Tombol untuk melewati cutscene.")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    [Tooltip("Kalau dicentang, skip baru berfungsi setelah timeline selesai.")]
    [SerializeField] private bool skipOnlyAfterEnded = false;

    private bool isLoading;
    private bool hasEnded;

    private void Awake()
    {
        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }
    }

    private void OnEnable()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped += OnCutsceneEnded;
        }

        if (nextSceneButton != null)
        {
            nextSceneButton.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnCutsceneEnded;
        }
    }

    private void Update()
    {
        if (isLoading) return;
        if (skipOnlyAfterEnded && !hasEnded) return;

        if (Input.GetKeyDown(skipKey))
        {
            LoadScene();
        }
    }

    private void OnCutsceneEnded(PlayableDirector director)
    {
        hasEnded = true;

        if (autoLoadNextScene)
        {
            LoadScene();
        }
        else if (nextSceneButton != null)
        {
            nextSceneButton.SetActive(true);
        }
    }

    public void LoadScene()
    {
        if (isLoading) return;
        isLoading = true;

        PointLocation.SetSpawnTarget(
            targetSpawnID,
            SceneManager.GetActiveScene().name
        );

        if (transition != null && TransitionManager.Instance() != null)
        {
            TransitionManager.Instance().Transition(
                sceneName,
                transition,
                startDelay
            );

            return;
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(sceneName);
            return;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Debug.LogError("[CutScene] Scene Home belum diisi!");
    }
}