using UnityEngine;

public class MiniMapPingSystem : MonoBehaviour
{
    public GameObject pingPrefab;

    public void Ping(Vector3 worldPos)
    {
        if (MiniMapManager.Instance == null)
            return;

        Vector3 center = MiniMapManager.Instance.GetCombatCenter();

        Vector3 relative = worldPos - center;

        Instantiate(
            pingPrefab,
            relative,
            Quaternion.identity
        );
    }
}