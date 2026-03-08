using UnityEngine;

public class MiniMapAutoDirector : MonoBehaviour
{
    public MiniMapDogfightCamera cameraSystem;
    public float switchInterval = 5f;

    float nextSwitch;

    void Update()
    {
        if (cameraSystem == null)
            return;

        if (MiniMapDataBus.Instance == null)
            return;

        if (Time.time < nextSwitch)
            return;

        nextSwitch = Time.time + switchInterval;

        Transform best = FindBestTarget();

        if (best != null)
            cameraSystem.SetTarget(best);
    }

    Transform FindBestTarget()
    {
        if (MiniMapDataBus.Instance == null)
            return null;

        var aircraft = MiniMapDataBus.Instance.aircraft;
        if (aircraft == null || aircraft.Count == 0)
            return null;

        float bestScore = -1f;
        Transform bestTarget = null;

        for (int i = 0; i < aircraft.Count; i++)
        {
            var a = aircraft[i];
            if (a == null) continue;

            float score = ComputeScore(a);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = a.transform;
            }
        }

        return bestTarget;
    }

    float ComputeScore(MiniMapMarker m)
    {
        if (m == null)
            return -1f;

        float score = 0f;

        var rb = m.rb;
        if (rb != null)
            score += rb.velocity.magnitude * 0.5f;

        if (MiniMapDataBus.Instance == null)
            return score;

        var aircraft = MiniMapDataBus.Instance.aircraft;
        if (aircraft == null)
            return score;

        for (int i = 0; i < aircraft.Count; i++)
        {
            var other = aircraft[i];
            if (other == null || other == m) continue;

            float d = Vector3.Distance(
                m.transform.position,
                other.transform.position
            );

            if (d < 0.3f)
                score += 50f;
        }

        return score;
    }
}