using UnityEngine;
using System.Collections.Generic;

public class MiniMapEngagementRing : MonoBehaviour
{
    public Transform ring;
    public float dogfightDistance = 0.08f;

    float eventTimer;

    void Update()
    {
        var aircraft = MiniMapDataBus.Instance.GetAircraft();

        if (aircraft == null || aircraft.Count < 2)
        {
            ring.gameObject.SetActive(false);
            return;
        }

        Vector3 center = Vector3.zero;
        int count = 0;

        for (int i = 0; i < aircraft.Count; i++)
        {
            for (int j = i + 1; j < aircraft.Count; j++)
            {
                float d = Vector3.Distance(
                    aircraft[i].transform.position,
                    aircraft[j].transform.position
                );

                if (d < dogfightDistance)
                {
                    center += aircraft[i].transform.position;
                    center += aircraft[j].transform.position;
                    count += 2;
                }
            }
        }

        if (count > 0)
        {
            center /= count;

            ring.position = center;
            ring.gameObject.SetActive(true);
            ring.Rotate(Vector3.up * 60f * Time.deltaTime);
            eventTimer += Time.deltaTime;

            if (eventTimer > 1f)
            {
                MiniMapDataBus.Instance.AddEvent(center, MiniMapEventType.Dogfight);
                eventTimer = 0f;
            }
        }
        else
        {
            ring.gameObject.SetActive(false);
        }
    }
}