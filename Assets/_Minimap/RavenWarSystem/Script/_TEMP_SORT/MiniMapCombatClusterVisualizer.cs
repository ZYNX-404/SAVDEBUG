using UnityEngine;
using System.Collections.Generic;

public class MiniMapCombatClusterVisualizer : MonoBehaviour
{
    public MiniMapCombatClusterSystem clusterSystem;

    public MiniMapDogfightSwirlPool swirlPool;

    public Transform ringPrefab;
    List<Transform> rings = new List<Transform>();

    void Update()
    {
        if (clusterSystem == null)
            return;

        var clusters = clusterSystem.source.clusters;

        swirlPool.ResetAll();

        EnsureRingCount(clusters.Count);

        for (int i = 0; i < clusters.Count; i++)
        {
            var c = clusters[i];

            // Swirl
            var swirl = swirlPool.Get();

            if (swirl != null)
            {
                swirl.transform.position = c.center;

                swirl.SetIntensity(c.intensity);
            }

            // Ring
            var ring = rings[i];

            ring.position = c.center;

            float scale = Mathf.Max(c.radius * 2f, 0.05f);

            ring.localScale = new Vector3(scale, 1f, scale);
        }

        for (int i = clusters.Count; i < rings.Count; i++)
        {
            rings[i].gameObject.SetActive(false);
        }
    }

    void EnsureRingCount(int count)
    {
        while (rings.Count < count)
        {
            var r = Instantiate(ringPrefab, transform);
            rings.Add(r);
        }

        for (int i = 0; i < rings.Count; i++)
        {
            rings[i].gameObject.SetActive(i < count);
        }
    }
}