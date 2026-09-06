using System.Collections;
using UnityEngine;

public class SceneLocked : MonoBehaviour
{
    [Header("Level Verification Link")]
    [SerializeField] private string verificationKey = "Escuri_Completed";

    [Header("Level / Portal Object")]
    [SerializeField] private GameObject level;
    [SerializeField] private InteractScenePanel interactScenePanel;

    [Header("Panels")]
    [SerializeField] private GameObject lockedPanel;
    [SerializeField] private GameObject passPanel;

    private const float NOTIF_START_DELAY = 2f;
    private const float PASS_PANEL_DURATION = 5f;

    private Coroutine notifCoroutine;

    /// <summary>Akses ke verificationKey dari luar (dipakai SaveDataJson).</summary>
    public string VerificationKey => verificationKey;

    public InteractScenePanel InteractScenePanelRef => interactScenePanel;

    private void Start()
    {
        InitializePanels();
        SetupTriggerRelay();
        CheckAndApplyLevelState();
    }

    private void InitializePanels()
    {
        if (lockedPanel != null) lockedPanel.SetActive(false);
        if (passPanel != null) passPanel.SetActive(false);
    }

    private void SetupTriggerRelay()
    {
        GameObject targetObj = interactScenePanel != null ? interactScenePanel.gameObject : level;
        if (targetObj != null)
        {
            SceneLockedRelay relay = targetObj.GetComponent<SceneLockedRelay>();
            if (relay == null)
            {
                relay = targetObj.AddComponent<SceneLockedRelay>();
            }
            relay.Init(this);
        }
    }

    public void CheckAndApplyLevelState()
    {
        bool isUnlocked = false;

        if (SaveDataJson.Instance != null && SaveDataJson.Instance.HasSaveFile)
        {
            isUnlocked = SaveDataJson.Instance.IsLevelUnlocked(verificationKey);
        }
        else
        {
            isUnlocked = SceneVerfied.IsVerified(verificationKey);
        }

        if (level != null)
        {
            level.SetActive(true);
        }

        if (interactScenePanel != null)
        {
            interactScenePanel.enabled = isUnlocked;
        }

        if (isUnlocked && SceneVerfied.HasPendingNotification(verificationKey))
        {
            SceneVerfied.ClearNotification(verificationKey);
            if (notifCoroutine != null) StopCoroutine(notifCoroutine);
            notifCoroutine = StartCoroutine(PassNotificationSequence());
        }
    }

    public void UnlockLevel(bool autoSave = true)
    {
        SceneVerfied.SetVerified(verificationKey, true, true);
        CheckAndApplyLevelState();

        if (autoSave && SaveDataJson.Instance != null)
        {
            SaveDataJson.Instance.SaveGame();
        }

        Debug.Log($"[SceneLocked] Level '{verificationKey}' berhasil dibuka dan disimpan.");
    }

    private IEnumerator PassNotificationSequence()
    {
        yield return new WaitForSecondsRealtime(NOTIF_START_DELAY);
        ShowPassPanel();
        yield return new WaitForSecondsRealtime(PASS_PANEL_DURATION);
        ClosePassPanel();
    }

    public void OnPortalTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!SceneVerfied.IsVerified(verificationKey))
            {
                ShowLockedPanel();
            }
        }
    }

    public void OnPortalTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CloseLockedPanel();
        }
    }

    private void OnTriggerEnter(Collider other) => OnPortalTriggerEnter(other);
    private void OnTriggerExit(Collider other) => OnPortalTriggerExit(other);

    public bool TryAccessLevel()
    {
        if (SceneVerfied.IsVerified(verificationKey))
        {
            return true;
        }

        ShowLockedPanel();
        return false;
    }

    public void ShowLockedPanel()
    {
        if (lockedPanel != null) lockedPanel.SetActive(true);
    }

    public void CloseLockedPanel()
    {
        if (lockedPanel != null) lockedPanel.SetActive(false);
    }

    public void ShowPassPanel()
    {
        if (passPanel != null) passPanel.SetActive(true);
    }

    public void ClosePassPanel()
    {
        if (passPanel != null) passPanel.SetActive(false);
    }

    [ContextMenu("Debug - Simulasikan Map Terbuka (Unlocked)")]
    private void DebugSimulateUnlocked()
    {
        UnlockLevel(true);
    }

    [ContextMenu("Debug - Reset Status Terkunci")]
    private void DebugResetToLocked()
    {
        SceneVerfied.ResetVerification(verificationKey);
        CheckAndApplyLevelState();
        if (SaveDataJson.Instance != null) SaveDataJson.Instance.SaveGame();
        Debug.Log($"[SceneLocked] Level '{verificationKey}' di-reset ke Locked.");
    }
}

/// <summary>
/// Komponen helper untuk meneruskan event trigger collider dari portal ke SceneLocked Manager.
/// </summary>
public class SceneLockedRelay : MonoBehaviour
{
    private SceneLocked sceneLocked;

    public void Init(SceneLocked manager) => sceneLocked = manager;

    private void OnTriggerEnter(Collider other)
    {
        if (sceneLocked != null) sceneLocked.OnPortalTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (sceneLocked != null) sceneLocked.OnPortalTriggerExit(other);
    }
}
