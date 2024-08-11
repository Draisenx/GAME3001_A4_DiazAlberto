using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    NONE,
    PLAYER,
    ENEMY
}

public class Bullet : MonoBehaviour
{
    public UnitType type = UnitType.NONE;
    public float damage;

    void Start()
    {
        Color[] colors = { Color.grey, Color.green, Color.red };
        GetComponent<SpriteRenderer>().color = colors[(int)type];
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Waypoint")) return;

        if ((collision.CompareTag("Enemy") && type == UnitType.PLAYER) ||
            (collision.CompareTag("Player") && type == UnitType.ENEMY))
        {
            collision.GetComponent<Health>().Damage(damage);
        }

        Destroy(gameObject);
    }
}
