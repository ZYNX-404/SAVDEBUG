using UnityEngine;

public class MiniMapMissileMarker : MonoBehaviour
{
    public Rigidbody target;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (MiniMapDataBus.Instance != null)
            MiniMapDataBus.Instance.RegisterMissile(rb);
    }

    void OnDestroy()
    {
        if (MiniMapDataBus.Instance != null)
            MiniMapDataBus.Instance.RemoveMissile(rb);
    }

    public Rigidbody GetRB()
    {
        return rb;
    }
}