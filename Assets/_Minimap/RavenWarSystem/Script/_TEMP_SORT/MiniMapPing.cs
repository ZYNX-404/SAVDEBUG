using UnityEngine;

public class MiniMapPing : MonoBehaviour
{
    public float lifeTime = 5f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > lifeTime)
            Destroy(gameObject);
    }
}