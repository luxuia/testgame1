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
