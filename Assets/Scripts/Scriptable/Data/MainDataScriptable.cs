using System.Collections.Generic;
using UnityEngine;

namespace JsonClass
{
    public partial class MainDataScriptable : ScriptableObject
    {
        public List<MainData> mainData = new List<MainData>();
    }

    [System.Serializable]
    public partial class MainData
    {
        public int stageMonsterCount;
        public int levelPerPlayerAttack;
        public int levelPerPlayerHp;
        public int levelPerEnemyHp;
        public int levelPerEnemyAttack;
    }

}
