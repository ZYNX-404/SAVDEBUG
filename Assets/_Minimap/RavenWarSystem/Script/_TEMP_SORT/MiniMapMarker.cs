using UnityEngine;
using System.Collections.Generic;

public class MiniMapMarker : MonoBehaviour
{
    [Header("References")]
    public Transform aircraft;          // 実機
    public Transform miniTransform;     // ミニ機体
    public Transform mapRoot;           // MiniMapRoot

    [Header("Scale")]
    public float worldSize = 100000f;
    public float miniMapSize = 1f;

    [Header("Height")]
    public float heightScale = 0.005f;

    [Header("Smoothing")]
    public float posSmooth = 12f;
    public float rotSmooth = 15f;

    [Header("Physics")]
    public Rigidbody rb;

    [Header("Altitude Color")]
    public Renderer arrowRenderer;
    public float minAltitude = 0f;
    public float maxAltitude = 5000f;

    [Header("Trail")]
    public LineRenderer trailLine;
    public int trailLength = 12;
    public float trailMinDistance = 0.01f;

    Queue<Vector3> trailPoints = new Queue<Vector3>();
    Vector3 lastTrailPos;

    public int attackers = 0;

    float logicalScale;
    bool scaleInitialized;

    public Vector3 position => transform.position;
    public int team;
    public bool isAce;

    public Vector3 WorldPosition { get; private set; }
    public Vector3 LocalPosition { get; private set; }

    public LineRenderer trail;
    List<Vector3> history = new List<Vector3>();
    public int maxPoints = 40;
    public float recordInterval = 0.2f;

    public LineRenderer velocityLine;
    public float vectorScale = 0.0005f;

    float timer;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        MiniMapManager.Instance?.Register(this);
    }

    void OnDisable()
    {
        MiniMapManager.Instance?.Unregister(this);
    }

    void TryInitScale()
    {
        if (scaleInitialized) return;
        if (worldSize <= 0f || miniMapSize <= 0f) return;

        logicalScale = miniMapSize / worldSize;
        scaleInitialized = true;
    }

    public void Tick()
    {
        if (aircraft == null || miniTransform == null || mapRoot == null)
            return;

        TryInitScale();

        WorldPosition = aircraft.position;

        float zoom = 1f;
        if (MiniMapManager.Instance != null)
            zoom = MiniMapManager.Instance.mapZoom;

        Vector3 targetPos = new Vector3(
            WorldPosition.x * logicalScale * zoom,
            aircraft.position.y * heightScale,
            WorldPosition.z * logicalScale * zoom
        );

        LocalPosition = targetPos;

        float dt = Time.deltaTime;
        float pt = 1f - Mathf.Exp(-posSmooth * dt);

        miniTransform.localPosition =
            Vector3.Lerp(miniTransform.localPosition, targetPos, pt);

        Quaternion targetRot =
            Quaternion.Inverse(mapRoot.rotation) * aircraft.rotation;

        float rt = 1f - Mathf.Exp(-rotSmooth * dt);

        miniTransform.localRotation =
            Quaternion.Slerp(miniTransform.localRotation, targetRot, rt);

        // altitude scale
        float altitudeFactor =
            Mathf.Lerp(
                0.7f,
                1.3f,
                Mathf.InverseLerp(minAltitude, maxAltitude, aircraft.position.y)
            );

        miniTransform.localScale = Vector3.one * altitudeFactor;

        // 青 → 赤
        float altitude01 = Mathf.InverseLerp(minAltitude, maxAltitude, aircraft.position.y);
        Color altitudeColor = Color.Lerp(Color.blue, Color.red, altitude01);

        // 少し緑を混ぜて中間色をなじませる
        altitudeColor = Color.Lerp(altitudeColor, Color.green, 0.3f);

        if (arrowRenderer != null)
        {
            arrowRenderer.material.color = isAce ? Color.yellow : altitudeColor;
        }

        timer += Time.deltaTime;

        if (timer > recordInterval)
        {
            timer = 0f;

            history.Add(miniTransform.localPosition);

            if (history.Count > maxPoints)
                history.RemoveAt(0);

            if (trail != null)
            {
                trail.positionCount = history.Count;

                for (int i = 0; i < history.Count; i++)
                    trail.SetPosition(i, history[i]);
            }
        }

        if (velocityLine != null && rb != null)
        {
            Vector3 v = rb.velocity * vectorScale;

            velocityLine.SetPosition(0, miniTransform.localPosition);
            velocityLine.SetPosition(1, miniTransform.localPosition + v);
        }

        UpdateTrail();
    }

    void UpdateTrail()
    {
        if (trailLine == null || miniTransform == null)
            return;

        Vector3 pos = miniTransform.position;

        if (trailPoints.Count == 0 ||
            Vector3.Distance(pos, lastTrailPos) > trailMinDistance)
        {
            trailPoints.Enqueue(pos);
            lastTrailPos = pos;

            while (trailPoints.Count > trailLength)
                trailPoints.Dequeue();
        }

        trailLine.positionCount = trailPoints.Count;
        trailLine.SetPositions(trailPoints.ToArray());
    }
}