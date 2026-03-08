using UnityEngine;
using System.Collections.Generic;

public class InterceptPrediction : MonoBehaviour
{
    public GameObject interceptMarker;

    public float predictTime = 5f;
    public int steps = 20;

    List<GameObject> markers = new List<GameObject>();

    void Update()
    {
        var missiles = MiniMapDataBus.Instance.GetMissiles();

        EnsureMarkers(missiles.Count);

        for (int i = 0; i < missiles.Count; i++)
        {
            Predict(missiles[i], markers[i]);
        }
    }

    void EnsureMarkers(int count)
    {
        while (markers.Count < count)
        {
            var m = Instantiate(interceptMarker, transform);
            markers.Add(m);
        }
    }

    void Predict(Rigidbody missile, GameObject marker)
    {
        if (missile == null)
        {
            marker.SetActive(false);
            return;
        }

        var mm = missile.GetComponent<MiniMapMissileMarker>();

        if (mm == null || mm.target == null)
        {
            marker.SetActive(false);
            return;
        }

        Vector3 mp = missile.position;
        Vector3 mv = missile.velocity;

        Vector3 tp = mm.target.position;
        Vector3 tv = mm.target.velocity;

        float bestDist = float.MaxValue;
        Vector3 bestPos = Vector3.zero;

        for (int i = 0; i < steps; i++)
        {
            float t = predictTime * i / steps;

            Vector3 mFuture = mp + mv * t;
            Vector3 tFuture = tp + tv * t;

            float d = Vector3.Distance(mFuture, tFuture);

            if (d < bestDist)
            {
                bestDist = d;
                bestPos = mFuture;
            }
        }

        if (bestDist < 0.05f)
        {
            marker.transform.position = bestPos;
            marker.SetActive(true);
        }
        else
        {
            marker.SetActive(false);
        }
    }
}