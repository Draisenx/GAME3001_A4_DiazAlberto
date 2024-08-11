using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionTreeTheory : MonoBehaviour
{
    public bool isVisible;
    public bool isAudible;
    public bool isNear;
    public bool isFlank;

    void Creep()
    {
        Debug.Log("Creeping. . .");
    }

    void Attack()
    {
        Debug.Log("Boom! Headshot.");
    }

    void Move()
    {
        Debug.Log("Gotta go fast!!! *Distorted Sonic noises intensify*");
    }

    void Traverse()
    {
        if (isVisible)
        {
            if (isNear)
            {
                Attack();
            }
            else
            {
                if (isFlank)
                {
                    Move();
                }
                else
                {
                    Attack();
                }
            }
        }
        else
        {
            if (isAudible)
            {
                Creep();
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        Traverse();
    }
}
