using UnityEngine;

public class AIAircraftSpawner : MonoBehaviour
{
    public GameObject aircraftPrefab;

    public int count = 6;

    public float radius = 500f;

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos =
                transform.position +
                Random.insideUnitSphere * radius;

            pos.y = transform.position.y;

            Instantiate(aircraftPrefab, pos, Quaternion.identity);
        }
    }
}