using UnityEngine;
using SaccFlightAndVehicles;

public class RavenFlightAssist : MonoBehaviour
{
    public SaccAirVehicle sav;
    public Rigidbody rb;

    public float lowSpeedAssist = 1.2f;
    public float highSpeedAssist = 0.6f;
    public float speedForFullEffect = 200f;

    public float aoaBoostStart = 15f;
    public float aoaBoost = 1.3f;

    public float assistTorque = 5f;

    void Start()
    {
        if (sav == null)
            sav = GetComponent<SaccAirVehicle>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        float speed = rb.velocity.magnitude;

        // 速度依存補助
        float t = Mathf.Clamp01(speed / speedForFullEffect);
        float assist = Mathf.Lerp(lowSpeedAssist, highSpeedAssist, t);

        // AoA計算
        float aoa = Vector3.Angle(transform.forward, rb.velocity);

        if (aoa > aoaBoostStart)
        {
            assist *= aoaBoost;
        }

        // 機体の角速度を減衰させる
        Vector3 pitchDamp =
            -transform.right * rb.angularVelocity.x * assist;

        rb.AddTorque(pitchDamp * assistTorque, ForceMode.Acceleration);
    }
}