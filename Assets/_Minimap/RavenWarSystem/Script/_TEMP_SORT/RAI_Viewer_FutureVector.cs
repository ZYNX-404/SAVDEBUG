using UnityEngine;

public class MiniMapFutureVector : MiniMapIndicator
{   
    public LineRenderer line;
    public float predictTime = 2f;
    public float scale = 0.02f;
    void Start()
    {
        line.positionCount = 2;
        if (rb.velocity.sqrMagnitude < 0.01f)
        {
            line.enabled = false;
            return;
        }
    line.enabled = true;
    }
    protected override void UpdateIndicator()
    {
        Vector3 start = transform.position;
        Vector3 future = start + rb.velocity * predictTime * scale;

        line.SetPosition(0, start);
        line.SetPosition(1, future);
    }
}