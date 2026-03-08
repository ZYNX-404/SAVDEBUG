using UnityEngine;

public class MiniMapVectorIndicator : MiniMapIndicator
{
    public LineRenderer line;
    public float scale = 0.02f;

    protected override void UpdateIndicator()
    {
        Vector3 vel = rb.velocity;

        Vector3 start = transform.localPosition;
        Vector3 end = start + vel * scale;

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}