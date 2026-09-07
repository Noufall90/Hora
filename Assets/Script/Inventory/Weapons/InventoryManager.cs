using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public static InventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                InventoryManager[] managers = Resources.FindObjectsOfTypeAll<InventoryManager>();
                foreach (var mgr in managers)
                {
                    if (mgr != null && mgr.gameObject != null && mgr.gameObject.scene.isLoaded)
                    {
                        instance = mgr;
                        break;
                    }
                }
            }
            return instance;
        }
    }

    [Header("Inventory UI Content")]
    public Transform ItemContent;
    public GameObject InventoryItem;
    public List<Item> items = new List<Item>();

    [Header("Item Database")]
    [SerializeField] private Item[] itemDatabase;
    public Item[] AllItems => itemDatabase;

    [Header("Desc Item UI")]
    public TMP_Text itemName;
    public TMP_Text itemDescription;
    public TMP_Text itemStats;
    public GameObject equipButton;

    [Header("Selected & Equipped Items")]
    public Item selectedItem;
    public InventoryItemManager selectedSlot;
    public Item equippedMeleeItem;
    public Item equippedPistolItem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        SetupEquipButton();
    }

    private void Start()
    {
        SetupEquipButton();

        // Load item & weapon dari save file jika ada
        if (SaveDataJson.Instance != null && SaveDataJson.Instance.HasSaveFile)
        {
            ApplyFromSave();
        }
        else
        {
            ListItems();
        }
    }

    // ── Save / Load Integration ───────────────────────────

    public void ApplyFromSave()
    {
        if (SaveDataJson.Instance == null || SaveDataJson.Instance.Data == null) return;

        GameSaveData data = SaveDataJson.Instance.Data;
        items.Clear();
        equippedMeleeItem = null;
        equippedPistolItem = null;

        Item[] allItems = itemDatabase;
        if (allItems == null || allItems.Length == 0)
        {
            Debug.LogError("[InventoryManager] Item Database belum di-assign di Inspector!");
            return;
        }

        foreach (string savedName in data.inventoryItemNames)
        {
            Item found = FindSO(allItems, savedName);
            if (found != null)
            {
                items.Add(found);
            }
            else
            {
                Debug.LogWarning($"[InventoryManager] Item '{savedName}' tidak ditemukan di Item Database.");
            }
        }

        if (!string.IsNullOrEmpty(data.equippedMeleeName))
        {
            Item meleeItem = FindSO(allItems, data.equippedMeleeName);
            equippedMeleeItem = meleeItem;
            if (meleeItem != null && PlayerWeapons.WeaponsManager.Instance != null)
            {
                PlayerWeapons.WeaponsManager.Instance.EquipMelee(meleeItem.indexWeapons);
            }
        }

        if (!string.IsNullOrEmpty(data.equippedPistolName))
        {
            Item pistolItem = FindSO(allItems, data.equippedPistolName);
            equippedPistolItem = pistolItem;
            if (pistolItem != null && PlayerWeapons.WeaponsManager.Instance != null)
            {
                PlayerWeapons.WeaponsManager.Instance.EquipPistol(pistolItem.indexWeapons);
            }
        }

        ListItems();
    }

    private static Item FindSO(Item[] array, string assetName)
    {
        if (array == null || string.IsNullOrEmpty(assetName)) return null;
        foreach (Item item in array)
        {
            if (item != null && (item.name == assetName || item.itemName == assetName))
                return item;
        }
        return null;
    }

    // ── UI Setup ──────────────────────────────────────────

    private void SetupEquipButton()
    {
        if (equipButton != null)
        {
            Button btn = equipButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(EquipSelectedItem);
                btn.onClick.AddListener(EquipSelectedItem);
            }
            else
            {
                Debug.LogWarning("[InventoryManager] GameObject 'equipButton' tidak memiliki komponen UI Button!");
            }
        }
    }

    public void SelectItem(Item item, InventoryItemManager slot)
    {
        selectedItem = item;
        selectedSlot = slot;

        if (item != null)
        {
            if (itemName != null) itemName.text = item.itemName;
            if (itemDescription != null) itemDescription.text = item.descriptionItem;
            if (itemStats != null) itemStats.text = item.statsItem;
            Debug.Log($"[InventoryManager] Item dipilih: {item.itemName} ({item.itemType})");
        }

        UpdateEquipButtonText();
    }

    public void EquipSelectedItem()
    {
        if (selectedItem == null)
        {
            Debug.LogWarning("[InventoryManager] Tidak ada item yang dipilih untuk di-equip!");
            return;
        }

        switch (selectedItem.itemType)
        {
            case ItemType.Melee:
                equippedMeleeItem = selectedItem;
                if (PlayerWeapons.WeaponsManager.Instance != null)
                {
                    PlayerWeapons.WeaponsManager.Instance.EquipMelee(selectedItem.indexWeapons);
                }
                else
                {
                    Debug.LogWarning("[InventoryManager] PlayerWeapons.WeaponsManager.Instance tidak ditemukan di Scene!");
                }
                Debug.Log($"[InventoryManager] Berhasil Equip Melee: {selectedItem.itemName} (Index: {selectedItem.indexWeapons})");
                break;

            case ItemType.Pistol:
                equippedPistolItem = selectedItem;
                if (PlayerWeapons.WeaponsManager.Instance != null)
                {
                    PlayerWeapons.WeaponsManager.Instance.EquipPistol(selectedItem.indexWeapons);
                }
                else
                {
                    Debug.LogWarning("[InventoryManager] PlayerWeapons.WeaponsManager.Instance tidak ditemukan di Scene!");
                }
                Debug.Log($"[InventoryManager] Berhasil Equip Pistol: {selectedItem.itemName} (Index: {selectedItem.indexWeapons})");
                break;
        }

        RefreshEquipIcons();
        UpdateEquipButtonText();

        // Auto-save status equipment
        if (SaveDataJson.Instance != null)
        {
            SaveDataJson.Instance.SaveGame();
        }
    }

    public void RefreshEquipIcons()
    {
        if (ItemContent == null) return;

        foreach (Transform child in ItemContent)
        {
            InventoryItemManager slot = child.GetComponent<InventoryItemManager>();
            if (slot != null && slot.item != null)
            {
                bool isEquipped = (slot.item == equippedMeleeItem || slot.item == equippedPistolItem);
                slot.SetEquipped(isEquipped);
            }
        }
    }

    public void UpdateEquipButtonText()
    {
        if (equipButton == null || selectedItem == null) return;

        bool isEquipped = (selectedItem == equippedMeleeItem || selectedItem == equippedPistolItem);
        string buttonLabel = isEquipped ? "DIPAKAI" : "EQUIP";

        TMP_Text tmpText = equipButton.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = buttonLabel;
        }
        else
        {
            Text uiText = equipButton.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                uiText.text = buttonLabel;
            }
        }
    }

    public void Add(Item item)
    {
        if (item != null)
        {
            items.Add(item);
            Debug.Log($"[InventoryManager] Item '{item.itemName}' ditambahkan ke list! Total item: {items.Count}");
            ListItems();

            if (SaveDataJson.Instance != null)
            {
                SaveDataJson.Instance.SaveGame();
            }
        }
    }

    public void OpenInventory(GameObject inventoryPanel)
    {
        UIHandler.OpenWindow(inventoryPanel, false);
    }

    public void CloseInventory(GameObject inventoryPanel)
    {
        if (!inventoryPanel.activeSelf) return;

        UIHandler.CloseWindow(inventoryPanel);
    }

    public void ListItems()
    {
        if (ItemContent == null || InventoryItem == null) return;

        foreach (Transform child in ItemContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in items)
        {
            if (item == null) continue;

            GameObject obj = Instantiate(InventoryItem, ItemContent);

            Transform iconTransform = obj.transform.Find("ItemIcon");
            if (iconTransform != null)
            {
                Image itemIcon = iconTransform.GetComponent<Image>();
                if (itemIcon != null && item.icon != null)
                {
                    itemIcon.sprite = item.icon;
                }
            }

            ItemController itemController = obj.GetComponent<ItemController>();
            if (itemController != null)
            {
                itemController.item = item;
            }

            InventoryItemManager itemSlotManager = obj.GetComponent<InventoryItemManager>();
            if (itemSlotManager != null)
            {
                itemSlotManager.Setup(item);
                bool isEquipped = (item == equippedMeleeItem || item == equippedPistolItem);
                itemSlotManager.SetEquipped(isEquipped);
            }
        }
    }
}