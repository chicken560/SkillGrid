using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class FloatingEnemy : MonoBehaviour
{
    [Header("Wander")]
    public Transform wanderCenter;
    public float wanderRadius = 6f;
    public float changeTargetInterval = 4f;
    [Tooltip("How fast the agent moves horizontally (NavMeshAgent speed will be set to match).")]
    public float wanderSpeed = 1.6f;
    public float turnSpeed = 6f;

    [Header("Vertical / drift")]
    public float bobAmplitude = 0.35f;
    public float bobFrequency = 0.9f;
    [Tooltip("Vertical noise amount")]
    public float verticalDrift = 0.8f;
    public float minHeightOffset = -1.5f;
    public float maxHeightOffset = 3.5f;

    [Header("Perlin noise")]
    public float noiseStrength = 0.6f;
    public float noiseSpeed = 0.25f;
    public float noiseSeed = 0f;

    [Header("NavMesh hover")]
    public bool useNavAgentIfAvailable = true;
    [Tooltip("Height above the NavMesh surface to maintain")]
    public float hoverHeight = 1.2f;
    [Tooltip("Radius used when sampling NavMesh near target XZ")]
    public float navSampleRadius = 2f;

    [Header("Grounding fallback")]
    public bool useGrounding = true;
    public LayerMask groundLayer = ~0;
    public float groundProbeHeight = 3f;
    public float groundOffset = 0.18f;

    [Header("Visual")]
    public Renderer visualRenderer;
    public Vector2 staticScrollSpeed = new Vector2(0.08f, -0.06f);
    public Vector2 staticEmissionRange = new Vector2(0.04f, 0.45f);
    public float staticPulseSpeed = 1.9f;
    public float scalePulse = 0.03f;
    public float visualSpinSpeed = 12f;

    // internals
    private NavMeshAgent _agent;
    private Vector3 _wanderCenterPos;
    private Vector3 _targetPosition;
    private float _targetTimer;
    private Vector3 _smoothVel = Vector3.zero;
    private Vector3 _horizontalVel = Vector3.zero;
    private float _verticalSmoothVel;
    private float _currentDesiredY;
    private float _smoothedY;
    private float _noiseX;
    private float _noiseZ;
    private Material _matInstance;
    private Vector3 _visualBaseLocalPos;
    private Vector3 _visualBaseLocalScale;
    private Quaternion _visualBaseLocalRot;
    private float _lastSetTargetTime;

    // tuning for smoothing (exposed if you want later)
    private const float Y_SMOOTH_TIME = 0.35f;
    private const float XY_SMOOTH_TIME = 0.35f;

    void Awake()
    {
        if (wanderCenter == null)
        {
            var go = new GameObject($"{name}_WanderCenter");
            go.transform.position = transform.position;
            wanderCenter = go.transform;
            go.hideFlags = HideFlags.DontSave;
        }

        _wanderCenterPos = wanderCenter.position;
        PickNewTarget();

        if (visualRenderer != null)
        {
            _matInstance = visualRenderer.material;
            _visualBaseLocalPos = visualRenderer.transform.localPosition;
            _visualBaseLocalScale = visualRenderer.transform.localScale;
            _visualBaseLocalRot = visualRenderer.transform.localRotation;
        }

        _noiseX = Random.value * 100f + noiseSeed;
        _noiseZ = Random.value * 100f + noiseSeed * 13f;

        if (useNavAgentIfAvailable)
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                // Important: let agent compute pathing but don't let it move the transform Y.
                // To avoid jitter we keep updatePosition=false and updateRotation=false,
                // then we smoothly drive transform position to agent.nextPosition.xz + smoothed Y.
                _agent.updatePosition = false;
                _agent.updateRotation = false;
                _agent.speed = Mathf.Max(0.01f, wanderSpeed);
                _agent.autoBraking = false;
            }
        }

        // init smoothed Y
        _smoothedY = transform.position.y;
    }

    void Start()
    {
        if (_agent != null && !_agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                _agent.Warp(hit.position);
        }
    }

    void Update()
    {
        _wanderCenterPos = wanderCenter.position;

        // pick new wander target periodically or if agent has reached destination
        _targetTimer += Time.deltaTime;
        bool needNewTarget = false;
        if (_targetTimer >= changeTargetInterval)
            needNewTarget = true;

        if (_agent != null)
        {
            // if no path or close to destination request a new one
            if (!_agent.hasPath || _agent.pathPending || _agent.pathStatus != NavMeshPathStatus.PathComplete ||
                (_agent.remainingDistance <= Mathf.Max(0.2f, _agent.stoppingDistance + 0.1f)))
            {
                needNewTarget = true;
            }
        }
        else
        {
            Vector3 toTarget = _targetPosition - transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude < 0.5f) needNewTarget = true;
        }

        if (needNewTarget)
        {
            // Prefer sampling on NavMesh so movement is reachable
            Vector3 navPoint;
            if (SampleRandomPointOnNavMesh(_wanderCenterPos, wanderRadius, out navPoint))
            {
                _targetPosition = new Vector3(navPoint.x, _wanderCenterPos.y, navPoint.z);
                if (_agent != null)
                    _agent.SetDestination(new Vector3(_targetPosition.x, _agent.transform.position.y, _targetPosition.z));
            }
            else
            {
                Vector2 c = Random.insideUnitCircle * wanderRadius;
                _targetPosition = _wanderCenterPos + new Vector3(c.x, 0f, c.y);
                if (_agent != null)
                    _agent.SetDestination(new Vector3(_targetPosition.x, _agent.transform.position.y, _targetPosition.z));
            }

            _targetTimer = 0f;
            _lastSetTargetTime = Time.time;
        }

        // perlin noise offsets and bob
        float tt = Time.time * noiseSpeed;
        float nx = (Mathf.PerlinNoise(tt + _noiseX, _noiseX) - 0.5f) * 2f;
        float nz = (Mathf.PerlinNoise(tt + _noiseZ, _noiseZ) - 0.5f) * 2f;
        Vector3 noiseOffset = new Vector3(nx, 0f, nz) * noiseStrength;

        float bob = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        float vdrift = (Mathf.PerlinNoise(Time.time * 0.13f + _noiseX * 0.1f, _noiseZ * 0.1f) - 0.5f) * 2f * verticalDrift;

        // desired XZ based on target + noise
        Vector3 desiredXZ = _targetPosition + noiseOffset;
        Vector3 offset = desiredXZ - _wanderCenterPos;
        offset.y = 0f;
        if (offset.magnitude > wanderRadius)
            desiredXZ = _wanderCenterPos + offset.normalized * wanderRadius;

        // compute desiredY. Prefer NavMesh sample under desired XZ if agent exists
        float desiredY = _wanderCenterPos.y + bob + vdrift;
        if (_agent != null && _agent.isOnNavMesh)
        {
            NavMeshHit navHit;
            Vector3 samplePos = new Vector3(desiredXZ.x, _wanderCenterPos.y + 1f, desiredXZ.z);
            if (NavMesh.SamplePosition(samplePos, out navHit, navSampleRadius, NavMesh.AllAreas))
                desiredY = navHit.position.y + hoverHeight + bob * 0.25f + vdrift * 0.25f;
        }
        else if (useGrounding)
        {
            float groundY;
            Vector3 sample = transform.position;
            if (TryGetGroundHeight(sample, out groundY))
                desiredY = Mathf.Max(desiredY, groundY + groundOffset);
        }

        desiredY = Mathf.Clamp(desiredY, _wanderCenterPos.y + minHeightOffset, _wanderCenterPos.y + maxHeightOffset);
        _currentDesiredY = desiredY;

        // horizontal velocity sample for rotation smoothing
        if (_agent != null && _agent.hasPath)
            _horizontalVel = new Vector3(_agent.velocity.x, 0f, _agent.velocity.z);
        else
            _horizontalVel = new Vector3(_smoothVel.x, 0f, _smoothVel.z);

        // face movement direction
        Vector3 flatVel = _horizontalVel;
        if (flatVel.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatVel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }
        else
        {
            transform.rotation *= Quaternion.Euler(0f, visualSpinSpeed * Time.deltaTime * 0.08f, 0f);
        }

        // visual effects
        if (_matInstance != null && visualRenderer != null)
        {
            if (_matInstance.HasProperty("_MainTex"))
            {
                Vector2 off = _matInstance.mainTextureOffset;
                off += staticScrollSpeed * Time.deltaTime;
                _matInstance.mainTextureOffset = off;
            }
            if (_matInstance.HasProperty("_EmissionColor"))
            {
                float pulse = Mathf.PerlinNoise(Time.time * staticPulseSpeed + _noiseX, _noiseZ);
                float emission = Mathf.Lerp(staticEmissionRange.x, staticEmissionRange.y, pulse);
                _matInstance.SetColor("_EmissionColor", Color.white * emission);
                _matInstance.EnableKeyword("_EMISSION");
            }
        }

        if (visualRenderer != null)
        {
            float sp = 1f + Mathf.Sin(Time.time * (0.6f + noiseSpeed)) * scalePulse;
            visualRenderer.transform.localScale = _visualBaseLocalScale * sp;
            visualRenderer.transform.localPosition = _visualBaseLocalPos + Vector3.up * (bob * 0.04f);
            visualRenderer.transform.localRotation = _visualBaseLocalRot * Quaternion.Euler((Mathf.PerlinNoise(Time.time * 0.4f + _noiseX, 0f) * 6f) - 3f, 0f, 0f);
        }
    }

    void LateUpdate()
    {
        // Smoothly apply hover Y and XZ from agent.nextPosition (agent computes path but does not move transform)
        Vector3 agentPos = (_agent != null && _agent.isOnNavMesh) ? _agent.nextPosition : transform.position;
        Vector3 target = new Vector3(agentPos.x, _currentDesiredY, agentPos.z);

        // Smooth horizontal/vertical separately to avoid snapping.
        float smoothTime = XY_SMOOTH_TIME / Mathf.Max(0.1f, wanderSpeed);
        Vector3 newPos = Vector3.SmoothDamp(transform.position, target, ref _smoothVel, smoothTime, Mathf.Infinity, Time.deltaTime);

        // Smooth Y a bit more conservatively
        _smoothedY = Mathf.SmoothDamp(transform.position.y, _currentDesiredY, ref _verticalSmoothVel, Y_SMOOTH_TIME, Mathf.Infinity, Time.deltaTime);
        newPos.y = _smoothedY;

        transform.position = newPos;

        // Keep agent internally synced to our XZ to avoid it correcting us (prevent tug-of-war)
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.nextPosition = new Vector3(transform.position.x, agentPos.y, transform.position.z);
        }
    }

    private bool SampleRandomPointOnNavMesh(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 22; i++)
        {
            Vector3 randomPoint = center + (Vector3)(Random.insideUnitCircle * radius);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    private bool TryGetGroundHeight(Vector3 samplePos, out float groundY)
    {
        RaycastHit hit;
        Vector3 origin = samplePos + Vector3.up * groundProbeHeight;
        if (Physics.Raycast(origin, Vector3.down, out hit, groundProbeHeight * 2f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            groundY = hit.point.y;
            return true;
        }
        groundY = 0f;
        return false;
    }

    private void PickNewTarget()
    {
        Vector2 c = Random.insideUnitCircle * wanderRadius;
        _targetPosition = _wanderCenterPos + new Vector3(c.x, 0f, c.y);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 c = (wanderCenter != null) ? wanderCenter.position : transform.position;
        Gizmos.color = Color.cyan * 0.8f;
        Gizmos.DrawWireSphere(c, wanderRadius);
    }
}