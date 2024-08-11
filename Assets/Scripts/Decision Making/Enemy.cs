using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType : int
{
    NONE,
    SHOTGUN,
    SNIPER
}

public class Enemy : MonoBehaviour
{
    [SerializeField]
    Transform player;

    Health health;
    Rigidbody2D rb;

    [SerializeField]
    Transform[] waypoints;
    int waypoint = 0;

    const float moveSpeed = 7.5f;
    const float turnSpeed = 1080.0f;
    const float viewDistance = 5.0f;

    [SerializeField]
    GameObject bulletPrefab;
    Timer shootCooldown = new Timer();

    [SerializeField]
    GameObject shotgunPrefab;
    [SerializeField]
    GameObject sniperPrefab;

    WeaponType weaponType = WeaponType.NONE;

    bool hasShotgun = false;
    bool hasSniper = false;
    float weaponSwitchTime = 5.0f; // Time to switch between sniper and shotgun when both are equipped

    //const float cooldownOffensive = 0.05f;
    //const float cooldownOffensive = 0.05f;
    const float cooldownSniper = 0.75f;
    const float cooldownShotgun = 0.25f;

    float neutralStateTimer = 0f;
    const float neutralStateDuration = 1.0f;

    enum State
    {
        NEUTRAL,
        OFFENSIVE,
        DEFENSIVE
    };

    State statePrev, stateCurr;
    Color color;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        Respawn();

        //shootCooldown.total = 0.25f;

        //statePrev = stateCurr = State.NEUTRAL;
        //OnTransition(stateCurr));
    }

    void Update()
    {
        float rotation = Steering.RotateTowardsVelocity(rb, turnSpeed, Time.deltaTime);
        rb.MoveRotation(rotation);

        // Respawn enemy if health is below zero
        if (health.health <= 0.0f)
            Respawn();

        // Test defensive transition by reducing to 1/4th health!
        if (Input.GetKeyDown(KeyCode.T))
        {
            health.health *= 0.25f;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            int type = (int)weaponType;
            type++;
            type = type % 3;
            weaponType = (WeaponType)type;
            Debug.Log($"Weapon type changed to: {weaponType}");  // Debug weapon type change
        }

        // State-selection
        if (stateCurr != State.DEFENSIVE)
        {
            float playerDistance = Vector2.Distance(transform.position, player.position);
            stateCurr = playerDistance <= viewDistance ? State.OFFENSIVE : State.NEUTRAL;

            // Transition to defensive state if we're below 25% health
            if (health.health <= Health.maxHealth * 0.25f)
                stateCurr = State.DEFENSIVE;
        }

        // State-specific transition
        if (stateCurr != statePrev)
            OnTransition(stateCurr);

        // Add timer for neutral state
        if (stateCurr == State.NEUTRAL)
        {
            neutralStateTimer += Time.deltaTime;
            if (neutralStateTimer > neutralStateDuration)
            {
                SeekVisibility();  // Seek nearest point of visibility with line of sight to the player
                neutralStateTimer = 0f; // Reset timer after seeking
            }
        }

        // Check if both weapons are equipped and alternate between them
        if (hasShotgun && hasSniper)
        {
            weaponSwitchTime -= Time.deltaTime;
            if (weaponSwitchTime <= 0)
            {
                weaponType = (weaponType == WeaponType.SHOTGUN) ? WeaponType.SNIPER : WeaponType.SHOTGUN;
                weaponSwitchTime = 5.0f;
                Debug.Log($"Weapon switched to: {weaponType}");  // Debug weapon switch
            }
        }

        // State-specific update
        switch (stateCurr)
        {
            case State.NEUTRAL:
                Patrol();
                break;

            case State.OFFENSIVE:
                if (weaponType == WeaponType.NONE)
                {
                    Defend();  // Flee when the enemy has no weapons
                }
                else if (IsInPlayerDetectionRadius())
                {
                    Attack();
                }
                else
                {
                    SeekVisibility();  // Move toward a waypoint with line of sight to the player
                }
                break;

            case State.DEFENSIVE:
                if (!IsInPlayerDetectionRadius())
                {
                    SeekCover();  // Seek the cover-point (waypoint with no line of sight to the player) furthest from the player
                }
                else
                {
                    Defend();  // Flee and shoot the player if within detection radius
                }
                break;
        }

        // If you're feeling adventurous, change this to apply force within fixed update based on state!
        statePrev = stateCurr;
        Debug.DrawLine(transform.position, transform.position + transform.right * viewDistance, color);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // You might want to add an EnemyBullet vs PlayerBullet tag, maybe even remove the Bullet tag.
        if (collision.CompareTag("Bullet"))
        {
            // TODO -- damage enemy if it gets hit with a *Player* bullet
            // (be careful not to damage the enemy if it collides with its own bullets)
        }
        else if (collision.CompareTag("Shotgun") || collision.CompareTag("Sniper"))
        {
            // Example: Assign weapon to the enemy
            if (collision.CompareTag("Shotgun"))
            {
                if (!hasShotgun) // Ensure the enemy hasn't already picked up a shotgun
                {
                    hasShotgun = true;
                    weaponType = hasSniper ? WeaponType.SNIPER : WeaponType.SHOTGUN;
                    Destroy(collision.gameObject);
                }
            }
            else if (collision.CompareTag("Sniper"))
            {
                if (!hasSniper) // Ensure the enemy hasn't already picked up a sniper
                {
                    hasSniper = true;
                    weaponType = hasSniper ? WeaponType.SNIPER : WeaponType.SHOTGUN;
                    Destroy(collision.gameObject);
                }
            }
        }

        // If we seek the nearest waypoint WHILE being inside of the nearest waypoint,
        // we get stuck seeking the same waypoint forever XD
        // The solution is to change our patrolling system to be distance-based
        //if (collision.CompareTag("Waypoint"))
        //{
        //    waypoint++;
        //    waypoint %= waypoints.Length;
        //}
    }

    void SeekVisibility()
    {
        int nearestVisibleWaypoint = -1;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (HasLineOfSightToPlayer(waypoints[i].position))
            {
                float distance = Vector2.Distance(transform.position, waypoints[i].position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestVisibleWaypoint = i;
                }
            }
        }

        if (nearestVisibleWaypoint != -1)
        {
            Vector3 steeringForce = Steering.Seek(rb, waypoints[nearestVisibleWaypoint].position, moveSpeed);
            rb.AddForce(steeringForce);
        }
    }

    void SeekCover()
    {
        int furthestCoverWaypoint = -1;
        float furthestDistance = float.MinValue;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (!HasLineOfSightToPlayer(waypoints[i].position))
            {
                float distance = Vector2.Distance(player.position, waypoints[i].position);
                if (distance > furthestDistance)
                {
                    furthestDistance = distance;
                    furthestCoverWaypoint = i;
                }
            }
        }

        if (furthestCoverWaypoint != -1)
        {
            Vector3 steeringForce = Steering.Seek(rb, waypoints[furthestCoverWaypoint].position, moveSpeed);
            rb.AddForce(steeringForce);
        }
    }

    bool IsInPlayerDetectionRadius()
    {
        float playerDistance = Vector2.Distance(transform.position, player.position);
        return playerDistance <= viewDistance;
    }

    bool HasLineOfSightToPlayer(Vector3 position)
    {
        Vector3 direction = (player.position - position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(position, direction, viewDistance);
        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    void Attack()
    {
        Debug.Log("Attacking");
        Vector3 steeringForce = Vector2.zero;
        steeringForce += Steering.Seek(rb, player.position, moveSpeed);
        rb.AddForce(steeringForce);

        Shoot();
    }

    void Defend()
    {
        Debug.Log("Defending");
        // Check if the enemy has no weapons
        if (!hasShotgun && !hasSniper)
        {
            // Set the enemy color to blue
            color = Color.blue;
            GetComponent<SpriteRenderer>().color = color;
        }
        Vector3 steeringForce = Vector2.zero;
        steeringForce += Steering.Flee(rb, player.position, moveSpeed);
        rb.AddForce(steeringForce);

        Shoot();
    }

    void Patrol()
    {
        // Increment waypoint if close enough
        float distance = Vector2.Distance(transform.position, waypoints[waypoint].transform.position);
        if (distance <= 2.5f)
        {
            waypoint++;
            waypoint %= waypoints.Length;
        }

        // Seek waypoint
        Vector3 steeringForce = Vector2.zero;
        steeringForce += Steering.Seek(rb, waypoints[waypoint].transform.position, moveSpeed);
        rb.AddForce(steeringForce);
    }

    void Shoot()
    {
        Vector3 playerDirection = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, playerDirection, viewDistance);
        bool playerHit = hit && hit.collider.CompareTag("Player");

        shootCooldown.Tick(Time.deltaTime);
        if (playerHit && shootCooldown.Expired())
        {
            shootCooldown.Reset();
            Debug.Log($"Current Weapon: {weaponType}");  // Debug weapon type
            switch (weaponType)
            {
                case WeaponType.SHOTGUN:
                    ShootShotgun();
                    break;
                case WeaponType.SNIPER:
                    ShootSniper();
                    break;
            }
        }
    }

    void ShootShotgun()
    {
        // AB = B - A
        Vector3 forward = (player.position - transform.position).normalized;
        Vector3 left = Quaternion.Euler(0.0f, 0.0f, 30.0f) * forward;
        Vector3 right = Quaternion.Euler(0.0f, 0.0f, -30.0f) * forward;

        Utilities.CreateBullet(bulletPrefab, transform.position, forward, 10.0f, 20.0f, UnitType.ENEMY);
        Utilities.CreateBullet(bulletPrefab, transform.position, left, 10.0f, 20.0f, UnitType.ENEMY);
        Utilities.CreateBullet(bulletPrefab, transform.position, right, 10.0f, 20.0f, UnitType.ENEMY);
    }

    void ShootSniper()
    {
        Vector3 forward = (player.position - transform.position).normalized;
        Debug.Log("Shooting sniper");
        GameObject bullet = Utilities.CreateBullet(bulletPrefab, transform.position, forward, 20.0f, 50.0f, UnitType.ENEMY);
        if (bullet != null)
        {
            Debug.Log("Sniper bullet created");
        }
    }

    void OnTransition(State state)
    {
        switch (state)
        {
            case State.NEUTRAL:
                color = Color.magenta;
                waypoint = Utilities.NearestPosition(transform.position, waypoints);
                break;

            case State.OFFENSIVE:
                color = Color.red;
                //shootCooldown.total = cooldownOffensive;
                break;

            case State.DEFENSIVE:
                color = Color.blue;
                //shootCooldown.total = cooldownDefensive;
                break;
        }
        GetComponent<SpriteRenderer>().color = color;
    }
    void Respawn()
    {
        statePrev = stateCurr = State.NEUTRAL;
        OnTransition(stateCurr);
        health.health = Health.maxHealth;
        transform.position = new Vector3(0.0f, 3.0f);
    }
}
