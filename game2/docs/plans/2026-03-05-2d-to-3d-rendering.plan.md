# Project Porcupine 2D→3D 渲染实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 Project Porcupine 的场景渲染从 2D（Tilemap + Sprite）改为 3D（体素 Mesh），参考并照搬 MinecraftClone 的渲染管线代码。

**Architecture:** 保留 World/Tile/Furniture 数据层，新增 WorldTileAccessor 适配器将 Tile 映射为 BlockData，移植 BlockMeshBuilder、MeshBuilder 等生成 3D Mesh，用 MeshRenderer 替代 Tilemap。

**Tech Stack:** Unity、C#、Mesh API、Graphics.DrawMesh

---

## 前置：依赖与命名空间

- 新建命名空间 `ProjectPorcupine.Rendering3D` 或直接放在全局（与 PP 一致）
- 移除 MinecraftClone 的 `ILuaCallCSharp`、`XLua` 等依赖，改为空接口或删除
- `MathUtility.RotatePoint` 可内联或新建 `RenderingMathUtility`

---

### Task 1: 复制基础配置与枚举

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/BlockFace.cs`
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/BlockFaceCorner.cs`
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/PhysicState.cs`（简化版，仅 Solid/Fluid）
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/BlockFlags.cs`

**Step 1:** 从 MinecraftClone 复制 `BlockFace`、`BlockFaceCorner` 枚举定义

**Step 2:** 复制 `PhysicState`（仅保留 Solid、Fluid、Air 等 BlockMeshBuilder 用到的）

**Step 3:** 复制 `BlockFlags`（BlockData 用到的 AlwaysInvisible 等）

**参考路径:** `ref_proj/MinecraftClone-Unity-master/Assets/Scripts/Configurations/`

---

### Task 2: 复制 BlockVertexData、BlockMesh、BlockData 结构

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/BlockVertexData.cs`
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/BlockMesh.cs`
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/BlockData.cs`
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/PhysicSystem/AABB.cs`（BlockMesh 用）
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/PhysicSystem/PhysicMaterial.cs`（可选，BlockData 引用）

**Step 1:** 复制并移除 `ILuaCallCSharp`、`XLua` 特性

**Step 2:** 确保 BlockData 包含：ID, InternalName, LightValue, LightOpacity, PhysicState, Mesh, Material, Textures

**Step 3:** BlockMesh 包含：Pivot, BoundingBox, Faces（FaceData[]）

---

### Task 3: 复制 MeshBuilder 与 BlockMeshVertexData

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/BlockMeshVertexData.cs`
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/MeshBuilder.cs`

**Step 1:** 复制 `BlockMeshVertexData`，移除 `ILuaCallCSharp`、`[XLua.GCOptimize]`

**Step 2:** 复制 `MeshBuilder<TVertex, TIndex>`，移除 `ILuaCallCSharp`

**Step 3:** 确认 `Unity.Collections`、`UnityEngine.Rendering` 引用存在（Unity 自带）

---

### Task 4: 复制 LightingUtility 与 MathUtility

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/LightingUtility.cs`
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/MathUtility.cs`

**Step 1:** 复制 `LightingUtility`，移除 `[XLua.LuaCallCSharp]`，`IWorldRAccessor` 改为接口（后续定义）

**Step 2:** 复制 `MathUtility`（仅 RotatePoint、RoundToInt 等 BlockMeshBuilder 用到的）

---

### Task 5: 定义 IWorldRAccessor 接口

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/IWorldRAccessor.cs`

**Step 1:** 定义接口（与 MinecraftClone 一致）：
```csharp
public interface IWorldRAccessor
{
    bool Accessible { get; }
    Vector3Int WorldSpaceOrigin { get; }
    BlockData GetBlock(int x, int y, int z, BlockData defaultValue = null);
    Quaternion GetBlockRotation(int x, int y, int z, Quaternion defaultValue = default);
    int GetMixedLightLevel(int x, int y, int z, int defaultValue = 0);
    int GetSkyLight(int x, int y, int z, int defaultValue = 0);
    int GetAmbientLight(int x, int y, int z, int defaultValue = 0);
    int GetTopVisibleBlockY(int x, int z, int defaultValue = 0);
}
```

---

### Task 6: 复制 BlockMeshBuilder

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/BlockMeshBuilder.cs`

**Step 1:** 复制 `BlockMeshBuilder`，将 `IWorldRAccessor` 引用改为本项目的接口

**Step 2:** `accessor.World.BlockDataTable.GetMesh` 需改为通过参数传入 BlockTable/GetMesh 委托，或新建 `ITileBlockTable`

**Step 3:** 移除 `PhysicState` 中不存在的引用，确保 `ClipFace` 等逻辑可编译

---

### Task 7: 创建 TileBlockTable（TileType → BlockData 映射）

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/TileBlockTable.cs`

**Step 1:** 新建类，在 `SpriteManager.Initialize` 之后、`TileSpriteController` 构造时初始化

**Step 2:** 遍历 `PrototypeManager.TileType.Values`，为每个 TileType 创建 BlockData：
- InternalName = tileType.Type
- Mesh = 0（使用默认立方体 BlockMesh）
- Material = 0
- Textures = 根据 SpriteManager.GetSprite("Tile", type) 的贴图索引
- LightOpacity = 15（不透明）
- PhysicState = Solid

**Step 3:** 提供 `GetBlockData(string tileType)`、`GetMesh(int index)`、`GetMaterial(int index)`、`GetTextureArray()`

**Step 4:** 创建默认立方体 BlockMesh（6 面，每面 4 顶点 6 索引）

---

### Task 8: 创建 WorldTileAccessor（World → IWorldRAccessor）

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/WorldTileAccessor.cs`

**Step 1:** 实现 `IWorldRAccessor`，内部持有 `World world`、`TileBlockTable table`

**Step 2:** 坐标映射：PP (x,y,z) → 3D (x, z, y)，即 `GetBlock(ppX, ppY, ppZ)` 内部调用 `world.GetTileAt(ppX, ppY, ppZ)`

**Step 3:** `GetBlock`：若 Tile.Type == Empty 返回 null；否则 `table.GetBlockData(tile.Type.Type)`；若 Tile.Furniture != null，可返回家具对应的 BlockData（或先仅处理 Tile）

**Step 4:** `GetTopVisibleBlockY`：对固定世界，遍历 y 从 Depth-1 到 0，找到第一个非空 Tile 的 y

**Step 5:** `GetSkyLight`/`GetAmbientLight`：先返回固定值（如 15），不做光照

---

### Task 9: 创建 AddWorldSection 扩展（遍历 World 生成 Mesh）

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Rendering3D/TileMeshBuilderExtensions.cs`

**Step 1:** 实现 `AddWorld(this BlockMeshBuilder<TIndex> builder, World world, WorldTileAccessor accessor)`：
- 遍历 world 的每个 Tile (x,y,z)
- 坐标转换为 3D：(x, z, y)
- 若 Tile 非空，调用 `builder.AddBlock(pos3D, Vector3Int.zero, blockData, accessor)`

**Step 2:** Accessor 的坐标空间需与 builder 一致（世界坐标）

---

### Task 10: 创建 TileMeshController（替代 TileSpriteController）

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Controllers/Rendering3D/TileMeshController.cs`
- Modify: `ProjectPorcupine/Assets/Scripts/Controllers/WorldController.cs`

**Step 1:** 新建 `TileMeshController`：
- 持有 `World world`、`TileBlockTable table`、`WorldTileAccessor accessor`
- 持有 `Mesh terrainMesh`、`Material terrainMaterial`
- 在构造函数或 `Initialize` 中：创建 BlockMeshBuilder，调用 AddWorld，ApplyAndClearBuffers 到 terrainMesh
- 提供 `RebuildMesh()` 方法，供 Tile 变化时调用

**Step 2:** 在 `Update` 或通过 `world.OnTileChanged` 在变化时调用 `RebuildMesh()`

**Step 3:** 使用 `Graphics.DrawMesh(terrainMesh, Matrix4x4.identity, material, 0)` 或创建 GameObject + MeshFilter + MeshRenderer 渲染

**Step 4:** 在 WorldController.Start 中，用 `TileMeshController` 替代 `TileSpriteController`（或先并存，通过开关切换）

---

### Task 11: 贴图与 Shader

**Files:**
- Create/Copy: `ProjectPorcupine/Assets/Resources/Shaders/BlockTextureArray.shader`（或使用 MinecraftClone 的）
- Modify: `ProjectPorcupine/Assets/Scripts/Rendering3D/TileBlockTable.cs`

**Step 1:** 将 Tile 的 Sprite 贴图打包为 Texture2DArray（或使用 Sprite 的 texture + 裁剪 UV）

**Step 2:** 若 MinecraftClone Shader 依赖 Texture2DArray，复制 Shader 并调整

**Step 3:** 若无 Shader，可先用 Unity 标准 Shader + 单张贴图做 MVP

---

### Task 12: 相机改为 3D 透视

**Files:**
- Modify: `ProjectPorcupine/Assets/Scripts/Controllers/InputOutput/CameraController.cs`

**Step 1:** 将 `Camera.main.orthographic = true` 改为 `false`，设置 `fieldOfView`（如 60）

**Step 2:** 相机位置：从 `(Width/2, Height/2, z)` 改为 3D 坐标，如 `(Width/2, Depth*2, Height/2)` 俯视

**Step 3:** 移除或简化 `CreateLayerCameras`（3D 单相机即可）

**Step 4:** 调整 `GetTileAtWorldCoord`：使用射线检测或坐标换算，将 3D 世界坐标映射回 Tile (x,y,z)

---

### Task 13: 禁用原 2D 渲染

**Files:**
- Modify: `ProjectPorcupine/Assets/Scripts/Controllers/WorldController.cs`

**Step 1:** 注释或条件编译 `TileSpriteController` 的创建与使用

**Step 2:** 保留 `FurnitureSpriteController` 暂时或改为 3D 渲染（后续 Task）

**Step 3:** 移除 `objectParent` 中的 Grid、Tilemap 相关

---

### Task 14: Furniture 3D 渲染（可选，Phase 2）

**Files:**
- Create: `ProjectPorcupine/Assets/Scripts/Controllers/Rendering3D/FurnitureMeshController.cs`

**Step 1:** 将 Furniture 映射为 BlockData（或使用预制体实例化）

**Step 2:** 合并到同一 Mesh 或单独 DrawMesh

---

## 验证步骤

1. 运行游戏，生成新世界
2. 场景显示为 3D 体素地面（Tile 类型对应的立方体）
3. 相机可平移、缩放（改为 FOV/距离）
4. 修改 Tile 后 Mesh 能正确重建（若已接 OnTileChanged）

---

## 执行选项

**Plan complete and saved to `docs/plans/2026-03-05-2d-to-3d-rendering.plan.md`.**

**两种执行方式：**

1. **Subagent-Driven（本会话）** — 按 Task 分派子 agent，每步审查
2. **Parallel Session（新会话）** — 在新会话中用 executing-plans 批量执行

**选哪种？**
