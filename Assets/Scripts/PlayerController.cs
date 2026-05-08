using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Attributes")]
    public int playerHealth = 100; //Health attribute for the player
    [Header("Player Movement")]
    public float moveSpeed = 5f; // Movement speed for the player
    public float jumpForce = 5f; // Jump force for the player
    public float sprintDuration = 2f; // How many seconds they can sprint
    private float sprintTimer;
    private bool isSprinting = false;
    private bool isGrounded; // Flag to check if the player is on the ground
    private Rigidbody rb; // Reference to the player's Rigidbody component
    public void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
    }
    void Update()
    {
        Onmove(); // Move the player based on the calculated movement and jump vectors
    }
    public void Onmove()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Get horizontal input (A/D or Left/Right)    
        float verticalInput = Input.GetAxis("Vertical"); // Get vertical input (W/S or Up/Down)
        Vector3 move = new Vector3(horizontalInput, 0, verticalInput) * moveSpeed * Time.deltaTime; // Calculate movement vector based on input and speed
        if (Input.GetButtonDown("Jump") && isGrounded) // Check if the jump button is pressed and the player is grounded
        {
            Jump(); // Apply jump force to the player's Rigidbody
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isSprinting) // Check if the sprint key is pressed and the player is not already sprinting
        {
            isSprinting = true;
            moveSpeed *= 2; // Double the movement speed for sprinting
            sprintTimer = sprintDuration; // Reset the sprint timer
        }
        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime; // Count down

            if (sprintTimer <= 0)
            {
                StopSprint();
            }
        }
        transform.Translate(move); // Move the player based on the calculated movement and jump vectors
    }

    void Jump()
    {
        // Vector3.up is (0, 1, 0)
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false; // Prevent infinite jumping
    }
    public void takeDamage(int damage)
    {
        playerHealth -= damage; // Example damage value
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Basic check to see if we've touched the floor again
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    void StopSprint()
    {
        isSprinting = false;
        moveSpeed /= 2;
    }
}