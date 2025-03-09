using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] Transform forward;
    [SerializeField] float radius;
    [SerializeField] float timer;
    [SerializeField] string projectile;

    Vector3 target;
    int damage;
    float current = 0;
    bool isClick = false;

    public void Init(int target)
    {
        damage = target;
    }

    public void SetRot(Vector3 rot)
    {
        isClick = true;
        target = rot;
    }

    public void ClickOff()
    {
        isClick = false;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.GameState != ClientEnum.GameState.Game)
        {
            return;
        }

        if (!isClick)
        {
            Monster monster = GameManager.Instance.GetMonster(gameObject);
            if (monster != null)
            {
                target = monster.transform.position + Vector3.up * .5f + monster.Noraml * .5f;
            }
        }

        transform.LookAt(target);
        current -= Time.deltaTime;

        if (current <= 0 && ((GameManager.Instance.IsMonster && XDist(target.x) < radius) || isClick))
        {
            current = timer;
            Projectile tile = PoolManager.Instance.Dequeue(projectile).GetComponent<Projectile>();
            Vector3 normal = (forward.position - transform.position).normalized;
            tile.transform.parent = null;
            tile.transform.position = transform.position + transform.forward  * .5f;
            tile.Shoot(normal,damage);
            tile.gameObject.SetActive(true);
        }
    }

    public float XDist(float x)
    {
        float max = x > transform.position.x ? x : transform.position.x;
        float min = x > transform.position.x ? transform.position.x : x;

        return Mathf.Abs(max - min);
    }
}
