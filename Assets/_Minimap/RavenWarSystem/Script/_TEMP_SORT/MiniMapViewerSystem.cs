using UnityEngine;

public class MiniMapViewerSystem : MonoBehaviour
{
    public static MiniMapViewerSystem Instance;

    public Transform viewer;

    void Awake()
    {
        Instance = this;
    }

    public float DistanceToViewer(Vector3 pos)
    {
        if (viewer == null)
            return 999f;

        return Vector3.Distance(viewer.position, pos);
    }
}