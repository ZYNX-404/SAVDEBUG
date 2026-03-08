using UnityEngine;
using System.Collections.Generic;

public class MiniMapBattleFlowSystem : MonoBehaviour
{
    public Vector3 frontline;

    public float frontlineStrength;

    Vector3 lastCenter;

    public Vector3 flow;

    public void EvaluateFlow()
    {
        var aircraft = MiniMapDataBus.Instance.GetAircraft();

        if (aircraft == null || aircraft.Count == 0)
            return;

        Vector3 center = Vector3.zero;

        int count = 0;

        foreach (var a in aircraft)
        {
            center += a.transform.position;
            count++;
        }

        if (count == 0) return;

        center /= count;

        // 戦場の移動方向
        flow = center - lastCenter;

        // 保存
        lastCenter = center;

        frontline = center;

        frontlineStrength = count;
    }
}