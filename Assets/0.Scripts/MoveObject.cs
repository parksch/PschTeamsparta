using ClientEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public GameManager.Cell currentCell = null;

    protected Vector3 currentNormal;

    public MoveType CurrentMove()
    {
        if (currentNormal == Vector3.up * 2f)
        {
            return MoveType.Up;
        }
        else if (currentNormal == Vector3.left)
        {
            return MoveType.Left;
        }
        else if (currentNormal == Vector3.right)
        {
            return MoveType.Right;
        }
        else if (currentNormal == Vector3.down * 2f)
        {
            return MoveType.Down;
        }
        else
        {
            return MoveType.Stop;
        }
    }

    public void SetMove(MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.Stop:
                currentNormal = Vector3.zero;
                break;
            case MoveType.Left:
                currentNormal = Vector3.left;
                break;
            case MoveType.Right:
                currentNormal = Vector3.right;
                break;
            case MoveType.Up:
                currentNormal = Vector3.up * 2f;
                break;
            case MoveType.Down:
                currentNormal = Vector3.down * 2f;
                break;
            default:
                break;
        }
    }

}
