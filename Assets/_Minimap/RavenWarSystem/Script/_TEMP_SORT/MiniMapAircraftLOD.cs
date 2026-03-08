using UnityEngine;

public class MiniMapAircraftLOD : MonoBehaviour
{
    public GameObject dot;
    public GameObject arrow;
    public GameObject model;

    MiniMapAircraftLODLevel currentLevel;

    public void SetLOD(MiniMapAircraftLODLevel level)
    {
        if (currentLevel == level) return;

        currentLevel = level;

        if (dot != null) dot.SetActive(level == MiniMapAircraftLODLevel.Dot);
        if (arrow != null) arrow.SetActive(level == MiniMapAircraftLODLevel.Arrow);
        if (model != null) model.SetActive(level == MiniMapAircraftLODLevel.Model);
    }
}