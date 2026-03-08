using UnityEngine;
using System.Collections.Generic;

public class MiniMapCombatHeatmap : MonoBehaviour
{
    public GameObject heatPointPrefab;

    public float lifeTime = 10f;
    public float scale = 0.02f;

    List<HeatPoint> points = new List<HeatPoint>();


    class HeatPoint
    {
        public Transform t;
        public Renderer r;
        public float time;
        public float intensity;
    }


    void Update()
    {
        UpdatePoints();
    }


    // 外部システムからHeat追加
    public void AddHeat(Vector3 pos, float intensity)
    {
        SpawnPoint(pos, intensity);
    }


    void SpawnPoint(Vector3 pos, float intensity)
    {
        var go = Instantiate(heatPointPrefab, transform);

        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;

        HeatPoint p = new HeatPoint();

        p.t = go.transform;
        p.r = go.GetComponent<Renderer>();
        p.time = Time.time;
        p.intensity = intensity;

        points.Add(p);
    }


    void UpdatePoints()
    {
        float now = Time.time;

        for (int i = points.Count - 1; i >= 0; i--)
        {
            var p = points[i];

            float age = now - p.time;

            if (age > lifeTime)
            {
                Destroy(p.t.gameObject);
                points.RemoveAt(i);
                continue;
            }

            float fade = 1f - (age / lifeTime);

            float heat = p.intensity * fade;

            if (p.r != null)
            {
                Color c = Color.Lerp(Color.yellow, Color.red, heat);
                c.a = fade;

                p.r.material.color = c;
            }
        }
    }


    // DirectorやAIが参照できるHeat取得
    public float GetHeat(Vector3 position)
    {
        float heat = 0f;

        foreach (var p in points)
        {
            float d = Vector3.Distance(position, p.t.position);

            float contribution =
                p.intensity / (1f + d * 10f);

            heat += contribution;
        }

        return heat;
    }
}