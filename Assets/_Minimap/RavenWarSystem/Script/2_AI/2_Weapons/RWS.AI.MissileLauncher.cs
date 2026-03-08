using UnityEngine;

public class AIMissileLauncher : MonoBehaviour
{
    public GameObject missilePrefab;

    public Transform launchPoint;

    public float launchRange = 600f;

    public float cooldown = 4f;

    float nextFire;

    public void TryFire(Transform target)
    {
        if (target == null) return;

        if (Time.time < nextFire) return;

        float d = Vector3.Distance(
            transform.position,
            target.position
        );

        if (d > launchRange)
            return;

        // 発射角度チェック
        Vector3 toTarget = target.position - transform.position;
        float aspect = Vector3.Dot(transform.forward, toTarget.normalized);

        if (aspect < 0.3f)
            return;

        Fire(target);
    }

    void Fire(Transform target)
    {
        nextFire = Time.time + cooldown;

        GameObject m =
            Instantiate(
                missilePrefab,
                launchPoint.position,
                launchPoint.rotation
            );

        var missile =
            m.GetComponent<SimpleMissile>();

        missile.SetTarget(target);

        if (MiniMapDataBus.Instance != null)
        {
            MiniMapDataBus.Instance.RegisterMissile(
                missile.GetComponent<Rigidbody>()
            );
        }
    }
}