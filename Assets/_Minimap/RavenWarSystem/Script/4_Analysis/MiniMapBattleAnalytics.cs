using UnityEngine;
using System.Collections.Generic;

public class MiniMapBattleAnalytics : MonoBehaviour
{
    public Dictionary<MiniMapMarker, MiniMapPlayerStats> stats =
        new Dictionary<MiniMapMarker, MiniMapPlayerStats>();
    // public MiniMapIslandManager islandManager;

    void Update()
    {
        var aircraft = MiniMapDataBus.Instance.GetAircraft();

        foreach (var a in aircraft)
        {
            if (a == null) continue;

            if (!stats.ContainsKey(a))
                stats[a] = new MiniMapPlayerStats(a);

            AnalyzeAircraft(stats[a]);
        }
    }

    void AnalyzeAircraft(MiniMapPlayerStats s)
    {
        if (s.marker.rb == null) return;

        float speed = s.marker.rb.velocity.magnitude;

        // var island =
        //     islandManager.GetIsland(
        //         s.marker.transform.position
        //     );

        int value = 0;

        // if (island != null)
        //     value = island.tacticalValue;

        s.aggressionScore =
            s.dogfightTime * 2f +
            s.missileLaunches * 3f +
            speed * 0.01f +
            value * 2f;
    }

    public MiniMapPlayerStats GetMostAggressive()
    {
        float best = 0;
        MiniMapPlayerStats result = null;

        foreach (var s in stats.Values)
        {
            if (s.aggressionScore > best)
            {
                best = s.aggressionScore;
                result = s;
            }
        }

        return result;
    }
}