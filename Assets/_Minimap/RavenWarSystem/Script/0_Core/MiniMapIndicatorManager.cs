using UnityEngine;

public class MiniMapIndicatorManager : MonoBehaviour
{
    MiniMapIndicator[] indicators;

    void Awake()
    {
        indicators = GetComponentsInChildren<MiniMapIndicator>(true);
    }

    public void SetLevel(MiniMapIndicatorLevel level)
    {
        if (indicators == null)
            return;

        foreach (var indicator in indicators)
        {
            if (indicator == null) continue;
            indicator.SetLevel(level);
        }
    }
}
