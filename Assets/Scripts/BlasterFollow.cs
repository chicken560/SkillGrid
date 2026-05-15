using UnityEngine;

public class BlasterFollow : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // Drag your Main Camera here

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0.2f, -0.2f, 0.4f); // Adjusted to sit slightly in front/below
    public bool smoothFollowPosition = true;
    public float positionSmoothSpeed = 20f;

    [Header("Rotation Settings")]
    public bool smoothFollowRotation = true;
    public float rotationSmoothSpeed = 15f; // Slightly slower than position creates a nice "weight" feel

    void LateUpdate()
    {
        // Fallback to Main Camera if nothing is assigned
        if (cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            else
                return;
        }

        // --- POSITION ---
        // Transform offset to world space relative to camera's rotation
        Vector3 worldOffset = cameraTransform.TransformDirection(offset);
        Vector3 targetPosition = cameraTransform.position + worldOffset;

        if (smoothFollowPosition)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }

        // --- ROTATION ---
        if (smoothFollowRotation)
        {
            // Smoothly match the camera's rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, cameraTransform.rotation, rotationSmoothSpeed * Time.deltaTime);
        }
        else
        {
            // Snap instantly
            transform.rotation = cameraTransform.rotation;
        }
    }
}