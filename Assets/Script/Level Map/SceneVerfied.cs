using System.Collections;
using UnityEngine;

public class SceneVerfied : MonoBehaviour
{
    private const string PREFS_PREFIX = "LEVEL_VERIFIED_";
    private const string NOTIF_PREFIX = "LEVEL_NOTIF_";

    public static SceneVerfied Instance { get; private set; }

    [Header("Verification Settings")]
    [SerializeField] private string levelKey = "Escuri_Completed";
    [Tooltip("Jika true, level akan diverifikasi otomatis saat scene start. Jika false, verifikasi menunggu dipanggil oleh Spawner/Event.")]
    [SerializeField] private bool verifyOnStart = false;

    [Header("Target Interact Scene")]
    [SerializeField] private InteractScene interactScene;
    [SerializeField] private InteractScenePanel interactScenePanel;

    public string LevelKey => levelKey;
    public InteractScene InteractSceneRef => interactScene;
    public InteractScenePanel InteractScenePanelRef => interactScenePanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (verifyOnStart)
        {
            VerifyLevel();
        }
    }

    public void VerifyLevel()
    {
        SetVerified(levelKey, true, true);

        if (interactScene != null)
        {
            interactScene.gameObject.SetActive(true);
            interactScene.enabled = true;
        }

        if (interactScenePanel != null)
        {
            interactScenePanel.gameObject.SetActive(true);
            interactScenePanel.enabled = true;
        }

        Debug.Log($"[SceneVerfied] Level '{levelKey}' berhasil diverifikasi dan disimpan ke PlayerPrefs.");
    }

    public void VerifyAndLoadScene()
    {
        VerifyLevel();

        if (interactScene != null)
        {
            interactScene.LoadedScene();
        }
        else if (interactScenePanel != null)
        {
            interactScenePanel.LoadedScene();
        }
    }

    public static bool IsVerified(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        return PlayerPrefs.GetInt(PREFS_PREFIX + key, 0) == 1;
    }

    public static void SetVerified(string key, bool isVerified = true, bool setPendingNotification = true)
    {
        if (string.IsNullOrEmpty(key)) return;

        PlayerPrefs.SetInt(PREFS_PREFIX + key, isVerified ? 1 : 0);
        if (isVerified && setPendingNotification)
        {
            PlayerPrefs.SetInt(NOTIF_PREFIX + key, 1);
        }
        PlayerPrefs.Save();
    }

    public static bool HasPendingNotification(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        return PlayerPrefs.GetInt(NOTIF_PREFIX + key, 0) == 1;
    }

    public static void ClearNotification(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        PlayerPrefs.SetInt(NOTIF_PREFIX + key, 0);
        PlayerPrefs.Save();
    }

    public static void ResetVerification(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        PlayerPrefs.DeleteKey(PREFS_PREFIX + key);
        PlayerPrefs.DeleteKey(NOTIF_PREFIX + key);
        PlayerPrefs.Save();
    }

    [ContextMenu("Debug - Verifikasi Level Ini")]
    private void DebugVerifyLevel()
    {
        VerifyLevel();
    }

    [ContextMenu("Debug - Reset Progress Level Ini")]
    private void DebugResetProgress()
    {
        ResetVerification(levelKey);
        Debug.Log($"[SceneVerfied] Progress level '{levelKey}' berhasil di-reset.");
    }
}
