using UnityEngine;

public class RAI_Viewer_LeadIndicator : MonoBehaviour
{
    public Rigidbody target;
    public Transform marker;

    public float leadTime = 1.5f;
    public float scale = 0.02f;

    void Update()
    {
        if (target == null) return;

        Vector3 future =
            target.position +
            target.velocity * leadTime * scale;

        marker.position = future;
    }
}