using UnityEditor;
using UnityEngine;
using System.IO;
using LegendaryTerrain;

namespace LegendaryTerrain.Editor
{
    public static class LegendaryTerrainGenerator
    {
        private const float BlockSize = 1f;

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
                if (cell.Attribute == CellAttribute.Walk)
                {
                    CreateBlock(root.transform, x, y, 0, "Ground");
                }
                else if (cell.Attribute == CellAttribute.HighWall || cell.Attribute == CellAttribute.LowWall)
                {
                    CreateBlock(root.transform, x, y, 1, "Wall");
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

        private static void CreateBlock(Transform parent, int x, int y, int z, string tag)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"{tag}_{x}_{y}";
            cube.transform.SetParent(parent);
            cube.transform.localPosition = new Vector3(x * BlockSize, z * BlockSize, y * BlockSize);
            cube.GetComponent<Renderer>().sharedMaterial = tag == "Ground"
                ? AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat")
                : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
        }
    }
}
