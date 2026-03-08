using UnityEngine;
using System.Collections.Generic;
public class AircraftState
{
    public int id;

    public Transform transform;

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 forward;

    public float speed;

    public bool alive = true;
}
