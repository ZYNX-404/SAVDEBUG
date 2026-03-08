using UnityEngine;

public class MiniMapIsland : MonoBehaviour
{
    public float radius = 0.05f;

    public int tacticalValue = 1;

    public bool IsInside(Vector3 pos)
    {
        return Vector3.Distance(
            transform.position,
            pos
        ) < radius;
    }
}