using UnityEngine;

public class MiniMapZoomSlider : MonoBehaviour
{
    public Transform knob;

    public float minZoom = 0.5f;
    public float maxZoom = 5f;

    public float sliderLength = 0.3f;

    void Update()
    {
        float t = Mathf.InverseLerp(
            -sliderLength,
             sliderLength,
             knob.localPosition.x
        );

        float zoom = Mathf.Lerp(minZoom, maxZoom, t);

        if (MiniMapManager.Instance != null)
            MiniMapManager.Instance.mapZoom = zoom;
    }
}