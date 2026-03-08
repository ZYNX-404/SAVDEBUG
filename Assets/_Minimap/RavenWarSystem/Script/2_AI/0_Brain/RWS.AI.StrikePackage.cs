using UnityEngine;

public class StrikePackageDirector : MonoBehaviour
{
    public DirectorAI director;
    public DynamicFrontlineSystem frontline;

    public float strikeCooldown = 25f;

    float nextStrike;

    void Update()
    {
        if (Time.time < nextStrike)
            return;

        nextStrike = Time.time + strikeCooldown;

        LaunchStrike();
    }

    void LaunchStrike()
    {
        if (director == null || frontline == null)
            return;

        Vector3 target = frontline.frontline;

        int friendly = director.CountTeam(0);
        int enemy = director.CountTeam(1);

        // 優勢側が攻撃
        if (friendly > enemy + 2)
        {
            SpawnPackage(0, target);
        }

        if (enemy > friendly + 2)
        {
            SpawnPackage(1, target);
        }
    }

    void SpawnPackage(int team, Vector3 target)
    {
        Vector3 spawn =
            team == 0 ? director.baseA.position :
                        director.baseB.position;

        // Escort
        director.SpawnPair(spawn, team);

        // Strike
        director.SpawnPair(spawn, team);

        Debug.Log("Strike Package Launched");
    }
}