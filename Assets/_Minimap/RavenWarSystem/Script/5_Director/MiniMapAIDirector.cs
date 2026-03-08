using UnityEngine;
using System.Collections.Generic;

public class MiniMapAIDirector : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTarget;
    public MiniMapDogfightCamera dogfightCamera;
    public MiniMapCombatClusterSystem clusterSystem;
    public MiniMapCinematicSystem cinematicSystem;
    public MiniMapStrategicDirector strategicDirector;
    public MiniMapBattleAnalytics analytics;
    // public IslandTacticalSystem islandManager;
    public MiniMapBattleFlowSystem battleFlow;
    public MiniMapCombatHeatmap heatmap;
    public MiniMapAirspaceControl airspace;
    public MiniMapCombatNarrative narrative;
    public MiniMapClusterCamera clusterCamera;
    public MiniMapAdaptiveCamera adaptiveCamera;

    [Header("Camera")]
    public float cameraMoveSpeed = 2f;
    public Vector3 cameraOffset = new Vector3(0f, 4f, -6f);

    [Header("Director")]
    public float directorInterval = 0.5f;
    public float focusLockDuration = 4f;

    float directorTimer;
    float focusLockTimer;

    float interestScore;

    public string debugReason;
    public float debugScore;

    CombatCluster focusCluster;
    Vector3 focusPosition;

    MiniMapMarker focusAircraft;
    Transform currentCameraTarget;

    Queue<Vector3> eventFocusQueue = new Queue<Vector3>();


    void Update()
    {
        directorTimer -= Time.deltaTime;
        focusLockTimer -= Time.deltaTime;

        if (directorTimer <= 0f)
        {
            directorTimer = directorInterval;
            EvaluateBattlefield();
        }

        UpdateCamera();
    }


    void EvaluateBattlefield()
    {
        if (cinematicSystem != null && cinematicSystem.IsPlaying())
            return;

        if (focusLockTimer > 0f)
            return;

        focusAircraft = null;
        focusCluster = null;

        interestScore *= 0.99f;

        debugReason = "None";
        debugScore = 0f;

        if (HandleEventFocus())
            return;

            EvaluateClusters();
            EvaluateDogfights();
            EvaluateMissiles();
            EvaluateAggressiveAircraft();
            EvaluateNarrative();
            EvaluateBattleFlow();
            //EvaluateIslands();
            EvaluateStrategicFallback();
    }


    bool HandleEventFocus()
    {
        if (eventFocusQueue.Count == 0)
            return false;

        focusPosition = eventFocusQueue.Dequeue();
        focusLockTimer = 3f;
        interestScore = 10f;

        return true;
    }


    void EvaluateClusters()
    {
        if (clusterSystem == null)
            return;

        foreach (var cluster in clusterSystem.clusters)
        {
            float heat = heatmap != null
                ? heatmap.GetHeat(cluster.center)
                : 0f;

            float score =
                cluster.size * 3f +
                cluster.intensity * 5f +
                heat * 4f;

            float distance =
                Vector3.Distance(cameraTarget.position, cluster.center);

            distance = Mathf.Min(distance, 50f);

            score -= distance * 0.1f;

            if (score > interestScore)
            {
                interestScore = score;

                focusCluster = cluster;

                Vector3 lookAhead = cluster.velocity * 0.5f;

                if (lookAhead.magnitude > 10f)
                    lookAhead = lookAhead.normalized * 10f;

                focusPosition = cluster.center + lookAhead;

                focusAircraft = null;

                debugReason = "Cluster";
                debugScore = score;
            }
        }
    }
    void EvaluateDogfights()
    {
        var dogfight = FindObjectOfType<MiniMapDogfightDetection>();

        if (dogfight == null) return;

        foreach (var pair in dogfight.pairs)
        {
            float score = ComputeDogfightScore(pair);

            if (score > interestScore)
            {
                interestScore = score;

                focusPosition =
                    (pair.a.transform.position +
                    pair.b.transform.position) * 0.5f;

                focusAircraft = pair.a;

                focusCluster = null;

                debugReason = "Dogfight";
                debugScore = score;
            }
        }
    }
    float ComputeDogfightScore(
        DogfightPair pair)
    {
        float score = 0f;

        Vector3 posA = pair.a.transform.position;
        Vector3 posB = pair.b.transform.position;

        float distance = Vector3.Distance(posA, posB);

        score += Mathf.Clamp01(0.2f - distance) * 10f;

        Rigidbody rbA = pair.a.GetComponent<Rigidbody>();
        Rigidbody rbB = pair.b.GetComponent<Rigidbody>();

        if (rbA && rbB)
        {
            float speedDiff =
                Mathf.Abs(rbA.velocity.magnitude -
                        rbB.velocity.magnitude);

            score += speedDiff * 0.1f;
        }

        return score;
    }
    void EvaluateMissiles()
    {
        var missiles = MiniMapDataBus.Instance.GetMissiles();

        foreach (var m in missiles)
        {
            if (m == null)
                continue;

            float heat = heatmap != null
                ? heatmap.GetHeat(m.position)
                : 0f;

            float score = 3f + heat * 2f;

            var mm = m.GetComponent<MiniMapMissileMarker>();

            if (mm != null && mm.target != null)
                score += 5f;

            if (score > interestScore)
            {
                interestScore = score;

                focusPosition = m.position;
                focusAircraft = null;
                focusCluster = null;

                debugReason = "Missile";
                debugScore = score;
            }
        }
    }


    void EvaluateAggressiveAircraft()
    {

        if (analytics == null)
            return;

        var best = analytics.GetMostAggressive();

        if (best == null)
            return;

        float heat = heatmap != null
            ? heatmap.GetHeat(best.marker.transform.position)
            : 0f;

        float score =
            best.aggressionScore +
            heat * 3f;

        if (score > interestScore)
        {
            interestScore = score;

            focusAircraft = best.marker;

            Vector3 pos = best.marker.transform.position;

            Rigidbody rb = best.marker.GetComponent<Rigidbody>();

            if (rb != null)
                pos += rb.velocity * 0.5f;

            focusPosition = pos;

            debugReason = "Aggressive Aircraft";
            debugScore = score;
        }
    }
    void EvaluateBattleFlow()
    {
        if (battleFlow == null) return;

        battleFlow.EvaluateFlow();

        float score =
            battleFlow.frontlineStrength * 0.5f;

        if (score > interestScore)
        {
            interestScore = score;

            focusPosition = battleFlow.frontline;

            focusAircraft = null;
            focusCluster = null;

            debugReason = "Frontline";
            debugScore = score;
        }
    }
    void EvaluateNarrative()
    {
        if (narrative == null)
            return;

        if (narrative.phase ==
            MiniMapCombatNarrative.BattlePhase.Dogfight)
        {
            interestScore += 3f;
        }
    }

    //void EvaluateIslands()
    //{
        //if (islandManager == null)
        //    return;

     //var island = islandManager.GetIsland(focusPosition);
//
//   if (island == null)
//       return;
//
//   interestScore += island.tacticalValue * 0.2f;
// }
    void EvaluateAirspace()
    {
        if (airspace == null)
            return;

        Vector3 pos = cameraTarget.position;

        float control =
            airspace.GetControl(pos);

        float score = control * 3f;

        if (score > interestScore)
        {
            interestScore = score;

            focusPosition = pos;

            debugReason = "Airspace Control";
            debugScore = score;
        }
    }
    void EvaluateStrategicFallback()
    {
        if (strategicDirector == null)
            return;

        strategicDirector.EvaluateStrategic();

        float score = strategicDirector.strategicScore;

        if (score > interestScore)
        {
            interestScore = score;

            focusPosition = strategicDirector.strategicFocus;

            focusAircraft = null;
            focusCluster = null;
        }
    }


    void DecideCameraMode()
    {
        if (adaptiveCamera == null)
            return;

        if (focusCluster != null && focusCluster.size > 2)
        {
            adaptiveCamera.SetMode(MiniMapCameraMode.Cluster);
            return;
        }

        if (focusAircraft != null)
        {
            adaptiveCamera.SetMode(MiniMapCameraMode.Dogfight);
            return;
        }

        adaptiveCamera.SetMode(MiniMapCameraMode.Idle);
    }


    void UpdateCamera()
    {
        if (cinematicSystem != null && cinematicSystem.IsPlaying())
            return;

        Vector3 desiredPos = focusPosition + cameraOffset;

        cameraTarget.position = Vector3.Lerp(
            cameraTarget.position,
            desiredPos,
            Time.deltaTime * cameraMoveSpeed
        );

        if (focusAircraft != null)
        {
            if (currentCameraTarget != focusAircraft.transform)
            {
                currentCameraTarget = focusAircraft.transform;
                dogfightCamera.SetTarget(currentCameraTarget);
            }
        }

        DecideCameraMode();
    }


    public void RegisterEventFocus(Vector3 position)
    {
        eventFocusQueue.Enqueue(position);
    }
}