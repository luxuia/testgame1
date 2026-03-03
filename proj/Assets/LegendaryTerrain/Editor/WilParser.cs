using System;
using System.IO;
using UnityEngine;

namespace LegendaryTerrain.Editor
{
    /// <summary>
    /// 解析传奇 WIL/WIX 文件，导出贴图。基于 XadillaX/wil-parser 格式。
    /// WIX: 44 字节标题 + 4 字节图片数 + 每图 4 字节偏移量。
    /// WIL: 40 字节标题 + 4 字节 count + 4 字节 colorCount + 4 字节 paletteSize + 调色板 + 图片数据。
    /// 每张图片: 8 字节 (宽2 高2 px2 py2) + width*height 字节调色板索引。
    /// </summary>
    public static class WilParser
    {
        private const int WixTitleSize = 44;
        private const int WixHeaderSize = WixTitleSize + 4;
        private const int WilTitleSize = 40;
        private const int WilCountOffset = 44;
        private const int WilColorCountOffset = 48;
        private const int WilPaletteSizeOffset = 52;
        private const int WilPaletteOffset = 56;
        private const int ImageInfoSize = 8;

        public static bool TryParse(string wilPath, out WilData data)
        {
            data = null;
            string wixPath = Path.Combine(Path.GetDirectoryName(wilPath), Path.GetFileNameWithoutExtension(wilPath) + ".wix");
            if (!File.Exists(wixPath))
            {
                wixPath = Path.ChangeExtension(wilPath, ".wix");
            }
            if (!File.Exists(wilPath) || !File.Exists(wixPath))
                return false;

            try
            {
                byte[] wixBytes = File.ReadAllBytes(wixPath);
                byte[] wilBytes = File.ReadAllBytes(wilPath);

                int indexCount = BitConverter.ToInt32(wixBytes, WixTitleSize);
                if (indexCount < 0 || indexCount > 100000)
                    return false;

                int[] positions = new int[indexCount];
                for (int i = 0; i < indexCount; i++)
                    positions[i] = BitConverter.ToInt32(wixBytes, WixHeaderSize + i * 4);

                int count = BitConverter.ToInt32(wilBytes, WilCountOffset);
                int colorCount = BitConverter.ToInt32(wilBytes, WilColorCountOffset);
                int paletteSize = BitConverter.ToInt32(wilBytes, WilPaletteSizeOffset);
                if (paletteSize <= 0 || paletteSize > 4096)
                    paletteSize = 1024; // 256*4 常见

                byte[] palette = new byte[paletteSize];
                Array.Copy(wilBytes, WilPaletteOffset, palette, 0, Math.Min(paletteSize, wilBytes.Length - WilPaletteOffset));

                data = new WilData { Positions = positions, WilBytes = wilBytes, Palette = palette, ColorCount = colorCount };
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WilParser] {ex.Message}");
                return false;
            }
        }

        /// <summary>提取单张图片为 Texture2D。</summary>
        public static Texture2D ExtractImage(WilData data, int index)
        {
            if (data == null || index < 0 || index >= data.Positions.Length)
                return null;

            int pos = data.Positions[index];
            if (pos + ImageInfoSize >= data.WilBytes.Length)
                return null;

            int width = BitConverter.ToInt16(data.WilBytes, pos);
            int height = BitConverter.ToInt16(data.WilBytes, pos + 2);
            if (width <= 0 || height <= 0 || width > 512 || height > 512)
                return null;

            int pixelCount = width * height;
            if (pos + ImageInfoSize + pixelCount > data.WilBytes.Length)
                return null;

            var tex = new Texture2D(width, height);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int by = 0; by < height; by++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = by * width + x;
                    byte palIdx = data.WilBytes[pos + ImageInfoSize + idx];
                    Color c = GetColorFromPalette(data.Palette, palIdx);
                    tex.SetPixel(x, height - 1 - by, c); // Unity 坐标系 Y 向上
                }
            }
            tex.Apply();
            return tex;
        }

        private static Color GetColorFromPalette(byte[] palette, byte index)
        {
            int i = index * 4;
            if (i + 3 >= palette.Length) return Color.clear;
            byte b = palette[i];
            byte g = palette[i + 1];
            byte r = palette[i + 2];
            byte a = index == 0 ? (byte)0 : (byte)255;
            return new Color32(r, g, b, a);
        }

        /// <summary>将地表瓦片（索引 0–N）合并为 32×32 可平铺贴图。</summary>
        public static Texture2D CreateTileAtlas(WilData data, int startIndex, int tileCount, int tileSize = 32)
        {
            if (data == null || tileCount <= 0) return null;

            int cols = Mathf.Min(8, tileCount);
            int rows = (tileCount + cols - 1) / cols;
            int width = cols * tileSize;
            int height = rows * tileSize;

            var atlas = new Texture2D(width, height);
            atlas.filterMode = FilterMode.Bilinear;
            atlas.wrapMode = TextureWrapMode.Repeat;

            for (int i = 0; i < tileCount; i++)
            {
                var tex = ExtractImage(data, startIndex + i);
                if (tex == null) continue;

                int col = i % cols;
                int row = i / cols;
                int x = col * tileSize;
                int y = (rows - 1 - row) * tileSize;

                for (int py = 0; py < tileSize && py < tex.height; py++)
                for (int px = 0; px < tileSize && px < tex.width; px++)
                    atlas.SetPixel(x + px, y + py, tex.GetPixel(px, py));

                UnityEngine.Object.DestroyImmediate(tex);
            }
            atlas.Apply();
            return atlas;
        }
    }

    public class WilData
    {
        public int[] Positions;
        public byte[] WilBytes;
        public byte[] Palette;
        public int ColorCount;
    }
}
