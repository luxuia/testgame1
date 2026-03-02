# Legendary Terrain

从传奇 2 Crystal 数据生成 Unity 3D 方块地形。

## 使用

1. **Tools > Legendary > Download Crystal Data** — 从 Crystal.Database 克隆/拉取数据，并复制 Jev 的 Maps、Envir、Server.MirDB 到本地
2. **Tools > Legendary > Generate Terrain from Map** — 解析 .map 文件并生成方块地形场景

数据位于 `StreamingAssets/LegendaryData/`。

## 数据格式

### .map 格式

支持传奇 2 的 .map 格式 v0–v4，由 `Mir2MapParser` 自动检测并解析：

- **v0** — 经典格式，52 字节头 + 每格 14 字节
- **v1** — 带 XOR 加密的变体
- **v2** — 每格 14 字节变体
- **v3** — 每格 32 字节扩展格式
- **v4** — 64 字节头 + 每格 12 字节

单元格属性：Walk（可行走）、HighWall、LowWall、Door。

### RespawnInfo 与 MirDB

`Server.MirDB` 为 Crystal 的二进制数据库，包含 `MapInfo` 与 `RespawnInfo`：

- **MapInfo** — 地图索引、文件名、标题
- **RespawnInfo** — 怪物索引、刷新坐标、数量、扩散、延迟、方向

生成地形时会根据当前地图匹配 RespawnInfo，在对应位置放置 SpawnMarker（球体标记）。

### 调试

- **Tools > Legendary > Test MirDB Parser** — 测试 MirDB 解析，打印 MapInfo 数量与首个地图的 Respawn 数量
