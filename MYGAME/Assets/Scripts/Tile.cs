using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector] public Vector2Int gridPos;

    [Header("Walkable Settings")]
    public bool isWalkable = true;
    public Color walkableColor = Color.green;
    public Color unwalkableColor = Color.red;

    [Header("Move Range Settings")]
    public static int maxMoveDistance = 3;

    private Renderer rend;
    private Color baseColor;
    public Color hoverColor = new Color(1f, 1f, 0.6f);

    public int x;
    public int z;

    [Header("Selection Indicator")]
    public SelectionIndicator selectionIndicator;
    private bool isHovered = false;

    public bool isInitialized = false;

    private bool isInRange = false;
    private static Vector3 playerPosition;
    private static int playerGridX, playerGridZ;
    private const int MAX_MOVE_DISTANCE = 3;

    // 添加静态属性以便其他类访问
    public static int PlayerTileX => playerGridX;
    public static int PlayerTileZ => playerGridZ;
    public static Vector3 PlayerWorldPosition => playerPosition;
    public static int MoveRange => MAX_MOVE_DISTANCE;

    public void Init(int x, int y, bool walkable = true)
    {
        this.x = x;
        this.z = y;
        this.isWalkable = walkable;
        gridPos = new Vector2Int(x, y);

        gameObject.name = $"Tile_{x}_{y}";

        InitializeIndicator();
        UpdateIndicatorColor();
    }

    void Start()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;

        if (!isInitialized)
        {
            InitializeIndicator();
        }

        UpdateIndicatorColor();
    }

    void Update()
    {
        if (Time.frameCount % 15 == 0)
        {
            UpdateRangeStatus();
        }
    }

    private void UpdateRangeStatus()
    {
        if (playerPosition == Vector3.zero) return;

        // 获取Tile的实际尺寸（考虑缩放）
        Vector3 tileSize = GetTileSize();
        float scaleFactor = Mathf.Max(tileSize.x, 1f); // 确保至少为1

        // 考虑缩放的曼哈顿距离计算
        int scaledPlayerX = Mathf.RoundToInt(playerGridX * scaleFactor);
        int scaledPlayerZ = Mathf.RoundToInt(playerGridZ * scaleFactor);

        int manhattanDistance = Mathf.Abs(x - scaledPlayerX) + Mathf.Abs(z - scaledPlayerZ);
        int scaledMoveRange = Mathf.RoundToInt(MAX_MOVE_DISTANCE * scaleFactor);

        bool newInRange = manhattanDistance <= scaledMoveRange;

        if (newInRange != isInRange)
        {
            isInRange = newInRange;
            UpdateIndicatorColor();
        }
    }

    private void InitializeIndicator()
    {
        if (isInitialized) return;

        if (selectionIndicator == null)
        {
            selectionIndicator = GetComponentInChildren<SelectionIndicator>(true);
        }

        if (selectionIndicator != null)
        {
            selectionIndicator.SetVisibility(false);
            isInitialized = true;
        }
    }

    void OnMouseEnter()
    {
        isHovered = true;

        if (rend != null)
            rend.material.color = hoverColor;

        if (selectionIndicator != null && isInitialized)
        {
            selectionIndicator.SetVisibility(true);
            UpdateRangeStatus();
            UpdateIndicatorColor();
        }
    }

    void OnMouseExit()
    {
        isHovered = false;

        if (rend != null)
            rend.material.color = baseColor;

        if (selectionIndicator != null && isInitialized)
        {
            selectionIndicator.SetVisibility(false);
        }
    }

    public void ResetColor()
    {
        if (rend != null)
            rend.material.color = baseColor;
    }

    public void UpdateIndicatorColor()
    {
        if (selectionIndicator == null || !isInitialized) return;

        Color targetColor;

        if (!isWalkable)
        {
            targetColor = unwalkableColor;
        }
        else if (isInRange)
        {
            targetColor = walkableColor;
        }
        else
        {
            targetColor = unwalkableColor;
        }

        selectionIndicator.SetEmissionColor(targetColor);
    }

    public bool IsInMoveRange()
    {
        return isWalkable && isInRange;
    }

    public void SetWalkable(bool walkable)
    {
        isWalkable = walkable;
        UpdateIndicatorColor();

        if (rend != null)
        {
            rend.material.color = walkable ? baseColor : new Color(0.8f, 0.3f, 0.3f);
        }
    }

    // 原有的玩家位置更新方法
    public static void UpdatePlayerPosition(Vector3 position, int gridX, int gridZ)
    {
        playerPosition = position;
        playerGridX = gridX;
        playerGridZ = gridZ;
    }

    // 新增：考虑缩放的玩家位置更新方法
    public static void UpdatePlayerPositionScaled(Vector3 position, int gridX, int gridZ, float scaleFactor = 1f)
    {
        playerPosition = position;
        playerGridX = Mathf.RoundToInt(gridX / scaleFactor);
        playerGridZ = Mathf.RoundToInt(gridZ / scaleFactor);

        Debug.Log($"玩家位置更新(缩放): 世界位置 {position}, 网格坐标 ({gridX}, {gridZ}) -> 缩放坐标 ({playerGridX}, {playerGridZ}), 缩放因子 {scaleFactor}");
    }

    void OnValidate()
    {
        if (Application.isPlaying && isInitialized)
        {
            UpdateIndicatorColor();
        }
    }

    public void ForceUpdateRangeStatus()
    {
        if (playerPosition == Vector3.zero) return;

        // 同样的缩放计算
        Vector3 tileSize = GetTileSize();
        float scaleFactor = Mathf.Max(tileSize.x, 1f);

        int scaledPlayerX = Mathf.RoundToInt(playerGridX * scaleFactor);
        int scaledPlayerZ = Mathf.RoundToInt(playerGridZ * scaleFactor);

        int manhattanDistance = Mathf.Abs(x - scaledPlayerX) + Mathf.Abs(z - scaledPlayerZ);
        int scaledMoveRange = Mathf.RoundToInt(MAX_MOVE_DISTANCE * scaleFactor);

        bool newInRange = manhattanDistance <= scaledMoveRange;

        if (newInRange != isInRange)
        {
            isInRange = newInRange;
            UpdateIndicatorColor();
            Debug.Log($"强制更新 Tile ({x},{z}) - 距离: {manhattanDistance}, 缩放范围: {scaledMoveRange}, 在范围内: {isInRange}");
        }
    }

    // 获取曼哈顿距离（调试用）
    public int GetManhattanDistance()
    {
        Vector3 tileSize = GetTileSize();
        float scaleFactor = Mathf.Max(tileSize.x, 1f);

        int scaledPlayerX = Mathf.RoundToInt(playerGridX * scaleFactor);
        int scaledPlayerZ = Mathf.RoundToInt(playerGridZ * scaleFactor);

        return Mathf.Abs(x - scaledPlayerX) + Mathf.Abs(z - scaledPlayerZ);
    }

    public Vector3 GetTileSize()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size;
        }
        return transform.localScale;
    }

    [ContextMenu("显示Tile信息")]
    public void ShowTileInfo()
    {
        Vector3 size = GetTileSize();
        Debug.Log($"Tile: {name}, 坐标: ({x},{z}), 尺寸: {size.x:F2}x{size.z:F2}, 位置: {transform.position}");
    }
}