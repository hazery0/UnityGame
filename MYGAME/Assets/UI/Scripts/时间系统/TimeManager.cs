using UnityEngine;
using System.Collections;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    public bool CanMove()
    {
        return currentMoves > 0;
    }
    
    [System.Serializable]
    public enum TimeSegment { 早上 = 0, 下午 = 1, 晚上 = 2, 凌晨 = 3 }
    
    [Header("时间设置")]
    public TimeSegment currentTime = TimeSegment.早上;
    public int maxMoves = 3;
    public int currentMoves = 3;
    public float timeSegmentDuration = 60f;
    
    [Header("UI引用")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI weightStatusText;
    
    [Header("测试数据")]
    public float currentWeight = 0f;
    public float maxWeight = 25f;
    public float currentEnergy = 100f;
    public float maxEnergy = 100f;
    
    private Coroutine timeCycleCoroutine;
    private bool isTimePaused = false;
    private PlayerStats playerStats; // 添加PlayerStats引用

    // 事件委托
    public System.Action<TimeSegment> OnTimeSegmentChanged;
    public System.Action<int, int> OnMoveCountChanged;

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
        StartTimeCycle();
        UpdateUI();
        playerStats = FindObjectOfType<PlayerStats>();
        Debug.Log("时间管理器启动成功，PlayerStats引用: " + (playerStats != null));
        
        // 初始同步
        SyncPlayerStats();
    }

    void StartTimeCycle()
    {
        if (timeCycleCoroutine != null)
            StopCoroutine(timeCycleCoroutine);
            
        timeCycleCoroutine = StartCoroutine(TimeCycle());
    }

    private IEnumerator TimeCycle()
    {
        while (true)
        {
            if (!isTimePaused)
            {
                yield return new WaitForSeconds(timeSegmentDuration);
                NextTimeSegment();
            }
            else
            {
                yield return null;
            }
        }
    }
    
    public void NextTimeSegment()
    {
        int nextTime = ((int)currentTime + 1) % 4;
        currentTime = (TimeSegment)nextTime;
        
        // 根据负重更新移动次数
        UpdateMaxMovesBasedOnWeight();
        currentMoves = maxMoves;
        
        // 同步到PlayerStats
        SyncPlayerStats();
        
        // 时间段切换时的特殊效果
        ApplyTimeSegmentEffects();
        
        // 触发事件
        OnTimeSegmentChanged?.Invoke(currentTime);
        OnMoveCountChanged?.Invoke(currentMoves, maxMoves);
        
        Debug.Log($"时间切换到：{currentTime}，移动次数重置为：{currentMoves}/{maxMoves}");
        UpdateUI();
    }
    
    public bool ConsumeMove()
    {
        if (currentMoves > 0 && !isTimePaused)
        {
            currentMoves--;
            
            // 同步到PlayerStats
            SyncPlayerStats();
            
            // 模拟消耗精力
            currentEnergy -= GetEnergyConsumptionPerMove();
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            
            Debug.Log($"消耗移动次数，剩余：{currentMoves}，精力：{currentEnergy:F0}/{maxEnergy}");
            
            // 触发事件
            OnMoveCountChanged?.Invoke(currentMoves, maxMoves);
            
            // 如果移动次数用完，自动进入下一个时间段
            if (currentMoves <= 0)
            {
                StartCoroutine(AutoNextTimeSegment());
            }
            
            UpdateUI();
            return true;
        }
        return false;
    }
    
    // 同步方法
    private void SyncPlayerStats()
    {
        if (playerStats != null)
        {
            playerStats.currentTimeSegmentMoves = currentMoves;
            playerStats.OnStatsUpdated?.Invoke();
            Debug.Log($"同步完成: TimeManager移动次数={currentMoves}, PlayerStats移动次数={playerStats.currentTimeSegmentMoves}");
        }
        else
        {
            Debug.LogWarning("PlayerStats未找到，无法同步");
        }
    }
    
    private IEnumerator AutoNextTimeSegment()
    {
        yield return new WaitForSeconds(1f);
        if (currentMoves <= 0 && !isTimePaused)
        {
            NextTimeSegment();
        }
    }
    
    private void UpdateMaxMovesBasedOnWeight()
    {
        float weightRatio = currentWeight / maxWeight;
        
        if (weightRatio < 0.4f) 
        {
            maxMoves = 4;
            if (weightStatusText != null) weightStatusText.text = "状态：浑身轻松";
        }
        else if (weightRatio < 0.6f) 
        {
            maxMoves = 3;
            if (weightStatusText != null) weightStatusText.text = "状态：略有负担";
        }
        else if (weightRatio < 0.8f) 
        {
            maxMoves = 2;
            if (weightStatusText != null) weightStatusText.text = "状态：满满当当";
        }
        else 
        {
            maxMoves = 1;
            if (weightStatusText != null) weightStatusText.text = "状态：不堪重负";
        }
    }
    
    private void ApplyTimeSegmentEffects()
    {
        switch (currentTime)
        {
            case TimeSegment.晚上:
            case TimeSegment.凌晨:
                Debug.Log("夜幕降临，视野范围减小");
                break;
            case TimeSegment.早上:
                Debug.Log("清晨到来，新的一天开始");
                break;
        }
    }
    
    private float GetEnergyConsumptionPerMove()
    {
        float weightRatio = currentWeight / maxWeight;
        float baseConsumption = 10f;
        
        if (weightRatio < 0.4f) return baseConsumption * 0.5f;
        if (weightRatio < 0.6f) return baseConsumption;
        if (weightRatio < 0.8f) return baseConsumption * 1.5f;
        return baseConsumption * 2f;
    }
    
    private void UpdateUI()
    {
        if (timeText != null)
        {
            timeText.text = $"{GetTimeSegmentChineseName(currentTime)}";
            
            switch (currentTime)
            {
                case TimeSegment.早上:
                    timeText.color = Color.yellow;
                    break;
                case TimeSegment.下午:
                    timeText.color = Color.yellow;
                    break;
                case TimeSegment.晚上:
                    timeText.color = Color.blue;
                    break;
                case TimeSegment.凌晨:
                    timeText.color = Color.gray;
                    break;
            }
        }
        
        if (movesText != null)
        {
            movesText.text = $"移动：{currentMoves}/{maxMoves}";
            
            if (currentMoves == 0) movesText.color = Color.red;
            else if (currentMoves <= maxMoves / 2) movesText.color = Color.yellow;
            else movesText.color = Color.green;
        }
    }
    
    private string GetTimeSegmentChineseName(TimeSegment segment)
    {
        switch (segment)
        {
            case TimeSegment.早上: return "早上";
            case TimeSegment.下午: return "下午";
            case TimeSegment.晚上: return "晚上";
            case TimeSegment.凌晨: return "凌晨";
            default: return "未知";
        }
    }
    
    public void ForceSleep()
    {
        Debug.Log("强制睡觉，恢复精力");
        
        SkipTimeSegments(2);
        currentEnergy = 50f;
        UpdateUI();
    }
    
    public void SkipTimeSegments(int segmentsToSkip)
    {
        StartCoroutine(SkipTimeCoroutine(segmentsToSkip));
    }

    private IEnumerator SkipTimeCoroutine(int segmentsToSkip)
    {
        isTimePaused = true;
        
        for (int i = 0; i < segmentsToSkip; i++)
        {
            yield return new WaitForSeconds(0.5f);
            NextTimeSegment();
        }
        
        isTimePaused = false;
    }
    
    public void TogglePauseTime()
    {
        isTimePaused = !isTimePaused;
        Debug.Log($"时间{(isTimePaused ? "暂停" : "恢复")}");
    }
    
    public void SetTimeScale(float duration)
    {
        timeSegmentDuration = Mathf.Max(1f, duration);
        Debug.Log($"时间流速设置为：{duration}秒/时间段");
    }
    
    public void SetWeight(float weight)
    {
        currentWeight = Mathf.Clamp(weight, 0f, maxWeight);
        UpdateMaxMovesBasedOnWeight();
        UpdateUI();
        Debug.Log($"负重设置为：{currentWeight}kg，最大移动次数：{maxMoves}");
    }
    
    public void SetEnergy(float energy)
    {
        currentEnergy = Mathf.Clamp(energy, 0f, maxEnergy);
        UpdateUI();
        Debug.Log($"精力设置为：{currentEnergy:F0}/{maxEnergy}");
    }

    // 测试方法
    public void TestNextTime()
    {
        NextTimeSegment();
    }
    
    public void TestConsumeMove()
    {
        ConsumeMove();
    }
    
    public void TestForceSleep()
    {
        ForceSleep();
    }
    
    public void TestSetWeight(float weight)
    {
        SetWeight(weight);
    }
    
    public void TestSetEnergy(float energy)
    {
        SetEnergy(energy);
    }
    
    // 公共属性访问器
    public bool IsTimePaused => isTimePaused;
    public float CurrentWeight => currentWeight;
    public float CurrentEnergy => currentEnergy;
    public int CurrentMoves => currentMoves;
    public int MaxMoves => maxMoves;
}