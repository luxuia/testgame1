using System.Collections.Generic;
using System.IO;

namespace LegendaryTerrain
{
    public static class MirDBParser
    {
        public static List<Mir2MapInfo> Parse(string mirDbPath)
        {
            if (!File.Exists(mirDbPath)) return new List<Mir2MapInfo>();
            // TODO: 解析 Server.MirDB 二进制格式，参考 Crystal Envir 加载逻辑
            return new List<Mir2MapInfo>();
        }
    }
}
