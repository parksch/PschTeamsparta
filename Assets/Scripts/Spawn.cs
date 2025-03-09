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

    public void Set(bool onOff)
    {
        if (onOff)
        {
            currentTime = spawnTimer;
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    private void FixedUpdate()
    {
        if (currentTime <= 0 && GameManager.Instance.CheckMonsterCount)
        {
            currentTime = spawnTimer;
            if (GameManager.Instance.IsEnemyHere(transform.position))
            {
                GameObject monster = PoolManager.Instance.Dequeue(monsterPrefab.GetComponent<ObjectPool>().ID);
                monster.transform.parent = monsterParent;
                monster.transform.position = transform.position;
                GameManager.Instance.AddMonster(monster);
            }
        }

        currentTime -= Time.deltaTime;
    }
}
