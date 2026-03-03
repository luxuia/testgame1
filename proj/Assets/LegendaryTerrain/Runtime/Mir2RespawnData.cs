using UnityEngine;
using System;
using System.Collections.Generic;

namespace LegendaryTerrain
{
    [Serializable]
    public class Mir2RespawnInfo
    {
        public int MonsterIndex;
        public Vector2Int Location;
        public ushort Count;
        public ushort Spread;
        public ushort Delay;
        public byte Direction;
    }

    [Serializable]
    public class Mir2SafeZone
    {
        public Vector2Int Location;
        public ushort Size;
        public bool StartPoint;
    }

    [Serializable]
    public class Mir2MapInfo
    {
        public int Index;
        public string FileName;
        public string Title;
        public List<Mir2SafeZone> SafeZones = new List<Mir2SafeZone>();
        public List<Mir2RespawnInfo> Respawns = new List<Mir2RespawnInfo>();
    }
}
