using UnityEngine;

public enum EntityType
{
    Tree, Bush, 
    Bull, Wolf
}

[System.Serializable]
public class EntityData
{
    public EntityType type;
    public Vector2 worldPosition; 
    public int health;

    public int aiState; 
    public int herdID = -1;
    public EntityData(EntityType type, Vector2 pos, int hp = 10)
    {
        this.type = type;
        this.worldPosition = pos;
        this.health = hp;
    }
}