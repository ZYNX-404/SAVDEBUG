using UnityEngine;

public class MiniMapAltitudeIndicator : MiniMapIndicator
{
    public Transform altitudeBar;
    public float scale = 0.001f;
    public float seaLevel = 0f;

    public GameObject altitudeRing;
    protected override void UpdateIndicator()
    {
        /*
        if (rb == null) return;

        float h = (rb.position.y - seaLevel) * scale;

        h = Mathf.Clamp(h, -0.5f, 0.5f);
        
        altitudeBar.localScale =
            new Vector3(1, Mathf.Abs(h), 1);

        altitudeBar.localPosition =
            new Vector3(0, h * 0.5f, 0);
        */ 
    }

}