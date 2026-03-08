using UnityEngine;

public class MiniMapBattleEvent
{
    public Vector3 position;
    public float time;
    public MiniMapEventType type;

    public MiniMapBattleEvent(Vector3 pos, float t, MiniMapEventType tp)
    {
        position = pos;
        time = t;
        type = tp;
    }
}