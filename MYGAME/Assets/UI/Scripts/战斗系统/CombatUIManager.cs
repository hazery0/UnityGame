using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUIManager : MonoBehaviour
{
    public static CombatUIManager Instance;
    
    [Header("UI组件")]
    public GameObject combatCanvas;
    public GameObject combatPanel;
    
    [Header("信息显示")]
    public TextMeshProUGUI enemyInfoText;
    public TextMeshProUGUI turnCounterText;
    
    [Header("战斗日志")]
    public TextMeshProUGUI combatLogText;
    public ScrollRect logScrollRect;
    
    [Header("行动按钮")]
    public Button meleeAttackButton;
    public Button rangedAttackButton;
    public Button escapeButton;
    public Button itemButton;
    
    [Header("玩家状态")]
    public Slider healthSlider;
    public Slider energySlider;
    public Slider hungerSlider;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI hungerText;
    
    private CombatManager combatManager;
    private PlayerStats playerStats;
    
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
        combatManager = CombatManager.Instance;
        playerStats = FindObjectOfType<PlayerStats>();
        
        // 绑定事件
        if (combatManager != null)
        {
            combatManager.OnCombatLog += AddCombatLog;
            combatManager.OnBattleStart += OnBattleStart;
            combatManager.OnBattleEnd += OnBattleEnd;
        }
        
        // 绑定按钮
        BindUIEvents();
        
        // 初始隐藏
        HideCombatUI();
    }
    
    void Update()
    {
        if (combatPanel.activeInHierarchy)
        {
            UpdatePlayerStatus();
        }
    }
    
    void BindUIEvents()
    {
        if (meleeAttackButton != null)
            meleeAttackButton.onClick.AddListener(() => combatManager.PlayerMeleeAttack());
        if (rangedAttackButton != null)
            rangedAttackButton.onClick.AddListener(() => combatManager.PlayerRangedAttack());
        if (escapeButton != null)
            escapeButton.onClick.AddListener(() => combatManager.PlayerEscape());
        if (itemButton != null)
            itemButton.onClick.AddListener(OnItemButtonClick);
    }
    
    // ========== UI控制 ==========
    
    public void ShowCombatUI()
    {
        if (combatCanvas != null) combatCanvas.SetActive(true);
        if (combatPanel != null) combatPanel.SetActive(true);
        
        UpdateUI();
    }
    
    public void HideCombatUI()
    {
        if (combatCanvas != null) combatCanvas.SetActive(false);
        if (combatPanel != null) combatPanel.SetActive(false);
    }
    
    void UpdateUI()
    {
        UpdatePlayerStatus();
        UpdateEnemyInfo();
        UpdateActionButtons();
    }
    
    void UpdatePlayerStatus()
    {
        if (playerStats == null) return;
        
        // 更新滑块和文本
        if (healthSlider != null)
        {
            healthSlider.maxValue = playerStats.maxHealth;
            healthSlider.value = playerStats.CurrentHealth;
        }
        if (healthText != null)
            healthText.text = $"{playerStats.CurrentHealth}/{playerStats.maxHealth}";
            
        if (energySlider != null)
        {
            energySlider.maxValue = playerStats.maxEnergy;
            energySlider.value = playerStats.CurrentEnergy;
        }
        if (energyText != null)
            energyText.text = $"{playerStats.CurrentEnergy:F0}/{playerStats.maxEnergy}";
            
        if (hungerSlider != null)
        {
            hungerSlider.maxValue = playerStats.maxHunger;
            hungerSlider.value = playerStats.CurrentHunger;
        }
        if (hungerText != null)
            hungerText.text = $"{playerStats.CurrentHunger:F0}/{playerStats.maxHunger}";
    }
    
    void UpdateEnemyInfo()
    {
        if (combatManager?.currentBattle?.CurrentEnemy == null) return;
        
        Enemy enemy = combatManager.currentBattle.CurrentEnemy;
        if (enemyInfoText != null)
        {
            enemyInfoText.text = $"{enemy.enemyName}\n" +
                               $"力量: {enemy.strength} 精准: {enemy.accuracy} 敏捷: {enemy.agility}";
        }
        
        if (turnCounterText != null)
        {
            turnCounterText.text = $"回合: {combatManager.currentTurn}";
        }
    }
    
    void UpdateActionButtons()
    {
        if (combatManager?.currentBattle == null) return;
        
        bool canMelee = !combatManager.currentBattle.isRangedOnly;
        bool canAct = combatManager.isInCombat;
        
        if (meleeAttackButton != null)
        {
            meleeAttackButton.interactable = canMelee && canAct;
            meleeAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = 
                canMelee ? Color.white : Color.gray;
        }
        
        if (rangedAttackButton != null) rangedAttackButton.interactable = canAct;
        if (escapeButton != null) escapeButton.interactable = canAct;
        if (itemButton != null) itemButton.interactable = canAct;
    }
    
    // ========== 事件处理 ==========
    
    void OnBattleStart(BattleResult result)
    {
        ShowCombatUI();
        AddCombatLog(result.message);
    }
    
    void OnBattleEnd(BattleResult result)
    {
        AddCombatLog($"<color=orange>战斗结束: {result.message}</color>");
        
        // 延迟关闭UI
        StartCoroutine(HideUIAfterDelay(2f));
    }
    
    System.Collections.IEnumerator HideUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideCombatUI();
    }
    
    void OnItemButtonClick()
    {
        // 打开物品选择
        AddCombatLog("打开物品栏...");
    }
    
    // ========== 战斗日志 ==========
    
    public void AddCombatLog(string message)
    {
        if (combatLogText != null)
        {
            string timestamp = $"[{System.DateTime.Now:HH:mm:ss}] ";
            combatLogText.text = timestamp + message + "\n" + combatLogText.text;
            
            // 限制行数
            string[] lines = combatLogText.text.Split('\n');
            if (lines.Length > 20)
            {
                combatLogText.text = string.Join("\n", lines, 0, 15);
            }
            
            // 自动滚动
            if (logScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                logScrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
    
    void OnDestroy()
    {
        // 取消事件注册
        if (combatManager != null)
        {
            combatManager.OnCombatLog -= AddCombatLog;
            combatManager.OnBattleStart -= OnBattleStart;
            combatManager.OnBattleEnd -= OnBattleEnd;
        }
    }
}