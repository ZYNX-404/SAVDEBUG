using UnityEngine;
using System.Collections.Generic;

public enum AirspaceControl
{
    Neutral,
    Friendly,
    Enemy,
    Contested
}

public class MiniMapAirspaceSystem : MonoBehaviour
{
    public List<MiniMapAirspaceZone> zones = new List<MiniMapAirspaceZone>();

    float timer;

    void Start()
    {
        zones.AddRange(FindObjectsOfType<MiniMapAirspaceZone>());
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 1f)
        {
            timer = 0;

            foreach (var z in zones)
            {
                if (z != null)
                    z.UpdateControl();
            }
        }
    }

    public MiniMapAirspaceZone GetZone(Vector3 pos)
    {
        foreach (var z in zones)
        {
            if (z == null) continue;

            if (z.IsInside(pos))
                return z;
        }

        return null;
    }

    public Vector3 GetClosestBase(Vector3 pos)
    {
        float best = float.MaxValue;
        Vector3 bestPos = pos;

        foreach (var z in zones)
        {
            if (z == null) continue;

            if (z.zoneType != AirspaceZoneType.Base)
                continue;

            float d = Vector3.Distance(pos, z.transform.position);

            if (d < best)
            {
                best = d;
                bestPos = z.transform.position;
            }
        }

        return bestPos;
    }
    public float GetBattleInfluence(Vector3 pos)
    {
        var zone = GetZone(pos);

        if (zone == null)
            return 0f;

        switch (zone.control)
        {
            case AirspaceControl.Friendly:
                return 1f;

            case AirspaceControl.Enemy:
                return 1f;

            case AirspaceControl.Contested:
                return 2f;

            default:
                return 0f;
        }
    }
}
