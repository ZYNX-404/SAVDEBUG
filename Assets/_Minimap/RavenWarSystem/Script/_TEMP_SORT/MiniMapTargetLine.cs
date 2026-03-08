using UnityEngine;

public class MiniMapTargetLine : MiniMapIndicator
{
    public Transform target;
    public LineRenderer line;

    protected override void UpdateIndicator()
    {
        if (target == null) return;

        line.SetPosition(0, transform.position);
        line.SetPosition(1, target.position);
    }
}