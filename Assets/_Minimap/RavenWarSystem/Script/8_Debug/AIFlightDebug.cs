using UnityEngine;
using RWS.AI;

public class AIFlightDebug : MonoBehaviour
{
    public Brain brain;
    public float height = 60f;

    void OnDrawGizmos()
    {
        if (brain == null) return;
        if (brain.target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            brain.target.transform.position
        );
    }

    void OnGUI()
    {
        if (brain == null) return;
        if (Camera.main == null) return;
        if (brain.rb == null) return;

        Vector3 world = transform.position + Vector3.up * height;
        Vector3 screen = Camera.main.WorldToScreenPoint(world);

        if (screen.z < 0) return;

        float speed = brain.rb.velocity.magnitude;

        float dist = 0f;
        if (brain.target != null)
        {
            dist = Vector3.Distance(
                brain.transform.position,
                brain.target.WorldPosition
            );
        }

        screen.y = Screen.height - screen.y;

        string text =
            brain.state + "\n" +
            brain.role + "\n" +
            "Flight: " + brain.flightId + "\n" +
            "Speed: " + speed.ToString("F1") + "\n" +
            "Distance: " + dist.ToString("F1") + "\n" +
            "MissileThreat: " + brain.missileThreat + "\n" +
            "Target: " + (brain.target != null ? brain.target.name : "none");

        GUI.Label(new Rect(screen.x, screen.y, 220, 120), text);
    }
}