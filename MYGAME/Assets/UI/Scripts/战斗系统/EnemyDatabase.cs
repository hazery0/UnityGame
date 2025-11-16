using UnityEngine;
using System.Collections.Generic;

public class EnemyDatabase : MonoBehaviour
{
    public static EnemyDatabase Instance;
    
    [Header("敌人列表")]
    public List<Enemy> allEnemies = new List<Enemy>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEnemies();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeEnemies()
    {
        allEnemies.Clear();
        
        // ========== 人类敌人 ==========
        allEnemies.Add(new Enemy("虚弱掠夺者", 8, 9, 7)
        {
            description = "营养不良的掠夺者，战斗能力较弱",
            lootTable = new List<LootDrop>
            {
                new LootDrop { item = GetItemByName("脏绷带"), minQuantity = 1, maxQuantity = 2, dropChance = 0.7f },
                new LootDrop { item = GetItemByName("废金属"), minQuantity = 1, maxQuantity = 3, dropChance = 0.8f }
            }
        });
        
        allEnemies.Add(new Enemy("强盗哨兵", 12, 14, 10)
        {
            description = "装备较好的强盗，擅长远程攻击",
            lootTable = new List<LootDrop>
            {
                new LootDrop { item = GetItemByName("手枪"), minQuantity = 1, maxQuantity = 1, dropChance = 0.3f },
                new LootDrop { item = GetItemByName("弹药"), minQuantity = 5, maxQuantity = 10, dropChance = 0.6f },
                new LootDrop { item = GetItemByName("防弹背心"), minQuantity = 1, maxQuantity = 1, dropChance = 0.2f }
            }
        });
        
        allEnemies.Add(new Enemy("掠夺者狂战士", 17, 5, 8)
        {
            description = "狂暴的近战战士，力量惊人但精准度差",
            lootTable = new List<LootDrop>
            {
                new LootDrop { item = GetItemByName("破冰斧"), minQuantity = 1, maxQuantity = 1, dropChance = 0.4f },
                new LootDrop { item = GetItemByName("废金属"), minQuantity = 3, maxQuantity = 6, dropChance = 0.9f },
                new LootDrop { item = GetItemByName("类固醇合剂"), minQuantity = 1, maxQuantity = 1, dropChance = 0.3f }
            }
        });
        
        // ========== 动物敌人 ==========
        allEnemies.Add(new Enemy("变异狼", 14, 2, 16)
        {
            description = "受辐射变异的狼，速度极快",
            lootTable = new List<LootDrop>
            {
                new LootDrop { item = GetItemByName("生肉"), minQuantity = 2, maxQuantity = 4, dropChance = 1f },
                new LootDrop { item = GetItemByName("变异肉"), minQuantity = 1, maxQuantity = 2, dropChance = 0.5f },
                new LootDrop { item = GetItemByName("狼牙"), minQuantity = 1, maxQuantity = 3, dropChance = 0.7f }
            }
        });
        
        // ========== 变异生物 ==========
        allEnemies.Add(new Enemy("辐射蜘蛛", 10, 0, 14)
        {
            description = "巨型辐射蜘蛛，带有剧毒",
            lootTable = new List<LootDrop>
            {
                new LootDrop { item = GetItemByName("毒液"), minQuantity = 1, maxQuantity = 2, dropChance = 0.8f },
                new LootDrop { item = GetItemByName("蜘蛛丝"), minQuantity = 2, maxQuantity = 5, dropChance = 0.6f },
                new LootDrop { item = GetItemByName("抗辐射药"), minQuantity = 1, maxQuantity = 1, dropChance = 0.3f }
            }
        });
        
        // ========== 机械敌人 ==========
        allEnemies.Add(new Enemy("安全机器人", 16, 15, 4)
        {
            description = "旧世界的安保机器人，精准但笨重",
            lootTable = new List<LootDrop>
            {
                new LootDrop { item = GetItemByName("电子零件"), minQuantity = 3, maxQuantity = 6, dropChance = 1f },
                new LootDrop { item = GetItemByName("动力电池"), minQuantity = 1, maxQuantity = 1, dropChance = 0.4f },
                new LootDrop { item = GetItemByName("精密镜片"), minQuantity = 1, maxQuantity = 2, dropChance = 0.3f }
            }
        });
        
        Debug.Log($"敌人数据库初始化完成，共{allEnemies.Count}种敌人");
    }
    
    // 安全的物品获取方法
    private Item GetItemByName(string itemName)
    {
        if (ItemDatabase.Instance != null)
        {
            return ItemDatabase.Instance.FindItemByName(itemName);
        }
        else
        {
            Debug.LogWarning("ItemDatabase未找到，创建临时物品");
            return CreateTempItem(itemName);
        }
    }
    
    private Item CreateTempItem(string itemName)
    {
        return new Item
        {
            itemName = itemName,
            itemType = ItemType.Material,
            isStackable = true,
            maxStackSize = 10
        };
    }
    
    // 根据名称查找敌人
    public Enemy FindEnemyByName(string name)
    {
        return allEnemies.Find(e => e.enemyName == name);
    }
    
    // 获取所有敌人
    public List<Enemy> GetAllEnemies()
    {
        return allEnemies;
    }
    
    // 获取随机敌人
    public Enemy GetRandomEnemy()
    {
        if (allEnemies.Count == 0) return null;
        return allEnemies[Random.Range(0, allEnemies.Count)];
    }
    
    // 生成敌人战利品（简化版，只掉落物品）
    public List<Item> GenerateLoot(Enemy enemy)
    {
        List<Item> loot = new List<Item>();
        
        // 只生成掉落物品，没有金币
        if (enemy.lootTable != null)
        {
            foreach (LootDrop drop in enemy.lootTable)
            {
                if (Random.value <= drop.dropChance && drop.item != null)
                {
                    Item lootItem = drop.item.Clone();
                    lootItem.stackCount = Random.Range(drop.minQuantity, drop.maxQuantity + 1);
                    loot.Add(lootItem);
                }
            }
        }
        
        return loot;
    }
}

// ========== Enemy类定义 ==========
[System.Serializable]
public class Enemy
{
    public string enemyName;
    public int strength;
    public int accuracy;
    public int agility;
    public string description;
    public List<LootDrop> lootTable;
    
    // 简化构造函数 - 只有4个基本属性
    public Enemy(string name, int str, int acc, int agi)
    {
        enemyName = name;
        strength = str;
        accuracy = acc;
        agility = agi;
        lootTable = new List<LootDrop>();
    }
}

// ========== 战利品掉落定义 ==========
[System.Serializable]
public class LootDrop
{
    public Item item;
    public int minQuantity = 1;
    public int maxQuantity = 1;
    public float dropChance = 0.5f;
}