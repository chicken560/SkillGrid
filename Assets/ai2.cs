using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class ai2 : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player Transform (will auto-find GameObject tagged 'Player' if empty).")]
    public Transform player;

    [Header("Chase")]
    public float chaseDistance = 12f;
    public float loseInterestDistance = 18f;
    public float stoppingDistance = 1.2f;
    [Tooltip("How often (s) the agent updates its path to the player.")]
    public float pathUpdateInterval = 0.35f;

    [Header("Agent Movement")]
    public float moveSpeed = 3.8f;
    public float angularSpeed = 120f;
    public bool autoBraking = false;

    [Header("Contact / End Run")]
    [Tooltip("If true touching the player ends the run immediately.")]
    public bool endRunOnTouch = true;
    [Tooltip("If true, load next scene (by name or build index) when run ends.")]
    public bool loadNextSceneOnEnd = true;
    [Tooltip("Optional scene name to load. If empty the next build index is used.")]
    public string nextSceneName = "";
    [Tooltip("Optional delay (seconds) before executing the end-run action.")]
    public float endRunDelay = 0f;

    [Header("Player handling")]
    [Tooltip("If true, will disable detected PlayerController component during end-run handling.")]
    public bool disablePlayerControllerOnEnd = true;

    [Header("Next level helper (optional)")]
    [Tooltip("Optional NextLevelInstant component to call for scene transitions (assign in Inspector).")]
    public NextLevelInstant nextLevelComponent;

    [Header("Events")]
    [Tooltip("Called when the player is touched (before scene load or other actions).")]
    public UnityEvent onPlayerTouched;

    [Header("Debug")]
    [Tooltip("Read-only — shows whether the AI is currently chasing the player.")]
    [SerializeField] private bool _isChasing = false;
    public bool isChasing => _isChasing;

    private NavMeshAgent agent;
    private float pathTimer;
    private bool _handlingEnd;

    void Reset()
    {
        // ensure there's a trigger collider for touch detection
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            var sc = gameObject.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.6f;
        }
        else
        {
            col.isTrigger = true;
        }
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("ai2 requires a NavMeshAgent component. Disabling script.");
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = autoBraking;

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        // if NextLevelInstant not assigned, try to find one in scene
        if (nextLevelComponent == null)
            nextLevelComponent = FindObjectOfType<NextLevelInstant>();
    }

    void Update()
    {
        if (player == null || _handlingEnd || agent == null) return;

        float d = Vector3.Distance(transform.position, player.position);

        // decide whether to chase
        if (d <= chaseDistance)
        {
            _isChasing = true;
            pathTimer += Time.deltaTime;
            if (pathTimer >= pathUpdateInterval)
            {
                pathTimer = 0f;
                if (agent.isOnNavMesh)
                    agent.SetDestination(player.position);
            }
            agent.isStopped = false;
        }
        else if (d > loseInterestDistance)
        {
            _isChasing = false;
            agent.ResetPath();
            agent.isStopped = true;
        }
        // else keep current behavior (allows brief pursuit beyond chaseDistance)
    }

    // Trigger-based contact detection (requires this collider to be a trigger and player to have a Rigidbody)
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ai2: OnTriggerEnter with '{other?.name}'");
        if (!_handlingEnd && IsPlayerCollider(other))
        {
            Debug.Log("ai2: Player detected by trigger.");
            if (endRunOnTouch)
            {
                StartCoroutine(HandleEndRunRoutine(other.transform));
            }
            else
            {
                onPlayerTouched?.Invoke();
            }
        }
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null) return false;
        if (player != null && other.transform == player) return true;
        if (other.CompareTag("Player")) return true;
        if (other.GetComponent<PlayerController>() != null) return true;
        if (other.name.ToLower() == "player") return true;
        return false;
    }

    private IEnumerator HandleEndRunRoutine(Transform playerTransform)
    {
        _handlingEnd = true;
        onPlayerTouched?.Invoke();

        Debug.Log("ai2: Handling end-run.");

        if (disablePlayerControllerOnEnd)
        {
            var pc = playerTransform.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = false;
                Debug.Log("ai2: PlayerController disabled.");
            }
        }

        if (endRunDelay > 0f)
            yield return new WaitForSeconds(endRunDelay);

        // Prefer calling assigned NextLevelInstant for consistent behavior
        if (loadNextSceneOnEnd)
        {
            if (nextLevelComponent != null)
            {
                Debug.Log("ai2: Using NextLevelInstant to load next level.");
                nextLevelComponent.TriggerLoad();
            }
            else
            {
                // direct scene load fallback
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    if (Application.CanStreamedLevelBeLoaded(nextSceneName))
                    {
                        Debug.Log($"ai2: Loading scene by name: {nextSceneName}");
                        SceneManager.LoadScene(nextSceneName);
                    }
                    else
                    {
                        Debug.LogWarning($"ai2: scene '{nextSceneName}' cannot be loaded (not in Build Settings).");
                    }
                }
                else
                {
                    int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
                    if (nextIndex < SceneManager.sceneCountInBuildSettings)
                    {
                        Debug.Log($"ai2: Loading next scene index: {nextIndex}");
                        SceneManager.LoadScene(nextIndex);
                    }
                    else
                    {
                        Debug.LogWarning("ai2: no next scene in Build Settings.");
                    }
                }
            }
        }

        // If not loading scene, just mark finished and optionally re-enable player control.
        if (!loadNextSceneOnEnd)
        {
            if (disablePlayerControllerOnEnd)
            {
                var pc = playerTransform.GetComponent<PlayerController>();
                if (pc != null) pc.enabled = true;
            }
            _handlingEnd = false;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red * 0.7f;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.yellow * 0.6f;
        Gizmos.DrawWireSphere(transform.position, loseInterestDistance);
    }
#endif
}