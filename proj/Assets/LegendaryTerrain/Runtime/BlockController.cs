using UnityEngine;

namespace LegendaryTerrain
{
    public enum BlockType
    {
        Ground,
        Wall,
        Door,
        Water,
        Tree,
        House,
        Bridge,
        SpawnMarker
    }

    /// <summary>
    /// 可破坏方块组件。挂载到 Wall、Tree、House、Bridge。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BlockController : MonoBehaviour
    {
        public BlockType BlockType;
        public Vector2Int GridPosition;
        public int Health;
        public int MaxHealth = 100;

        private BlockRegistry _registry;

        public void Init(BlockRegistry registry, Vector2Int pos, BlockType type, int maxHealth = 100)
        {
            _registry = registry;
            GridPosition = pos;
            BlockType = type;
            MaxHealth = maxHealth;
            Health = maxHealth;
            _registry.Register(pos, this);
        }

        public void TakeDamage(int amount)
        {
            if (_registry == null) return;
            Health -= amount;
            if (Health <= 0)
            {
                _registry.Unregister(GridPosition);
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            _registry?.Unregister(GridPosition);
        }

        public static bool IsDestructible(BlockType type)
        {
            return type == BlockType.Wall || type == BlockType.Tree ||
                   type == BlockType.House || type == BlockType.Bridge;
        }
    }
}
