
using UnityEngine;

public class AIBreakTurn : MonoBehaviour
{
    public Rigidbody rb;

    public float missileDetectDistance = 120f;

    public float breakTurnTime = 2.5f;

    public float breakTurnStrength = 1.5f;

    float breakTimer;

    bool breaking;

    void Update()
    {
        DetectMissile();

        if (breaking)
        {
            breakTimer -= Time.deltaTime;

            if (breakTimer <= 0)
                breaking = false;
        }
    }

    void DetectMissile()
    {
        var missiles = MiniMapDataBus.Instance.GetMissiles();

        foreach (var m in missiles)
        {
            if (m == null) continue;

            float d =
                Vector3.Distance(
                    transform.position,
                    m.position
                );

            if (d < missileDetectDistance)
            {
                StartBreakTurn();
                return;
            }
        }
    }

    void StartBreakTurn()
    {
        breaking = true;
        breakTimer = breakTurnTime;
    }

    public Vector3 ModifyDirection(Vector3 currentDir)
    {
        if (!breaking)
            return currentDir;

        Vector3 side =
            Vector3.Cross(Vector3.up, currentDir);

        return (currentDir + side * breakTurnStrength).normalized;
    }
}