using UnityEngine;
using System.Collections.Generic;

namespace RWS.AI
{
    public enum AIState
    {
        Taxi,
        Takeoff,
        Patrol,
        Engage,
        Extend,
        BreakTurn,
        CAP,
        ReturnToBattle,
        RTB,
        Landing,
        Merge,
        Intercept,
        Evade
    }

    public enum AIRole
    {
        Patrol,
        CAP,
        Intercept,
        Strike,
        Leader,
        Wingman,
        ElementLeader,
        ElementWingman
    }

    public class Brain : MonoBehaviour
    {
        public AIState state = AIState.Taxi;
        public AIRole role;

        public MiniMapMarker self;
        public Rigidbody rb;
        public MiniMapMarker target;

        public MiniMapCombatHeatmap heatmap;
        public MiniMapAirspaceSystem airspaceSystem;

        public Transform leader;
        public Vector3 desiredPosition;

        float thinkTimer;
        public float thinkInterval = 0.5f;

        float combatCommitTimer = 0f;
        float combatCommitTime = 8f;

        public bool isAce = false;
        float flankSign;
        public TaxiPath taxiPath;
        public int taxiIndex = 0;
        public float taxiArriveDist = 20f;
        public float taxiLookAhead = 60f;

        public Vector3 missionPoint;
        public MiniMapAirspaceZone capZone;
        public float capRadius = 1500f;
        Vector3 capCenter;

        public int flightId = -1;
        public bool missileThreat;
        public float aceAim = 1.5f;
        public float aceAggression = 1.4f;

        public GameObject missilePrefab;
        public Transform missileHardpoint;

        public float missileRange = 1200f;
        public float missileCooldown = 6f;
        float nextMissileTime;
        public Transform incomingMissile;
        [Range(0f, 1f)] public float health = 1f;
        public int team;
        public AIState currentState;

        void Start()
        {
            state = AIState.Taxi;

            if (self == null)
                self = GetComponent<MiniMapMarker>();

            if (rb == null)
                rb = GetComponent<Rigidbody>();

            flankSign = Random.value > 0.5f ? 1f : -1f;

            if (AWACS.Instance == null)
                Debug.LogWarning($"[AI] {name} : AWACS.Instance is null");
        }

        void Update()
        {   
            thinkTimer += Time.deltaTime;
            if (thinkTimer < thinkInterval)
                return;

            thinkTimer = 0f;

            if (AWACS.Instance != null)
            {
                Vector3 toCenter = AWACS.Instance.battleCenter - transform.position;

                if (toCenter.magnitude > AWACS.Instance.battleRadius)
                {
                    state = AIState.ReturnToBattle;
                    ClearTarget();
                }
            }

            combatCommitTimer -= thinkInterval;
            UpdateTarget();
            Think();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(desiredPosition, 200f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, desiredPosition);
        }

        void UpdateTarget()
        {
            if (state != AIState.Patrol && state != AIState.Engage)
                return;

            ClearTarget();

            if (AWACS.Instance != null && self != null)
                target = AWACS.Instance.ReserveTarget(self);

            combatCommitTimer = combatCommitTime;
        }

        void SetTarget(MiniMapMarker t)
        {
            if (target != null)
                target.attackers--;

            target = t;

            if (target != null)
                target.attackers++;
        }

        void ThinkRole()
        {
            switch (role)
            {
                case AIRole.Patrol:
                    ThinkPatrol();
                    break;

                case AIRole.CAP:
                    ThinkCAP();
                    break;

                case AIRole.Intercept:
                    break;

                case AIRole.Strike:
                    break;

                case AIRole.Leader:
                case AIRole.ElementLeader:
                    ThinkLeader();
                    break;

                case AIRole.Wingman:
                case AIRole.ElementWingman:
                    ThinkWingman();
                    break;
            }
        }

        void Think()
        {
            // 地上フェーズは専用処理のみ
            if (state == AIState.Taxi)
            {
                ThinkTaxi();
                return;
            }

            if (state == AIState.Takeoff)
            {
                ThinkTakeoff();
                return;
            }

            // 空戦フェーズのみ戦術判断
            EvaluateTacticalState();
            ThinkRole();

            switch (state)
            {
                case AIState.Patrol:
                    ThinkPatrol();
                    break;

                case AIState.BreakTurn:
                    ThinkBreakTurn();
                    break;

                case AIState.Extend:
                    ThinkExtend();
                    break;

                case AIState.ReturnToBattle:
                    ThinkReturnToBattle();
                    break;

                case AIState.RTB:
                    ThinkRTB();
                    break;

                case AIState.Landing:
                    ThinkLanding();
                    break;

                case AIState.Merge:
                    ThinkMerge();
                    break;
            }
        }

        bool IsTaxiBlocked(float checkDist = -1f)
        {
            if (checkDist < 0f && rb != null)
                checkDist = Mathf.Lerp(20f, 80f, rb.velocity.magnitude / 50f);

            if (MiniMapManager.Instance == null)
                return false;

            foreach (var m in MiniMapManager.Instance.aircraft)
            {
                if (m == null || m == self) continue;
                if (self != null && m.team != self.team) continue;

                Vector3 to = m.WorldPosition - self.WorldPosition;
                float forward = Vector3.Dot(transform.forward, to.normalized);

                if (forward > 0f && to.magnitude < checkDist)
                    return true;
            }

            return false;
        }
        void ThinkTaxi()
        {
            if (taxiPath == null || taxiPath.Count == 0)
            {
                state = AIState.Takeoff;
                return;
            }

            // 最後の2点は Takeoff 用に使う
            // なので、その1つ前に到達したら離陸へ移行
            int takeoffStartIndex = Mathf.Max(0, taxiPath.Count - 3);

            if (IsTaxiBlocked())
            {
                desiredPosition = transform.position - transform.forward * 5f;
                return;
            }

            Vector3 p = taxiPath.GetPoint(taxiIndex);
            p.y = transform.position.y;

            Vector3 toP = p - transform.position;
            Vector3 aim = p;

            if (toP.sqrMagnitude > 0.01f)
                aim = p + toP.normalized * taxiLookAhead;

            aim.y = transform.position.y;
            desiredPosition = aim;

            if (Vector3.Distance(transform.position, p) < taxiArriveDist)
            {
                if (taxiIndex >= takeoffStartIndex)
                {
                    state = AIState.Takeoff;
                    return;
                }

                taxiIndex++;
            }
        }

void ThinkTakeoff()
{
    Vector3 takeoffDir = transform.forward;
    Vector3 runwayCenter = transform.position;

    if (taxiPath != null && taxiPath.Count >= 2)
    {
        Vector3 a = taxiPath.GetPoint(taxiPath.Count - 2);
        Vector3 b = taxiPath.GetPoint(taxiPath.Count - 1);

        takeoffDir = (b - a).normalized;
        takeoffDir.y = 0f;
        takeoffDir.Normalize();

        Vector3 fromA = transform.position - a;
        float along = Vector3.Dot(fromA, takeoffDir);

        Vector3 projected = a + takeoffDir * along;

        Vector3 lateral = projected - transform.position;
        lateral.y = 0f;

        float maxLateralCorrection = 40f;
        if (lateral.magnitude > maxLateralCorrection)
            lateral = lateral.normalized * maxLateralCorrection;

        runwayCenter = transform.position + lateral;
    }

    runwayCenter.y = transform.position.y;

    desiredPosition = runwayCenter + takeoffDir * 1200f;
    desiredPosition.y = transform.position.y;

    if (rb != null)
    {
        float speed = rb.velocity.magnitude;
        float upSpeed = Vector3.Dot(rb.velocity, Vector3.up);

        if (speed > 80f && transform.position.y > 5f)
            state = AIState.Patrol;
    }
}
        void ThinkPatrol()
        {
           if (missionPoint != Vector3.zero)
            {
                desiredPosition = missionPoint;

                if (Vector3.Distance(transform.position, missionPoint) < 500f)
                    missionPoint = Vector3.zero;

                return;
            }

            if (role == AIRole.Wingman && leader != null)
            {
                FollowLeader();
                return;
            }

            if (role == AIRole.Wingman && leader == null)
                role = AIRole.Patrol;

            if (target != null)
            {
                float distToTarget =
                    Vector3.Distance(transform.position, target.WorldPosition);

                if (distToTarget < 6000f)
                    state = AIState.Engage;
            }

            if (heatmap != null && self != null)
            {
                if (heatmap.GetHeat(self.WorldPosition) > 0.8f)
                    state = AIState.Engage;
            }

            if (airspaceSystem != null)
            {
                var zone = airspaceSystem.GetZone(transform.position);

                if (zone != null)
                {
                    if (zone.control == AirspaceControl.Enemy ||
                        zone.control == AirspaceControl.Contested)
                    {
                        state = AIState.Engage;
                    }
                }
            }

            if (AWACS.Instance != null && self != null)
                desiredPosition = AWACS.Instance.GetInterceptVector(self);
        }

        void ThinkMerge()
        {
            if (target == null || self == null)
            {
                state = AIState.Patrol;
                return;
            }

            Vector3 toTarget = target.WorldPosition - self.WorldPosition;

            desiredPosition =
                self.WorldPosition + toTarget.normalized * 800f;

            if (toTarget.magnitude < 500f)
                state = AIState.Engage;
        }

        void ThinkExtend()
        {
            if (target == null)
            {
                state = AIState.Patrol;
                return;
            }

            Vector3 dir =
                (transform.position - target.WorldPosition).normalized;

            desiredPosition =
                transform.position + dir * 2000f;
        }

        bool HasEnergyAdvantage(MiniMapMarker enemy)
        {
            if (enemy == null || rb == null || enemy.rb == null)
                return true;

            float myEnergy =
                rb.velocity.magnitude +
                transform.position.y * 0.002f;

            float enemyEnergy =
                enemy.rb.velocity.magnitude +
                enemy.WorldPosition.y * 0.002f;

            return myEnergy > enemyEnergy;
        }

        void ThinkBreakTurn()
        {
            if (incomingMissile == null)
            {
                state = AIState.Engage;
                return;
            }

            Vector3 missileDir =
                (incomingMissile.position - transform.position).normalized;

            Vector3 breakDir =
                Vector3.Cross(missileDir, Vector3.up);

            if (Random.value < 0.5f)
                breakDir = -breakDir;

            Vector3 evadePos =
                transform.position +
                breakDir * 800f +
                Vector3.up * 200f;

            evadePos += Vector3.up * 300f;
            evadePos += transform.forward * 200f;
            desiredPosition = evadePos;

            float missileDist =
                Vector3.Distance(transform.position, incomingMissile.position);

            if (missileDist > 1200f)
                state = AIState.Extend;
        }

        void ThinkBarrelRollDefense()
        {
            Vector3 forward = transform.forward;

            Vector3 rollOffset =
                Vector3.Cross(forward, Vector3.up) * 400f;

            Vector3 pos =
                transform.position +
                forward * 600f +
                rollOffset;

            desiredPosition = pos;
        }

        void OnDestroy()
        {
            ClearTarget();
        }

        void ThinkReturnToBattle()
        {
            if (AWACS.Instance == null)
                return;

            Vector3 center = AWACS.Instance.battleCenter;
            desiredPosition = center;

            float d =
                Vector3.Distance(transform.position, center);

            if (d < 2000f)
                state = AIState.Patrol;
        }

        void ThinkRTB()
        {
            if (airspaceSystem == null)
                return;

            Vector3 basePos =
                airspaceSystem.GetClosestBase(transform.position);

            desiredPosition = basePos;

            float d =
                Vector3.Distance(transform.position, basePos);

            if (d < 500f)
                state = AIState.Landing;
        }

        void ClearTarget()
        {
            if (target != null)
            {
                target.attackers--;
                target = null;
            }
        }

        void ThinkLanding()
        {
            desiredPosition =
                transform.position - transform.forward * 200f;

            if (rb != null && rb.velocity.magnitude < 10f)
                Destroy(gameObject);
        }

        void FollowLeader()
        {
            if (leader == null)
                return;

            var leaderBrain = leader.GetComponent<Brain>();

            if (leaderBrain == null || leaderBrain.target == null)
            {
                Vector3 offset =
                    leader.right * 120f - leader.forward * 150f;

                desiredPosition = leader.position + offset;
                return;
            }

            var enemy = leaderBrain.target;
            Vector3 enemyPos = enemy.WorldPosition;

            Vector3 dir =
                (enemyPos - leader.position).normalized;

            Vector3 side =
                Vector3.Cross(Vector3.up, dir);

            float flankDistance = Random.Range(400f, 900f);

            float delay = 200f;
            Vector3 flankPos =
                enemyPos
                + side * flankSign * flankDistance
                - dir * delay;

            desiredPosition = flankPos;
        }

        void ThinkCAP()
        {
            if (capZone == null)
            {
                state = AIState.Patrol;
                return;
            }

            if (capCenter == Vector3.zero)
                capCenter = capZone.transform.position;

            float angle = Time.time * 0.2f;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                0f,
                Mathf.Sin(angle)
            ) * capRadius;

            desiredPosition = capCenter + offset;

            if (MiniMapDataBus.Instance == null || self == null)
                return;

            var aircraft = MiniMapDataBus.Instance.aircraft;

            foreach (var a in aircraft)
            {
                if (a == null) continue;
                if (a.team == self.team) continue;

                if (capZone.IsInside(a.WorldPosition))
                {
                    SetTarget(a);
                    state = AIState.Engage;
                    return;
                }
            }
        }

        float ComputeThreat(MiniMapMarker m)
        {
            if (m == null || self == null)
                return -999f;

            float distance = Vector3.Distance(self.WorldPosition, m.WorldPosition);
            float attackersPenalty = m.attackers * 500f;

            float score = 0f;
            score += Mathf.Clamp(3000f - distance, 0f, 3000f);
            score -= attackersPenalty;

            if (m.rb != null)
                score += m.rb.velocity.magnitude * 0.2f;

            return score;
        }

        Vector3 GetWingmanFormationPosition()
        {
            if (leader == null)
                return transform.position;

            Vector3 offset =
                leader.right * -80f +
                leader.forward * -120f;

            return leader.position + offset;
        }

        Vector3 GetPursuitPoint(MiniMapMarker enemy)
        {
            if (enemy == null || enemy.rb == null)
                return transform.position + transform.forward * 500f;

            Vector3 enemyPos = enemy.WorldPosition;
            Vector3 enemyVel = enemy.rb.velocity;

            float d = Vector3.Distance(transform.position, enemyPos);

            if (d < 800f)
                return enemyPos - enemyVel.normalized * 200f;

            if (d < 1800f)
                return enemyPos;

            return enemyPos + enemyVel * 1.2f;
        }

        void TryFireMissile()
        {
            if (target == null || self == null)
                return;

            if (Time.time < nextMissileTime)
                return;

            float d =
                Vector3.Distance(
                    self.WorldPosition,
                    target.WorldPosition);

            if (d > missileRange)
                return;

            if (missilePrefab == null || missileHardpoint == null)
                return;

            nextMissileTime = Time.time + missileCooldown;

            var m = Instantiate(
                missilePrefab,
                missileHardpoint.position,
                missileHardpoint.rotation);

            var missile = m.GetComponent<SimpleMissile>();

            if (missile != null)
                missile.SetTarget(target.transform);
        }

        void ThinkWingman()
        {
            if (leader == null)
                return;

            Vector3 offset =
                leader.right * 150f - leader.forward * 200f;

            desiredPosition = leader.position + offset;
        }

        void ThinkLeader()
        {
            if (target == null)
            {
                if (AWACS.Instance != null && self != null)
                    target = AWACS.Instance.ReserveTarget(self);

                if (target == null)
                    return;
            }

            if (AWACS.Instance != null && self != null)
                desiredPosition = AWACS.Instance.GetInterceptVector(self);
        }

        MiniMapMarker ChooseThreatTarget()
        {
            if (MiniMapDataBus.Instance == null)
                return null;

            MiniMapMarker best = null;
            float bestScore = -999f;

            foreach (var m in MiniMapDataBus.Instance.aircraft)
            {
                if (m == null) continue;
                if (m.team == team) continue;

                float score = ComputeThreat(m);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = m;
                }
            }

            return best;
        }

        void EvaluateTacticalState()
        {
            if (state == AIState.Taxi ||
                state == AIState.Takeoff ||
                state == AIState.Landing)
                return;

            float scoreEngage = 0f;
            float scoreBreak = 0f;
            float scoreExtend = 0f;
            float scoreRTB = 0f;

            if (AWACS.Instance != null && self != null)
            {
                var cluster = AWACS.Instance.GetHighestThreatCluster();

                if (cluster != null)
                {
                    float d = Vector3.Distance(
                        self.WorldPosition,
                        cluster.center);

                    scoreEngage += Mathf.Clamp(1500f - d, 0f, 1500f);
                }
            }

            if (target != null && self != null)
            {
                float d = Vector3.Distance(
                    self.WorldPosition,
                    target.WorldPosition);

                scoreEngage += Mathf.Clamp(2000f - d, 0f, 2000f);
            }

            if (incomingMissile != null)
                scoreBreak += 3000f;

            float speed = rb != null ? rb.velocity.magnitude : 0f;

            if (speed < 80f)
                scoreExtend += 1500f;

            if (health < 0.2f)
                scoreRTB += 4000f;

            float best = Mathf.Max(scoreEngage, scoreBreak, scoreExtend, scoreRTB);

            if (scoreBreak == best)
                state = AIState.BreakTurn;
            else if (best == scoreExtend)
                state = AIState.Extend;
            else if (best == scoreRTB)
                state = AIState.RTB;
            else
                state = AIState.Engage;
        }

        bool ShouldTwoCircle(Rigidbody myRb, Rigidbody enemyRb)
        {
            if (myRb == null || enemyRb == null)
                return false;

            float mySpeed = myRb.velocity.magnitude;
            float enemySpeed = enemyRb.velocity.magnitude;

            float speedDiff = Mathf.Abs(mySpeed - enemySpeed);

            if (speedDiff > 40f)
                return true;

            Vector3 relVel = myRb.velocity - enemyRb.velocity;

            float facing =
                Vector3.Dot(
                    relVel.normalized,
                    (enemyRb.position - myRb.position).normalized);

            return facing < -0.3f;
        }
        public void ResetAfterCrash()
        {
            state = AIState.Taxi;
            target = null;
            missileThreat = false;
            desiredPosition = transform.position;
            taxiIndex = 0;

            Debug.Log($"AI RESET AFTER CRASH | {name}");
        }
    }
}