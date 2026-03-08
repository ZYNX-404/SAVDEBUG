using UnityEngine;

public class MiniMapAutoZoomCamera : MonoBehaviour
{
    public float minZoom = 10f;
    public float maxZoom = 60f;

    public float zoomSpeed = 3f;

    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (MiniMapManager.Instance == null) return;

        float dist = GetBattleSize();

        float targetZoom = Mathf.Lerp(minZoom, maxZoom, dist / 5000f);

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * zoomSpeed
        );
    }

    float GetBattleSize()
    {
        var list = MiniMapManager.Instance.aircraft;

        float maxDist = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;

            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[j] == null) continue;

                float d = Vector3.Distance(
                    list[i].transform.position,
                    list[j].transform.position
                );

                if (d > maxDist)
                    maxDist = d;
            }
        }

        return maxDist;
    }
}