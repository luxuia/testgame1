# 传奇场景地形生成 - 设计文档

> 基于 brainstorming 技能产出，用户已确认需求。

## 目标

从 [Crystal](https://github.com/Suprcode/Crystal) 与 [Crystal.Database](https://github.com/Suprcode/Crystal.Database) 下载传奇 2 的场景与配置数据，用 C# 在 Unity 中解析 `.map` 与 `RespawnInfo`，动态生成 3D 方块地形场景。

## 用户确认的决策

| 项目 | 选择 |
|------|------|
| 地形表现 | 3D 方块（类似 Minecraft） |
| 数据存储 | StreamingAssets |
| 怪物刷新点 | 解析 RespawnInfo |

## 数据来源

- **Crystal** (https://github.com/Suprcode/Crystal) — C# 解析逻辑参考，GPL v2
- **Crystal.Database** (https://github.com/Suprcode/Crystal.Database) — Jev/Envir、Jev/Maps（400+ .map）、Server.MirDB

## 架构概览

```
[下载] git clone Crystal + Crystal.Database
    → StreamingAssets/LegendaryData/{Maps,Envir,Server.MirDB}

[解析] Mir2MapParser (.map → CellGrid)
       MirDBParser (Server.MirDB → MapInfo+RespawnInfo)

[生成] CellGrid + RespawnInfo → Unity Scene
    → Walk → 地面方块
    → HighWall/LowWall → 墙方块
    → RespawnInfo → 怪物刷新点标记（可后续接 Prefab）
```

## 组件

1. **LegendaryDataDownloader** — Editor 菜单，git clone 到 StreamingAssets
2. **Mir2MapParser** — 移植 Map.cs 解析逻辑（v0–v7、v100），无 Server 依赖
3. **MirDBParser** — 解析 Server.MirDB 获取 MapInfo、RespawnInfo（需逆向 Envir 加载逻辑）
4. **LegendaryTerrainGenerator** — 根据 CellGrid + RespawnInfo 生成 3D 方块场景

## 数据流

- `.map` → `CellGrid[Width,Height]`，每格 `CellAttribute` 为 Walk / HighWall / LowWall / Door
- `Server.MirDB` → `MapInfo`（含 FileName、Respawns、SafeZones 等）→ `RespawnInfo`（MonsterIndex, Location, Count, Spread, Delay）
- `MapInfo.FileName` 与 `Maps/*.map` 对应（如 `0.map`、`0100.map`）

## 错误处理

- 下载失败：检查 git、网络、权限
- 解析失败：记录格式版本，跳过无法识别的文件
- 大地图：分块生成或 LOD，避免单帧卡顿

## 目录结构

```
Assets/
  LegendaryTerrain/
    Editor/
      LegendaryDataDownloader.cs
      LegendaryMapImporter.cs
      LegendaryTerrainGenerator.cs
    Runtime/
      Mir2MapParser.cs
      Mir2MapData.cs
      MirDBParser.cs
    Prefabs/
      Block_Ground.prefab
      Block_Wall.prefab
      SpawnMarker.prefab

StreamingAssets/
  LegendaryData/
    Maps/
    Envir/
    Server.MirDB
```

## 后续扩展

- 怪物 Prefab 映射（MonsterIndex → Unity Prefab）
- 贴图转换（WIL/WIX → Unity Texture）
- 多地图切换、传送门
