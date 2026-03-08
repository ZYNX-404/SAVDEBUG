using UnityEngine;

public class MiniMapDirectorDebug : MonoBehaviour
{
    public MiniMapAIDirector director;

    void OnGUI()
    {
        if (director == null) return;

        GUILayout.BeginArea(new Rect(20,20,300,200),"Director Debug","box");

        GUILayout.Label("Focus Reason : " + director.debugReason);
        GUILayout.Label("Score        : " + director.debugScore.ToString("F2"));

        GUILayout.EndArea();
    }
}