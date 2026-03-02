# 传奇场景地形生成 - 实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 从 Crystal/Crystal.Database 下载数据，解析 .map 与 RespawnInfo，在 Unity 中生成 3D 方块地形场景。

**Architecture:** Editor 下载工具 → 数据存 StreamingAssets → 运行时/Editor 解析 .map 与 MirDB → 根据 CellGrid 生成方块，根据 RespawnInfo 放置刷新点标记。

**Tech Stack:** Unity 2022 LTS, C#, git, Crystal.Database (Jev)

---

## Task 1: 创建目录结构与 asmdef

**Files:**
- Create: `Assets/LegendaryTerrain/Runtime/LegendaryTerrain.Runtime.asmdef`
- Create: `Assets/LegendaryTerrain/Editor/LegendaryTerrain.Editor.asmdef`

**Step 1: 创建 Runtime asmdef**

```json
{
  "name": "LegendaryTerrain.Runtime",
  "rootNamespace": "LegendaryTerrain",
  "references": ["UnityEngine.CoreModule"],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

**Step 2: 创建 Editor asmdef**

```json
{
  "name": "LegendaryTerrain.Editor",
  "rootNamespace": "LegendaryTerrain.Editor",
  "references": ["LegendaryTerrain.Runtime", "UnityEngine.CoreModule", "UnityEditor"],
  "includePlatforms": ["Editor"],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

**Step 3: 验证**

在 Unity 中刷新，确认无编译错误。

**Step 4: Commit**

```bash
git add Assets/LegendaryTerrain/
git commit -m "chore: add LegendaryTerrain asmdef"
```

---

## Task 2: 定义 Mir2MapData 数据结构

**Files:**
- Create: `Assets/LegendaryTerrain/Runtime/Mir2MapData.cs`

**Step 1: 实现 CellAttribute 与 Mir2CellGrid**

```csharp
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
```

**Step 2: 验证**

Unity 刷新，无编译错误。

**Step 3: Commit**

```bash
git add Assets/LegendaryTerrain/Runtime/Mir2MapData.cs
git commit -m "feat: add Mir2MapData structures"
```

---

## Task 3: 实现 Mir2MapParser（.map 解析）

**Files:**
- Create: `Assets/LegendaryTerrain/Runtime/Mir2MapParser.cs`

**Step 1: 实现 FindType 与 v0 解析**

参考 Crystal Map.cs 的 FindType、LoadMapCellsv0，移植为静态方法，输出 Mir2CellGrid。仅实现 v0 格式（最常见）。

```csharp
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
    }
}
```

**Step 2: 验证**

在 Editor 中写临时测试：读取 `StreamingAssets/LegendaryData/Jev/Maps/0.map`（需先下载），调用 `Mir2MapParser.ParseFile`，打印 Width/Height。若无数据可先跳过，仅验证编译。

**Step 3: Commit**

```bash
git add Assets/LegendaryTerrain/Runtime/Mir2MapParser.cs
git commit -m "feat: add Mir2MapParser for v0 map format"
```

---

## Task 4: 实现 LegendaryDataDownloader

**Files:**
- Create: `Assets/LegendaryTerrain/Editor/LegendaryDataDownloader.cs`

**Step 1: 实现下载菜单与 git clone**

```csharp
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace LegendaryTerrain.Editor
{
    public static class LegendaryDataDownloader
    {
        private const string DataPath = "StreamingAssets/LegendaryData";
        private const string CrystalDbUrl = "https://github.com/Suprcode/Crystal.Database.git";

        [MenuItem("Tools/Legendary/Download Crystal Data")]
        public static void Download()
        {
            string basePath = Path.Combine(Application.dataPath, DataPath);
            string dbPath = Path.Combine(basePath, "Crystal.Database");

            Directory.CreateDirectory(basePath);

            if (Directory.Exists(Path.Combine(dbPath, ".git")))
            {
                UnityEngine.Debug.Log("Crystal.Database exists, pulling...");
                RunGit(dbPath, "pull");
            }
            else
            {
                UnityEngine.Debug.Log("Cloning Crystal.Database...");
                RunGit(basePath, $"clone --depth 1 {CrystalDbUrl} Crystal.Database");
            }

            CopyJevData(basePath, dbPath);
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("Legendary data ready at " + basePath);
        }

        private static void CopyJevData(string basePath, string dbPath)
        {
            string jev = Path.Combine(dbPath, "Jev");
            string destMaps = Path.Combine(basePath, "Maps");
            string destEnvir = Path.Combine(basePath, "Envir");

            if (Directory.Exists(Path.Combine(jev, "Maps")))
            {
                CopyDir(Path.Combine(jev, "Maps"), destMaps);
            }
            if (Directory.Exists(Path.Combine(jev, "Envir")))
            {
                CopyDir(Path.Combine(jev, "Envir"), destEnvir);
            }

            string mirDb = Path.Combine(jev, "Server.MirDB");
            if (File.Exists(mirDb))
            {
                File.Copy(mirDb, Path.Combine(basePath, "Server.MirDB"), true);
            }
        }

        private static void CopyDir(string src, string dest)
        {
            if (Directory.Exists(dest)) Directory.Delete(dest, true);
            Directory.CreateDirectory(dest);
            foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                string rel = f.Substring(src.Length + 1);
                string d = Path.Combine(dest, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(d));
                File.Copy(f, d, true);
            }
        }

        private static void RunGit(string cwd, string args)
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = cwd,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var p = Process.Start(psi);
            p.WaitForExit(60000);
            string err = p.StandardError.ReadToEnd();
            if (p.ExitCode != 0) UnityEngine.Debug.LogError("git: " + err);
        }
    }
}
```

**Step 2: 验证**

菜单 Tools > Legendary > Download Crystal Data，检查 `Assets/StreamingAssets/LegendaryData/Maps` 下是否有 .map 文件。

**Step 3: Commit**

```bash
git add Assets/LegendaryTerrain/Editor/LegendaryDataDownloader.cs
git commit -m "feat: add LegendaryDataDownloader"
```

---

## Task 5: 定义 RespawnInfo 与 MirDB 解析占位

**Files:**
- Create: `Assets/LegendaryTerrain/Runtime/Mir2RespawnData.cs`
- Create: `Assets/LegendaryTerrain/Runtime/MirDBParser.cs`

**Step 1: RespawnInfo 结构**

```csharp
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
    public class Mir2MapInfo
    {
        public int Index;
        public string FileName;
        public string Title;
        public List<Mir2RespawnInfo> Respawns = new List<Mir2RespawnInfo>();
    }
}
```

**Step 2: MirDBParser 占位（先返回空列表）**

```csharp
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
```

**Step 3: 验证**

编译通过。

**Step 4: Commit**

```bash
git add Assets/LegendaryTerrain/Runtime/Mir2RespawnData.cs Assets/LegendaryTerrain/Runtime/MirDBParser.cs
git commit -m "feat: add RespawnInfo structures and MirDBParser placeholder"
```

---

## Task 6: 实现 LegendaryTerrainGenerator（方块生成）

**Files:**
- Create: `Assets/LegendaryTerrain/Editor/LegendaryTerrainGenerator.cs`

**Step 1: 实现生成菜单**

```csharp
using UnityEditor;
using UnityEngine;
using System.IO;

namespace LegendaryTerrain.Editor
{
    public static class LegendaryTerrainGenerator
    {
        private const float BlockSize = 1f;

        [MenuItem("Tools/Legendary/Generate Terrain from Map")]
        public static void Generate()
        {
            string mapsPath = Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Maps");
            if (!Directory.Exists(mapsPath))
            {
                UnityEngine.Debug.LogError("Run Download Crystal Data first.");
                return;
            }

            string mapFile = Path.Combine(mapsPath, "0.map");
            if (!File.Exists(mapFile)) mapFile = FindFirstMap(mapsPath);
            if (mapFile == null)
            {
                UnityEngine.Debug.LogError("No .map files found.");
                return;
            }

            var grid = Mir2MapParser.ParseFile(mapFile);
            var root = new GameObject("LegendaryTerrain");
            root.transform.position = Vector3.zero;

            for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
            {
                var cell = grid.Get(x, y);
                if (cell.Attribute == CellAttribute.Walk)
                {
                    CreateBlock(root.transform, x, y, 0, "Ground");
                }
                else if (cell.Attribute == CellAttribute.HighWall || cell.Attribute == CellAttribute.LowWall)
                {
                    CreateBlock(root.transform, x, y, 1, "Wall");
                }
            }

            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();
            UnityEngine.Debug.Log($"Generated terrain {grid.Width}x{grid.Height} from {Path.GetFileName(mapFile)}");
        }

        private static string FindFirstMap(string dir)
        {
            foreach (var f in Directory.GetFiles(dir, "*.map", SearchOption.AllDirectories))
                return f;
            return null;
        }

        private static void CreateBlock(Transform parent, int x, int y, int z, string tag)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"{tag}_{x}_{y}";
            cube.transform.SetParent(parent);
            cube.transform.localPosition = new Vector3(x * BlockSize, z * BlockSize, y * BlockSize);
            cube.GetComponent<Renderer>().sharedMaterial = tag == "Ground"
                ? AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat")
                : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
        }
    }
}
```

**Step 2: 验证**

下载数据后，执行 Tools > Legendary > Generate Terrain from Map，场景中应出现方块地形。

**Step 3: Commit**

```bash
git add Assets/LegendaryTerrain/Editor/LegendaryTerrainGenerator.cs
git commit -m "feat: add LegendaryTerrainGenerator"
```

---

## Task 7: 集成 RespawnInfo 到生成流程（占位显示）

**Files:**
- Modify: `Assets/LegendaryTerrain/Editor/LegendaryTerrainGenerator.cs`

**Step 1: 在 Generate 中调用 MirDBParser 并放置 SpawnMarker**

在 `Generate()` 中，解析 MirDB 后根据当前地图 FileName 匹配 Respawns，在对应 Location 放置一个简单 Cube 作为 SpawnMarker（不同颜色或命名区分）。

```csharp
// 在 CreateBlock 循环之后添加：
string mirDbPath = Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Server.MirDB");
var mapInfos = MirDBParser.Parse(mirDbPath);
string currentMapName = Path.GetFileNameWithoutExtension(mapFile);
foreach (var info in mapInfos)
{
    if (info.FileName != currentMapName) continue;
    foreach (var r in info.Respawns)
    {
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = $"Spawn_Monster{r.MonsterIndex}";
        marker.transform.SetParent(root.transform);
        marker.transform.localPosition = new Vector3(r.Location.x * BlockSize, 0.5f, r.Location.y * BlockSize);
        marker.transform.localScale = Vector3.one * 0.5f;
    }
}
```

**Step 2: 验证**

当 MirDBParser 返回空时，不放置 SpawnMarker，地形仍正常生成。后续实现 MirDB 解析后，SpawnMarker 会正确出现。

**Step 3: Commit**

```bash
git add Assets/LegendaryTerrain/Editor/LegendaryTerrainGenerator.cs
git commit -m "feat: integrate RespawnInfo spawn markers (placeholder)"
```

---

## Task 8: 实现 MirDB 解析（MapInfo + RespawnInfo）

**Files:**
- Modify: `Assets/LegendaryTerrain/Runtime/MirDBParser.cs`

**Step 1: 研究 Crystal Envir 加载逻辑**

从 https://github.com/Suprcode/Crystal 的 Envir.cs、MapInfo 加载相关代码，确定 Server.MirDB 的二进制布局（列表长度、每个 MapInfo 的 BinaryReader 读取顺序）。

**Step 2: 实现 MirDBParser.Parse**

按 MapInfo(BinaryReader) 与 RespawnInfo(BinaryReader) 的字段顺序实现解析。注意版本号（LoadVersion）可能影响字段。

**Step 3: 验证**

解析 `StreamingAssets/LegendaryData/Server.MirDB`，打印 MapInfo 数量及首个地图的 Respawn 数量。

**Step 4: Commit**

```bash
git add Assets/LegendaryTerrain/Runtime/MirDBParser.cs
git commit -m "feat: implement MirDBParser for MapInfo and RespawnInfo"
```

---

## Task 9: 扩展 Map 格式支持（v1-v4）

**Files:**
- Modify: `Assets/LegendaryTerrain/Runtime/Mir2MapParser.cs`

**Step 1: 添加 LoadMapCellsV1、V2、V3、V4**

从 Crystal Map.cs 移植对应方法，在 Parse 的 switch 中增加 case 1,2,3,4。

**Step 2: 验证**

用不同格式的 .map 文件测试（可从 Crystal.Database 中选取）。

**Step 3: Commit**

```bash
git add Assets/LegendaryTerrain/Runtime/Mir2MapParser.cs
git commit -m "feat: support map formats v1-v4"
```

---

## Task 10: 文档与收尾

**Files:**
- Create: `Assets/LegendaryTerrain/README.md`

**Step 1: 编写使用说明**

```markdown
# Legendary Terrain

从传奇 2 Crystal 数据生成 Unity 3D 方块地形。

## 使用

1. Tools > Legendary > Download Crystal Data
2. Tools > Legendary > Generate Terrain from Map

数据位于 StreamingAssets/LegendaryData/。
```

**Step 2: Commit**

```bash
git add Assets/LegendaryTerrain/README.md
git commit -m "docs: add LegendaryTerrain README"
```

---

## 执行选项

计划已保存至 `docs/plans/2026-03-02-legendary-terrain.md`。

**两种执行方式：**

1. **Subagent-Driven（本会话）** — 按任务派发子代理，每任务后做代码审查，快速迭代  
2. **Parallel Session（独立会话）** — 在新会话中用 executing-plans 批量执行，带检查点

**选择哪种？**
