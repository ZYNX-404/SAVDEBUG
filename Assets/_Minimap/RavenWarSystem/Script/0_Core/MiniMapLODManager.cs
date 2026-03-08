using UnityEngine;

public class MiniMapLODManager : MonoBehaviour
{
    public static MiniMapLODManager Instance;

    public float fullDistance = 0.5f;
    public float midDistance = 2f;
    public float lowDistance = 4f;

    void Awake()
    {
        Instance = this;
    }

    public MiniMapLODLevel GetLOD(Vector3 pos)
    {
        float d = MiniMapViewerSystem.Instance.DistanceToViewer(pos);

        if (d < fullDistance)
            return MiniMapLODLevel.Full;

        if (d < midDistance)
            return MiniMapLODLevel.Mid;

        if (d < lowDistance)
            return MiniMapLODLevel.Low;

        return MiniMapLODLevel.Dot;
    }
}