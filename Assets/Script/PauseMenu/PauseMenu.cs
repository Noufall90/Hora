using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scene")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool isPaused;

    private void Start()
    {
        pausePanel.SetActive(false);
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
        isPaused = true;

        Time.timeScale = 0f;
        Pause.On();
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;

        Time.timeScale = 1f;
        Pause.Off();
        pausePanel.SetActive(false);
    }

     public void QuitToMainMenu()
    {
        isPaused = false;

        Time.timeScale = 1f;
        Pause.ForceResume();
        pausePanel.SetActive(false);

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(mainMenuScene);
        else
            SceneManager.LoadScene(mainMenuScene);
    }
}