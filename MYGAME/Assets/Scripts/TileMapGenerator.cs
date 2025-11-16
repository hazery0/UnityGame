using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 将枚举移到类外部
public enum TileType
{
    Default = 0,
    Mountain = 1,
    Type02 = 2
}

[ExecuteAlways]
public class TileMapGenerator : MonoBehaviour
{
    [Header("工作模式")]
    public bool useManualTiles = true;

    [Header("调试信息")]
    public int gridWidth = 0;
    public int gridHeight = 0;
    public int minX = 0;
    public int maxX = 0;
    public int minZ = 0;
    public int maxZ = 0;
    public int totalTilesInGrid = 0;

    [Header("网格可视化")]
    public bool showGridGizmos = true;
    public Color gridGizmoColor = Color.cyan;

    [Header("自动生成设置（非手动模式使用）")]
    public int startX = -25;
    public int startZ = -25;
    public int LengthX = 50;
    public int LengthZ = 50;
    public GameObject defaultTilePrefab;
    public GameObject mountainPrefab;
    public GameObject tilePrefab02;
    public List<TileTypeRow> tileMap = new List<TileTypeRow>();

    private Tile[,] tiles;
    private Dictionary<Vector2Int, Tile> tileDictionary;
    private Transform tilesParent;

    void Start()
    {
        if (Application.isPlaying)
        {
            Debug.Log("=== TileMapGenerator Start ===");
            InitializeTiles();
        }
    }

    public void InitializeTiles()
    {
        if (useManualTiles)
        {
            CollectManualTiles();
        }
        else
        {
            GenerateMap();
        }
        Debug.Log($"✅ Tile系统初始化完成: {gridWidth}x{gridHeight} 网格, {totalTilesInGrid} 个Tile");

    }

    private void UseScaledTileCoordinates(Tile[] allTiles)
    {
        if (allTiles.Length == 0) return;

        // 获取第一个Tile的尺寸（考虑Scale）
        Vector3 tileSize = Vector3.zero;
        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null)
                {
                    tileSize = renderer.bounds.size;
                    break;
                }
            }
        }

        // 如果无法通过Renderer获取，使用transform的scale
        if (tileSize == Vector3.zero && allTiles[0] != null)
        {
            tileSize = allTiles[0].transform.localScale;
            Debug.LogWarning($"使用Transform缩放作为Tile尺寸: {tileSize.x:F2}x{tileSize.z:F2}");
        }

        Debug.Log($"检测到Tile尺寸: {tileSize.x:F2} x {tileSize.z:F2}");

        // 计算网格边界（基于实际位置）
        float minWorldX = float.MaxValue, maxWorldX = float.MinValue;
        float minWorldZ = float.MaxValue, maxWorldZ = float.MinValue;

        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                Vector3 worldPos = tile.transform.position;
                minWorldX = Mathf.Min(minWorldX, worldPos.x);
                maxWorldX = Mathf.Max(maxWorldX, worldPos.x);
                minWorldZ = Mathf.Min(minWorldZ, worldPos.z);
                maxWorldZ = Mathf.Max(maxWorldZ, worldPos.z);
            }
        }

        // 根据Tile尺寸计算网格坐标
        Dictionary<Vector2Int, Tile> coordinateMap = new Dictionary<Vector2Int, Tile>();

        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                Vector3 worldPos = tile.transform.position;

                // 根据世界位置和Tile尺寸计算网格坐标
                int gridX = Mathf.RoundToInt((worldPos.x - minWorldX) / tileSize.x);
                int gridZ = Mathf.RoundToInt((worldPos.z - minWorldZ) / tileSize.z);

                Vector2Int coord = new Vector2Int(gridX, gridZ);

                // 处理坐标冲突
                if (coordinateMap.ContainsKey(coord))
                {
                    Debug.LogWarning($"坐标冲突: ({gridX},{gridZ}) 已有 {coordinateMap[coord].name}，当前 {tile.name}");
                    // 寻找最近的空坐标
                    for (int offset = 1; offset < 10; offset++)
                    {
                        Vector2Int newCoord = new Vector2Int(gridX + offset, gridZ);
                        if (!coordinateMap.ContainsKey(newCoord))
                        {
                            coord = newCoord;
                            break;
                        }
                    }
                }

                coordinateMap[coord] = tile;
                tile.x = coord.x;
                tile.z = coord.y;
                tile.gameObject.name = $"Tile_{coord.x}_{coord.y}";

                Debug.Log($"设置 {tile.name} 坐标为 ({coord.x},{coord.y})，世界位置: {worldPos}");
            }
        }

        // 计算网格尺寸
        minX = 0; maxX = 0; minZ = 0; maxZ = 0;
        foreach (var coord in coordinateMap.Keys)
        {
            minX = Mathf.Min(minX, coord.x);
            maxX = Mathf.Max(maxX, coord.x);
            minZ = Mathf.Min(minZ, coord.y);
            maxZ = Mathf.Max(maxZ, coord.y);
        }

        gridWidth = maxX - minX + 1;
        gridHeight = maxZ - minZ + 1;

        // 创建网格数组
        tiles = new Tile[gridWidth, gridHeight];
        tileDictionary = new Dictionary<Vector2Int, Tile>();
        totalTilesInGrid = 0;

        foreach (var kvp in coordinateMap)
        {
            int arrayX = kvp.Key.x - minX;
            int arrayZ = kvp.Key.y - minZ;

            if (arrayX >= 0 && arrayX < gridWidth && arrayZ >= 0 && arrayZ < gridHeight)
            {
                tiles[arrayX, arrayZ] = kvp.Value;
                tileDictionary[kvp.Key] = kvp.Value;
                totalTilesInGrid++;
            }
        }

        Debug.Log($"✅ 缩放Tile网格构建完成: {gridWidth}x{gridHeight}, 有效Tile: {totalTilesInGrid}");
        Debug.Log($"📏 世界范围: X[{minWorldX:F1}-{maxWorldX:F1}] Z[{minWorldZ:F1}-{maxWorldZ:F1}]");
    }

    private void CollectManualTiles()
    {
        Tile[] allTiles = Resources.FindObjectsOfTypeAll<Tile>();
        List<Tile> sceneTiles = new List<Tile>();

        foreach (Tile tile in allTiles)
        {
            if (tile.gameObject.scene.IsValid() && tile.gameObject.activeInHierarchy)
            {
                sceneTiles.Add(tile);
            }
        }

        Debug.Log($"找到 {sceneTiles.Count} 个场景中的Tile");

        if (sceneTiles.Count == 0)
        {
            Debug.LogError("❌ 没有找到任何Tile！");
            tiles = new Tile[0, 0];
            return;
        }

        // 使用考虑缩放的新方法
        UseScaledTileCoordinates(sceneTiles.ToArray());
    }

    private void CalculateGridBounds(Tile[] allTiles)
    {
        minX = int.MaxValue; maxX = int.MinValue;
        minZ = int.MaxValue; maxZ = int.MinValue;

        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                minX = Mathf.Min(minX, tile.x);
                maxX = Mathf.Max(maxX, tile.x);
                minZ = Mathf.Min(minZ, tile.z);
                maxZ = Mathf.Max(maxZ, tile.z);
            }
        }

        gridWidth = maxX - minX + 1;
        gridHeight = maxZ - minZ + 1;

        Debug.Log($"📐 网格边界: X[{minX}~{maxX}] Z[{minZ}~{maxZ}]");
        Debug.Log($"📏 网格尺寸: {gridWidth}x{gridHeight}");
    }

    private void CreateTileGrid(Tile[] allTiles)
    {
        tiles = new Tile[gridWidth, gridHeight];
        tileDictionary = new Dictionary<Vector2Int, Tile>();
        totalTilesInGrid = 0;

        // 先记录所有Tile到字典
        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                Vector2Int coord = new Vector2Int(tile.x, tile.z);
                if (!tileDictionary.ContainsKey(coord))
                {
                    tileDictionary[coord] = tile;
                }
                else
                {
                    Debug.LogWarning($"⚠️ 重复坐标的Tile: ({tile.x},{tile.z}) - {tile.name}");
                }
            }
        }

        // 填充网格数组
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                int worldX = x + minX;
                int worldZ = z + minZ;
                Vector2Int coord = new Vector2Int(worldX, worldZ);

                if (tileDictionary.ContainsKey(coord))
                {
                    tiles[x, z] = tileDictionary[coord];
                    totalTilesInGrid++;
                }
                else
                {
                    tiles[x, z] = null;
                }
            }
        }

        Debug.Log($"✅ 网格构建完成: {totalTilesInGrid}/{allTiles.Length} 个Tile放入网格");
    }

    // 自动生成地图的方法
    private void GenerateMap()
    {
        if (defaultTilePrefab == null)
        {
            Debug.LogError("TileMapGenerator: 默认地块 prefab 没有赋值！");
            return;
        }

        if (tilesParent != null)
            DestroyImmediate(tilesParent.gameObject);

        CreateTilesParent();
        CreateTilesArray();
    }

    private void CreateTilesParent()
    {
        tilesParent = new GameObject("Tiles").transform;
        tilesParent.parent = transform;
        tilesParent.localPosition = Vector3.zero;
    }

    private void CreateTilesArray()
    {
        tiles = new Tile[LengthX, LengthZ];

        for (int x = 0; x < LengthX; x++)
        {
            for (int z = 0; z < LengthZ; z++)
            {
                TileType tileType = tileMap[z].cols[x];
                CreateTileAt(x, z, GetPrefabByType(tileType), IsWalkable(tileType));
            }
        }

        // 重新计算网格信息
        Tile[] allTiles = FindObjectsOfType<Tile>();
        CalculateGridBounds(allTiles);
        CreateTileGrid(allTiles);
    }

    private GameObject GetPrefabByType(TileType type)
    {
        return type switch
        {
            TileType.Mountain => mountainPrefab != null ? mountainPrefab : defaultTilePrefab,
            TileType.Type02 => tilePrefab02 != null ? tilePrefab02 : defaultTilePrefab,
            _ => defaultTilePrefab
        };
    }

    private bool IsWalkable(TileType type)
    {
        return type == TileType.Default || type == TileType.Type02;
    }

    private Tile CreateTileAt(int x, int z, GameObject prefabToUse, bool walkable)
    {
        if (prefabToUse == null)
        {
            Debug.LogError($"TileMapGenerator: 地块预制体为空！（坐标：{x},{z}）");
            return null;
        }

        Vector3 prefabScale = prefabToUse.transform.localScale;
        Vector3 pos = new Vector3(
            Mathf.RoundToInt(startX + x * prefabScale.x),
            prefabToUse.transform.position.y,
            Mathf.RoundToInt(startZ + z * prefabScale.z)
        );

        return GenerateTile(pos, x, z, prefabToUse, walkable);
    }

    public Tile GenerateTile(Vector3 position, int x, int y, GameObject prefab, bool walkable = true)
    {
        GameObject go = Instantiate(prefab, position, prefab.transform.rotation, tilesParent);
        go.name = $"Tile_{x}_{y}";

        Tile tile = go.GetComponent<Tile>();
        if (tile == null)
            tile = go.AddComponent<Tile>();

        tile.Init(x, y);
        tile.isWalkable = walkable;

        if (go.GetComponent<Collider>() == null)
            go.AddComponent<BoxCollider>();

        go.tag = "Tile";

        return tile;
    }

    public Tile GetTileAtCoordinates(int x, int z)
    {
        if (tileDictionary != null && tileDictionary.ContainsKey(new Vector2Int(x, z)))
        {
            return tileDictionary[new Vector2Int(x, z)];
        }
        return null;
    }

    public bool TryGetGridIndex(Tile tile, out int gridX, out int gridZ)
    {
        gridX = -1;
        gridZ = -1;

        if (tile == null) return false;

        gridX = tile.x - minX;
        gridZ = tile.z - minZ;

        bool inGrid = gridX >= 0 && gridX < gridWidth && gridZ >= 0 && gridZ < gridHeight;

        if (!inGrid)
        {
            Debug.LogWarning($"🚫 Tile ({tile.x},{tile.z}) 不在网格内. 网格范围: [{minX}-{maxX}], [{minZ}-{maxZ}]");
        }

        return inGrid;
    }

    public Tile[,] GetTiles()
    {
        if (tiles == null)
        {
            InitializeTiles();
        }
        return tiles;
    }

#if UNITY_EDITOR
    [ContextMenu("🔄 自动设置所有Tile坐标")]
    public void AutoSetTileCoordinates()
    {
        Tile[] allTiles = Resources.FindObjectsOfTypeAll<Tile>();
        int setCount = 0;
        
        foreach (Tile tile in allTiles)
        {
            if (tile != null && tile.gameObject.scene.IsValid())
            {
                Vector3 position = tile.transform.position;
                tile.x = Mathf.RoundToInt(position.x);
                tile.z = Mathf.RoundToInt(position.z);
                tile.gameObject.name = $"Tile_{tile.x}_{tile.z}";
                
                // 确保Tile组件已初始化
                if (!tile.isInitialized)
                {
                    tile.Init(tile.x, tile.z, tile.isWalkable);
                }
                setCount++;
            }
        }
        
        Debug.Log($"✅ 已为 {setCount} 个Tile设置坐标");
        
        // 重新构建网格
        CollectManualTiles();
    }

    [ContextMenu("📊 显示网格信息")]
    public void ShowGridInfo()
    {
        if (tiles == null)
        {
            Debug.Log("📭 网格未初始化");
            return;
        }

        Debug.Log($"📊 网格信息:");
        Debug.Log($"  尺寸: {gridWidth}x{gridHeight}");
        Debug.Log($"  坐标范围: X[{minX}-{maxX}] Z[{minZ}-{maxZ}]");
        Debug.Log($"  有效Tile数量: {totalTilesInGrid}");

    }

    [ContextMenu("🔍 检查网格完整性")]
    public void CheckGridIntegrity()
    {
        CollectManualTiles();
        
        int nullCount = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (tiles[x, z] == null)
                {
                    nullCount++;
                }
            }
        }

        Debug.Log($"🔍 网格完整性检查:");
        Debug.Log($"  总网格位置: {gridWidth * gridHeight}");
        Debug.Log($"  空位置: {nullCount}");
        Debug.Log($"  填充率: {((float)totalTilesInGrid / (gridWidth * gridHeight)) * 100f:F1}%");

        if (nullCount > totalTilesInGrid)
        {
            Debug.LogWarning("⚠️ 网格中有大量空位置，可能坐标不连续");
        }
    }
#endif

#if UNITY_EDITOR
[ContextMenu("🔄 重新计算缩放Tile坐标")]
public void RecalculateScaledCoordinates()
{
    Debug.Log("开始重新计算考虑缩放的Tile坐标...");
    CollectManualTiles();
}

[ContextMenu("📐 显示所有Tile尺寸")]
public void ShowAllTileSizes()
{
    Tile[] allTiles = FindObjectsOfType<Tile>();
    Dictionary<Vector3, int> sizeCount = new Dictionary<Vector3, int>();
    
    foreach (Tile tile in allTiles)
    {
        Vector3 size = tile.GetTileSize();
        size = new Vector3(Mathf.Round(size.x * 100) / 100, Mathf.Round(size.y * 100) / 100, Mathf.Round(size.z * 100) / 100);
        
        if (!sizeCount.ContainsKey(size))
            sizeCount[size] = 0;
        sizeCount[size]++;
    }
    
    Debug.Log("=== Tile尺寸统计 ===");
    foreach (var kvp in sizeCount)
    {
        Debug.Log($"尺寸 {kvp.Key}: {kvp.Value} 个Tile");
    }
}
#endif

    void OnDrawGizmos()
    {
        if (!showGridGizmos || tiles == null) return;

        Gizmos.color = gridGizmoColor;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (tiles[x, z] != null)
                {
                    Vector3 pos = tiles[x, z].transform.position;
                    Gizmos.DrawWireCube(pos + Vector3.up * 0.1f, new Vector3(0.9f, 0.1f, 0.9f));

                    // 显示坐标标签
#if UNITY_EDITOR
                    Handles.Label(pos + Vector3.up * 0.5f, $"{tiles[x, z].x},{tiles[x, z].z}");
#endif
                }
            }
        }
    }
}

// 序列化类也需要使用外部的TileType
[System.Serializable]
public class TileTypeRow
{
    public TileType[] cols;  // 现在使用外部的TileType枚举

    public TileTypeRow() { cols = new TileType[0]; }
    public TileTypeRow(int width) { cols = new TileType[width]; }

    public void Resize(int width)
    {
        if (cols.Length != width)
        {
            var newCols = new TileType[width];
            for (int i = 0; i < Mathf.Min(width, cols.Length); i++)
                newCols[i] = cols[i];
            cols = newCols;
        }
    }
}