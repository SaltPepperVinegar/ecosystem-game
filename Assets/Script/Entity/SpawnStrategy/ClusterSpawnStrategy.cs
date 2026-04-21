using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClusterStrategy", menuName = "WorldGen/SpawnStrategy/Cluster")]
public class ClusterSpawnStrategy : SpawnStrategy
{
    [Header("Cluster Settings")]
    public int minCount = 2;
    public int maxCount = 5;
    public float clusterRadius = 2.0f;
    public bool isHerdable = true;
    public override List<EntityData> Execute(Chunk chunk, System.Random prng, TerrainType targetTerrain)
    {
        List<EntityData> spawned = new List<EntityData>();
        if (potentialEntities == null || potentialEntities.Count == 0) return spawned;

        List<Vector2Int> validTiles = chunk.biomeIndex[targetTerrain];
        if (validTiles.Count == 0) return spawned;

        // 1. Lambda: Expected number of CLUSTERS in this chunk
        float lambda = validTiles.Count * spawnChancePerTile;

        // 2. Poisson: Actual number of CLUSTERS to attempt
        int clusterCount = GetPoisson(lambda, prng);
        if (clusterCount == 0) return spawned;

        for (int c = 0; c < clusterCount; c++)
        {
            Vector2Int centerTile = validTiles[prng.Next(validTiles.Count)];
            EntityType typeToSpawn = GetRandomType(prng);

            int herdID = -1;
            if (isHerdable && HerdManager.Instance != null)
            {
                herdID = HerdManager.Instance.CreateHerd(typeToSpawn).herdID;
            }

            // How many entities inside this specific cluster?
            int amountInCluster = prng.Next(minCount, maxCount + 1);

            for (int i = 0; i < amountInCluster; i++)
            {
                float offsetX = (float)(prng.NextDouble() * 2 - 1) * clusterRadius;
                float offsetY = (float)(prng.NextDouble() * 2 - 1) * clusterRadius;

                float localX = centerTile.x + offsetX;
                float localY = centerTile.y + offsetY;

                int checkX = Mathf.FloorToInt(localX);
                int checkY = Mathf.FloorToInt(localY);

                // Ensure it stays in bounds and on the correct terrain
                if (checkX < 0 || checkX >= Chunk.Size || checkY < 0 || checkY >= Chunk.Size) continue;
                if (chunk.GetCell(checkX, checkY).terrainType != targetTerrain) continue;

                float worldX = (chunk.coordinate.x * Chunk.Size) + localX;
                float worldY = (chunk.coordinate.y * Chunk.Size) + localY;

                EntityData newEntity = new EntityData(typeToSpawn, new Vector2(worldX, worldY));

                if (herdID != -1)
                {
                    HerdManager.Instance.AddEntityToHerd(newEntity, herdID);
                }

                spawned.Add(newEntity);
            }
        }
        return spawned;
    }
}