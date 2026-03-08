using UnityEngine;


public class MiniMapMissileIntercept : MonoBehaviour
{
    public Rigidbody missile;
    public Rigidbody target;

    public Transform interceptMarker;

    void Update()
    {
        if (missile == null || target == null) return;

        Vector3 relativePos = target.position - missile.position;
        Vector3 relativeVel = target.velocity - missile.velocity;

        float relSpeed = relativeVel.magnitude;

        if (relSpeed < 0.1f) return;

        float t = relativePos.magnitude / relSpeed;

        Vector3 futureTarget = target.position + target.velocity * t;

        interceptMarker.position = futureTarget;
    }
}