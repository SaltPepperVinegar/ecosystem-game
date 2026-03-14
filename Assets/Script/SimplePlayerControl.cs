using UnityEngine;
using UnityEngine.InputSystem;



// Temporary direct keyboard input.
// This will be replaced later with proper Input Actions / PlayerInput setup.

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed)
            moveInput.y -= 1f;
        if (Keyboard.current.aKey.isPressed)
            moveInput.x -= 1f;
        if (Keyboard.current.dKey.isPressed)
            moveInput.x += 1f;

        moveInput = moveInput.normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}