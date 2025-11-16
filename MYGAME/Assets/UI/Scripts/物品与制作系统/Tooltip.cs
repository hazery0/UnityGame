using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;
    
    [Header("UI组件")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public CanvasGroup canvasGroup;
    
    [Header("设置")]
    public float showDelay = 0.5f;
    public Vector2 offset = new Vector2(20, 20);
    
    private RectTransform rectTransform;
    private bool isShowing = false;
    private Coroutine showCoroutine;
    
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
        
        rectTransform = tooltipPanel.GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            
        HideTooltip();
    }
    
    void Update()
    {
        if (isShowing)
        {
            // 跟随鼠标位置
            Vector2 mousePos = Input.mousePosition;
            Vector2 tooltipPos = mousePos + offset;
            
            // 确保工具提示不会超出屏幕
            float pivotX = tooltipPos.x + rectTransform.rect.width > Screen.width ? 1 : 0;
            float pivotY = tooltipPos.y - rectTransform.rect.height < 0 ? 0 : 1;
            
            rectTransform.pivot = new Vector2(pivotX, pivotY);
            tooltipPanel.transform.position = tooltipPos;
        }
    }
    
    public void ShowTooltip(Item item, Vector3 position)
    {
        if (item == null) return;
        
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);
            
        showCoroutine = StartCoroutine(ShowTooltipCoroutine(item, position));
    }
    
    IEnumerator ShowTooltipCoroutine(Item item, Vector3 position)
    {
        yield return new WaitForSeconds(showDelay);
        
        if (item == null) yield break;
        
        // 设置内容
        if (titleText != null) titleText.text = item.itemName;
        if (descriptionText != null) descriptionText.text = item.itemDescription;
        
        // 生成属性文本
        if (statsText != null) statsText.text = GenerateStatsText(item);
        
        // 显示面板
        tooltipPanel.SetActive(true);
        isShowing = true;
        
        // 淡入效果
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            StartCoroutine(FadeInCoroutine(canvasGroup, 0.2f));
        }
    }
    
    IEnumerator FadeInCoroutine(CanvasGroup group, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }
        group.alpha = 1f;
    }
    
    public void HideTooltip()
    {
        isShowing = false;
        tooltipPanel.SetActive(false);
        
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }
    }
    
    string GenerateStatsText(Item item)
    {
        string stats = "";
        
        // 基础属性
        if (item.strengthBonus != 0) stats += $"力量 +{item.strengthBonus}\n";
        if (item.accuracyBonus != 0) stats += $"精准 +{item.accuracyBonus}\n";
        if (item.agilityBonus != 0) stats += $"敏捷 +{item.agilityBonus}\n";
        if (item.defenseBonus != 0) stats += $"防御 +{item.defenseBonus}\n";
        if (item.healthBonus != 0) stats += $"生命 +{item.healthBonus}\n";
        
        // 消耗品效果
        if (item.healthRestore > 0) stats += $"恢复生命: +{item.healthRestore}\n";
        if (item.hungerRestore > 0) stats += $"饱食度: +{item.hungerRestore}\n";
        if (item.energyRestore != 0) 
        {
            string sign = item.energyRestore > 0 ? "+" : "";
            stats += $"精力: {sign}{item.energyRestore}\n";
        }
        if (item.radiationReduction > 0) stats += $"降低辐射: -{item.radiationReduction}\n";
        
        // 治愈效果
        if (item.curesEffect != StatusEffectType.None) stats += $"治愈: {GetEffectName(item.curesEffect)}\n";
        
        // 负面效果
        if (item.negativeEffectChance > 0)
            stats += $"负面效果几率: {item.negativeEffectChance * 100}% ({GetEffectName(item.negativeEffect)})\n";
        
        stats += $"重量: {item.weight}kg";
        if (item.isStackable) stats += $"\n数量: {item.stackCount}/{item.maxStackSize}";
        
        return stats.TrimEnd('\n');
    }
    
    string GetEffectName(StatusEffectType effect)
    {
        return effect switch
        {
            StatusEffectType.Infection => "感染",
            StatusEffectType.Diarrhea => "腹泻",
            StatusEffectType.Bleeding => "流血",
            StatusEffectType.DeepWound => "深度裂伤",
            StatusEffectType.Fracture => "骨折",
            StatusEffectType.RadiationSickness => "辐射病",
            StatusEffectType.RadiationMutation => "辐射异变",
            StatusEffectType.RadiationDiscomfort => "辐射不适",
            _ => effect.ToString()
        };
    }
    
    void OnDisable()
    {
        HideTooltip();
    }
}