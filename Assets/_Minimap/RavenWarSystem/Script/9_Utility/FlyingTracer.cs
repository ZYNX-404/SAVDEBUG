using UnityEngine;

public class FlyingTracer : MonoBehaviour
{
    public LineRenderer line;

    public float speed = 400f;
    public float lifeTime = 0.15f;

    Vector3 startPos;
    Vector3 endPos;

    float travel;
    float distance;
    float timer;

    public void Fire(Vector3 start, Vector3 end)
    {
        startPos = start;
        endPos = end;

        travel = 0f;
        timer = 0f;

        distance = Vector3.Distance(start, end);

        if (line != null)
            line.enabled = true;
    }

    void Update()
    {
        if (line == null || !line.enabled)
            return;

        timer += Time.deltaTime;

        travel += speed * Time.deltaTime;

        float t = travel / distance;

        Vector3 head = Vector3.Lerp(startPos, endPos, t);
        Vector3 tail = Vector3.Lerp(startPos, endPos, t - 0.1f);

        line.SetPosition(0, tail);
        line.SetPosition(1, head);

        if (timer > lifeTime || t >= 1f)
        {
            line.enabled = false;
        }
    }
}

/* 呼び出す時はこんな感じで
if (Random.value < 0.2f)
{
    tracer.Fire(muzzle.position, hitPoint);
}
*/