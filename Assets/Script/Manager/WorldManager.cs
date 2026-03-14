using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The central controller for the game world. Handles initialization, 
/// dynamic chunk loading/unloading based on player position, and data queries.
/// </summary>
public class WorldManager : MonoBehaviour
{
    [Header("System References")]
    public ChunkRenderer chunkRenderer;
    public Transform playerTransform;

    [Header("Generation Settings")]
    public bool useRandomSeed = true;
    public int mapSeed = 114514;
    public int renderRadius = 2; // Renders a (2n+1) x (2n+1) grid of chunks


    public WorldMap worldMap;
    public MapGenerator mapGenerator;

    // Tracking state for dynamic loading
    private Vector2Int lastPlayerChunk = new Vector2Int(-9999, -9999);
    private HashSet<Vector2Int> loadedChunkCoords = new HashSet<Vector2Int>();

    private void Start()
    {
        if (useRandomSeed)
        {
            mapSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Debug.Log("Generated World with Random Seed: " + mapSeed);
        }
        InitializeGame(mapSeed);
        UpdateVisibleChunks(); 
    }
        
    private void Update()
    {
        if (playerTransform == null) return;

        // Calculate the chunk the player is currently standing in
        Vector2Int currentPlayerChunk = worldMap.GetChunkCoordFromWorldPos(
            Mathf.FloorToInt(playerTransform.position.x),
            Mathf.FloorToInt(playerTransform.position.y)
        );

        // Only process chunk loading if the player crosses a chunk boundary
        if (currentPlayerChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentPlayerChunk;
            UpdateVisibleChunks();
        }
    }

    public void InitializeGame(int seed)
    {
        worldMap = new WorldMap();
        mapGenerator = new MapGenerator(seed);

        if (chunkRenderer != null)
        {
            chunkRenderer.SubscribeToWorld(worldMap);
        }
    }

    /// <summary>
    /// Evaluates which chunks should be active, generating missing ones 
    /// and unloading distant ones.
    /// </summary>
    private void UpdateVisibleChunks()
    {
        HashSet<Vector2Int> chunksThatShouldBeLoaded = new HashSet<Vector2Int>();

        //Load chunks within the render radius
        for (int x = -renderRadius; x <= renderRadius; x++)
        {
            for (int y = -renderRadius; y <= renderRadius; y++)
            {
                Vector2Int targetCoord = new Vector2Int(lastPlayerChunk.x + x, lastPlayerChunk.y + y);
                chunksThatShouldBeLoaded.Add(targetCoord);
                                if (!loadedChunkCoords.Contains(targetCoord))
                {
                    LoadChunk(targetCoord);
                }
            }
        }

        //Unload chunks that are now outside the radius
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (Vector2Int loadedCoord in loadedChunkCoords)
        {
            if (!chunksThatShouldBeLoaded.Contains(loadedCoord))
            {
                chunksToUnload.Add(loadedCoord);
            }
        }
        foreach (Vector2Int coord in chunksToUnload)
        {
            UnloadChunk(coord);
        }
    }

    private void LoadChunk(Vector2Int coord)
    {
        Chunk newChunk = mapGenerator.GenerateChunk(coord);
        worldMap.AddChunk(newChunk); 
        loadedChunkCoords.Add(coord);
    }

    private void UnloadChunk(Vector2Int coord)
    {
        chunkRenderer.ClearChunk(coord);
        loadedChunkCoords.Remove(coord);
        worldMap.activeChunks.Remove(coord); 
    }



    // ==========================================
    // Public API for Game Logic
    // ==========================================

    public Cell GetCellAt(int x, int y)
    {
        Vector2Int chunkCoord = worldMap.GetChunkCoordFromWorldPos(x, y);

        if (!worldMap.activeChunks.ContainsKey(chunkCoord))
        {
            LoadChunk(chunkCoord);
        }

        return worldMap.GetCellAt(x, y).Value;
    }
}