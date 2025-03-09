using ClientEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MoveObject
{
    [SerializeField] PlayerSpot target;
    [SerializeField] bool isOn;
    [SerializeField] int hp;
    [SerializeField] int attack;
    [SerializeField] float speed;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rigid2d;

    public void Set(GameManager.Cell cell)
    {
        hp = DataManager.instance.stage * GameManager.Instance.MainData.levelPerEnemyHp;
        attack = DataManager.instance.stage * GameManager.Instance.MainData.levelPerEnemyAttack;
        currentCell = cell;
        transform.position = currentCell.pos;
        gameObject.SetActive(true);
        animator.Play("Run");
        animator.SetBool("IsAttacking",false);
        animator.SetBool("IsDead",false);
        animator.SetBool("IsIdle",false);
        isOn = true;
    }

    public void Hit(int damage)
    {
        UIManager.instance.SetDamageUI(transform.position + Vector3.up * .5f,damage);
        hp -= damage;

        if (hp <= 0)
        {
            GameManager.Instance.RemoveMonster(this);
        }
    }

    public void OnAttack()
    {
        target = GameManager.Instance.GetSpot(transform.position + Vector3.up * .5f);

        if (target != null && currentCell.grid.x == 0)
        {
            target.Hit(attack);
        }
    }

    private void FixedUpdate()
    {
        if (!isOn && GameManager.Instance.GameState != GameState.Game)
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
