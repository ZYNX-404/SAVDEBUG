using UnityEngine;

public class MiniMapIslandManager : MonoBehaviour
{
    public MiniMapIsland[] islands;

    public MiniMapIsland GetIsland(Vector3 pos)
    {
        foreach (var i in islands)
        {
            if (i.IsInside(pos))
                return i;
        }

        return null;
    }
}