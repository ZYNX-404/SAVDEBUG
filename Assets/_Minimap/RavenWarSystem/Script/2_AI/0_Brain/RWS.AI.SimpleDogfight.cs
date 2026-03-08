using UnityEngine;

public class SimpleDogfightAI : MonoBehaviour
{
    [Header("Controlled Aircraft")]
    public Transform aircraftTransform;
    public Rigidbody rb;

    [Header("Flight")]
    public float speed = 35f;
    public float turnSpeed = 2.5f;

    [Header("Target Search")]
    public float searchRange = 2000f;

    [Header("Dogfight")]
    public float idealDistance = 250f;
    public float distanceTolerance = 80f;
    public float orbitStrength = 0.6f;

    [Header("Weapons")]
    public AIMissileLauncher missileLauncher;

    Transform target;
    float attackOffset;

    void Reset()
    {
        TryAutoSetup();
    }

    void Awake()
    {
        TryAutoSetup();
    }

    void Start()
    {
        attackOffset = Random.Range(-250f, 250f);
    }

    void Update()
    {
        if (!HasValidSetup())
            return;

        FindTarget();

        if (target != null)
        {
            float d = Vector3.Distance(aircraftTransform.position, target.position);

            if (d > searchRange * 1.5f)
                target = null;
        }

        if (target == null)
            return;

        Vector3 toTarget = target.position - aircraftTransform.position;
        Vector3 dir = toTarget.normalized;

        // 横方向ベクトル
        Vector3 side = Vector3.Cross(Vector3.up, dir);

        // 未来位置予測
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        Vector3 enemyVel = targetRb != null ? targetRb.velocity : Vector3.zero;

        float dist = Vector3.Distance(aircraftTransform.position, target.position);
        float mySpeed = rb.velocity.magnitude + 1f;

        float leadTime = dist / mySpeed;
        leadTime = Mathf.Clamp(leadTime, 0f, 3f);

        Vector3 futurePos = target.position + enemyVel * leadTime;

        // 攻撃オフセット
        Vector3 aimPoint = futurePos + side * attackOffset;
        Vector3 toAim = aimPoint - aircraftTransform.position;

        dir = toAim.normalized;

        // 距離制御
        float distControl = 0f;

        if (dist > idealDistance + distanceTolerance)
            distControl = 0.4f;
        else if (dist < idealDistance - distanceTolerance)
            distControl = -0.4f;

        Vector3 desiredDir = dir + side * orbitStrength + dir * distControl;
        desiredDir.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(desiredDir);

        aircraftTransform.rotation = Quaternion.Slerp(
            aircraftTransform.rotation,
            targetRot,
            turnSpeed * Time.deltaTime
        );

        rb.velocity = aircraftTransform.forward * speed;

        if (missileLauncher != null)
        {
            missileLauncher.TryFire(target);
        }
    }

    void FindTarget()
    {
        if (target != null)
            return;

        if (MiniMapDataBus.Instance == null)
            return;

        var aircraft = MiniMapDataBus.Instance.GetAircraft();
        float best = float.MaxValue;

        foreach (var a in aircraft)
        {
            if (a == null)
                continue;

            Transform candidate = a.transform;

            if (candidate == aircraftTransform)
                continue;

            float d = Vector3.Distance(aircraftTransform.position, candidate.position);

            if (d < best && d < searchRange)
            {
                best = d;
                target = candidate;
            }
        }
    }

    void TryAutoSetup()
    {
        if (aircraftTransform == null)
        {
            Rigidbody parentRb = GetComponentInParent<Rigidbody>();

            if (parentRb != null)
                aircraftTransform = parentRb.transform;
        }

        if (rb == null)
        {
            if (aircraftTransform != null)
                rb = aircraftTransform.GetComponent<Rigidbody>();

            if (rb == null)
                rb = GetComponentInParent<Rigidbody>();
        }

        if (missileLauncher == null)
            missileLauncher = GetComponentInParent<AIMissileLauncher>();
    }

    bool HasValidSetup()
    {
        if (aircraftTransform == null)
        {
            Debug.LogWarning($"[{name}] SimpleDogfightAI: aircraftTransform is not assigned.", this);
            return false;
        }

        if (rb == null)
        {
            Debug.LogWarning($"[{name}] SimpleDogfightAI: Rigidbody is not assigned.", this);
            return false;
        }

        return true;
    }
}