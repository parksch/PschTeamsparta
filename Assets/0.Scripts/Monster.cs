using ClientEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Monster : MoveObject
{
    [SerializeField] PlayerSpot target;
    [SerializeField] bool isOn;
    [SerializeField] int hp;
    [SerializeField] int attack;
    [SerializeField] float speed;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rigid2d;

    public void Set()
    {
        currentCell = null;
        gameObject.SetActive(true);
        animator.Play("Run");
        animator.SetBool("IsAttacking",false);
        animator.SetBool("IsDead",false);
        animator.SetBool("IsIdle",false);
        isOn = true;
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

        GameManager.Instance.CheckMonsterGrid(this);

        rigid2d.velocity = currentNormal * speed;

        if (GameManager.Instance.IsPlayer(this))
        {
            animator.SetBool("IsAttacking", true);
        }
        else
        {
            animator.SetBool("IsAttacking", false);
        }

    }

}
