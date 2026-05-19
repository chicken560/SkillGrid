using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsJumpChase : MonoBehaviour
{
    [Header("CHASE TARGET (Drag Player Here!)")]
    [Tooltip("Drag your Player object from the Hierarchy into this box.")]
    public Transform player;

    [Header("Movement Sliders")]
    [Range(1f, 20f)] public float moveSpeed = 5f;
    [Range(50f, 720f)] public float rotationSpeed = 360f;

    [Header("Jumping Settings")]
    [Range(1f, 15f)] public float jumpForce = 6f;
    [Range(0.5f, 5f)] public float jumpCooldown = 1.5f;
    [Range(0.5f, 5f)] public float heightToTriggerJump = 1.2f;
    public LayerMask groundLayers;

    [Header("Scene Teleport")]
    public string gameOverSceneName = "GameOver";

    private Rigidbody rb;
    private NavMeshPath navPath;
    private float nextJumpTime;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        navPath = new NavMeshPath();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        CheckGrounded();

        // Calculate path dynamically using your baked NavMesh surface
        if (NavMesh.CalculatePath(transform.position, player.position, NavMesh.AllAreas, navPath))
        {
            if (navPath.corners.Length > 1)
            {
                Vector3 targetCorner = navPath.corners[1]; // Target the next node
                Vector3 moveDirection = (targetCorner - transform.position);
                moveDirection.y = 0;
                moveDirection.Normalize();

                // 1. FACE THE WAY IT IS GOING: Look toward the target corner instead of the player
                HandleLookDirection(moveDirection);

                // Apply dynamic velocity
                rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
            }
        }

        // Jump mechanics
        float heightDifference = player.position.y - transform.position.y;
        if (heightDifference >= heightToTriggerJump && isGrounded && Time.time >= nextJumpTime)
        {
            ExecuteJump(player.position);
        }
    }

    void HandleLookDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        // Smoothly rotate the enemy to point toward its movement vector
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void ExecuteJump(Vector3 playerPos)
    {
        nextJumpTime = Time.time + jumpCooldown;

        Vector3 jumpDirection = (playerPos - transform.position);
        jumpDirection.y = 0;
        jumpDirection.Normalize();

        Vector3 forceVector = (jumpDirection * moveSpeed * 0.5f) + (Vector3.up * jumpForce);
        rb.AddForce(forceVector, ForceMode.Impulse);
    }

    void CheckGrounded()
    {
        float raycastDistance = (GetComponent<Collider>().bounds.extents.y) + 0.2f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundLayers);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
