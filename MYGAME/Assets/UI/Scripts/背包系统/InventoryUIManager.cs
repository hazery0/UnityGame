using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;
    
    [Header("背包UI")]
    public GameObject inventoryUI;
    public bool IsUIOpen { get; private set; }
    
    [Header("物品显示")]
    public Transform itemsContainer;
    public GameObject itemSlotPrefab;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI capacityText;
    
    [Header("分类标签")]
    public Button allItemsButton;
    public Button weaponsButton;
    public Button armorButton;
    public Button consumablesButton;
    public Button materialsButton;
    public Button specialButton;
    
    [Header("功能按钮")]
    public Button restButton; // 修整按钮
    
    [Header("物品信息面板")]
    public GameObject itemInfoPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemStatsText;
    public Image itemIcon;
    
    private ItemSystem itemSystem;
    private PlayerStats playerStats;
    private ItemType currentFilter = ItemType.All;
    private Item selectedItem;
    
    // 事件 - 用于外部处理修整功能
    public System.Action OnRestButtonClicked;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        itemSystem = ItemSystem.Instance;
        playerStats = FindObjectOfType<PlayerStats>();
        
        // 初始隐藏UI
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (itemInfoPanel != null) itemInfoPanel.SetActive(false);
        
        // 绑定按钮事件
        BindUIEvents();
    }
    
    void Update()
    {
        // 按B键打开/关闭背包
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleInventory();
        }
        
        // 如果背包打开，按ESC关闭
        if (IsUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }
    }
    
    void BindUIEvents()
    {
        Debug.Log("🔗 开始绑定按钮事件...");
        
        // 分类按钮
        if (allItemsButton != null) allItemsButton.onClick.AddListener(() => FilterItems(ItemType.All));
        if (weaponsButton != null) weaponsButton.onClick.AddListener(() => FilterItems(ItemType.Weapon));
        if (armorButton != null) armorButton.onClick.AddListener(() => FilterItems(ItemType.Armor));
        if (consumablesButton != null) consumablesButton.onClick.AddListener(() => FilterItems(ItemType.Consumable));
        if (materialsButton != null) materialsButton.onClick.AddListener(() => FilterItems(ItemType.Material));
        if (specialButton != null) specialButton.onClick.AddListener(() => FilterItems(ItemType.Special));
        
        // 修整按钮 - 简化处理
        if (restButton != null) restButton.onClick.AddListener(OnRestButtonClick);
        
        Debug.Log("✅ 所有按钮事件绑定完成");
    }
    
    // ========== 修整按钮点击事件 ==========
    void OnRestButtonClick()
    {
        Debug.Log("🎯 背包UI中的修整按钮被点击");
        
        // 触发事件，由外部系统处理修整功能
        OnRestButtonClicked?.Invoke();
        
        // 关闭背包界面
        CloseInventory();
    }
    
    // ========== 主要UI控制方法 ==========
    public void ToggleInventory()
    {
        IsUIOpen = !IsUIOpen;
        
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(IsUIOpen);
            
            if (IsUIOpen)
            {
                RefreshInventory();
                Time.timeScale = 0.1f; // 轻微减速
                ShowMouseCursor();
                Debug.Log("✅ 打开背包界面");
            }
            else
            {
                CloseAllPanels();
                Time.timeScale = 1f; // 恢复游戏
                HideMouseCursor();
                Debug.Log("✅ 关闭背包界面");
            }
        }
    }
    
    void ShowMouseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void HideMouseCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void CloseInventory()
    {
        IsUIOpen = false;
        if (inventoryUI != null) inventoryUI.SetActive(false);
        CloseAllPanels();
        Time.timeScale = 1f;
        HideMouseCursor();
    }
    
    // ========== 物品管理方法 ==========
    void RefreshInventory()
    {
        if (itemSystem == null || itemsContainer == null) return;
        
        // 清空现有物品显示
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 获取过滤后的物品列表
        List<Item> filteredItems = GetFilteredItems();
        
        // 显示物品
        foreach (Item item in filteredItems)
        {
            CreateItemSlot(item);
        }
        
        // 更新负重信息
        UpdateWeightInfo();
    }
    
    List<Item> GetFilteredItems()
    {
        if (itemSystem == null) return new List<Item>();
        
        if (currentFilter == ItemType.All)
            return itemSystem.playerInventory;
        
        return itemSystem.playerInventory.Where(item => item.itemType == currentFilter).ToList();
    }
    
    void CreateItemSlot(Item item)
    {
        if (itemSlotPrefab == null) return;
        
        GameObject slotObj = Instantiate(itemSlotPrefab, itemsContainer);
        InventoryItemSlot slot = slotObj.GetComponent<InventoryItemSlot>();
        
        if (slot != null)
        {
            slot.Initialize(item, this);
        }
    }
    
    void FilterItems(ItemType filterType)
    {
        currentFilter = filterType;
        RefreshInventory();
        
        // 更新按钮状态
        UpdateFilterButtons();
        Debug.Log($"切换到分类: {currentFilter}");
    }
    
    void UpdateFilterButtons()
    {
        // 这里可以添加按钮高亮逻辑
    }
    
    void UpdateWeightInfo()
    {
        if (itemSystem == null) return;
        
        // 使用ItemSystem的当前重量
        if (weightText != null)
            weightText.text = $"负重: {itemSystem.currentWeight:F1}/{playerStats.maxWeight}kg";
        
        if (capacityText != null)
            capacityText.text = $"容量: {itemSystem.playerInventory.Count}/{itemSystem.maxInventorySlots}";
    }
    
    // ========== 物品信息显示 ==========
    public void ShowItemOptions(Item item)
    {
        selectedItem = item;
        ShowItemInfo(item);
        
        // 根据物品类型显示不同选项
        HandleItemRightClick(item);
    }
    
    void ShowItemInfo(Item item)
    {
        if (itemInfoPanel == null) return;
        
        itemInfoPanel.SetActive(true);
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.itemDescription;
        if (itemIcon != null && item.icon != null)
            itemIcon.sprite = item.icon;
        
        // 显示物品属性
        if (itemStatsText != null)
            itemStatsText.text = GetItemStatsText(item);
    }
    
    string GetItemStatsText(Item item)
    {
        string stats = "";
        
        // 基础属性
        if (item.strengthBonus != 0) stats += $"力量 +{item.strengthBonus}\n";
        if (item.accuracyBonus != 0) stats += $"精准 +{item.accuracyBonus}\n";
        if (item.agilityBonus != 0) stats += $"敏捷 +{item.agilityBonus}\n";
        if (item.defenseBonus != 0) stats += $"防御 +{item.defenseBonus}\n";
        if (item.healthBonus != 0) stats += $"生命 +{item.healthBonus}\n";
        
        // 医疗效果
        if (item.healthRestore > 0) stats += $"恢复生命: +{item.healthRestore}\n";
        if (item.radiationReduction > 0) stats += $"降低辐射: -{item.radiationReduction}\n";
        if (item.curesEffect != StatusEffectType.None) stats += $"治愈: {GetEffectName(item.curesEffect)}\n";
        
        // 食物效果
        if (item.hungerRestore > 0) stats += $"饱食度: +{item.hungerRestore}\n";
        if (item.energyRestore != 0) stats += $"精力: {(item.energyRestore > 0 ? "+" : "")}{item.energyRestore}\n";
        
        // 负面效果
        if (item.negativeEffectChance > 0)
            stats += $"负面效果几率: {item.negativeEffectChance * 100}% ({GetEffectName(item.negativeEffect)})\n";
        
        stats += $"重量: {item.weight}kg";
        if (item.isStackable) stats += $"\n数量: {item.stackCount}/{item.maxStackSize}";
        
        return stats;
    }
    
    string GetEffectName(StatusEffectType effect)
    {
        return effect switch
        {
            StatusEffectType.Infection => "感染",
            StatusEffectType.Diarrhea => "腹泻",
            StatusEffectType.Bleeding => "流血",
            StatusEffectType.DeepWound => "深度裂伤",
            StatusEffectType.Fracture => "骨折",
            StatusEffectType.RadiationSickness => "辐射病",
            StatusEffectType.RadiationMutation => "辐射异变",
            StatusEffectType.RadiationDiscomfort => "辐射不适",
            _ => effect.ToString()
        };
    }
    
    void HandleItemRightClick(Item item)
    {
        // 根据物品类型处理右键点击
        switch (item.itemType)
        {
            case ItemType.Weapon:
            case ItemType.Armor:
            case ItemType.Accessory:
                // 装备物品
                itemSystem.EquipItem(item);
                RefreshInventory();
                Debug.Log($"装备了: {item.itemName}");
                break;
                
            case ItemType.Consumable:
                // 使用消耗品
                itemSystem.UseConsumable(item, playerStats);
                RefreshInventory();
                Debug.Log($"使用了: {item.itemName}");
                break;
                
            case ItemType.Material:
                // 材料只显示信息
                ShowItemInfo(item);
                break;
                
            default:
                // 其他物品只显示信息
                ShowItemInfo(item);
                break;
        }
    }
    
    public void DiscardItem(Item item)
    {
        if (itemSystem != null)
        {
            itemSystem.RemoveItemFromInventory(item);
            RefreshInventory();
            Debug.Log($"丢弃物品: {item.itemName}");
        }
    }
    
    void CloseAllPanels()
    {
        if (itemInfoPanel != null) itemInfoPanel.SetActive(false);
    }
    
    // ========== 公共方法 ==========
    public void ShowMessage(string message)
    {
        Debug.Log(message);
        // 这里可以添加UI消息显示
    }
    
    // ========== 工具方法 ==========
    void OnDestroy()
    {
        // 取消事件注册
        if (playerStats != null)
        {
            playerStats.OnStatsUpdated -= UpdateWeightInfo;
        }
    }
}