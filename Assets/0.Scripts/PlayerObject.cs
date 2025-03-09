using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    [SerializeField] Transform weaponParent;
    [SerializeField] Weapon target;


    public void Set(string weapon,int damage)
    {
        if (weapon != "")
        {
            target = PoolManager.Instance.Dequeue(weapon).GetComponent<Weapon>();
            target.Init(damage);
            target.transform.parent = weaponParent;
            target.transform.localPosition = Vector3.zero;
            target.gameObject.SetActive(true);
        }
    }

    public void SetRot(Vector3 rot)
    {
        target.SetRot(rot);
    }

    public void ClickOff() => target.ClickOff();
}
