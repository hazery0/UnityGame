using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RestSystem : MonoBehaviour
{
    public static RestSystem Instance;
    
    [Header("UI引用")]
    public GameObject restPanel;
    public TextMeshProUGUI timeInfoText;
    public TextMeshProUGUI costInfoText;
    public TextMeshProUGUI resultInfoText;
    
    [Header("修整按钮")]
    public Button sleepButton;
    public Button trainButton;
    public Button craftButton; // 制作按钮
    public Button cancelButton;
    
    [Header("训练子菜单")]
    public GameObject trainSubMenu;
    public Button strengthTrainButton;
    public Button accuracyTrainButton;
    public Button agilityTrainButton;
    public Button backButton;
    
    [Header("制作面板引用")]
    public GameObject craftingPanel; // 制作面板
    public Button closeCraftingButton; // 制作面板关闭按钮
    
    [Header("属性显示")]
    public TextMeshProUGUI currentStrengthText;
    public TextMeshProUGUI currentAccuracyText;
    public TextMeshProUGUI currentAgilityText;
    public TextMeshProUGUI currentHealthText;
    public TextMeshProUGUI currentEnergyText;
    public TextMeshProUGUI currentHungerText;
    
    [Header("修整效果设置")]
    public int baseHungerCost = 5;
    public int trainEnergyCost = 20;
    public int craftEnergyCost = 20;
    public float daySleepEnergyRecovery = 20f;
    public float nightSleepEnergyRecovery = 30f;
    public int sleepHealthRecovery = 10;
    
    private PlayerStats playerStats;
    private TimeManager timeManager;
    private bool isResting = false;
    private RestType currentRestType;
    
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
        timeManager = TimeManager.Instance;
        
        // 初始隐藏UI
        if (restPanel != null) restPanel.SetActive(false);
        if (trainSubMenu != null) trainSubMenu.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(false);
        
        // 绑定按钮事件
        BindUIEvents();
    }
    
    void BindUIEvents()
    {
        Debug.Log("🔗 开始绑定修整系统按钮事件...");
        
        // 修整按钮
        if (sleepButton != null) sleepButton.onClick.AddListener(() => StartRest(RestType.Sleep));
        if (trainButton != null) trainButton.onClick.AddListener(OpenTrainMenu);
        if (craftButton != null) craftButton.onClick.AddListener(StartCrafting);
        if (cancelButton != null) cancelButton.onClick.AddListener(CloseRestPanel);
        
        // 训练按钮
        if (strengthTrainButton != null) strengthTrainButton.onClick.AddListener(() => StartTraining(AttributeType.Strength));
        if (accuracyTrainButton != null) accuracyTrainButton.onClick.AddListener(() => StartTraining(AttributeType.Accuracy));
        if (agilityTrainButton != null) agilityTrainButton.onClick.AddListener(() => StartTraining(AttributeType.Agility));
        if (backButton != null) backButton.onClick.AddListener(CloseTrainMenu);
        
        // 制作面板关闭按钮
        if (closeCraftingButton != null) closeCraftingButton.onClick.AddListener(CloseCraftingPanel);
        
        Debug.Log("✅ 修整系统按钮事件绑定完成");
    }
    
    // ========== 公开方法 ==========
    
    // 从背包UI调用
    public void OpenRestPanelFromInventory()
    {
        Debug.Log("🎯 从背包UI打开修整面板");
        
        if (isResting) 
        {
            Debug.LogWarning("正在修整中，无法打开面板");
            return;
        }
        
        // 检查是否在安全区域
        if (!IsInSafeArea())
        {
            Debug.LogWarning("只能在安全区域进行修整！");
            ShowMessage("只能在安全区域进行修整！");
            return;
        }
        
        if (restPanel != null)
        {
            restPanel.SetActive(true);
            UpdateRestInfo();
            Time.timeScale = 0.1f; // 轻微减速
            ShowMouseCursor();
            Debug.Log("✅ 修整面板已打开");
        }
        else
        {
            Debug.LogError("❌ 修整面板未分配！");
        }
    }
    
    // ========== 制作系统 ==========
    
    void StartCrafting()
    {
        Debug.Log("🛠️ 点击制作按钮");
        
        if (isResting) return;
        
        // 检查资源是否足够
        if (!CanAffordCrafting())
        {
            ShowMessage("资源不足，无法制作！");
            return;
        }
        
        // 开始制作修整
        StartRest(RestType.Craft);
        
        // 打开制作面板
        OpenCraftingPanel();
    }
    
    void OpenCraftingPanel()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(true);
            Debug.Log("✅ 制作面板已打开");
        }
        else
        {
            Debug.LogError("❌ 制作面板未分配！");
        }
    }
    
    void CloseCraftingPanel()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
            Debug.Log("✅ 制作面板已关闭");
            
            // 完成制作修整
            CompleteCraftingRest();
        }
    }
    
    void CompleteCraftingRest()
    {
        if (currentRestType == RestType.Craft && isResting)
        {
            // 消耗制作精力
            playerStats.CurrentEnergy -= craftEnergyCost;
            
            ShowMessage("制作完成！");
            Debug.Log("🛠️ 制作修整完成");
            
            // 关闭修整面板
            StartCoroutine(CloseAfterDelay(1f));
        }
    }
    
    bool CanAffordCrafting()
    {
        return playerStats != null && 
               playerStats.CurrentHunger >= baseHungerCost && 
               playerStats.CurrentEnergy >= craftEnergyCost;
    }
    
    // ========== 修整核心逻辑 ==========
    
    void StartRest(RestType restType)
    {
        if (isResting || playerStats == null || timeManager == null) 
        {
            Debug.LogWarning("无法开始修整");
            return;
        }
        
        // 检查饱食度是否足够
        if (playerStats.CurrentHunger < baseHungerCost)
        {
            ShowMessage("饱食度不足，无法修整！");
            return;
        }
        
        currentRestType = restType;
        isResting = true;
        
        // 消耗基础饱食度
        playerStats.CurrentHunger -= baseHungerCost;
        Debug.Log($"修整消耗饱食度: -{baseHungerCost}");
        
        // 根据修整类型执行不同逻辑
        switch (restType)
        {
            case RestType.Sleep:
                StartSleep();
                break;
                
            case RestType.Train:
                // 训练在StartTraining中处理
                break;
                
            case RestType.Craft:
                // 制作在OpenCraftingPanel中处理
                break;
        }
        
        // ========== 修改：使用您TimeManager的方法 ==========
        // 切换到下一个时间段
        timeManager.NextTimeSegment();
        
        // 更新UI
        UpdatePlayerStats();
        
        // 显示结果
        ShowRestResult();
        
        Debug.Log($"开始{restType}修整，时间推进到{GetCurrentTimeName()}");
    }
    
    void StartSleep()
    {
        if (playerStats == null || timeManager == null) return;
        
        // ========== 修改：根据时间段判断 ==========
        bool isNightTime = IsNightTime();
        float energyRecovery = isNightTime ? nightSleepEnergyRecovery : daySleepEnergyRecovery;
        int healthRecovery = sleepHealthRecovery;
        
        playerStats.CurrentEnergy += energyRecovery;
        playerStats.CurrentHealth += healthRecovery;
        
        string timeOfDay = isNightTime ? "夜晚" : "白天";
        string result = $"{timeOfDay}休息恢复了{energyRecovery}点精力和{healthRecovery}点生命值";
        ShowMessage(result);
        
        Debug.Log($"💤 睡觉修整: {result}");
        
        // 关闭修整面板
        StartCoroutine(CloseAfterDelay(2f));
    }
    
    // ========== 训练系统 ==========
    
    void OpenTrainMenu()
    {
        if (trainSubMenu != null)
        {
            trainSubMenu.SetActive(true);
            UpdateAttributeDisplays();
            Debug.Log("打开训练菜单");
        }
    }
    
    void CloseTrainMenu()
    {
        if (trainSubMenu != null)
        {
            trainSubMenu.SetActive(false);
            Debug.Log("关闭训练菜单");
        }
    }
    
    void StartTraining(AttributeType attribute)
    {
        if (playerStats == null) return;
        
        // 检查精力是否足够
        if (playerStats.CurrentEnergy < trainEnergyCost)
        {
            ShowMessage("精力不足，无法训练！");
            return;
        }
        
        // 消耗精力
        playerStats.CurrentEnergy -= trainEnergyCost;
        
        // 提升属性
        string attributeName = "";
        switch (attribute)
        {
            case AttributeType.Strength:
                playerStats.strength++;
                attributeName = "力量";
                break;
            case AttributeType.Accuracy:
                playerStats.accuracy++;
                attributeName = "精准";
                break;
            case AttributeType.Agility:
                playerStats.agility++;
                attributeName = "敏捷";
                break;
        }
        
        // 更新有效属性
        playerStats.UpdateEffectiveStats();
        
        ShowMessage($"{attributeName}训练完成！{attributeName}+1");
        Debug.Log($"💪 {attributeName}训练完成，当前{attributeName}: {GetAttributeValue(attribute)}");
        
        // 关闭训练菜单
        CloseTrainMenu();
        
        // 开始训练修整
        StartRest(RestType.Train);
    }
    
    int GetAttributeValue(AttributeType attribute)
    {
        return attribute switch
        {
            AttributeType.Strength => playerStats.strength,
            AttributeType.Accuracy => playerStats.accuracy,
            AttributeType.Agility => playerStats.agility,
            _ => 0
        };
    }
    
    void UpdateAttributeDisplays()
    {
        if (playerStats == null) return;
        
        // 更新训练按钮上的当前属性值
        if (strengthTrainButton != null)
        {
            TextMeshProUGUI text = strengthTrainButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = $"力量训练 (当前: {playerStats.strength})";
        }
        
        if (accuracyTrainButton != null)
        {
            TextMeshProUGUI text = accuracyTrainButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = $"精准训练 (当前: {playerStats.accuracy})";
        }
        
        if (agilityTrainButton != null)
        {
            TextMeshProUGUI text = agilityTrainButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = $"敏捷训练 (当前: {playerStats.agility})";
        }
    }
    
    // ========== UI更新方法 ==========
    
    void UpdateRestInfo()
    {
        if (playerStats == null || timeManager == null) return;
        
        // 时间信息
        if (timeInfoText != null)
        {
            string nextTime = GetNextTimeSegmentName();
            timeInfoText.text = $"当前时间: {GetCurrentTimeName()}\n下一时段: {nextTime}";
        }
        
        // 消耗信息
        if (costInfoText != null)
        {
            costInfoText.text = $"修整消耗:\n饱食度: -{baseHungerCost}";
            
            // 根据类型显示额外消耗
            string extraCost = "";
            if (currentRestType == RestType.Train) extraCost = $"\n精力: -{trainEnergyCost}";
            if (currentRestType == RestType.Craft) extraCost = $"\n精力: -{craftEnergyCost}";
            
            costInfoText.text += extraCost;
        }
        
        UpdatePlayerStats();
    }
    
    void UpdatePlayerStats()
    {
        if (playerStats == null) return;
        
        if (currentStrengthText != null) currentStrengthText.text = playerStats.strength.ToString();
        if (currentAccuracyText != null) currentAccuracyText.text = playerStats.accuracy.ToString();
        if (currentAgilityText != null) currentAgilityText.text = playerStats.agility.ToString();
        if (currentHealthText != null) currentHealthText.text = $"{playerStats.CurrentHealth}/{playerStats.maxHealth}";
        if (currentEnergyText != null) currentEnergyText.text = $"{playerStats.CurrentEnergy:F0}/{playerStats.maxEnergy}";
        if (currentHungerText != null) currentHungerText.text = $"{playerStats.CurrentHunger:F0}/{playerStats.maxHunger}";
    }
    
    void ShowRestResult()
    {
        if (resultInfoText != null)
        {
            string result = currentRestType switch
            {
                RestType.Sleep => "休息结束，感觉精力充沛！",
                RestType.Train => "训练完成，身体能力有所提升！",
                RestType.Craft => "制作完成，获得了新的物品！",
                _ => "修整完成"
            };
            
            resultInfoText.text = result;
        }
    }
    
    // ========== 时间相关方法（适配您的TimeManager） ==========
    
    string GetCurrentTimeName()
    {
        if (timeManager == null) return "未知时间";
        
        // 使用您TimeManager的TimeSegment枚举
        return timeManager.currentTime switch
        {
            TimeManager.TimeSegment.早上 => "早上",
            TimeManager.TimeSegment.下午 => "下午", 
            TimeManager.TimeSegment.晚上 => "晚上",
            TimeManager.TimeSegment.凌晨 => "凌晨",
            _ => "未知时间"
        };
    }
    
    string GetNextTimeSegmentName()
    {
        if (timeManager == null) return "未知时间";
        
        // 计算下一个时间段
        int nextTime = ((int)timeManager.currentTime + 1) % 4;
        TimeManager.TimeSegment nextSegment = (TimeManager.TimeSegment)nextTime;
        
        return nextSegment switch
        {
            TimeManager.TimeSegment.早上 => "早上",
            TimeManager.TimeSegment.下午 => "下午",
            TimeManager.TimeSegment.晚上 => "晚上", 
            TimeManager.TimeSegment.凌晨 => "凌晨",
            _ => "未知时间"
        };
    }
    
    bool IsNightTime()
    {
        if (timeManager == null) return false;
        
        // 根据您的时间段判断是否为夜晚
        return timeManager.currentTime == TimeManager.TimeSegment.晚上 || 
               timeManager.currentTime == TimeManager.TimeSegment.凌晨;
    }
    
    // ========== 工具方法 ==========
    
    void ShowMessage(string message)
    {
        Debug.Log(message);
        if (resultInfoText != null) resultInfoText.text = message;
    }
    
    bool IsInSafeArea()
    {
        // 这里实现安全区域检查逻辑
        // 暂时返回true用于测试
        return true;
    }
    
    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        CloseRestPanel();
        isResting = false;
    }
    
    public void CloseRestPanel()
    {
        if (restPanel != null)
        {
            restPanel.SetActive(false);
            if (trainSubMenu != null) trainSubMenu.SetActive(false);
            if (craftingPanel != null) craftingPanel.SetActive(false);
            Time.timeScale = 1f;
            HideMouseCursor();
            isResting = false;
            Debug.Log("✅ 修整面板已关闭");
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
}

// 修整类型枚举
public enum RestType
{
    Sleep, Train, Craft
}

public enum AttributeType
{
    Strength, Accuracy, Agility
}