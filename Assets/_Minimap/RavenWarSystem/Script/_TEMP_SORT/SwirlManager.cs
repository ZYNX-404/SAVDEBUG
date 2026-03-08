using UnityEngine;
using System.Collections.Generic;

public class SwirlManager : MonoBehaviour
{
    public MiniMapDogfightSwirlPool pool;

    public float dogfightDistance = 0.08f;
    public float heightOffset = 0.01f;

    void Update()
    {
        if (MiniMapDataBus.Instance == null) return;

        var aircraft = MiniMapDataBus.Instance.aircraft;

        if (aircraft == null || aircraft.Count < 2)
        {
            pool.ResetAll();
            return;
        }

        pool.ResetAll();

        for (int i = 0; i < aircraft.Count; i++)
        {
            for (int j = i + 1; j < aircraft.Count; j++)
            {
                var a = aircraft[i];
                var b = aircraft[j];

                if (a == null || b == null) continue;

                float d = (a.transform.position - b.transform.position).sqrMagnitude;
                if (d > dogfightDistance * dogfightDistance) continue;

                var swirl = pool.Get();
                if (swirl == null) return;

                Vector3 center =
                    (a.transform.position +
                     b.transform.position) * 0.5f;

                center.y += heightOffset;

                swirl.transform.position = center;

                float intensity =
                    ComputeTurnIntensity(a) +
                    ComputeTurnIntensity(b);

                swirl.SetIntensity(intensity);
            }
        }
    }

    float ComputeTurnIntensity(MiniMapMarker m)
    {
        var indicator = m.GetComponent<AircraftVectorIndicator>();

        if (indicator == null) return 0f;

        var line = indicator.line;

        if (line == null || line.positionCount < 3)
            return 0f;

        int n = line.positionCount;

        Vector3 a = line.GetPosition(n - 3);
        Vector3 b = line.GetPosition(n - 2);
        Vector3 c = line.GetPosition(n - 1);

        Vector3 ab = (b - a).normalized;
        Vector3 bc = (c - b).normalized;

        float angle = Vector3.Angle(ab, bc);

        return angle / 45f;
    }
}