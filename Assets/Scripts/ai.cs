using UnityEngine;
using UnityEngine.AI;

public class ai : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Target Transform. If left empty the script will try to find a GameObject named \"player\".")]
    public Transform player;

    [Header("Agent Settings")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.2f;

    [Header("Fallback Search")]
    [Tooltip("If the direct path to the player is blocked, search this far (meters) from the player for a reachable NavMesh point.")]
    public float searchRadius = 6f;
    [Tooltip("How many random samples to try when searching for a reachable point.")]
    public int searchSamples = 12;

    [Header("Destination smoothing")]
    [Tooltip("Minimum distance a new destination must differ from the current one to update immediately.")]
    public float minDestinationMoveDist = 0.6f;
    [Tooltip("Minimum seconds between forced destination updates.")]
    public float minUpdateInterval = 0.25f;

    [Header("Path checks")]
    [Tooltip("Throttle heavy path checks to avoid rapid recalculation (seconds).")]
    public float pathCheckInterval = 0.12f;

    private NavMeshAgent agent;
    private Vector3 lastDestination;
    private float lastDestinationSetTime;
    private float lastPathCheckTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("ai: NavMeshAgent component missing. Disabling script.");
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = false;

        lastDestination = transform.position;
        lastDestinationSetTime = -999f;
        lastPathCheckTime = -999f;
    }

    void Start()
    {
        // Auto-assign player by name if not set
        if (player == null)
        {
            var pgo = GameObject.Find("player");
            if (pgo != null) player = pgo.transform;
        }

        // Optional sanity check for scene naming
        if (gameObject.name != "anemy")
        {
            Debug.LogWarning($"ai: this GameObject is named \"{gameObject.name}\". The expected enemy name is \"anemy\".");
        }

        // Try to place agent on NavMesh if it's off
        if (!agent.isOnNavMesh)
        {
            NavMeshHit near;
            if (NavMesh.SamplePosition(transform.position, out near, 2.0f, NavMesh.AllAreas))
                agent.Warp(near.position);
        }
    }

    void Update()
    {
        if (player == null || agent == null) return;

        if (!agent.isOnNavMesh)
        {
            NavMeshHit near;
            if (NavMesh.SamplePosition(transform.position, out near, 2.0f, NavMesh.AllAreas))
                agent.Warp(near.position);
            else
                return;
        }

        // Throttle path calculations — still let the agent move to its current destination between checks.
        if (Time.time - lastPathCheckTime < pathCheckInterval)
            return; // skip heavy checks this frame

        lastPathCheckTime = Time.time;

        // Calculate direct path to the player's exact position
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(agent.transform.position, player.position, NavMesh.AllAreas, path);

        // If fully reachable, go straight for the player
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            TrySetDestination(player.position);
            return;
        }

        // If partial path exists, move to the last valid corner (furthest reachable point toward the goal)
        if (path.status == NavMeshPathStatus.PathPartial && path.corners != null && path.corners.Length > 0)
        {
            Vector3 lastCorner = path.corners[path.corners.Length - 1];
            TrySetDestination(lastCorner);
            return;
        }

        // When path is invalid (no partial), try sampling near the player for reachable points
        NavMeshHit hit;
        Vector3 bestPos = Vector3.zero;
        float bestPlayerDist = Mathf.Infinity;
        bool found = false;

        // First quick sample at player's position (useful if player is slightly off mesh)
        if (NavMesh.SamplePosition(player.position, out hit, Mathf.Max(1f, searchRadius), NavMesh.AllAreas))
        {
            NavMeshPath p2 = new NavMeshPath();
            NavMesh.CalculatePath(agent.transform.position, hit.position, NavMesh.AllAreas, p2);
            if (p2.status == NavMeshPathStatus.PathComplete)
            {
                TrySetDestination(hit.position);
                return;
            }
            else if (p2.status == NavMeshPathStatus.PathPartial && p2.corners != null && p2.corners.Length > 0)
            {
                Vector3 partial = p2.corners[p2.corners.Length - 1];
                float pd = Vector3.Distance(partial, player.position);
                if (pd < bestPlayerDist)
                {
                    bestPlayerDist = pd;
                    bestPos = partial;
                    found = true;
                }
            }
        }

        // Randomized sampling around player: prefer samples that are reachable AND closest to the player.
        for (int i = 0; i < searchSamples; i++)
        {
            Vector3 rnd = player.position + Random.insideUnitSphere * searchRadius;
            rnd.y = player.position.y; // bias sampling near player's height
            if (NavMesh.SamplePosition(rnd, out hit, Mathf.Max(0.5f, searchRadius * 0.25f), NavMesh.AllAreas))
            {
                NavMeshPath p = new NavMeshPath();
                NavMesh.CalculatePath(agent.transform.position, hit.position, NavMesh.AllAreas, p);
                if (p.status == NavMeshPathStatus.PathComplete)
                {
                    float playerDist = Vector3.Distance(hit.position, player.position);
                    if (playerDist < bestPlayerDist)
                    {
                        bestPlayerDist = playerDist;
                        bestPos = hit.position;
                        found = true;
                    }
                }
                else if (p.status == NavMeshPathStatus.PathPartial && p.corners != null && p.corners.Length > 0)
                {
                    Vector3 partialReach = p.corners[p.corners.Length - 1];
                    float playerDist = Vector3.Distance(partialReach, player.position);
                    if (playerDist < bestPlayerDist)
                    {
                        bestPlayerDist = playerDist;
                        bestPos = partialReach;
                        found = true;
                    }
                }
            }
        }

        if (found)
        {
            TrySetDestination(bestPos);
            return;
        }

        // Last-resort: move horizontally toward player's projected position so the agent keeps moving
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            Vector3 fallback = transform.position + dir.normalized * Mathf.Min(searchRadius, dir.magnitude);
            if (NavMesh.SamplePosition(fallback, out hit, 1.0f, NavMesh.AllAreas))
                TrySetDestination(hit.position);
        }
    }

    // Attempt to set destination but avoid rapid tiny destination changes and avoid interrupting an in-flight path.
    private void TrySetDestination(Vector3 dest)
    {
        // If agent is currently calculating or applying a path, avoid interrupting unless destination is meaningfully different.
        bool hasAgentDestination = agent.hasPath;
        Vector3 currentDest = hasAgentDestination ? agent.destination : lastDestination;

        float distToCurrent = Vector3.Distance(dest, currentDest);
        float now = Time.time;

        // If new destination is very close to current, skip to avoid jitter.
        if (distToCurrent < minDestinationMoveDist)
            return;

        // If agent is mid-path and still far from destination, avoid spamming SetDestination.
        if (hasAgentDestination && agent.pathPending == false && agent.remainingDistance > Mathf.Max(0.5f, agent.stoppingDistance + 0.2f))
        {
            // allow update only if enough time passed since last update
            if (now - lastDestinationSetTime < minUpdateInterval)
                return;
        }

        // All checks passed — set destination.
        agent.SetDestination(dest);
        lastDestination = dest;
        lastDestinationSetTime = now;
    }
}