using UnityEngine;
using System.Collections.Generic;

public class MiniMapFrontLineSystem : MonoBehaviour
{
    public MiniMapCombatHeatmap heatmap;

    public LineRenderer line;

    public int sampleCount = 64;

    public float mapSize = 4f;

    public float heatThreshold = 0.6f;
    
    List<Vector3> smoothedPoints = new List<Vector3>();
    public float smoothing = 0.15f;
    float[] lastFrontZ;
    void Start()
    {
        lastFrontZ = new float[sampleCount];
    }
    void Update()
    {
        if (heatmap == null || line == null)
            return;

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)(sampleCount - 1);

            float x = Mathf.Lerp(-mapSize, mapSize, t);

            float bestZ = 0f;
            float bestHeat = 0f;
            float smooth = 0.6f;

            for (float z = -mapSize; z <= mapSize; z += 0.1f)
            {
                Vector3 pos = new Vector3(x, 0, z);

                float heat = heatmap.GetHeat(pos);

                if (heat > bestHeat)
                {
                    bestHeat = heat;
                    bestZ = z;
                }
            }

            if (bestHeat > heatThreshold)
            {
                float z = bestZ;

                // 前フレームと平均
                z = Mathf.Lerp(lastFrontZ[i], z, smooth);

                // 隣の列と平均
                if (i > 0)
                    z = (z + lastFrontZ[i - 1]) * 0.5f;

                lastFrontZ[i] = z;

                points.Add(new Vector3(x, 0.02f, z));
            }
        }

        if (smoothedPoints.Count != points.Count)
        {
            smoothedPoints = new List<Vector3>(points);
        }

        for (int i = 0; i < points.Count; i++)
        {
            smoothedPoints[i] =
                Vector3.Lerp(smoothedPoints[i], points[i], smoothing);
        }

        line.positionCount = smoothedPoints.Count;

        for (int i = 0; i < smoothedPoints.Count; i++)
        {
            line.SetPosition(i, smoothedPoints[i]);
        }
    }
}