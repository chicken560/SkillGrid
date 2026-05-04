using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;

    void Start()
    {
        // Locks the cursor to the middle of the screen and hides it
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Calculate vertical rotation and clamp it (so you can't flip upside down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply vertical rotation to the camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player body horizontally
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
