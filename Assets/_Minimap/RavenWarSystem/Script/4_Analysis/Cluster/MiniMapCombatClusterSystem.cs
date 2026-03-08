using System.Collections.Generic;
using UnityEngine;

public class MiniMapCombatClusterSystem : MonoBehaviour
{
    public MiniMapCombatCluster source;

    public List<CombatCluster> clusters = new List<CombatCluster>();

    void Update()
    {
        clusters.Clear();

        if (source == null)
            return;

        foreach (var c in source.clusters)
        {
            if (c == null) continue;
                ComputeClusterCenter(c);
                clusters.Add(c);
        }
    }
    void ComputeClusterCenter(CombatCluster cluster)
    {
        if (cluster.aircraft.Count == 0)
            return;

        Vector3 center = Vector3.zero;

        foreach (var marker in cluster.aircraft)
        {
            if (marker == null) continue;
            center += marker.transform.position;
        }

        center /= cluster.aircraft.Count;

        cluster.center = center;

        float maxDist = 0f;

        foreach (var marker in cluster.aircraft)
        {
            if (marker == null) continue;

            float d = Vector3.Distance(center, marker.transform.position);

            if (d > maxDist)
                maxDist = d;
        }

        cluster.radius = maxDist;

        cluster.intensity =
            cluster.aircraft.Count / Mathf.Max(cluster.radius, 0.01f);

        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var marker in cluster.aircraft)
        {
            if (marker == null || marker.rb == null) continue;

            sum += marker.rb.velocity;
            count++;
        }

        cluster.velocity = count > 0 ? sum / count : Vector3.zero;
    }
}