using UnityEngine;

public class MiniMapCombatNarrative : MonoBehaviour
{
    public enum BattlePhase
    {
        Calm,
        Dogfight,
        Missile,
        Push,
        Regroup
    }

    public BattlePhase phase;

    public MiniMapDogfightDetection dogfight;
    public MiniMapCombatClusterSystem cluster;
    public MiniMapCombatHeatmap heatmap;

    void Update()
    {
        EvaluatePhase();
    }

    void EvaluatePhase()
    {
        if (dogfight != null && dogfight.pairs.Count > 2)
        {
            phase = BattlePhase.Dogfight;
            return;
        }

        if (cluster != null && cluster.clusters.Count > 0)
        {
            phase = BattlePhase.Push;
            return;
        }

        phase = BattlePhase.Calm;
    }
}