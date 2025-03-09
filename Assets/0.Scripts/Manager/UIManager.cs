using ClientEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas world;
    [SerializeField] List<GameObject> spotUpgrades;
    [SerializeField] GameObject boxUpgrade;
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject powerButton;
    [SerializeField] Camera gameCamera;
    [SerializeField] Vector3 uiCameraPos;
    [SerializeField] Vector3 gameCameraPos;

    public static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {

    }

    public void SetUI(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Buy:
                gameCamera.transform.position = uiCameraPos;
                boxUpgrade.SetActive(true);
                startButton.SetActive(true);
                powerButton.SetActive(true);
                break;
            case GameState.Game:
                gameCamera.transform.position = gameCameraPos;
                boxUpgrade.SetActive(false);
                startButton.SetActive(false);
                powerButton.SetActive(false);
                break;
            case GameState.End:
                break;
            default:
                break;
        }

        UpdateSpotList();
    }

    public void OnClickStart()
    {
        GameManager.Instance.ChangeState(GameState.Game);
    }
    
    public void SetDamageUI(Vector3 pos,int value)
    {
        DamageUI damageUI = PoolManager.Instance.Dequeue("DamageUI").GetComponent<DamageUI>();
        damageUI.transform.SetParent(world.transform);
        damageUI.Set(pos,value);
    }

    public void OnClickBuyBox(int i)
    {
        DataManager.SpotData spot = DataManager.instance.spotDatas[i];
        spot.type = SpotType.Box;
        spot.level += 1;
        GameManager.Instance.SetSpot();
        UpdateSpotList();
    }

    public void UpdateSpotList()
    {
        for (int i = 0; i < DataManager.instance.spotDatas.Count; i++)
        {
            if (i < spotUpgrades.Count)
            {
                if (DataManager.instance.spotDatas[i].type != SpotType.Empty)
                {
                    spotUpgrades[i].SetActive(true);
                }
                else
                {
                    spotUpgrades[i].SetActive(false);
                }
            }
            
        }
    }
}
