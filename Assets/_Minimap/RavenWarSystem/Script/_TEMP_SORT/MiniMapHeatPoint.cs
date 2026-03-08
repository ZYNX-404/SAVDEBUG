using UnityEngine;

public class MiniMapHeatPoint : MonoBehaviour
{
    public float life = 20f;

    Renderer r;
    float startLife;

    void Start()
    {
        r = GetComponent<Renderer>();
        startLife = life;
    }

    void Update()
    {
        life -= Time.deltaTime;

        float a = life / startLife;

        Color c = r.material.color;
        c.a = a;

        r.material.color = c;

        if (life <= 0)
            Destroy(gameObject);
    }
}