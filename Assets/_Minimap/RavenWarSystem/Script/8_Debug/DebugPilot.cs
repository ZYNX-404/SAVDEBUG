
using UnityEngine;
using System.Collections.Generic;

public class DebugPilot : MonoBehaviour
{
    public Transform target;

    public float turnSpeed = 30f;

    void Update()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;

        Quaternion rot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            rot,
            turnSpeed * Time.deltaTime
        );
    }
}