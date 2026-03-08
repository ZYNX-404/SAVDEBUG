using UnityEngine;

public class SimpleMissile : MonoBehaviour
{
    public Rigidbody rb;

    public float speed = 160f;

    public float turnSpeed = 3f;

    Transform target;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            rb.velocity = transform.forward * speed;
            return;
        }

        Vector3 dir =
            (target.position - transform.position).normalized;

        Quaternion rot =
            Quaternion.LookRotation(dir);
        if(Vector3.Dot(transform.forward, dir) < 0)
            return;
        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rot,
                turnSpeed * Time.fixedDeltaTime
            );

        rb.velocity = transform.forward * speed;
    }
}