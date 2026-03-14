using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScatterStrategy", menuName = "WorldGen/SpawnStrategy/Scatter")]
public class ScatterSpawnStrategy : SpawnStrategy
{
    [Header("Scatter Settings")]
    [Tooltip("Max random offset from the center of the tile")]
    public float maxOffset = 0.4f; 

    public override void Execute(Chunk chunk, System.Random prng, TerrainType targetTerrain)
    {
        if (potentialEntities == null || potentialEntities.Count == 0) return;

        List<Vector2Int> validTiles = chunk.biomeIndex[targetTerrain];
        if (validTiles.Count == 0) return;

        // 1. Lambda: Expected number of individuals in this chunk
        float lambda = validTiles.Count * spawnChancePerTile;

        // 2. Poisson: Actual number to spawn this time
        int spawnCount = GetPoisson(lambda, prng);
        if (spawnCount == 0) return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2Int tile = validTiles[prng.Next(validTiles.Count)];
            EntityType typeToSpawn = GetRandomType(prng);

            // Add a slight offset so they aren't perfectly grid-aligned
            float offsetX = (float)(prng.NextDouble() * 2 - 1) * maxOffset;
            float offsetY = (float)(prng.NextDouble() * 2 - 1) * maxOffset;

            float worldX = (chunk.coordinate.x * Chunk.Size) + tile.x + offsetX;
            float worldY = (chunk.coordinate.y * Chunk.Size) + tile.y + offsetY;

            chunk.entities.Add(new EntityData(typeToSpawn, new Vector2(worldX, worldY)));
        }
    }
}