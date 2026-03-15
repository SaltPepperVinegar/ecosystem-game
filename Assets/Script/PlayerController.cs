using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f, bulletSpeed = 15.0f;
    [SerializeField] private Transform gun;
    [SerializeField] private GameObject bulletGo;
    [SerializeField] private int enemyLayer;
    [SerializeField] private TMP_Text moneyUI;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector3 mousePos;
    private float money = 0;

    private readonly Vector3 cursorOffset = new Vector3(0.5f, -0.5f, 0);
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
        GameObject bullet = Instantiate(bulletGo, gun.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = -gun.forward * bulletSpeed;
        bullet.GetComponent<CollisionPassthrough>().triggerEnterEvent.AddListener(BulletTriggerHit);
        Destroy(bullet, 10.0f);
    }

    void OnMouseMove(InputValue value)
    {
        mousePos = Camera.main.ScreenToWorldPoint(value.Get<Vector2>()) + cursorOffset;
        mousePos.z = 0;
        gun.rotation = Quaternion.LookRotation(gun.position - mousePos, -Vector3.forward);
    }

    void BulletTriggerHit(GameObject bullet, Collider2D other)
    {
        if (other.gameObject.layer == enemyLayer)
        {
            Destroy(other.gameObject);
            Destroy(bullet);

            money += 10;
            moneyUI.text = "$" + money;
        }
    }
}
