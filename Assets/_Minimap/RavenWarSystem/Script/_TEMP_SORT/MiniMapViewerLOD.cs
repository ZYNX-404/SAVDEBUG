using UnityEngine;

public class MiniMapViewerLOD : MonoBehaviour
{
    public GameObject dot;
    public GameObject arrow;
    public GameObject model;

    public float dotDistance = 3f;
    public float arrowDistance = 0.8f;

    void Update()
    {
        if (MiniMapViewerSystem.Instance == null)
            return;

        float d =
            MiniMapViewerSystem.Instance.DistanceToViewer(
                transform.position
            );

        if (d > dotDistance)
        {
            dot.SetActive(true);
            arrow.SetActive(false);
            model.SetActive(false);
        }
        else if (d > arrowDistance)
        {
            dot.SetActive(false);
            arrow.SetActive(true);
            model.SetActive(false);
        }
        else
        {
            dot.SetActive(false);
            arrow.SetActive(false);
            model.SetActive(true);
        }
    }
}