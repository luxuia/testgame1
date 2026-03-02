using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 单块地形数据，包含根物体和可破坏块列表。
    /// </summary>
    public class TerrainChunk
    {
        public Vector2Int ChunkCoord;
        public GameObject Root;
        public List<BlockController> Destructibles = new List<BlockController>();

        public void Destroy()
        {
            if (Root != null)
            {
                Object.Destroy(Root);
                Root = null;
            }
            Destructibles.Clear();
        }
    }
}
