using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MiniMapKillReplay : MonoBehaviour
{
    public LineRenderer replayLine;

    public float replaySpeed = 0.5f;

    Coroutine currentReplay;

    public void PlayReplay(MiniMapReplayBuffer buffer)
    {
        if (buffer == null) return;

        if (currentReplay != null)
            StopCoroutine(currentReplay);

        currentReplay = StartCoroutine(Replay(buffer));
    }

    IEnumerator Replay(MiniMapReplayBuffer buffer)
    {
        var positions = buffer.GetPositions();

        if (positions == null || positions.Length == 0)
            yield break;

        replayLine.positionCount = positions.Length;

        replayLine.SetPositions(positions);

        yield return new WaitForSeconds(replaySpeed);

        replayLine.positionCount = 0;

        currentReplay = null;
    }
}