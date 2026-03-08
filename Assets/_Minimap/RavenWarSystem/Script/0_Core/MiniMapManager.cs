using UnityEngine;
using System.Collections.Generic;


public class MiniMapManager : MonoBehaviour
{
    public static MiniMapManager Instance;

    [Header("Aircraft")]
    public List<MiniMapMarker> aircraft = new List<MiniMapMarker>();

    [Header("Cluster Settings")]
    public float clusterDistance = 2000f;

    [Header("Dogfight Settings")]
    public float dogfightDistance = 800f;
    public float minSpeed = 40f;

    [Header("Indicators")]
    public Transform engagementRing;
    public Transform dogfightIndicator;

    [Header("Heatmap")]
    public MiniMapCombatHeatmap heatmap;
    public MiniMapKillCam killCam;
    
    [Header("Map Zoom")]
   
    public float mapZoom = 1f;
    public float baseMapSize = 4f;
    
    public float zoom = 1f;
    public float minZoom = 0.5f;
    public float maxZoom = 5f;


    Vector3 combatCenter;
    Vector3 lastCombatCenter;
    Vector3 combatCenterSmooth;
    Vector3 combatFlow;
    public bool timeFrozen;
    public List<MiniMapMarker> markers = new List<MiniMapMarker>();

    public float worldSize = 100000f;
    public float miniMapSize = 1f;
    public float heightScale = 0.005f;

    float logicalScale;

    void Start()
    {
        logicalScale = miniMapSize / worldSize;
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (timeFrozen)
            return;

        for (int i = aircraft.Count - 1; i >= 0; i--)
        {
            var m = aircraft[i];

            if (m == null)
            {
                aircraft.RemoveAt(i);
                continue;
            }

            m.Tick();
        }

        UpdateCombatCluster();
        DetectDogfight();

        if (Input.GetKey(KeyCode.Equals))
            mapZoom += Time.deltaTime;

        if (Input.GetKey(KeyCode.Minus))
            mapZoom -= Time.deltaTime;

        mapZoom = Mathf.Clamp(mapZoom, minZoom, maxZoom);   
    
    }

    public Vector3 WorldToMiniMap(Vector3 worldPos)
    {
        return new Vector3(
            worldPos.x * logicalScale * mapZoom,
            worldPos.y * heightScale,
            worldPos.z * logicalScale * mapZoom
        );
    }

    // -------------------------
    // Aircraft Register
    // -------------------------

    public void Register(MiniMapMarker marker)
    {
        if (!aircraft.Contains(marker))
            aircraft.Add(marker);
    }

    public void Unregister(MiniMapMarker marker)
    {
        aircraft.Remove(marker);
    }

    // -------------------------
    // Combat Cluster
    // -------------------------

    void UpdateCombatCluster()
    {
        if (aircraft.Count == 0) return;

        Vector3 bestCenter = Vector3.zero;
        int bestCount = 0;

        for (int i = 0; i < aircraft.Count; i++)
        {
            var a = aircraft[i];
            if (a == null) continue;

            Vector3 center = Vector3.zero;
            int count = 0;

            for (int j = 0; j < aircraft.Count; j++)
            {
                var b = aircraft[j];
                if (b == null) continue;

                float dist = Vector3.Distance(a.transform.position, b.transform.position);

                if (dist < clusterDistance)
                {
                    center += b.transform.position;
                    count++;
                }
            }

            if (count > bestCount)
            {
                bestCount = count;
                bestCenter = center / count;
            }
        }

        combatCenterSmooth = Vector3.Lerp(
            combatCenterSmooth,
            bestCenter,
            Time.deltaTime * 2f);

        combatCenter = combatCenterSmooth;
        combatFlow = combatCenter - lastCombatCenter;
        lastCombatCenter = combatCenter;

        if (engagementRing != null)
        {
            engagementRing.position = combatCenter;
        }
    }
    public Vector3 GetCombatFlow()
    {
        return combatFlow;
    }
    // -------------------------
    // Dogfight Detection
    // -------------------------

    void DetectDogfight()
    {
        if (dogfightIndicator == null) return;

        bool found = false;

        for (int i = 0; i < aircraft.Count; i++)
        {
            var a = aircraft[i];
            if (a == null || a.rb == null) continue;

            for (int j = i + 1; j < aircraft.Count; j++)
            {
                var b = aircraft[j];
                if (b == null || b.rb == null) continue;

                float dist = Vector3.Distance(a.transform.position, b.transform.position);
                if (dist > dogfightDistance) continue;

                float speedA = a.rb.velocity.magnitude;
                float speedB = b.rb.velocity.magnitude;

                if (speedA < minSpeed || speedB < minSpeed) continue;

                Vector3 center = (a.transform.position + b.transform.position) * 0.5f;

                Vector3 miniPos = WorldToMiniMap(center);
                dogfightIndicator.localPosition = miniPos;
                dogfightIndicator.gameObject.SetActive(true);

                if (heatmap != null)
                    heatmap.AddHeat(center, 1f);

                found = true;
                break;
            }

            if (found) break;
        }

        if (!found)
        {
            dogfightIndicator.gameObject.SetActive(false);
        }
    }

    public Vector3 GetCombatCenter()
    {
        return combatCenter;
    }

}