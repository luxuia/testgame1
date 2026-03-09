# Project Porcupine 2D → 3D 场景渲染设计

> **目标**：参考 MinecraftClone 的代码，将 Project Porcupine 的场景渲染从 2D（Tilemap + SpriteRenderer）改为 3D（体素 Mesh 渲染）。

---

## 一、现状对比

### Project Porcupine（当前 2D）

| 组件 | 实现 |
|------|------|
| 世界数据 | `World` + `Tile[,,]`，固定尺寸 Width×Height×Depth |
| 地面/墙体 | `TileSpriteController` → Tilemap + TileBase |
| 家具 | `FurnitureSpriteController` → SpriteRenderer |
| 坐标 | X,Y 水平，Z 为层深（0=顶层） |
| 相机 | 正交、多层级相机（每层独立 near/far） |

### MinecraftClone（参考 3D）

| 组件 | 实现 |
|------|------|
| 世界数据 | `ChunkManager` + `Chunk`，16×256×16 区块 |
| 方块 | `BlockData` + `BlockMesh` + `BlockTable` |
| 渲染 | `SectionMeshManager` → `BlockMeshBuilder` → Mesh |
| 渲染管理 | `SectionRenderingManager`（视锥剔除、DrawMesh） |
| 坐标 | X,Z 水平，Y 为高度 |

---

## 二、设计方案：适配器 + 渲染管线移植

**核心思路**：保留 Project Porcupine 的 `World`、`Tile`、`Furniture` 数据层，仅替换渲染层。通过适配器将 Tile 数据映射为 BlockData，复用 MinecraftClone 的 Mesh 生成与渲染逻辑。

### 2.1 坐标映射

```
PP (X, Y, Z)  →  3D Unity (X, Z, Y)
- PP X → 3D X（水平）
- PP Y → 3D Z（水平）
- PP Z → 3D Y（层深 → 高度，0=地面）
```

### 2.2 架构

```
┌─────────────────────────────────────────────────────────────┐
│                    Project Porcupine 数据层                    │
│  World → Tile[,,] + FurnitureManager（保持不变）               │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  WorldRAccessor（适配器）                                      │
│  - GetBlock(x,y,z) → 从 Tile 映射为 BlockData                  │
│  - TileType → BlockData 映射表                                 │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  MinecraftClone 渲染管线（照搬）                               │
│  - TileMeshBuilder（基于 BlockMeshBuilder 简化）               │
│  - SectionMeshManager / 简化版（固定世界，无 Chunk 流式）        │
│  - SectionRenderingManager（DrawMesh）                        │
└─────────────────────────────────────────────────────────────┘
```

### 2.3 家具（Furniture）处理

- **方案 A**：家具作为「方块」渲染，每个 Furniture 占 1 格，映射为特殊 BlockData
- **方案 B**：家具保持独立 GameObject，使用 MeshRenderer + 预制体/贴图
- **推荐**：方案 A（与 Tile 统一用体素渲染），复杂家具可后续用预制体扩展

---

## 三、需要移植/适配的 MinecraftClone 代码

### 3.1 直接照搬

| 文件 | 用途 |
|------|------|
| `MeshBuilder.cs` | 通用 Mesh 构建 |
| `BlockMeshBuilder.cs` | 方块 Mesh 生成（面剔除、AO） |
| `BlockMeshVertexData.cs` | 顶点结构 |
| `BlockMesh.cs` | 方块几何定义 |
| `RenderingUtility.cs` | Section 工具、视锥平面 |
| `LightingUtility.cs` | 环境光遮蔽 |
| `BlockMeshBuilder.AddSection` 扩展 | Section 遍历逻辑 |

### 3.2 需适配

| 原组件 | 适配方式 |
|--------|----------|
| `IWorldRAccessor` | 新建 `WorldTileAccessor`，从 `World.GetTileAt` 读取并映射为 BlockData |
| `Chunk3x3Accessor` | 固定世界无需 Chunk，用 `WorldTileAccessor` 直接访问 |
| `BlockTable` | 新建 `TileBlockTable`，从 `PrototypeManager.TileType` + 贴图资源构建 |
| `SectionMeshManager` | 简化为「按层或按块重建 Mesh」，无异步 Chunk 加载 |
| `SectionRenderingManager` | 简化为单次/按层渲染，无视锥剔除（世界小） |

### 3.3 不移植

- ChunkManager、Chunk、ChunkBuilder（无流式世界）
- WorldGeneratePipeline、TerrainGenerator（保留 PP 的 WorldGenerator）
- AssetManager、Lua、XLua 等 MinecraftClone 特有依赖

---

## 四、资源与 Shader

- 用户已拷贝美术资源，需确认路径与格式
- MinecraftClone 使用 Texture2DArray + 自定义 Shader
- 需将 PP 的 Tile/Furniture 贴图打包为 Texture2DArray，或使用简化 Shader（单贴图/多贴图）

---

## 五、相机

- 从正交改为透视
- 保留平移、缩放（改为 FOV/距离）
- 层切换改为相机高度（Y）或裁剪

---

## 六、实施顺序建议

1. 建立 TileType → BlockData 映射与 `WorldTileAccessor`
2. 移植 MeshBuilder、BlockMeshBuilder、BlockMeshVertexData
3. 实现简化版 TileMeshController（替代 TileSpriteController）
4. 接入 Shader 与贴图
5. 相机改为 3D 透视
6. Furniture 映射为 BlockData 或独立 3D 渲染
7. 移除/禁用原 2D 渲染（Tilemap、SpriteController）

---

## 七、风险与简化

- **依赖**：MinecraftClone 的 `ILuaCallCSharp`、`MathUtility` 等，需替换或移除
- **光照**：可先不做光照，仅用固定环境光
- **性能**：固定小世界，可全量重建 Mesh，无需 Section 异步

---

**下一步**：编写详细实现计划（Task 粒度），按步骤执行。
