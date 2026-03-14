using System.Collections;
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
    public EntityManager entityManager;
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
    private Coroutine currentChunkUpdate;
    public List<BiomeSpawnConfig> biomeConfigs;

    private void Start()
    {
        if (useRandomSeed)
        {
            mapSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Debug.Log("Generated World with Random Seed: " + mapSeed);
        }
        InitializeGame(mapSeed);
        // Only process chunk loading if the player crosses a chunk boundary
        if (playerTransform != null)
        {
            Vector3 safePos = FindSafeSpawnPosition(playerTransform.position);
            playerTransform.position = safePos;
        }
        
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
            
            if (currentChunkUpdate != null)
            {
                StopCoroutine(currentChunkUpdate);
            }
            
            
            currentChunkUpdate = StartCoroutine(UpdateVisibleChunks());     
        }
        
    }

    public void InitializeGame(int seed)
    {
        worldMap = new WorldMap();
        mapGenerator = new MapGenerator(seed, biomeConfigs);

        if (chunkRenderer != null)
        {
            chunkRenderer.SubscribeToWorld(worldMap);
        }
        if (entityManager != null)
        {
            entityManager.SubscribeToWorld(worldMap);
        }
    }

    /// <summary>
    /// Evaluates which chunks should be active, generating missing ones 
    /// and unloading distant ones. Spreads generation across multiple frames to prevent lag.
    /// </summary>
    private IEnumerator UpdateVisibleChunks()
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
                    
                    // Pause execution here until the next frame
                    yield return null;
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
        if (worldMap.archivedChunks.TryGetValue(coord, out Chunk existingChunk))
        {
            worldMap.AddChunk(existingChunk);
        } else
        {
            Chunk newChunk = mapGenerator.GenerateChunk(coord);
            worldMap.AddChunk(newChunk);
        }

        loadedChunkCoords.Add(coord);
    }

    private void UnloadChunk(Vector2Int coord)
    {
        if(worldMap.activeChunks.TryGetValue(coord, out Chunk chunk))
        {

            worldMap.UnloadChunk(chunk);
            loadedChunkCoords.Remove(coord);

        }

    }

    private Vector3 FindSafeSpawnPosition(Vector3 startPos)
    {
        int startX = Mathf.FloorToInt(startPos.x);
        int startY = Mathf.FloorToInt(startPos.y);
        int maxSearchRadius = 50;

        for (int radius = 0; radius < maxSearchRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                    {
                        int checkX = startX + x;
                        int checkY = startY + y;
                        
                        TerrainType terrain = GetCellAt(checkX, checkY).terrainType;

                        if (terrain != TerrainType.River && terrain != TerrainType.Pond)
                        {
                            return new Vector3(checkX + 0.5f, checkY + 0.5f, 0f);
                        }
                    }
                }
            }
        }

        Debug.LogWarning("cannot find a safe spawn point ");

        return startPos;
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