using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Attributes")]
    public int playerHealth = 100; //Health attribute for the player
    [Header("Player Movement")]
    public float moveSpeed = 5f; // Movement speed for the player
    public float jumpForce = 5f; // Jump force for the player
    private Rigidbody rb; // Reference to the player's Rigidbody component
      public void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
    }
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Get horizontal input (A/D or Left/Right)    
        float verticalInput = Input.GetAxis("Vertical"); // Get vertical input (W/S or Up/Down)
        Vector3 move = new Vector3(horizontalInput, 0, verticalInput) * moveSpeed * Time.deltaTime; // Calculate movement vector based on input and speed
        if (Input.GetButtonDown("Jump") && Mathf.Abs(rb.linearVelocity.y) < 0.001f) // Check if the jump button is pressed and the player is grounded
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Apply jump force to the player's Rigidbody
        }
        transform.Translate(move); // Move the player based on the calculated movement and jump vectors
    }
    public void takeDamage(int damage)
    {
        playerHealth -= damage; // Example damage value
    }
}