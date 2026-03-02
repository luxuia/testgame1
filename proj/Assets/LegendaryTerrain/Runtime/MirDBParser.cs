using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// Parses Crystal Server.MirDB binary format.
    /// Format: Version, CustomVersion, header indices, then MapInfoList (count + MapInfo[]).
    /// MapInfo contains SafeZones, Respawns, Movements, etc. We extract Index, FileName, Title, Respawns.
    /// </summary>
    public static class MirDBParser
    {
        private const int MinVersion = 60;
        private const int MaxSupportedVersion = 117;

        public static List<Mir2MapInfo> Parse(string mirDbPath)
        {
            if (!File.Exists(mirDbPath)) return new List<Mir2MapInfo>();

            try
            {
                using var stream = File.OpenRead(mirDbPath);
                using var reader = new BinaryReader(stream);

                int loadVersion = reader.ReadInt32();
                int loadCustomVersion = reader.ReadInt32();

                if (loadVersion < MinVersion || loadVersion > MaxSupportedVersion)
                    return new List<Mir2MapInfo>();

                // Skip header indices to reach MapInfoList
                reader.ReadInt32(); // MapIndex
                reader.ReadInt32(); // ItemIndex
                reader.ReadInt32(); // MonsterIndex
                reader.ReadInt32(); // NPCIndex
                reader.ReadInt32(); // QuestIndex
                if (loadVersion >= 63) reader.ReadInt32(); // GameshopIndex
                if (loadVersion >= 66) reader.ReadInt32(); // ConquestIndex
                if (loadVersion >= 68) reader.ReadInt32(); // RespawnIndex

                int mapCount = reader.ReadInt32();
                var result = new List<Mir2MapInfo>(mapCount);

                for (int i = 0; i < mapCount; i++)
                {
                    var info = ReadMapInfo(reader, loadVersion, loadCustomVersion);
                    if (info != null)
                        result.Add(info);
                }

                return result;
            }
            catch (Exception)
            {
                return new List<Mir2MapInfo>();
            }
        }

        private static Mir2MapInfo ReadMapInfo(BinaryReader reader, int version, int customVersion)
        {
            var info = new Mir2MapInfo
            {
                Index = reader.ReadInt32(),
                FileName = reader.ReadString(),
                Title = reader.ReadString()
            };

            reader.ReadUInt16(); // MiniMap
            reader.ReadByte();   // Light
            reader.ReadUInt16(); // BigMap

            // SafeZones
            int szCount = reader.ReadInt32();
            for (int i = 0; i < szCount; i++)
            {
                reader.ReadInt32(); reader.ReadInt32(); // Location
                reader.ReadUInt16();                   // Size
                reader.ReadBoolean();                  // StartPoint
            }

            // Respawns - we need these
            int respawnCount = reader.ReadInt32();
            for (int i = 0; i < respawnCount; i++)
            {
                info.Respawns.Add(ReadRespawnInfo(reader, version));
            }

            // Movements
            int movCount = reader.ReadInt32();
            for (int i = 0; i < movCount; i++)
            {
                ReadMovementInfo(reader, version);
            }

            // Booleans and strings
            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadString();
            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
            reader.ReadBoolean(); reader.ReadBoolean();
            reader.ReadInt32();  // FireDamage
            reader.ReadBoolean(); reader.ReadInt32(); // Lightning, LightningDamage
            reader.ReadByte();   // MapDarkLight

            // MineZones
            int mzCount = reader.ReadInt32();
            for (int i = 0; i < mzCount; i++)
            {
                reader.ReadInt32(); reader.ReadInt32(); // Location
                reader.ReadUInt16(); reader.ReadByte(); // Size, Mine
            }
            reader.ReadByte(); // MineIndex

            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); // NoMount, NeedBridle, NoFight
            reader.ReadUInt16(); // Music

            if (version >= 78) reader.ReadBoolean(); // NoTownTeleport
            if (version >= 79) reader.ReadBoolean(); // NoReincarnation
            if (version >= 110) reader.ReadUInt16(); // WeatherParticles
            if (version >= 111) { reader.ReadBoolean(); reader.ReadByte(); } // GT, GTIndex
            if (version >= 114)
            {
                reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
                reader.ReadBoolean(); reader.ReadBoolean();
                reader.ReadInt32(); reader.ReadBoolean();
                reader.ReadBoolean(); reader.ReadInt32();
            }

            return info;
        }

        private static Mir2RespawnInfo ReadRespawnInfo(BinaryReader reader, int version)
        {
            var r = new Mir2RespawnInfo
            {
                MonsterIndex = reader.ReadInt32(),
                Location = new Vector2Int(reader.ReadInt32(), reader.ReadInt32()),
                Count = reader.ReadUInt16(),
                Spread = reader.ReadUInt16(),
                Delay = reader.ReadUInt16(),
                Direction = reader.ReadByte()
            };
            reader.ReadString(); // RoutePath
            if (version > 67)
            {
                reader.ReadUInt16();  // RandomDelay
                reader.ReadInt32();   // RespawnIndex
                reader.ReadBoolean(); // SaveRespawnTime
                reader.ReadUInt16();  // RespawnTicks
            }
            return r;
        }

        private static void ReadMovementInfo(BinaryReader reader, int version)
        {
            reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32(); // MapIndex, Source
            reader.ReadInt32(); reader.ReadInt32(); // Destination
            reader.ReadBoolean(); reader.ReadBoolean(); // NeedHole, NeedMove
            if (version >= 69) reader.ReadInt32(); // ConquestIndex
            if (version >= 95) { reader.ReadBoolean(); reader.ReadInt32(); } // ShowOnBigMap, Icon
        }
    }
}
