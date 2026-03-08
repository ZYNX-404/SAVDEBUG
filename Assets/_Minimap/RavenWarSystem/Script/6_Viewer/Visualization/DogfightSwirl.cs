using UnityEngine;

public class DogfightSwirl : MonoBehaviour
{
    public float rotateSpeed = 60f;
    public float scaleSpeed = 5f;

    float targetScale = 1f;
    Renderer r;

    void Awake()
    {
        r = GetComponentInChildren<Renderer>();
    }
    void Update()
    {
        if (MiniMapViewerSystem.Instance == null) return;

        float d =
            MiniMapViewerSystem.Instance.DistanceToViewer(
                transform.position
            );

        // 遠い場合は回転もスケール更新もしない
        if (d > 4f)
            return;

        // 回転
        transform.Rotate(
            Vector3.up * rotateSpeed * Time.deltaTime
        );

        // スケール補間
        float current = transform.localScale.x;

        float s = Mathf.Lerp(
            current,
            targetScale,
            Time.deltaTime * scaleSpeed
        );

        transform.localScale = new Vector3(s, s, s);
    }

    public void SetIntensity(float intensity)
    {
        targetScale = Mathf.Clamp(intensity, 0.3f, 2.0f);
    }
}