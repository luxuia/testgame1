using UnityEngine;
using System.IO;

namespace LegendaryTerrain
{
    /// <summary>
    /// 场景管理入口，协调地形、怪物、角色的生命周期。
    /// </summary>
    public class LegendarySceneManager : MonoBehaviour
    {
        [SerializeField] private string _mapFileName = "0";
        [SerializeField] private bool _useChunkedTerrain = true;

        private BlockRegistry _blockRegistry;
        private TerrainChunkManager _chunkManager;
        private GameObject _terrainRoot;
        private GameObject _monstersRoot;
        private GameObject _characterRoot;

        private void Awake()
        {
            _blockRegistry = new BlockRegistry();

            var terrainGo = new GameObject("Terrain");
            terrainGo.transform.SetParent(transform);
            _terrainRoot = terrainGo;

            var monstersGo = new GameObject("Monsters");
            monstersGo.transform.SetParent(transform);
            _monstersRoot = monstersGo;

            var characterGo = new GameObject("Character");
            characterGo.transform.SetParent(transform);
            _characterRoot = characterGo;
        }

        private void Start()
        {
            if (_useChunkedTerrain)
            {
                _chunkManager = _terrainRoot.AddComponent<TerrainChunkManager>();
                _chunkManager.MapFileName = _mapFileName;
                _blockRegistry = _chunkManager.GetBlockRegistry();
            }
            else
            {
                var terrainGenerator = new TerrainGenerator(_blockRegistry);
                var root = terrainGenerator.Generate(_mapFileName);
                if (root != null)
                {
                    root.transform.SetParent(_terrainRoot.transform);
                    root.transform.localPosition = Vector3.zero;
                }
            }

            var mapInfo = GetMapInfo();
            if (mapInfo != null)
            {
                var spawner = new MonsterSpawner(_blockRegistry);
                spawner.Spawn(mapInfo, _monstersRoot.transform);
            }

            SpawnCharacter();
        }

        private Mir2MapInfo GetMapInfo()
        {
            string mirDbPath = Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Server.MirDB");
            if (!File.Exists(mirDbPath)) return null;
            var infos = MirDBParser.Parse(mirDbPath);
            foreach (var info in infos)
            {
                if (info.FileName == _mapFileName) return info;
            }
            return null;
        }

        private void SpawnCharacter()
        {
            var prefab = LegendaryTerrainConfig.LoadCharacterPlayer();
            if (prefab == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = "Character_Player";
                go.transform.SetParent(_characterRoot.transform);
                go.transform.localPosition = GetSpawnPosition();
                go.AddComponent<LegendaryCharacterController>();
                return;
            }
            var instance = Object.Instantiate(prefab, _characterRoot.transform);
            instance.transform.position = GetSpawnPosition();
            if (instance.GetComponent<LegendaryCharacterController>() == null)
                instance.AddComponent<LegendaryCharacterController>();
        }

        private Vector3 GetSpawnPosition()
        {
            return new Vector3(0, 0.5f, 0);
        }
    }
}
