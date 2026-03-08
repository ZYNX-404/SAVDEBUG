using UnityEngine;

public class MiniMapReplayBuffer : MonoBehaviour
{
    public int bufferSize = 150;

    struct Frame
    {
        public Vector3 pos;
        public Quaternion rot;
    }

    Frame[] frames;
    int index;

    void Start()
    {
        frames = new Frame[bufferSize];
    }

    void Update()
    {
        frames[index].pos = transform.position;
        frames[index].rot = transform.rotation;

        index++;

        if (index >= bufferSize)
            index = 0;
    }

    public Vector3[] GetPositions()
    {
        Vector3[] result = new Vector3[bufferSize];

        for (int i = 0; i < bufferSize; i++)
        {
            int idx = (index + i) % bufferSize;
            result[i] = frames[idx].pos;
        }

        return result;
    }
}