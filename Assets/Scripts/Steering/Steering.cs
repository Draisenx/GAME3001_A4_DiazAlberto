using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SteeringBehaviour
{
    SEEK,
    FLEE,
    COUNT
}

public class Steering : MonoBehaviour
{
    public static Vector3 Seek(Rigidbody2D seeker, Vector2 target, float speed)
    {
        Vector2 current = seeker.velocity;
        Vector2 desired = (target - seeker.position).normalized * speed;
        return desired - current;
    }

    public static Vector3 Flee(Rigidbody2D seeker, Vector2 target, float speed)
    {
        Vector2 current = seeker.velocity;
        Vector2 desired = (seeker.position - target).normalized * speed;
        return desired - current;
    }

    public static Vector3 Avoid(Rigidbody2D seeker, float speed, float rayLength, float rayAngle = 15.0f, bool drawRays = false)
    {
        Transform transform = seeker.transform;
        Vector3 left = Quaternion.Euler(0.0f, 0.0f,   rayAngle) * transform.right;
        Vector3 right = Quaternion.Euler(0.0f, 0.0f, -rayAngle) * transform.right;
        if (Physics2D.Raycast(transform.position, left, rayLength))
        {
            return Seek(seeker, transform.position - transform.up * rayLength, speed);
        }
        if (Physics2D.Raycast(transform.position, right, rayLength))
        {
            return Seek(seeker, transform.position + transform.up * rayLength, speed);
        }

        if (drawRays)
        {
            Debug.DrawLine(transform.position, transform.position + left * rayLength, Color.cyan);
            Debug.DrawLine(transform.position, transform.position + right * rayLength, Color.cyan);
        }
        
        return Vector3.zero;
    }

    public static float RotateTowardsVelocity(Rigidbody2D seeker, float turnSpeed, float deltaTime)
    {
        float currentRotation = seeker.rotation;
        float desiredRotation = Vector2.SignedAngle(Vector3.right, seeker.velocity.normalized);
        float deltaRotation = turnSpeed * deltaTime;
        float rotation = Mathf.MoveTowardsAngle(currentRotation, desiredRotation, deltaRotation);
        return rotation;
    }

    SteeringBehaviour behaviour = SteeringBehaviour.SEEK;
    Rigidbody2D rb;
    float moveSpeed = 10.0f;
    float turnSpeed = 1080.0f;
    float rayLength = 2.5f;
    float rayAngle = 15.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Acquire target (world-space mouse cursor)
        Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0.0f;

        // Seek or flee target
        Vector3 steeringForce = Vector3.zero;
        switch (behaviour)
        {
            case SteeringBehaviour.SEEK:
                steeringForce += Seek(rb, target, moveSpeed);
                break;
        
            case SteeringBehaviour.FLEE:
                steeringForce += Flee(rb, target, moveSpeed);
                break;
        }

        // Summate & apply steering forces
        steeringForce += Seek(rb, target, moveSpeed);
        steeringForce += Avoid(rb, moveSpeed, rayLength, rayAngle, true);
        rb.AddForce(steeringForce);

        // Rotate towards the direction of motion (needed for avoidance probe orientation)
        float rotation = RotateTowardsVelocity(rb, turnSpeed, Time.deltaTime);
        rb.MoveRotation(rotation);

        // Switch steering behaviours when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int index = (int)behaviour;
            index++;
            index %= (int)SteeringBehaviour.COUNT;
            behaviour = (SteeringBehaviour)index;
        }
    }
}
