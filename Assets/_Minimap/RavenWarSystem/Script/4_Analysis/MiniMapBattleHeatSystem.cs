using UnityEngine;

public class MiniMapBattleHeatSystem : MonoBehaviour
{
    public MiniMapCombatHeatmap heatmap;

    [Header("Heat Values")]
    public float missileHeat = 2f;
    public float dogfightHeat = 6f;
    public float interceptHeat = 4f;
    public float killHeat = 10f;

    void Update()
    {
        if (heatmap == null) return;

        EvaluateDogfights();
        EvaluateMissiles();
    }

    void EvaluateDogfights()
    {
        var dogfight = FindObjectOfType<MiniMapDogfightDetection>();
        if (dogfight == null) return;

        foreach (var p in dogfight.pairs)
        {
            Vector3 center =
                (p.a.transform.position +
                 p.b.transform.position) * 0.5f;

            heatmap.AddHeat(center, dogfightHeat);
        }
    }

    void EvaluateMissiles()
    {
        var missiles = MiniMapDataBus.Instance.GetMissiles();

        foreach (var m in missiles)
        {
            if (m == null) continue;

            heatmap.AddHeat(m.position, missileHeat);
        }
    }

    public void RegisterKill(Vector3 pos)
    {
        if (heatmap == null) return;

        heatmap.AddHeat(pos, killHeat);
    }
}