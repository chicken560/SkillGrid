using UnityEngine;

public class BlasterFollow : MonoBehaviour
{
    [Header("Settings")]
    public Transform playerTransform; // Drag your Player here in the Inspector
    public Vector3 offset = new Vector3(0.2f, 0.2f, 0.2f); // Your coordinates
    public bool smoothFollow = true;
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // Calculate the target position
        Vector3 targetPosition = playerTransform.position + offset;

        if (smoothFollow)
        {
            // Smoothly move from current position to target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            // Snap instantly to the player
            transform.position = targetPosition;
        }

        // Match the player's rotation
        transform.rotation = playerTransform.rotation;
    }
}