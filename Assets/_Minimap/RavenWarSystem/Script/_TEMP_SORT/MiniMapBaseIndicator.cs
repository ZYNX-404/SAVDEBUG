using UnityEngine;
using SaccFlightAndVehicles;

public class MiniMapBaseIndicator : MonoBehaviour
{
    [Header("Vehicle")]
    public SaccAirVehicle sav;

    protected Rigidbody rb;

    protected virtual void Start()
    {
        if (sav != null)
            rb = sav.VehicleRigidbody;
    }

    protected virtual void Update()
    {
        if (sav == null) return;
        if (rb == null) return;

        OnIndicatorUpdate();
    }

    protected virtual void OnIndicatorUpdate()
    {
        // 継承先で処理を書く
    }
}