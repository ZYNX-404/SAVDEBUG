using UnityEngine;
using SaccFlightAndVehicles;

namespace RWS.AI
{
    public class AIFlightController : MonoBehaviour
    {
        public Brain brain;
        public Rigidbody rb;
        public SaccAirVehicle sav;
        public DFUNC_ToggleEngine engineToggle;

        public float turnSpeed = 2f;

        [Header("Sacc Bridge Minimal")]
        public bool debugSacc = true;
        public float startupDelay = 8f;
        public float engineRetryInterval = 2f;
        public float dfuncStartupWait = 5.5f;

        bool loggedSavRefDelayed;
        private bool saccReady = false;
        private bool engineWasOnLastFrame = false;
        private bool engineStartRequested = false;
        private bool justEngineOnPending = false;

        private float startupTimer = 0f;
        private float nextEngineRetryTime = 0f;
        private float engineOnTime = -1f;
        private float justEngineOnAt = -1f;

        private int _dbgLastAiHeartbeatFrame = -9999;
        private int _dbgLastReadyBlockedFrame = -9999;
        private int _dbgLastLoopActiveFrame = -9999;
        
        private bool dumpedGoComps;

        private static string GetPath(Transform t)
        {
            if (t == null) return "null";
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        void DumpComponents(GameObject go)
        {
            var comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                Debug.LogWarning(
                    $"[GO COMP] go={go.name}#{go.GetInstanceID()} | " +
                    $"type={(c != null ? c.GetType().FullName : "null")} | " +
                    $"compId={(c != null ? c.GetInstanceID().ToString() : "null")}"
                );
            }
        }

        void Update()
        {
            startupTimer += Time.deltaTime;

            if (!dumpedGoComps && startupTimer > 1.0f && sav != null)
            {
                dumpedGoComps = true;
                DumpComponents(sav.gameObject);
            }
            // 追加: 開始直後の時差を避けて、少し遅らせて1回だけ参照確認
            if (debugSacc && !loggedSavRefDelayed && startupTimer > 1.0f)
            {
                loggedSavRefDelayed = true;
                Debug.Log(
                    $"AI SAV REF DELAYED | " +
                    $"savCompId={(sav != null ? sav.GetInstanceID().ToString() : "null")} | " +
                    $"savGoId={(sav != null ? sav.gameObject.GetInstanceID().ToString() : "null")} | " +
                    $"savObj={(sav != null ? sav.gameObject.name : "null")} | " +
                    $"savPath={(sav != null ? GetPath(sav.transform) : "null")} | " +
                    $"startupTimer={startupTimer:F2}"
                );
            }
        }
        private System.Collections.IEnumerator LogEngineStateFramesCoroutine()
        {
            yield return null;
            LogEngineStateAtLabel("NEXTFRAME+1");

            yield return null;
            LogEngineStateAtLabel("NEXTFRAME+2");

            yield return null;
            LogEngineStateAtLabel("NEXTFRAME+3");
        }

        public void LogEngineStateAtLabel(string label)
        {
            Debug.LogWarning(
                $"[AI ENGINE {label}] | " +
                $"frame={Time.frameCount} | " +
                $"time={Time.time:F2} | " +
                $"name={name} | " +
                $"savObj={(sav != null ? sav.gameObject.name + "#" + sav.gameObject.GetInstanceID() : "null")} | " +
                $"EngineOn={(sav != null ? sav._EngineOn : false)} | " +
                $"ThrottleInput={(sav != null ? sav.ThrottleInput : -1f):F2} | " +
                $"PlayerThrottle={(sav != null ? sav.PlayerThrottle : -1f):F2} | " +
                $"EngineOutput={(sav != null ? sav.EngineOutput : -1f):F2}"
            );
        }

        void HandleDeferredJustEngineOn()
        {
            //Debug.LogWarning($"[RWS_CANARY_JEO] frame={Time.frameCount} time={Time.time:F2} name={name}");

            if (justEngineOnPending)
            {
                /*Debug.LogWarning(
                    $"[AI JUSTENGINEON CHECK] | " +
                    $"frame={Time.frameCount} | " +
                    $"time={Time.time:F2} | " +
                    $"name={name} | " +
                    $"justEngineOnPending={justEngineOnPending} | " +
                    $"engineStartRequested={engineStartRequested} | " +
                    $"justEngineOnAt={justEngineOnAt:F2} | " +
                    $"dueIn={(justEngineOnAt - Time.time):F2} | " +
                    $"savNull={(sav == null)} | " +
                    $"EngineOn={(sav != null ? sav._EngineOn : false)} | " +
                    $"engineToggleNull={(engineToggle == null)}"
                );*/
            }

            if (!justEngineOnPending) return;
            if (Time.time < justEngineOnAt) return;

            Debug.LogWarning(
                $"[AI JUSTENGINEON FIRE] | " +
                $"frame={Time.frameCount} | " +
                $"time={Time.time:F2} | " +
                $"name={name} | " +
                $"savObj={(sav != null ? sav.gameObject.name + "#" + sav.gameObject.GetInstanceID() : "null")} | " +
                $"EngineOnBefore={(sav != null ? sav._EngineOn : false)} | " +
                $"engineStartRequested={engineStartRequested} | " +
                $"engineToggleNull={(engineToggle == null)}"
            );

            if (sav == null)
            {
                Debug.LogWarning(
                    $"[AI JUSTENGINEON ABORT] reason=sav_null | " +
                    $"frame={Time.frameCount} | time={Time.time:F2} | name={name}"
                );
                justEngineOnPending = false;
                engineStartRequested = false;
                return;
            }

            if (engineToggle == null)
            {
                Debug.LogWarning(
                    $"[AI JUSTENGINEON ABORT] reason=engine_toggle_null | " +
                    $"frame={Time.frameCount} | time={Time.time:F2} | name={name}"
                );
                justEngineOnPending = false;
                engineStartRequested = false;
                return;
            }

            if (sav._EngineOn)
            {
                Debug.LogWarning(
                    $"[AI JUSTENGINEON ABORT] reason=already_engine_on | " +
                    $"frame={Time.frameCount} | time={Time.time:F2} | name={name}"
                );
                justEngineOnPending = false;
                engineStartRequested = false;
                return;
            }

            if (debugSacc)
            {
                Debug.Log($"AI DEFERRED JustEngineOn SENT | {name}");
            }

            engineToggle.SendCustomEvent("JustEngineOn");
            justEngineOnPending = false;
            engineStartRequested = false;

            Debug.LogWarning(
                $"[AI JUSTENGINEON DONE] | " +
                $"frame={Time.frameCount} | " +
                $"time={Time.time:F2} | " +
                $"name={name} | " +
                $"justEngineOnPending={justEngineOnPending} | " +
                $"engineStartRequested={engineStartRequested} | " +
                $"EngineOnAfter={(sav != null ? sav._EngineOn : false)}"
            );

            StartCoroutine(LogEngineStateFramesCoroutine());
        }
        void FixedUpdate()
        {
            CacheReferences();

            if (!HasRequiredReferences())
                return;

            startupTimer += Time.fixedDeltaTime;

            if (Time.frameCount - _dbgLastAiHeartbeatFrame >= 120)
            {
                _dbgLastAiHeartbeatFrame = Time.frameCount;
                Debug.LogWarning(
                    $"[AI HEARTBEAT] " +
                    $"frame={Time.frameCount} | " +
                    $"time={Time.time:F2} | " +
                    $"name={name} | " +
                    $"state={brain.state} | " +
                    $"startupTimer={startupTimer:F2} | " +
                    $"saccReady={saccReady} | " +
                    $"savNull={(sav == null)} | " +
                    $"engineToggleNull={(engineToggle == null)} | " +
                    $"EngineOn={(sav != null ? sav._EngineOn : false)}"
                );
            }

            if (!UpdateSaccReady())
            {
                if (Time.frameCount - _dbgLastReadyBlockedFrame >= 60)
                {
                    _dbgLastReadyBlockedFrame = Time.frameCount;
                    Debug.LogWarning(
                        $"[AI READY BLOCKED] " +
                        $"frame={Time.frameCount} | " +
                        $"time={Time.time:F2} | " +
                        $"name={name} | " +
                        $"startupTimer={startupTimer:F2} | " +
                        $"savNull={(sav == null)} | " +
                        $"entityNull={(sav == null || sav.EntityControl == null)} | " +
                        $"animNull={(sav == null || sav.VehicleAnimator == null)} | " +
                        $"rbNull={(sav == null || sav.VehicleRigidbody == null)} | " +
                        $"savIsOwner={(sav != null ? sav.IsOwner : false)} | " +
                        $"entityIsOwner={(sav != null && sav.EntityControl != null ? sav.EntityControl.IsOwner : false)} | " +
                        $"engineToggleNull={(engineToggle == null)}"
                    );
                }
                return;
            }

            HandleDeferredJustEngineOn();
            EnsureEngineOn();

            if (Time.frameCount - _dbgLastLoopActiveFrame >= 60)
            {
                _dbgLastLoopActiveFrame = Time.frameCount;
                Debug.LogWarning(
                    $"[AI LOOP ACTIVE] " +
                    $"frame={Time.frameCount} | " +
                    $"time={Time.time:F2} | " +
                    $"name={name} | " +
                    $"state={brain.state} | " +
                    $"EngineOn={(sav != null ? sav._EngineOn : false)} | " +
                    $"engineStartRequested={engineStartRequested} | " +
                    $"justEngineOnPending={justEngineOnPending} | " +
                    $"justEngineOnAt={justEngineOnAt:F2} | " +
                    $"nextEngineRetryTime={nextEngineRetryTime:F2}"
                );
            }

            if (!IsEngineReadyForThrottle())
                return;

            ApplySaccThrottle();
            RunSteering();
        }

        void CacheReferences()
        {
            if (brain == null)
                brain = GetComponent<Brain>();

            if (rb == null)
                rb = GetComponentInParent<Rigidbody>();

            if (sav == null)
                sav = GetComponentInParent<SaccAirVehicle>();
                /*
                Debug.LogWarning(
                    $"[AI SAV PICKUP] | " +
                    $"frame={Time.frameCount} | " +
                    $"name={name} | " +
                    $"savId={sav.GetInstanceID()} | " +
                    $"savGoId={sav.gameObject.GetInstanceID()} | " +
                    $"savPath={GetPath(sav.transform)} | " +
                    $"entityId={(sav.EntityControl != null ? sav.EntityControl.GetInstanceID().ToString() : "null")} | " +
                    $"entityGoId={(sav.EntityControl != null ? sav.EntityControl.gameObject.GetInstanceID().ToString() : "null")} | " +
                    $"entityPath={(sav.EntityControl != null ? GetPath(sav.EntityControl.transform) : "null")}"
                );
                */
            if (engineToggle == null)
                engineToggle = GetComponentInParent<DFUNC_ToggleEngine>();

            if (engineToggle != null && engineToggle.EntityControl == null && sav != null && sav.EntityControl != null)
                engineToggle.EntityControl = sav.EntityControl;

            if (engineToggle != null && engineToggle.SAVControl == null && sav != null)
                engineToggle.SAVControl = sav;

            if (sav == null)
                sav = GetComponentInParent<SaccAirVehicle>();
        }

        bool HasRequiredReferences()
        {
            return brain != null
                && rb != null
                && sav != null;
        }

        bool UpdateSaccReady()
        {
            if (saccReady)
                return true;

            if (sav == null) return false;
            if (sav.EntityControl == null) return false;
            if (engineToggle == null) return false;
            if (engineToggle.SAVControl == null) return false;
            if (engineToggle.EntityControl == null) return false;
            if (sav.EntityControl.dead || sav.EntityControl.wrecked) return false;
            if (startupTimer < startupDelay) return false;

            saccReady = true;

            if (debugSacc)
            {
                Debug.Log($"AI SACC READY LIGHT | {name}");
            }

            return true;
        }

        void UpdateEngineEdgeTracking()
        {
            if (sav == null)
                return;

            bool engineOnNow = sav._EngineOn;

            if (engineOnNow && !engineWasOnLastFrame)
            {
                engineOnTime = Time.time;

                if (debugSacc)
                {
                    Debug.Log(
                        $"AI ENGINE ON RISING EDGE | " +
                        $"{name} | " +
                        $"time={engineOnTime:F2}"
                    );
                }
            }

            if (!engineOnNow && engineWasOnLastFrame)
            {
                engineStartRequested = false;

                if (debugSacc)
                {
                    Debug.Log(
                        $"AI ENGINE OFF EDGE | {name} | " +
                        $"savObj={(sav != null ? sav.gameObject.name + "#" + sav.gameObject.GetInstanceID() : "null")} | " +
                        $"entityObj={(sav != null && sav.EntityControl != null ? sav.EntityControl.gameObject.name + "#" + sav.EntityControl.gameObject.GetInstanceID() : "null")} | " +
                        $"time={Time.time:F2} | " +
                        $"nextRetryIn={Mathf.Max(0f, nextEngineRetryTime - Time.time):F2} | " +
                        $"saccReady={saccReady} | " +
                        $"sav.IsOwner={sav.IsOwner} | " +
                        $"entity.IsOwner={(sav != null && sav.EntityControl != null && sav.EntityControl.IsOwner)}"
                    );
                }
            }

            engineWasOnLastFrame = engineOnNow;
        }
        void EnsureEngineOn()
        {   
            /*
            Debug.Log(
                $"[AI ENGINE CANARY] EnsureEngineOn ENTER | " +
                $"frame={Time.frameCount} | " +
                $"time={Time.time:F2} | " +
                $"name={name} | " +
                $"debugSacc={debugSacc}"
            );
                 */
            if (debugSacc)
            {
                /*Debug.Log(
                    $"[AI ENGINE GATE] " +
                    $"frame={Time.frameCount} | " +
                    $"time={Time.time:F2} | " +
                    $"name={name} | " +
                    $"State={brain.state} | " +
                    $"saccReady={saccReady} | " +
                    $"savNull={(sav == null)} | " +
                    $"entityNull={(sav == null || sav.EntityControl == null)} | " +
                    $"dead={(sav != null && sav.EntityControl != null ? sav.EntityControl.dead : false)} | " +
                    $"wrecked={(sav != null && sav.EntityControl != null ? sav.EntityControl.wrecked : false)} | " +
                    $"sav.IsOwner={(sav != null ? sav.IsOwner : false)} | " +
                    $"entity.IsOwner={(sav != null && sav.EntityControl != null ? sav.EntityControl.IsOwner : false)} | " +
                    $"engineToggleNull={(engineToggle == null)} | " +
                    $"Fuel={(sav != null ? sav.Fuel : -1f):F2} | " +
                    $"EngineOn={(sav != null ? sav._EngineOn : false)} | " +
                    $"engineStartRequested={engineStartRequested} | " +
                    $"justEngineOnPending={justEngineOnPending} | " +
                    $"justEngineOnAt={justEngineOnAt:F2} | " +
                    $"nextEngineRetryTime={nextEngineRetryTime:F2} | " +
                    $"retryIn={(nextEngineRetryTime - Time.time):F2}"
                );*/
            }

            if (sav == null)
            {
                if (debugSacc)
                    Debug.LogWarning($"[AI ENGINE START BLOCKED] reason=sav_null | frame={Time.frameCount} | name={name}");
                return;
            }

            if (!saccReady)
            {
                if (debugSacc)
                    Debug.LogWarning($"[AI ENGINE START BLOCKED] reason=sacc_not_ready | frame={Time.frameCount} | name={name}");
                return;
            }

            if (sav.EntityControl == null)
            {
                if (debugSacc)
                    Debug.LogWarning($"[AI ENGINE START BLOCKED] reason=entity_null | frame={Time.frameCount} | name={name}");
                return;
            }

            if (sav.EntityControl.dead)
            {
                if (debugSacc)
                    Debug.LogWarning($"[AI ENGINE START BLOCKED] reason=dead | frame={Time.frameCount} | name={name}");
                return;
            }

            if (sav.EntityControl.wrecked)
            {
                if (debugSacc)
                    Debug.LogWarning($"[AI ENGINE START BLOCKED] reason=wrecked | frame={Time.frameCount} | name={name}");
                return;
            }

            if (sav.Fuel <= 0f)
            {
                if (debugSacc)
                    Debug.LogWarning(
                        $"[AI ENGINE START BLOCKED] reason=no_fuel | " +
                        $"frame={Time.frameCount} | name={name} | Fuel={sav.Fuel:F2}"
                    );
                return;
            }

            sav.Piloting = true;

            if (sav._EngineOn)
            {
                if (debugSacc)
                    Debug.LogWarning(
                        $"[AI ENGINE START BLOCKED] reason=already_engine_on | " +
                        $"frame={Time.frameCount} | name={name} | EngineOn={sav._EngineOn}"
                    );
                return;
            }

            if (engineStartRequested)
            {
                if (debugSacc)
                    Debug.LogWarning(
                        $"[AI ENGINE START BLOCKED] reason=engine_start_already_requested | " +
                        $"frame={Time.frameCount} | name={name} | " +
                        $"justEngineOnPending={justEngineOnPending} | justEngineOnAt={justEngineOnAt:F2}"
                    );
                return;
            }

            if (Time.time < nextEngineRetryTime)
            {
                if (debugSacc)
                    Debug.LogWarning(
                        $"[AI ENGINE START BLOCKED] reason=retry_cooldown | " +
                        $"frame={Time.frameCount} | name={name} | " +
                        $"time={Time.time:F2} | nextEngineRetryTime={nextEngineRetryTime:F2} | " +
                        $"retryIn={(nextEngineRetryTime - Time.time):F2}"
                    );
                return;
            }

            if (debugSacc)
                Debug.Log($"AI ENGINE START TRY | expected DFUNC startup wait={dfuncStartupWait:F2}s");

            nextEngineRetryTime = Time.time + engineRetryInterval;

            if (debugSacc)
            {
                Debug.Log(
                    $"AI ENGINE START TRY | " +
                    $"frame={Time.frameCount} | " +
                    $"savObj={(sav != null ? sav.gameObject.name + "#" + sav.gameObject.GetInstanceID() : "null")} | " +
                    $"entityObj={(sav != null && sav.EntityControl != null ? sav.EntityControl.gameObject.name + "#" + sav.EntityControl.gameObject.GetInstanceID() : "null")} | " +
                    $"State={brain.state} | " +
                    $"sav.IsOwner={sav.IsOwner} | " +
                    $"entity.IsOwner={sav.EntityControl.IsOwner} | " +
                    $"engineToggleNull={(engineToggle == null)} | " +
                    $"Fuel={sav.Fuel:F2} | " +
                    $"EngineOn={sav._EngineOn} | " +
                    $"engineStartRequested={engineStartRequested} | " +
                    $"justEngineOnPending={justEngineOnPending} | " +
                    $"justEngineOnAt={justEngineOnAt:F2} | " +
                    $"nextEngineRetryTime={nextEngineRetryTime:F2}"
                );
            }

            if (engineToggle != null)
            {
                if (debugSacc)
                {
                    Debug.Log(
                        $"[AI ENGINE START PATH] mode=DFUNC | " +
                        $"frame={Time.frameCount} | name={name} | " +
                        $"engineToggleObj={engineToggle.gameObject.name}#{engineToggle.gameObject.GetInstanceID()}"
                    );
                }

                engineToggle.SendCustomEvent("KeyboardInput");
                engineStartRequested = true;
                justEngineOnPending = true;
                justEngineOnAt = Time.time + dfuncStartupWait;

                Debug.LogWarning(
                    $"[AI ENGINE REQUEST ARMED] mode=DFUNC | " +
                    $"frame={Time.frameCount} | time={Time.time:F2} | name={name} | " +
                    $"engineStartRequested={engineStartRequested} | " +
                    $"justEngineOnPending={justEngineOnPending} | " +
                    $"justEngineOnAt={justEngineOnAt:F2} | " +
                    $"dueIn={(justEngineOnAt - Time.time):F2}"
                );

                if (debugSacc)
                {
                    Debug.Log(
                        $"AI DFUNC KEYBOARDINPUT SENT | " +
                        $"frame={Time.frameCount} | name={name} | " +
                        $"justEngineOnPending={justEngineOnPending} | justEngineOnAt={justEngineOnAt:F2}"
                    );
                }
            }
            else
            {
                if (debugSacc)
                {
                    Debug.Log(
                        $"[AI ENGINE START PATH] mode=FALLBACK_SETENGINEON | " +
                        $"frame={Time.frameCount} | name={name}"
                    );
                }

                sav.SendCustomEvent("SetEngineOn");

                engineStartRequested = true;
                justEngineOnPending = true;
                justEngineOnAt = Time.time;

                Debug.LogWarning(
                    $"[AI ENGINE REQUEST ARMED] mode=FALLBACK_SETENGINEON | " +
                    $"frame={Time.frameCount} | time={Time.time:F2} | name={name} | " +
                    $"engineStartRequested={engineStartRequested} | " +
                    $"justEngineOnPending={justEngineOnPending} | " +
                    $"justEngineOnAt={justEngineOnAt:F2} | " +
                    $"dueIn={(justEngineOnAt - Time.time):F2}"
                );

                if (debugSacc)
                    Debug.Log($"AI FALLBACK SetEngineOn SENT | frame={Time.frameCount} | {name}");
            }
        }

        bool IsEngineReadyForThrottle()
        {
            if (sav == null) return false;
            if (!sav._EngineOn) return false;
            if (engineOnTime < 0f) return false;

            return Time.time >= engineOnTime + dfuncStartupWait;
        }


        void ApplySaccThrottle()
        {
            if (sav == null) return;
            if (brain == null) return;
            if (sav.EntityControl == null) return;
            if (sav.EntityControl.dead || sav.EntityControl.wrecked) return;
            if (!sav._EngineOn) return;

            float throttle = GetThrottleForState(brain.state);

            sav._DisablePhysicsAndInputs = false;
            sav._ThrottleOverridden = true;
            sav.ThrottleOverride = throttle;
            sav.ThrottleInput = throttle;
            sav.PlayerThrottle = throttle;

            if (debugSacc && Time.frameCount % 10 == 0)
            {
                Debug.Log(
                    $"AI THRUST LIVE | " +
                    $"State={brain.state} | " +
                    $"EngineOn={sav._EngineOn} | " +
                    $"ThrottleOverride={sav.ThrottleOverride:F2} | " +
                    $"ThrottleInput={sav.ThrottleInput:F2} | " +
                    $"PlayerThrottle={sav.PlayerThrottle:F2} | " +
                    $"EngineOutput={sav.EngineOutput:F2} | " +
                    $"DisablePhysicsAndInputs={sav._DisablePhysicsAndInputs} | " +
                    $"Fuel={sav.Fuel:F2}"
                );
            }
        }

        float GetThrottleForState(AIState state)
        {
            switch (state)
            {
                case AIState.Taxi:
                    return 0.25f;

                case AIState.Takeoff:
                    return 1.0f;

                case AIState.Landing:
                case AIState.RTB:
                    return 0.35f;

                default:
                    return 0.7f;
            }
        }

        void RunSteering()
        {
            Vector3 targetPos = brain.desiredPosition;
            Vector3 toTarget = targetPos - transform.position;

            if (toTarget.sqrMagnitude < 0.01f)
                return;

            Vector3 steer = toTarget.normalized;
            Quaternion rot = Quaternion.LookRotation(steer);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                Time.fixedDeltaTime * turnSpeed
            );
        }
    }
}