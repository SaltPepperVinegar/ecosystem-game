using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnStrategy : ScriptableObject
{
    [System.Serializable]
    public class SpawnableEntity
    {
        public EntityType type;
        public float weight = 1.0f;
    }

    public List<SpawnableEntity> potentialEntities;

    [Range(0f, 1f)]
    [Tooltip("Expected number of spawns per valid tile (e.g., 0.02 = 2 per 100 tiles)")]
    public float spawnChancePerTile;

    public abstract List<EntityData> Execute(Chunk chunk, System.Random prng, TerrainType targetTerrain);

    protected EntityType GetRandomType(System.Random prng)
    {
        if (potentialEntities == null || potentialEntities.Count == 0) return default;

        float totalWeight = 0;
        foreach (var entity in potentialEntities) totalWeight += entity.weight;
        if (totalWeight <= 0) return potentialEntities[prng.Next(potentialEntities.Count)].type;

        float pick = (float)prng.NextDouble() * totalWeight;
        float cumulativeWeight = 0;

        foreach (var entity in potentialEntities)
        {
            cumulativeWeight += entity.weight;
            if (pick <= cumulativeWeight) return entity.type;
        }

        return potentialEntities[0].type;
    }

    protected int GetPoisson(float lambda, System.Random prng)
    {
        double L = System.Math.Exp(-lambda);
        double p = 1.0;
        int k = 0;

        do
        {
            k++;
            p *= prng.NextDouble();
        } while (p > L);

        return k - 1;
    }
}