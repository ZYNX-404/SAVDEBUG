using UnityEngine;

public class MiniMapDogfightIndicator : MonoBehaviour
{
    public float rotateSpeed = 120f;
    public float pulseSpeed = 2f;

    Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.25f;

        transform.localScale = baseScale * pulse;
    }
}