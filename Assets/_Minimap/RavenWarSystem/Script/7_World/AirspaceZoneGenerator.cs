using UnityEngine;
using UnityEditor;

public class AirspaceZoneGenerator
{
    [MenuItem("Tools/MiniMap/Generate Airspace Zones")]
    static void GenerateZones()
    {
        var islands = GameObject.FindGameObjectsWithTag("Island");

        foreach (var island in islands)
        {
            GameObject zoneObj = new GameObject("AirspaceZone");

            zoneObj.transform.position =
                island.transform.position + Vector3.up * 0.05f;

            var zone = zoneObj.AddComponent<MiniMapAirspaceZone>();

            zone.radius = 0.4f;
            zone.control = AirspaceControl.Neutral;

            Debug.Log("Zone created for " + island.name);
        }
    }
}