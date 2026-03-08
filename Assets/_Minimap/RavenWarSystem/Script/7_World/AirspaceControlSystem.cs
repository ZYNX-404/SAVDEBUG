using UnityEngine;
using System.Collections.Generic;
public class AirspaceCell
{
    public float blue;
    public float red;
}
public class AirspaceControlSystem : MonoBehaviour
{
    public int gridSize = 32;
    public float worldSize = 100000f;
    public float influenceRadius = 15000f;

    AirspaceCell[,] cells;
    public Team team;
    //MiniMapMarkers ac;
    float timer;
    
    void Start()
    {
        cells = new AirspaceCell[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        {
            cells[x,y] = new AirspaceCell();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 0.2f)
        {
            timer = 0;
            UpdateAirspace();
        }
    }

    void UpdateAirspace()
    {
        ClearGrid();
        CalculateInfluence();
    }

    void ClearGrid()
    {
        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        {
            cells[x,y].blue = 0;
            cells[x,y].red = 0;
        }
    }

    int WorldToCell(float coord)
    {
        float half = worldSize * 0.5f;
        float normalized = (coord + half) / worldSize;
        return Mathf.Clamp((int)(normalized * gridSize), 0, gridSize - 1);
    }

    Vector3 CellToWorld(int x, int y)
    {
        float cellSize = worldSize / gridSize;

        float wx = x * cellSize - worldSize * 0.5f + cellSize * 0.5f;
        float wz = y * cellSize - worldSize * 0.5f + cellSize * 0.5f;

        return new Vector3(wx, 0, wz);
    }
    void CalculateInfluence()
    {
        var aircraft = MiniMapDataBus.Instance.GetAircraft();

        foreach (var ac in aircraft)
        {
            ApplyInfluence(ac);
        }
    }
    
    public float GetControlAtWorld(Vector3 pos)
    {
        int x = WorldToCell(pos.x);
        int y = WorldToCell(pos.z);

        return cells[x,y].blue - cells[x,y].red;
    }

    void ApplyInfluence(MiniMapMarker ac)
    {
        Vector3 pos = ac.transform.position;

        int cx = WorldToCell(pos.x);
        int cy = WorldToCell(pos.z);

        float cellSize = worldSize / gridSize;
        int radius = Mathf.CeilToInt(influenceRadius / cellSize);

        float radiusSq = influenceRadius * influenceRadius;

        for (int x = cx - radius; x <= cx + radius; x++)
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            if (x < 0 || x >= gridSize || y < 0 || y >= gridSize)
                continue;

            Vector3 cellPos = CellToWorld(x, y);

            Vector3 diff = pos - cellPos;
            float distSq = diff.sqrMagnitude;

            if (distSq > radiusSq)
                continue;

            float influence = Mathf.Exp(-(distSq) / radiusSq);

            if ((Team)ac.team == Team.Blue)
                cells[x,y].blue += influence;
            else
                cells[x,y].red += influence;
        }
    }


}
