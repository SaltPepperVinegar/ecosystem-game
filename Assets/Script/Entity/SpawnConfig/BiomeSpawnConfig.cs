using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeSpawnConfig", menuName = "WorldGen/BiomeConfig")]
public class BiomeSpawnConfig : ScriptableObject
{
    public TerrainType targetTerrain;
    public List<SpawnStrategy> strategies;
    
    public void PopulateChunk(Chunk chunk, System.Random prng)
    {
        if (!chunk.biomeIndex.ContainsKey(targetTerrain) || chunk.biomeIndex[targetTerrain].Count == 0)
        {
            return; 
        }
        foreach (var strategy in strategies)
        {
            Debug.Log("execute: ");
            strategy.Execute(chunk, prng, targetTerrain);
        }
    }
}