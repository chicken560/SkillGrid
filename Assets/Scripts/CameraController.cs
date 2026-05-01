using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Player GameObject to follow and rotate (assign the object that moves).")]
    public GameObject Player;

    [Tooltip("Mouse sensitivity (degrees per second).")]
    public float mouseSensitivity = 200f;

    [Tooltip("Height of the camera above the player's position (eye height).")]
    public float cameraHeight = 1.6f;

    [Tooltip("Lock and hide the cursor on start.")]
    public bool lockCursor = true;

    [Tooltip("Minimum and maximum pitch (up/down) in degrees.")]
    public float minPitch = -85f;
    public float maxPitch = 85f;

    float pitch = 0f; // up/down
    float yaw = 0f;   // left/right

    void Start()
    {
        if (Player == null)
        {
            Debug.LogError("CameraController: Player is not assigned.");
            enabled = false;
            return;
        }

        // Initialize yaw to current player rotation Y so camera doesn't snap
        yaw = Player.transform.eulerAngles.y;
        pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // Optional cursor unlock toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Read mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Update yaw and pitch
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply yaw to player (rotate player around Y)
        Player.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Apply pitch to camera (local rotation around X)
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Keep the camera at the player's eye position
        Vector3 targetPos = Player.transform.position + Vector3.up * cameraHeight;
        transform.position = targetPos;
    }
}
