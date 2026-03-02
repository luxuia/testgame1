using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 按 RespawnInfo 在刷新点实例化怪物 Prefab。
    /// </summary>
    public class MonsterSpawner
    {
        private readonly BlockRegistry _registry;

        public MonsterSpawner(BlockRegistry registry)
        {
            _registry = registry;
        }

        public void Spawn(Mir2MapInfo mapInfo, Transform parent)
        {
            if (mapInfo?.Respawns == null) return;

            var config = Resources.Load<MonsterPrefabConfig>("LegendaryTerrain/MonsterPrefabConfig");

            foreach (var r in mapInfo.Respawns)
            {
                var prefab = config != null ? config.GetPrefab(r.MonsterIndex) : null;
                GameObject monster;
                if (prefab != null)
                {
                    monster = Object.Instantiate(prefab, parent);
                }
                else
                {
                    monster = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    monster.transform.SetParent(parent);
                    monster.transform.localScale = Vector3.one * 0.8f;
                }
                monster.name = $"Monster_{r.MonsterIndex}_{r.Location.x}_{r.Location.y}";
                monster.transform.position = new Vector3(r.Location.x * TerrainGenerator.BlockSize, 0.5f, r.Location.y * TerrainGenerator.BlockSize);

                var mc = monster.GetComponent<MonsterController>();
                if (mc == null)
                    mc = monster.AddComponent<MonsterController>();
                mc.MonsterIndex = r.MonsterIndex;
                mc.RespawnInfo = r;

                if (monster.GetComponent<DistanceCulling>() == null)
                    monster.AddComponent<DistanceCulling>();
            }
        }
    }
}
