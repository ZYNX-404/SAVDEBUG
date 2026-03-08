using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CombatRadioLog : MonoBehaviour
{
    public static CombatRadioLog Instance;

    public GameObject logPrefab;
    public Transform logRoot;

    public int maxLogs = 20;

    Queue<TextMeshProUGUI> logs = new Queue<TextMeshProUGUI>();

    void Awake()
    {
        Instance = this;
    }

    public void AddLog(string text)
    {
        TextMeshProUGUI log;

        if (logs.Count >= maxLogs)
        {
            log = logs.Dequeue();
        }
        else
        {
            var go = Instantiate(logPrefab, logRoot);
            log = go.GetComponent<TextMeshProUGUI>();
        }

        log.text = text;

        logs.Enqueue(log);

        UpdatePositions();
    }

    void UpdatePositions()
    {
        int i = 0;

        foreach (var l in logs)
        {
            l.transform.localPosition = new Vector3(0, -i * 20f, 0);
            i++;
        }
    }
}