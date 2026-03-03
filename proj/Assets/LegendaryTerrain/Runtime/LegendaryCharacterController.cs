using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 玩家角色移动控制，与方块坐标对齐。支持攻击射线检测可破坏块。
    /// </summary>
    public class LegendaryCharacterController : MonoBehaviour
    {
        public float MoveSpeed = 5f;
        [Tooltip("攻击伤害")]
        public int AttackDamage = 25;
        [Tooltip("攻击射线长度（格数）")]
        public float AttackRange = 2f;

        public Vector2Int GridPosition => new Vector2Int(
            Mathf.RoundToInt(transform.position.x / TerrainGenerator.BlockSize),
            Mathf.RoundToInt(transform.position.z / TerrainGenerator.BlockSize));

        private Vector3 _lastMoveDir = new Vector3(0, 0, 1);

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (h != 0 || v != 0)
            {
                var dir = new Vector3(h, 0, v).normalized;
                _lastMoveDir = dir;
                transform.position += dir * (MoveSpeed * Time.deltaTime);
            }

            if (Input.GetButtonDown("Fire1"))
                TryAttack();
        }

        private void TryAttack()
        {
            var origin = transform.position + Vector3.up * 0.5f;
            var distance = AttackRange * TerrainGenerator.BlockSize;

            if (Physics.Raycast(origin, _lastMoveDir, out var hit, distance))
            {
                var block = hit.collider.GetComponent<BlockController>();
                if (block != null && BlockController.IsDestructible(block.BlockType))
                    block.TakeDamage(AttackDamage);
            }
        }
    }
}
