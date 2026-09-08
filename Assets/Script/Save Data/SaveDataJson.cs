using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PotionSaveEntry
{
    public string potionName;
    public int count;
}

[Serializable]
public class LevelSaveEntry
{
    public string key;
    public bool isUnlocked;
}

[Serializable]
public class DialogueSaveEntry
{
    public string npcId;
    public bool isCompleted;
}

[Serializable]
public class GameSaveData
{
    public int totalCoin;
    public bool tutorialCompleted;

    public List<string> inventoryItemNames = new List<string>();
    public string equippedMeleeName;
    public string equippedPistolName;

    public List<PotionSaveEntry> potionInventory = new List<PotionSaveEntry>();

    public List<string> purchasedMeleeNames = new List<string>();
    public List<string> purchasedPistolNames = new List<string>();
    public List<string> purchasedPotionNames = new List<string>();

    public List<LevelSaveEntry> levelStates = new List<LevelSaveEntry>();
    public List<DialogueSaveEntry> dialogueStates = new List<DialogueSaveEntry>();

    public string lastSavedScene;
    public string saveTimestamp;

    public int playerHealth;
    public int playerMaxHealth;
}

public static class DialogueSaveRegistry
{
    public static Dictionary<string, bool> CompletedDialogues = new Dictionary<string, bool>();

    public static void MarkCompleted(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return;
        CompletedDialogues[npcId] = true;
    }

    public static bool IsCompleted(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return false;
        return CompletedDialogues.TryGetValue(npcId, out bool val) && val;
    }

    public static void Clear()
    {
        CompletedDialogues.Clear();
    }
}

public class SaveDataJson : MonoBehaviour
{
    public static SaveDataJson Instance { get; private set; }

    private static string SavePath => Path.Combine(Application.persistentDataPath, "hora_save.json");

    public GameSaveData Data { get; private set; } = new GameSaveData();

    public bool HasSaveFile => File.Exists(SavePath);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (HasSaveFile)
            {
                LoadGame();
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        if (Data == null)
        {
            Data = new GameSaveData();
        }

        // HANYA update data jika manager terkait aktif di scene saat ini
        CollectCoinData();
        CollectInventoryData();
        CollectPotionInventoryData();
        CollectBuyMeeleData();
        CollectBuyPistolData();
        CollectBuyPotionData();
        CollectLevelData();
        CollectDialogueData();
        CollectHealthData();

        Data.lastSavedScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Data.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        try
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveDataJson] Gagal menyimpan game: {ex.Message}");
        }
    }

    private void CollectCoinData()
    {
        // Hanya update jika CoinCounter sedang aktif di scene
        if (CoinCounter.Instance != null)
        {
            Data.totalCoin = CoinCounter.Instance.Coin;
        }
    }

    private void CollectInventoryData()
    {
        if (InventoryManager.Instance == null) return;
        InventoryManager.Instance.EnsureInitialized();

        Data.inventoryItemNames.Clear();
        foreach (Item item in InventoryManager.Instance.items)
        {
            if (item != null && !string.IsNullOrEmpty(item.name))
            {
                Data.inventoryItemNames.Add(item.name);
            }
        }

        Data.equippedMeleeName = InventoryManager.Instance.equippedMeleeItem != null
            ? InventoryManager.Instance.equippedMeleeItem.name
            : string.Empty;

        Data.equippedPistolName = InventoryManager.Instance.equippedPistolItem != null
            ? InventoryManager.Instance.equippedPistolItem.name
            : string.Empty;
    }

    private void CollectPotionInventoryData()
    {
        if (InventoryPotionManager.Instance == null) return;
        InventoryPotionManager.Instance.EnsureInitialized();

        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (PotionItem p in InventoryPotionManager.Instance.potionItems)
        {
            if (p == null || string.IsNullOrEmpty(p.name)) continue;
            if (!counts.ContainsKey(p.name)) counts[p.name] = 0;
            counts[p.name]++;
        }

        Data.potionInventory.Clear();
        foreach (var kv in counts)
        {
            Data.potionInventory.Add(new PotionSaveEntry { potionName = kv.Key, count = kv.Value });
        }
    }

    private void CollectBuyMeeleData()
    {
        if (InventoryManager.Instance == null) return;
        InventoryManager.Instance.EnsureInitialized();

        foreach (Item item in InventoryManager.Instance.items)
        {
            if (item != null && item.itemType == ItemType.Melee && !string.IsNullOrEmpty(item.name))
            {
                if (!Data.purchasedMeleeNames.Contains(item.name))
                {
                    Data.purchasedMeleeNames.Add(item.name);
                }
            }
        }
    }

    private void CollectBuyPistolData()
    {
        if (InventoryManager.Instance == null) return;
        InventoryManager.Instance.EnsureInitialized();

        foreach (Item item in InventoryManager.Instance.items)
        {
            if (item != null && item.itemType == ItemType.Pistol && !string.IsNullOrEmpty(item.name))
            {
                if (!Data.purchasedPistolNames.Contains(item.name))
                {
                    Data.purchasedPistolNames.Add(item.name);
                }
            }
        }
    }

    private void CollectBuyPotionData()
    {
        foreach (PotionSaveEntry entry in Data.potionInventory)
        {
            if (!string.IsNullOrEmpty(entry.potionName) && !Data.purchasedPotionNames.Contains(entry.potionName))
            {
                Data.purchasedPotionNames.Add(entry.potionName);
            }
        }
    }

    private void CollectLevelData()
    {
        SceneLocked[] locks = FindObjectsByType<SceneLocked>(FindObjectsSortMode.None);
        if (locks == null || locks.Length == 0) return;

        foreach (SceneLocked sl in locks)
        {
            if (sl == null) continue;
            string key = sl.VerificationKey;
            if (string.IsNullOrEmpty(key)) continue;

            bool isUnlocked = SceneVerfied.IsVerified(key);
            LevelSaveEntry existing = Data.levelStates.Find(x => x.key == key);
            if (existing != null)
            {
                existing.isUnlocked = isUnlocked;
            }
            else
            {
                Data.levelStates.Add(new LevelSaveEntry
                {
                    key = key,
                    isUnlocked = isUnlocked
                });
            }
        }
    }

    private void CollectDialogueData()
    {
        Data.dialogueStates.Clear();
        foreach (var kv in DialogueSaveRegistry.CompletedDialogues)
        {
            Data.dialogueStates.Add(new DialogueSaveEntry { npcId = kv.Key, isCompleted = kv.Value });
        }
    }

    private void CollectHealthData()
    {
        if (PlayerData.PlayerHealth.Instance != null)
        {
            Data.playerHealth = PlayerData.PlayerHealth.Instance.CurrentHealth;
            Data.playerMaxHealth = PlayerData.PlayerHealth.Instance.MaxHealth;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LOAD SYSTEM
    // ─────────────────────────────────────────────────────────────────────────

    public void LoadGame()
    {
        if (!HasSaveFile)
        {
            Data = new GameSaveData();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            Data = JsonUtility.FromJson<GameSaveData>(json);

            if (Data == null)
            {
                Data = new GameSaveData();
                return;
            }

            // Restore dialogue registry
            DialogueSaveRegistry.Clear();
            if (Data.dialogueStates != null)
            {
                foreach (DialogueSaveEntry entry in Data.dialogueStates)
                {
                    DialogueSaveRegistry.CompletedDialogues[entry.npcId] = entry.isCompleted;
                }
            }

            // Restore level states ke PlayerPrefs
            if (Data.levelStates != null)
            {
                foreach (LevelSaveEntry entry in Data.levelStates)
                {
                    SceneVerfied.SetVerified(entry.key, entry.isUnlocked, false);
                }
            }

            Debug.Log($"[SaveDataJson] Data berhasil dimuat dari: {SavePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveDataJson] Error saat memuat data: {ex.Message}");
            Data = new GameSaveData();
        }
    }

    public void ApplyLoadedDataToScene()
    {
        ApplyCoinData();
        ApplyInventoryData();
        ApplyPotionInventoryData();
        ApplyLevelData();
        ApplyHealthData();
    }

    private void ApplyCoinData()
    {
        if (CoinCounter.Instance != null && Data != null)
        {
            CoinCounter.Instance.SetCoin(Data.totalCoin);
        }
    }

    private void ApplyInventoryData()
    {
        if (InventoryManager.Instance == null || Data == null) return;

        InventoryManager.Instance.items.Clear();
        InventoryManager.Instance.equippedMeleeItem = null;
        InventoryManager.Instance.equippedPistolItem = null;

        Item[] allItems = InventoryManager.Instance.AllItems;
        if (allItems == null || allItems.Length == 0)
        {
            Debug.LogError("[SaveDataJson] Item Database belum di-assign di Inspector!");
            return;
        }

        foreach (string savedName in Data.inventoryItemNames)
        {
            Item found = FindSO(allItems, savedName);
            if (found != null)
            {
                InventoryManager.Instance.items.Add(found);
            }
            else
            {
                Debug.LogWarning($"[SaveDataJson] Item '{savedName}' tidak ditemukan di Item Database.");
            }
        }

        if (!string.IsNullOrEmpty(Data.equippedMeleeName))
        {
            Item meleeItem = FindSO(allItems, Data.equippedMeleeName);
            InventoryManager.Instance.equippedMeleeItem = meleeItem;
            if (meleeItem != null && PlayerWeapons.WeaponsManager.Instance != null)
            {
                PlayerWeapons.WeaponsManager.Instance.EquipMelee(meleeItem.indexWeapons);
            }
        }

        if (!string.IsNullOrEmpty(Data.equippedPistolName))
        {
            Item pistolItem = FindSO(allItems, Data.equippedPistolName);
            InventoryManager.Instance.equippedPistolItem = pistolItem;
            if (pistolItem != null && PlayerWeapons.WeaponsManager.Instance != null)
            {
                PlayerWeapons.WeaponsManager.Instance.EquipPistol(pistolItem.indexWeapons);
            }
        }

        InventoryManager.Instance.ListItems();
    }

    private void ApplyPotionInventoryData()
    {
        if (InventoryPotionManager.Instance == null || Data == null) return;

        InventoryPotionManager.Instance.potionItems.Clear();

        PotionItem[] allPotions = InventoryPotionManager.Instance.AllPotions;
        if (allPotions == null || allPotions.Length == 0)
        {
            Debug.LogError("[SaveDataJson] Potion Database belum di-assign di Inspector!");
            return;
        }

        foreach (PotionSaveEntry entry in Data.potionInventory)
        {
            PotionItem found = FindSO(allPotions, entry.potionName);
            if (found != null)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    InventoryPotionManager.Instance.potionItems.Add(found);
                }
            }
            else
            {
                Debug.LogWarning($"[SaveDataJson] PotionItem '{entry.potionName}' tidak ditemukan di Potion Database.");
            }
        }

        InventoryPotionManager.Instance.ListPotions();
    }

    private void ApplyLevelData()
    {
        SceneLocked[] locks = FindObjectsByType<SceneLocked>(FindObjectsSortMode.None);
        if (locks == null) return;

        foreach (SceneLocked sl in locks)
        {
            if (sl != null)
            {
                sl.CheckAndApplyLevelState();
            }
        }
    }

    private void ApplyHealthData()
    {
        if (PlayerData.PlayerHealth.Instance == null || Data == null) return;

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Home" || currentScene == "PortalMap")
        {
            PlayerData.PlayerHealth.Instance.ResetToFull();
            return;
        }

        if (Data.playerHealth > 0 && Data.playerMaxHealth > 0)
        {
            PlayerData.PlayerHealth.Instance.SetHealth(Data.playerHealth, Data.playerMaxHealth);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CHECKERS / QUERIES
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsMeleePurchased(string meleeName)
    {
        return Data != null && Data.purchasedMeleeNames != null && Data.purchasedMeleeNames.Contains(meleeName);
    }

    public bool IsPistolPurchased(string pistolName)
    {
        return Data != null && Data.purchasedPistolNames != null && Data.purchasedPistolNames.Contains(pistolName);
    }

    public bool IsPotionPurchased(string potionName)
    {
        return Data != null && Data.purchasedPotionNames != null && Data.purchasedPotionNames.Contains(potionName);
    }

    public bool IsLevelUnlocked(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        if (Data != null && Data.levelStates != null)
        {
            LevelSaveEntry found = Data.levelStates.Find(x => x.key == key);
            if (found != null) return found.isUnlocked;
        }
        return SceneVerfied.IsVerified(key);
    }

    public bool IsDialogueCompleted(string npcId)
    {
        return DialogueSaveRegistry.IsCompleted(npcId);
    }

    public void MarkDialogueCompleted(string npcId)
    {
        DialogueSaveRegistry.MarkCompleted(npcId);
        CollectDialogueData();
        SaveGame();
    }

    public bool IsTutorialCompleted()
    {
        return Data != null && Data.tutorialCompleted;
    }

    public void MarkTutorialCompleted()
    {
        if (Data == null) Data = new GameSaveData();
        Data.tutorialCompleted = true;
        SaveGame();
    }

    public void ResetData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        Data = new GameSaveData();
        DialogueSaveRegistry.Clear();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private static T FindSO<T>(T[] array, string assetName) where T : ScriptableObject
    {
        if (array == null || string.IsNullOrEmpty(assetName)) return null;
        foreach (T obj in array)
        {
            if (obj == null) continue;
            if (obj.name == assetName) return obj;
            if (obj is Item item && item.itemName == assetName) return obj;
            if (obj is PotionItem potion && potion.itemName == assetName) return obj;
        }
        return null;
    }

    [ContextMenu("Debug - Save Game")]
    private void DebugSave() => SaveGame();

    [ContextMenu("Debug - Load & Apply")]
    private void DebugLoad()
    {
        LoadGame();
        ApplyLoadedDataToScene();
    }

    [ContextMenu("Debug - Reset Data")]
    private void DebugReset() => ResetData();

    [ContextMenu("Debug - Print Save Path")]
    private void DebugPath() => Debug.Log($"[SaveDataJson] Path: {SavePath}");
}