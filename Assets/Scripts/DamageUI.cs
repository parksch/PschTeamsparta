using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageUI : MonoBehaviour
{
    [SerializeField] Animator ani;
    [SerializeField] Text text;

    public void Set(Vector3 pos,int value)
    {
        transform.localScale = Vector3.one;
        transform.position = pos;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        text.text = value.ToString();
        gameObject.SetActive(true);
        ani.Play("Damage", -1, 0f);
    }

    public void End()
    {
        PoolManager.Instance.Enqueue(GetComponent<ObjectPool>(), gameObject);
    }
}
