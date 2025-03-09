using ClientEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpot : MonoBehaviour
{
    [SerializeField] string boxCode = "Box";
    [SerializeField] string hero = "Hero";
    [SerializeField] string weapon;
    [SerializeField] Transform parent;
    [SerializeField] PlayerObject playerObject;
    [SerializeField] HpPanel hpPanel;
    [SerializeField] SpotType spotType;
    [SerializeField] int attack;
    [SerializeField] int maxHp;
    
    
    int curHp;

    public string Weapon => weapon;
    public int Attack => attack;
    public int Hp => curHp;
    public int MaxHp => maxHp;
    public SpotType SpotType => spotType;

    public void Hit(int attack)
    {
        if (curHp == 0)
        {
            return;
        }

        curHp -= attack;

        if (curHp < 0)
        {
            curHp = 0;
        }

        hpPanel.SetValue(curHp / (float)maxHp);

        if (curHp == 0)
        {
            GameManager.Instance.UpdateSpot(this);
        }
    }

    public void SetSpot(DataManager.SpotData spotData)
    {
        if (playerObject != null)
        {
            PoolManager.Instance.Enqueue(playerObject.GetComponent<ObjectPool>(),playerObject.gameObject);
            playerObject = null;
        }

        attack = GameManager.Instance.MainData.levelPerPlayerAttack * spotData.level;
        maxHp = GameManager.Instance.MainData.levelPerPlayerHp * spotData.level;
        curHp = maxHp;
        spotType = spotData.type;
        weapon = spotData.weapon;

        if (maxHp == 0)
        {
            hpPanel.SetValue(0f);
        }
        else
        {
            hpPanel.SetValue(1f);
        }

        Create(spotData.type, weapon);
    }

    public void UpdateSpot(GameManager.CurSpot cur)
    {
        if (playerObject != null)
        {
            PoolManager.Instance.Enqueue(playerObject.GetComponent<ObjectPool>(), playerObject.gameObject);
            playerObject = null;
        }

        attack = cur.attack;
        maxHp = cur.maxHp;
        curHp = cur.curHp;
        spotType = cur.type;
        weapon = cur.weapon;
        Create(cur.type, cur.weapon);
        hpPanel.SetValue(curHp / (float)maxHp);
    }

    void Create(SpotType spotType,string weapon)
    {
        switch (spotType)
        {
            case SpotType.Player:
                playerObject = PoolManager.Instance.Dequeue(hero).GetComponent<PlayerObject>();
                playerObject.transform.parent = parent;
                playerObject.transform.localPosition = Vector3.zero;
                playerObject.Set(weapon, attack);
                playerObject.gameObject.SetActive(true);
                break;
            case SpotType.Box:
                playerObject = PoolManager.Instance.Dequeue(boxCode).GetComponent<PlayerObject>();
                playerObject.transform.parent = parent;
                playerObject.transform.localPosition = Vector3.zero;
                playerObject.Set(weapon, attack);
                playerObject.gameObject.SetActive(true);
                break;
            case SpotType.Empty:
                if (playerObject != null)
                {
                    PoolManager.Instance.Enqueue(playerObject.GetComponent<ObjectPool>(), playerObject.gameObject);
                }
                break;
            default:
                break;
        }
    }

    public void SetRot(Vector3 target)
    {
        playerObject.SetRot(target);
    }

    public void ClickOff() => playerObject.ClickOff();
}
