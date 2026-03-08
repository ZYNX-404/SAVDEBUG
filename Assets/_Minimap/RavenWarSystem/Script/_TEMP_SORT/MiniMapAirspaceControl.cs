using UnityEngine;
using System.Collections.Generic;

public class MiniMapAirspaceControl : MonoBehaviour
{
    public float cellSize = 5f;
    public int gridSize = 20;

    public float influenceRadius = 6f;

    public float[,] control;

    void Awake()
    {
        control = new float[gridSize, gridSize];
    }

    void Update()
    {
        EvaluateControl();
    }
    public float GetControl(Vector3 pos)
    {
        return 0f;
    }
    void EvaluateControl()
    {
        var aircraft = MiniMapDataBus.Instance.GetAircraft();

        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        {
            control[x, y] = 0f;

            Vector3 cellPos =
                new Vector3(
                    (x - gridSize*0.5f) * cellSize,
                    0,
                    (y - gridSize*0.5f) * cellSize
                );

            foreach (var a in aircraft)
            {
                float d =
                    Vector3.Distance(
                        a.transform.position,
                        cellPos);

                if (d > influenceRadius)
                    continue;

                float influence =
                    1f - d / influenceRadius;

                control[x,y] += influence;
            }
        }
    }
}