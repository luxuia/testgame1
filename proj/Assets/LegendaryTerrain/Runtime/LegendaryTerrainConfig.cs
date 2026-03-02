using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 传奇地形 Prefab 资源路径。Prefab 需放在 Resources/LegendaryTerrain/ 下。
    /// </summary>
    public static class LegendaryTerrainConfig
    {
        public const string ResourcesFolder = "LegendaryTerrain";

        public const string BlockGround = "LegendaryTerrain/Block_Ground";
        public const string BlockWall = "LegendaryTerrain/Block_Wall";
        public const string BlockDoor = "LegendaryTerrain/Block_Door";
        public const string BlockSpawnMarker = "LegendaryTerrain/Block_SpawnMarker";
        public const string BlockWater = "LegendaryTerrain/Block_Water";
        public const string BlockTree = "LegendaryTerrain/Block_Tree";
        public const string BlockHouse = "LegendaryTerrain/Block_House";
        public const string BlockBridge = "LegendaryTerrain/Block_Bridge";
        public const string CharacterPlayer = "LegendaryTerrain/Character_Player";

        public static GameObject LoadBlockGround() => Resources.Load<GameObject>(BlockGround);
        public static GameObject LoadBlockWall() => Resources.Load<GameObject>(BlockWall);
        public static GameObject LoadBlockDoor() => Resources.Load<GameObject>(BlockDoor);
        public static GameObject LoadBlockSpawnMarker() => Resources.Load<GameObject>(BlockSpawnMarker);
        public static GameObject LoadBlockWater() => Resources.Load<GameObject>(BlockWater);
        public static GameObject LoadBlockTree() => Resources.Load<GameObject>(BlockTree);
        public static GameObject LoadBlockHouse() => Resources.Load<GameObject>(BlockHouse);
        public static GameObject LoadBlockBridge() => Resources.Load<GameObject>(BlockBridge);
        public static GameObject LoadCharacterPlayer() => Resources.Load<GameObject>(CharacterPlayer);

        public static Material LoadMaterialForBlock(string blockType)
        {
            var prefab = blockType switch
            {
                "Ground" => LoadBlockGround(),
                "Wall" => LoadBlockWall(),
                "Door" => LoadBlockDoor(),
                "Water" => LoadBlockWater(),
                "Bridge" => LoadBlockBridge(),
                _ => null
            };
            return prefab?.GetComponentInChildren<Renderer>()?.sharedMaterial;
        }
    }
}
