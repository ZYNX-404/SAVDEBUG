using UnityEngine;

public class MiniMapDogfightCamera : MonoBehaviour
{
    public Transform cameraPivot;

    public float spiralRadius = 0.25f;
    public float spiralHeight = 0.15f;
    public float spiralSpeed = 30f;

    public float followSpeed = 3f;
    public float dogfightDistance = 0.08f;

    Transform targetA;
    Transform targetB;

    float angle;
    float searchTimer;

    public void SetTarget(Transform target)
    {
        targetA = target;
        targetB = target;
    }

    public void SetTargets(Transform a, Transform b)
    {
        targetA = a;
        targetB = b;
    }

    void Update()
    {
        if (cameraPivot == null)
            return;

        if (!ValidTargets())
        {
            SearchDogfight();
            return;
        }

        Vector3 center = (targetA.position + targetB.position) * 0.5f;

        angle += Time.deltaTime * spiralSpeed;

        Vector3 offset =
            new Vector3(
                Mathf.Cos(angle),
                0f,
                Mathf.Sin(angle)
            ) * spiralRadius;

        Vector3 desired =
            center +
            offset +
            Vector3.up * spiralHeight;

        cameraPivot.position =
            Vector3.Lerp(
                cameraPivot.position,
                desired,
                Time.deltaTime * followSpeed
            );

        Quaternion rot =
            Quaternion.LookRotation(center - cameraPivot.position);

        cameraPivot.rotation =
            Quaternion.Slerp(
                cameraPivot.rotation,
                rot,
                Time.deltaTime * followSpeed
            );

        float d = Vector3.Distance(targetA.position, targetB.position);
        if (d > dogfightDistance * 1.5f)
        {
            targetA = null;
            targetB = null;
        }
    }

    bool ValidTargets()
    {
        if (targetA == null || targetB == null)
            return false;

        if (!targetA.gameObject.activeInHierarchy ||
            !targetB.gameObject.activeInHierarchy)
            return false;

        return true;
    }

    void SearchDogfight()
    {
        if (MiniMapDataBus.Instance == null || MiniMapDataBus.Instance.aircraft == null)
            return;

        searchTimer += Time.deltaTime;

        if (searchTimer < 0.5f)
            return;

        searchTimer = 0f;

        var aircraft = MiniMapDataBus.Instance.aircraft;

        float best = dogfightDistance;

        Transform a = null;
        Transform b = null;

        for (int i = 0; i < aircraft.Count; i++)
        {
            if (aircraft[i] == null) continue;

            for (int j = i + 1; j < aircraft.Count; j++)
            {
                if (aircraft[j] == null) continue;

                float d = Vector3.Distance(
                    aircraft[i].transform.position,
                    aircraft[j].transform.position
                );

                if (d < best)
                {
                    best = d;
                    a = aircraft[i].transform;
                    b = aircraft[j].transform;
                }
            }
        }

        targetA = a;
        targetB = b;
    }
}