using ClientEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpot : MonoBehaviour
{
    [SerializeField] string boxCode = "Box";
    [SerializeField] string hero = "Hero";
    [SerializeField] Transform parent;
    [SerializeField] PlayerObject playerObject;
    [SerializeField] HpPanel hpPanel;
    [SerializeField] SpotType spotType;
    [SerializeField] int attack;
    [SerializeField] int maxHp;
    
    int curHp;

    public SpotType SpotType => spotType;

    public void Hit(int attack)
    {
        curHp -= attack;

        if (curHp < 0)
        {
            curHp = 0;
            hpPanel.gameObject.SetActive(false);
        }
    }

    public void SetSpot(DataManager.SpotData spotData)
    {
        if (playerObject != null)
        {
            PoolManager.Instance.Enqueue(playerObject.GetComponent<ObjectPool>(),playerObject.gameObject);
            playerObject = null;
        }

        spotType = spotData.type;
        hpPanel.SetValue(1f);

        switch (spotType)
        {
            case SpotType.Player:
                playerObject = PoolManager.Instance.Dequeue(hero).GetComponent<PlayerObject>();
                playerObject.transform.parent = parent;
                playerObject.transform.localPosition = Vector3.zero;
                playerObject.Set(spotData.weapon, attack);
                playerObject.gameObject.SetActive(true);
                hpPanel.gameObject.SetActive(true);
                break;
            case SpotType.Box:
                playerObject = PoolManager.Instance.Dequeue(boxCode).GetComponent<PlayerObject>();
                playerObject.transform.parent = parent;
                playerObject.transform.localPosition = Vector3.zero;
                playerObject.Set(spotData.weapon, attack);
                playerObject.gameObject.SetActive(true);
                hpPanel.gameObject.SetActive(true);
                break;
            case SpotType.Empty:
                if (playerObject != null)
                {
                    PoolManager.Instance.Enqueue(playerObject.GetComponent<ObjectPool>(), playerObject.gameObject);
                }
                hpPanel.gameObject.SetActive(false);
                break;
            default:
                break;
        }

        curHp = 10;
    }

    public void SetRot(Vector3 target)
    {
        playerObject.SetRot(target);
    }

    public void ClickOff() => playerObject.ClickOff();
}
