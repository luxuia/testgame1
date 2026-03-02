using UnityEditor;
using UnityEngine;
using System.IO;
using LegendaryTerrain;

namespace LegendaryTerrain.Editor
{
    public static class LegendaryTerrainGenerator
    {
        private const float BlockSize = 1f;

        [MenuItem("Tools/Legendary/Create Scene Manager")]
        public static void CreateSceneManager()
        {
            var existing = Object.FindObjectOfType<LegendaryTerrain.LegendarySceneManager>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            var go = new GameObject("SceneManager");
            go.AddComponent<LegendaryTerrain.LegendarySceneManager>();
            Selection.activeGameObject = go;
            UnityEngine.Debug.Log("SceneManager created. Run Tools > Legendary > Create Block Prefabs first if prefabs are missing.");
        }

        [MenuItem("Tools/Legendary/Test MirDB Parser")]
        public static void TestMirDBParser()
        {
            string mirDbPath = Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Server.MirDB");
            if (!File.Exists(mirDbPath))
            {
                UnityEngine.Debug.LogError("Server.MirDB not found. Run Download Crystal Data first.");
                return;
            }
            var mapInfos = MirDBParser.Parse(mirDbPath);
            UnityEngine.Debug.Log($"[MirDB] Parsed {mapInfos.Count} MapInfo(s).");
            if (mapInfos.Count > 0)
            {
                var first = mapInfos[0];
                UnityEngine.Debug.Log($"[MirDB] First map: Index={first.Index}, FileName={first.FileName}, Title={first.Title}, Respawns={first.Respawns.Count}");
            }
        }

        [MenuItem("Tools/Legendary/Generate Terrain from Map")]
        public static void Generate()
        {
            string mapsPath = Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Maps");
            if (!Directory.Exists(mapsPath))
            {
                UnityEngine.Debug.LogError("Run Download Crystal Data first.");
                return;
            }

            string mapFile = Path.Combine(mapsPath, "0.map");
            if (!File.Exists(mapFile)) mapFile = FindFirstMap(mapsPath);
            if (mapFile == null)
            {
                UnityEngine.Debug.LogError("No .map files found.");
                return;
            }

            var grid = Mir2MapParser.ParseFile(mapFile);
            var root = new GameObject("LegendaryTerrain");
            root.transform.position = Vector3.zero;

            for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
            {
                var cell = grid.Get(x, y);
                string blockType;
                int z = 0;
                if (cell.FishingCell)
                {
                    blockType = "Water";
                    z = 0;
                }
                else if (cell.Attribute == CellAttribute.Walk)
                {
                    blockType = "Ground";
                }
                else if (cell.Attribute == CellAttribute.HighWall || cell.Attribute == CellAttribute.LowWall)
                {
                    blockType = "Wall";
                    z = 1;
                }
                else if (cell.Attribute == CellAttribute.Door)
                {
                    blockType = "Door";
                }
                else
                {
                    blockType = "Ground";
                }
                CreateBlock(root.transform, x, y, z, blockType);

                // 仅在可行走地面上放置 Middle/Front 占位物（树、房子、桥等）
                if (cell.Attribute == CellAttribute.Walk && blockType == "Ground")
                {
                    string overlay = TileIndexMapper.GetOverlayBlockType(cell);
                    if (!string.IsNullOrEmpty(overlay))
                        CreateBlock(root.transform, x, y, 1, overlay, $"{overlay}_{x}_{y}");
                }
            }

            string mirDbPath = Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Server.MirDB");
            var mapInfos = MirDBParser.Parse(mirDbPath);
            UnityEngine.Debug.Log($"[MirDB] Parsed {mapInfos.Count} MapInfo(s). First map Respawns: {(mapInfos.Count > 0 ? mapInfos[0].Respawns.Count : 0)}");
            string currentMapName = Path.GetFileNameWithoutExtension(mapFile);
            foreach (var info in mapInfos)
            {
                if (info.FileName != currentMapName) continue;
                foreach (var r in info.Respawns)
                {
                    CreateBlock(root.transform, r.Location.x, r.Location.y, 1, "SpawnMarker", $"Spawn_Monster{r.MonsterIndex}");
                }
            }

            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();
            UnityEngine.Debug.Log($"Generated terrain {grid.Width}x{grid.Height} from {Path.GetFileName(mapFile)}");
        }

        private static string FindFirstMap(string dir)
        {
            foreach (var f in Directory.GetFiles(dir, "*.map", SearchOption.AllDirectories))
                return f;
            return null;
        }

        private static void CreateBlock(Transform parent, int x, int y, int z, string blockType, string customName = null)
        {
            var prefab = LoadBlockPrefab(blockType);
            GameObject block;
            if (prefab != null)
            {
                block = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                block.transform.SetParent(parent);
            }
            else
            {
                block = GameObject.CreatePrimitive(blockType == "SpawnMarker" ? PrimitiveType.Sphere : PrimitiveType.Cube);
                block.GetComponent<Renderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
            }
            block.name = !string.IsNullOrEmpty(customName) ? customName : $"{blockType}_{x}_{y}";
            block.transform.localPosition = new Vector3(x * BlockSize, z * BlockSize, y * BlockSize);
            block.transform.localRotation = Quaternion.identity;
            block.transform.localScale = blockType == "SpawnMarker" ? Vector3.one * 0.5f : Vector3.one;
        }

        private static GameObject LoadBlockPrefab(string blockType)
        {
            return blockType switch
            {
                "Ground" => LegendaryTerrainConfig.LoadBlockGround(),
                "Wall" => LegendaryTerrainConfig.LoadBlockWall(),
                "Door" => LegendaryTerrainConfig.LoadBlockDoor(),
                "SpawnMarker" => LegendaryTerrainConfig.LoadBlockSpawnMarker(),
                "Water" => LegendaryTerrainConfig.LoadBlockWater(),
                "Tree" => LegendaryTerrainConfig.LoadBlockTree(),
                "House" => LegendaryTerrainConfig.LoadBlockHouse(),
                "Bridge" => LegendaryTerrainConfig.LoadBlockBridge(),
                _ => null
            };
        }
    }
}
