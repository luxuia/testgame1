using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 玩家角色移动控制，与方块坐标对齐。
    /// </summary>
    public class LegendaryCharacterController : MonoBehaviour
    {
        public float MoveSpeed = 5f;
        public Vector2Int GridPosition => new Vector2Int(
            Mathf.RoundToInt(transform.position.x / TerrainGenerator.BlockSize),
            Mathf.RoundToInt(transform.position.z / TerrainGenerator.BlockSize));

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (h != 0 || v != 0)
            {
                var dir = new Vector3(h, 0, v).normalized;
                transform.position += dir * (MoveSpeed * Time.deltaTime);
            }
        }
    }
}
