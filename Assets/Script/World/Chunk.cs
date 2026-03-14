using System;
using UnityEngine;

[Serializable]
public class Chunk
{
    public const int Size = 16;
    
    public Vector2Int coordinate; 
    public Cell[] cells;
    
    public Chunk(Vector2Int coord)
    {
        coordinate = coord;
        cells = new Cell[Size * Size];
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
        cells[GetIndex(localX, localY)] = newCell;
    }
}