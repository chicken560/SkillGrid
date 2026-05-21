using UnityEngine;

public class RetroBillboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (camTransform == null) return;

        // 1. Calculate the direction the camera is facing
        Vector3 targetDirection = camTransform.forward;

        // 2. CRITICAL: Flatten the vertical (Y) direction to zero.
        // This stops the sprite from tilting back and showing its stem/top edge.
        targetDirection.y = 0;

        // 3. Keep the target directional vector valid
        if (targetDirection != Vector3.zero)
        {
            // Force the flat plane of the sprite to face the flattened camera direction
            transform.rotation = Quaternion.LookRotation(-targetDirection, Vector3.up);
        }
    }
}
