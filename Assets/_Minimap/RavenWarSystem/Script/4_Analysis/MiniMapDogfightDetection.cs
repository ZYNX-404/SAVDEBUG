using UnityEngine;
using System.Collections.Generic;

public class MiniMapDogfightDetection : MonoBehaviour
{
    [Header("Dogfight")]
    public float dogfightDistance = 1500f;

    [Header("Heading")]
    public float headingThreshold = -0.3f;

    [Header("Closing Speed")]
    public float closingSpeedThreshold = -0.02f;

    [Header("Aspect")]
    public float aspectThreshold = 0.5f;

    [Header("Energy")]
    public float altitudeWeight = 0.002f;

    public List<DogfightPair> pairs = new List<DogfightPair>();
    public Rigidbody rb;
    float timer = 0f;
    public float interval = 0.25f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
 
    void Update()
    {
        timer += Time.deltaTime;

        if (timer < interval)
            return;

        timer = 0f;

        RunDetection();
    }
    void RunDetection()
    {
        pairs.Clear();

        var aircraft = MiniMapDataBus.Instance.aircraft;
        if (aircraft == null) return;

        for (int i = 0; i < aircraft.Count; i++)
        {
            for (int j = i + 1; j < aircraft.Count; j++)
            {
                var a = aircraft[i];
                var b = aircraft[j];

                if (a == null || b == null) continue;

                Vector3 posA = a.WorldPosition;
                Vector3 posB = b.WorldPosition;

                Vector3 dirAB = (posB - posA).normalized;
                Vector3 dirBA = -dirAB;

                float dist = Vector3.Distance(posA, posB);

                float headingA =
                    Vector3.Dot(a.transform.forward, dirAB);

                float headingB =
                    Vector3.Dot(b.transform.forward, dirBA);

                var rbA = a.rb;
                var rbB = b.rb;

                if (rbA == null || rbB == null) continue;


                Vector3 relativeVelocity =
                    rbB.velocity - rbA.velocity;

                float closingSpeed =
                    Vector3.Dot(relativeVelocity, dirAB);

                float aspectA =
                    Vector3.Dot(b.transform.forward, -dirAB);

                float aspectB =
                    Vector3.Dot(a.transform.forward, -dirBA);

                bool aAdv = aspectA > aspectThreshold;
                bool bAdv = aspectB > aspectThreshold;

                float speedA = rbA.velocity.magnitude;
                float speedB = rbB.velocity.magnitude;

                float altA = posA.y;
                float altB = posB.y;

                float energyA = speedA + altA * altitudeWeight;
                float energyB = speedB + altB * altitudeWeight;

                bool aEnergyAdv = energyA > energyB;
                bool bEnergyAdv = energyB > energyA;

                if (dist < dogfightDistance &&
                    headingA > headingThreshold &&
                    headingB > headingThreshold &&
                    closingSpeed < closingSpeedThreshold)
                {
                    DogfightPair p = new DogfightPair();

                    p.a = a;
                    p.b = b;

                    p.distance = dist;
                    p.closingSpeed = closingSpeed;

                    p.aspectA = aspectA;
                    p.aspectB = aspectB;

                    p.aHasAdvantage = aAdv;
                    p.bHasAdvantage = bAdv;

                    p.energyA = energyA;
                    p.energyB = energyB;

                    p.aEnergyAdvantage = aEnergyAdv;
                    p.bEnergyAdvantage = bEnergyAdv;

                    pairs.Add(p);
                }
            }
        }
    }
}