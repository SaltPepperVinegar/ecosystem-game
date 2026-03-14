using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap
{
    public Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();
    public Vector2Int GetChunkCoordFromWorldPos(int worldX, int worldY)
    {
        int cx = Mathf.FloorToInt((float)worldX / Chunk.Size);
        int cy = Mathf.FloorToInt((float)worldY / Chunk.Size);
        return new Vector2Int(cx, cy);
    }

    public event Action<Chunk> OnChunkRefreshed;

    public Vector2Int GetLocalCoordFromWorldPos(int worldX, int worldY)
    {
        int lx = worldX % Chunk.Size;
        int ly = worldY % Chunk.Size;

        if (lx < 0) lx += Chunk.Size;
        if (ly < 0) ly += Chunk.Size;

        return new Vector2Int(lx, ly);
    }
    public void AddChunk(Chunk chunk)
    {
        activeChunks[chunk.coordinate] = chunk;
        OnChunkRefreshed?.Invoke(chunk);
    }

    public Cell? GetCellAt(int worldX, int worldY)
    {
        Vector2Int chunkCoord = GetChunkCoordFromWorldPos(worldX, worldY);

        if (activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            Vector2Int localCoord = GetLocalCoordFromWorldPos(worldX, worldY);
            return chunk.GetCell(localCoord.x, localCoord.y);
        }


        return null;
    }
    
    public void ModifyCellTerrain(int worldX, int worldY, TerrainType newTerrain)
    {
        Vector2Int chunkCoord = GetChunkCoordFromWorldPos(worldX, worldY);
        if (activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            Vector2Int localCoord = GetLocalCoordFromWorldPos(worldX, worldY);
            Cell cell = chunk.GetCell(localCoord.x, localCoord.y);
            
            if (cell.terrainType != newTerrain)
            {
                cell.terrainType = newTerrain;
                chunk.SetCell(localCoord.x, localCoord.y, cell);
                
                // Trigger the event so the visuals update automatically
                OnChunkRefreshed?.Invoke(chunk); 
            }
        }
    }
}