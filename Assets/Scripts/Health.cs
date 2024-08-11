using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public const float maxHealth = 100.0f;
    public float health = maxHealth;

    public void Damage(float damage)
    {
        health -= damage;
    }
}
