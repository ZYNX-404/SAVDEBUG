using UnityEngine;
using System.Collections.Generic;

public class DynamicFrontlineSystem : MonoBehaviour
{
    public MiniMapManager manager;
    public MiniMapCombatClusterSystem clusterSystem;

    public Vector3 frontline;
    public float smooth = 2f;

    void Update()
    {
        Vector3 a = ComputeTeamCenter(0);
        Vector3 b = ComputeTeamCenter(1);

        Vector3 mid = (a + b) * 0.5f;

        // クラスタがあるなら前線をそちらへ寄せる
        if (clusterSystem != null && clusterSystem.clusters.Count > 0)
        {
            CombatCluster strongest = GetStrongestCluster();

            if (strongest != null)
                mid = strongest.center;
        }

        frontline = Vector3.Lerp(frontline, mid, Time.deltaTime * smooth);
    }

    Vector3 ComputeTeamCenter(int team)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var m in manager.aircraft)
        {
            if (m == null) continue;
            if (m.team != team) continue;

            sum += m.WorldPosition;
            count++;
        }

        if (count == 0)
            return Vector3.zero;

        return sum / count;
    }

    CombatCluster GetStrongestCluster()
    {
        CombatCluster best = null;
        float bestScore = 0f;

        foreach (var c in clusterSystem.clusters)
        {
            if (c == null) continue;

            float score = c.aircraft.Count;

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        return best;
    }
}