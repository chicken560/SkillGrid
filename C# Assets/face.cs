using UnityEngine;

[DisallowMultipleComponent]
public class face : MonoBehaviour
{
    public enum TargetMode
    {
        Direct,
        ByTag,
        ByName
    }

    [Header("Target")]
    public TargetMode targetMode = TargetMode.Direct;
    [Tooltip("Assign the player transform when using Direct mode.")]
    public Transform player;
    [Tooltip("Used when TargetMode.ByTag")]
    public string playerTag = "Player";
    [Tooltip("Used when TargetMode.ByName")]
    public string playerName = "player";
    [ContextMenu("Find Target Now")]
    public void FindTargetNow() => AutoAssignPlayer();

    [Header("Facing (sprite stays flat)")]
    [Tooltip("Always keep the sprite flat (no X/Z tilt). This prevents stems/pivots from popping up.")]
    public bool keepFlat = true;
    [Tooltip("If true, rotate smoothly.")]
    public bool smooth = true;
    [Range(0.01f, 20f)] public float smoothSpeed = 10f;
    [Tooltip("Optional local offset to aim at (e.g., chest/head).")]
    public Vector3 lookOffset = Vector3.zero;

    [Header("Activation")]
    [Tooltip("If > 0, only face the target when within this range. Set 0 to always face.")]
    public float activationRange = 0f;
    [Tooltip("Try to auto-find the player if null.")]
    public bool tryAutoFind = true;

    void Reset()
    {
        if (playerTag == null) playerTag = "Player";
        if (playerName == null) playerName = "player";
    }

    void Awake()
    {
        AutoAssignPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            if (tryAutoFind) AutoAssignPlayer();
            if (player == null) return;
        }

        // Optional range gating
        if (activationRange > 0f)
        {
            float sqr = (player.position - transform.position).sqrMagnitude;
            if (sqr > activationRange * activationRange) return;
        }

        Vector3 targetWorld = player.position + lookOffset;
        Vector3 dir = targetWorld - transform.position;

        if (keepFlat)
        {
            // remove vertical component so sprite remains flat on the ground
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion desired = Quaternion.LookRotation(dir.normalized, Vector3.up);
            ApplyRotation(desired);
            return;
        }

        // If not forced-flat, do full 3D look (may tilt)
        if (dir.sqrMagnitude < 0.00001f) return;
        Quaternion desiredFull = Quaternion.LookRotation(dir.normalized, Vector3.up);
        ApplyRotation(desiredFull);
    }

    void ApplyRotation(Quaternion desired)
    {
        if (smooth)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        }
        else
        {
            transform.rotation = desired;
        }
    }

    void AutoAssignPlayer()
    {
        if (targetMode == TargetMode.Direct && player != null) return;

        switch (targetMode)
        {
            case TargetMode.Direct:
                // keep inspector assignment
                break;
            case TargetMode.ByTag:
                var go = GameObject.FindGameObjectWithTag(playerTag);
                player = go != null ? go.transform : null;
                break;
            case TargetMode.ByName:
                var go2 = GameObject.Find(playerName);
                player = go2 != null ? go2.transform : null;
                break;
        }
    }
}