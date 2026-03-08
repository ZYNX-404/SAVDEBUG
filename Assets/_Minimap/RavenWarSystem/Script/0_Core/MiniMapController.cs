using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    public LineRenderer line;
    public float scale = 0.02f;

    Rigidbody rb;
    public Transform combatFlowArrow;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        Tick();
        
        Vector3 flow = MiniMapManager.Instance.GetCombatFlow();
        if (flow.magnitude > 10f)
        {
            combatFlowArrow.rotation =
                Quaternion.LookRotation(flow);
        }
    }
    public void Tick()
    {
        if (rb == null || line == null) return;

        Vector3 start = transform.position;
        Vector3 end = start + rb.velocity * scale;

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}