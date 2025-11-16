using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;
    
    [Header("UI引用")]
    public GameObject characterUI;
    public bool IsUIOpen { get; private set; }
    
    [Header("属性文本")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI radiationText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI agilityText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI movesText;
    
    [Header("状态效果显示")]
    public Transform statusEffectsContainer;
    public GameObject statusEffectPrefab;
    
    [Header("装备槽")]
    public EquipmentSlot weaponSlot;
    public EquipmentSlot armorSlot;
    public EquipmentSlot accessorySlot;
    
    [Header("技能显示")]
    public TextMeshProUGUI skillText;
    
    private PlayerStats playerStats;
    private ItemSystem itemSystem;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        itemSystem = ItemSystem.Instance;
        
        if (characterUI != null)
            characterUI.SetActive(false);
            
        // 注册UI更新事件
        RegisterEvents();
    }
    
    void Update()
    {
        // 如果UI打开，实时更新显示
        if (IsUIOpen)
        {
            UpdateStatsUI();
        }
    }
    
    void RegisterEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsUpdated += UpdateStatsUI;
            playerStats.OnStatusEffectsChanged += UpdateStatusEffectsUI;
        }
        
        if (itemSystem != null)
        {
            itemSystem.OnEquipmentChanged += UpdateEquipmentUI;
        }
    }
    
    void UnregisterEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsUpdated -= UpdateStatsUI;
            playerStats.OnStatusEffectsChanged -= UpdateStatusEffectsUI;
        }
        
        if (itemSystem != null)
        {
            itemSystem.OnEquipmentChanged -= UpdateEquipmentUI;
        }
    }
    
    public void ToggleCharacterUI()
    {
        IsUIOpen = !IsUIOpen;
        
        if (characterUI != null)
            characterUI.SetActive(IsUIOpen);
        
        if (IsUIOpen)
        {
            // UI打开时更新所有显示
            UpdateAllUI();
        }
    }
    
    void UpdateAllUI()
    {
        UpdateStatsUI();
        UpdateEquipmentUI();
        UpdateStatusEffectsUI();
    }
    
    // ========== UI更新方法 ==========
    
    void UpdateStatsUI()
    {
        if (playerStats == null) return;
        
        // 更新基础生存属性
        if (healthText != null)
            healthText.text = $"血量：{playerStats.CurrentHealth}/{playerStats.maxHealth}";
        
        if (energyText != null)
            energyText.text = $"精力：{playerStats.CurrentEnergy:F0}/{playerStats.maxEnergy}";
        
        if (hungerText != null)
            hungerText.text = $"饱食度：{playerStats.CurrentHunger:F0}/{playerStats.maxHunger}";
        
        if (radiationText != null)
            radiationText.text = $"辐射值：{playerStats.CurrentRadiation:F0}";
        
        // 更新战斗属性（显示有效值）
        if (strengthText != null)
            strengthText.text = $"力量：{playerStats.GetEffectiveStrength()}";
        
        if (accuracyText != null)
            accuracyText.text = $"精准：{playerStats.GetEffectiveAccuracy()}";
        
        if (agilityText != null)
            agilityText.text = $"敏捷：{playerStats.GetEffectiveAgility()}";
        
        if (defenseText != null)
            defenseText.text = $"防御：{playerStats.GetEffectiveDefense()}";
        
        // 更新负重和移动信息
        if (weightText != null)
            weightText.text = $"负重：{playerStats.currentWeight:F1}/{playerStats.maxWeight}kg";
        
        if (movesText != null)
            movesText.text = $"移动：{playerStats.currentTimeSegmentMoves}/{playerStats.GetMaxMovesPerSegment()}";
        
        // 更新技能显示
        if (skillText != null)
            skillText.text = $"技能：{GetSkillName(playerStats.selectedSkill)}";
    }
    
    void UpdateStatusEffectsUI()
    {
        if (playerStats == null || statusEffectsContainer == null || statusEffectPrefab == null) return;
        
        // 清空现有状态效果显示
        foreach (Transform child in statusEffectsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 显示当前状态效果
        foreach (var effect in playerStats.activeEffects)
        {
            GameObject effectObj = Instantiate(statusEffectPrefab, statusEffectsContainer);
            TextMeshProUGUI effectText = effectObj.GetComponentInChildren<TextMeshProUGUI>();
            if (effectText != null)
            {
                effectText.text = effect.description;
                effectText.color = GetEffectColor(effect.type);
            }
        }
    }
    
    void UpdateEquipmentUI()
    {
        if (itemSystem == null) return;
        
        // 使用ItemSystem中的装备数据
        UpdateEquipmentSlotDisplay(itemSystem.equippedWeapon, weaponSlot);
        UpdateEquipmentSlotDisplay(itemSystem.equippedArmor, armorSlot);
        UpdateEquipmentSlotDisplay(itemSystem.equippedAccessory, accessorySlot);
    }
    
    void UpdateEquipmentSlotDisplay(Item item, EquipmentSlot slot)
    {
        if (slot != null)
        {
            if (item != null)
            {
                // 装备物品
                slot.equippedItem = item;
                slot.UpdateSlotDisplay(item.itemName, true);
            }
            else
            {
                // 清空槽位
                slot.equippedItem = null;
                slot.UpdateSlotDisplay("未装备", false);
            }
        }
    }
    
    // ========== 装备操作 ==========
    
    public void UnequipItem(EquipmentType slotType)
    {
        if (itemSystem == null) return;
        
        switch (slotType)
        {
            case EquipmentType.Weapon:
                itemSystem.UnequipWeapon();
                break;
            case EquipmentType.Armor:
                itemSystem.UnequipArmor();
                break;
            case EquipmentType.Accessory:
                itemSystem.UnequipAccessory();
                break;
        }
        
        UpdateEquipmentUI();
        Debug.Log($"卸下了{slotType}装备");
    }
    
    public void ShowEquipmentSelection(EquipmentType slotType)
    {
        // 打开背包界面让玩家选择装备
        Debug.Log($"打开{slotType}装备选择界面");
        // 这里可以调用背包系统的装备选择功能
    }
    
    // ========== 工具方法 ==========
    
    string GetSkillName(PlayerSkill skill)
    {
        return skill switch
        {
            PlayerSkill.CombatExpert => "战斗狂人",
            PlayerSkill.Sniper => "狙击手",
            PlayerSkill.ParkourExpert => "跑酷专家",
            PlayerSkill.Navigator => "航海家",
            PlayerSkill.Tank => "肉盾",
            PlayerSkill.MarchAnt => "行军蚁",
            PlayerSkill.Medic => "医师",
            _ => "无"
        };
    }
    
    Color GetEffectColor(StatusEffectType effectType)
    {
        return effectType switch
        {
            StatusEffectType.EnergyBonus or StatusEffectType.HungerBonus => Color.green,
            StatusEffectType.TiredPenalty or StatusEffectType.FullPenalty or StatusEffectType.RadiationSickness => Color.red,
            _ => Color.yellow
        };
    }
    
    // ========== 公共接口 ==========
    
    public void ShowCharacterUI()
    {
        IsUIOpen = true;
        if (characterUI != null) characterUI.SetActive(true);
        UpdateAllUI();
    }
    
    public void HideCharacterUI()
    {
        IsUIOpen = false;
        if (characterUI != null) characterUI.SetActive(false);
    }
    
    void OnDestroy()
    {
        UnregisterEvents();
    }
}

// ========== EquipmentSlot 类定义 ==========
[System.Serializable]
public class EquipmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI组件")]
    public TextMeshProUGUI slotNameText;
    public TextMeshProUGUI itemNameText;
    public Image backgroundImage;
    
    [Header("装备类型")]
    public EquipmentType slotType;
    
    [Header("状态")]
    public Item equippedItem;
    public bool isOccupied = false;
    
    private CharacterManager characterManager;
    
    public void Initialize(EquipmentType type, CharacterManager manager)
    {
        slotType = type;
        characterManager = manager;
        
        // 设置槽位名称
        if (slotNameText != null)
        {
            slotNameText.text = GetSlotTypeName(type);
        }
        
        ClearSlot();
    }
    
    public void UpdateSlot(Item item)
    {
        if (item != null)
        {
            EquipItem(item);
        }
        else
        {
            ClearSlot();
        }
    }
    
    public void UpdateSlotDisplay(string itemName, bool isEquipped)
    {
        if (itemNameText != null)
        {
            itemNameText.text = itemName;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isEquipped ? 
                new Color(0.3f, 0.3f, 0.3f, 1f) : 
                new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
        
        isOccupied = isEquipped;
    }
    
    public void EquipItem(Item item)
    {
        if (item == null || item.equipmentType != slotType) return;
        
        equippedItem = item;
        isOccupied = true;
        
        // 更新UI显示
        UpdateSlotDisplay(item.itemName, true);
    }
    
    public void ClearSlot()
    {
        equippedItem = null;
        isOccupied = false;
        
        // 清空UI显示
        UpdateSlotDisplay("未装备", false);
    }
    
    // 点击装备槽
    public void OnSlotClicked()
    {
        if (isOccupied)
        {
            // 卸下装备
            characterManager.UnequipItem(slotType);
        }
        else
        {
            // 打开装备选择
            Debug.Log($"点击{slotType}槽位，打开装备选择");
            characterManager.ShowEquipmentSelection(slotType);
        }
    }
    
    // 鼠标悬停事件
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 背景高亮
        if (backgroundImage != null)
        {
            backgroundImage.color = isOccupied ? 
                new Color(0.4f, 0.4f, 0.3f, 1f) : 
                new Color(0.3f, 0.3f, 0.2f, 0.7f);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // 恢复背景颜色
        if (backgroundImage != null)
        {
            backgroundImage.color = isOccupied ? 
                new Color(0.3f, 0.3f, 0.3f, 1f) : 
                new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }
    
    string GetSlotTypeName(EquipmentType type)
    {
        return type switch
        {
            EquipmentType.Weapon => "武器",
            EquipmentType.Armor => "防具",
            EquipmentType.Accessory => "饰品",
            _ => "装备"
        };
    }
}

