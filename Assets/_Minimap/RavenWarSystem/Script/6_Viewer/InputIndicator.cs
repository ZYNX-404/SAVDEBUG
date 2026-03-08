using UnityEngine;
using SaccFlightAndVehicles;

public class InputIndicator : MiniMapIndicator
{
    public SaccAirVehicle sav;

    public Transform pitchIndicator;
    public Transform yawIndicator;

    public float maxPitchAngle = 25f;
    public float maxYawAngle = 25f;

    protected override void UpdateIndicator()
    {
        if (sav == null) return;

        float pitchInput = 0f;
        float yawInput = 0f;

        object rawPitch = sav.GetProgramVariable("pitch");
        if (rawPitch is float p)
            pitchInput = p;

        object rawYaw = sav.GetProgramVariable("yaw");
        if (rawYaw is float y)
            yawInput = y;

        // 正規化
        float pitchNorm = Mathf.Clamp(pitchInput / 2.5f, -1f, 1f);
        float yawNorm = Mathf.Clamp(yawInput / 2.5f, -1f, 1f);

        // Pitch（上下）
        if (pitchIndicator != null)
        {
            pitchIndicator.localRotation =
                Quaternion.Euler(pitchNorm * maxPitchAngle, 0f, 0f);
        }

        // Yaw（左右）
        if (yawIndicator != null)
        {
            yawIndicator.localRotation =
                Quaternion.Euler(0f, yawNorm * maxYawAngle, 0f);
        }
    }
}