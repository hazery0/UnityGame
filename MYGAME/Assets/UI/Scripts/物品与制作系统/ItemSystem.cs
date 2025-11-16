using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemSystem : MonoBehaviour
{
    public static ItemSystem Instance;
    
    [Header("物品数据库")]
    public ItemDatabase itemDatabase;
    
    [Header("玩家物品栏")]
    public List<Item> playerInventory = new List<Item>();
    public int maxInventorySlots = 20;
    
    [Header("装备物品")]
    public Item equippedWeapon;
    public Item equippedArmor;
    public Item equippedAccessory;
    
    [Header("负重系统")]
    public float currentWeight = 0f;
    
    // ========== 添加缺失的事件 ==========
    public System.Action OnEquipmentChanged; // 添加这个事件
    
    private PlayerStats playerStats;
    
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
        playerStats = FindObjectOfType<PlayerStats>();
        
        // 初始化一些基础物品
        InitializeStarterItems();
        
        // 计算初始负重
        UpdateTotalWeight();
    }
    
    void InitializeStarterItems()
    {
        // 添加初始物品
        AddItemToInventory(itemDatabase.FindItemByName("撬棍"));
        AddItemToInventory(itemDatabase.FindItemByName("粗制弓"));
        AddItemToInventory(itemDatabase.FindItemByName("脏绷带"), 3);
        AddItemToInventory(itemDatabase.FindItemByName("变异果"), 5);
        AddItemToInventory(itemDatabase.FindItemByName("水"), 2);
    }
    
    // ========== 物品管理方法 ==========
    
    public bool AddItemToInventory(Item item, int quantity = 1)
    {
        if (item == null) return false;
        
        // 检查背包空间
        if (playerInventory.Count >= maxInventorySlots && !item.isStackable)
        {
            Debug.LogWarning("背包已满，无法添加物品");
            return false;
        }
        
        // 检查负重
        float newWeight = currentWeight + (item.weight * quantity);
        if (newWeight > playerStats.maxWeight)
        {
            Debug.LogWarning("负重已满，无法添加物品");
            return false;
        }
        
        // 检查是否可堆叠
        if (item.isStackable)
        {
            Item existingItem = playerInventory.Find(i => i.itemName == item.itemName);
            if (existingItem != null && existingItem.stackCount < existingItem.maxStackSize)
            {
                // 堆叠到现有物品
                int spaceLeft = existingItem.maxStackSize - existingItem.stackCount;
                int toAdd = Mathf.Min(quantity, spaceLeft);
                existingItem.stackCount += toAdd;
                
                // 如果还有剩余，创建新堆叠
                if (toAdd < quantity)
                {
                    Item newStack = item.Clone();
                    newStack.stackCount = quantity - toAdd;
                    playerInventory.Add(newStack);
                }
                
                UpdateTotalWeight();
                Debug.Log($"堆叠物品: {item.itemName}，数量: {existingItem.stackCount}");
                return true;
            }
        }
        
        // 添加新物品
        Item newItem = item.Clone();
        newItem.stackCount = quantity;
        playerInventory.Add(newItem);
        
        UpdateTotalWeight();
        Debug.Log($"获得物品: {item.itemName} x{quantity}");
        return true;
    }
    
    public bool RemoveItemFromInventory(Item item, int quantity = 1)
    {
        if (item == null || !playerInventory.Contains(item)) return false;
        
        if (item.isStackable && item.stackCount > quantity)
        {
            // 减少堆叠数量
            item.stackCount -= quantity;
            Debug.Log($"移除物品: {item.itemName} x{quantity}，剩余: {item.stackCount}");
        }
        else
        {
            // 完全移除物品
            playerInventory.Remove(item);
            Debug.Log($"移除物品: {item.itemName}");
        }
        
        UpdateTotalWeight();
        return true;
    }
    
    public bool HasItem(string itemName, int requiredQuantity = 1)
    {
        int totalCount = 0;
        foreach (Item item in playerInventory.Where(i => i.itemName == itemName))
        {
            totalCount += item.stackCount;
            if (totalCount >= requiredQuantity) return true;
        }
        return false;
    }
    
    public int GetItemCount(string itemName)
    {
        return playerInventory
            .Where(i => i.itemName == itemName)
            .Sum(i => i.stackCount);
    }
    
    public void UpdateTotalWeight()
    {
        currentWeight = 0f;
        
        // 装备重量
        if (equippedWeapon != null) currentWeight += equippedWeapon.weight;
        if (equippedArmor != null) currentWeight += equippedArmor.weight;
        if (equippedAccessory != null) currentWeight += equippedAccessory.weight;
        
        // 背包物品重量
        foreach (Item item in playerInventory)
        {
            currentWeight += item.weight * item.stackCount;
        }
        
        // 更新玩家状态
        if (playerStats != null)
        {
            playerStats.currentWeight = currentWeight;
            playerStats.OnStatsUpdated?.Invoke();
        }
    }
    
    // ========== 装备系统 ==========
    
    public void EquipItem(Item item)
    {
        if (item == null || !item.isEquippable) return;
        
        Debug.Log($"尝试装备: {item.itemName}");
        
        // 根据装备类型处理
        switch (item.equipmentType)
        {
            case EquipmentType.Weapon:
                if (equippedWeapon != null) UnEquipItem(equippedWeapon);
                equippedWeapon = item;
                break;
                
            case EquipmentType.Armor:
                if (equippedArmor != null) UnEquipItem(equippedArmor);
                equippedArmor = item;
                break;
                
            case EquipmentType.Accessory:
                if (equippedAccessory != null) UnEquipItem(equippedAccessory);
                equippedAccessory = item;
                break;
        }
        
        // 从背包移除
        if (playerInventory.Contains(item))
        {
            playerInventory.Remove(item);
        }
        
        UpdateTotalWeight();
        ApplyEquipmentEffects();
        
        // 触发装备变更事件
        OnEquipmentChanged?.Invoke();
        
        Debug.Log($"装备了: {item.itemName}");
    }
    
    public void UnEquipItem(Item item)
    {
        if (item == null) return;
        
        // 卸下装备效果
        RemoveEquipmentEffects(item);
        
        // 放回背包
        if (AddItemToInventory(item))
        {
            // 清空装备槽
            switch (item.equipmentType)
            {
                case EquipmentType.Weapon:
                    equippedWeapon = null;
                    break;
                case EquipmentType.Armor:
                    equippedArmor = null;
                    break;
                case EquipmentType.Accessory:
                    equippedAccessory = null;
                    break;
            }
            
            UpdateTotalWeight();
            
            // 触发装备变更事件
            OnEquipmentChanged?.Invoke();
            
            Debug.Log($"卸下了: {item.itemName}");
        }
        else
        {
            Debug.LogWarning("背包已满，无法卸下装备");
        }
    }
    
    // ========== 添加缺失的卸下方法 ==========
    public void UnequipWeapon()
    {
        if (equippedWeapon != null)
        {
            UnEquipItem(equippedWeapon);
        }
    }
    
    public void UnequipArmor()
    {
        if (equippedArmor != null)
        {
            UnEquipItem(equippedArmor);
        }
    }
    
    public void UnequipAccessory()
    {
        if (equippedAccessory != null)
        {
            UnEquipItem(equippedAccessory);
        }
    }
    
    public void UnEquipItem(EquipmentType slotType)
    {
        switch (slotType)
        {
            case EquipmentType.Weapon:
                if (equippedWeapon != null) UnEquipItem(equippedWeapon);
                break;
            case EquipmentType.Armor:
                if (equippedArmor != null) UnEquipItem(equippedArmor);
                break;
            case EquipmentType.Accessory:
                if (equippedAccessory != null) UnEquipItem(equippedAccessory);
                break;
        }
    }
    
    private void ApplyEquipmentEffects()
    {
        if (playerStats == null) return;
        
        // 重置基础属性
        playerStats.strength = 10;
        playerStats.accuracy = 10;
        playerStats.agility = 10;
        playerStats.defense = 10;
        
        // 应用装备加成
        Item[] equippedItems = { equippedWeapon, equippedArmor, equippedAccessory };
        foreach (Item item in equippedItems)
        {
            if (item != null)
            {
                playerStats.strength += item.strengthBonus;
                playerStats.accuracy += item.accuracyBonus;
                playerStats.agility += item.agilityBonus;
                playerStats.defense += item.defenseBonus;
                playerStats.maxHealth += item.healthBonus;
            }
        }
        
        playerStats.OnStatsUpdated?.Invoke();
    }
    
    private void RemoveEquipmentEffects(Item item)
    {
        if (playerStats == null || item == null) return;
        
        playerStats.strength -= item.strengthBonus;
        playerStats.accuracy -= item.accuracyBonus;
        playerStats.agility -= item.agilityBonus;
        playerStats.defense -= item.defenseBonus;
        playerStats.maxHealth -= item.healthBonus;
        
        // 确保属性不低于1
        playerStats.strength = Mathf.Max(1, playerStats.strength);
        playerStats.accuracy = Mathf.Max(1, playerStats.accuracy);
        playerStats.agility = Mathf.Max(1, playerStats.agility);
        playerStats.defense = Mathf.Max(1, playerStats.defense);
        playerStats.maxHealth = Mathf.Max(1, playerStats.maxHealth);
    }
    
    // ========== 物品使用系统 ==========
    
    public void UseConsumable(Item item, PlayerStats targetStats = null)
    {
        if (item == null) return;
        
        PlayerStats stats = targetStats ?? playerStats;
        if (stats == null) return;
        
        Debug.Log($"使用物品: {item.itemName}");
        
        // 应用治疗效果
        if (item.healthRestore > 0)
        {
            stats.Heal((int)item.healthRestore);
        }
        
        // 应用辐射效果
        if (item.radiationReduction != 0)
        {
            if (item.radiationReduction > 0)
                stats.ReduceRadiation(item.radiationReduction);
            else
                stats.IncreaseRadiation(-item.radiationReduction);
        }
        
        // 应用饱食度效果
        if (item.hungerRestore > 0)
        {
            stats.CurrentHunger = Mathf.Min(stats.maxHunger, stats.CurrentHunger + item.hungerRestore);
        }
        
        // 应用精力效果
        if (item.energyRestore != 0)
        {
            stats.CurrentEnergy = Mathf.Clamp(stats.CurrentEnergy + item.energyRestore, 0, stats.maxEnergy);
        }
        
        // 治愈状态效果
        if (item.curesEffect != StatusEffectType.None)
        {
            stats.CureStatusEffect(item.curesEffect);
        }
        
        // 检查负面效果
        if (item.negativeEffectChance > 0f && Random.value <= item.negativeEffectChance)
        {
            stats.ApplyStatusEffect(item.negativeEffect, 3600); // 1小时持续时间
            Debug.Log($"触发负面效果: {item.negativeEffect}");
        }
        
        // 应用临时属性加成
        if (item.strengthBonus != 0 || item.accuracyBonus != 0 || item.agilityBonus != 0)
        {
            stats.ApplyTemporaryBonus(item.strengthBonus, item.accuracyBonus, item.agilityBonus, 600); // 10分钟
        }
        
        // 从背包移除
        if (item.isStackable && item.stackCount > 1)
        {
            item.stackCount--;
            Debug.Log($"使用了: {item.itemName}，剩余数量: {item.stackCount}");
        }
        else
        {
            playerInventory.Remove(item);
            Debug.Log($"使用了: {item.itemName}，物品已消耗");
        }
        
        UpdateTotalWeight();
        stats.OnStatsUpdated?.Invoke();
    }
    
    // ========== 工具方法 ==========
    
    public List<Item> GetEquippableItems()
    {
        return playerInventory.Where(item => item.isEquippable).ToList();
    }
    
    public List<Item> GetConsumableItems()
    {
        return playerInventory.Where(item => item.itemType == ItemType.Consumable).ToList();
    }
    
    public List<Item> GetCraftingMaterials()
    {
        return playerInventory.Where(item => item.itemType == ItemType.Material).ToList();
    }
    
    public void DebugInventory()
    {
        Debug.Log("=== 背包内容 ===");
        Debug.Log($"物品数量: {playerInventory.Count}/{maxInventorySlots}");
        Debug.Log($"总重量: {currentWeight:F1}/{playerStats.maxWeight}kg");
        
        foreach (Item item in playerInventory)
        {
            string stackInfo = item.isStackable ? $" x{item.stackCount}" : "";
            Debug.Log($"- {item.itemName}{stackInfo} ({item.itemType})");
        }
        
        Debug.Log("=== 装备 ===");
        Debug.Log($"武器: {equippedWeapon?.itemName ?? "无"}");
        Debug.Log($"防具: {equippedArmor?.itemName ?? "无"}");
        Debug.Log($"饰品: {equippedAccessory?.itemName ?? "无"}");
    }
    
}