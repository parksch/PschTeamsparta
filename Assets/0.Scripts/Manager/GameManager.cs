using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientEnum;
using Unity.VisualScripting;
using System.Threading;

public class GameManager : MonoBehaviour
{
    [SerializeField] Cell[,] cells;
    [SerializeField] Vector2Int max;
    [SerializeField] float cellSize;
    [SerializeField] float closeDist;
    [SerializeField] Transform cellStartPos;
    [SerializeField] Spawn monsterSpawn;
    [SerializeField] List<Monster> monsters = new List<Monster>();
    [SerializeField] Animator ground;
    [SerializeField] Animator back;

    public static GameManager Instance;

    public bool IsPlayer(Monster monster)
    {
        Vector2Int grid = GetGrid(monster);
        return grid.x <= 0;
    }

    [System.Serializable]
    public class Cell
    {
        public Vector2Int grid;
        public Vector3 pos;
        public MoveObject game;
    }

    private void Awake()
    {
        Instance = this;
        cells = new Cell[max.y,max.x];

        for (int y = 0; y < max.y; y++)
        {
            for (int x = 0; x < max.x; x++)
            {
                cells[y,x] = new Cell();
                cells[y,x].grid = new Vector2Int(x,y);
                cells[y,x].pos = new Vector3(cellStartPos.position.x + (x * cellSize), cellStartPos.position.y + (y * cellSize), 0);
            }
        }
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        ground.SetFloat("Speed", 1f);
        back.SetFloat("Speed", 1f);
        monsterSpawn.gameObject.SetActive(true);
    }

    public void AddMonster(GameObject go)
    {
        Monster monster = go.GetComponent<Monster>();
        monster.Set();
    }

    public void CheckMonsterGrid(Monster monster)
    {
        Vector2Int monsterGrid = GetGrid(monster);

        if (monster.currentCell == null)
        {
            if (cells[monsterGrid.y,monsterGrid.x].game == null)
            {
                monster.currentCell = cells[monsterGrid.y, monsterGrid.x];
                cells[monsterGrid.y, monsterGrid.x].game = monster;
                monster.SetMove(MoveType.Left);
            }
        }
        else
        {
            MoveType moveType = MoveType.Stop;

            if (IsClose(monster, monster.currentCell.grid))
            {
                monster.currentCell.game = null;
                monster.currentCell = cells[monsterGrid.y, monsterGrid.x];
                monster.currentCell.game = monster;

                if (monster.currentCell.grid.y > 0)
                {
                    if (monster.currentCell.grid.x - 1 >= 0)
                    {
                        if (cells[monster.currentCell.grid.y - 1, monster.currentCell.grid.x - 1].game != null)
                        {
                            MoveObject other = cells[monster.currentCell.grid.y - 1, monster.currentCell.grid.x - 1].game;
                            if (other.CurrentMove() != MoveType.Up)
                            {
                                monster.SetMove(MoveType.Left);
                            }
                        }
                        else if (cells[monster.currentCell.grid.y, monster.currentCell.grid.x - 1].game != null)
                        {
                            monster.SetMove(MoveType.Stop);
                        }
                        else
                        {
                            monster.SetMove(MoveType.Down);
                        }
                    }
                    else
                    {
                        if (cells[monster.currentCell.grid.y - 1, monster.currentCell.grid.x].game == null)
                        {
                            monster.SetMove(MoveType.Down);
                        }
                        else
                        {
                            monster.SetMove(MoveType.Stop);
                        }
                    }
                }
                else
                {
                    if (monster.currentCell.grid.x -1 >= 0)
                    {
                        if (cells[monster.currentCell.grid.y, monster.currentCell.grid.x - 1].game != null)
                        {
                            if (cells[monster.currentCell.grid.y + 1, monster.currentCell.grid.x].game == null)
                            {
                                monster.SetMove(MoveType.Up);
                            }
                            else
                            {
                                if (CheckCloseDist(monster.currentCell.pos.x, monster.transform.position.x))
                                {
                                    monster.SetMove(MoveType.Stop);
                                }
                            }
                        }
                    }
                    else if (monster.currentCell.grid.x == 0)
                    {
                        if (cells[monster.currentCell.grid.y + 1, monster.currentCell.grid.x].game != null && cells[monster.currentCell.grid.y + 1, monster.currentCell.grid.x + 1].game == null)
                        {
                            monster.SetMove(MoveType.Right);
                        }
                        else if (CheckCloseDist(monster.currentCell.pos.x, monster.transform.position.x))
                        {
                            monster.SetMove(MoveType.Stop);
                        }
                    }
                    else
                    {
                        monster.SetMove(MoveType.Left);
                    }
                }

                if (!CheckAround(monster.currentCell.grid, moveType))
                {
                    monster.SetMove(MoveType.Stop);
                }
                else
                {
                    monster.SetMove(moveType);
                }

            }
        }
    }

    bool IsClose(Monster monster,Vector2Int grid)
    {
        switch (monster.CurrentMove())
        {
            case MoveType.Stop:
                return true;
            case MoveType.Left:
                if (grid.x - 1 > 0)
                {
                    if (Mathf.Abs(Mathf.Abs(cells[grid.y,grid.x - 1].pos.x) - Mathf.Abs(monster.transform.position.x)) < closeDist)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
                break;
            case MoveType.Right:
                if (grid.x + 1 < max.x)
                {
                    if (Mathf.Abs(Mathf.Abs(cells[grid.y, grid.x + 1].pos.x) - Mathf.Abs(monster.transform.position.x)) < closeDist)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
                break;
            case MoveType.Up:
                if (grid.y + 1 < max.y)
                {
                    if (Mathf.Abs(Mathf.Abs(cells[grid.y + 1, grid.x].pos.y) - Mathf.Abs(monster.transform.position.y)) < closeDist)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
                break;
            case MoveType.Down:
                if (grid.y - 1 > 0)
                {
                    if (Mathf.Abs(Mathf.Abs(cells[grid.y - 1, grid.x].pos.y) - Mathf.Abs(monster.transform.position.y)) < closeDist)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
                break;
            default:
                break;
        }

        return false;
    }

    bool CheckCloseDist(float value, float value2)
    {
        float a = value > value2 ? value : value2;
        float b = value > value2 ? value2 : value;

        float result = Mathf.Abs(a - b);
        return result <= closeDist;
    }

    bool CheckAround(Vector2Int grid,MoveType moveType)
    {

        switch (moveType)
        {
            case MoveType.Left:
                if (grid.x - 1 > 0 && cells[grid.y,grid.x - 1] != null)
                {
                    return false;
                }
                if (grid.y + 1 < max.y && grid.x - 1 > 0 && cells[grid.y + 1,grid.x - 1].game != null)
                {
                    if(cells[grid.y + 1, grid.x - 1].game.CurrentMove() == MoveType.Down)
                    {
                        return false;
                    }
                }
                if (grid.y - 1 > 0 && grid.x - 1 > 0 && cells[grid.y - 1,grid.x - 1].game != null)
                {
                    if (cells[grid.y - 1, grid.x - 1].game.CurrentMove() == MoveType.Up)
                    {
                        return false;
                    }
                }
                if ( grid.x - 2 > 0 && cells[grid.y, grid.x - 2].game != null)
                {
                    if (cells[grid.y, grid.x - 2].game.CurrentMove() == MoveType.Right)
                    {
                        return false;
                    }
                }
                break;
            case MoveType.Right:
                if (grid.x + 1 < max.x && cells[grid.y, grid.x + 1] != null)
                {
                    return false;
                }
                if (grid.y + 1 < max.y && grid.x - 1 > 0 && cells[grid.y + 1, grid.x - 1].game != null)
                {
                    if (cells[grid.y + 1, grid.x - 1].game.CurrentMove() == MoveType.Down)
                    {
                        return false;
                    }
                }
                if (grid.y - 1 > 0 && grid.x - 1 > 0 && cells[grid.y - 1, grid.x - 1].game != null)
                {
                    if (cells[grid.y - 1, grid.x - 1].game.CurrentMove() == MoveType.Up)
                    {
                        return false;
                    }
                }
                if (grid.x - 2 > 0 && cells[grid.y, grid.x - 2].game != null)
                {
                    if (cells[grid.y, grid.x - 2].game.CurrentMove() == MoveType.Right)
                    {
                        return false;
                    }
                }
                break;
            case MoveType.Up:
                break;
            case MoveType.Down:
                break;
            default:
                break;
        }

        return true;
    }

    Vector2Int GetGrid(Monster monster)
    {
        Vector3 offset = monster.transform.position - cellStartPos.position;
        offset.x = offset.x / cellSize;
        offset.y = offset.y / cellSize;

        Vector2Int monsterGrid = new Vector2Int((int)offset.x, (int)offset.y);

        return monsterGrid;
    }

    private void OnDrawGizmosSelected()
    {
        if (cells == null)
        {
            return;
        }

        foreach (Cell cell in cells)
        {
            if (cell.game != null)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.white;
            }

            Gizmos.DrawWireSphere(cell.pos,.1f);
        }
    }
}
