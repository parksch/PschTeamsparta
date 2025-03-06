using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    [SerializeField] float bossTimer;
    [SerializeField] float spawnTimer;
    [SerializeField] Transform monsterParent;
    [SerializeField] Monster monsterPrefab;

    float currentTime = 0;

    private void FixedUpdate()
    {
        if (currentTime <= 0)
        {
            currentTime = spawnTimer;
            GameObject monster = PoolManager.Instance.Dequeue(monsterPrefab.GetComponent<ObjectPool>().ID);
            monster.transform.parent = monsterParent;
            monster.transform.position = transform.position;
            GameManager.Instance.AddMonster(monster);
        }

        currentTime -= Time.deltaTime;
    }
}
