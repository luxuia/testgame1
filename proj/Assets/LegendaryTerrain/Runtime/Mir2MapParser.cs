using System;
using System.IO;
using UnityEngine;

namespace LegendaryTerrain
{
    public static class Mir2MapParser
    {
        public static Mir2CellGrid Parse(byte[] fileBytes)
        {
            byte format = FindType(fileBytes);
            return format switch
            {
                0 => LoadMapCellsV0(fileBytes),
                1 => LoadMapCellsV1(fileBytes),
                2 => LoadMapCellsV2(fileBytes),
                3 => LoadMapCellsV3(fileBytes),
                4 => LoadMapCellsV4(fileBytes),
                5 => LoadMapCellsV5(fileBytes),
                6 => LoadMapCellsV6(fileBytes),
                7 => LoadMapCellsV7(fileBytes),
                100 => LoadMapCellsV100(fileBytes),
                _ => throw new NotSupportedException($"Map format {format} not supported")
            };
        }

        public static Mir2CellGrid ParseFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return Parse(bytes);
        }

        private static byte FindType(byte[] input)
        {
            if (input.Length < 4) return 255;
            if ((input[2] == 0x43) && (input[3] == 0x23)) return 100;
            if (input[0] == 0) return 5;
            if ((input[0] == 0x0F) && (input[5] == 0x53) && (input[14] == 0x33)) return 6;
            if ((input[0] == 0x15) && (input[4] == 0x32) && (input[6] == 0x41) && (input[19] == 0x31)) return 4;
            if ((input[0] == 0x10) && (input[2] == 0x61) && (input[7] == 0x31) && (input[14] == 0x31)) return 1;
            if (input.Length >= 20 && ((input[4] == 0x0F) || ((input[4] == 0x03) && (input[18] == 0x0D) && (input[19] == 0x0A))))
            {
                int w = input[0] + (input[1] << 8);
                int h = input[2] + (input[3] << 8);
                return (input.Length > (52 + (w * h * 14))) ? (byte)3 : (byte)2;
            }
            if (input.Length >= 12 && (input[0] == 0x0D) && (input[1] == 0x4C) && (input[7] == 0x20) && (input[11] == 0x6D)) return 7;
            return 0;
        }

        private static Mir2CellGrid LoadMapCellsV0(byte[] fileBytes)
        {
            int offSet = 0;
            int width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int height = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };
            offSet = 52;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk, BackIndex = 0, MiddleIndex = 1 };

                short backImg = BitConverter.ToInt16(fileBytes, offSet);
                if ((backImg & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                cell.BackImage = (backImg & 0x8000) != 0 ? (backImg & 0x7FFF) | 0x20000000 : backImg;
                offSet += 2;

                cell.MiddleImage = BitConverter.ToInt16(fileBytes, offSet);
                if ((cell.MiddleImage & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;

                cell.FrontImage = BitConverter.ToInt16(fileBytes, offSet);
                if ((cell.FrontImage & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;

                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 4; // DoorIndex, DoorOffset, FrontAnimationFrame, FrontAnimationTick

                cell.FrontIndex = (short)(fileBytes[offSet++] + 2);
                byte light = fileBytes[offSet++];
                cell.FishingCell = (light == 100 || light == 101);

                grid.Set(x, y, cell);
            }

            return grid;
        }

        private static Mir2CellGrid LoadMapCellsV1(byte[] fileBytes)
        {
            int offSet = 21;
            int w = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(fileBytes, offSet);
            int width = w ^ xor;
            int height = h ^ xor;

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };
            offSet = 54;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk, BackIndex = 0, MiddleIndex = 1 };

                int backImg = (int)(BitConverter.ToInt32(fileBytes, offSet) ^ 0xAA38AA38);
                if ((backImg & 0x20000000) != 0) cell.Attribute = CellAttribute.HighWall;
                cell.BackImage = backImg;
                offSet += 4;

                cell.MiddleImage = (short)(BitConverter.ToInt16(fileBytes, offSet) ^ xor);
                if ((cell.MiddleImage & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;

                cell.FrontImage = (short)(BitConverter.ToInt16(fileBytes, offSet) ^ xor);
                offSet += 2;

                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 4; // DoorIndex, DoorOffset, FrontAnimationFrame, FrontAnimationTick

                cell.FrontIndex = (short)(fileBytes[offSet++] + 2);
                if (cell.FrontIndex == 102) cell.FrontIndex = 90;
                byte light = fileBytes[offSet++];
                cell.FishingCell = (light == 100 || light == 101);
                offSet += 1; // Unknown

                grid.Set(x, y, cell);
            }

            return grid;
        }

        private static Mir2CellGrid LoadMapCellsV2(byte[] fileBytes)
        {
            int offSet = 0;
            int width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int height = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };
            offSet = 52;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk };

                short backImg = BitConverter.ToInt16(fileBytes, offSet);
                if ((backImg & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                cell.BackImage = (backImg & 0x8000) != 0 ? (backImg & 0x7FFF) | 0x20000000 : backImg;
                offSet += 2;

                cell.MiddleImage = BitConverter.ToInt16(fileBytes, offSet);
                if ((cell.MiddleImage & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;

                cell.FrontImage = BitConverter.ToInt16(fileBytes, offSet);
                if ((cell.FrontImage & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;

                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 4; // DoorIndex, DoorOffset, FrontAnimationFrame, FrontAnimationTick

                cell.FrontIndex = (short)(fileBytes[offSet++] + 120);
                byte light = fileBytes[offSet++];
                cell.FishingCell = (light == 100 || light == 101);
                cell.BackIndex = (short)(fileBytes[offSet++] + 100);
                cell.MiddleIndex = (short)(fileBytes[offSet++] + 110);

                grid.Set(x, y, cell);
            }

            return grid;
        }

        private static Mir2CellGrid LoadMapCellsV3(byte[] fileBytes)
        {
            int offSet = 0;
            int width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int height = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };
            offSet = 52;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk };

                short backImg = BitConverter.ToInt16(fileBytes, offSet);
                if ((backImg & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                cell.BackImage = (backImg & 0x8000) != 0 ? (backImg & 0x7FFF) | 0x20000000 : backImg;
                offSet += 2;

                cell.MiddleImage = BitConverter.ToInt16(fileBytes, offSet);
                if ((cell.MiddleImage & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;

                cell.FrontImage = BitConverter.ToInt16(fileBytes, offSet);
                if ((cell.FrontImage & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;

                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 4; // DoorIndex, DoorOffset, FrontAnimationFrame, FrontAnimationTick

                cell.FrontIndex = (short)(fileBytes[offSet++] + 120);
                byte light = fileBytes[offSet++];
                cell.FishingCell = (light == 100 || light == 101);
                cell.BackIndex = (short)(fileBytes[offSet++] + 100);
                cell.MiddleIndex = (short)(fileBytes[offSet++] + 110);

                offSet += 7;  // TileAnimationImage(2) + 5
                offSet += 1;  // TileAnimationFrames
                offSet += 2;  // TileAnimationOffset
                offSet += 14; // light/blending options

                grid.Set(x, y, cell);
            }

            return grid;
        }

        private static Mir2CellGrid LoadMapCellsV4(byte[] fileBytes)
        {
            int offSet = 31;
            int w = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(fileBytes, offSet);
            int width = w ^ xor;
            int height = h ^ xor;

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };
            offSet = 64;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk, BackIndex = 0, MiddleIndex = 1 };

                short backImg = (short)(BitConverter.ToInt16(fileBytes, offSet) ^ xor);
                if ((backImg & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                cell.BackImage = (backImg & 0x8000) != 0 ? (backImg & 0x7FFF) | 0x20000000 : backImg;
                offSet += 2;

                cell.MiddleImage = (short)(BitConverter.ToInt16(fileBytes, offSet) ^ xor);
                if ((cell.MiddleImage & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;

                cell.FrontImage = (short)(BitConverter.ToInt16(fileBytes, offSet) ^ xor);
                offSet += 2;

                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 4; // DoorIndex, DoorOffset, FrontAnimationFrame, FrontAnimationTick

                cell.FrontIndex = (short)(fileBytes[offSet++] + 2);
                byte light = fileBytes[offSet++];
                cell.FishingCell = (light == 100 || light == 101);

                grid.Set(x, y, cell);
            }

            return grid;
        }

        /// <summary>Wemade Mir3 maps (no title, start with blank bytes).</summary>
        private static Mir2CellGrid LoadMapCellsV5(byte[] fileBytes)
        {
            if (fileBytes.Length < 28) throw new InvalidDataException("Map format 5: file too short");
            int offSet = 22;
            int width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int height = BitConverter.ToInt16(fileBytes, offSet);

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };

            // Pre-fill BackIndex/BackImage from 2x2 blocks (Map Editor format)
            int backOff = 28;
            int blocksX = width / 2;
            int blocksY = height / 2;
            for (int bx = 0; bx < blocksX; bx++)
            for (int by = 0; by < blocksY; by++)
            {
                short bi = fileBytes[backOff] != 255 ? (short)(fileBytes[backOff] + 200) : (short)-1;
                int bimg = BitConverter.ToInt16(fileBytes, backOff + 1) + 1;
                backOff += 3;
                for (int i = 0; i < 4; i++)
                {
                    int cx = bx * 2 + (i % 2);
                    int cy = by * 2 + (i / 2);
                    if (cx < width && cy < height)
                    {
                        var c = grid.Get(cx, cy);
                        c.BackIndex = bi;
                        c.BackImage = bimg;
                        grid.Set(cx, cy, c);
                    }
                }
            }

            offSet = 28 + 3 * blocksX * blocksY;
            if (fileBytes.Length < offSet + (long)width * height * 14)
                throw new InvalidDataException($"Map format 5: file too short for {width}x{height}");

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = grid.Get(x, y);
                byte flag = fileBytes[offSet++];
                if ((flag & 0x01) != 1) cell.Attribute = CellAttribute.HighWall;
                else if ((flag & 0x02) != 2) cell.Attribute = CellAttribute.LowWall;
                if ((flag & 0x01) != 1) cell.BackImage = (cell.BackImage & 0x7FFF) | 0x20000000;

                offSet += 2; // MiddleAnimationFrame, FrontAnimationFrame
                cell.FrontIndex = fileBytes[offSet] != 255 ? (short)(fileBytes[offSet] + 200) : (short)-1;
                offSet++;
                cell.MiddleIndex = fileBytes[offSet] != 255 ? (short)(fileBytes[offSet] + 200) : (short)-1;
                offSet++;
                cell.MiddleImage = (short)(BitConverter.ToInt16(fileBytes, offSet) + 1);
                offSet += 2;
                cell.FrontImage = (short)(BitConverter.ToInt16(fileBytes, offSet) + 1);
                offSet += 2;
                if (cell.FrontImage == 1 && cell.FrontIndex == 200) cell.FrontIndex = -1;
                offSet += 3; // doors
                offSet += 2; // light (no FishingCell in Mir3)

                grid.Set(x, y, cell);
            }

            return grid;
        }

        /// <summary>Shanda Mir3 maps (title: (C) SNDA, MIR3.).</summary>
        private static Mir2CellGrid LoadMapCellsV6(byte[] fileBytes)
        {
            if (fileBytes.Length < 40) throw new InvalidDataException("Map format 6: file too short");
            int offSet = 16;
            int width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int height = BitConverter.ToInt16(fileBytes, offSet);

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };
            offSet = 40;
            if (fileBytes.Length < offSet + (long)width * height * 20)
                throw new InvalidDataException($"Map format 6: file too short for {width}x{height}");

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk };
                byte flag = fileBytes[offSet++];
                if ((flag & 0x01) != 1) cell.Attribute = CellAttribute.HighWall;
                else if ((flag & 0x02) != 2) cell.Attribute = CellAttribute.LowWall;

                cell.BackIndex = fileBytes[offSet] != 255 ? (short)(fileBytes[offSet] + 300) : (short)-1;
                offSet++;
                cell.MiddleIndex = fileBytes[offSet] != 255 ? (short)(fileBytes[offSet] + 300) : (short)-1;
                offSet++;
                cell.FrontIndex = fileBytes[offSet] != 255 ? (short)(fileBytes[offSet] + 300) : (short)-1;
                offSet++;
                cell.BackImage = (short)(BitConverter.ToInt16(fileBytes, offSet) + 1);
                offSet += 2;
                cell.MiddleImage = (short)(BitConverter.ToInt16(fileBytes, offSet) + 1);
                offSet += 2;
                cell.FrontImage = (short)(BitConverter.ToInt16(fileBytes, offSet) + 1);
                offSet += 2;
                if (cell.FrontImage == 1 && cell.FrontIndex == 300) cell.FrontIndex = -1; // empty sentinel
                offSet += 2; // MiddleAnimationFrame, FrontAnimationFrame
                offSet += 1; // FrontAnimationFrame handling
                offSet += 8; // Light + padding

                if ((flag & 0x01) != 1) cell.BackImage |= 0x20000000;
                if ((flag & 0x02) != 2) cell.FrontImage = (short)((ushort)cell.FrontImage | 0x8000);

                grid.Set(x, y, cell);
            }

            return grid;
        }

        /// <summary>3/4 Heroes map format (myth/lifcos).</summary>
        private static Mir2CellGrid LoadMapCellsV7(byte[] fileBytes)
        {
            if (fileBytes.Length < 54) throw new InvalidDataException("Map format 7: file too short");
            int offSet = 21;
            int width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 4;
            int height = BitConverter.ToInt16(fileBytes, offSet);

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };
            offSet = 54;
            if (fileBytes.Length < offSet + (long)width * height * 15)
                throw new InvalidDataException($"Map format 7: file too short for {width}x{height}");

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk, BackIndex = 0, MiddleIndex = 1 };

                int backImg = BitConverter.ToInt32(fileBytes, offSet);
                if ((backImg & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                cell.BackImage = (backImg & 0x8000) != 0 ? (backImg & 0x7FFF) | 0x20000000 : backImg;
                offSet += 4;

                cell.MiddleImage = BitConverter.ToInt16(fileBytes, offSet);
                if ((cell.MiddleImage & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;

                cell.FrontImage = BitConverter.ToInt16(fileBytes, offSet);
                offSet += 2;

                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 4; // DoorIndex, DoorOffset, FrontAnimationFrame, FrontAnimationTick

                cell.FrontIndex = (short)(fileBytes[offSet++] + 2);
                byte light = fileBytes[offSet++];
                cell.FishingCell = (light == 100 || light == 101);
                offSet += 1; // Unknown

                grid.Set(x, y, cell);
            }

            return grid;
        }

        /// <summary>
        /// C# custom map format (header: 0x43 0x23 at bytes 2-3).
        /// Version 1.0 only. Header: version(2) + "C#"(2) + width(2) + height(2), cell data from offset 8.
        /// </summary>
        private static Mir2CellGrid LoadMapCellsV100(byte[] fileBytes)
        {
            if (fileBytes.Length < 8 || fileBytes[0] != 1 || fileBytes[1] != 0)
                throw new NotSupportedException("Map format 100: only version 1.0 supported");

            int offset = 4;
            int width = BitConverter.ToInt16(fileBytes, offset);
            offset += 2;
            int height = BitConverter.ToInt16(fileBytes, offset);

            var grid = new Mir2CellGrid { Width = width, Height = height, Cells = new Mir2Cell[width * height] };
            offset = 8;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk };

                cell.BackIndex = BitConverter.ToInt16(fileBytes, offset);
                offset += 2;
                cell.BackImage = BitConverter.ToInt32(fileBytes, offset);
                if ((cell.BackImage & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offset += 4;

                cell.MiddleIndex = BitConverter.ToInt16(fileBytes, offset);
                offset += 2;
                cell.MiddleImage = BitConverter.ToInt16(fileBytes, offset);
                if ((cell.MiddleImage & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offset += 2;

                cell.FrontIndex = BitConverter.ToInt16(fileBytes, offset);
                offset += 2;
                cell.FrontImage = BitConverter.ToInt16(fileBytes, offset);
                offset += 2;

                if (fileBytes[offset] > 0) cell.Attribute = CellAttribute.Door;
                offset += 6; // DoorIndex, DoorOffset, FrontAnim, MiddleAnim

                offset += 2; // TileAnimationImage
                offset += 2; // TileAnimationOffset
                offset += 1; // TileAnimationFrames
                byte light = fileBytes[offset++];
                cell.FishingCell = (light == 100 || light == 101);

                grid.Set(x, y, cell);
            }

            return grid;
        }
    }
}
