using UnityEngine;
using System.Collections.Generic;
public enum AirspaceZoneType
{
    Island,
    Frontline,
    Patrol,
    Base
}

public class MiniMapAirspaceZone : MonoBehaviour
{
    public float radius = 0.3f;

    public AirspaceZoneType zoneType;

    public AirspaceControl control;
    
    public MiniMapCombatHeatmap heatmap;
    public List<MiniMapAirspaceZone> zones = new List<MiniMapAirspaceZone>();

    public void UpdateControl()
    {
        int friendly = 0;
        int enemy = 0;

        foreach (var a in MiniMapManager.Instance.aircraft)
        {
            if (a == null) continue;

            if (!IsInside(a.WorldPosition))
                continue;

            if (a.team == 0)
                friendly++;
            else
                enemy++;
        }

        if (friendly > enemy)
            control = AirspaceControl.Friendly;
        else if (enemy > friendly)
            control = AirspaceControl.Enemy;
        else if (friendly == 0 && enemy == 0)
            control = AirspaceControl.Neutral;
        else
            control = AirspaceControl.Contested;
    }
    public bool IsInside(Vector3 pos)
    {
        return Vector3.Distance(transform.position, pos) < radius;
    }
    public float GetInfluence()
    {
        switch(control)
        {
            case AirspaceControl.Friendly:
                return 1f;

            case AirspaceControl.Enemy:
                return -1f;

            case AirspaceControl.Contested:
                return 0f;

            default:
                return 0f;
        }
    }
    public float GetBattleInfluence(Vector3 pos)
    {
        float heat = heatmap.GetHeat(pos);

        float air = 0f;

        foreach(var z in zones)
        {
            if(z.IsInside(pos))
            {
                air = z.GetInfluence();
                break;
            }
        }

        return heat + air * 0.5f;
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

            if (z.zoneType != AirspaceZoneType.Base) continue;

            float d = Vector3.Distance(pos, z.transform.position);

            if (d < best)
            {
                best = d;
                bestPos = z.transform.position;
            }
        }

        return bestPos;
    }
}