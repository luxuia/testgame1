using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace LegendaryTerrain
{
    /// <summary>
    /// 运行时地形生成器。从 .map 解析并生成方块地形，可破坏块挂载 BlockController。
    /// </summary>
    public class TerrainGenerator
    {
        public const float BlockSize = 1f;

        private readonly BlockRegistry _registry;
        private Mir2CellGrid _cachedGrid;

        public TerrainGenerator(BlockRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>
        /// 加载并缓存地图数据，返回 grid。用于分块生成。
        /// </summary>
        public Mir2CellGrid LoadMap(string mapFileName)
        {
            string mapsPath = Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Maps");
            string mapFile = Path.Combine(mapsPath, $"{mapFileName}.map");
            if (!File.Exists(mapFile))
                mapFile = FindFirstMap(mapsPath);
            if (mapFile == null)
            {
                Debug.LogError("[TerrainGenerator] No .map files found.");
                return null;
            }
            _cachedGrid = Mir2MapParser.ParseFile(mapFile);
            return _cachedGrid;
        }

        /// <summary>
        /// 生成单块地形，用于分块加载。useMeshCombining 为 true 时合并不可破坏块以减少 Draw Call。
        /// </summary>
        public TerrainChunk GenerateChunk(Vector2Int chunkCoord, int chunkSize, Transform parent, bool useMeshCombining = true)
        {
            if (_cachedGrid == null) return null;

            int minX = chunkCoord.x * chunkSize;
            int minY = chunkCoord.y * chunkSize;
            int maxX = Mathf.Min(minX + chunkSize, _cachedGrid.Width);
            int maxY = Mathf.Min(minY + chunkSize, _cachedGrid.Height);

            var root = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
            root.transform.SetParent(parent);
            root.transform.localPosition = Vector3.zero;

            var chunk = new TerrainChunk { ChunkCoord = chunkCoord, Root = root };
            var staticBlocks = new List<(Vector3 position, string blockType)>();

            for (int x = minX; x < maxX; x++)
            for (int y = minY; y < maxY; y++)
            {
                var cell = _cachedGrid.Get(x, y);
                string blockType;
                int z = 0;
                if (cell.FishingCell) blockType = "Water";
                else if (cell.Attribute == CellAttribute.Walk) blockType = "Ground";
                else if (cell.Attribute == CellAttribute.HighWall) { blockType = "Wall"; z = 1; }
                else if (cell.Attribute == CellAttribute.LowWall) { blockType = TileIndexMapper.GetBlockTypeForLowWall(cell); z = 1; }
                else if (cell.Attribute == CellAttribute.Door) blockType = "Door";
                else blockType = "Ground";

                var bt = StringToBlockType(blockType);
                var pos = new Vector3(x * BlockSize, z * BlockSize, y * BlockSize);

                if (BlockController.IsDestructible(bt))
                {
                    var block = CreateBlock(root.transform, x, y, z, blockType, null);
                    var bc = block.GetComponent<BlockController>();
                    if (bc != null) chunk.Destructibles.Add(bc);
                }
                else if (useMeshCombining)
                {
                    staticBlocks.Add((pos, blockType));
                }
                else
                {
                    CreateBlock(root.transform, x, y, z, blockType, null);
                }

                if (cell.Attribute == CellAttribute.Walk && blockType == "Ground")
                {
                    string overlay = TileIndexMapper.GetOverlayBlockType(cell);
                    if (!string.IsNullOrEmpty(overlay))
                    {
                        var ot = StringToBlockType(overlay);
                        var overlayPos = new Vector3(x * BlockSize, BlockSize, y * BlockSize);
                        if (BlockController.IsDestructible(ot))
                        {
                            var overlayBlock = CreateBlock(root.transform, x, y, 1, overlay, $"{overlay}_{x}_{y}");
                            var bc = overlayBlock.GetComponent<BlockController>();
                            if (bc != null) chunk.Destructibles.Add(bc);
                        }
                        else if (useMeshCombining)
                        {
                            staticBlocks.Add((overlayPos, overlay));
                        }
                        else
                        {
                            CreateBlock(root.transform, x, y, 1, overlay, $"{overlay}_{x}_{y}");
                        }
                    }
                }
            }

            if (useMeshCombining && staticBlocks.Count > 0)
            {
                var combined = ChunkMeshBuilder.BuildCombinedMeshes(staticBlocks, LegendaryTerrainConfig.LoadMaterialForBlock);
                foreach (var (mat, mesh) in combined)
                {
                    var go = new GameObject("CombinedMesh");
                    go.transform.SetParent(root.transform);
                    go.transform.localPosition = Vector3.zero;
                    var mf = go.AddComponent<MeshFilter>();
                    mf.sharedMesh = mesh;
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = mat;
                }
            }

            return chunk;
        }

        private GameObject CreateBlock(Transform parent, int x, int y, int z, string blockType, string customName)
        {
            var prefab = LoadBlockPrefab(blockType);
            GameObject block;
            if (prefab != null)
                block = Object.Instantiate(prefab, parent);
            else
            {
                block = GameObject.CreatePrimitive(blockType == "SpawnMarker" ? PrimitiveType.Sphere : PrimitiveType.Cube);
                block.transform.SetParent(parent);
            }
            block.name = !string.IsNullOrEmpty(customName) ? customName : $"{blockType}_{x}_{y}";
            block.transform.localPosition = new Vector3(x * BlockSize, z * BlockSize, y * BlockSize);
            block.transform.localRotation = Quaternion.identity;
            block.transform.localScale = blockType == "SpawnMarker" ? Vector3.one * 0.5f : Vector3.one;

            var blockTypeEnum = StringToBlockType(blockType);
            if (BlockController.IsDestructible(blockTypeEnum))
            {
                var bc = block.GetComponent<BlockController>();
                if (bc == null) bc = block.AddComponent<BlockController>();
                bc.Init(_registry, new Vector2Int(x, y), blockTypeEnum);
            }
            return block;
        }

        /// <summary>
        /// 生成完整地形，返回根 GameObject。
        /// </summary>
        public GameObject Generate(string mapFileName)
        {
            string mapsPath = Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Maps");
            string mapFile = Path.Combine(mapsPath, $"{mapFileName}.map");
            if (!File.Exists(mapFile))
                mapFile = FindFirstMap(mapsPath);
            if (mapFile == null)
            {
                Debug.LogError("[TerrainGenerator] No .map files found.");
                return null;
            }

            _cachedGrid = Mir2MapParser.ParseFile(mapFile);
            var grid = _cachedGrid;
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
                else if (cell.Attribute == CellAttribute.HighWall)
                {
                    blockType = "Wall";
                    z = 1;
                }
                else if (cell.Attribute == CellAttribute.LowWall)
                {
                    blockType = TileIndexMapper.GetBlockTypeForLowWall(cell);
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
                CreateBlock(root.transform, x, y, z, blockType, null);

                if (cell.Attribute == CellAttribute.Walk && blockType == "Ground")
                {
                    string overlay = TileIndexMapper.GetOverlayBlockType(cell);
                    if (!string.IsNullOrEmpty(overlay))
                        CreateBlock(root.transform, x, y, 1, overlay, $"{overlay}_{x}_{y}");
                }
            }

            Debug.Log($"[TerrainGenerator] Generated {grid.Width}x{grid.Height} from {Path.GetFileName(mapFile)}");
            return root;
        }

        public static string FindFirstMap(string dir)
        {
            if (!Directory.Exists(dir)) return null;
            foreach (var f in Directory.GetFiles(dir, "*.map", SearchOption.AllDirectories))
                return f;
            return null;
        }

        private static BlockType StringToBlockType(string s)
        {
            return s switch
            {
                "Ground" => BlockType.Ground,
                "Wall" => BlockType.Wall,
                "Door" => BlockType.Door,
                "Water" => BlockType.Water,
                "Tree" => BlockType.Tree,
                "House" => BlockType.House,
                "Bridge" => BlockType.Bridge,
                "SpawnMarker" => BlockType.SpawnMarker,
                _ => BlockType.Ground
            };
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
