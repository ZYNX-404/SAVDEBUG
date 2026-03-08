using UnityEngine;

public class MiniMapDebugOverlay : MonoBehaviour
{
    public MiniMapCombatClusterSystem clusterSystem;
    public MiniMapCombatHeatmap heatmap;
    public MiniMapAIDirector director;

    public bool debugClusters = true;
    public bool debugHeatmap = true;

    void OnDrawGizmos()
    {
        if (debugClusters)
            DrawClusters();

        if (debugHeatmap)
            DrawHeatmap();

        DrawDirectorFocus();
    }

    void DrawClusters()
    {
        if (clusterSystem == null) return;

        Gizmos.color = Color.red;

        foreach (var cluster in clusterSystem.clusters)
        {
            Gizmos.DrawWireSphere(cluster.center, 0.2f);
            Gizmos.DrawSphere(cluster.center, 0.05f);
        }
    }

    void DrawDirectorFocus()
    {
        if (director == null) return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(
            director.transform.position,
            0.15f
        );
    }

    void DrawHeatmap()
    {
        if (heatmap == null) return;

        float range = 2f;
        float step = 0.2f;

        for (float x = -range; x < range; x += step)
        {
            for (float z = -range; z < range; z += step)
            {
                Vector3 p = new Vector3(x, 0, z);

                float h = heatmap.GetHeat(p);

                if (h > 0.2f)
                {
                    Gizmos.color = Color.Lerp(Color.blue, Color.red, h);
                    Gizmos.DrawCube(p, Vector3.one * 0.05f);
                }
            }
        }
    }
}