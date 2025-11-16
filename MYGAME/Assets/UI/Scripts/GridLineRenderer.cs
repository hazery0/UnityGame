using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLineRenderer : MonoBehaviour
{
public int gridSize = 10;
    public float cellSize = 1f;
    public Color lineColor = Color.black;
    
    void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        
        // 绘制横向网格线
        for (int i = 0; i <= gridSize; i++)
        {
            float z = i * cellSize;
            Gizmos.DrawLine(new Vector3(0, 0.01f, z), new Vector3(gridSize * cellSize, 0.01f, z));
        }
        
        // 绘制纵向网格线
        for (int i = 0; i <= gridSize; i++)
        {
            float x = i * cellSize;
            Gizmos.DrawLine(new Vector3(x, 0.01f, 0), new Vector3(x, 0.01f, gridSize * cellSize));
        }
    }
}