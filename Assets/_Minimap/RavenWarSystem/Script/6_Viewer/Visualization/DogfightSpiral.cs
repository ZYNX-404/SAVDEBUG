using UnityEngine;
using System.Collections.Generic;

public class DogfightSpiral : MonoBehaviour
{
    public LineRenderer line;

    public int maxPoints = 40;

    List<Vector3> points = new List<Vector3>();

    public void UpdateSpiral(List<Vector3> trail)
    {
        if (trail == null || trail.Count < 3)
        {
            line.positionCount = 0;
            return;
        }

        points.Clear();

        int start = Mathf.Max(0, trail.Count - maxPoints);

        for (int i = start; i < trail.Count; i++)
        {
            Vector3 p = trail[i];
            points.Add(p);
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
}