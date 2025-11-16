using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance;
    
    [System.Serializable]
    public class CraftingRecipe
    {
        public string recipeName;
        public Item resultItem;
        public int resultQuantity = 1;
        public List<ItemRequirement> requirements;
        public float craftTime = 5f;
        public bool requiresWorkbench = false;
        
        [TextArea]
        public string description;
    }
    
    [System.Serializable]
    public struct ItemRequirement
    {
        public Item item;
        public int quantity;
    }
    
    [Header("配方列表")]
    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    [Header("引用系统")]
    public ItemDatabase itemDatabase;
    
    private ItemSystem itemSystem;
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
        itemSystem = ItemSystem.Instance;
        playerStats = FindObjectOfType<PlayerStats>();
        
        // 自动查找ItemDatabase
        if (itemDatabase == null)
        {
            itemDatabase = FindObjectOfType<ItemDatabase>();
            if (itemDatabase == null)
            {
                Debug.LogError("❌ ItemDatabase未找到！");
            }
        }
        
        InitializeRecipes();
    }
    
    void InitializeRecipes()
    {
        // ========== 医疗品配方 ==========
        CreateRecipe("消毒绷带", "可靠的医疗绷带", itemDatabase.FindItemByName("消毒绷带"), 1,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("布料"), quantity = 1 },
                new ItemRequirement { item = itemDatabase.FindItemByName("化学试剂"), quantity = 1 }
            });
        
        CreateRecipe("医疗包", "战地医疗工具包", itemDatabase.FindItemByName("医疗包"), 1,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("布料"), quantity = 2 },
                new ItemRequirement { item = itemDatabase.FindItemByName("胶带"), quantity = 1 },
                new ItemRequirement { item = itemDatabase.FindItemByName("化学试剂"), quantity = 2 }
            });
        
        CreateRecipe("抗辐射药", "降低辐射的药物", itemDatabase.FindItemByName("抗辐射药"), 3,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("化学试剂"), quantity = 3 },
                new ItemRequirement { item = itemDatabase.FindItemByName("过滤水"), quantity = 1 }
            });
        
        // ========== 武器配方 ==========
        CreateRecipe("破冰斧", "重型劈砍工具", itemDatabase.FindItemByName("破冰斧"), 1,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("废金属"), quantity = 2 },
                new ItemRequirement { item = itemDatabase.FindItemByName("木板"), quantity = 1 }
            });
        
        CreateRecipe("复合弓", "改进的远程武器", itemDatabase.FindItemByName("复合弓"), 1,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("木板"), quantity = 2 },
                new ItemRequirement { item = itemDatabase.FindItemByName("绳子"), quantity = 2 },
                new ItemRequirement { item = itemDatabase.FindItemByName("废金属"), quantity = 1 }
            });
        
        CreateRecipe("猎枪", "威力强大的霰弹枪", itemDatabase.FindItemByName("猎枪"), 1,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("废金属"), quantity = 3 },
                new ItemRequirement { item = itemDatabase.FindItemByName("电子零件"), quantity = 2 },
                new ItemRequirement { item = itemDatabase.FindItemByName("胶带"), quantity = 2 },
                new ItemRequirement { item = itemDatabase.FindItemByName("木板"), quantity = 1 }
            }, true);
        
        // ========== 防具配方 ==========
        CreateRecipe("加固皮衣", "皮革防护服", itemDatabase.FindItemByName("加固皮衣"), 1,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("布料"), quantity = 3 },
                new ItemRequirement { item = itemDatabase.FindItemByName("胶带"), quantity = 2 }
            });
        
        CreateRecipe("防弹背心", "军用防护装备", itemDatabase.FindItemByName("防弹背心"), 1,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("废金属"), quantity = 2 },
                new ItemRequirement { item = itemDatabase.FindItemByName("塑料"), quantity = 3 },
                new ItemRequirement { item = itemDatabase.FindItemByName("布料"), quantity = 2 }
            }, true);
        
        // ========== 食物配方 ==========
        CreateRecipe("烤肉串", "烤制的肉串", itemDatabase.FindItemByName("烤肉串"), 2,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("生肉"), quantity = 1 },
                new ItemRequirement { item = itemDatabase.FindItemByName("木板"), quantity = 1 }
            });
        
        CreateRecipe("炖肉汤", "营养丰富的肉汤", itemDatabase.FindItemByName("炖肉汤"), 1,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("生肉"), quantity = 1 },
                new ItemRequirement { item = itemDatabase.FindItemByName("水"), quantity = 1 },
                new ItemRequirement { item = itemDatabase.FindItemByName("蔬菜"), quantity = 1 }
            });
        
        CreateRecipe("抗辐射茶", "降低辐射的茶饮", itemDatabase.FindItemByName("抗辐射茶"), 2,
            new List<ItemRequirement> {
                new ItemRequirement { item = itemDatabase.FindItemByName("稀有草药"), quantity = 2 },
                new ItemRequirement { item = itemDatabase.FindItemByName("过滤水"), quantity = 1 }
            });
        
        Debug.Log($"配方系统初始化完成，共{allRecipes.Count}个配方");
    }
    
    void CreateRecipe(string name, string desc, Item result, int resultQty, List<ItemRequirement> requirements, bool needWorkbench = false)
    {
        if (result == null) return;
        
        CraftingRecipe recipe = new CraftingRecipe
        {
            recipeName = name,
            description = desc,
            resultItem = result,
            resultQuantity = resultQty,
            requirements = requirements,
            requiresWorkbench = needWorkbench
        };
        
        allRecipes.Add(recipe);
    }
    
    // ========== 核心制作方法 ==========
    
    public bool CanCraftRecipe(CraftingRecipe recipe)
    {
        if (recipe == null) return false;
        
        // 检查工作台需求
        if (recipe.requiresWorkbench && !HasWorkbench())
        {
            Debug.Log("需要工作台才能制作");
            return false;
        }
        
        // 检查材料是否足够
        foreach (ItemRequirement requirement in recipe.requirements)
        {
            if (!itemSystem.HasItem(requirement.item.itemName, requirement.quantity))
            {
                Debug.Log($"材料不足: {requirement.item.itemName} x{requirement.quantity}");
                return false;
            }
        }
        
        return true;
    }
    
    public bool CraftItem(CraftingRecipe recipe)
    {
        if (!CanCraftRecipe(recipe)) return false;
        
        Debug.Log($"开始制作: {recipe.recipeName}");
        
        // 消耗材料
        foreach (ItemRequirement requirement in recipe.requirements)
        {
            if (!ConsumeMaterials(requirement.item.itemName, requirement.quantity))
            {
                Debug.LogError($"消耗材料失败: {requirement.item.itemName}");
                return false;
            }
        }
        
        // 消耗精力
        if (playerStats != null)
        {
            playerStats.CurrentEnergy -= 20f;
            playerStats.CurrentHunger -= 5f;
        }
        
        // 获得成品
        for (int i = 0; i < recipe.resultQuantity; i++)
        {
            if (!itemSystem.AddItemToInventory(recipe.resultItem))
            {
                Debug.LogWarning("背包已满，制作完成但无法获得所有物品");
                break;
            }
        }
        
        Debug.Log($"制作成功: {recipe.resultItem.itemName} x{recipe.resultQuantity}");
        return true;
    }
    
    public bool CraftItemByName(string recipeName)
    {
        CraftingRecipe recipe = allRecipes.Find(r => r.recipeName == recipeName);
        if (recipe != null)
        {
            return CraftItem(recipe);
        }
        return false;
    }
    
    // ========== 材料管理 ==========
    
    private bool ConsumeMaterials(string itemName, int quantity)
    {
        int remaining = quantity;
        
        // 找到所有该材料
        List<Item> materials = itemSystem.playerInventory
            .Where(item => item.itemName == itemName)
            .OrderBy(item => item.stackCount)
            .ToList();
        
        foreach (Item material in materials)
        {
            int toRemove = Mathf.Min(remaining, material.stackCount);
            material.stackCount -= toRemove;
            remaining -= toRemove;
            
            if (material.stackCount <= 0)
            {
                itemSystem.playerInventory.Remove(material);
            }
            
            if (remaining <= 0) break;
        }
        
        if (remaining > 0)
        {
            Debug.LogError($"材料消耗错误: {itemName}，需要{quantity}但只消耗了{quantity - remaining}");
            return false;
        }
        
        itemSystem.UpdateTotalWeight();
        return true;
    }
    
    // ========== 工具方法 ==========
    
    public List<CraftingRecipe> GetAvailableRecipes()
    {
        return allRecipes.Where(recipe => CanCraftRecipe(recipe)).ToList();
    }
    
    public List<CraftingRecipe> GetRecipesByCategory(ItemType category)
    {
        return allRecipes.Where(recipe => recipe.resultItem.itemType == category).ToList();
    }
    
    public bool HasWorkbench()
    {
        // 这里可以检查玩家是否在工作台附近
        // 暂时返回true用于测试
        return true;
    }
    
    public string GetRequirementsText(CraftingRecipe recipe)
    {
        if (recipe == null) return "";
        
        string text = "所需材料:\n";
        foreach (ItemRequirement requirement in recipe.requirements)
        {
            int haveCount = itemSystem.GetItemCount(requirement.item.itemName);
            string color = haveCount >= requirement.quantity ? "green" : "red";
            text += $"<color={color}>{requirement.item.itemName} x{requirement.quantity} ({haveCount})</color>\n";
        }
        
        if (recipe.requiresWorkbench)
        {
            text += "\n需要工作台";
        }
        
        return text;
    }
    
    // ========== 批量制作 ==========
    
    public int GetMaxCraftableAmount(CraftingRecipe recipe)
    {
        if (recipe == null) return 0;
        
        int maxAmount = int.MaxValue;
        
        foreach (ItemRequirement requirement in recipe.requirements)
        {
            int haveCount = itemSystem.GetItemCount(requirement.item.itemName);
            int canCraft = Mathf.FloorToInt(haveCount / requirement.quantity);
            maxAmount = Mathf.Min(maxAmount, canCraft);
        }
        
        return maxAmount;
    }
    
    public bool CraftMultiple(CraftingRecipe recipe, int amount)
    {
        if (amount <= 0) return false;
        
        for (int i = 0; i < amount; i++)
        {
            if (!CraftItem(recipe))
            {
                Debug.Log($"批量制作中断，成功制作{i}个");
                return i > 0;
            }
        }
        
        Debug.Log($"批量制作成功: {recipe.resultItem.itemName} x{amount}");
        return true;
    }
}