using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 传送点触发器占位。玩家进入时调用 LegendarySceneManager.LoadMap。
    /// 挂载到场景中的传送点 GameObject，需带 Collider（IsTrigger=true）。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MapTransitionTrigger : MonoBehaviour
    {
        [Tooltip("目标地图文件名（不含 .map）")]
        public string TargetMapFileName = "0";

        [Tooltip("目标出生点坐标（地图格子）")]
        public Vector2Int TargetLocation;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<LegendaryCharacterController>() == null) return;

            var manager = FindObjectOfType<LegendarySceneManager>();
            if (manager != null)
                manager.LoadMap(TargetMapFileName, TargetLocation);
        }
    }
}
