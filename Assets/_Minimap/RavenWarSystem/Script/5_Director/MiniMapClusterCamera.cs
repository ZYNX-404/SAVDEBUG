using UnityEngine;

public class MiniMapClusterCamera : MonoBehaviour
{
    public Transform cameraTarget;

    public float baseDistance = 6f;
    public float sizeScale = 1.2f;

    public float height = 4f;

    public float velocityLookahead = 1.5f;

    public float moveSpeed = 3f;

    CombatCluster currentCluster;

    public void SetCluster(CombatCluster cluster)
    {
        currentCluster = cluster;
    }

    void Update()
    {
        if (currentCluster == null) return;

        Vector3 center = currentCluster.center;

        Vector3 velocity = currentCluster.velocity;

        Vector3 lookAhead =
            velocity * velocityLookahead;

        Vector3 focus =
            center + lookAhead;

        float distance =
            baseDistance +
            currentCluster.size * sizeScale;

        Vector3 desiredPos =
            focus +
            Vector3.back * distance +
            Vector3.up * height;

        cameraTarget.position =
            Vector3.Lerp(
                cameraTarget.position,
                desiredPos,
                Time.deltaTime * moveSpeed
            );
    }
}