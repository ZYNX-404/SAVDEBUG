using UnityEngine;

public class AITaxi : MonoBehaviour
{
    [Header("Controlled Aircraft")]
    public Transform aircraftTransform;
    public Rigidbody rb;

    [Header("Taxi Path")]
    public Transform[] taxiPoints;
    public float arriveDistance = 8f;

    [Header("Movement")]
    public float minSpeed = 5f;
    public float maxSpeed = 20f;
    public float turnSpeed = 2f;

    int index = 0;

    void Reset()
    {
        TryAutoSetup();
    }

    void Awake()
    {
        TryAutoSetup();
    }

    void FixedUpdate()
    {
        if (aircraftTransform == null || rb == null)
            return;

        if (taxiPoints == null || taxiPoints.Length == 0)
            return;

        if (index >= taxiPoints.Length)
            return;

        Transform target = taxiPoints[index];
        if (target == null)
            return;

        Vector3 dir = target.position - aircraftTransform.position;
        dir.y = 0f;

        float dist = dir.magnitude;

        if (dist < arriveDistance)
        {
            index++;
            return;
        }

        float speed = Mathf.Lerp(minSpeed, maxSpeed, Mathf.Clamp01(dist / 20f));

        Vector3 moveDir = dir.normalized;
        Vector3 desiredVelocity = moveDir * speed;

        rb.velocity = new Vector3(desiredVelocity.x, rb.velocity.y, desiredVelocity.z);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            aircraftTransform.rotation = Quaternion.Slerp(
                aircraftTransform.rotation,
                targetRot,
                Time.fixedDeltaTime * turnSpeed
            );
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
    }
}