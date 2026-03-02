using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 怪物占位组件，挂载到怪物 Prefab 上。后续可扩展 AI、血量等。
    /// </summary>
    public class MonsterController : MonoBehaviour
    {
        public int MonsterIndex;
        public Mir2RespawnInfo RespawnInfo;
    }
}
