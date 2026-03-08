using UnityEngine;
using System.Collections.Generic;
using RWS.AI;
public class DirectorAI : MonoBehaviour
{
    public static DirectorAI Instance;

    [Header("Bases")]
    public Transform baseA;
    public Transform baseB;

    [Header("Spawn")]
    public GameObject fighterPrefab;
    public int maxAircraftPerTeam = 20;
    public int maxScrambleAircraft = 6;

    [Header("Taxi")]
    public TaxiPath taxiPathA;
    public TaxiPath taxiPathB;

    [Header("Battlefield")]
    public MiniMapCombatCluster combatCluster;
    public MiniMapAirspaceSystem airspaceSystem;
    public DynamicFrontlineSystem frontline;
    [SerializeField] MiniMapFrontLineSystem frontLine;

    public float mapSize = 50000f;
    public float scrambleRadius = 8000f;
    public float scrambleCooldown = 20f;
    public float reinforceCooldown = 20f;

    [Header("CAP Control")]
    public int maxCAPPerTeam = 4;
    public float capSpawnCooldown = 20f;

    float nextCAPSpawnTimeTeam0 = 0f;
    float nextCAPSpawnTimeTeam1 = 0f;

    [Header("Debug/State")]
    public int team;

    float thinkTimer;
    float nextReinforceTime = 0f;
    float nextScrambleTime = 0f;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (baseA == null || baseB == null)
        {
            Debug.LogWarning($"[DirectorAI] baseA または baseB が未設定です: {name}");
        }
    }

    void Update()
    {
        thinkTimer += Time.deltaTime;

        if (thinkTimer > 5f)
        {
            thinkTimer = 0f;
            EvaluateBattle();
        }
    }
    bool HasValidBases()
    {
        return baseA != null && baseB != null;
    }

    void EvaluateBattle()
    {
        if (!HasValidBases())
            return;
        MaintainAircraftNumbers();
        EvaluateFrontLineMissions();
        CheckScramble();
    }

    void MaintainAircraftNumbers()
    {
        int friendly = CountTeam(0);
        int enemy = CountTeam(1);

        if (baseA != null && friendly < maxAircraftPerTeam)
            SpawnPair(baseA.position, 0);

        if (baseB != null && enemy < maxAircraftPerTeam)
            SpawnPair(baseB.position, 1);

        SpawnReinforcement();
    }
    bool SpawnCAP(int team)
    {
        if (frontline == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnCAP aborted: frontline is null");
            return false;
        }

        if (fighterPrefab == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnCAP aborted: fighterPrefab is null");
            return false;
        }

        Vector3 pos = frontline.frontline;

        var obj = Instantiate(fighterPrefab, pos, Quaternion.identity);
        if (obj == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnCAP aborted: Instantiate returned null");
            return false;
        }

        var brain = obj.GetComponent<Brain>();
        var marker = obj.GetComponent<MiniMapMarker>();

        if (marker != null)
            marker.team = team;

        if (brain == null)
        {
            Debug.LogWarning($"[DirectorAI] SpawnCAP aborted: Brain not found on {obj.name}");
            Destroy(obj);
            return false;
        }

        brain.team = team;
        brain.state = AIState.CAP;
        brain.role = AIRole.CAP;

        if (team == 0)
            brain.taxiPath = taxiPathA;
        else
            brain.taxiPath = taxiPathB;

        var cap = FindCAPZone(team);
        if (cap != null)
            brain.capZone = cap;
        else
            Debug.LogWarning($"[DirectorAI] SpawnCAP: no CAP zone found for team {team}");

        return true;
    }
    void SpawnFighter(Vector3 pos, int team)
    {
        if (fighterPrefab == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnFighter aborted: fighterPrefab is null");
            return;
        }

        var go = Instantiate(fighterPrefab, pos, Quaternion.identity);
        if (go == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnFighter aborted: Instantiate returned null");
            return;
        }

        var marker = go.GetComponent<MiniMapMarker>();
        if (marker != null)
            marker.team = team;

        var brain = go.GetComponent<Brain>();
        if (brain == null)
        {
            Debug.LogWarning($"[DirectorAI] SpawnFighter aborted: Brain not found on {go.name}");
            Destroy(go);
            return;
        }

        brain.team = team;

        if (Random.value < 0.08f)
        {
            brain.isAce = true;
            Debug.Log("ACE SPAWNED");
        }

        brain.role = AIRole.Patrol;

        if (team == 0)
            brain.taxiPath = taxiPathA;
        else
            brain.taxiPath = taxiPathB;

        brain.state = AIState.Taxi;

        if (AWACS.Instance != null)
            brain.missionPoint = AWACS.Instance.battleCenter;
    }

    public void SpawnPair(Vector3 pos, int team)
    {
        if (fighterPrefab == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnPair aborted: fighterPrefab is null");
            return;
        }

        TaxiPath taxi = (team == 0) ? taxiPathA : taxiPathB;

        var leaderObj = Instantiate(fighterPrefab, pos, Quaternion.identity);
        if (leaderObj == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnPair aborted: leader Instantiate returned null");
            return;
        }

        var leaderBrain = leaderObj.GetComponent<Brain>();
        if (leaderBrain == null)
        {
            Debug.LogWarning($"[DirectorAI] SpawnPair aborted: Brain not found on leader {leaderObj.name}");
            Destroy(leaderObj);
            return;
        }

        var leaderMarker = leaderObj.GetComponent<MiniMapMarker>();
        if (leaderMarker != null)
            leaderMarker.team = team;

        leaderBrain.role = AIRole.Leader;
        leaderBrain.team = team;
        leaderBrain.taxiPath = taxi;
        leaderBrain.state = AIState.Taxi;

        if (Random.value < 0.08f)
            leaderBrain.isAce = true;

        var wingObj = Instantiate(fighterPrefab, pos + Vector3.right * 30f, Quaternion.identity);
        if (wingObj == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnPair aborted: wing Instantiate returned null");
            Destroy(leaderObj);
            return;
        }

        var wingBrain = wingObj.GetComponent<Brain>();
        if (wingBrain == null)
        {
            Debug.LogWarning($"[DirectorAI] SpawnPair aborted: Brain not found on wing {wingObj.name}");
            Destroy(wingObj);
            Destroy(leaderObj);
            return;
        }

        var wingMarker = wingObj.GetComponent<MiniMapMarker>();
        if (wingMarker != null)
            wingMarker.team = team;

        wingBrain.role = AIRole.Wingman;
        wingBrain.leader = leaderObj.transform;
        wingBrain.team = team;
        wingBrain.taxiPath = taxi;
        wingBrain.state = AIState.Taxi;

        if (Random.value < 0.08f)
            wingBrain.isAce = true;
    }
    public int CountTeam(int team)
    {
        if (MiniMapManager.Instance == null)
            return 0;

        if (MiniMapManager.Instance.aircraft == null)
            return 0;

        int c = 0;

        foreach (var a in MiniMapManager.Instance.aircraft)
        {
            if (a == null)
                continue;

            if (a.team == team)
                c++;
        }

        return c;
    }

    int CountCAP(int team)
    {
        int count = 0;

        var brains = FindObjectsOfType<Brain>();
        foreach (var b in brains)
        {
            if (b == null) continue;
            if (b.team != team) continue;
            if (b.role != AIRole.CAP) continue;

            count++;
        }

        return count;
    }

    Vector3 GetFrontLineCenter()
    {
        if (frontLine == null)
            return Vector3.zero;

        if (frontLine.line == null)
            return Vector3.zero;

        if (frontLine.line.positionCount == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;

        for (int i = 0; i < frontLine.line.positionCount; i++)
            sum += frontLine.line.GetPosition(i);

        return sum / frontLine.line.positionCount;
    }

    bool EnemyDeepStrike()
    {
        if (MiniMapManager.Instance == null)
            return false;

        if (MiniMapManager.Instance.aircraft == null)
            return false;

        if (AWACS.Instance == null)
            return false;

        foreach (var a in MiniMapManager.Instance.aircraft)
        {
            if (a == null) continue;
            if (a.team != 1) continue;

            Vector3 center = AWACS.Instance.battleCenter;
            float d = Vector3.Distance(a.WorldPosition, center);

            if (d > mapSize * 0.4f)
                return true;
        }

        return false;
    }
    void SpawnReinforcement()
    {
        if (MiniMapManager.Instance == null)
            return;

        if (MiniMapManager.Instance.aircraft == null)
            return;

        if (MiniMapManager.Instance.aircraft.Count > 40)
            return;

        if (Time.time < nextReinforceTime)
            return;

        nextReinforceTime = Time.time + reinforceCooldown;

        if (combatCluster == null)
            return;

        var cluster = FindPriorityCluster();
        if (cluster == null)
            return;

        Vector3 pos = GetFrontLineCenter();

        if (pos == Vector3.zero && frontline != null)
            pos = frontline.frontline;

        int friendly = CountTeam(0);
        int enemy = CountTeam(1);

        if (friendly < enemy)
            SpawnPair(pos, 0);
        else if (enemy < friendly)
            SpawnPair(pos, 1);
    }

    CombatCluster FindPriorityCluster()
    {
        if (combatCluster == null || combatCluster.clusters == null || airspaceSystem == null)
            return null;

        CombatCluster best = null;
        float bestScore = float.MinValue;

        foreach (var c in combatCluster.clusters)
        {
            if (c == null)
                continue;

            float influence = airspaceSystem.GetBattleInfluence(c.center);
            float score = c.size + influence * 2f;

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        return best;
    }
    bool CanSpawnCAP(int team)
    {
        if (team == 0)
        {
            if (Time.time < nextCAPSpawnTimeTeam0)
                return false;
        }
        else
        {
            if (Time.time < nextCAPSpawnTimeTeam1)
                return false;
        }

        if (CountCAP(team) >= maxCAPPerTeam)
            return false;

        return true;
    }
    void MarkCAPSpawned(int team)
    {
        if (team == 0)
            nextCAPSpawnTimeTeam0 = Time.time + capSpawnCooldown;
        else
            nextCAPSpawnTimeTeam1 = Time.time + capSpawnCooldown;
    }

    void SpawnIntercept()
    {
        if (baseA == null)
        {
            Debug.LogWarning("[DirectorAI] SpawnIntercept aborted: baseA is null");
            return;
        }

        Vector3 pos = baseA.position;
        SpawnFighter(pos, 0);
    }

    MiniMapAirspaceZone FindCAPZone(int team)
    {
        if (airspaceSystem == null)
        {
            Debug.LogWarning("[DirectorAI] FindCAPZone failed: airspaceSystem is null");
            return null;
        }

        if (airspaceSystem.zones == null)
        {
            Debug.LogWarning("[DirectorAI] FindCAPZone failed: zones is null");
            return null;
        }

        foreach (var z in airspaceSystem.zones)
        {
            if (z == null) continue;
            if (z.zoneType != AirspaceZoneType.Base) continue;

            return z;
        }

        Debug.LogWarning("[DirectorAI] FindCAPZone: no matching Base zone found");
        return null;
    }

    void EvaluateFrontLineMissions()
    {
        if (CanSpawnCAP(0) && SpawnCAP(0))
            MarkCAPSpawned(0);

        if (CanSpawnCAP(1) && SpawnCAP(1))
            MarkCAPSpawned(1);

        if (EnemyDeepStrike())
            SpawnIntercept();
    }

    bool EnemyNearBase(Transform baseTransform, int enemyTeam)
    {
        if (baseTransform == null)
            return false;

        if (MiniMapManager.Instance == null)
            return false;

        if (MiniMapManager.Instance.aircraft == null)
            return false;

        foreach (var a in MiniMapManager.Instance.aircraft)
        {
            if (a == null) continue;
            if (a.team != enemyTeam) continue;

            float d = Vector3.Distance(a.WorldPosition, baseTransform.position);

            if (d < scrambleRadius)
                return true;
        }

        return false;
    }
    void CheckScramble()
    {
        if (Time.time < nextScrambleTime)
            return;

        int friendly = CountTeam(0);
        int enemy = CountTeam(1);

        if (baseA != null && EnemyNearBase(baseA, 1) && friendly < maxScrambleAircraft)
        {
            SpawnPair(baseA.position, 0);
            nextScrambleTime = Time.time + scrambleCooldown;
        }

        if (baseB != null && EnemyNearBase(baseB, 0) && enemy < maxScrambleAircraft)
        {
            SpawnPair(baseB.position, 1);
            nextScrambleTime = Time.time + scrambleCooldown;
        }
    }
}