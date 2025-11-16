using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    public float floatHeight = 2f;
    public float floatSpeed = 2f;
    public float rotationSpeed = 90f;
    public float floatAmplitude = 0.1f; // 漂浮幅度

    [Header("Visual Settings")]
    public bool enablePulseEffect = true;
    public float pulseMin = 1f;
    public float pulseMax = 3f;
    public float pulseSpeed = 2f;

    private Vector3 startPosition;
    private bool isVisible = false;
    private Renderer indicatorRenderer;
    private Material indicatorMaterial;
    private Color originalEmissionColor;
    private Color currentEmissionColor;

    void Start()
    {
        // 初始化位置
        startPosition = transform.localPosition;

        // 获取渲染器和材质
        indicatorRenderer = GetComponentInChildren<Renderer>();
        if (indicatorRenderer != null)
        {
            indicatorMaterial = indicatorRenderer.material;
            // 保存原始发光颜色
            if (indicatorMaterial != null && indicatorMaterial.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = indicatorMaterial.GetColor("_EmissionColor");
                currentEmissionColor = originalEmissionColor;
            }
        }
        else
        {
            Debug.LogWarning("SelectionIndicator 未找到Renderer组件");
        }

        // 初始隐藏
        SetVisibility(false);

        Debug.Log("SelectionIndicator 初始化完成");
    }

    void Update()
    {
        if (!isVisible) return;

        // 漂浮动画
        HandleFloatingAnimation();

        // 旋转动画
        HandleRotationAnimation();

        // 脉冲效果
        if (enablePulseEffect)
        {
            HandlePulseEffect();
        }
    }

    private void HandleFloatingAnimation()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = new Vector3(
            startPosition.x,
            newY + floatHeight,
            startPosition.z
        );
    }

    private void HandleRotationAnimation()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    private void HandlePulseEffect()
    {
        if (indicatorMaterial != null && indicatorMaterial.HasProperty("_EmissionColor"))
        {
            float pulse = pulseMin + (Mathf.Sin(Time.time * pulseSpeed) + 1) * 0.5f * (pulseMax - pulseMin);
            Color pulseColor = currentEmissionColor * pulse;
            indicatorMaterial.SetColor("_EmissionColor", pulseColor);
        }
    }

    public void SetVisibility(bool visible)
    {
        isVisible = visible;
        gameObject.SetActive(visible);

        if (visible)
        {
            // 重置位置和旋转
            transform.localPosition = new Vector3(
                startPosition.x,
                startPosition.y + floatHeight,
                startPosition.z
            );
            transform.rotation = Quaternion.identity;

            Debug.Log("SelectionIndicator 已显示");
        }
        else
        {
            Debug.Log("SelectionIndicator 已隐藏");
        }
    }

    // 设置漂浮高度
    public void SetFloatHeight(float height)
    {
        floatHeight = height;
    }

    // 设置动画速度
    public void SetAnimationSpeed(float newFloatSpeed, float newRotationSpeed)
    {
        floatSpeed = newFloatSpeed;
        rotationSpeed = newRotationSpeed;
    }

    // 设置脉冲效果
    public void SetPulseEffect(bool enable, float min = 1f, float max = 3f, float speed = 2f)
    {
        enablePulseEffect = enable;
        pulseMin = min;
        pulseMax = max;
        pulseSpeed = speed;
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    // 设置发射颜色
    public void SetEmissionColor(Color newColor)
    {
        currentEmissionColor = newColor;

        if (indicatorMaterial != null && indicatorMaterial.HasProperty("_EmissionColor"))
        {
            if (isVisible && enablePulseEffect)
            {
                // 如果可见且有脉冲效果，颜色会在Update中处理
                return;
            }
            else
            {
                // 直接设置颜色
                indicatorMaterial.SetColor("_EmissionColor", newColor);
            }
        }
    }

    // 重置为原始颜色
    public void ResetEmissionColor()  // ← 全新方法
    {
        SetEmissionColor(originalEmissionColor);
    }
}