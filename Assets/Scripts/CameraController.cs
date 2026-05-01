using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Player GameObject to follow and rotate (assign the object that moves).")]
    public GameObject Player;

    [Tooltip("Mouse sensitivity multiplier.")]
    [Range(10f, 1000f)]
    public float mouseSensitivity = 200f;

    [Tooltip("Camera height above the player's origin (eye level).")]
    public float cameraHeight = 1.6f;

    [Tooltip("Lock and hide the cursor on start.")]
    public bool lockCursor = true;

    [Tooltip("Pitch limits (degrees).")]
    public float minPitch = -85f;
    public float maxPitch = 85f;

    float pitch = 0f; // camera up/down
    float yaw = 0f;   // player left/right

    void Start()
    {
        if (Player == null)
        {
            Debug.LogError("CameraController: Player is not assigned.");
            enabled = false;
            return;
        }

        // Initialize yaw and pitch to avoid snapping
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
        // Unlock cursor if requested
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Read mouse using sensitivity (inspector adjustable)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply yaw to player and pitch to camera
        Player.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Follow player position at eye height
        transform.position = Player.transform.position + Vector3.up * cameraHeight;
    }
}
