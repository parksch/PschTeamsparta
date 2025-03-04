using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] PlayerSpot target;
    [SerializeField] bool isOn;
    [SerializeField] int hp;
    [SerializeField] int attack;
    [SerializeField] float speed;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rigid2d;
    [SerializeField] Vector3 normal;

    Vector3 currentNormal;

    public void SetNormal(Vector3 pos)
    {
        currentNormal = pos;
    }

    public void ResetNormal()
    {
        currentNormal = normal;
    }

    public void Set()
    {
        animator.Play("Run");
        animator.SetBool("IsAttacking",false);
        animator.SetBool("IsDead",false);
        animator.SetBool("IsIdle",false);
        isOn = true;
        currentNormal = normal;
    }

    public void OnAttack()
    {
        if (target != null)
        {
            target.Hit(attack);
        }
    }

    private void FixedUpdate()
    {
        if (!isOn)
        {
            return;
        }

        rigid2d.velocity = currentNormal * speed;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            target = collision.GetComponent<PlayerSpot>();
            animator.SetBool("IsAttacking", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            target = null;
            animator.SetBool("IsAttacking", false);
        }

    }
}
