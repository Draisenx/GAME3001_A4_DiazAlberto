using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    GameObject bulletPrefab;

    Health health;

    Rigidbody2D rb;
    const float moveForce = 50.0f;
    const float maxSpeed = 10.0f;
    Vector2 direction = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector2.up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            direction += Vector2.down;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector2.left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            direction += Vector2.right;
        }
        direction = direction.normalized;

        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0.0f;
        Vector3 mouseDirection = (mouse - transform.position).normalized;

        if (Input.GetMouseButtonDown(0))
            Utilities.CreateBullet(bulletPrefab, transform.position, mouseDirection, 15.0f, 25.0f, UnitType.PLAYER);

        // Respawn player if health is below zero
        if (health.health <= 0.0f)
            Respawn();
    }

    void FixedUpdate()
    {
        // Apply force based on input direction and reset for next input
        rb.AddForce(direction * moveForce);
        direction = Vector2.zero;

        // Limit velocity
        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    void Respawn()
    {
        health.health = Health.maxHealth;
        transform.position = new Vector3(0.0f, -3.0f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Shotgun"))
        {
            if (!WeaponSpawner.playerHasShotgun)
            {
                WeaponSpawner.playerHasShotgun = true;
                Destroy(collision.gameObject);
                Debug.Log("Picked up shotgun");
            }
        }
        else if (collision.CompareTag("Sniper"))
        {
            if (!WeaponSpawner.playerHasSniper)
            {
                WeaponSpawner.playerHasSniper = true;
                Destroy(collision.gameObject);
                Debug.Log("Picked up sniper");
            }
        }
    }
}
