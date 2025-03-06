using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] List<ObjectPool> prefabs;
    public static PoolManager Instance;
    
    Dictionary<string,Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;

        foreach (ObjectPool pool  in prefabs)
        {
            pools[pool.ID] = new Queue<GameObject>();
        }
    }

    public void Enqueue(string id,GameObject target)
    {
        target.SetActive(false);
        target.transform.parent = transform;
        target.transform.localPosition = Vector3.zero;
        pools[id].Enqueue(target);
    }

    public GameObject Dequeue(string target)
    {
        if (pools[target].Count == 0)
        {
            GameObject gameObject = Instantiate(prefabs.Find(x => x.ID == target).gameObject, transform);
            gameObject.SetActive(false);
            pools[target].Enqueue(gameObject);
        }

        GameObject clone = pools[target].Dequeue();

        return clone;
    }

}
