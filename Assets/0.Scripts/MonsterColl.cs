using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterColl : MonoBehaviour
{
    [SerializeField] Monster target;
    [SerializeField] string layerName;
    [SerializeField] Vector3 normal;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(layerName))
        {
            target.SetNormal(normal);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(layerName))
        {
            target.SetNormal(new Vector3(-1, -1));
        }
    }
}
