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

    public Transform ItemContent;
    public GameObject InventoryItem;
    public List<Item> items = new List<Item>();

    [Header("Desc Item")]
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
    }

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
        else
        {
            Debug.LogWarning("[InventoryManager] Field 'equipButton' belum di-assign di Unity Inspector!");
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
        Debug.Log("[InventoryManager] Tombol Equip Ditekan!");
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
                Debug.Log($"[InventoryManager] Berhasil Equip Melee: {selectedItem.itemName} (Index Weapons: {selectedItem.indexWeapons})");
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
                Debug.Log($"[InventoryManager] Berhasil Equip Pistol: {selectedItem.itemName} (Index Weapons: {selectedItem.indexWeapons})");
                break;
        }

        RefreshEquipIcons();
        UpdateEquipButtonText();
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
        }
    }

    public void ListItems()
    {
        if (ItemContent == null)
        {
            Debug.LogWarning("[InventoryManager] ItemContent Transform belum di-assign di Unity Inspector!");
            return;
        }

        if (InventoryItem == null)
        {
            Debug.LogWarning("[InventoryManager] InventoryItem Prefab belum di-assign di Unity Inspector!");
            return;
        }

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