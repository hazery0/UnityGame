using System.Collections.Generic;
using UnityEngine;

public class GridEventSystem : MonoBehaviour
{
    public static GridEventSystem Instance;
    
    [System.Serializable]
    public class GridEvent
    {
        public Vector2Int coord;      // 2x2网格坐标
        public EventType eventType;
        public int eventParam;
        public bool isOneTime = true; // 默认一次性事件
        [TextArea] public string eventDescription; // 事件描述
        [HideInInspector] public bool hasTriggered;
        
        // 战斗相关参数（如果是战斗事件）
        public string enemyName;
        public int enemyStrength;
        public int enemyAccuracy;
        public int enemyAgility;
        public bool isRangedOnly = false; // 是否只能远程攻击
        public int combatStages = 1;     // 战斗阶段数（用于空投点等多阶段战斗）
    }

    public enum EventType 
    { 
        // 资源点事件
        ScrapYard,           // 废品站
        AbandonedCamp,       // 废弃营地（存档点）
        RaiderCamp,          // 强盗营地
        MedicalStation,      // 医疗站
        AbandonedRestaurant, // 废弃餐厅
        
        // 遭遇事件
        AnimalGroup,         // 动物群（强制远程）
        RadiationAnimalGroup,// 辐射动物群
        WanderingRaider,     // 流浪强盗
        Airdrop,            // 空投点（多阶段战斗）
        
        // 环境事件
        RadiationStorm,      // 辐射风暴
        NormalWeather,       // 正常天气
        
        // 基础测试事件（保留原有）
        Damage, 
        Heal 
    }

    [Header("事件配置")]
    public List<GridEvent> gridEvents = new();

    private Dictionary<Vector2Int, GridEvent> _eventDict;
    private PlayerStats _playerStats;

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
        _playerStats = FindObjectOfType<PlayerStats>();
        _eventDict = new Dictionary<Vector2Int, GridEvent>();
        
        foreach (var e in gridEvents)
        {
            _eventDict[e.coord] = e;
        }
        
        // 自动生成一些示例事件（开发测试用）
        GenerateTestEvents();
    }

    public bool TryGetEvent(Vector2Int coord, out GridEvent gridEvent)
    {
        if (_eventDict.TryGetValue(coord, out gridEvent))
        {
            if (gridEvent.isOneTime && gridEvent.hasTriggered)
                return false;
            return true;
        }
        return false;
    }

    public void MarkEventTriggered(Vector2Int coord)
    {
        if (_eventDict.ContainsKey(coord))
        {
            _eventDict[coord].hasTriggered = true;
        }
    }

    // 执行事件逻辑
    public void ExecuteEvent(GridEvent gridEvent)
    {
        if (_playerStats == null) return;
        
        // 触发事件消耗精力（根据设计文档）
        _playerStats.ConsumeEvent();
        
        switch (gridEvent.eventType)
        {
            case EventType.ScrapYard:
                ExecuteScrapYardEvent(gridEvent);
                break;
                
            case EventType.AbandonedCamp:
                ExecuteAbandonedCampEvent(gridEvent);
                break;
                
            case EventType.RaiderCamp:
                ExecuteRaiderCampEvent(gridEvent);
                break;
                
            case EventType.MedicalStation:
                ExecuteMedicalStationEvent(gridEvent);
                break;
                
            case EventType.AbandonedRestaurant:
                ExecuteRestaurantEvent(gridEvent);
                break;
                
            case EventType.AnimalGroup:
                ExecuteAnimalGroupEvent(gridEvent, false);
                break;
                
            case EventType.RadiationAnimalGroup:
                ExecuteAnimalGroupEvent(gridEvent, true);
                break;
                
            case EventType.WanderingRaider:
                ExecuteWanderingRaiderEvent(gridEvent);
                break;
                
            case EventType.Airdrop:
                ExecuteAirdropEvent(gridEvent);
                break;
                
            case EventType.RadiationStorm:
                ExecuteRadiationStormEvent(gridEvent);
                break;
                
            // 保留原有基础事件
            case EventType.Damage:
                _playerStats.TakeDamage(gridEvent.eventParam);
                Debug.Log($"触发伤害事件，扣血{gridEvent.eventParam}");
                break;
                
            case EventType.Heal:
                _playerStats.Heal(gridEvent.eventParam);
                Debug.Log($"触发治疗事件，回血{gridEvent.eventParam}");
                break;
        }
        
        // 标记已触发（如果是一次性事件）
        if (gridEvent.isOneTime)
        {
            MarkEventTriggered(gridEvent.coord);
        }
        
        Debug.Log($"触发事件: {gridEvent.eventType} - {gridEvent.eventDescription}");
    }

    // ========== 具体事件执行方法 ==========
    private void ExecuteScrapYardEvent(GridEvent gridEvent)
    {
        // 概率获得基础材料
        if (Random.value > 0.3f)
        {
            Debug.Log("在废品站找到了有用的材料！");
            // 这里应该调用物品系统添加材料
            // InventorySystem.Instance.AddItem(ItemType.Material, Random.Range(1, 4));
        }
        else
        {
            Debug.Log("废品站已经被搜刮干净了...");
        }
    }

    private void ExecuteAbandonedCampEvent(GridEvent gridEvent)
    {
        Debug.Log("到达废弃营地，可以存档和存放物品");
        // 触发存档界面
        // SaveSystem.ShowSaveMenu();
        // 触发物品存储界面
        // StorageSystem.ShowStorageUI();
    }

    private void ExecuteRaiderCampEvent(GridEvent gridEvent)
    {
        Debug.Log("发现强盗营地，需要战斗才能获取物资");
        StartCombat(gridEvent);
    }

    private void ExecuteMedicalStationEvent(GridEvent gridEvent)
    {
        if (Random.value > 0.4f)
        {
            Debug.Log("在医疗站找到了医疗用品！");
            // InventorySystem.Instance.AddItem(ItemType.Medical, Random.Range(1, 3));
        }
        else
        {
            Debug.Log("医疗站里什么都没有...");
        }
    }

    private void ExecuteRestaurantEvent(GridEvent gridEvent)
    {
        if (Random.value > 0.5f)
        {
            Debug.Log("在废弃餐厅找到了食物！");
            // InventorySystem.Instance.AddItem(ItemType.Food, Random.Range(1, 3));
            _playerStats.CurrentHunger += 20f; // 直接恢复一些饱食度
        }
        else
        {
            Debug.Log("餐厅里的食物已经变质了...");
        }
    }

    private void ExecuteAnimalGroupEvent(GridEvent gridEvent, bool isRadiation)
    {
        string animalType = isRadiation ? "辐射动物" : "普通动物";
        Debug.Log($"遭遇{animalType}群");
        StartCombat(gridEvent);
    }

    private void ExecuteWanderingRaiderEvent(GridEvent gridEvent)
    {
        Debug.Log("遭遇流浪强盗！");
        StartCombat(gridEvent);
    }

    private void ExecuteAirdropEvent(GridEvent gridEvent)
    {
        Debug.Log("发现空投点！但周围有很多敌人...");
        StartCombat(gridEvent);
    }

    private void ExecuteRadiationStormEvent(GridEvent gridEvent)
    {
        Debug.Log("遭遇辐射风暴！");
        _playerStats.TakeDamage(10);
        _playerStats.CurrentRadiation += 15f;
        Debug.Log("受到辐射伤害，辐射值增加");
    }

    // ========== 战斗系统调用 ==========
private void StartCombat(GridEvent gridEvent)
{
    if (CombatManager.Instance != null && CombatManager.Instance.isInCombat == false)
    {
        // 使用正确的方法名和参数
        CombatManager.Instance.StartBattle(
            gridEvent.enemyName,
            gridEvent.enemyStrength,
            gridEvent.enemyAccuracy,
            gridEvent.enemyAgility,
            gridEvent.isRangedOnly,
            gridEvent.combatStages
        );
    }
    else
    {
        Debug.LogWarning("战斗管理器未找到或已在战斗中");
    }
}

    // ========== 测试事件生成 ==========
    private void GenerateTestEvents()
    {
        // 示例事件生成（开发测试用）
        AddTestEvent(new Vector2Int(1, 1), EventType.ScrapYard, 0, "废品站：可能找到基础材料");
        AddTestEvent(new Vector2Int(2, 2), EventType.MedicalStation, 0, "医疗站：可能找到医疗品");
        AddTestEvent(new Vector2Int(3, 3), EventType.RaiderCamp, 0, "强盗营地", "强盗", 15, 12, 10);
    }

    private void AddTestEvent(Vector2Int coord, EventType type, int param, string desc, 
                             string enemyName = "", int str = 0, int acc = 0, int agi = 0)
    {
        if (!_eventDict.ContainsKey(coord))
        {
            var newEvent = new GridEvent
            {
                coord = coord,
                eventType = type,
                eventParam = param,
                eventDescription = desc,
                enemyName = enemyName,
                enemyStrength = str,
                enemyAccuracy = acc,
                enemyAgility = agi
            };
            
            gridEvents.Add(newEvent);
            _eventDict[coord] = newEvent;
        }
    }

    // ========== 工具方法 ==========
    public void ResetAllEvents()
    {
        foreach (var e in gridEvents)
        {
            e.hasTriggered = false;
        }
        Debug.Log("所有事件状态已重置");
    }

    public List<GridEvent> GetActiveEvents()
    {
        List<GridEvent> activeEvents = new List<GridEvent>();
        foreach (var e in gridEvents)
        {
            if (!e.hasTriggered || !e.isOneTime)
            {
                activeEvents.Add(e);
            }
        }
        return activeEvents;
    }
}