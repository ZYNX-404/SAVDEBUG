using UnityEngine;

public class EngagementRing : MonoBehaviour
{
    public void SetSize(float size)
    {
        transform.localScale = new Vector3(size, 0.01f, size);
    }
}