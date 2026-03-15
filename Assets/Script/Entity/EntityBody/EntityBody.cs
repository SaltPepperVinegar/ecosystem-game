using UnityEngine;

public class EntityBody : MonoBehaviour
{
    public EntityData Data { get; private set; }

    public void Initialize(EntityData data)
    {
        this.Data = data;
    }

    public void PrepareForPool()
    {
        // Save current physical state back to data before recycling
        if (Data != null)
        {
            Data.worldPosition = transform.position;
        }
    }

    public void RemoveEntity()
    {
        EntityManager.Instance.RemoveEntity(Data);
    }

}