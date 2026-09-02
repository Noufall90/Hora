using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scene")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("Transition Settings")]
    [SerializeField] private TransitionSettings transition;
    [SerializeField] private float startDelay = 0.5f;

    private bool isPaused;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (isPaused)
            Resume();
        else
            OpenPause();
    }

    public void OpenPause()
    {
        if (pausePanel == null) return;

        isPaused = true;
        Pause.On();
        UIHandler.OpenWindow(pausePanel);
    }

    public void Resume()
    {
        if (pausePanel == null) return;

        isPaused = false;
        Pause.Off();
        UIHandler.CloseWindow(pausePanel);
    }

    public void QuitToMainMenu()
    {
        isPaused = false;
        Pause.ForceResume();
        UIHandler.CloseWindow(pausePanel);

        if (TransitionManager.Instance() != null && transition != null)
        {
            TransitionManager.Instance().Transition(mainMenuScene, transition, startDelay);
        }
        else if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(mainMenuScene);
        }
        else if (!string.IsNullOrEmpty(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}