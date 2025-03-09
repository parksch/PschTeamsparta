using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] string id;
    [SerializeField] ClientEnum.ObjectPool pool;

    public ClientEnum.ObjectPool PoolType => pool;
    public string ID => id;
}
