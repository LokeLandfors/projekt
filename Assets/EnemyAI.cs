using UnityEngine;
using UnityEngine.AI;

/// Simple state-machine enemy for a Diablo-style game.
/// States: Idle -> Chase -> Attack -> (back to Chase) ... -> Dead
/// Works for both melee and ranged enemies.
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, IDamageable
{
    public enum State { Idle, Chase, Attack, Dead }
    public enum AttackType { Melee, Ranged }

    [Header("References")]
    public Transform player;              // Leave empty: auto-finds object tagged "Player"
    public Animator animator;             // Optional. Params: float "Speed", trigger "Attack", trigger "Die"

    [Header("Detection")]
    public float sightRange = 15f;        // Starts chasing inside this range (if it can see you)
    public float loseSightRange = 25f;    // Gives up if you get this far away
    public LayerMask obstacleMask;        // Walls/terrain that block line of sight
    public float eyeHeight = 1.5f;

    [Header("Attack")]
    public AttackType attackType = AttackType.Melee;
    public float attackRange = 2f;        // Melee ~2, ranged ~10
    public float attackCooldown = 1.5f;
    public int damage = 10;

    [Header("Ranged only")]
    public GameObject projectilePrefab;   // Prefab with EnemyProjectile on it
    public Transform firePoint;
    public float projectileSpeed = 15f;

    [Header("Health")]
    public int maxHealth = 50;

    [Header("Performance")]
    public float repathInterval = 0.2f;   // Don't recalculate path every frame (matters with 50+ enemies)

    public State CurrentState => state;

    NavMeshAgent agent;
    State state = State.Idle;
    int health;
    float lastAttackTime = -999f;
    float nextRepathTime;
    Vector3 homePosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = maxHealth;
        homePosition = transform.position;

        // Slightly different priorities make a swarm spread out instead of stacking
        agent.avoidancePriority = Random.Range(30, 70);
        agent.stoppingDistance = attackRange * 0.8f;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (state == State.Dead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle:   UpdateIdle(dist);   break;
            case State.Chase:  UpdateChase(dist);  break;
            case State.Attack: UpdateAttack(dist); break;
        }

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    // ---------------- States ----------------

    void UpdateIdle(float dist)
    {
        if (dist <= sightRange && HasLineOfSight())
        {
            state = State.Chase;
            return;
        }

        // Walk back home if we lost the player
        if (Vector3.Distance(transform.position, homePosition) > 1f && Time.time >= nextRepathTime)
        {
            agent.isStopped = false;
            agent.SetDestination(homePosition);
            nextRepathTime = Time.time + repathInterval;
        }
    }

    void UpdateChase(float dist)
    {
        if (dist > loseSightRange)
        {
            state = State.Idle;
            return;
        }

        if (dist <= attackRange && HasLineOfSight())
        {
            state = State.Attack;
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        if (Time.time >= nextRepathTime)
        {
            agent.SetDestination(player.position);
            nextRepathTime = Time.time + repathInterval;
        }
    }

    void UpdateAttack(float dist)
    {
        FacePlayer();

        // Player moved away or ducked behind a wall -> chase again
        if (dist > attackRange * 1.2f || !HasLineOfSight())
        {
            state = State.Chase;
            agent.isStopped = false;
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            if (animator != null) animator.SetTrigger("Attack");
            DoAttack();
            // Tip: if you use animations, call DoAttack() from an Animation Event instead,
            // so damage happens when the swing actually lands.
        }
    }

    // ---------------- Actions ----------------

    public void DoAttack()
    {
        if (attackType == AttackType.Melee)
        {
            IDamageable target = player.GetComponentInParent<IDamageable>();
            if (target != null) target.TakeDamage(damage);
        }
        else if (projectilePrefab != null)
        {
            Transform origin = firePoint != null ? firePoint : transform;
            Vector3 aimPoint = player.position + Vector3.up * 1f;
            Vector3 dir = (aimPoint - origin.position).normalized;

            GameObject proj = Instantiate(projectilePrefab, origin.position, Quaternion.LookRotation(dir));
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null) ep.Init(dir * projectileSpeed, damage, gameObject);
        }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 8f);
    }

    bool HasLineOfSight()
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * 1f;
        Vector3 dir = target - eye;
        // If the ray hits an obstacle before reaching the player, we can't see them
        return !Physics.Raycast(eye, dir.normalized, dir.magnitude, obstacleMask);
    }

    // ---------------- Health ----------------

    public void TakeDamage(int amount)
    {
        if (state == State.Dead) return;

        health -= amount;

        // Getting hit by a spell makes the enemy aggro even if it didn't see you
        if (state == State.Idle) state = State.Chase;

        if (health <= 0) Die();
    }

    void Die()
    {
        state = State.Dead;
        agent.isStopped = true;
        agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (animator != null) animator.SetTrigger("Die");

        // Drop loot / give XP here
        Destroy(gameObject, 3f);
    }

    // Shows ranges in the Scene view when the enemy is selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
