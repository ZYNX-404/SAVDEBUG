using UnityEngine;

public class MiniMapKillCam : MonoBehaviour
{
    public Transform cameraRoot;

    public float focusTime = 3f;
    public float moveSpeed = 3f;

    Vector3 targetPos;

    bool focusing;
    float timer;

    void Update()
    {
        if (!focusing) return;

        cameraRoot.position = Vector3.Lerp(
            cameraRoot.position,
            targetPos,
            Time.deltaTime * moveSpeed
        );

        timer += Time.deltaTime;

        if (timer > focusTime)
        {
            focusing = false;
            timer = 0f;
        }
    }

    public void TriggerKill(Vector3 pos)
    {
        targetPos = pos;

        focusing = true;
        timer = 0f;
    }
}