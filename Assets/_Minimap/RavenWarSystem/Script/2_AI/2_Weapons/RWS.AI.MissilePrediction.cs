using UnityEngine;
using System.Collections.Generic;

public class MiniMapMissilePrediction : MonoBehaviour
{
    public LineRenderer linePrefab;

    public float predictTime = 3f;
    public int steps = 12;

    List<LineRenderer> lines = new List<LineRenderer>();

    void Update()
    {
        var missiles = MiniMapDataBus.Instance.GetMissiles();

        EnsureLines(missiles.Count);

        for (int i = 0; i < missiles.Count; i++)
        {
            DrawPrediction(lines[i], missiles[i]);
        }
    }

    void EnsureLines(int count)
    {
        while (lines.Count < count)
        {
            var l = Instantiate(linePrefab, transform);
            lines.Add(l);
        }
    }

    void DrawPrediction(LineRenderer line, Rigidbody rb)
    {
        if (rb == null)
        {
            line.positionCount = 0;
            return;
        }

        Vector3 pos = rb.position;
        Vector3 vel = rb.velocity;

        line.positionCount = steps;

        for (int i = 0; i < steps; i++)
        {
            float t = predictTime * i / (steps - 1);

            Vector3 p = pos + vel * t;

            line.SetPosition(i, p);
        }
    }
}