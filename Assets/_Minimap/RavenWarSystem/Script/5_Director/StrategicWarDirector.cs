using UnityEngine;
using System.Collections.Generic;

public class StrategicWarDirector : MonoBehaviour
{
    public MiniMapCombatClusterSystem clusterSystem;
    public DirectorAI director;

    public Transform baseA;
    public Transform baseB;

    public float thinkInterval = 4f;

    public int strikeThreshold = 5;
    public int defendThreshold = 4;
    
    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < thinkInterval)
            return;

        timer = 0f;

        EvaluateWar();
    }

    void EvaluateWar()
    {
        if (clusterSystem == null)
            return;

        if (clusterSystem.clusters.Count == 0)
            return;

        foreach (var c in clusterSystem.clusters)
        {
            if (c == null)
                continue;

            int teamA = CountTeam(c, 0);
            int teamB = CountTeam(c, 1);

            if (teamA + teamB < 2)
                continue;

            // --- 攻撃チャンス ---
            if (teamA > teamB + strikeThreshold)
            {
                LaunchStrike(c.center, 0);
                continue;
            }

            if (teamB > teamA + strikeThreshold)
            {
                LaunchStrike(c.center, 1);
                continue;
            }

            // --- 防衛 ---
            if (NearBase(c.center, baseA.position) && teamB > defendThreshold)
            {
                ScrambleDefense(0);
                continue;
            }

            if (NearBase(c.center, baseB.position) && teamA > defendThreshold)
            {
                ScrambleDefense(1);
                continue;
            }

            // --- 均衡戦闘 ---
            if (Mathf.Abs(teamA - teamB) <= 1)
            {
                ReinforceFront(c.center);
            }
        }
    }

    int CountTeam(CombatCluster cluster, int team)
    {
        int count = 0;

        foreach (var m in cluster.aircraft)
        {
            if (m == null) continue;

            if (m.team == team)
                count++;
        }

        return count;
    }

    bool NearBase(Vector3 pos, Vector3 basePos)
    {
        float d = Vector3.Distance(pos, basePos);
        return d < 10000f;
    }

    void LaunchStrike(Vector3 target, int team)
    {
        Debug.Log("STRIKE PACKAGE LAUNCHED");

        if (director == null)
            return;

        Vector3 spawn =
            team == 0 ? baseA.position : baseB.position;

        director.SpawnPair(spawn, team);
        director.SpawnPair(spawn, team);
    }

    void ScrambleDefense(int team)
    {
        Debug.Log("SCRAMBLE DEFENSE");

        if (director == null)
            return;

        Vector3 spawn =
            team == 0 ? baseA.position : baseB.position;

        director.SpawnPair(spawn, team);
    }

    void ReinforceFront(Vector3 pos)
    {
        Debug.Log("REINFORCE FRONT");

        if (director == null)
            return;

        int friendly = director.CountTeam(0);
        int enemy = director.CountTeam(1);

        if (friendly < enemy)
            director.SpawnPair(baseA.position, 0);

        if (enemy < friendly)
            director.SpawnPair(baseB.position, 1);
    }
}