using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPotionManager : MonoBehaviour
{
    public static InventoryPotionManager instance;
    public static InventoryPotionManager Instance
    {
        get
        {
            if (instance == null)
            {
                InventoryPotionManager[] managers = Resources.FindObjectsOfTypeAll<InventoryPotionManager>();
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

    [Header("Potion Content & Slot Prefab")]
    public Transform potionContent;
    public GameObject potionInventoryItem;
    public List<PotionItem> potionItems = new List<PotionItem>();

    [Header("Potion Desc UI")]
    public TMP_Text potionName;
    public TMP_Text potionDescription;
    public TMP_Text potionStats;
    public GameObject useButton;

    [Header("Selected Potion")]
    public PotionItem selectedPotion;
    public InventoryPotionItemManager selectedSlot;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        SetupUseButton();
    }

    private void Start()
    {
        SetupUseButton();
    }

    private void SetupUseButton()
    {
        if (useButton != null)
        {
            Button btn = useButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(UseSelectedPotion);
                btn.onClick.AddListener(UseSelectedPotion);
            }
        }
    }

    public void Add(PotionItem potion)
    {
        if (potion != null)
        {
            potionItems.Add(potion);
            Debug.Log($"[InventoryPotionManager] Potion '{potion.itemName}' ditambahkan ke list! Total potion: {potionItems.Count}");
            ListPotions();
        }
    }

    public void SelectPotion(PotionItem potion, InventoryPotionItemManager slot)
    {
        selectedPotion = potion;
        selectedSlot = slot;

        if (potion != null)
        {
            if (potionName != null) potionName.text = potion.itemName;
            if (potionDescription != null) potionDescription.text = potion.descriptionItem;
            if (potionStats != null) potionStats.text = potion.statsItem;
            Debug.Log($"[InventoryPotionManager] Potion dipilih: {potion.itemName}");
        }
    }

    public void UseSelectedPotion()
    {
        if (selectedPotion == null)
        {
            Debug.LogWarning("[InventoryPotionManager] Tidak ada potion yang dipilih!");
            return;
        }

        PlayerData.PlayerHealth playerHealth = FindFirstObjectByType<PlayerData.PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerData.PlayerHealth>();
        }

        if (playerHealth != null)
        {
            switch (selectedPotion.type)
            {
                case PotionType.Healer:
                    playerHealth.Heal(selectedPotion.effectAmount);
                    Debug.Log($"[InventoryPotionManager] Potion Healer digunakan! HP +{selectedPotion.effectAmount}");
                    break;

                case PotionType.Shield:
                    playerHealth.AddShield((float)selectedPotion.effectAmount);
                    Debug.Log($"[InventoryPotionManager] Potion Shield digunakan! Shield +{selectedPotion.effectAmount}");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("[InventoryPotionManager] PlayerData.PlayerHealth tidak ditemukan di Scene!");
        }

        potionItems.Remove(selectedPotion);

        if (!potionItems.Contains(selectedPotion))
        {
            selectedPotion = null;
            selectedSlot = null;

            if (potionName != null) potionName.text = "";
            if (potionDescription != null) potionDescription.text = "";
            if (potionStats != null) potionStats.text = "";
        }

        ListPotions();
    }

    public void ListPotions()
    {
        if (potionContent == null || potionInventoryItem == null) return;

        foreach (Transform child in potionContent)
        {
            Destroy(child.gameObject);
        }

        Dictionary<PotionItem, int> potionCounts = new Dictionary<PotionItem, int>();
        List<PotionItem> uniquePotions = new List<PotionItem>();

        foreach (var potion in potionItems)
        {
            if (potion == null) continue;
            if (!potionCounts.ContainsKey(potion))
            {
                potionCounts[potion] = 0;
                uniquePotions.Add(potion);
            }
            potionCounts[potion]++;
        }

        foreach (var potion in uniquePotions)
        {
            GameObject obj = Instantiate(potionInventoryItem, potionContent);

            PotionItemController controller = obj.GetComponent<PotionItemController>();
            if (controller != null)
            {
                controller.potionItem = potion;
            }

            InventoryPotionItemManager slotManager = obj.GetComponent<InventoryPotionItemManager>();
            if (slotManager != null)
            {
                slotManager.Setup(potion, potionCounts[potion]);
            }
        }
    }
}
