using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    
    // ========== 基础属性 ==========
    [Header("生存属性")]
    public int maxHealth = 100;
    public float maxHunger = 100f;
    public float maxEnergy = 100f;
    public float maxRadiation = 100f;
    
    [Header("战斗属性")]
    public int strength = 10;      // 改回 strength
    public int accuracy = 10;      // 改回 accuracy
    public int agility = 10;       // 改回 agility
    public int defense = 10;       // 改回 defense
    
    [Header("负重系统")]
    public float maxWeight = 25f;
    
    [Header("时间系统")]
    public int maxTimeSegmentMoves = 3;
    
    [Header("技能系统")]
    public PlayerSkill selectedSkill = PlayerSkill.None;
    
    // ========== 当前状态 ==========
    [SerializeField] private int _currentHealth = 100;
    [SerializeField] private float _currentHunger = 100f;
    [SerializeField] private float _currentEnergy = 100f;
    [SerializeField] private float _currentRadiation = 0f;
    [SerializeField] private float _currentWeight = 0f;
    [SerializeField] private int _currentTimeSegmentMoves = 3;
    
    // ========== 状态效果 ==========
    public List<StatusEffect> activeEffects = new List<StatusEffect>();
    
    // ========== 事件委托 ==========
    public System.Action OnStatsUpdated;
    public System.Action OnStatusEffectsChanged;
    public System.Action OnAttributeChanged;
    
    // ========== 属性访问器 ==========
    public int CurrentHealth
    {
        get => _currentHealth;
        set 
        { 
            _currentHealth = Mathf.Clamp(value, 0, GetEffectiveMaxHealth());
            OnStatsUpdated?.Invoke();
        }
    }
    
    public float CurrentHunger
    {
        get => _currentHunger;
        set 
        { 
            _currentHunger = Mathf.Clamp(value, 0f, maxHunger);
            OnStatsUpdated?.Invoke();
            UpdateHungerStatus();
        }
    }
    
    public float CurrentEnergy
    {
        get => _currentEnergy;
        set 
        { 
            _currentEnergy = Mathf.Clamp(value, 0f, maxEnergy);
            OnStatsUpdated?.Invoke();
            UpdateEnergyStatus();
            if (_currentEnergy <= 0) ForceSleep();
        }
    }
    
    public float CurrentRadiation
    {
        get => _currentRadiation;
        set 
        { 
            _currentRadiation = Mathf.Clamp(value, 0f, maxRadiation);
            OnStatsUpdated?.Invoke();
            UpdateRadiationStatus();
        }
    }
    
    public float currentWeight
    {
        get => _currentWeight;
        set 
        { 
            _currentWeight = Mathf.Max(0f, value);
            OnStatsUpdated?.Invoke();
        }
    }
    
    public int currentTimeSegmentMoves
    {
        get => _currentTimeSegmentMoves;
        set 
        { 
            _currentTimeSegmentMoves = Mathf.Clamp(value, 0, GetMaxMovesPerSegment());
            OnStatsUpdated?.Invoke();
        }
    }
    
    // ========== 初始化 ==========
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
        InitializeStats();
    }
    
    void InitializeStats()
    {
        _currentHealth = maxHealth;
        _currentHunger = maxHunger;
        _currentEnergy = maxEnergy;
        _currentRadiation = 0f;
        _currentTimeSegmentMoves = GetMaxMovesPerSegment();
        
        UpdateAllStatusEffects();
        OnStatsUpdated?.Invoke();
    }
    
    void Update()
    {
        UpdateStatusEffectsDuration();
    }
    
    // ========== 消耗相关方法 ==========
    public void ConsumeMovement()
    {
        if (currentTimeSegmentMoves > 0)
        {
            currentTimeSegmentMoves--;
            
            // 根据策划案：移动消耗5饱食度
            CurrentHunger -= 5f;
            
            // 根据负重计算精力消耗
            float energyCost = GetEnergyConsumptionPerMove();
            CurrentEnergy -= energyCost;
            
            Debug.Log($"移动消耗: 饱食度-5, 精力-{energyCost:F1}");
        }
    }
    
    public void ConsumeEvent()
    {
        // 根据策划案：触发事件消耗5饱食度 + 10精力
        CurrentHunger -= 5f;
        CurrentEnergy -= 10f;
        Debug.Log("事件消耗: 饱食度-5, 精力-10");
    }
    
    public void ConsumeCombat()
    {
        // 根据策划案：战斗消耗5饱食度
        CurrentHunger -= 5f;
        CurrentEnergy -= 15f;
        Debug.Log("战斗消耗: 饱食度-5, 精力-15");
    }
    
    // ========== 状态效果系统 ==========
    void UpdateAllStatusEffects()
    {
        UpdateHungerStatus();
        UpdateEnergyStatus();
        UpdateRadiationStatus();
        OnStatusEffectsChanged?.Invoke();
    }
    
    void UpdateHungerStatus()
    {
        activeEffects.RemoveAll(e => e.type == StatusEffectType.HungerBonus || 
                                   e.type == StatusEffectType.FullPenalty ||
                                   e.type == StatusEffectType.Diarrhea);
        
        if (_currentHunger < 30f) // 食不果腹：全属性+1
        {
            AddStatusEffect(new StatusEffect {
                type = StatusEffectType.HungerBonus,
                strengthBonus = 1,
                accuracyBonus = 1,
                agilityBonus = 1,
                description = "食不果腹：全属性+1"
            });
        }
        else if (_currentHunger > 70f) // 吃得饱饱：全属性-1
        {
            AddStatusEffect(new StatusEffect {
                type = StatusEffectType.FullPenalty,
                strengthBonus = -1,
                accuracyBonus = -1,
                agilityBonus = -1,
                description = "吃得饱饱：全属性-1"
            });
        }
    }
    
    void UpdateEnergyStatus()
    {
        activeEffects.RemoveAll(e => e.type == StatusEffectType.EnergyBonus || 
                                   e.type == StatusEffectType.TiredPenalty);
        
        if (_currentEnergy > 70f) // 精力充沛：全属性+1
        {
            AddStatusEffect(new StatusEffect {
                type = StatusEffectType.EnergyBonus,
                strengthBonus = 1,
                accuracyBonus = 1,
                agilityBonus = 1,
                description = "精力充沛：全属性+1"
            });
        }
        else if (_currentEnergy < 30f) // 疲惫不堪：全属性-1
        {
            AddStatusEffect(new StatusEffect {
                type = StatusEffectType.TiredPenalty,
                strengthBonus = -1,
                accuracyBonus = -1,
                agilityBonus = -1,
                description = "疲惫不堪：全属性-1"
            });
        }
    }
    
    void UpdateRadiationStatus()
    {
        activeEffects.RemoveAll(e => e.type.ToString().Contains("Radiation"));
        
        if (_currentRadiation > 80f) // 异变：血上限-60，全属性+3
        {
            AddStatusEffect(new StatusEffect {
                type = StatusEffectType.RadiationMutation,
                maxHealthBonus = -60,
                strengthBonus = 3,
                accuracyBonus = 3,
                agilityBonus = 3,
                description = "异变：血量上限-60，全属性+3"
            });
        }
        else if (_currentRadiation > 60f) // 辐射病：血上限-30，全属性+1
        {
            AddStatusEffect(new StatusEffect {
                type = StatusEffectType.RadiationSickness,
                maxHealthBonus = -30,
                strengthBonus = 1,
                accuracyBonus = 1,
                agilityBonus = 1,
                description = "辐射病：血量上限-30，全属性+1"
            });
        }
        else if (_currentRadiation > 40f) // 身体不适：血上限-20
        {
            AddStatusEffect(new StatusEffect {
                type = StatusEffectType.RadiationDiscomfort,
                maxHealthBonus = -20,
                description = "身体不适：血量上限-20"
            });
        }
    }
    
    void UpdateStatusEffectsDuration()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeEffects[i];
            effect.duration -= Time.deltaTime;
            effect.remainingTime = effect.duration;
            activeEffects[i] = effect;
            
            if (effect.duration <= 0)
            {
                activeEffects.RemoveAt(i);
                OnStatusEffectsChanged?.Invoke();
            }
        }
    }
    
    void AddStatusEffect(StatusEffect effect)
    {
        // 检查是否已存在相同类型的效果
        var existingEffect = activeEffects.Find(e => e.type == effect.type);
        if (existingEffect.type != StatusEffectType.None)
        {
            // 刷新持续时间
            existingEffect.duration = effect.duration;
            existingEffect.remainingTime = effect.duration;
        }
        else
        {
            activeEffects.Add(effect);
        }
        OnStatusEffectsChanged?.Invoke();
    }
    
    // ========== 恢复旧的方法 ==========
    public void UpdateEffectiveStats()
    {
        // 手动更新所有状态效果
        UpdateAllStatusEffects();
        
        // 触发更新事件
        OnStatsUpdated?.Invoke();
        OnStatusEffectsChanged?.Invoke();
        OnAttributeChanged?.Invoke();
        
        Debug.Log("属性已更新");
    }
    
    public void ApplyStatusEffect(StatusEffectType effectType, float duration = 3600f)
    {
        StatusEffect effect = new StatusEffect();
        
        switch (effectType)
        {
            case StatusEffectType.Infection:
                effect = new StatusEffect {
                    type = StatusEffectType.Infection,
                    maxHealthBonus = -15,
                    description = "感染：血量上限-15",
                    duration = duration,
                    remainingTime = duration
                };
                break;
                
            case StatusEffectType.Fracture:
                effect = new StatusEffect {
                    type = StatusEffectType.Fracture,
                    agilityBonus = -2,
                    description = "骨折：敏捷-2",
                    duration = duration,
                    remainingTime = duration
                };
                break;
                
            case StatusEffectType.Bleeding:
                effect = new StatusEffect {
                    type = StatusEffectType.Bleeding,
                    maxHealthBonus = -10,
                    description = "流血：血量上限-10",
                    duration = duration,
                    remainingTime = duration
                };
                break;
                
            case StatusEffectType.DeepWound:
                effect = new StatusEffect {
                    type = StatusEffectType.DeepWound,
                    maxHealthBonus = -30,
                    description = "深度裂伤：血量上限-30",
                    duration = duration,
                    remainingTime = duration
                };
                break;
                
            case StatusEffectType.Diarrhea:
                effect = new StatusEffect {
                    type = StatusEffectType.Diarrhea,
                    maxHungerBonus = -30,
                    description = "腹泻：饱食度上限-30",
                    duration = duration,
                    remainingTime = duration
                };
                break;
                
            default:
                Debug.LogWarning($"未知的状态效果类型: {effectType}");
                return;
        }
        
        // 添加效果
        activeEffects.Add(effect);
        OnStatusEffectsChanged?.Invoke();
        UpdateEffectiveStats();
        
        Debug.Log($"施加状态效果: {effect.description}");
    }
    
    public void ApplyTemporaryBonus(int strBonus, int accBonus, int agiBonus, float duration = 600f)
    {
        StatusEffect tempEffect = new StatusEffect
        {
            type = StatusEffectType.EnergyBonus,
            strengthBonus = strBonus,
            accuracyBonus = accBonus,
            agilityBonus = agiBonus,
            duration = duration,
            remainingTime = duration,
            description = $"临时加成：力量+{strBonus} 精准+{accBonus} 敏捷+{agiBonus}"
        };
        
        activeEffects.Add(tempEffect);
        OnStatusEffectsChanged?.Invoke();
        UpdateEffectiveStats();
        
        Debug.Log($"应用临时加成: 力量+{strBonus} 精准+{accBonus} 敏捷+{agiBonus} 持续{duration}秒");
    }
    
    // ========== 属性计算方法 ==========
    public int GetEffectiveStrength()
    {
        int effective = strength;
        foreach (var effect in activeEffects)
            effective += effect.strengthBonus;
        return Mathf.Max(1, effective);
    }
    
    public int GetEffectiveAccuracy()
    {
        int effective = accuracy;
        foreach (var effect in activeEffects)
            effective += effect.accuracyBonus;
        return Mathf.Max(1, effective);
    }
    
    public int GetEffectiveAgility()
    {
        int effective = agility;
        foreach (var effect in activeEffects)
            effective += effect.agilityBonus;
        return Mathf.Max(1, effective);
    }
    
    public int GetEffectiveDefense()
    {
        int effective = defense;
        foreach (var effect in activeEffects)
            effective += effect.defenseBonus;
        return Mathf.Max(1, effective);
    }
    
    public int GetEffectiveMaxHealth()
    {
        int effective = maxHealth;
        foreach (var effect in activeEffects)
            effective += effect.maxHealthBonus;
        return Mathf.Max(1, effective);
    }
    
    public float GetEffectiveMaxHunger()
    {
        float effective = maxHunger;
        foreach (var effect in activeEffects)
            effective += effect.maxHungerBonus;
        return Mathf.Max(10f, effective);
    }
    
    public int GetMaxMovesPerSegment()
    {
        float weightRatio = currentWeight / maxWeight;
        if (weightRatio < 0.4f) return 4;
        if (weightRatio < 0.6f) return 3;
        if (weightRatio < 0.8f) return 2;
        return 1;
    }
    
    float GetEnergyConsumptionPerMove()
    {
        float weightRatio = currentWeight / maxWeight;
        float baseCost = 10f;
        
        if (weightRatio < 0.4f) return baseCost * 0.5f;
        if (weightRatio < 0.6f) return baseCost;
        if (weightRatio < 0.8f) return baseCost * 1.5f;
        return baseCost * 2f;
    }
    
    // ========== 强制睡觉系统 ==========
    void ForceSleep()
    {
        Debug.Log("精力耗尽，强制睡觉恢复");
        
        TimeManager timeManager = FindObjectOfType<TimeManager>();
        if (timeManager != null)
        {
            timeManager.ForceSleep();
        }
        else
        {
            Debug.LogWarning("TimeManager未找到，直接恢复精力");
            CurrentEnergy = 50f;
        }
    }
    
    // ========== 公共接口 ==========
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        Debug.Log($"受到{damage}点伤害，当前生命值：{CurrentHealth}");
    }
    
    public void Heal(int amount)
    {
        CurrentHealth += amount;
        Debug.Log($"恢复{amount}点生命值，当前生命值：{CurrentHealth}");
    }
    
    public void IncreaseRadiation(float amount)
    {
        CurrentRadiation += amount;
        Debug.Log($"辐射值增加{amount}，当前：{CurrentRadiation}");
    }
    
    public void ReduceRadiation(float amount)
    {
        CurrentRadiation = Mathf.Max(0, CurrentRadiation - amount);
        Debug.Log($"辐射值减少{amount}，当前：{CurrentRadiation}");
    }
    
    // ========== 状态检查方法 ==========
    public bool HasInjury()
    {
        return activeEffects.Exists(e => 
            e.type == StatusEffectType.Infection ||
            e.type == StatusEffectType.Fracture ||
            e.type == StatusEffectType.Bleeding ||
            e.type == StatusEffectType.DeepWound ||
            e.type == StatusEffectType.Diarrhea);
    }
    
    public bool HasDiarrhea()
    {
        return activeEffects.Exists(e => e.type == StatusEffectType.Diarrhea);
    }
    
    public string GetInjuryReport()
    {
        string report = "";
        foreach (var effect in activeEffects)
        {
            if (effect.type == StatusEffectType.Infection ||
                effect.type == StatusEffectType.Fracture ||
                effect.type == StatusEffectType.Bleeding ||
                effect.type == StatusEffectType.DeepWound ||
                effect.type == StatusEffectType.Diarrhea)
            {
                report += $"{effect.description}（剩余：{effect.remainingTime / 60:F0}分钟）\n";
            }
        }
        return report.TrimEnd('\n');
    }
    // ========== 在 PlayerStats 类中添加这些方法 ==========

public void CureStatusEffect(StatusEffectType effectType)
{
    switch (effectType)
    {
        case StatusEffectType.Infection:
            CureInfection();
            break;
        case StatusEffectType.Fracture:
            CureFracture();
            break;
        case StatusEffectType.Bleeding:
            CureBleeding();
            break;
        case StatusEffectType.DeepWound:
            CureDeepWound();
            break;
        case StatusEffectType.Diarrhea:
            CureDiarrhea();
            break;
        default:
            // 对于辐射状态等其他效果，使用通用的移除方法
            RemoveStatusEffect(effectType);
            break;
    }
    
    Debug.Log($"治愈了: {GetEffectName(effectType)}");
}

public void RemoveStatusEffect(StatusEffectType effectType)
{
    var effect = activeEffects.Find(e => e.type == effectType);
    if (effect.type != StatusEffectType.None)
    {
        activeEffects.Remove(effect);
        OnStatusEffectsChanged?.Invoke();
        OnStatsUpdated?.Invoke();
    }
}

// 确保这些治愈方法存在
public void CureInfection()
{
    RemoveStatusEffect(StatusEffectType.Infection);
    Debug.Log("感染已治愈");
}

public void CureFracture()
{
    RemoveStatusEffect(StatusEffectType.Fracture);
    Debug.Log("骨折已治愈");
}

public void CureBleeding()
{
    RemoveStatusEffect(StatusEffectType.Bleeding);
    Debug.Log("流血已治愈");
}

public void CureDeepWound()
{
    RemoveStatusEffect(StatusEffectType.DeepWound);
    Debug.Log("深度裂伤已治愈");
}

public void CureDiarrhea()
{
    RemoveStatusEffect(StatusEffectType.Diarrhea);
    Debug.Log("腹泻已治愈");
}
    // ========== 工具方法 ==========
    string GetEffectName(StatusEffectType effectType)
    {
        return effectType switch
        {
            StatusEffectType.Infection => "感染",
            StatusEffectType.Fracture => "骨折",
            StatusEffectType.Bleeding => "流血",
            StatusEffectType.DeepWound => "深度裂伤",
            StatusEffectType.Diarrhea => "腹泻",
            StatusEffectType.RadiationDiscomfort => "身体不适",
            StatusEffectType.RadiationSickness => "辐射病",
            StatusEffectType.RadiationMutation => "异变",
            StatusEffectType.EnergyBonus => "精力充沛",
            StatusEffectType.TiredPenalty => "疲惫不堪",
            StatusEffectType.HungerBonus => "食不果腹",
            StatusEffectType.FullPenalty => "吃得饱饱",
            _ => effectType.ToString()
        };
    }
}

// ========== 枚举定义 ==========
public enum PlayerSkill
{
    None, CombatExpert, Sniper, ParkourExpert, Navigator, Tank, MarchAnt, Medic
}

public enum StatusEffectType
{
    None, 
    // 精力状态
    EnergyBonus,        // 精力充沛：全属性+1
    TiredPenalty,       // 疲惫不堪：全属性-1
    
    // 饱食度状态
    HungerBonus,        // 食不果腹：全属性+1
    FullPenalty,        // 吃得饱饱：全属性-1
    Diarrhea,           // 腹泻：饱食度上限-30
    
    // 辐射状态
    RadiationDiscomfort, // 身体不适：血上限-20
    RadiationSickness,   // 辐射病：血上限-30，全属性+1
    RadiationMutation,   // 异变：血上限-60，全属性+3
    
    // 伤害状态
    Infection,          // 感染：血上限-15
    Fracture,          // 骨折：敏捷-2
    Bleeding,          // 流血：血上限-10
    DeepWound          // 深度裂伤：血上限-30
}

[System.Serializable]
public struct StatusEffect
{
    public StatusEffectType type;
    public string description;
    public int strengthBonus;
    public int accuracyBonus;
    public int agilityBonus;
    public int defenseBonus;
    public int maxHealthBonus;
    public int maxHungerBonus; // 饱食度上限加成
    public float duration;
    public float remainingTime;
}