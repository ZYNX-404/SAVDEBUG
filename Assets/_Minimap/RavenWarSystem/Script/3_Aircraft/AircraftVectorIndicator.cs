using UnityEngine;
using SaccFlightAndVehicles;
using System.Collections.Generic;

public class AircraftVectorIndicator : MonoBehaviour
{
    [Header("Refs")]
    public SaccAirVehicle sav;                 // ← SaccAirVehicle を入れる
    public SkinnedMeshRenderer arrowSMR;
    public Renderer arrowRenderer;

    [Header("Forward Accel (Length & Hue)")]
    public float maxForwardAccel = 25f;
    public float deadZoneForward = 1.5f;
    public float snapSpeedForward = 8f;

    [Header("Pitch Input (Wedge & Brightness)")]
    public float pitchNormScale = 1f;          // まず1で試す（機体ごとに調整）
    public float deadZonePitch = 0.05f;
    public float snapSpeedPitch = 10f;
    public float maxAngleUp = 25f;
    public float maxAngleDown = 22.5f;
    public GameObject trailPrefab;
    [Header("Team")]
    public Renderer aircraftRenderer;
    public int team = 0;
    public Color teamColorFriendly = Color.cyan;
    public Color teamColorEnemy = Color.red;
    public Color teamColorNeutral = Color.gray;
    
    float timer;
    Vector3 lastVel;
    float forwardNorm; // -1..1
    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
    public LineRenderer line;
    List<Vector3> trail = new List<Vector3>();
    Vector3 lastTrailPos;
    float updateTimer;

    float trailDistance = 0.01f;
    int maxTrail = 80;
    float pitchNorm;
    
    void Start()
    {
        if (sav != null && sav.VehicleRigidbody != null)
        {
            lastVel = sav.VehicleRigidbody.velocity;
        }
        if (arrowRenderer != null)
        {
            mpb = new MaterialPropertyBlock();
            arrowRenderer.GetPropertyBlock(mpb);
        }


            if (arrowRenderer != null)
        {
            arrowRenderer.GetPropertyBlock(mpb);

            mpb.SetVector("_MainTex_ST",
                new Vector4(1f, 1f,
                    0.5f + forwardNorm * 0.5f,
                    0.5f + pitchNorm * 0.5f));

            mpb.SetColor("_Color", GetTeamColor());
            arrowRenderer.SetPropertyBlock(mpb);
        }

    }
    Color GetTeamColor()
    {
        switch(team)
        {
            case 0: return teamColorFriendly;
            case 1: return teamColorEnemy;
            default: return teamColorNeutral;
        }
     }
    void Update()
    {
        if (sav == null || sav.VehicleRigidbody == null) return;

        float dt = Time.deltaTime;

        float updateRate = 0.05f;

        updateTimer += dt;

        if(updateTimer < updateRate)
            return;

        updateTimer = 0f;

        // -------------------------
        // 1) forwardAccel（差分）
        // -------------------------
        var rb = sav.VehicleRigidbody;

        Vector3 v = rb.velocity;
        Vector3 accel = (v - lastVel) / dt;
        lastVel = v;

        float forwardAccel = Vector3.Dot(accel, rb.transform.forward);

        if (Mathf.Abs(forwardAccel) < deadZoneForward) forwardAccel = 0f;
        forwardAccel = Mathf.Clamp(forwardAccel, -maxForwardAccel, maxForwardAccel);

        float targetForwardNorm = forwardAccel / maxForwardAccel;
        forwardNorm = Mathf.MoveTowards(forwardNorm, targetForwardNorm, snapSpeedForward * dt);

        // -------------------------
        // 2) pitch（入力）
        // -------------------------

        float pitchInput = 0f;

        Vector3 angular = sav.VehicleRigidbody.angularVelocity;

        float pitchNorm = Mathf.Clamp(pitchInput / 2.5f, -1f, 1f);

        // -------------------------
        // 3) BlendShape（長さ）
        // -------------------------
        if (arrowSMR != null)
            arrowSMR.SetBlendShapeWeight(0, 50f + forwardNorm * 50f);

        // -------------------------
        // 4) 回転（上下：Z回転）
        // -------------------------

        float angle = pitchNorm >= 0f
            ? pitchNorm * maxAngleUp
            : pitchNorm * maxAngleDown;

        transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        // -------------------------
        // 5) 回転（左右：Y回転）
        // -------------------------

        object rawYaw = sav.GetProgramVariable("yaw");
        float yawInput = 0f;

        if (rawYaw is float fy)
            yawInput = fy;

        float yawNorm = Mathf.Clamp(yawInput / 2.5f, -1f, 1f);
        Vector3 ang = sav.VehicleRigidbody.angularVelocity;

        float pitch = Mathf.Clamp(ang.x * 20f, -30f, 30f);
        float yaw   = Mathf.Clamp(ang.y * 20f, -30f, 30f);
        float roll  = Mathf.Clamp(ang.z * 20f, -30f, 30f);

        transform.localRotation = Quaternion.Euler(pitch, yaw, roll);
        
        // -------------------------
        // 6) UV（色）
        // U=forward, V=pitch
        // -------------------------
        if (arrowRenderer != null)
        {
            arrowRenderer.GetPropertyBlock(mpb);

            mpb.SetVector("_MainTex_ST",
                new Vector4(1f, 1f,
                    0.5f + forwardNorm * 0.5f,
                    0.5f + pitchNorm * 0.5f));

            mpb.SetColor("_Color", GetTeamColor());

            arrowRenderer.SetPropertyBlock(mpb);
        }
        
        Vector3 p = transform.localPosition;

        if (Vector3.Distance(p, lastTrailPos) > trailDistance)
        {
            lastTrailPos = p;

            trail.Add(p);

            if (trail.Count > maxTrail)
                trail.RemoveAt(0);

            line.positionCount = trail.Count;
            for(int i = 0; i < trail.Count; i++)
            {
                line.SetPosition(i, trail[i]);
            }
        }
    }
}

