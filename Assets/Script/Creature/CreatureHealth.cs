using UnityEngine;
using DG.Tweening; 

[RequireComponent(typeof(EntityBody))]
public class CreatureHealth : MonoBehaviour
{
    private EntityBody entityBody;
    private SpriteRenderer sprite;

    [Header("Hit Feedback Settings")]
    public float hitDuration = 0.2f;
    public float shakeStrength = 0.2f;
    private Color hitColor = new Color32(255, 112, 112, 255);    
    private Color originalColor;

    public void Awake()
    {
        this.entityBody = GetComponent<EntityBody>();
        
        this.sprite = GetComponent<SpriteRenderer>();

        if (this.sprite != null)
        {
            this.originalColor = sprite.color;
        }
    }

    public void TakeDamage(int Damage)
    {   
        entityBody.Data.health -= Damage;

        transform.DOKill(complete: true);   
        sprite.DOKill(complete: true);

        if (entityBody.Data.health > 0)
        {
            transform.DOShakePosition(hitDuration, strength: shakeStrength, vibrato: 20);

            sprite.DOColor(hitColor, hitDuration / 2f).SetLoops(2, LoopType.Yoyo);
        }
        else
        {
            ResetState();
            entityBody.RemoveEntity();
        }
    }

    private void OnDisable()
    {
        ResetState();
    }

    private void ResetState()
    {
        transform.DOKill();
        sprite.DOKill();

        if (sprite != null)
        {
            sprite.color = originalColor;
        }

    }
}