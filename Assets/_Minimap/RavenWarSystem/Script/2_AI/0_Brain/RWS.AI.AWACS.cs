using UnityEngine;
using System.Collections.Generic;

namespace RWS.AI
{
    public class AWACS : MonoBehaviour
    {
        public static AWACS Instance;

        [Header("Battlefield")]
        public Vector3 battleCenter;
        public float battleRadius = 20000f;
        public float tacticalThinkInterval = 3f;

        [Header("Refs")]
        public MiniMapCombatClusterSystem combatCluster;
        public MiniMapCombatHeatmap heatmap;
        public DirectorAI director;

        [Header("Runtime")]
        public List<MiniMapMarker> friendly = new List<MiniMapMarker>();
        public List<MiniMapMarker> enemy = new List<MiniMapMarker>();

        private readonly Dictionary<MiniMapMarker, int> reservations =
            new Dictionary<MiniMapMarker, int>();

        private float flightTimer;
        private float nextThink;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (MiniMapDataBus.Instance == null)
                return;

            UpdateLists();
            UpdateBattleCenter();
            TacticalThink();

            flightTimer += Time.deltaTime;
            if (flightTimer > 5f)
            {
                flightTimer = 0f;
                RebuildFlights();
            }
        }

        void RebuildFlights()
        {
            if (MiniMapDataBus.Instance == null)
                return;

            foreach (var m in MiniMapDataBus.Instance.aircraft)
            {
                if (m == null) continue;

                var brain = m.GetComponent<Brain>();
                if (brain != null)
                    brain.flightId = -1;
            }

            var t0 = CollectBrains(0);
            AssignFlightsOf4(t0);

            var t1 = CollectBrains(1);
            AssignFlightsOf4(t1);
        }

        List<Brain> CollectBrains(int team)
        {
            List<Brain> list = new List<Brain>();

            if (MiniMapDataBus.Instance == null)
                return list;

            foreach (var m in MiniMapDataBus.Instance.aircraft)
            {
                if (m == null) continue;
                if (m.team != team) continue;

                var brain = m.GetComponent<Brain>();
                if (brain == null) continue;

                list.Add(brain);
            }

            return list;
        }

        void UpdateLists()
        {
            friendly.Clear();
            enemy.Clear();

            if (MiniMapDataBus.Instance == null)
                return;

            foreach (var m in MiniMapDataBus.Instance.aircraft)
            {
                if (m == null) continue;

                if (m.team == 0)
                    friendly.Add(m);
                else
                    enemy.Add(m);
            }
        }

        void TacticalThink()
        {
            if (Time.time < nextThink)
                return;

            nextThink = Time.time + tacticalThinkInterval;

            var threat = GetHighestThreatCluster();
            if (threat == null)
                return;

            int friendlyCount = CountNear(threat.center, 0, battleRadius * 0.5f);
            int enemyCount = CountNear(threat.center, 1, battleRadius * 0.5f);

            // 劣勢なら増援要求
            if (friendlyCount < enemyCount)
            {
                if (director != null)
                    director.SpawnPair(threat.center, 0);
            }
        }

        void UpdateBattleCenter()
        {
            if (MiniMapDataBus.Instance == null)
                return;

            Vector3 sum = Vector3.zero;
            int count = 0;

            foreach (var m in MiniMapDataBus.Instance.aircraft)
            {
                if (m == null) continue;

                sum += m.WorldPosition;
                count++;
            }

            if (count > 0)
                battleCenter = sum / count;
        }

        int CountNear(Vector3 center, int team, float radius)
        {
            int count = 0;

            if (MiniMapDataBus.Instance == null)
                return count;

            float sqrRadius = radius * radius;

            foreach (var m in MiniMapDataBus.Instance.aircraft)
            {
                if (m == null) continue;
                if (m.team != team) continue;

                Vector3 delta = m.WorldPosition - center;
                if (delta.sqrMagnitude <= sqrRadius)
                    count++;
            }

            return count;
        }

        public MiniMapMarker FindBestTarget(MiniMapMarker self)
        {
            if (self == null)
                return null;

            MiniMapMarker best = null;
            float bestScore = float.MaxValue;

            var targetList = (self.team == 0) ? enemy : friendly;

            foreach (var m in targetList)
            {
                if (m == null) continue;

                float d = Vector3.Distance(self.WorldPosition, m.WorldPosition);
                float friendDist = DistanceToNearestFriendly(self, m);

                float score = d + m.attackers * 1500f;
                score -= Mathf.Clamp(2000f - friendDist, 0, 2000f);

                if (score < bestScore)
                {
                    bestScore = score;
                    best = m;
                }
            }

            if (best != null)
                best.attackers++;

            return best;
        }

        float DistanceToNearestFriendly(MiniMapMarker self, MiniMapMarker enemyTarget)
        {
            if (self == null || enemyTarget == null)
                return float.MaxValue;

            float best = float.MaxValue;
            var list = (self.team == 0) ? friendly : enemy;

            foreach (var f in list)
            {
                if (f == null) continue;
                if (f == self) continue;

                float d = Vector3.Distance(enemyTarget.WorldPosition, f.WorldPosition);
                if (d < best)
                    best = d;
            }

            return best;
        }

        public CombatCluster GetHighestThreatCluster()
        {
            if (combatCluster == null || heatmap == null || combatCluster.clusters == null)
                return null;

            CombatCluster best = null;
            float bestScore = 0f;

            foreach (var c in combatCluster.clusters)
            {
                if (c == null) continue;

                float heat = heatmap.GetHeat(c.center);
                float score = c.size * 2f + heat;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }

            return best;
        }

        public Vector3 GetInterceptVector(MiniMapMarker self)
        {
            if (self == null)
                return battleCenter;

            var threat = GetHighestThreatCluster();
            if (threat == null)
                return battleCenter;

            Vector3 dir = (threat.center - self.WorldPosition).normalized;
            return threat.center - dir * 800f;
        }

        public MiniMapMarker ReserveTarget(MiniMapMarker self)
        {
            if (self == null || MiniMapDataBus.Instance == null)
                return null;

            MiniMapMarker best = null;
            float bestScore = float.MaxValue;

            foreach (var m in MiniMapDataBus.Instance.aircraft)
            {
                if (m == null) continue;
                if (m.team == self.team) continue;

                reservations.TryGetValue(m, out int attackers);

                float d = Vector3.Distance(self.WorldPosition, m.WorldPosition);
                float score = d + attackers * 3000f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = m;
                }
            }

            if (best != null)
            {
                if (!reservations.ContainsKey(best))
                    reservations[best] = 0;

                reservations[best]++;
            }

            return best;
        }

        public void ReleaseTarget(MiniMapMarker target)
        {
            if (target == null)
                return;

            if (!reservations.ContainsKey(target))
                return;

            reservations[target]--;

            if (reservations[target] <= 0)
                reservations.Remove(target);
        }

        public void AssignWingmen(List<Brain> aircraft)
        {
            if (aircraft == null)
                return;

            for (int i = 0; i < aircraft.Count; i += 2)
            {
                if (i + 1 >= aircraft.Count)
                    break;

                var leader = aircraft[i];
                var wing = aircraft[i + 1];

                if (leader == null || wing == null)
                    continue;

                leader.role = AIRole.Leader;
                wing.role = AIRole.Wingman;
                wing.leader = leader.transform;
            }
        }

        public void AssignFlightsOf4(List<Brain> brains)
        {
            if (brains == null)
                return;

            int flightCounter = 0;
            List<Brain> remaining = new List<Brain>(brains);

            while (remaining.Count >= 4)
            {
                var leader = remaining[0];
                remaining.RemoveAt(0);

                if (leader == null)
                    continue;

                // leaderに一番近い3機
                remaining.Sort((x, y) =>
                {
                    float da = (x == null)
                        ? float.MaxValue
                        : Vector3.Distance(leader.transform.position, x.transform.position);

                    float db = (y == null)
                        ? float.MaxValue
                        : Vector3.Distance(leader.transform.position, y.transform.position);

                    return da.CompareTo(db);
                });

                var b = remaining[0];
                var c = remaining[1];
                var d = remaining[2];

                remaining.RemoveRange(0, 3);

                if (b == null || c == null || d == null)
                    continue;

                int id = flightCounter++;

                leader.role = AIRole.Leader;
                b.role = AIRole.Wingman;
                b.leader = leader.transform;

                c.role = AIRole.ElementLeader;
                d.role = AIRole.ElementWingman;
                d.leader = c.transform;

                leader.flightId = id;
                b.flightId = id;
                c.flightId = id;
                d.flightId = id;
            }
        }

        void CreateStrikePackage(List<Brain> brains)
        {
            if (brains == null || brains.Count < 4)
                return;

            var a = brains[0];
            var b = brains[1];
            var c = brains[2];
            var d = brains[3];

            if (a == null || b == null || c == null || d == null)
                return;

            a.role = AIRole.Leader;

            b.role = AIRole.Wingman;
            b.leader = a.transform;

            c.role = AIRole.ElementLeader;

            d.role = AIRole.ElementWingman;
            d.leader = c.transform;
        }
    }
}