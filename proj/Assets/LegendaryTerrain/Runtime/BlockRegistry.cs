using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 方块坐标索引，用于查询、寻路、可破坏块管理。
    /// </summary>
    public class BlockRegistry
    {
        private readonly Dictionary<Vector2Int, BlockController> _blocks = new Dictionary<Vector2Int, BlockController>();

        public void Register(Vector2Int pos, BlockController block)
        {
            _blocks[pos] = block;
        }

        public void Unregister(Vector2Int pos)
        {
            _blocks.Remove(pos);
        }

        public BlockController GetBlockAt(Vector2Int pos)
        {
            return _blocks.TryGetValue(pos, out var block) ? block : null;
        }

        public bool HasBlockAt(Vector2Int pos)
        {
            return _blocks.ContainsKey(pos);
        }

        public void Clear()
        {
            _blocks.Clear();
        }
    }
}
