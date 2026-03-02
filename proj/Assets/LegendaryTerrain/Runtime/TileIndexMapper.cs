using System;

namespace LegendaryTerrain
{
    /// <summary>
    /// 将 Back/Middle/Front 贴图索引映射到地块类型（树、房子、桥等）。
    /// 基于 Crystal Map Editor 的 WIL 索引约定，可扩展。
    /// </summary>
    public static class TileIndexMapper
    {
        /// <summary>
        /// 根据单元格贴图索引返回叠加地块类型，无则返回 null。
        /// </summary>
        public static string GetOverlayBlockType(Mir2Cell cell)
        {
            // 优先检查 Middle 层（树、房子等）
            if (HasMiddleObject(cell))
                return MapMiddleToBlockType(cell.MiddleIndex, cell.MiddleImage);

            // 再检查 Front 层（前景装饰）
            if (HasFrontObject(cell))
                return MapFrontToBlockType(cell.FrontIndex, cell.FrontImage);

            return null;
        }

        private static bool HasMiddleObject(Mir2Cell cell)
        {
            if (cell.MiddleIndex < 0) return false;
            if (cell.MiddleImage <= 0) return false;
            // 0x8000 表示 LowWall，仍为有效对象
            return true;
        }

        private static bool HasFrontObject(Mir2Cell cell)
        {
            if (cell.FrontIndex < 0) return false;
            if (cell.FrontImage <= 0) return false;
            return true;
        }

        /// <summary>
        /// Middle 层常见索引：1=树木/物体，100+=扩展 WIL。简化占位映射。
        /// </summary>
        private static string MapMiddleToBlockType(short middleIndex, short middleImage)
        {
            int img = middleImage & 0x7FFF;
            if (img <= 0) return null;

            // 索引 1 通常为树木/装饰
            if (middleIndex == 0 || middleIndex == 1)
                return "Tree";
            // 100-120 常见为建筑
            if (middleIndex >= 100 && middleIndex < 120)
                return "House";
            // 110+ 部分为桥
            if (middleIndex >= 110 && middleIndex < 115)
                return "Bridge";

            return "Tree"; // 默认占位
        }

        /// <summary>
        /// Front 层前景装饰，多为树、栏杆等。
        /// </summary>
        private static string MapFrontToBlockType(short frontIndex, short frontImage)
        {
            int img = frontImage & 0x7FFF;
            if (img <= 0) return null;

            if (frontIndex == 0 || frontIndex == 1 || frontIndex == 2)
                return "Tree";
            if (frontIndex >= 90 && frontIndex <= 95)
                return "Tree";

            return null; // 不叠加，避免重复
        }
    }
}
