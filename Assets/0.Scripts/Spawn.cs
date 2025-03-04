using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    [SerializeField] float bossTimer;
    [SerializeField] float spawnTimer;
    [SerializeField] Transform monsterParent;
    [SerializeField] Monster monsterPrefab;
    Queue<Monster> pools = new Queue<Monster>();
    float currentTime = 0;

    public Monster Get()
    {
        if (pools.Count == 0)
        {
            Monster create = Instantiate(monsterPrefab,monsterParent);
            create.transform.position = transform.position;
            create.gameObject.SetActive(false);
            pools.Enqueue(create);
        }

        Monster monster = pools.Dequeue();
        monster.gameObject.SetActive(true);
        return monster;
    }

    public void Enqueue(Monster monster)
    {
        monster.transform.position = transform.position;
        monster.gameObject.SetActive(false);
        pools.Enqueue(monster);
    }

    private void FixedUpdate()
    {
        if (currentTime <= 0)
        {
            currentTime = spawnTimer;
            Monster monster = Get();
            monster.Set();
        }

        currentTime -= Time.deltaTime;
    }
}
