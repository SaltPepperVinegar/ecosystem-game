using System;
using UnityEngine;
using UnityEngine.Events;

public class CollisionPassthrough : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<GameObject, Collider2D> triggerEnterEvent;

    private void OnTriggerEnter2D(Collider2D other)
    {
        triggerEnterEvent.Invoke(gameObject, other);
    }
    private void OnDestroy()
    {
        triggerEnterEvent.RemoveAllListeners();
    }
}
