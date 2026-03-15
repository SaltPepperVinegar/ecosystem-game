using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform gun;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector3 mousePos;

    private Vector3 cursorOffset = new Vector3(0.5f, -0.5f, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnShoot(InputValue value)
    {
        
    }

    void OnMouseMove(InputValue value)
    {
        mousePos = Camera.main.ScreenToWorldPoint(value.Get<Vector2>()) + cursorOffset;
        mousePos.z = 0;
        gun.rotation = Quaternion.LookRotation(gun.position - mousePos, -Vector3.forward);
    }
}
