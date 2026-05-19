using UnityEngine;

/// <summary>
/// Simple, robust billboard that keeps a 2D sprite/quad always facing the camera.
/// Default behaviour only yaws so the sprite stays visually "flat" (no pitch/tilt).
/// Includes a local `lookOffset` you can edit with the scene-handle editor (see Assets/Editor/OffsetHandleEditor.cs).
/// </summary>
[DisallowMultipleComponent]
public class BillboardFaceSimple : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera to face. If null Camera.main will be used.")]
    public Camera targetCamera;

    [Header("Offsets")]
    [Tooltip("Local offset (in this object's local space) used as the pivot/aim point for facing.\n" +
             "Edit this in Scene view with the OffsetHandleEditor (property name: 'lookOffset').")]
    public Vector3 lookOffset = Vector3.zero;

    [Tooltip("Additional yaw rotation (degrees) applied after facing the camera.")]
    [Range(-180f, 180f)]
    public float yawOffsetDegrees = 0f;

    [Header("Behaviour")]
    [Tooltip("When true only rotate around world Y (no pitch) so sprite stays flat to the ground.")]
    public bool onlyRotateY = true;

    [Tooltip("Smooth rotation instead of snapping.")]
    public bool smooth = true;
    [Tooltip("Smoothing speed (higher = faster).")]
    [Range(0.1f, 50f)]
    public float smoothSpeed = 12f;

    [Tooltip("If true the object will face away from the camera (useful for some shader setups).")]
    public bool faceAway = false;

    void Reset()
    {
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

        // world point on this object used as the origin for aiming (useful to offset the sprite center)
        Vector3 worldPivot = transform.TransformPoint(lookOffset);

        // compute vector from pivot -> camera
        Vector3 toCam = targetCamera.transform.position - worldPivot;

        if (onlyRotateY)
        {
            // keep flat: ignore vertical difference so sprite doesn't tilt up/down
            toCam.y = 0f;
            if (toCam.sqrMagnitude < 0.0001f) return;
        }
        else
        {
            if (toCam.sqrMagnitude < 0.00001f) return;
        }

        Vector3 forward = faceAway ? -toCam.normalized : toCam.normalized;
        Quaternion desired = Quaternion.LookRotation(forward, Vector3.up);

        // apply yaw offset
        if (!Mathf.Approximately(yawOffsetDegrees, 0f))
            desired = desired * Quaternion.Euler(0f, yawOffsetDegrees, 0f);

        if (smooth)
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        else
            transform.rotation = desired;
    }

    [ContextMenu("Assign Main Camera")]
    void AssignMainCamera()
    {
        targetCamera = Camera.main;
        Debug.Log($"BillboardFaceSimple: targetCamera set to {(targetCamera != null ? targetCamera.name : "null")}");
    }
}