using UnityEngine;

public class BlasterFollow : MonoBehaviour
{
    [Header("Settings")]
    public Transform playerTransform; // Drag your Player here in the Inspector
    public Vector3 offset = new Vector3(0.2f, 0.2f, 0.2f); // Local offset relative to player
    public bool smoothFollow = true;
    public float smoothSpeed = 20f;

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // Transform offset to world space relative to player's rotation
        Vector3 worldOffset = playerTransform.TransformDirection(offset);

        // Calculate the target position using the rotated offset
        Vector3 targetPosition = playerTransform.position + worldOffset;

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