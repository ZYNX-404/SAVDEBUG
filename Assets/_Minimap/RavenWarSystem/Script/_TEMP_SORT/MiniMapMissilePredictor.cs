using UnityEngine;

public class MiniMapMissilePredictor : MonoBehaviour
{
    public Rigidbody missileRb;
    public Transform target;

    public LineRenderer line;

    public float predictTime = 2f;
    public int steps = 20;

    public float updateInterval = 0.1f;

    float nextUpdate;

    void Update()
    {
        if (missileRb == null) return;

        if (Time.time < nextUpdate)
            return;

        nextUpdate = Time.time + updateInterval;

        PredictPath();
    }

    void PredictPath()
    {
        int dynamicSteps = steps;

        if (MiniMapViewerSystem.Instance != null)
        {
            float d =
                MiniMapViewerSystem.Instance.DistanceToViewer(
                    missileRb.position
                );

            if (d > 4f)
                dynamicSteps = 6;
            else if (d > 2f)
                dynamicSteps = 12;
        }

        if (line.positionCount != dynamicSteps)
            line.positionCount = dynamicSteps;

        Vector3 pos = missileRb.position;
        Vector3 vel = missileRb.velocity;

        float dt = predictTime / dynamicSteps;

        for (int i = 0; i < dynamicSteps; i++)
        {
            if (target != null)
            {
                Vector3 dir =
                    (target.position - pos).normalized;

                vel = Vector3.Lerp(
                    vel,
                    dir * vel.magnitude,
                    0.15f
                );
            }

            pos += vel * dt;

            line.SetPosition(i, pos);
        }
    }
}