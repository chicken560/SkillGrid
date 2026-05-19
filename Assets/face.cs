using UnityEngine;

/// <summary>
/// Keeps a 2D sprite / quad always facing the player's camera so it appears flat to the view.
/// Safe, simple, and editor-friendly. Replace existing face scripts with this one (or remove duplicates).
/// </summary>
[DisallowMultipleComponent]
public class BillboardFace : MonoBehaviour
{
    [Header("Camera Target")]
    [Tooltip("If empty the script will use Camera.main at runtime.")]
    public Camera targetCamera;

    [Header("Behavior")]
    [Tooltip("If true the sprite will exactly match the camera rotation (parallel to screen).")]
    public bool screenAligned = true;
    [Tooltip("If true the sprite will only yaw (rotate around world Y) to face the camera.")]
    public bool onlyRotateY = false;
    [Tooltip("Smooth rotation instead of snapping.")]
    public bool smooth = true;
    [Tooltip("Smoothing speed (higher = faster).")]
    [Range(1f, 50f)]
    public float smoothSpeed = 12f;

    void Reset()
    {
        // Provide a sensible default in the Inspector
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main;
    }

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Quaternion desired;

        if (screenAligned)
        {
            // Make the object's forward match the camera forward so the object plane is parallel to the screen.
            // Using camera.forward keeps the sprite plane facing the camera (no tilt relative to screen).
            Vector3 camForward = targetCamera.transform.forward;
            if (onlyRotateY)
            {
                camForward.y = 0f;
                if (camForward.sqrMagnitude < 0.0001f) return;
                desired = Quaternion.LookRotation(camForward.normalized, Vector3.up);
            }
            else
            {
                desired = Quaternion.LookRotation(camForward.normalized, targetCamera.transform.up);
            }
        }
        else
        {
            // Classic "look at camera" billboard (sprite's forward points toward camera)
            Vector3 toCam = targetCamera.transform.position - transform.position;
            if (onlyRotateY)
            {
                toCam.y = 0f;
                if (toCam.sqrMagnitude < 0.0001f) return;
                desired = Quaternion.LookRotation(toCam.normalized, Vector3.up);
            }
            else
            {
                if (toCam.sqrMagnitude < 0.0001f) return;
                desired = Quaternion.LookRotation(-toCam.normalized, Vector3.up); // negative so sprite faces camera
            }
        }

        if (smooth)
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        else
            transform.rotation = desired;
    }

    [ContextMenu("Assign Main Camera")]
    void AssignMainCamera()
    {
        targetCamera = Camera.main;
        Debug.Log($"BillboardFace: targetCamera set to {(targetCamera != null ? targetCamera.name : "null")}");
    }
}