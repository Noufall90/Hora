using UnityEngine;
using UnityEngine.UI;
using EasyTransition;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;

    [Header("Scene Settings")]
    [SerializeField] private string gameScene = "GameScene";

    [Header("Transition Settings")]
    [SerializeField] private TransitionSettings transition;
    [SerializeField] private float startDelay = 0.5f;

    [Header("Spawn Settings")]
    [SerializeField] private string targetSpawnID;

    [Header("Confirmation & Sub-Menus")]
    [SerializeField] private GameObject confirmNewGamePanel;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject helpMenu;

    private string SavePath
    {
        get
        {
            return Path.Combine(
                Application.persistentDataPath,
                "hora_save.json"
            );
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (confirmNewGamePanel != null)
            confirmNewGamePanel.SetActive(false);

        RefreshMenuUI();
    }

    public void RefreshMenuUI()
    {
        bool hasSave = HasSave();

        if (continueButton != null)
            continueButton.interactable = hasSave;
    }

    private bool HasSave()
    {
        if (SaveDataJson.Instance != null)
            return SaveDataJson.Instance.HasSaveFile;

        return File.Exists(SavePath);
    }

    public void NewGame()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }

        if (HasSave())
        {
            if (confirmNewGamePanel != null)
            {
                confirmNewGamePanel.SetActive(true);
            }

            return;
        }
        ExecuteNewGame();
    }

    public void ExecuteNewGame()
    {
        if (confirmNewGamePanel != null)
            confirmNewGamePanel.SetActive(false);

        if (SaveDataJson.Instance != null)
        {
            SaveDataJson.Instance.ResetData();
        }
        else
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    File.Delete(SavePath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[MainMenu] Gagal menghapus save: {ex.Message}");
                }
            }
        }

        PointLocation.SetSpawnTarget(
            targetSpawnID,
            SceneManager.GetActiveScene().name
        );

        LoadGameScene();
    }

    public void CancelNewGame()
    {
        Debug.Log("[MainMenu] New Game dibatalkan.");

        if (confirmNewGamePanel != null)
            confirmNewGamePanel.SetActive(false);
    }

    public void Continue()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }

        if (SaveDataJson.Instance == null)
        {
            Debug.LogError("[MainMenu] SaveDataJson.Instance tidak ditemukan!");
            return;
        }

        if (!SaveDataJson.Instance.HasSaveFile)
        {
            Debug.LogWarning("[MainMenu] Tidak ada save file.");
            return;
        }

        SaveDataJson.Instance.LoadGame();

        PointLocation.SetSpawnTarget(
            targetSpawnID,
            SceneManager.GetActiveScene().name
        );

        LoadGameScene();
    }

    private void LoadGameScene()
    {
        Debug.Log($"[MainMenu] Loading scene: {gameScene}");

        if (TransitionManager.Instance() != null &&
            transition != null)
        {
            TransitionManager.Instance().Transition(
                gameScene,
                transition,
                startDelay
            );

            return;
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(gameScene);
            return;
        }

        if (!string.IsNullOrEmpty(gameScene))
        {
            SceneManager.LoadScene(gameScene);
            return;
        }
    }

    public void OpenSettings()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        if (settingsMenu != null)
            settingsMenu.SetActive(false);
    }

    public void OpenHelp()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        if (helpMenu != null)
            helpMenu.SetActive(true);
    }

    public void CloseHelp()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        if (helpMenu != null)
            helpMenu.SetActive(false);
    }

    public void OpenNewGamePanel()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        if (confirmNewGamePanel != null)
            confirmNewGamePanel.SetActive(true);
    }

    public void CloseNewGamePanel()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        if (confirmNewGamePanel != null)
            confirmNewGamePanel.SetActive(false);
    }

    public void ExitGame()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}