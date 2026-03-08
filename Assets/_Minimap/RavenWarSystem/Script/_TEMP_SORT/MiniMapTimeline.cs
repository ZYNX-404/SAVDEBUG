using UnityEngine;
using System.Collections.Generic;

public class MiniMapTimeline : MonoBehaviour
{
    public float historyTime = 30f;

    List<MiniMapBattleEvent> events = new List<MiniMapBattleEvent>();

    public void AddEvent(Vector3 pos, MiniMapEventType type)
    {
        MiniMapBattleEvent e =
            new MiniMapBattleEvent(pos, Time.time, type);

        events.Add(e);
    }

    void Update()
    {
        float limit = Time.time - historyTime;

        for (int i = events.Count - 1; i >= 0; i--)
        {
            if (events[i].time < limit)
                events.RemoveAt(i);
        }
    }

    public List<MiniMapBattleEvent> GetEvents()
    {
        return events;
    }
}