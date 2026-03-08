using UnityEngine;

public class MiniMapAircraftMarker : MonoBehaviour
{
    public Rigidbody aircraftRb;

    public Transform markerRoot;
    public Transform miniAircraft;

    MiniMapIndicator[] indicators;
    MiniMapMarker marker;

    void Awake()
    {
        marker = GetComponent<MiniMapMarker>();
        indicators = GetComponentsInChildren<MiniMapIndicator>();

        if (aircraftRb == null)
            aircraftRb = GetComponent<Rigidbody>();

        if (marker != null && marker.rb == null)
            marker.rb = aircraftRb;
    }

    void Start()
    {
        if (MiniMapDataBus.Instance != null && marker != null)
            MiniMapDataBus.Instance.RegisterAircraft(marker);
    }

    void OnDestroy()
    {
        if (MiniMapDataBus.Instance != null && marker != null)
            MiniMapDataBus.Instance.RemoveAircraft(marker);
    }

    public void UpdateMarker(Vector3 pos, Quaternion rot)
    {
        markerRoot.position = pos;
        markerRoot.rotation = rot;
    }

    public void SetIndicatorLevel(MiniMapIndicatorLevel level)
    {
        foreach (var i in indicators)
            i.SetLevel(level);
    }
}
