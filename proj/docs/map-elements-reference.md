# 传奇地形地图元素参考

## 一、当前解析的地图元素

### 1. 从 CellAttribute 派生的基础块

| 属性 | 来源（.map 解析） | 生成块类型 | 说明 |
|------|------------------|------------|------|
| **Walk** | 无 0x8000 阻挡 | Ground | 可行走地面 |
| **HighWall** | backImg \| frontImg & 0x8000 | Wall | 高墙，不可穿越 |
| **LowWall** | middleImg & 0x8000 | Wall | 矮墙，不可穿越 |
| **Door** | fileBytes[DoorOffset] > 0 | Door | 门 |
| **FishingCell** | light 100–101（应为 100–119） | Water | 水域 |

### 2. 从 TileIndexMapper 派生的叠加块（仅 Walk 时）

| 层 | 索引范围 | 映射块类型 | 说明 |
|----|----------|------------|------|
| **Middle** | 0–1 | Tree | 树木/装饰 |
| **Middle** | 100–119 | House | 建筑 |
| **Middle** | 110–114 | Bridge | 桥 |
| **Front** | 0–2, 90–95 | Tree | 前景树 |

### 3. BlockType 枚举（完整）

| BlockType | Prefab | 可破坏 |
|-----------|--------|--------|
| Ground | Block_Ground | 否 |
| Wall | Block_Wall | 是 |
| Door | Block_Door | 是 |
| Water | Block_Water | 否 |
| Tree | Block_Tree | 是 |
| House | Block_House | 是 |
| Bridge | Block_Bridge | 是 |
| SpawnMarker | Block_SpawnMarker | 否 |

---

## 二、树与城墙未区分的问题

### 当前逻辑

```
middleImg & 0x8000 → Attribute = LowWall → blockType = "Wall"
```

**问题**：传奇 .map 中，`0x8000` 表示「阻挡/不可穿越」。树木和矮墙都会在 middle 层使用该标志，因此都被解析为 `LowWall`，最终都生成 `Wall`，无法区分树和城墙。

### 区分思路

树与城墙的差异主要在 **WIL 贴图索引**（BackIndex/MiddleIndex/FrontIndex），而非 0x8000 标志：

| 类型 | 典型 MiddleIndex 范围 | 典型 BackIndex |
|------|----------------------|----------------|
| 树木 | 1, 2, 90–99（树木 WIL） | 地面类 |
| 城墙/石墙 | 100+（建筑 WIL） | 墙类 |
| 栅栏 | 特定索引 | - |

### 建议修改

1. **按索引区分**：在 `TileIndexMapper` 或解析阶段，根据 `MiddleIndex`/`BackIndex` 判断是树还是墙，而不是仅依赖 0x8000。
2. **LowWall 细分**：对 `middleImg & 0x8000` 的格子，再根据 `MiddleIndex` 映射为 Tree 或 Wall。
3. **参考 Crystal**：对照 `Server/MirEnvir/Map.cs` 中如何用索引区分树、墙、建筑。

---

## 三、Mir2Cell 字段说明

| 字段 | 类型 | 含义 |
|------|------|------|
| Attribute | CellAttribute | Walk / HighWall / LowWall / Door |
| BackIndex | short | 背景层 WIL 资源编号 |
| BackImage | int | 背景层图片索引（0x8000=高墙） |
| MiddleIndex | short | 中间层 WIL 编号（树、建筑等） |
| MiddleImage | short | 中间层图片索引（0x8000=矮墙） |
| FrontIndex | short | 前景层 WIL 编号 |
| FrontImage | short | 前景层图片索引 |
| FishingCell | bool | 是否为水域 |

---

## 四、传奇地表贴图应用

### 方式一：WIL 提取（推荐）

1. 从传奇客户端获取 `Tiles.wil` 与 `Tiles.wix`（通常在 DATA 目录）
2. 菜单 **Tools > Legendary > Extract WIL to Textures**
3. 选择 `Tiles.wil`，贴图将导出到 `StreamingAssets/LegendaryData/Textures/`
4. 执行 **Tools > Legendary > Create Block Prefabs** 以应用

### 方式二：手动放置

将已提取的 `Block_Ground.png` 等放入 `Assets/StreamingAssets/LegendaryData/Textures/`，再执行 Create Block Prefabs。存在同名文件时优先使用传奇贴图源。

---

## 五、待办

- [x] 传奇地表贴图应用（WIL 提取 + 外部贴图源）
- [ ] 扩展 FishingCell 判断：`light >= 100 && light <= 119`
- [ ] 按 MiddleIndex/BackIndex 区分树与墙
- [ ] 对照 Crystal Map.cs 完善索引→块类型映射表
