using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public float current = 0.0f;
    public float total = 0.0f;

    public void Tick(float dt)
    {
        current += dt;
    }

    public bool Expired()
    {
        return current >= total;
    }

    public void Reset()
    {
        current = 0.0f;
    }
}
