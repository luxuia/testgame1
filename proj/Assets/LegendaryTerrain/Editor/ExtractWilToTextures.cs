using UnityEditor;
using UnityEngine;
using System.IO;

namespace LegendaryTerrain.Editor
{
    /// <summary>
    /// 从传奇 Tiles.wil 提取地表贴图到 StreamingAssets/LegendaryData/Textures/。
    /// 菜单: Tools > Legendary > Extract WIL to Textures
    /// </summary>
    public static class ExtractWilToTextures
    {
        private static string LegacyTexturesPath => Path.Combine(Application.dataPath, "StreamingAssets", "LegendaryData", "Textures");

        [MenuItem("Tools/Legendary/Extract WIL to Textures")]
        public static void Execute()
        {
            string wilPath = EditorUtility.OpenFilePanel("选择 Tiles.wil", "", "wil");
            if (string.IsNullOrEmpty(wilPath)) return;

            if (!WilParser.TryParse(wilPath, out var data))
            {
                EditorUtility.DisplayDialog("解析失败", "无法解析 WIL/WIX 文件，请确保 Tiles.wil 与 Tiles.wix 在同一目录。", "确定");
                return;
            }

            Directory.CreateDirectory(LegacyTexturesPath);

            // 地表瓦片索引 0 作为 Block_Ground
            if (data.Positions.Length > 0)
            {
                var ground = WilParser.ExtractImage(data, 0);
                if (ground != null)
                {
                    string groundPath = Path.Combine(LegacyTexturesPath, "Block_Ground.png");
                    File.WriteAllBytes(groundPath, ground.EncodeToPNG());
                    Object.DestroyImmediate(ground);
                    Debug.Log("[Legendary] 已导出 Block_Ground.png（地表索引 0）");
                }
            }

            // 可选：导出多瓦片图集供后续扩展
            int tileCount = Mathf.Min(16, data.Positions.Length);
            var atlas = WilParser.CreateTileAtlas(data, 0, tileCount, 32);
            if (atlas != null)
            {
                string atlasPath = Path.Combine(LegacyTexturesPath, "Tiles_Atlas.png");
                File.WriteAllBytes(atlasPath, atlas.EncodeToPNG());
                Object.DestroyImmediate(atlas);
                Debug.Log($"[Legendary] 已导出 Tiles_Atlas.png（{tileCount} 张瓦片）");
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", $"贴图已导出到:\n{LegacyTexturesPath}\n\n请执行 Tools > Legendary > Create Block Prefabs 以应用。", "确定");
        }
    }
}
