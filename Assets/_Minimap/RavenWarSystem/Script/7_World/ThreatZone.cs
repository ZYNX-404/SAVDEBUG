using UnityEngine;
using System.Collections.Generic;

public class MiniMapThreatZone : MonoBehaviour
{
    public Transform miniZone;
    public float radius = 2000f;

    public float worldSize = 100000f;
    public float mapSize = 1f;

    void Start()
    {
        float scale = mapSize / worldSize;

        float r = radius * scale;

        miniZone.localScale = new Vector3(r, 0.002f, r);
    }
}