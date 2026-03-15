using UnityEngine;

[RequireComponent(typeof(EntityBody))]
public class CreatureHealth : MonoBehaviour
{
    private EntityBody entityBody;

    public void Awake()
    {
        this.entityBody = GetComponent<EntityBody>();
    }


    public void TakeDamage(int Damage)
    {   
        entityBody.Data.health -= Damage;
        if (entityBody.Data.health <= 0)
        {
            entityBody.RemoveEntity();
        }
    }


}
