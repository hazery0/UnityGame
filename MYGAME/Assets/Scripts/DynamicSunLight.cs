using UnityEngine;

public class DynamicSunLight : MonoBehaviour
{
    [Header("时间管理器引用")]
    public TimeManager timeManager;

    [Header("光照强度设置")]
    public float maxLightIntensity = 1.5f;
    public float minLightIntensity = 0.1f;
    public float ambientIntensityMultiplier = 0.3f;

    [Header("光照颜色设置")]
    public Color morningColor = new Color(1f, 0.8f, 0.6f);     // 暖黄色
    public Color noonColor = new Color(1f, 0.95f, 0.8f);       // 亮白色
    public Color afternoonColor = new Color(1f, 0.7f, 0.4f);   // 橙色
    public Color eveningColor = new Color(0.8f, 0.5f, 0.3f);   // 红色
    public Color midnightColor = new Color(0.3f, 0.4f, 0.8f);  // 蓝色

    [Header("太阳角度设置")]
    [Range(0, 360)]
    public float sunriseAngle = 15f;      // 日出角度
    [Range(0, 360)]
    public float morningAngle = 45f;      // 早上角度
    [Range(0, 360)]
    public float noonAngle = 90f;         // 正午角度
    [Range(0, 360)]
    public float afternoonAngle = 135f;   // 下午角度
    [Range(0, 360)]
    public float sunsetAngle = 165f;      // 日落角度
    [Range(0, 360)]
    public float nightAngle = 180f;       // 夜晚角度

    [Header("平滑过渡")]
    public float rotationSmoothness = 2f;
    public float intensitySmoothness = 2f;
    public float colorSmoothness = 2f;

    [Header("调试")]
    public bool showDebugInfo = false;

    private Light sunLight;
    private RenderSettings ambientLight;
    private Quaternion targetRotation;
    private float targetIntensity;
    private Color targetColor;

    void Start()
    {
        sunLight = GetComponent<Light>();
        if (sunLight == null)
        {
            Debug.LogError("DynamicSunLight需要Light组件！");
            return;
        }

        // 如果没有指定时间管理器，尝试自动查找
        if (timeManager == null)
        {
            timeManager = TimeManager.Instance;
            if (timeManager == null)
            {
                timeManager = FindObjectOfType<TimeManager>();
            }
        }

        if (timeManager == null)
        {
            Debug.LogWarning("未找到TimeManager，使用默认时间设置");
        }

        // 初始设置
        UpdateSunLight();

        // 订阅时间变化事件
        if (timeManager != null)
        {
            timeManager.OnTimeSegmentChanged += OnTimeSegmentChanged;
        }

        Debug.Log("DynamicSunLight初始化完成");
    }

    void Update()
    {
        SmoothUpdateSunLight();

        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"太阳角度: {transform.rotation.eulerAngles.x:F1}°, 强度: {sunLight.intensity:F2}, 颜色: {sunLight.color}");
        }
    }

    void OnDestroy()
    {
        // 取消订阅事件
        if (timeManager != null)
        {
            timeManager.OnTimeSegmentChanged -= OnTimeSegmentChanged;
        }
    }

    void OnTimeSegmentChanged(TimeManager.TimeSegment newTimeSegment)
    {
        UpdateSunLight();
        Debug.Log($"时间变化到 {newTimeSegment}，更新太阳光照");
    }

    void UpdateSunLight()
    {
        if (sunLight == null) return;

        // 根据当前时间段设置目标值
        SetTargetValuesForTimeSegment();
    }

    void SetTargetValuesForTimeSegment()
    {
        if (timeManager == null) return;

        switch (timeManager.currentTime)
        {
            case TimeManager.TimeSegment.早上: // 早上 (6:00-12:00)
                targetRotation = Quaternion.Euler(morningAngle, 0, 0);
                targetIntensity = maxLightIntensity * 0.8f;
                targetColor = morningColor;
                break;

            case TimeManager.TimeSegment.下午: // 下午 (12:00-18:00)
                targetRotation = Quaternion.Euler(afternoonAngle, 0, 0);
                targetIntensity = maxLightIntensity;
                targetColor = afternoonColor;
                break;

            case TimeManager.TimeSegment.晚上: // 晚上 (18:00-24:00)
                targetRotation = Quaternion.Euler(sunsetAngle, 0, 0);
                targetIntensity = maxLightIntensity * 0.4f;
                targetColor = eveningColor;
                break;

            case TimeManager.TimeSegment.凌晨: // 凌晨 (0:00-6:00)
                targetRotation = Quaternion.Euler(nightAngle, 0, 0);
                targetIntensity = minLightIntensity;
                targetColor = midnightColor;
                break;
        }

        // 更新环境光
        UpdateAmbientLight();
    }

    void SmoothUpdateSunLight()
    {
        if (sunLight == null) return;

        // 平滑旋转
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);

        // 平滑强度
        sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetIntensity, intensitySmoothness * Time.deltaTime);

        // 平滑颜色
        sunLight.color = Color.Lerp(sunLight.color, targetColor, colorSmoothness * Time.deltaTime);
    }

    void UpdateAmbientLight()
    {
        // 设置环境光强度（基于主光强度）
        RenderSettings.ambientIntensity = sunLight.intensity * ambientIntensityMultiplier;

        // 设置环境光颜色（基于主光颜色但更柔和）
        Color ambientColor = Color.Lerp(targetColor, Color.white, 0.7f);
        RenderSettings.ambientLight = ambientColor;
    }

    // 公共方法：手动设置太阳状态（用于调试或特殊事件）
    public void SetSunState(float angle, float intensity, Color color)
    {
        targetRotation = Quaternion.Euler(angle, 0, 0);
        targetIntensity = intensity;
        targetColor = color;
    }

    // 公共方法：重置为自动模式
    public void ResetToAutoMode()
    {
        UpdateSunLight();
    }

    // 公共方法：获取当前光照信息
    public SunLightInfo GetCurrentLightInfo()
    {
        return new SunLightInfo
        {
            angle = transform.rotation.eulerAngles.x,
            intensity = sunLight.intensity,
            color = sunLight.color
        };
    }

    // 调试方法：在Scene视图中显示信息
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 12;

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
            $"Sun Light\nAngle: {transform.rotation.eulerAngles.x:F1}°\nIntensity: {sunLight.intensity:F2}", style);
#endif
    }
}

// 光照信息结构体
[System.Serializable]
public struct SunLightInfo
{
    public float angle;
    public float intensity;
    public Color color;
}

// 可选的月光脚本（如果需要夜晚有月光）
public class MoonLight : MonoBehaviour
{
    public Light moonLight;
    public TimeManager timeManager;
    public Color moonColor = new Color(0.7f, 0.8f, 1f, 0.3f);
    public float maxMoonIntensity = 0.3f;

    void Start()
    {
        moonLight = GetComponent<Light>();
        if (timeManager == null)
            timeManager = FindObjectOfType<TimeManager>();

        if (timeManager != null)
            timeManager.OnTimeSegmentChanged += OnTimeSegmentChanged;
    }

    void OnTimeSegmentChanged(TimeManager.TimeSegment newTimeSegment)
    {
        if (moonLight == null) return;

        // 只在晚上和凌晨显示月光
        bool shouldBeActive = (newTimeSegment == TimeManager.TimeSegment.晚上 ||
                              newTimeSegment == TimeManager.TimeSegment.凌晨);

        moonLight.enabled = shouldBeActive;

        if (shouldBeActive)
        {
            moonLight.intensity = maxMoonIntensity;
            moonLight.color = moonColor;
        }
    }

    void OnDestroy()
    {
        if (timeManager != null)
            timeManager.OnTimeSegmentChanged -= OnTimeSegmentChanged;
    }
}