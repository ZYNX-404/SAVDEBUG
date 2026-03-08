using UnityEngine;
using System.Collections.Generic;


public class MiniMapDataBus : MonoBehaviour
{
    public static MiniMapDataBus Instance;

    public List<MiniMapMarker> aircraft = new List<MiniMapMarker>();
    public List<Rigidbody> missiles = new List<Rigidbody>();

    public List<MiniMapBattleEvent> events =
        new List<MiniMapBattleEvent>();
    public int maxEvents = 200;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Aircraft

    public void RegisterAircraft(MiniMapMarker m)
    {
        if (m == null) return;

        if (!aircraft.Contains(m))
            aircraft.Add(m);
    }

    public void RemoveAircraft(MiniMapMarker m)
    {
        aircraft.Remove(m);
    }

    public List<MiniMapMarker> GetAircraft()
    {
        return aircraft;
    }

    // Missile

    public void RegisterMissile(Rigidbody m)
    {
        if (m == null) return;

        if (!missiles.Contains(m))
            missiles.Add(m);
    }

    public void RemoveMissile(Rigidbody m)
    {
        missiles.Remove(m);
    }

    public List<Rigidbody> GetMissiles()
    {
        return missiles;
    }

    // Events

    public void AddEvent(Vector3 pos, MiniMapEventType type)
    {
        events.Add(new MiniMapBattleEvent(pos, Time.time, type));

        if (events.Count > maxEvents)
            events.RemoveAt(0);
    }

    public List<MiniMapBattleEvent> GetEvents()
    {
        return events;
    }
    public void Cleanup()
    {
        aircraft.RemoveAll(m => m == null);
        missiles.RemoveAll(m => m == null);
    }

}
