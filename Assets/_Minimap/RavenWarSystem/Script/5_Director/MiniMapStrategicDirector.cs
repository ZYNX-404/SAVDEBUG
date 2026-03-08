using UnityEngine;

public class MiniMapStrategicDirector : MonoBehaviour
{
    public MiniMapCombatHeatmap heatmap;
    // public IslandTacticalSystem islandManager;

    public Vector3 strategicFocus;

    public float strategicScore;
//    public Vector3 GetHottestPoint()
//    {
//        return transform.position;
//    }

    public void EvaluateStrategic()
    {
        //Vector3 pos = heatmap.GetHottestPoint();

        //float heat = heatmap.GetHeat(pos);

        //var island = islandManager.GetIsland(pos);

        //float islandValue =
        //    island != null ? island.tacticalValue : 0f;

        //float score =
           // heat * 3f; //+
            //islandValue * 4f;
        float score = 0f;
        if (score > strategicScore)
        {
            strategicScore = score;
            //strategicFocus = pos;
        }
    }
}