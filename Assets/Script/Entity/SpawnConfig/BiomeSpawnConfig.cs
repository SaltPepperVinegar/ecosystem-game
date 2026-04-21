using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeSpawnConfig", menuName = "WorldGen/BiomeConfig")]
public class BiomeSpawnConfig : ScriptableObject
{
    public TerrainType targetTerrain;
    public List<SpawnStrategy> strategies;
    
    public List<EntityData> PopulateChunk(Chunk chunk, System.Random prng)
    {
        List<EntityData> spawned = new List<EntityData>();
        if (!chunk.biomeIndex.ContainsKey(targetTerrain) || chunk.biomeIndex[targetTerrain].Count == 0)
        {
            return spawned; 
        }
        foreach (var strategy in strategies)
        {
            spawned.AddRange(strategy.Execute(chunk, prng, targetTerrain));
        }
        return spawned;
    }
}