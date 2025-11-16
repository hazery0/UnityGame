using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    public Transform spawnPoint;
    public float moveSpeed = 3f;       // 每格移动速度
    public float rotationSpeed = 10f;  // 转向速度

    [Header("Tile Settings")]
    private float tileSize = 1f;       // 每格大小
    private bool isMoving = false;
    private Vector3 targetPosition;

    private Collider playerCollider;
    private GridPlayerController gridPlayerController;
    private Animator animator;

    void Start()
    {
        Debug.Log("=== PlayerMovement Start ===");

        // 获取组件
        gridPlayerController = GetComponent<GridPlayerController>();
        playerCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();

        // 如果没有Collider，自动添加
        if (playerCollider == null)
        {
            playerCollider = gameObject.AddComponent<BoxCollider>();
            Debug.Log("自动添加了BoxCollider组件");
        }

        // 检查Animator
        if (animator == null)
        {
            Debug.LogError("Animator组件未找到！");
        }

        InitializeTileSystem();

        // 设置出生点
        if (spawnPoint != null)
        {
            transform.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y + 1, spawnPoint.position.z);
            targetPosition = transform.position;
            Debug.Log($"传送到出生点: {transform.position}");
        }

        // 设置初始状态
        isMoving = false;
        SetWalking(false);

        Debug.Log("PlayerMovement初始化完成");
    }

    private void InitializeTileSystem()
    {
        TileMapGenerator tileMapGen = FindObjectOfType<TileMapGenerator>();
        if (tileMapGen != null)
        {
            tileMapGen.InitializeTiles();
            Debug.Log("TileMapGenerator 初始化完成");

            Tile[,] tiles = tileMapGen.GetTiles();
            if (tiles != null && tiles.Length > 1 && tiles[0, 0] != null && tiles[1, 0] != null)
            {
                tileSize = Vector3.Distance(tiles[0, 0].transform.position, tiles[1, 0].transform.position);
                Debug.Log($"Tile大小: {tileSize}");
            }
        }
        else
        {
            Debug.LogError("未找到 TileMapGenerator！");
        }
    }

    void Update()
    {
        if (!isMoving)
        {
            HandleMovementInput();
        }
        else
        {
            MoveToTarget();
        }
    }

    private void HandleMovementInput()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W))
            direction = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.S))
            direction = Vector3.back;
        else if (Input.GetKeyDown(KeyCode.A))
            direction = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.D))
            direction = Vector3.right;

        if (direction != Vector3.zero)
        {
            targetPosition = transform.position + direction * tileSize;

            // 检查目标地块是否可走
            if (!IsTargetTileWalkable(targetPosition))
            {
                Debug.Log("目标地块不可行走，无法移动");
                return;
            }

            // 检查是否可移动
            if (gridPlayerController != null && !gridPlayerController.CanMove())
            {
                Debug.Log("移动资源不足，无法移动");
                return;
            }

            isMoving = true;

            // 开始移动时设置动画
            SetWalking(true);

            // 消耗移动资源
            if (gridPlayerController != null)
                gridPlayerController.ConsumeMovement();
        }
    }

    //检查目标Tile是否可行走
    private bool IsTargetTileWalkable(Vector3 targetPos)
    {
        // 获取TileMapGenerator实例
        TileMapGenerator tileMapGen = FindObjectOfType<TileMapGenerator>();
        if (tileMapGen == null)
        {
            Debug.LogError("未找到TileMapGenerator");
            return false;
        }

        // 查找目标位置的Tile
        Tile targetTile = FindTileAtPosition(targetPos);
        if (targetTile == null)
        {
            Debug.LogWarning($"在位置 {targetPos} 未找到Tile");
            return false;
        }

        // 检查Tile是否可走
        if (!targetTile.isWalkable)
        {
            Debug.Log($"目标Tile ({targetTile.x},{targetTile.z}) 不可行走");
            return false;
        }

        return true;
    }

    // 根据世界坐标查找Tile（修复版）
    private Tile FindTileAtPosition(Vector3 worldPos)
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        Tile closestTile = null;
        float closestDistance = float.MaxValue;

        // 使用tileSize作为搜索半径
        float searchRadius = tileSize * 0.5f;

        foreach (Tile tile in allTiles)
        {
            if (tile == null) continue;

            // 计算与目标位置的距离
            float distance = Vector3.Distance(tile.transform.position, worldPos);

            // 如果距离在搜索半径内且是最近的Tile
            if (distance < searchRadius && distance < closestDistance)
            {
                closestTile = tile;
                closestDistance = distance;
            }
        }

        if (closestTile != null)
        {
            Debug.Log($"找到目标Tile: {closestTile.name} 在位置 {worldPos}, 距离: {closestDistance}");
        }

        return closestTile;
    }

    private void MoveToTarget()
    {
        // 转向
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 平滑移动
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        // 到达目标点
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;

            // 移动结束时设置动画
            SetWalking(false);

            UpdateAllTilesRangeStatus();
            Debug.Log($"到达Tile位置: {targetPosition}");
        }
    }

    private void SetWalking(bool walking)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", walking);
            Debug.Log($"设置移动动画状态: {walking}");
        }
    }

    private void UpdateAllTilesRangeStatus()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            tile.ForceUpdateRangeStatus();
        }
    }

    void OnDrawGizmos()
    {
        if (playerCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, playerCollider.bounds.size);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // 绘制目标位置
        if (isMoving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(targetPosition, new Vector3(0.5f, 0.1f, 0.5f));
        }
    }
}