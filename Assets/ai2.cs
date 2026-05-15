using UnityEngine;
using UnityEngine.AI;

public class ai2 : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public float chaseDistance = 10f;

    [Header("Frustration Settings")]
    public float frustrationThreshold = 5f; // Seconds until frustrated
    public float normalJumpForce = 5f;
    public float frustratedJumpForce = 12f;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private float outOfRangeTimer = 0f;
    private bool isFrustrated = false;
    private bool isGrounded;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseDistance)
        {
            // Reset timer and frustration when back in range
            outOfRangeTimer = 0f;
            isFrustrated = false;

            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            // Stop pathfinding when out of range
            agent.isStopped = true;

            // Increment frustration timer
            outOfRangeTimer += Time.deltaTime;
            if (outOfRangeTimer >= frustrationThreshold)
            {
                isFrustrated = true;
            }

            // Trigger jump behavior
            if (isGrounded)
            {
                Jump();
            }
        }

        // Check if enemy is touching the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    void Jump()
    {
        // Temporarily disable NavMeshAgent physical control to allow vertical physics movement
        agent.updatePosition = false;
        agent.updateRotation = false;

        // Choose jump force based on state
        float activeJumpForce = isFrustrated ? frustratedJumpForce : normalJumpForce;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, activeJumpForce, rb.linearVelocity.z);
    }

    void LateUpdate()
    {
        // Re-sync NavMeshAgent position with the Rigidbody physics position once back on ground
        if (isGrounded)
        {
            agent.nextPosition = transform.position;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
    }
}
