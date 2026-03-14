using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    [System.Serializable]
    public struct EntityPrefabMap {
        public EntityType type;
        public GameObject prefab;
    }

    public List<EntityPrefabMap> prefabLibrary;
    private WorldMap subscribedWorldMap;

    private Dictionary<EntityType, GameObject> typeToPrefab = new Dictionary<EntityType, GameObject>();

    private Dictionary<EntityType, Queue<GameObject>> pools = new Dictionary<EntityType, Queue<GameObject>>();
    private Dictionary<EntityData, GameObject> activeEntities = new Dictionary<EntityData, GameObject>();
    private Dictionary<Vector2Int, List<EntityData>> chunkToDataMap = new Dictionary<Vector2Int, List<EntityData>>();

    private void Awake() {
        foreach (var map in prefabLibrary) {
            pools[map.type] = new Queue<GameObject>();
            typeToPrefab[map.type] = map.prefab;
        }
        
    }

    public void SubscribeToWorld(WorldMap worldMap)
    {
        if (subscribedWorldMap != null)
        {
            subscribedWorldMap.OnChunkRefreshed -= SpawnEntitiesForChunk;
            subscribedWorldMap.OnChunkUnloaded -= DespawnEntitiesForChunk;
        }
        subscribedWorldMap = worldMap;
        subscribedWorldMap.OnChunkRefreshed += SpawnEntitiesForChunk;
        subscribedWorldMap.OnChunkUnloaded += DespawnEntitiesForChunk;

    }
    private void Update()
    {
        if (Time.frameCount % 10 == 0) 
        {
            PerformMigration();
        }
    }
    
    public void SpawnEntitiesForChunk(Chunk chunk) 
        {
            if (chunk.entities == null) return;

            foreach (EntityData data in chunk.entities) 
            {
                GameObject body = GetFromPool(data.type);
                body.transform.position = data.worldPosition;
                body.SetActive(true);

                activeEntities[data] = body;

                EntityBody entityScript = body.GetComponent<EntityBody>();
                if (entityScript != null)
                {
                    entityScript.Initialize(data);
                }
            }
        }

    public void DespawnEntitiesForChunk(Chunk chunk) 
    {
        if (chunk.entities == null) return;

        foreach (EntityData data in chunk.entities) 
        {
            if (activeEntities.TryGetValue(data, out GameObject body)) 
            {
                EntityBody entityScript = body.GetComponent<EntityBody>();
                if (entityScript != null)
                {
                    entityScript.PrepareForPool();
                }

                body.SetActive(false);
                pools[data.type].Enqueue(body);

                activeEntities.Remove(data);
            }
        }
    }
    public void RemoveEntity(EntityData data)
    {
        if (activeEntities.TryGetValue(data, out GameObject body))
        {
            // Return body to pool
            body.SetActive(false);
            pools[data.type].Enqueue(body);
            activeEntities.Remove(data);
        }

        // Remove soul from the data index
        Vector2Int coord = subscribedWorldMap.GetChunkCoordFromWorldPos(
            Mathf.FloorToInt(data.worldPosition.x),
            Mathf.FloorToInt(data.worldPosition.y)
        );

        if (subscribedWorldMap.activeChunks.TryGetValue(coord, out Chunk chunk))
        {
            chunk.entities.Remove(data);
        }
    }

    private GameObject GetFromPool(EntityType type) {
        if (pools[type].Count > 0) return pools[type].Dequeue();
        if (typeToPrefab.TryGetValue(type, out GameObject prefab)) {
                GameObject newObj = Instantiate(prefab, transform);
                return newObj;
        }
        Debug.LogError($"Prefab for {type} not found!");
        return null;    
    }



    private void PerformMigration()
    {
        foreach (var pair in activeEntities)
        {
            EntityData data = pair.Key;
            GameObject body = pair.Value;

            Vector2Int currentCoord = subscribedWorldMap.GetChunkCoordFromWorldPos(
                Mathf.FloorToInt(body.transform.position.x),
                Mathf.FloorToInt(body.transform.position.y)
            );

            Vector2Int recordedCoord = subscribedWorldMap.GetChunkCoordFromWorldPos(
                Mathf.FloorToInt(data.worldPosition.x),
                Mathf.FloorToInt(data.worldPosition.y)
            );
            if (currentCoord != recordedCoord)
            {
                data.worldPosition = body.transform.position;
                
                MoveEntityDataBetweenChunks(data, recordedCoord, currentCoord);
            }
        }
    }

    private void MoveEntityDataBetweenChunks(EntityData data, Vector2Int from, Vector2Int to)
    {
        if (subscribedWorldMap.activeChunks.TryGetValue(from, out Chunk fromChunk) &&
                subscribedWorldMap.activeChunks.TryGetValue(to, out Chunk toChunk))
            {
                fromChunk.entities.Remove(data);
                toChunk.entities.Add(data);
            }
    }

    private void OnDestroy()
    {
        if (subscribedWorldMap != null)
        {
            subscribedWorldMap.OnChunkRefreshed -= SpawnEntitiesForChunk;
            subscribedWorldMap.OnChunkUnloaded -= DespawnEntitiesForChunk;

        }
    }
}