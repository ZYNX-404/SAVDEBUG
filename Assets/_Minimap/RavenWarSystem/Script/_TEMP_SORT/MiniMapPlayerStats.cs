using UnityEngine;

public class MiniMapPlayerStats
{
    public MiniMapMarker marker;

    public float dogfightTime;
    public int missileLaunches;
    public int kills;
    public int nearMiss;

    public float aggressionScore;

    public MiniMapPlayerStats(MiniMapMarker m)
    {
        marker = m;
    }
}