using UnityEngine;

[System.Serializable]
public class Item : MonoBehaviour
{
    [Header("基础信息")]
    public string itemName;
    public string itemDescription;
    public Sprite icon;
    public ItemType itemType;
    public ItemRarity rarity = ItemRarity.Common;
    
    [Header("物品属性")]
    public float weight = 0.1f;
    public bool isStackable = false;
    public int stackCount = 1;
    public int maxStackSize = 1;
    public int value = 10;
    
    [Header("装备相关")]
    public bool isEquippable = false;
    public EquipmentType equipmentType;
    
    [Header("属性加成")]
    public int strengthBonus = 0;
    public int accuracyBonus = 0;
    public int agilityBonus = 0;
    public int defenseBonus = 0;
    public int healthBonus = 0;
    
    [Header("医疗效果")]
    public int healthRestore = 0;
    public float radiationReduction = 0;
    public StatusEffectType curesEffect; // 治愈的状态效果
    public float effectChance = 1f; // 效果触发几率
    public StatusEffectType negativeEffect; // 负面效果
    public float negativeEffectChance = 0f; // 负面效果几率
    
    [Header("食物效果")]
    public float hungerRestore = 0;
    public float energyRestore = 0;
    public bool requiresCooking = false;
    
    [Header("合成材料")]
    public CraftingRequirement[] craftingRequirements;
    
    // 克隆方法
    public Item Clone()
    {
        Item newItem = new Item();
        newItem.itemName = this.itemName;
        newItem.itemDescription = this.itemDescription;
        newItem.icon = this.icon;
        newItem.itemType = this.itemType;
        newItem.rarity = this.rarity;
        newItem.weight = this.weight;
        newItem.isStackable = this.isStackable;
        newItem.stackCount = this.stackCount;
        newItem.maxStackSize = this.maxStackSize;
        newItem.value = this.value;
        newItem.isEquippable = this.isEquippable;
        newItem.equipmentType = this.equipmentType;
        newItem.strengthBonus = this.strengthBonus;
        newItem.accuracyBonus = this.accuracyBonus;
        newItem.agilityBonus = this.agilityBonus;
        newItem.defenseBonus = this.defenseBonus;
        newItem.healthBonus = this.healthBonus;
        newItem.healthRestore = this.healthRestore;
        newItem.radiationReduction = this.radiationReduction;
        newItem.curesEffect = this.curesEffect;
        newItem.effectChance = this.effectChance;
        newItem.negativeEffect = this.negativeEffect;
        newItem.negativeEffectChance = this.negativeEffectChance;
        newItem.hungerRestore = this.hungerRestore;
        newItem.energyRestore = this.energyRestore;
        newItem.requiresCooking = this.requiresCooking;
        
        return newItem;
    }
    
    // 获取完整描述
    public string GetFullDescription()
    {
        string description = $"<b>{itemName}</b>\n";
        description += $"{itemDescription}\n\n";
        
        // 医疗效果
        if (healthRestore > 0) description += $"恢复生命: +{healthRestore}\n";
        if (radiationReduction > 0) description += $"降低辐射: -{radiationReduction}\n";
        if (curesEffect != StatusEffectType.None) description += $"治愈: {GetEffectName(curesEffect)}\n";
        
        // 食物效果
        if (hungerRestore > 0) description += $"饱食度: +{hungerRestore}\n";
        if (energyRestore > 0) description += $"精力: +{energyRestore}\n";
        
        // 属性加成
        if (strengthBonus != 0) description += $"力量: {(strengthBonus > 0 ? "+" : "")}{strengthBonus}\n";
        if (accuracyBonus != 0) description += $"精准: {(accuracyBonus > 0 ? "+" : "")}{accuracyBonus}\n";
        if (agilityBonus != 0) description += $"敏捷: {(agilityBonus > 0 ? "+" : "")}{agilityBonus}\n";
        if (defenseBonus != 0) description += $"防御: {(defenseBonus > 0 ? "+" : "")}{defenseBonus}\n";
        if (healthBonus != 0) description += $"生命上限: {(healthBonus > 0 ? "+" : "")}{healthBonus}\n";
        
        // 几率效果
        if (effectChance < 1f) description += $"触发几率: {effectChance * 100}%\n";
        if (negativeEffectChance > 0f) 
            description += $"负面效果几率: {negativeEffectChance * 100}% ({GetEffectName(negativeEffect)})\n";
        
        description += $"\n重量: {weight}kg";
        if (isStackable) description += $"\n数量: {stackCount}/{maxStackSize}";
        
        return description;
    }
    
    string GetEffectName(StatusEffectType effect)
    {
        switch (effect)
        {
            case StatusEffectType.Infection: return "感染";
            case StatusEffectType.Diarrhea: return "腹泻";
            case StatusEffectType.Bleeding: return "流血";
            case StatusEffectType.DeepWound: return "深度裂伤";
            case StatusEffectType.Fracture: return "骨折";
            case StatusEffectType.RadiationSickness: return "辐射病";
            default: return effect.ToString();
        }
    }
}

[System.Serializable]
public struct CraftingRequirement
{
    public Item item;
    public int quantity;
}

public enum ItemType
{
    All,        // 添加All类型（用于显示全部物品）
    Weapon,     // 武器
    Armor,      // 防具
    Accessory,  // 饰品
    Consumable, // 消耗品
    Material,   // 材料
    Special     // 特殊（添加Special类型）
}
public enum ItemRarity
{
    Common, Uncommon, Rare, Epic, Legendary
}

public enum EquipmentType
{
    Weapon,     // 武器
    Armor,      // 防具
    Accessory   // 饰品
}


