using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
	public int stage;
	public List<SpotData> spotDatas;

	public static DataManager instance;

    private void Awake()
    {
		instance = this;
    }

    [System.Serializable]
	public class SpotData
	{
		public ClientEnum.SpotType type;
		public int level;
		public string weapon;
	}
}
