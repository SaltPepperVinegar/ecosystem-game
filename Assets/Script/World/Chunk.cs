using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Chunk
{
    public const int Size = 16;
    
    public Vector2Int coordinate; 
    public Cell[] cells;

    public Dictionary<TerrainType, List<Vector2Int>> biomeIndex = new Dictionary<TerrainType, List<Vector2Int>>();
    public List<EntityData> entities = new List<EntityData>();
    
    public Chunk(Vector2Int coord)
    {
        coordinate = coord;
        cells = new Cell[Size * Size];
        foreach (TerrainType type in System.Enum.GetValues(typeof(TerrainType)))
        {
            biomeIndex[type] = new List<Vector2Int>();
        }
        
    }
    public int GetIndex(int localX, int localY)
    {
        return localX + localY * Size;
        
    }

    public Cell GetCell(int localX, int localY)
    {
        return cells[GetIndex(localX, localY)];
    }

    public void SetCell(int localX, int localY, Cell newCell)
    {   
        biomeIndex[newCell.terrainType].Add(new Vector2Int(localX, localY));

        cells[GetIndex(localX, localY)] = newCell;
    }

    public void RebuildIndex()
    {
        foreach (var list in biomeIndex.Values) list.Clear();
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                TerrainType type = GetCell(x, y).terrainType;
                biomeIndex[type].Add(new Vector2Int(x, y));
            }
        }
    }
}