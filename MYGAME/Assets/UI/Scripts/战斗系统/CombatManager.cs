using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ========== 枚举定义 ==========
public enum BattleResultType
{
    Victory,    // 胜利
    Defeat,     // 失败
    Escaped,    // 逃脱
    None
}

public enum CombatActionType
{
    MeleeAttack,    // 近战攻击
    RangedAttack,   // 远程攻击
    Escape,         // 逃跑
}


// ========== 数据结构 ==========
[System.Serializable]
public struct BattleResult
{
    public BattleResultType type;
    public bool success;
    public string message;
    public int damageDealt;
    public int damageTaken;
    public int experienceGained;
    public List<Item> lootDrops;
}

[System.Serializable]
public class Battle
{
    [Header("战斗参与者")]
    public PlayerStats player;
    public List<Enemy> enemies;
    
    [Header("战斗设置")]
    public bool isRangedOnly; // 是否只能远程攻击
    public int difficultyModifier = 10;
    
    [Header("战斗状态")]
    public int currentEnemyIndex = 0;
    public bool isBattleOver = false;
    public BattleResultType battleResult = BattleResultType.None;
    public int currentTurn = 0;
    
    public Enemy CurrentEnemy
    {
        get 
        { 
            return (currentEnemyIndex < enemies.Count && currentEnemyIndex >= 0) ? 
                   enemies[currentEnemyIndex] : null;
        }
    }
    
    public Battle(PlayerStats playerStats, List<Enemy> enemyList, bool rangedOnly = false)
    {
        player = playerStats;
        enemies = enemyList;
        isRangedOnly = rangedOnly;
    }
    
    // 检查战斗是否结束
    public bool CheckBattleEnd()
    {
        if (player.CurrentHealth <= 0)
        {
            isBattleOver = true;
            battleResult = BattleResultType.Defeat;
            return true;
        }
        
        if (currentEnemyIndex >= enemies.Count)
        {
            isBattleOver = true;
            battleResult = BattleResultType.Victory;
            return true;
        }
        
        return false;
    }
    
    // 移动到下一个敌人
    public void NextEnemy()
    {
        currentEnemyIndex++;
        currentTurn = 0;
    }
    
    // 获取战斗信息
    public string GetBattleInfo()
    {
        if (CurrentEnemy == null) return "战斗结束";
        
        return $"敌人: {CurrentEnemy.enemyName} ({currentEnemyIndex + 1}/{enemies.Count})\n" +
               $"力量: {CurrentEnemy.strength} 精准: {CurrentEnemy.accuracy} 敏捷: {CurrentEnemy.agility}";
    }
}

// ========== 主战斗管理器 ==========
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    
    [Header("战斗设置")]
    public int baseDifficultyModifier = 10;
    
    [Header("战斗状态")]
    public bool isInCombat = false;
    public Battle currentBattle;
    public int currentTurn = 0;
    
    [Header("UI引用")]
    public GameObject combatUI;
    public TextMeshProUGUI combatLogText;
    public Button meleeButton;
    public Button rangedButton;
    public Button escapeButton;
    
    [Header("玩家状态显示")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI agilityText;
    
    // 事件
    public System.Action<BattleResult> OnBattleStart;
    public System.Action<BattleResult> OnBattleEnd;
    public System.Action<string> OnCombatLog;
    
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
        
        // 绑定按钮事件
        BindUIEvents();
        
        // 初始隐藏UI
        if (combatUI != null) combatUI.SetActive(false);
    }
    
    
    void BindUIEvents()
    {
        if (meleeButton != null) meleeButton.onClick.AddListener(PlayerMeleeAttack);
        if (rangedButton != null) rangedButton.onClick.AddListener(PlayerRangedAttack);
        if (escapeButton != null) escapeButton.onClick.AddListener(PlayerEscape);
    }
    
    // ========== 战斗生命周期 ==========
    
    public void StartBattle(string enemyName, int enemyStrength, int enemyAccuracy, 
                          int enemyAgility, bool isRangedOnly = false, int enemyCount = 1)
    {
        if (isInCombat) 
        {
            Debug.LogWarning("战斗正在进行中，无法开始新战斗");
            return;
        }
        
        // 创建敌人列表
        List<Enemy> enemies = new List<Enemy>();
        for (int i = 0; i < enemyCount; i++)
        {
            enemies.Add(new Enemy(enemyName, enemyStrength, enemyAccuracy, enemyAgility));
        }
        
        // 创建战斗
        currentBattle = new Battle(playerStats, enemies, isRangedOnly);
        isInCombat = true;
        currentTurn = 0;
        
        // 显示战斗UI
        ShowCombatUI();
        
        // 触发事件
        OnBattleStart?.Invoke(new BattleResult { 
            type = BattleResultType.None,
            message = $"遭遇{enemyName}！战斗开始！"
        });
        
        AddCombatLog($"<color=red>⚔️ 战斗开始！遭遇{enemyName}</color>");
        AddCombatLog($"敌人属性: 力量{enemyStrength} 精准{enemyAccuracy} 敏捷{enemyAgility}");
        
        if (isRangedOnly)
        {
            AddCombatLog("<color=yellow>提示：此敌人只能远程攻击！</color>");
        }
        
        UpdateUI();
        Debug.Log($"战斗开始：玩家 vs {enemyCount}个{enemyName}");
    }
    
    public void EndBattle(BattleResultType resultType)
    {
        if (!isInCombat || currentBattle == null) return;
        
        BattleResult result = new BattleResult
        {
            type = resultType,
            message = GetBattleResultMessage(resultType)
        };
        
        // 处理战斗结果
        ProcessBattleResult(result);
        
        // 触发事件
        OnBattleEnd?.Invoke(result);
        
        isInCombat = false;
        currentBattle = null;
        currentTurn = 0;
        
        AddCombatLog($"<color=orange>战斗结束：{result.message}</color>");
        
        // 延迟关闭UI
        StartCoroutine(HideUIAfterDelay(2f));
    }
    
    IEnumerator HideUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideCombatUI();
    }
    
    // ========== 玩家行动 ==========
    
    public void PlayerMeleeAttack()
    {
        if (!CanPerformAction()) return;
        
        var result = ExecuteMeleeAttack();
        ProcessActionResult(result);
    }
    
    public void PlayerRangedAttack()
    {
        if (!CanPerformAction()) return;
        
        var result = ExecuteRangedAttack();
        ProcessActionResult(result);
    }
    
    public void PlayerEscape()
    {
        if (!CanPerformAction()) return;
        
        var result = ExecuteEscapeAttempt();
        ProcessActionResult(result);
    }
    
    bool CanPerformAction()
    {
        if (!isInCombat || currentBattle == null)
        {
            AddCombatLog("<color=red>战斗未开始或已结束</color>");
            return false;
        }
        
        if (currentBattle.CheckBattleEnd())
        {
            EndBattle(currentBattle.battleResult);
            return false;
        }
        
        return true;
    }
    
    // ========== 核心战斗逻辑 ==========
    
    BattleResult ExecuteMeleeAttack()
    {
        if (currentBattle.isRangedOnly)
        {
            return CreateInvalidResult("此敌人只能远程攻击！");
        }
        
        int playerDice = RollDice(3, 6);
        int playerTotal = playerDice + playerStats.GetEffectiveStrength();
        int enemyTotal = currentBattle.CurrentEnemy.strength + baseDifficultyModifier;
        
        var result = new BattleResult();
        currentTurn++;
        
        if (playerTotal > enemyTotal)
        {
            // 攻击成功
            int damage = CalculateDamage(playerStats.GetEffectiveStrength(), 0);
            result.success = true;
            result.damageDealt = damage;
            result.message = $"💥 近战攻击成功！投掷{playerDice}+力量{playerStats.GetEffectiveStrength()}={playerTotal} > 敌人{enemyTotal}，造成{damage}点伤害";
            
            DefeatCurrentEnemy();
        }
        else
        {
            // 攻击失败
            int damage = CalculateDamage(currentBattle.CurrentEnemy.strength, playerStats.defense);
            result.success = false;
            result.damageTaken = damage;
            result.message = $"❌ 近战攻击失败！投掷{playerDice}+力量{playerStats.GetEffectiveStrength()}={playerTotal} <= 敌人{enemyTotal}，受到{damage}点伤害";
            
            playerStats.TakeDamage(damage);
            CheckPlayerDefeat();
        }
        
        return result;
    }
    
    BattleResult ExecuteRangedAttack()
    {
        int playerDice = RollDice(3, 6);
        int playerTotal = playerDice + playerStats.GetEffectiveAccuracy();
        int enemyTotal = currentBattle.CurrentEnemy.accuracy + baseDifficultyModifier;
        
        var result = new BattleResult();
        currentTurn++;
        
        if (playerTotal > enemyTotal)
        {
            // 攻击成功
            int damage = CalculateDamage(playerStats.GetEffectiveAccuracy(), 0);
            result.success = true;
            result.damageDealt = damage;
            result.message = $"🎯 远程攻击成功！投掷{playerDice}+精准{playerStats.GetEffectiveAccuracy()}={playerTotal} > 敌人{enemyTotal}，造成{damage}点伤害";
            
            DefeatCurrentEnemy();
        }
        else
        {
            // 攻击失败
            int damage = CalculateDamage(currentBattle.CurrentEnemy.accuracy, playerStats.defense);
            result.success = false;
            result.damageTaken = damage;
            result.message = $"❌ 远程攻击失败！投掷{playerDice}+精准{playerStats.GetEffectiveAccuracy()}={playerTotal} <= 敌人{enemyTotal}，受到{damage}点伤害";
            
            playerStats.TakeDamage(damage);
            CheckPlayerDefeat();
        }
        
        return result;
    }
    
    BattleResult ExecuteEscapeAttempt()
    {
        int escapeDice = RollDice(1, 6);
        int escapeTotal = escapeDice + playerStats.GetEffectiveAgility();
        int enemyEscapeTotal = currentBattle.CurrentEnemy.agility + baseDifficultyModifier;
        
        var result = new BattleResult();
        currentTurn++;
        
        if (escapeTotal > enemyEscapeTotal)
        {
            // 逃跑成功
            result.success = true;
            result.type = BattleResultType.Escaped;
            result.message = $"🏃 逃跑成功！投掷{escapeDice}+敏捷{playerStats.GetEffectiveAgility()}={escapeTotal} > 敌人{enemyEscapeTotal}";
            
            EndBattle(BattleResultType.Escaped);
        }
        else
        {
            // 逃跑失败
            int enemyPower = Mathf.Max(currentBattle.CurrentEnemy.strength, currentBattle.CurrentEnemy.accuracy);
            int damage = CalculateDamage(enemyPower, playerStats.defense);
            
            result.success = false;
            result.damageTaken = damage;
            result.message = $"❌ 逃跑失败！投掷{escapeDice}+敏捷{playerStats.GetEffectiveAgility()}={escapeTotal} <= 敌人{enemyEscapeTotal}，受到{damage}点伤害";
            
            playerStats.TakeDamage(damage);
            CheckPlayerDefeat();
        }
        
        return result;
    }
    
    // ========== 战斗逻辑辅助方法 ==========
    
    void DefeatCurrentEnemy()
    {
        currentBattle.NextEnemy();
        
        if (currentBattle.CheckBattleEnd())
        {
            EndBattle(currentBattle.battleResult);
        }
    }
    
    void CheckPlayerDefeat()
    {
        if (playerStats.CurrentHealth <= 0)
        {
            EndBattle(BattleResultType.Defeat);
        }
    }
    
    void ProcessActionResult(BattleResult result)
    {
        AddCombatLog(result.message);
        UpdateUI();
        
        // 检查战斗是否结束
        if (currentBattle != null && currentBattle.CheckBattleEnd())
        {
            StartCoroutine(EndBattleWithDelay(currentBattle.battleResult, 1.5f));
        }
    }
    
    void ProcessBattleResult(BattleResult result)
    {
        // 处理战利品、经验等
        if (result.type == BattleResultType.Victory)
        {
            // 给予奖励
            AddCombatLog("获得战斗奖励！");
        }
    }
    
    IEnumerator EndBattleWithDelay(BattleResultType resultType, float delay)
    {
        yield return new WaitForSeconds(delay);
        EndBattle(resultType);
    }
    
    // ========== 工具函数 ==========
    
    public int RollDice(int diceCount, int diceSides = 6)
    {
        int total = 0;
        for (int i = 0; i < diceCount; i++)
        {
            total += Random.Range(1, diceSides + 1);
        }
        return total;
    }
    
    public int CalculateDamage(int attackerPower, int defenderDefense)
    {
        return Mathf.Max(1, (int)((attackerPower - defenderDefense) * 0.5f));
    }
    
    BattleResult CreateInvalidResult(string message)
    {
        return new BattleResult 
        { 
            success = false, 
            message = message,
            type = BattleResultType.None
        };
    }
    
    string GetBattleResultMessage(BattleResultType resultType)
    {
        return resultType switch
        {
            BattleResultType.Victory => "战斗胜利！",
            BattleResultType.Defeat => "战斗失败！",
            BattleResultType.Escaped => "成功逃脱！",
            _ => "战斗结束"
        };
    }
    
    // ========== UI控制 ==========
    
    void ShowCombatUI()
    {
        if (combatUI != null)
        {
            combatUI.SetActive(true);
            UpdateUI();
        }
    }
    
    void HideCombatUI()
    {
        if (combatUI != null)
        {
            combatUI.SetActive(false);
        }
    }
    
    void UpdateUI()
    {
        UpdatePlayerStatus();
        UpdateActionButtons();
    }
    
    void UpdatePlayerStatus()
    {
        if (playerStats == null) return;
        
        if (healthText != null)
            healthText.text = $"生命: {playerStats.CurrentHealth}/{playerStats.maxHealth}";
        if (energyText != null)
            energyText.text = $"精力: {playerStats.CurrentEnergy:F0}/{playerStats.maxEnergy}";
        if (hungerText != null)
            hungerText.text = $"饱食: {playerStats.CurrentHunger:F0}/{playerStats.maxHunger}";
        if (strengthText != null)
            strengthText.text = $"力量: {playerStats.GetEffectiveStrength()}";
        if (accuracyText != null)
            accuracyText.text = $"精准: {playerStats.GetEffectiveAccuracy()}";
        if (agilityText != null)
            agilityText.text = $"敏捷: {playerStats.GetEffectiveAgility()}";
    }
    
    void UpdateActionButtons()
    {
        if (currentBattle == null) return;
        
        bool canMelee = !currentBattle.isRangedOnly;
        bool canAct = isInCombat && !currentBattle.CheckBattleEnd();
        
        if (meleeButton != null) 
        {
            meleeButton.interactable = canMelee && canAct;
            meleeButton.GetComponentInChildren<TextMeshProUGUI>().color = 
                canMelee ? Color.white : Color.gray;
        }
        if (rangedButton != null) 
            rangedButton.interactable = canAct;
        if (escapeButton != null) 
            escapeButton.interactable = canAct;
    }
    
    void AddCombatLog(string message)
    {
        Debug.Log($"[战斗] {message}");
        
        if (combatLogText != null)
        {
            combatLogText.text = message + "\n" + combatLogText.text;
            
            // 限制日志行数
            string[] lines = combatLogText.text.Split('\n');
            if (lines.Length > 20)
            {
                combatLogText.text = string.Join("\n", lines, 0, 15);
            }
        }
        
        OnCombatLog?.Invoke(message);
    }
    
    // ========== 公共接口 ==========
    
    public void ForceEndBattle(BattleResultType resultType)
    {
        if (isInCombat) EndBattle(resultType);
    }
    
    // 测试方法
    public void TestBattle()
    {
        StartBattle("强盗哨兵", 12, 14, 10, false, 1);
    }
    
    void Update()
    {
        // 测试快捷键
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestBattle();
        }
        
        // 战斗时按ESC强制结束
        if (isInCombat && Input.GetKeyDown(KeyCode.Escape))
        {
            ForceEndBattle(BattleResultType.Escaped);
        }
    }
}