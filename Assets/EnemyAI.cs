using UnityEngine;
using UnityEngine.AI;

/// Movement-only enemy AI.
/// States: Idle (stay or walk home) -> Chase (follow the player)
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Chase }

    [Header("References")]
    public Transform player;              // Leave empty: auto-finds object tagged "Player"
    public Animator animator;             // Optional. Param: float "Speed"

    [Header("Detection")]
    public float sightRange = 15f;        // Starts chasing inside this range (if it can see you)
    public float loseSightRange = 25f;    // Gives up if you get this far away
    public LayerMask obstacleMask;        // Walls/terrain that block line of sight
    public float eyeHeight = 1.5f;

    [Header("Movement")]
    public float stoppingDistance = 2f;   // How close it gets to the player before stopping
    public float repathInterval = 0.2f;   // Don't recalculate path every frame (matters with many enemies)

    public State CurrentState => state;

    NavMeshAgent agent;
    State state = State.Idle;
    float nextRepathTime;
    Vector3 homePosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;

        // Slightly different priorities make a swarm spread out instead of stacking
        agent.avoidancePriority = Random.Range(30, 70);
        agent.stoppingDistance = stoppingDistance;
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
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle: UpdateIdle(dist); break;
            case State.Chase: UpdateChase(dist); break;
        }

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);
    }

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

        if (Time.time >= nextRepathTime)
        {
            agent.SetDestination(player.position);
            nextRepathTime = Time.time + repathInterval;
        }

        // Turn to face the player when standing still next to them
        if (dist <= stoppingDistance) FacePlayer();
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

    // Shows ranges in the Scene view when the enemy is selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseSightRange);
    }
}