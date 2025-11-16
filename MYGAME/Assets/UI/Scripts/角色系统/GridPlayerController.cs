using UnityEngine;

public class GridPlayerController : MonoBehaviour
{
    [Header("网格设置")]
    public float gridSize = 1f;

    [Header("事件检测")]
    public float eventCheckInterval = 0.1f;

    private PlayerStats playerStats;
    private TimeManager timeManager;
    private float lastCheckTime;
    private Vector2Int lastGridPosition;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        timeManager = TimeManager.Instance;

        // 初始化事件检测
        lastGridPosition = GetCurrentGridPosition();
        lastCheckTime = Time.time;

        Debug.Log("GridPlayerController初始化完成 - 仅事件检测模式");
    }

    void Update()
    {
        // 处理UI切换和事件检测
        HandleUIInput();
        CheckCurrentGridEvent();
    }

    void HandleUIInput()
    {
        // 按C键打开角色属性UI
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCharacterUI();
        }
    }

    // 获取当前网格位置（用于事件检测）
    public Vector2Int GetCurrentGridPosition()
    {
        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x / gridSize),
            Mathf.RoundToInt(transform.position.z / gridSize)
        );
    }

    // 持续检测当前网格事件
    void CheckCurrentGridEvent()
    {
        if (Time.time - lastCheckTime < eventCheckInterval) return;

        Vector2Int currentGrid = GetCurrentGridPosition();
        lastCheckTime = Time.time;

        if (currentGrid != lastGridPosition)
        {
            lastGridPosition = currentGrid;
            CheckGridEvent(currentGrid);
        }
    }

    // 外部调用的立即检测事件方法
    public void CheckImmediateEvent()
    {
        Vector2Int currentGrid = GetCurrentGridPosition();
        CheckGridEvent(currentGrid);
        Debug.Log($"移动到新位置，检测事件: {currentGrid}");
    }

    // 网格事件检测
    void CheckGridEvent(Vector2Int gridCoord)
    {
        if (GridEventSystem.Instance != null)
        {
            if (GridEventSystem.Instance.TryGetEvent(gridCoord, out var gridEvent))
            {
                if (gridEvent != null)
                {
                    ExecuteGridEvent(gridEvent);

                    // 标记一次性事件
                    if (gridEvent.isOneTime)
                    {
                        GridEventSystem.Instance.MarkEventTriggered(gridCoord);
                    }
                }
            }
        }
    }

    // 执行事件
    void ExecuteGridEvent(GridEventSystem.GridEvent gridEvent)
    {
        if (playerStats == null) return;

        // 触发事件消耗
        playerStats.ConsumeEvent();

        Debug.Log($"触发网格事件: {gridEvent.eventType} at {gridEvent.coord}");

        // 根据事件类型执行不同逻辑
        switch (gridEvent.eventType)
        {
            case GridEventSystem.EventType.Damage:
                playerStats.TakeDamage(gridEvent.eventParam);
                Debug.Log($"触发伤害事件，受到{gridEvent.eventParam}点伤害");
                break;

            case GridEventSystem.EventType.Heal:
                playerStats.Heal(gridEvent.eventParam);
                Debug.Log($"触发治疗事件，恢复{gridEvent.eventParam}点生命");
                break;

            case GridEventSystem.EventType.ScrapYard:
                Debug.Log("发现废品站，可以搜索材料");
                // 这里可以触发UI或其他交互
                break;

            case GridEventSystem.EventType.MedicalStation:
                Debug.Log("发现医疗站，可以搜索医疗用品");
                // 这里可以触发UI或其他交互
                break;

            default:
                Debug.Log($"触发未知事件类型: {gridEvent.eventType}");
                break;
        }
    }

    // 更新战争迷雾
    public void UpdateFogOfWar()
    {
        Vector2Int currentGrid = GetCurrentGridPosition();
        int visionRange = 2; // 2格视野

        for (int x = -visionRange; x <= visionRange; x++)
        {
            for (int y = -visionRange; y <= visionRange; y++)
            {
                Vector2Int revealGrid = new Vector2Int(currentGrid.x + x, currentGrid.y + y);
                RevealGrid(revealGrid);
            }
        }

        Debug.Log($"更新战争迷雾，中心位置: {currentGrid}");
    }

    // 显示网格
    void RevealGrid(Vector2Int gridCoord)
    {
        // 调用战争迷雾系统的显示逻辑
        Debug.Log($"显示网格: {gridCoord}");
    }

    // 切换角色属性UI
    void ToggleCharacterUI()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.ToggleCharacterUI();
        }
        else
        {
            Debug.LogWarning("CharacterManager实例未找到！");
        }
    }

    // 公共方法：获取玩家状态
    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }

    // 公共方法：检查是否可以移动（基于资源）
    public bool CanMove()
    {
        return playerStats != null && playerStats.currentTimeSegmentMoves > 0;
    }

    // 公共方法：消耗移动（由PlayerController调用）
    public void ConsumeMovement()
    {
        if (playerStats != null)
        {
            playerStats.ConsumeMovement();

            // 触发时间管理器
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.ConsumeMove();
            }

            UpdateFogOfWar();
            CheckImmediateEvent();
        }
    }

    // 调试显示
    void OnDrawGizmos()
    {
        // 绘制当前网格
        Gizmos.color = Color.blue;
        Vector2Int currentGrid = GetCurrentGridPosition();
        Vector3 gridCenter = new Vector3(currentGrid.x * gridSize, 0, currentGrid.y * gridSize);
        Gizmos.DrawWireCube(gridCenter, Vector3.one * gridSize);

        // 绘制视野范围
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawWireCube(gridCenter, Vector3.one * gridSize * 5f); // 5x5视野范围
    }
}