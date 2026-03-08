using UnityEngine;

public class MiniMapAdaptiveCamera : MonoBehaviour
{
    public MiniMapClusterCamera clusterCamera;
    public MiniMapDogfightCamera dogfightCamera;

    MiniMapCameraMode currentMode;
    public void SetTarget(Transform t)
    {
        transform.LookAt(t);
    }
    public void SetMode(MiniMapCameraMode mode)
    {
        if (currentMode == mode) return;

        currentMode = mode;

        clusterCamera.enabled =
            mode == MiniMapCameraMode.Cluster;

        dogfightCamera.enabled =
            mode == MiniMapCameraMode.Dogfight;
    }
}