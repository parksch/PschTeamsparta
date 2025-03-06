using ClientEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public GameManager.Cell currentCell;

    protected Vector3 currentNormal;

    public ClientEnum.MoveType CurrentMove()
    {
        if (currentNormal == Vector3.up)
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
        else if (currentNormal == Vector3.down)
        {
            return MoveType.Down;
        }
        else
        {
            return MoveType.Stop;
        }
    }

    public void SetMove(ClientEnum.MoveType moveType)
    {
        switch (moveType)
        {
            case ClientEnum.MoveType.Stop:
                currentNormal = Vector3.zero;
                break;
            case ClientEnum.MoveType.Left:
                currentNormal = Vector3.left;
                break;
            case ClientEnum.MoveType.Right:
                currentNormal = Vector3.right;
                break;
            case ClientEnum.MoveType.Up:
                currentNormal = Vector3.up;
                break;
            case ClientEnum.MoveType.Down:
                currentNormal = Vector3.down;
                break;
            default:
                break;
        }
    }

}
