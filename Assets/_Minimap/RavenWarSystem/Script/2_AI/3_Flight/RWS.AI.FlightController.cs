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

        void Update()
        {
            startupTimer += Time.deltaTime;

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
    
        void HandleDeferredJustEngineOn()
        {
            if (!justEngineOnPending) return;
            if (engineToggle == null) return;
            if (Time.time < justEngineOnAt) return;
            if (sav != null && sav._EngineOn)
            {
                justEngineOnPending = false;
                return;
            }

            if (debugSacc)
                Debug.Log($"AI DEFERRED JustEngineOn SENT | {name}");

            engineToggle.SendCustomEvent("JustEngineOn");
            justEngineOnPending = false;
        }
        void FixedUpdate()
        {
            CacheReferences();

            if (!HasRequiredReferences())
                return;

            startupTimer += Time.fixedDeltaTime;

            if (!UpdateSaccReady())
                return;

            UpdateEngineEdgeTracking();
            HandleDeferredJustEngineOn();
            EnsureEngineOn();
            
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

            if (engineToggle == null)
                engineToggle = GetComponentInParent<DFUNC_ToggleEngine>();

            if (engineToggle != null && engineToggle.EntityControl == null && sav != null && sav.EntityControl != null)
                engineToggle.EntityControl = sav.EntityControl;

            if (engineToggle != null && engineToggle.SAVControl == null && sav != null)
                engineToggle.SAVControl = sav;
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

            if (debugSacc && Time.frameCount % 30 == 0)
            {
                Debug.Log(
                    $"AI READY TRACE| " +
                    $"savId={(sav != null ? sav.GetInstanceID().ToString() : "null")} | " +
                    $"animId={(sav != null && sav.VehicleAnimator != null ? sav.VehicleAnimator.GetInstanceID().ToString() : "null")} | " +
                    $"rbId={(sav != null && sav.VehicleRigidbody != null ? sav.VehicleRigidbody.GetInstanceID().ToString() : "null")} | " +
                    $"savObj={(sav != null ? sav.gameObject.name : "null")} | " +
                    $"animObj={(sav != null && sav.VehicleAnimator != null ? sav.VehicleAnimator.gameObject.name : "null")} | " +
                    $"rbObj={(sav != null && sav.VehicleRigidbody != null ? sav.VehicleRigidbody.gameObject.name : "null")} | " +
                    $"sav.IsOwner={(sav != null ? sav.IsOwner.ToString() : "null")} | " +
                    $"entity.IsOwner={(sav != null && sav.EntityControl != null ? sav.EntityControl.IsOwner.ToString() : "null")} | " +
                    $"startupTimer={startupTimer:F2}"
                );
            }
            if (sav == null) return false;

            if (sav.EntityControl == null) return false;

            if (sav.VehicleAnimator == null) return false;

            if (sav.VehicleRigidbody == null) return false;

            if (!sav.IsOwner) return false;

            if (!sav.EntityControl.IsOwner) return false;

            if (startupTimer < startupDelay)
                return false;

            if (engineToggle == null)
                return false;

            if (engineToggle.SAVControl == null)
                return false;

            if (engineToggle.EntityControl == null)
                return false;

            if (sav.EntityControl.dead || sav.EntityControl.wrecked)
                return false;
            if (debugSacc && Time.frameCount % 30 == 0)
            {
                Debug.Log(
                    $"AI READY IDS | " +
                    $"savId={(sav != null ? sav.GetInstanceID().ToString() : "null")} | " +
                    $"animId={(sav != null && sav.VehicleAnimator != null ? sav.VehicleAnimator.GetInstanceID().ToString() : "null")} | " +
                    $"rbId={(sav != null && sav.VehicleRigidbody != null ? sav.VehicleRigidbody.GetInstanceID().ToString() : "null")} | " +
                    $"savObj={(sav != null ? sav.gameObject.name : "null")} | " +
                    $"animObj={(sav != null && sav.VehicleAnimator != null ? sav.VehicleAnimator.gameObject.name : "null")} | " +
                    $"rbObj={(sav != null && sav.VehicleRigidbody != null ? sav.VehicleRigidbody.gameObject.name : "null")} | " +
                    $"sav.IsOwner={(sav != null ? sav.IsOwner.ToString() : "null")} | " +
                    $"entity.IsOwner={(sav != null && sav.EntityControl != null ? sav.EntityControl.IsOwner.ToString() : "null")} | " +
                    $"startupTimer={startupTimer:F2}"
                );
            }
            saccReady = true;

            if (debugSacc)
            {
                Debug.Log(
                    $"AI SACC READY | " +
                    $"{name} | " +
                    $"sav.IsOwner={sav.IsOwner} | " +
                    $"entity.IsOwner={sav.EntityControl.IsOwner} | " +
                    $"Fuel={sav.Fuel:F2}"
                );
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
            if (sav == null) return;
            if (!saccReady) return;
            if (sav.EntityControl == null) return;
            if (sav.EntityControl.dead || sav.EntityControl.wrecked) return;
            if (sav.Fuel <= 0f) return;

            sav.Piloting = true;

            if (sav._EngineOn) return;
            if (engineStartRequested) return;
            if (Time.time < nextEngineRetryTime) return;
            if (debugSacc)
                Debug.Log($"AI ENGINE START TRY | expected DFUNC startup wait={dfuncStartupWait:F2}s");
            nextEngineRetryTime = Time.time + engineRetryInterval;

            if (debugSacc)
            {
                Debug.Log(
                    $"AI ENGINE START TRY | " +
                    $"savObj={(sav != null ? sav.gameObject.name + "#" + sav.gameObject.GetInstanceID() : "null")} | " +
                    $"entityObj={(sav != null && sav.EntityControl != null ? sav.EntityControl.gameObject.name + "#" + sav.EntityControl.gameObject.GetInstanceID() : "null")} | " +
                    $"State={brain.state} | " +
                    $"sav.IsOwner={sav.IsOwner} | " +
                    $"entity.IsOwner={sav.EntityControl.IsOwner} | " +
                    $"engineToggleNull={(engineToggle == null)} | " +
                    $"Fuel={sav.Fuel:F2} | " +
                    $"EngineOn={sav._EngineOn}"
                );
            }

            if (engineToggle != null)
            {
                engineToggle.SendCustomEvent("KeyboardInput");
                engineStartRequested = true;
                justEngineOnPending = true;
                justEngineOnAt = Time.time + dfuncStartupWait;


                if (debugSacc)
                    Debug.Log($"AI DFUNC KEYBOARDINPUT SENT | {name}");
            }
            else
            {
                sav.SendCustomEvent("SetEngineOn");

                if (debugSacc)
                    Debug.Log($"AI FALLBACK SetEngineOn SENT | {name}");
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