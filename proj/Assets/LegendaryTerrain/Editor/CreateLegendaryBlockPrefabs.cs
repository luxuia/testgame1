using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace LegendaryTerrain.Editor
{
    /// <summary>
    /// 创建传奇地形所需的地块 Prefab、材质和贴图。
    /// 织梦岛风格：地形/水用方块+层次贴图，树/房子用低多边形。
    /// 菜单: Tools > Legendary > Create Block Prefabs
    /// </summary>
    public static class CreateLegendaryBlockPrefabs
    {
        private const string BasePath = "Assets/LegendaryTerrain";
        private const string TexturesPath = BasePath + "/Textures";
        private const string MaterialsPath = BasePath + "/Materials";
        private const string PrefabsPath = BasePath + "/Prefabs";
        private const string ResourcesPath = BasePath + "/Resources/LegendaryTerrain";

        private static readonly System.Random Rng = new System.Random(42);

        [MenuItem("Tools/Legendary/Create Block Prefabs")]
        public static void Execute()
        {
            EnsureDirectories();
            CreateTextures();
            AssetDatabase.Refresh();
            CreateMaterials();
            AssetDatabase.Refresh();
            CreatePrefabs();
            AssetDatabase.Refresh();
            Debug.Log("Legendary block prefabs created (织梦岛风格). 贴图→材质→Prefab 已生成并绑定.");
        }

        [MenuItem("Tools/Legendary/Create Monster Prefabs")]
        public static void CreateMonsterPrefabs()
        {
            EnsureFolder(BasePath + "/Resources", "LegendaryTerrain");
            for (int i = 0; i <= 9; i++)
            {
                string prefabPath = $"{ResourcesPath}/Monster_{i}.prefab";
                var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (existing != null) AssetDatabase.DeleteAsset(prefabPath);

                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = $"Monster_{i}";
                go.transform.localScale = Vector3.one * 0.8f;
                go.AddComponent<LegendaryTerrain.MonsterController>();
                go.AddComponent<LegendaryTerrain.DistanceCulling>();

                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                UnityEngine.Object.DestroyImmediate(go);
            }
            UpdateMonsterPrefabConfig();
            AssetDatabase.Refresh();
            Debug.Log("Legendary monster prefabs created (Monster_0..Monster_9).");
        }

        private static void UpdateMonsterPrefabConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<LegendaryTerrain.MonsterPrefabConfig>($"{ResourcesPath}/MonsterPrefabConfig.asset");
            if (config == null) return;
            var so = new SerializedObject(config);
            var entries = so.FindProperty("_entries");
            entries.ClearArray();
            for (int i = 0; i <= 9; i++)
            {
                entries.InsertArrayElementAtIndex(i);
                var elem = entries.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("MonsterIndex").intValue = i;
                elem.FindPropertyRelative("ResourcesPath").stringValue = $"LegendaryTerrain/Monster_{i}";
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureDirectories()
        {
            EnsureFolder(BasePath, "Textures");
            EnsureFolder(BasePath, "Materials");
            EnsureFolder(BasePath, "Prefabs");
            EnsureFolder(BasePath, "Resources");
            EnsureFolder(BasePath + "/Resources", "LegendaryTerrain");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static void CreateTextures()
        {
            CreateGrassTexture("Block_Ground");
            CreateStoneTexture("Block_Wall");
            CreateWoodTexture("Block_Door");
            CreateBridgeTexture("Block_Bridge");
            CreateWaterTexture("Block_Water");
            CreateSolidTexture("Block_SpawnMarker", new Color(1f, 0.35f, 0.35f));
            CreateSolidTexture("Block_Tree", new Color(0.35f, 0.6f, 0.3f));
            CreateSolidTexture("Block_TreeTrunk", new Color(0.45f, 0.35f, 0.25f));
            CreateSolidTexture("Block_House", new Color(0.7f, 0.6f, 0.5f));
            CreateSolidTexture("Character_Player", new Color(0.9f, 0.7f, 0.5f));
        }

        private static void CreateTiledTexture(string name, int w, int h, Func<int, int, Color> generator)
        {
            string path = Path.Combine(Application.dataPath, "LegendaryTerrain", "Textures", $"{name}.png");
            var tex = new Texture2D(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, generator(x, y));
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void CreateGrassTexture(string name)
        {
            var c1 = new Color(0.45f, 0.65f, 0.35f);
            var c2 = new Color(0.5f, 0.7f, 0.4f);
            var c3 = new Color(0.38f, 0.55f, 0.3f);
            CreateTiledTexture(name, 32, 32, (x, y) =>
            {
                int tile = (x / 4) + (y / 4) * 8;
                var baseColor = (tile % 3) switch { 0 => c1, 1 => c2, _ => c3 };
                float noise = (float)(Rng.NextDouble() * 0.08 - 0.04);
                return baseColor + new Color(noise, noise, noise);
            });
        }

        private static void CreateStoneTexture(string name)
        {
            var baseColor = new Color(0.55f, 0.52f, 0.5f);
            var mortar = new Color(0.4f, 0.38f, 0.36f);
            CreateTiledTexture(name, 32, 32, (x, y) =>
            {
                bool isMortar = (x % 8) == 0 || (y % 8) == 0;
                var c = isMortar ? mortar : baseColor;
                float v = (float)(Rng.NextDouble() * 0.06 - 0.03);
                return c + new Color(v, v, v);
            });
        }

        private static void CreateWoodTexture(string name)
        {
            var light = new Color(0.65f, 0.5f, 0.35f);
            var dark = new Color(0.5f, 0.38f, 0.25f);
            CreateTiledTexture(name, 32, 32, (x, y) =>
            {
                int band = y / 4;
                return band % 2 == 0 ? light : dark;
            });
        }

        private static void CreateBridgeTexture(string name)
        {
            var light = new Color(0.5f, 0.42f, 0.32f);
            var dark = new Color(0.4f, 0.33f, 0.25f);
            CreateTiledTexture(name, 32, 32, (x, y) =>
            {
                int band = y / 3;
                return (band % 2 == 0 ? light : dark);
            });
        }

        private static void CreateWaterTexture(string name)
        {
            CreateTiledTexture(name, 32, 32, (x, y) =>
            {
                float cx = (x - 16f) / 16f;
                float cy = (y - 16f) / 16f;
                float dist = Mathf.Sqrt(cx * cx + cy * cy);
                float bright = 1f - dist * 0.15f;
                float wave = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.05f;
                return new Color(0.25f * bright + wave, 0.5f * bright + wave, 0.75f * bright + wave, 0.65f);
            });
        }

        private static void CreateSolidTexture(string name, Color color)
        {
            string path = Path.Combine(Application.dataPath, "LegendaryTerrain", "Textures", $"{name}.png");
            var tex = new Texture2D(16, 16);
            for (int i = 0; i < 256; i++) tex.SetPixel(i % 16, i / 16, color);
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void CreateMaterials()
        {
            CreateMaterial("Mat_Ground", "Block_Ground");
            CreateMaterial("Mat_Wall", "Block_Wall");
            CreateMaterial("Mat_Door", "Block_Door");
            CreateMaterial("Mat_SpawnMarker", "Block_SpawnMarker");
            CreateTransparentMaterial("Mat_Water", "Block_Water");
            CreateMaterial("Mat_Tree", "Block_Tree");
            CreateMaterial("Mat_TreeTrunk", "Block_TreeTrunk");
            CreateMaterial("Mat_House", "Block_House");
            CreateMaterial("Mat_Bridge", "Block_Bridge");
            CreateMaterial("Mat_Character", "Character_Player");
        }

        private static void CreateMaterial(string matName, string texName)
        {
            string matPath = $"{MaterialsPath}/{matName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null) AssetDatabase.DeleteAsset(matPath);

            string texPath = $"{TexturesPath}/{texName}.png";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null) { Debug.LogWarning($"Texture not found: {texPath}"); return; }

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null) mat.shader = Shader.Find("Standard");
            mat.mainTexture = tex;
            mat.name = matName;
            mat.enableInstancing = true;
            AssetDatabase.CreateAsset(mat, matPath);
        }

        private static void CreateTransparentMaterial(string matName, string texName)
        {
            string matPath = $"{MaterialsPath}/{matName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null) AssetDatabase.DeleteAsset(matPath);

            string texPath = $"{TexturesPath}/{texName}.png";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null) { Debug.LogWarning($"Texture not found: {texPath}"); return; }

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null) mat.shader = Shader.Find("Standard");
            mat.mainTexture = tex;
            mat.name = matName;
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.enableInstancing = true;
            AssetDatabase.CreateAsset(mat, matPath);
        }

        private static void CreatePrefabs()
        {
            CreateBlockPrefab("Block_Ground", PrimitiveType.Cube, "Mat_Ground", ResourcesPath);
            CreateBlockPrefab("Block_Wall", PrimitiveType.Cube, "Mat_Wall", ResourcesPath);
            CreateBlockPrefab("Block_Door", PrimitiveType.Cube, "Mat_Door", ResourcesPath);
            CreateBlockPrefab("Block_SpawnMarker", PrimitiveType.Sphere, "Mat_SpawnMarker", ResourcesPath);
            CreateBlockPrefab("Block_Water", PrimitiveType.Cube, "Mat_Water", ResourcesPath);
            CreateTreePrefab();
            CreateHousePrefab();
            CreateBlockPrefab("Block_Bridge", PrimitiveType.Cube, "Mat_Bridge", ResourcesPath);
            CreateCharacterPrefab();
            CreateMonsterPrefabConfig();
        }

        private static void CreateMonsterPrefabConfig()
        {
            string path = $"{ResourcesPath}/MonsterPrefabConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<MonsterPrefabConfig>(path) != null) return;
            var config = ScriptableObject.CreateInstance<MonsterPrefabConfig>();
            AssetDatabase.CreateAsset(config, path);
        }

        private static void CreateBlockPrefab(string prefabName, PrimitiveType primitive, string matName, string outputDir)
        {
            string prefabPath = $"{outputDir}/{prefabName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) AssetDatabase.DeleteAsset(prefabPath);

            var go = GameObject.CreatePrimitive(primitive);
            go.name = prefabName;

            string matPath = $"{MaterialsPath}/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat != null)
                go.GetComponent<Renderer>().sharedMaterial = mat;
            else
                Debug.LogWarning($"Material not found: {matPath}, prefab {prefabName} will use default.");

            if (primitive == PrimitiveType.Sphere)
                go.transform.localScale = Vector3.one * 0.5f;

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void CreateTreePrefab()
        {
            string prefabPath = $"{ResourcesPath}/Block_Tree.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) AssetDatabase.DeleteAsset(prefabPath);

            var root = new GameObject("Block_Tree");

            var trunkMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Mat_TreeTrunk.mat");
            var treeMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Mat_Tree.mat");

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(root.transform);
            trunk.transform.localPosition = new Vector3(0, 0.3f, 0);
            trunk.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            if (trunkMat != null) trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;

            var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "Foliage";
            foliage.transform.SetParent(root.transform);
            foliage.transform.localPosition = new Vector3(0, 0.7f, 0);
            foliage.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            if (treeMat != null) foliage.GetComponent<Renderer>().sharedMaterial = treeMat;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void CreateHousePrefab()
        {
            string prefabPath = $"{ResourcesPath}/Block_House.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) AssetDatabase.DeleteAsset(prefabPath);

            var root = new GameObject("Block_House");

            var houseMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Mat_House.mat");

            var base_ = GameObject.CreatePrimitive(PrimitiveType.Cube);
            base_.name = "Base";
            base_.transform.SetParent(root.transform);
            base_.transform.localPosition = new Vector3(0, 0.25f, 0);
            base_.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
            if (houseMat != null) base_.GetComponent<Renderer>().sharedMaterial = houseMat;

            var roof = CreatePyramidMesh();
            roof.name = "Roof";
            roof.transform.SetParent(root.transform);
            roof.transform.localPosition = new Vector3(0, 0.65f, 0);
            roof.transform.localScale = Vector3.one * 0.9f;
            if (houseMat != null) roof.GetComponent<Renderer>().sharedMaterial = houseMat;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static GameObject CreatePyramidMesh()
        {
            var go = new GameObject();
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();

            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(0, 0.4f, 0),
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, -0.5f)
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 1 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mf.sharedMesh = mesh;

            return go;
        }

        private static void CreateCharacterPrefab()
        {
            string prefabPath = $"{ResourcesPath}/Character_Player.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) AssetDatabase.DeleteAsset(prefabPath);

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Character_Player";
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Mat_Character.mat");
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            go.AddComponent<LegendaryTerrain.LegendaryCharacterController>();

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
