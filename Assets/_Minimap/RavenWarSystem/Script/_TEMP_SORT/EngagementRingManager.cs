using UnityEngine;
using System.Collections.Generic;

public class EngagementRingManager : MonoBehaviour
{
    public GameObject ringPrefab;

    List<EngagementRing> rings = new List<EngagementRing>();

    void Update()
    {
        var pairs = GetComponent<MiniMapDogfightDetection>().pairs;

        while (rings.Count < pairs.Count)
        {
            var r = Instantiate(ringPrefab, transform)
                .GetComponent<EngagementRing>();

            rings.Add(r);
        }

        for (int i = 0; i < rings.Count; i++)
        {
            if (i >= pairs.Count)
            {
                rings[i].gameObject.SetActive(false);
                continue;
            }

            rings[i].gameObject.SetActive(true);

            var p = pairs[i];

            Vector3 center =
                (p.a.transform.position + p.b.transform.position) * 0.5f;

            rings[i].transform.position = center;

            rings[i].SetSize(Mathf.Clamp(p.distance * 2f, 0.1f, 0.5f));
        }
    }
}