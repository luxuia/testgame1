---
name: crystal-map-parsing-reference
description: Use when debugging Mir2 map parsing, .map format precision, FishingCell, or terrain coordinate issues in Legendary Terrain. Consult Crystal source as authoritative reference.
---

# Crystal 地图解析参考

## 概述

解析传奇 2 `.map` 或地形坐标出现问题时，以 [Crystal 官方源码](https://github.com/Suprcode/Crystal) 为权威参考，对照 `Mir2MapParser` 实现。

## 何时使用

- `.map` 解析结果与预期不符
- 水域（FishingCell）识别不全或错误
- 地形块属性（HighWall、LowWall、Door）判断异常
- 坐标映射、分块、怪物刷新位置有偏差
- 需要确认某版本 .map 的字节布局

## 核心参考路径

| 用途 | Crystal 文件路径 |
|------|-----------------|
| **地图解析（权威）** | `Server/MirEnvir/Map.cs` |
| **格式检测与布局** | `LoadMapCellsv0` ~ `LoadMapCellsV100` |
| **可视化/简化版** | `Server.MirForms/VisualMapInfo/Class/ReadMap.cs` |
| **RespawnInfo 结构** | `Server/MirDatabase` + `MirDBParser` |

## 排查流程

1. **定位格式**：用 `FindType` 确定当前 .map 的版本（v0–v7、v100）
2. **对照实现**：打开 `Server/MirEnvir/Map.cs`，找到对应 `LoadMapCellsvN`
3. **逐字段比对**：offset、字节顺序、属性判断（如 `0x8000`、`0x20000000`）
4. **FishingCell**：Crystal 使用 `light >= 100 && light <= 119`，非仅 100–101

## 快速对照表

| 字段/属性 | Crystal 逻辑 | 常见差异 |
|-----------|-------------|----------|
| FishingCell | `light >= 100 && light <= 119` | 若只判 100–101 会漏掉部分水域 |
| HighWall | `backImg & 0x8000` 或 `frontImg & 0x8000` | 与实现一致 |
| LowWall | `middleImg & 0x8000` | 与实现一致 |
| Door | `fileBytes[offSet] > 0` | 与实现一致 |

## 常见错误

- **FishingCell 范围过窄**：只判 100–101，应改为 100–119
- **offset 错位**：某版本每格字节数不同，漏读或多读会导致后续格错位
- **字节序**：`BitConverter` 默认小端，与 Crystal 一致

## 参考链接

- Crystal 仓库：https://github.com/Suprcode/Crystal
- Map.cs 直接链接：https://github.com/Suprcode/Crystal/blob/master/Server/MirEnvir/Map.cs
