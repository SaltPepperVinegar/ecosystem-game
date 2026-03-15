using UnityEngine;

public class EntityBody : MonoBehaviour
{
    public EntityData Data { get; private set; }

    public void Initialize(EntityData data)
    {
        this.Data = data;
        // Here you can set health, visuals, or AI state based on data
        // e.g., GetComponent<HealthComponent>().currentHp = data.health;
    }

    public void PrepareForPool()
    {
        // Save current physical state back to data before recycling
        if (Data != null)
        {
            Data.worldPosition = transform.position;
            // Data.health = myCurrentHealth;
        }
    }

    public void RemoveEntity()
    {
        EntityManager.Instance.RemoveEntity(Data);
    }

}