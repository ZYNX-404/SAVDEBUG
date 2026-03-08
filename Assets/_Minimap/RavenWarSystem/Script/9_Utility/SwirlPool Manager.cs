using UnityEngine;
using System.Collections.Generic;

public class MiniMapDogfightSwirlPool : MonoBehaviour
{
    public GameObject swirlPrefab;
    public int poolSize = 8;

    List<DogfightSwirl> pool = new List<DogfightSwirl>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var s = Instantiate(swirlPrefab, transform)
                .GetComponent<DogfightSwirl>();

            s.gameObject.SetActive(false);
            pool.Add(s);
        }
    }

    public DogfightSwirl Get()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeSelf)
            {
                pool[i].gameObject.SetActive(true);
                return pool[i];
            }
        }

        return null;
    }

    public void ResetAll()
    {
        foreach (var s in pool)
            s.gameObject.SetActive(false);
    }
}