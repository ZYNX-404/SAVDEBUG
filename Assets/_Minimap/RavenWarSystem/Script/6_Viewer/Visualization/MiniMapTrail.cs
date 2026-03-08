using UnityEngine;
using System.Collections.Generic;

public class MiniMapTrail : MiniMapIndicator
{
    public LineRenderer line;

    public float distance = 0.02f;
    public int maxPoints = 60;

    List<Vector3> points = new List<Vector3>();
    Vector3 last;

    protected override void UpdateIndicator()
    {
        Vector3 p = transform.position;

        if (Vector3.Distance(p, last) < distance) return;

        last = p;

        points.Add(p);

        if (points.Count > maxPoints)
            points.RemoveAt(0);

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
}