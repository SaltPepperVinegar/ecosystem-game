using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HerdData
{
    public int herdID;
    public EntityType type;
    public Vector2 herdCenter;
    public Vector2 herdDestination;
    public List<EntityData> members = new List<EntityData>();
    
    public HerdData(int id, EntityType type)
    {
        this.herdID = id;
        this.type = type;
    }
}
