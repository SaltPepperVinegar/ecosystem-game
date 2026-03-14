using UnityEngine;
using UnityEngine.Tilemaps;


/// Handles the visual representation of Chunk data using unity Tilemap
public class ChunkRenderer : MonoBehaviour
{
    public Tilemap targetTilemap;

    [Header("Tile Assets")]
    [Tooltip("Assign tiles matching the integer value of the TerrainType enum")]
    public TileBase[] terrainTiles;
    private WorldMap subscribedWorldMap;

    public void SubscribeToWorld(WorldMap worldMap)
    {
        if (subscribedWorldMap != null)
        {
            subscribedWorldMap.OnChunkRefreshed -= RenderChunk;
        }
        subscribedWorldMap = worldMap;
        subscribedWorldMap.OnChunkRefreshed += RenderChunk;
    }

    public void RenderChunk(Chunk chunk)
    {
        if (chunk == null || targetTilemap == null) return;

        for (int y = 0; y < Chunk.Size; y++)
        {
            for (int x = 0; x < Chunk.Size; x++)
            {
                Cell cell = chunk.GetCell(x, y);

                int worldX = chunk.coordinate.x * Chunk.Size + x;
                int worldY = chunk.coordinate.y * Chunk.Size + y;
                Vector3Int tilePosition = new Vector3Int(worldX, worldY, 0);

                TileBase tileToDraw = GetTileForTerrain(cell.terrainType);

                targetTilemap.SetTile(tilePosition, tileToDraw);
            }
        }
    }
    public void ClearChunk(Vector2Int chunkCoord)
    {
        if (targetTilemap == null) return;

        Vector3Int startPos = new Vector3Int(chunkCoord.x * Chunk.Size, chunkCoord.y * Chunk.Size, 0);
        BoundsInt bounds = new BoundsInt(startPos.x, startPos.y, 0, Chunk.Size, Chunk.Size, 1);

        TileBase[] nullTiles = new TileBase[Chunk.Size * Chunk.Size];

        targetTilemap.SetTilesBlock(bounds, nullTiles);

        targetTilemap.CompressBounds();
    }

    // Retrieves the TileBase associated with a specific TerrainType.
    private TileBase GetTileForTerrain(TerrainType type)
    {
        int index = (int)type;

        if (index >= 0 && index < terrainTiles.Length)
        {
            return terrainTiles[index];
        }

        return null;
    }
    

    private void OnDestroy()
    {
        if (subscribedWorldMap != null)
        {
            subscribedWorldMap.OnChunkRefreshed -= RenderChunk;
        }
    }
}