using UnityEngine;

public class MiniMapZoneManager : MonoBehaviour
{
    public MiniMapAirspaceZone[] zones;

    public MiniMapAIDirector director;

    float timer;
    void Awake()
    {
        zones = FindObjectsOfType<MiniMapAirspaceZone>();
    }
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0.5f;
            CheckZones();
        }
    }

    void CheckZones()
    {
        var aircraft = MiniMapDataBus.Instance.GetAircraft();

        foreach (var z in zones)
        {
            int friendly = 0;
            int enemy = 0;

            foreach (var m in aircraft)
            {
                if (!z.IsInside(m.transform.position))
                    continue;

                if (m.team == 0)
                    friendly++;
                else
                    enemy++;
            }

            if (friendly > enemy)
                z.control = AirspaceControl.Friendly;
            else if (enemy > friendly)
                z.control = AirspaceControl.Enemy;
            else if (friendly > 0)
                z.control = AirspaceControl.Contested;
            else
                z.control = AirspaceControl.Neutral;
        }
    }

    void OnEnterZone(MiniMapMarker aircraft, MiniMapAirspaceZone zone)
    {
        if (director != null)
        {
            director.RegisterEventFocus(zone.transform.position);
        }
    }
}