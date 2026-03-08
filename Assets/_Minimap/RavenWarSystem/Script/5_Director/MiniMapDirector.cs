using UnityEngine;
using System.Collections.Generic;

public class MiniMapDirector : MonoBehaviour
{
    public int maxAircraft = 20;

    bool HasAircraftList()
    {
        return MiniMapManager.Instance != null
            && MiniMapManager.Instance.aircraft != null;
    }    
    
    void Update()
    {
        if (!HasAircraftList())
            return;

        int count = MiniMapManager.Instance.aircraft.Count;

        if(count < maxAircraft)
        {
            SpawnAircraft();
        }
    }

    void SpawnAircraft()
    {
        Debug.Log("Spawn AI");
    }
}