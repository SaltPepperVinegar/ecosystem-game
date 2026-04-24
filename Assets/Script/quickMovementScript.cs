using System.Collections;
using UnityEngine;

public class RandomWander : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minTime = 2f;
    [SerializeField] private float maxTime = 5f;

    [Header("Animations")]
    [SerializeField] private Animator anim;
    [SerializeField] private Transform visualTransform; 

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isMoving;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Safety check if not assigned in Inspector
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (visualTransform == null && anim != null) visualTransform = anim.transform;
        visualTransform = transform;
        StartCoroutine(WanderRoutine());
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            rb.linearVelocity = moveDirection * moveSpeed;
            
            if (moveDirection.x != 0)
            {
                float direction = moveDirection.x > 0 ? 1f : -1f;
                visualTransform.localScale = new Vector3(direction, 1, 1);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", rb.linearVelocity.magnitude);
        }
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            // STATE: STOP
            isMoving = false;
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));

            // STATE: WALK
            moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            isMoving = true;
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        }
    }
}