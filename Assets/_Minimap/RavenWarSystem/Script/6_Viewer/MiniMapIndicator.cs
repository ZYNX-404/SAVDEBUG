using UnityEngine;

public abstract class MiniMapIndicator : MonoBehaviour
{
    protected MiniMapMarker marker;
    protected Rigidbody rb;

    public MiniMapIndicatorLevel requiredLevel =
        MiniMapIndicatorLevel.Basic;

    MiniMapIndicatorLevel currentLevel;

    public void SetLevel(MiniMapIndicatorLevel level)
    {
        currentLevel = level;

        gameObject.SetActive(level >= requiredLevel);
    }
    protected virtual void Awake()
    {
        marker = GetComponentInParent<MiniMapMarker>();
        if (marker != null)
            rb = marker.rb;
    }

    protected virtual void Update()
    {
        if (rb == null) return;
        UpdateIndicator();
    }

    protected abstract void UpdateIndicator();


}