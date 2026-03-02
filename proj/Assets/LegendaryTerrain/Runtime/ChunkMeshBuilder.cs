using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 将不可破坏块按材质合并为合批网格，减少 Draw Call。
    /// </summary>
    public static class ChunkMeshBuilder
    {
        private static Mesh _cubeMesh;

        private static Mesh GetCubeMesh()
        {
            if (_cubeMesh != null) return _cubeMesh;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _cubeMesh = go.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(go);
            return _cubeMesh;
        }

        /// <summary>
        /// 按材质分组创建合并网格。返回 (Material, Mesh) 列表，每项对应一个 Draw Call。
        /// </summary>
        public static List<(Material mat, Mesh mesh)> BuildCombinedMeshes(
            IReadOnlyList<(Vector3 position, string blockType)> blocks,
            System.Func<string, Material> getMaterial)
        {
            var byMaterial = new Dictionary<Material, List<Matrix4x4>>();
            var cubeMesh = GetCubeMesh();

            foreach (var (pos, blockType) in blocks)
            {
                var mat = getMaterial(blockType);
                if (mat == null) continue;
                if (!byMaterial.ContainsKey(mat))
                    byMaterial[mat] = new List<Matrix4x4>();
                byMaterial[mat].Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
            }

            var result = new List<(Material, Mesh)>();
            foreach (var kv in byMaterial)
            {
                var combineInstances = new CombineInstance[kv.Value.Count];
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    combineInstances[i] = new CombineInstance
                    {
                        mesh = cubeMesh,
                        transform = kv.Value[i]
                    };
                }
                var combined = new Mesh();
                combined.CombineMeshes(combineInstances, true, true);
                result.Add((kv.Key, combined));
            }
            return result;
        }
    }
}
