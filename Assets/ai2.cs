using UnityEngine;
using UnityEngine.AI;
#if UNITY_AI_NAVIGATION
using Unity.AI.Navigation;
#endif

public class ai2 : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public float chaseDistance = 10f;

    [Header("Frustration Settings")]
    public float frustrationIncreaseRate = 25f; // Points per second when stuck/blocked
    public float frustrationDecreaseRate = 15f; // Points per second when moving cleanly
    public float maxFrustration = 100f;         // Threshold to force a path recalculation

    [Header("Performance Settings")]
    public float pathUpdateInterval = 0.5f;     // How often to update the NavMesh path

    [Header("Debug Info (Read Only)")]
    public float currentFrustration = 0f;
    public bool isStuckOrBlocked = false;

    private NavMeshAgent agent;
    private float pathTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseDistance)
        {
            agent.isStopped = false;

            // Periodically refresh the target path assignment
            pathTimer += Time.deltaTime;
            if (pathTimer >= pathUpdateInterval)
            {
                pathTimer = 0f;
                agent.SetDestination(player.position);
            }

            // Continuous status evaluation every frame
            EvaluateStuckStatus(distanceToPlayer);
            ManageFrustration();
        }
        else
        {
            agent.isStopped = true;
            isStuckOrBlocked = false;
            currentFrustration = Mathf.Max(0f, currentFrustration - frustrationDecreaseRate * Time.deltaTime);
        }
    }

    void EvaluateStuckStatus(float distanceToPlayer)
    {
        // RULE 1: If Unity declares the path broken/partial, it's blocked
        bool isPathBroken = (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid);

        // RULE 2: If the path is broken AND the agent is further away from the player than its stopping tolerance, it is stuck
        if (isPathBroken && distanceToPlayer > agent.stoppingDistance)
        {
            isStuckOrBlocked = true;
            return;
        }

        // RULE 3: Physical obstacle check (rubbing against a wall on a "valid" path)
        if (agent.hasPath && agent.velocity.sqrMagnitude < 0.1f && distanceToPlayer > agent.stoppingDistance)
        {
            isStuckOrBlocked = true;
        }
        else
        {
            isStuckOrBlocked = false;
        }
    }

    void ManageFrustration()
    {
        if (isStuckOrBlocked)
        {
            currentFrustration = Mathf.Min(maxFrustration, currentFrustration + frustrationIncreaseRate * Time.deltaTime);
        }
        else
        {
            currentFrustration = Mathf.Max(0f, currentFrustration - frustrationDecreaseRate * Time.deltaTime);
        }

        if (currentFrustration >= maxFrustration)
        {
            ExecuteFrustratedAction();
        }
    }

    void ExecuteFrustratedAction()
    {
        currentFrustration = 0f;
        agent.ResetPath(); // Completely clear the frozen path container

        // Force an immediate layout reset to look for stairs/ramps again
        agent.SetDestination(player.position);
    }
}
