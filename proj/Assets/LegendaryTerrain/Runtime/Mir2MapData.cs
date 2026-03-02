using UnityEngine;
using System;

namespace LegendaryTerrain
{
    public enum CellAttribute
    {
        Walk,
        HighWall,
        LowWall,
        Door
    }

    [Serializable]
    public struct Mir2Cell
    {
        public CellAttribute Attribute;
        /// <summary>Floor/Back 层资源索引 (WIL 编号)</summary>
        public short BackIndex;
        /// <summary>Floor/Back 层资源内图片索引</summary>
        public int BackImage;
        /// <summary>Middle 层资源索引 (树、房子、桥等)</summary>
        public short MiddleIndex;
        /// <summary>Middle 层资源内图片索引</summary>
        public short MiddleImage;
        /// <summary>Front 层资源索引</summary>
        public short FrontIndex;
        /// <summary>Front 层资源内图片索引</summary>
        public short FrontImage;
        /// <summary>可钓鱼格 (水)</summary>
        public bool FishingCell;
    }

    [Serializable]
    public class Mir2CellGrid
    {
        public int Width;
        public int Height;
        public Mir2Cell[] Cells;

        public Mir2Cell Get(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return default;
            return Cells[y * Width + x];
        }

        public void Set(int x, int y, Mir2Cell cell)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            Cells[y * Width + x] = cell;
        }
    }
}
