# Legendary Terrain

从传奇 2 Crystal 数据生成 Unity 3D 方块地形。

## 使用

1. **Tools > Legendary > Create Block Prefabs** — 首次使用前执行，创建地块 Prefab（含材质与贴图）
2. **Tools > Legendary > Download Crystal Data** — 从 Crystal.Database 克隆/拉取数据
3. **Tools > Legendary > Generate Terrain from Map** — 解析 .map 并生成地形（运行时加载 Prefab）

数据位于 `StreamingAssets/LegendaryData/`。地块 Prefab 位于 `Resources/LegendaryTerrain/`，可替换其中的贴图与材质。

## 数据格式

### .map 格式

支持 [Crystal](https://github.com/Suprcode/Crystal) 中定义的全部 .map 格式（v0–v7、v100），由 `Mir2MapParser` 自动检测并解析：

| 格式 | 来源 | 说明 |
|------|------|------|
| **v0** | 经典 Mir2 | 52 字节头 + 每格 14 字节 |
| **v1** | Wemade 2010 | Map 2010 Ver 1.0，XOR 加密 |
| **v2** | Shanda 旧版 | 52 字节头 + 每格 14 字节 |
| **v3** | Shanda 2012 | 52 字节头 + 每格 36 字节 |
| **v4** | Wemade AntiHack | Mir2 AntiHack，64 字节头 + 每格 12 字节 |
| **v5** | Wemade Mir3 | 无标题，空白字节开头 |
| **v6** | Shanda Mir3 | 标题 (C) SNDA, MIR3. |
| **v7** | 3/4 Heroes | myth/lifcos 格式 |
| **v100** | C# 自定义 | header 含 0x43 0x23，仅版本 1.0 |

单元格属性：Walk（可行走）、HighWall、LowWall、Door。

### RespawnInfo 与 MirDB

`Server.MirDB` 为 Crystal 的二进制数据库，包含 `MapInfo` 与 `RespawnInfo`：

- **MapInfo** — 地图索引、文件名、标题
- **RespawnInfo** — 怪物索引、刷新坐标、数量、扩散、延迟、方向

生成地形时会根据当前地图匹配 RespawnInfo，在对应位置放置 SpawnMarker（球体标记）。

### 调试

- **Tools > Legendary > Test MirDB Parser** — 测试 MirDB 解析，打印 MapInfo 数量与首个地图的 Respawn 数量
