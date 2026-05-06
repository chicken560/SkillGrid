using UnityEngine;

[DisallowMultipleComponent]
public class FloatingEnemy : MonoBehaviour
{
    [Header("Wander")]
    [Tooltip("Center point for wandering (if null, uses initial position)")]
    public Transform wanderCenter;
    [Tooltip("Maximum horizontal radius from center")]
    public float wanderRadius = 6f;
    [Tooltip("How often the enemy picks a new wander target (seconds)")]
    public float changeTargetInterval = 4f;
    [Tooltip("Overall speed of movement")]
    public float wanderSpeed = 1.6f;
    [Tooltip("How quickly the enemy turns to face its movement direction")]
    public float turnSpeed = 6f;

    [Header("Vertical drift / bob")]
    public float bobAmplitude = 0.35f;
    public float bobFrequency = 0.9f;
    [Tooltip("Small vertical wandering range (added to bob)")]
    public float verticalDrift = 0.8f;

    [Header("Perlin noise drift")]
    [Tooltip("Perlin noise multiplier for organic drifting (x/z)")]
    public float noiseStrength = 0.6f;
    [Tooltip("Noise speed multiplier")]
    public float noiseSpeed = 0.25f;
    [Tooltip("Seed for noise so multiple enemies differ")]
    public float noiseSeed = 0.0f;

    [Header("Visual / Static effect")]
    [Tooltip("Renderer whose material will be animated for 'static' shimmer. If left empty, visual effects are skipped.")]
    public Renderer visualRenderer;
    [Tooltip("Texture scroll speed for the material's main texture")]
    public Vector2 staticScrollSpeed = new Vector2(0.08f, -0.06f);
    [Tooltip("How strong the shimmer/emission is (min,max)")]
    public Vector2 staticEmissionRange = new Vector2(0.04f, 0.45f);
    [Tooltip("How fast the static pulse is")]
    public float staticPulseSpeed = 1.9f;

    [Header("Misc")]
    [Tooltip("Small uniform scale pulse while drifting")]
    public float scalePulse = 0.03f;
    [Tooltip("How fast the visual rotates slowly")]
    public float visualSpinSpeed = 12f;

    // internals
    private Vector3 _targetPosition;
    private Vector3 _wanderCenterPos;
    private float _targetTimer;
    private Vector3 _velocity = Vector3.zero;
    private Material _instancedMaterial;
    private Vector3 _visualBaseLocalPos;
    private Vector3 _visualBaseLocalScale;
    private Quaternion _visualBaseLocalRot;
    private float _noiseOffsetX;
    private float _noiseOffsetZ;

    void Awake()
    {
        if (wanderCenter == null)
        {
            // use a temporary empty transform at this position (keeps inspector clean)
            GameObject go = new GameObject($"{name}_WanderCenter");
            go.transform.position = transform.position;
            wanderCenter = go.transform;
            // hide created object in hierarchy
            go.hideFlags = HideFlags.DontSave;
        }

        _wanderCenterPos = wanderCenter.position;
        PickNewTarget();

        if (visualRenderer != null)
        {
            // use an instance of the material so we don't modify shared material
            _instancedMaterial = visualRenderer.material;
        }

        _visualBaseLocalPos = visualRenderer != null ? visualRenderer.transform.localPosition : Vector3.zero;
        _visualBaseLocalScale = visualRenderer != null ? visualRenderer.transform.localScale : Vector3.one;
        _visualBaseLocalRot = visualRenderer != null ? visualRenderer.transform.localRotation : Quaternion.identity;

        // randomize per-enemy noise seed offsets
        _noiseOffsetX = Random.value * 100f + noiseSeed;
        _noiseOffsetZ = Random.value * 100f + noiseSeed * 13f;
    }

    void Update()
    {
        // update wander center if user changed transform
        _wanderCenterPos = wanderCenter.position;

        // timer
        _targetTimer += Time.deltaTime;
        if (_targetTimer >= changeTargetInterval)
        {
            PickNewTarget();
            _targetTimer = 0f;
        }

        // Perlin noise based drift (horizontal)
        float t = Time.time * noiseSpeed;
        float nx = (Mathf.PerlinNoise(t + _noiseOffsetX, _noiseOffsetX) - 0.5f) * 2f;
        float nz = (Mathf.PerlinNoise(t + _noiseOffsetZ, _noiseOffsetZ) - 0.5f) * 2f;
        Vector3 noiseOffset = new Vector3(nx, 0f, nz) * noiseStrength;

        // bobbing + small vertical drift
        float bob = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        float vdrift = (Mathf.PerlinNoise(Time.time * 0.13f + _noiseOffsetX * 0.1f, _noiseOffsetZ * 0.1f) - 0.5f) * 2f * verticalDrift;

        // compute desired position
        Vector3 desired = _targetPosition + noiseOffset;
        desired.y = _wanderCenterPos.y + bob + vdrift;

        // smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, 0.8f / Mathf.Max(0.0001f, wanderSpeed), Mathf.Infinity, Time.deltaTime);

        // gentle facing direction based on velocity
        Vector3 flatVel = _velocity;
        flatVel.y = 0f;
        if (flatVel.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatVel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed * 0.55f);
        }
        else
        {
            // slow idle spin so it doesn't feel static
            transform.rotation *= Quaternion.Euler(0f, visualSpinSpeed * Time.deltaTime * 0.08f, 0f);
        }

        // visual effects
        if (_instancedMaterial != null && visualRenderer != null)
        {
            // scroll main texture if present
            if (_instancedMaterial.HasProperty("_MainTex"))
            {
                Vector2 offset = _instancedMaterial.mainTextureOffset;
                offset += staticScrollSpeed * Time.deltaTime;
                _instancedMaterial.mainTextureOffset = offset;
            }

            // emission / shimmer driven by Perlin pulse
            if (_instancedMaterial.HasProperty("_EmissionColor"))
            {
                float pulse = (Mathf.PerlinNoise(Time.time * staticPulseSpeed + _noiseOffsetX, _noiseOffsetZ) );
                float emission = Mathf.Lerp(staticEmissionRange.x, staticEmissionRange.y, pulse);
                Color baseEmission = Color.white * emission;
                _instancedMaterial.SetColor("_EmissionColor", baseEmission);
                _instancedMaterial.EnableKeyword("_EMISSION");
            }
        }

        // subtle scale pulse on visual root to feel 'breathy'
        if (visualRenderer != null)
        {
            float sp = 1f + Mathf.Sin(Time.time * (0.6f + noiseSpeed)) * scalePulse;
            visualRenderer.transform.localScale = _visualBaseLocalScale * sp;
            // keep visual positioned relative to root base (no teleport)
            visualRenderer.transform.localPosition = _visualBaseLocalPos + (Vector3.up * (bob * 0.04f));
            visualRenderer.transform.localRotation = _visualBaseLocalRot * Quaternion.Euler(Mathf.PerlinNoise(Time.time * 0.4f + _noiseOffsetX, 0f) * 6f - 3f, 0f, 0f);
        }
    }

    private void PickNewTarget()
    {
        Vector2 circle = Random.insideUnitCircle * wanderRadius;
        // keep new target near wander center horizontally, allow small random vertical within verticalDrift
        _targetPosition = _wanderCenterPos + new Vector3(circle.x, 0f, circle.y);
        _targetPosition.y = _wanderCenterPos.y;
    }

    void OnDrawGizmosSelected()
    {
        // draw wander radius
        Gizmos.color = Color.cyan * 0.8f;
        Vector3 c = (wanderCenter != null) ? wanderCenter.position : transform.position;
        Gizmos.DrawWireSphere(new Vector3(c.x, c.y, c.z), wanderRadius);
    }
}