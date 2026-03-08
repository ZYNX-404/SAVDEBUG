using UnityEngine;

public class MiniMapCinematicSystem : MonoBehaviour
{
    public Transform cameraPivot;

    public float cinematicTime = 2f;
    public float moveSpeed = 4f;

    float timer;
    bool playing;

    Vector3 targetPos;

    public void PlayCinematic(Vector3 position)
    {
        targetPos = position;
        timer = cinematicTime;
        playing = true;
    }

    void Update()
    {
        if (!playing) return;

        timer -= Time.deltaTime;

        Vector3 desired =
            targetPos +
            Vector3.up * 0.2f +
            Vector3.back * 0.3f;

        cameraPivot.position =
            Vector3.Lerp(
                cameraPivot.position,
                desired,
                Time.deltaTime * moveSpeed
            );

        Quaternion rot =
            Quaternion.LookRotation(
                targetPos - cameraPivot.position
            );

        cameraPivot.rotation =
            Quaternion.Slerp(
                cameraPivot.rotation,
                rot,
                Time.deltaTime * moveSpeed
            );

        if (timer <= 0f)
            playing = false;
    }

    public bool IsPlaying()
    {
        return playing;
    }
}