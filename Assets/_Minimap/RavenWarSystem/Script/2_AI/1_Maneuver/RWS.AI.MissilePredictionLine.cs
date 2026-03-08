using UnityEngine;

public class MiniMapMissilePredictionLine : MonoBehaviour
{
    public Rigidbody missileRb;
    public Transform target;

    public LineRenderer line;

    public float predictTime = 2f;

    void Update()
    {
        if (missileRb == null) return;

        Vector3 pos = missileRb.position;
        Vector3 vel = missileRb.velocity;

        Vector3 future = pos + vel * predictTime;

        if (target != null)
        {
            Vector3 dir = (target.position - pos).normalized;
            future += dir * vel.magnitude * 0.5f;
        }

        line.SetPosition(0, pos);
        line.SetPosition(1, future);
    }
}