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
            if ((input[4] == 0x0F) || ((input[4] == 0x03) && (input[18] == 0x0D) && (input[19] == 0x0A)))
            {
                int w = input[0] + (input[1] << 8);
                int h = input[2] + (input[3] << 8);
                return (input.Length > (52 + (w * h * 14))) ? (byte)3 : (byte)2;
            }
            if ((input[0] == 0x0D) && (input[1] == 0x4C) && (input[7] == 0x20) && (input[11] == 0x6D)) return 7;
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
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk };

                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 4;
                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 3;
                offSet += 1; // light

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
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk };

                if (((BitConverter.ToInt32(fileBytes, offSet) ^ 0xAA38AA38) & 0x20000000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 6;
                if (((BitConverter.ToInt16(fileBytes, offSet) ^ xor) & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;
                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 5;
                offSet += 2; // light + 1

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

                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;
                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 5;
                offSet += 2; // light + 2

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

                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 2;
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;
                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 12;
                offSet += 18; // light + 17

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
                var cell = new Mir2Cell { Attribute = CellAttribute.Walk };

                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.HighWall;
                offSet += 2;
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0) cell.Attribute = CellAttribute.LowWall;
                offSet += 4;
                if (fileBytes[offSet] > 0) cell.Attribute = CellAttribute.Door;
                offSet += 6;

                grid.Set(x, y, cell);
            }

            return grid;
        }
    }
}
