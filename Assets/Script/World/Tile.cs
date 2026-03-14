using System;
using UnityEngine;
[Serializable]
public enum TerrainType : byte
{
    Pond = 0,
    River = 1,
    Grass = 2,
    Forest = 3,
    Savanna = 4,
}

[Serializable]
public struct Cell
{
    public TerrainType terrainType;
}