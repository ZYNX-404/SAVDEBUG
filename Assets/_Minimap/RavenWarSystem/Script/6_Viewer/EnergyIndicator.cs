using UnityEngine;

public class EnergyIndicator : MiniMapIndicator
{
    public Renderer ringRenderer;

    public float speedWeight = 1f;
    public float altitudeWeight = 0.5f;

    public float maxSpeed = 300f;
    public float maxAltitude = 5000f;

    MaterialPropertyBlock mpb;   // ← これを追加

    protected override void Awake()
    {
        base.Awake();
        mpb = new MaterialPropertyBlock();
    }

    protected override void UpdateIndicator()
    {
        if (rb == null) return;

        float speed = rb.velocity.magnitude;
        float altitude = rb.position.y;

        float speedEnergy = Mathf.Clamp01(speed / maxSpeed);
        float altitudeEnergy = Mathf.Clamp01(altitude / maxAltitude);

        float energy =
            speedEnergy * speedWeight +
            altitudeEnergy * altitudeWeight;

        energy /= (speedWeight + altitudeWeight);

        UpdateColor(energy);
    }

    void UpdateColor(float energy)
    {
        if (ringRenderer == null) return;

        Color c;

        if (energy > 0.7f)
            c = Color.green;
        else if (energy > 0.4f)
            c = Color.yellow;
        else
            c = Color.red;

        mpb.SetColor("_Color", c);
        ringRenderer.SetPropertyBlock(mpb);
    }
}