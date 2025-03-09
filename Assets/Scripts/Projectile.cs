using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] float speed;
    [SerializeField] float lifeTime;

    int damage = 0;
    float current = 0;
    Vector3 dir;

    public void Shoot(Vector3 normal,int targetDamage)
    {
        damage = targetDamage;
        current = lifeTime;
        dir = normal;
    }
    
    public void FixedUpdate()
    {
        current -= Time.deltaTime;

        rigid.MovePosition(transform.position + dir * Time.deltaTime * speed);

        if (current < 0)
        {
            PoolManager.Instance.Enqueue(GetComponent<ObjectPool>(), gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            collision.GetComponent<Monster>().Hit(damage);
        }
        PoolManager.Instance.Enqueue(GetComponent<ObjectPool>(), gameObject);
    }
}
