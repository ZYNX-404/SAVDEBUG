using System.Collections.Generic;
using UnityEngine;

public class CombatCluster
{
    public List<MiniMapMarker> aircraft = new List<MiniMapMarker>();

    public Vector3 center;
    public float radius;

    public int size;

    public int teamA;
    public int teamB;

    public float intensity;
    public float lastUpdate;
    public float age;

    public Vector3 velocity; // ← これ追加
    
}

public class MiniMapCombatCluster : MonoBehaviour
{
    public List<CombatCluster> clusters = new List<CombatCluster>();
    public MiniMapDogfightDetection dogfightDetection;
    public float mergeDistance = 600f;
    public int splitThreshold = 8;

    void Update()
    {
        BuildClusters();
        
        MergeClusters();
        SplitClusters();
    }

    void BuildClusters()
    {
        clusters.Clear();

        if (dogfightDetection == null)
            return;

        var pairs = dogfightDetection.pairs;

        HashSet<MiniMapMarker> visited = new HashSet<MiniMapMarker>();

        foreach (var pair in pairs)
        {
            if (pair == null || pair.a == null || pair.b == null)
                continue;

            if (visited.Contains(pair.a) && visited.Contains(pair.b))
                continue;

            CombatCluster cluster = new CombatCluster();

            Queue<MiniMapMarker> queue = new Queue<MiniMapMarker>();

            queue.Enqueue(pair.a);
            queue.Enqueue(pair.b);

            while (queue.Count > 0)
            {
                MiniMapMarker marker = queue.Dequeue();

                if (marker == null || visited.Contains(marker))
                    continue;

                visited.Add(marker);
                cluster.aircraft.Add(marker);

                foreach (var p in pairs)
                {
                    if (p == null || p.a == null || p.b == null)
                        continue;

                    if (p.a == marker && !visited.Contains(p.b))
                        queue.Enqueue(p.b);

                    if (p.b == marker && !visited.Contains(p.a))
                        queue.Enqueue(p.a);
                }
            }

        cluster.size = cluster.aircraft.Count;

        ComputeClusterCenter(cluster);

        ComputeTeamBalance(cluster);

        clusters.Add(cluster);
        }
    }

    void ComputeClusterCenter(CombatCluster cluster)
    {
        if (cluster.aircraft.Count == 0)
            return;

        Vector3 center = Vector3.zero;

        foreach (var marker in cluster.aircraft)
        {
            center += marker.transform.position;
        }

        center /= cluster.aircraft.Count;

        cluster.center = center;

        float maxDist = 0f;

        foreach (var marker in cluster.aircraft)
        {
            float d = Vector3.Distance(center, marker.transform.position);

            if (d > maxDist)
                maxDist = d;
        }

        cluster.radius = maxDist;
    }
    void ComputeTeamBalance(CombatCluster cluster)
    {
        cluster.teamA = 0;
        cluster.teamB = 0;

        foreach (var m in cluster.aircraft)
        {
            if (m.team == 0)
                cluster.teamA++;

            if (m.team == 1)
                cluster.teamB++;
        }

        cluster.intensity =
            cluster.size +
            Mathf.Abs(cluster.teamA - cluster.teamB) * 0.5f;
    }
    public CombatCluster GetStrongestCluster()
    {
        CombatCluster best = null;
        int bestSize = 0;

        foreach (var c in clusters)
        {
            if (c == null) continue;

            if (c.size > bestSize)
            {
                bestSize = c.size;
                best = c;
            }
        }

        return best;
    }
    void MergeClusters()
    {
        for (int i = 0; i < clusters.Count; i++)
        {
            var a = clusters[i];
            if (a == null) continue;

            for (int j = i + 1; j < clusters.Count; j++)
            {
                var b = clusters[j];
                if (b == null) continue;

                float d = Vector3.Distance(a.center, b.center);

                if (d < mergeDistance)
                {
                    a.aircraft.AddRange(b.aircraft);
                    clusters.RemoveAt(j);
                    j--;

                    ComputeClusterCenter(a);
                }
            }
        }
    }
    void SplitClusters()
    {
        List<CombatCluster> newClusters = new List<CombatCluster>();

        foreach (var c in clusters)
        {
            if (c.aircraft.Count < splitThreshold)
                continue;

            CombatCluster a = new CombatCluster();
            CombatCluster b = new CombatCluster();

            foreach (var m in c.aircraft)
            {
                if (Random.value > 0.5f)
                    a.aircraft.Add(m);
                else
                    b.aircraft.Add(m);
            }

            ComputeClusterCenter(a);
            ComputeClusterCenter(b);

            newClusters.Add(a);
            newClusters.Add(b);
        }

        clusters.AddRange(newClusters);
    }
}
