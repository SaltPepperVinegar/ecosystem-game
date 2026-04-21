using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    public int seed;

    //Scale of the noise. Lower values create larger, more contiguous terrain features.
    public float scale = 0.07f;

    // Random offsets to break the natural symmetry of Mathf.PerlinNoise at (0,0).
    private float seedOffsetX;
    private float seedOffsetY;

    private List<BiomeSpawnConfig> biomeConfigs;

    public MapGenerator(int inputSeed, List<BiomeSpawnConfig> biomeConfigs)
    {
        this.biomeConfigs = biomeConfigs;
        seed = inputSeed;
        System.Random randomGenerator = new System.Random(seed);
        seedOffsetX = randomGenerator.Next(-100000, 100000);
        seedOffsetY = randomGenerator.Next(-100000, 100000);
    }


    // Generates terrain data for an entire chunk based on its coordinate.
    public Chunk GenerateChunk(Vector2Int chunkCoord)
    {
        Chunk newChunk = new Chunk(chunkCoord);

        // Generate terrain 
        for (int y = 0; y < Chunk.Size; y++)
        {
            for (int x = 0; x < Chunk.Size; x++)
            {
                int worldX = chunkCoord.x * Chunk.Size + x;
                int worldY = chunkCoord.y * Chunk.Size + y;

                TerrainType type = EvaluateTerrain(worldX, worldY);
                Cell cell = new Cell { terrainType = type };
                newChunk.SetCell(x, y, cell);
            }
        } 
        // Populate entities 
        System.Random prng = new System.Random(seed + chunkCoord.GetHashCode());

        if (biomeConfigs != null)
        {
            List<EntityData> chunkEntities = new List<EntityData>();
            foreach (BiomeSpawnConfig config in biomeConfigs)
            {   
                chunkEntities.AddRange(config.PopulateChunk(newChunk, prng));
            }

            if (EntityManager.Instance != null && chunkEntities.Count > 0)
            {
                EntityManager.Instance.RegisterEntities(chunkEntities);
            }
        }
        
        return newChunk;
    }


    // Evaluates the terrain type for a specific world coordinate using Perlin noise.
    private TerrainType EvaluateTerrain(int worldX, int worldY)
    {
        // Calculate sample coordinates with seed offsets.
        float sampleX = worldX * scale + seedOffsetX;
        float sampleY = worldY * scale + seedOffsetY;

        //Base terrain noise (Range: 0.0 to 1.0).
        float baseNoise = Mathf.PerlinNoise(sampleX, sampleY);

        float riverScale = scale * 2.0f;
        float riverValue = Mathf.PerlinNoise(worldX * riverScale + seedOffsetX + 5000f,
                                            worldY * riverScale + seedOffsetY + 5000f);
        // Standard Ridge Noise
        float riverPath = Mathf.Abs(riverValue - 0.5f);

        // River Mask 
        float maskValue = Mathf.PerlinNoise(worldX * scale * 0.6f + seedOffsetX + 8000f,
                                            worldY * scale * 0.6f + seedOffsetY + 8000f);

        float finalRiverValue = riverPath + (1.0f - maskValue) * 0.05f;
        if (baseNoise >= 0.00f && baseNoise < 0.10f)
        {
            return TerrainType.Pond;
        }
        if (IsRiver(worldX, worldY, baseNoise))
        {
            return TerrainType.River;
        }
        if (baseNoise >= 0.15f && baseNoise < 0.40f)
        {
            return TerrainType.Grass;
        }
        if (baseNoise >= 0.40f && baseNoise <= 0.75f)
        {
            return TerrainType.Savanna;
        }
        if (baseNoise > 0.75f)
        {
            return TerrainType.Forest;
        }

        return TerrainType.Savanna;
    }
    
    private bool IsRiver(
        int worldX,
        int worldY,
        float baseNoise,
        float riverFrequencyMultiplier = 0.35f,
        float riverRarity = 0.35f,     // 0 = common, 1 = rare              
        float riverMinWidth = 0.015f,
        float riverMaxWidth = 0.045f,
        float riverMinElevation = 0.18f,
        float riverMaxElevation = 0.72f,
        float riverMaskFrequencyMultiplier = 0.25f,
        float riverWidthFrequencyMultiplier = 0.5f
    )
    {
        float riverFreq = scale * riverFrequencyMultiplier;

        // Main river field
        float riverNoise = Mathf.PerlinNoise(
            worldX * riverFreq + seedOffsetX + 5000f,
            worldY * riverFreq + seedOffsetY + 5000f
        );

        float riverDistance = Mathf.Abs(riverNoise - 0.5f);
        float widthNoise = Mathf.PerlinNoise(
            worldX * riverFreq * riverWidthFrequencyMultiplier + seedOffsetX + 9000f,
            worldY * riverFreq * riverWidthFrequencyMultiplier + seedOffsetY + 9000f
        );

        float riverWidth = Mathf.Lerp(riverMinWidth, riverMaxWidth, widthNoise);

        float maskNoise = Mathf.PerlinNoise(
            worldX * riverFreq * riverMaskFrequencyMultiplier + seedOffsetX + 13000f,
            worldY * riverFreq * riverMaskFrequencyMultiplier + seedOffsetY + 13000f
        );

        float rarityThreshold = Mathf.Lerp(0.15f, 0.75f, riverRarity);

        if (baseNoise < riverMinElevation || baseNoise > riverMaxElevation)
            return false;

        if (maskNoise < rarityThreshold)
            return false;

        return riverDistance < riverWidth;
    }
}