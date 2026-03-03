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
        private const int MaxSupportedVersion = 200;

        /// <summary>
        /// 诊断解析失败原因。返回 (版本, 地图数, 错误信息)。
        /// </summary>
        public static (int version, int mapCount, string error) GetDiagnostics(string mirDbPath)
        {
            if (!File.Exists(mirDbPath))
                return (0, 0, "文件不存在");

            try
            {
                using var stream = File.OpenRead(mirDbPath);
                using var reader = new BinaryReader(stream);

                int loadVersion = reader.ReadInt32();
                int loadCustomVersion = reader.ReadInt32();

                if (loadVersion < MinVersion || loadVersion > MaxSupportedVersion)
                    return (loadVersion, 0, $"版本 {loadVersion} 超出支持范围 [{MinVersion}-{MaxSupportedVersion}]");

                reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32();
                if (loadVersion >= 63) reader.ReadInt32();
                if (loadVersion >= 66) reader.ReadInt32();
                if (loadVersion >= 68) reader.ReadInt32();

                int mapCount = reader.ReadInt32();
                return (loadVersion, mapCount, null);
            }
            catch (Exception ex)
            {
                return (0, 0, $"解析异常: {ex.Message}");
            }
        }

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
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning($"[MirDB] 版本 {loadVersion} 超出支持范围 [{MinVersion}-{MaxSupportedVersion}]，路径: {mirDbPath}");
#endif
                    return new List<Mir2MapInfo>();
                }

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
                if (mapCount < 0 || mapCount > 10000)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning($"[MirDB] mapCount={mapCount} 异常，可能格式不匹配");
#endif
                    return new List<Mir2MapInfo>();
                }

                var result = new List<Mir2MapInfo>(mapCount);
                long streamLength = stream.Length;

                for (int i = 0; i < mapCount; i++)
                {
                    if (stream.Position >= streamLength) break;
                    try
                    {
                        var info = ReadMapInfo(reader, loadVersion, loadCustomVersion);
                        if (info != null)
                            result.Add(info);
                    }
                    catch (EndOfStreamException)
                    {
#if UNITY_EDITOR
                        UnityEngine.Debug.LogWarning($"[MirDB] 第 {i + 1}/{mapCount} 个 MapInfo 解析时到达流末尾，已返回 {result.Count} 条");
#endif
                        break;
                    }
                    catch (IOException ex) when (ex.Message.Contains("end of the stream") || ex.Message.Contains("end of stream"))
                    {
#if UNITY_EDITOR
                        UnityEngine.Debug.LogWarning($"[MirDB] 第 {i + 1}/{mapCount} 个 MapInfo 解析时流结束，已返回 {result.Count} 条");
#endif
                        break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[MirDB] 解析异常: {ex.Message}\n路径: {mirDbPath}");
#endif
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
                info.SafeZones.Add(new Mir2SafeZone
                {
                    Location = new Vector2Int(reader.ReadInt32(), reader.ReadInt32()),
                    Size = reader.ReadUInt16(),
                    StartPoint = reader.ReadBoolean()
                });
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

            // NoTeleport, NoReconnect, NoReconnectMap, NoRandom..NoNames, Fight, Fire (13 bools + 1 string)
            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadString();
            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
            reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
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
