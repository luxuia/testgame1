# 世界轴调整：XZ 平面 + Y 高度

> **目标**：将世界轴调整为标准 3D 约定——XZ 为水平面（地面），Y 为高度，为 3D 游戏做准备。

---

## 一、当前 vs 目标

### 当前 PP 约定

| PP 坐标 | 含义 | World 数组 |
|---------|------|------------|
| x | 水平维度 1 | 0..Width-1 |
| y | 水平维度 2 | 0..Height-1 |
| z | 层/楼层 | 0..Depth-1 |

- `World.GetTileAt(x, y, z)` → `tiles[x, y, z]`
- 2D 渲染时：`Vector3(tile.X, tile.Y, tile.Z)` 直接映射到 Unity

### 目标 3D 约定（Unity 标准）

| Unity 轴 | 含义 |
|----------|------|
| X | 水平（地面） |
| Z | 水平（地面） |
| Y | 高度（垂直） |

**映射关系：**
```
PP (x, y, z)  →  Unity (X, Y, Z)
Unity X = PP.x
Unity Y = PP.z   （层 → 高度）
Unity Z = PP.y   （水平维度 2）
```

---

## 二、需要修改的模块

### 2.1 坐标转换工具（新建）

- `Rendering3DCoordUtility.cs`
  - `PPToWorld(ppX, ppY, ppZ)` → `Vector3(ppX, ppZ, ppY)`
  - `WorldToPP(world)` → 反向转换

### 2.2 渲染位置

所有设置 `transform.position` 的地方，需用 `PPToWorld`：

| 文件 | 当前 | 调整后 |
|------|------|--------|
| CharacterSpriteController | `(X, Y, Z)` | `PPToWorld(X, Y, Z)` |
| FurnitureSpriteController | `tile.Vector3` | `PPToWorld(tile.X, tile.Y, tile.Z)` |
| UtilitySpriteController | `(X, Y, Z)` | `PPToWorld(X, Y, Z)` |
| InventorySpriteController | `(X, Y, Z)` | `PPToWorld(X, Y, Z)` |
| JobSpriteController | `tile.Vector3` | `PPToWorld(...)` |
| BuildModeController 预览 | `(x, y, layer)` | `PPToWorld(x, y, layer)` |

### 2.3 相机

- 相机俯视 XZ 平面：`position = (centerX, height, centerZ)`，`rotation = (90, 0, 0)`
- 移动轴：`(frameMoveHorizontal, 0, frameMoveVertical)`（X、Z）
- 边界限制：Clamp X 和 Z，不限制 Y

### 2.4 鼠标/选择

- 射线与 `y = CurrentLayer` 平面求交
- `CurrentFramePosition` 格式：`(worldX, worldZ, CurrentLayer)`（PP 格式）

### 2.5 数据层（保持不变）

- `World.GetTileAt(x, y, z)` 不变
- `Tile.X, Tile.Y, Tile.Z` 不变
- 仅渲染层做坐标转换

---

## 三、实施顺序

1. 新建 `Rendering3DCoordUtility`
2. 修改所有 SpriteController 的 `transform.position`
3. 修改相机初始化与移动
4. 修改鼠标射线与选择逻辑
5. 修改 BuildModeController 预览位置

---

## 四、兼容性

- **存档**：World/Tile 数据格式不变，无需迁移
- **旧存档相机**：若从 2D 存档加载，相机位置可能需手动调整
