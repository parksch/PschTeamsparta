using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientEnum;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameState gameState;
    [SerializeField] Cell[,] cells;
    [SerializeField] Vector2Int max;
    [SerializeField] float cellSize;
    [SerializeField] float closeDist;
    [SerializeField] Transform cellStartPos;
    [SerializeField] Spawn monsterSpawn;
    [SerializeField] List<PlayerSpot> spotList;
    [SerializeField] List<Monster> monsters = new List<Monster>();
    [SerializeField] Animator ground;
    [SerializeField] Animator back;
    [SerializeField] PlayerSpot masterSpot;

    public GameState GameState => gameState;
    public static GameManager Instance;

    public bool IsPlayer(Monster monster)
    {
        Vector2Int grid = GetGrid(monster);
        return grid.x <= 0 ;
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

    void Update()
    {
        if (GameState != GameState.Game)
        {
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            masterSpot.ClickOff();
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z -= Camera.main.transform.position.z;
            Vector3 pos = Camera.main.ScreenToWorldPoint(mousePos);
            masterSpot.SetRot(pos);
        }
    }

    public void Init()
    {
        UIManager.instance.Init();
        gameState = GameState.Buy;
        ground.SetFloat("Speed", 0f);
        back.SetFloat("Speed", 0f);
        monsterSpawn.Set(false);

        for (int i = 0; i < DataManager.instance.spotDatas.Count; i++)
        {
            spotList[i].SetSpot(DataManager.instance.spotDatas[i]);
        }

        masterSpot = spotList.Find(x => x.SpotType == SpotType.Player);
        UIManager.instance.SetUI(gameState);
    }

    public void ChangeState(GameState game)
    {
        gameState = game;
        UIManager.instance.SetUI(gameState);

        ground.SetFloat("Speed", gameState == GameState.Game ? 1 : 0);
        back.SetFloat("Speed", gameState == GameState.Game ? 1 : 0);
        monsterSpawn.Set(gameState == GameState.Game);
    }

    public void AddMonster(GameObject go)
    {
        Monster monster = go.GetComponent<Monster>();
        Vector2Int monsterGrid = GetGrid(monster);
        monster.Set(cells[monsterGrid.y, monsterGrid.x]);
        cells[monsterGrid.y, monsterGrid.x].game = monster;
        monsters.Add(monster);
    }

    public Monster GetMonster()
    {
        if (monsters.Count > 0)
        {
            return monsters[0];
        }
        else
        {
            return null;
        }
    }

    public void UpdateSpot()
    {

    }

    public void RemoveMonster(Monster target)
    {
        Cell cell = target.currentCell;
        cell.game = null;
        monsters.Remove(target);
        PoolManager.Instance.Enqueue(target.GetComponent<ObjectPool>(),target.gameObject);
    }

    public void CheckMonsterGrid(Monster monster)
    {
        MoveType moveType = MoveType.Stop;

        if (IsClose(monster, monster.currentCell.grid))
        {
            if (monster.currentCell.game == monster)
            {
                monster.currentCell.game = null;
            }

            switch (monster.CurrentMove())
            {
                case MoveType.Left:
                    if (monster.currentCell.grid.x - 1 >= 0)
                    {
                        monster.currentCell = cells[monster.currentCell.grid.y, monster.currentCell.grid.x - 1];
                    }
                    break;
                case MoveType.Right:
                    if (monster.currentCell.grid.x + 1 < max.x)
                    {
                        monster.currentCell = cells[monster.currentCell.grid.y, monster.currentCell.grid.x + 1];
                    }
                    break;
                case MoveType.Up:
                    if (monster.currentCell.grid.y + 1 < max.y)
                    {
                        monster.currentCell = cells[monster.currentCell.grid.y + 1, monster.currentCell.grid.x];
                    }
                    break;
                case MoveType.Down:
                    if (monster.currentCell.grid.y - 1 >= 0)
                    {
                        monster.currentCell = cells[monster.currentCell.grid.y - 1, monster.currentCell.grid.x];
                    }
                    break;
                default:
                    break;
            }

            if (monster.currentCell.game != null)
            {
                monster.currentCell.game.SetMove(MoveType.Right);
            }

            monster.currentCell.game = monster;
            monster.transform.position = new Vector3(monster.transform.position.x,monster.currentCell.pos.y,0);

            moveType = GetMoveType(monster.currentCell.grid);

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

    bool IsClose(Monster monster,Vector2Int grid)
    {
        switch (monster.CurrentMove())
        {
            case MoveType.Stop:
                return true;
            case MoveType.Left:
                if (grid.x - 1 >= 0)
                {
                    if (CheckCloseDist(cells[grid.y, grid.x - 1].pos.x, monster.transform.position.x))
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
                    if (CheckCloseDist(cells[grid.y, grid.x + 1].pos.x, monster.transform.position.x))
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
                    if (CheckCloseDist(cells[grid.y + 1, grid.x].pos.y, monster.transform.position.y))
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
                if (grid.y - 1 >= 0)
                {
                    if (CheckCloseDist(cells[grid.y - 1, grid.x].pos.y, monster.transform.position.y))
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
                if (grid.x - 1 >= 0 && cells[grid.y,grid.x - 1].game != null)
                {
                    if (cells[grid.y, grid.x - 1].game.CurrentMove() == MoveType.Stop)
                    {
                        return false;
                    }
                }
                if (grid.y + 1 < max.y && grid.x - 1 >= 0 && cells[grid.y + 1,grid.x - 1].game != null)
                {
                    if(GetMoveType(cells[grid.y + 1, grid.x - 1].grid) == MoveType.Down)
                    {
                        return false;
                    }
                }
                if (grid.y - 1 >= 0 && grid.x - 1 >= 0 && cells[grid.y - 1,grid.x - 1].game != null)
                {
                    if (cells[grid.y - 1, grid.x - 1].game.CurrentMove() == MoveType.Up)
                    {
                        return false;
                    }
                }
                if ( grid.x - 2 >= 0 && cells[grid.y, grid.x - 2].game != null)
                { 
                    if (cells[grid.y, grid.x - 2].game.CurrentMove() == MoveType.Right)
                    {
                        return false;
                    }
                }
                if (grid.x == 0)
                {
                    if (CheckCloseDist(cells[grid.y, grid.x].pos.x, cells[grid.y, grid.x].game.transform.position.x))
                    {
                        return false;
                    }
                }
                break;
            case MoveType.Right:
                if (grid.y + 1 < max.y && grid.x + 1 < max.x && cells[grid.y + 1, grid.x + 1].game != null)
                {
                    if (GetMoveType(cells[grid.y + 1, grid.x + 1].grid) == MoveType.Down)
                    {
                        return false;
                    }
                }
                if (grid.y - 1 >= 0 && grid.x + 1 >= 0 && cells[grid.y - 1, grid.x + 1].game != null)
                {
                    if (cells[grid.y - 1, grid.x + 1].game.CurrentMove() == MoveType.Up)
                    {
                        return false;
                    }
                }
                if (grid.x + 2 < max.x && cells[grid.y, grid.x + 2].game != null)
                {
                    if (cells[grid.y, grid.x + 2].game.CurrentMove() == MoveType.Left)
                    {
                        return false;
                    }
                }
                if (grid.x == max.x - 1)
                {
                    if (CheckCloseDist(cells[grid.y, grid.x].pos.x, cells[grid.y, grid.x].game.transform.position.x))
                    {
                        return false;
                    }
                }
                break;
            case MoveType.Up:
                if (grid.y + 1 < max.y && cells[grid.y + 1,grid.x].game != null)
                {
                    return false;
                }
                if (grid.y + 2 < max.y && cells[grid.y + 2,grid.x].game != null )
                {
                    if (GetMoveType(cells[grid.y + 2, grid.x].grid) == MoveType.Down)
                    {
                        return false;
                    }
                }
                if (grid.y + 1 < max.y && grid.x - 1 >= 0 && cells[grid.y + 1, grid.x - 1].game != null)
                {
                    if (cells[grid.y + 1,grid.x - 1].game.CurrentMove() == MoveType.Right)
                    {
                        return false;
                    }
                }
                if (grid.y + 1 < max.y && grid.x + 1 < max.x && cells[grid.y + 1, grid.x + 1].game != null)
                {
                    if (GetMoveType(cells[grid.y + 1, grid.x + 1].grid) == MoveType.Left)
                    {
                        return false;
                    }
                }
                if (grid.y == max.y - 1)
                {
                    if (CheckCloseDist(cells[grid.y, grid.x].pos.y, cells[grid.y, grid.x].game.transform.position.y))
                    {
                        return false;
                    }
                }
                break;
            case MoveType.Down:
                if (grid.y - 1 >= 0 && cells[grid.y - 1, grid.x].game != null)
                {
                    return false;
                }
                if (grid.y - 2 >= 0 && cells[grid.y - 2, grid.x].game != null)
                {
                    if (cells[grid.y - 2, grid.x].game.CurrentMove() == MoveType.Up)
                    {
                        return false;
                    }
                }
                if (grid.y - 1 >= 0 && grid.x - 1 >= 0 && cells[grid.y - 1, grid.x - 1].game != null)
                {
                    if (cells[grid.y - 1, grid.x - 1].game.CurrentMove() == MoveType.Right)
                    {
                        return false;
                    }
                }
                if (grid.y - 1 >= 0 && grid.x + 1 < max.x && cells[grid.y - 1, grid.x + 1].game != null)
                {
                    if (cells[grid.y - 1, grid.x + 1].game.CurrentMove() == MoveType.Left)
                    {
                        return false;
                    }
                }
                if (grid.y == 0)
                {
                    if (CheckCloseDist(cells[grid.y, grid.x].pos.y, cells[grid.y, grid.x].game.transform.position.y))
                    {
                        return false;
                    }
                }
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

        float xRemain = offset.x - (int)offset.x;
        float yRemain = offset.y - (int)offset.y;

        offset.x -= (xRemain < closeDist ? 1 : 0);
        offset.x += (xRemain > 1 - closeDist ? 1 : 0);

        offset.y += (yRemain > 1 - closeDist ? 1 : 0);
        offset.y -= (yRemain < closeDist ? 1 : 0);

        if (offset.x >= max.x)
        {
            offset.x = max.x - 1;
        }
        else if (offset.x < 0)
        {
            offset.x = 0;
        }
        
        if (offset.y >= max.y)
        {
            offset.y = max.y - 1;
        }
        else if (offset.y < 0)
        {
            offset.y = 0;
        }

        Vector2Int monsterGrid = new Vector2Int((int)offset.x, (int)offset.y);

        return monsterGrid;
    }

    MoveType GetMoveType(Vector2Int grid)
    {
        if (grid.y > 0)
        {
            if ((grid.x - 1 >= 0 && 
                (cells[grid.y - 1, grid.x - 1].game != null) || 
                 cells[grid.y - 1, grid.x].game != null))

            {
                if (grid.x - 1 >= 0 && cells[grid.y,grid.x - 1].game == null)
                {
                    return MoveType.Left;
                }
            }

            if (cells[grid.y - 1, grid.x].game == null)
            {
                return MoveType.Down;
            }
        }

        if (grid.x - 1 >= 0)
        {
            if (grid.y + 1 < max.y &&
                cells[grid.y, grid.x - 1].game != null &&
                cells[grid.y + 1, grid.x - 1].game == null &&
                cells[grid.y + 1, grid.x + 1].game == null &&
                cells[grid.y + 1, grid.x].game == null)
            {
                if (cells[grid.y, grid.x - 1].game.CurrentMove() == MoveType.Stop && 
                   (cells[grid.y, grid.x + 1].game == null ||
                   (cells[grid.y, grid.x + 1].game != null && GetMoveType(cells[grid.y, grid.x + 1].grid) != MoveType.Up)))
                {
                    return MoveType.Up;
                }
            }

            if (cells[grid.y, grid.x - 1].game != null && 
                cells[grid.y, grid.x - 1].game.CurrentMove() == MoveType.Right)
            {
                return MoveType.Right;
            }
            else
            {
                return MoveType.Left;
            }
        }

        if (grid.y + 1 < max.y && grid.x + 1 < max.x && cells[grid.y + 1, grid.x].game != null && 
            cells[grid.y + 1, grid.x + 1].game == null)
        {
            return MoveType.Right;
        }

        return MoveType.Stop;
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
