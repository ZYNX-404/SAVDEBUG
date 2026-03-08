using UnityEngine;

public class MiniMapDogfightSwirlManager : MonoBehaviour
{
    public MiniMapDogfightSwirlPool pool;

    public float dogfightDistance = 0.08f;

    void Update()
    {
        pool.ResetAll();

        var aircraft = MiniMapDataBus.Instance.aircraft;

        for (int i = 0; i < aircraft.Count; i++)
        {
            for (int j = i + 1; j < aircraft.Count; j++)
            {
                var a = aircraft[i];
                var b = aircraft[j];

                if (a == null || b == null) continue;

                float d = (a.transform.position - b.transform.position).sqrMagnitude;
                if (d > dogfightDistance * dogfightDistance) continue;

                if (a.rb == null || b.rb == null) continue;

                Vector3 dirA = a.rb.velocity.normalized;
                Vector3 dirB = b.rb.velocity.normalized;

                float facing = Vector3.Dot(dirA, dirB);

                // 同方向飛行は除外
                if (facing > -0.3f) continue;

                var swirl = pool.Get();
                if (swirl == null) continue;

                Vector3 center =
                    (a.transform.position +
                     b.transform.position) * 0.5f;

                swirl.transform.position = center;

                // 強度を距離で調整
                swirl.SetIntensity(1f - (d / dogfightDistance));
            }
        }
    }
}