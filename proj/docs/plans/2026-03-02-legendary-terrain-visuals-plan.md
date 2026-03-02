# 传奇地形视觉升级 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 LegendaryTerrain 贴图与模型升级为织梦岛风格：地形/水用方块+层次贴图，树/房子用低多边形模型。

**Architecture:** 在 CreateLegendaryBlockPrefabs 中扩展程序化贴图生成（32×32 瓦片纹理）和程序化模型构建（树=圆柱+球体，房子=立方+三角屋顶）。材质对 Water 启用透明混合。

**Tech Stack:** Unity Editor C#、Texture2D 程序化生成、Mesh 组合、URP Lit/Transparent

---

## Task 1: 程序化贴图生成（Ground/Wall/Door/Bridge）

**Files:** Modify: `Assets/LegendaryTerrain/Editor/CreateLegendaryBlockPrefabs.cs`

**Step 1:** 在 `CreateTextures` 前添加辅助方法 `CreateTiledTexture(name, width, height, generator)`，其中 generator 为 `Func<int,int,Color>`。

**Step 2:** 实现 `CreateGrassTexture`：32×32，草地色块（绿/黄绿/深绿 2–3 色块拼接），每 4×4 像素随机选色，轻微噪点。

**Step 3:** 实现 `CreateStoneTexture`：32×32，灰褐色块，砖缝分割线（每 8 像素一条细线）。

**Step 4:** 实现 `CreateWoodTexture`：32×32，木纹横向条纹（棕色/深棕交替）。

**Step 5:** 实现 `CreateBridgeTexture`：32×32，类似木板纹理。

**Step 6:** 将 `CreateSolidTexture` 调用替换为上述新纹理调用（Ground→Grass, Wall→Stone, Door→Wood, Bridge→Bridge）。SpawnMarker 保留纯色 16×16。

**Step 7:** 删除 `CreateSolidTexture` 中对 Block_Ground/Wall/Door/Bridge 的旧调用，改为调用新方法。保留 Block_Water 单独处理（Task 2）。

---

## Task 2: 水贴图与透明材质

**Files:** Modify: `Assets/LegendaryTerrain/Editor/CreateLegendaryBlockPrefabs.cs`

**Step 1:** 实现 `CreateWaterTexture`：32×32，半透明蓝（rgba 0.2,0.4,0.7,0.6），中心略亮、边缘略暗，可加轻微波纹噪点。

**Step 2:** 实现 `CreateTransparentMaterial(matName, texName)`：使用 URP Lit，设置 Surface Type = Transparent，Blend Mode = Alpha。

**Step 3:** 将 Mat_Water 改为使用 `CreateTransparentMaterial`。

**Step 4:** 若 URP Lit 默认不透明，需通过 `material.SetFloat("_Surface", 1)` 和 `material.SetFloat("_Blend", 0)` 等设置；或使用 `Shader.Find("Universal Render Pipeline/Lit")` 后设置 `renderQueue` 和 `RenderType`。

---

## Task 3: 低多边形树模型

**Files:** Modify: `Assets/LegendaryTerrain/Editor/CreateLegendaryBlockPrefabs.cs`

**Step 1:** 添加 `CreateTreePrefab()`：创建空 GameObject，命名为 Block_Tree。

**Step 2:** 创建树干：`GameObject.CreatePrimitive(PrimitiveType.Cylinder)`，scale (0.2, 0.3, 0.2)，位置 Y=0.3；材质 Mat_Tree（树干用深棕，可新建 Mat_TreeTrunk 或复用）。

**Step 3:** 创建树冠：`GameObject.CreatePrimitive(PrimitiveType.Sphere)`，scale (0.8, 0.8, 0.8)，位置 Y=0.7；材质 Mat_Tree（绿色）。

**Step 4:** 将树干、树冠设为 Block_Tree 的子物体，保存为 Prefab 到 ResourcesPath。

**Step 5:** 在 `CreatePrefabs` 中，将 Tree 的创建从 `CreateBlockPrefab("Block_Tree", ...)` 改为调用 `CreateTreePrefab()`。

---

## Task 4: 低多边形房子模型

**Files:** Modify: `Assets/LegendaryTerrain/Editor/CreateLegendaryBlockPrefabs.cs`

**Step 1:** 添加 `CreateHousePrefab()`：创建空 GameObject，命名为 Block_House。

**Step 2:** 创建底座：`GameObject.CreatePrimitive(PrimitiveType.Cube)`，scale (0.8, 0.5, 0.8)，位置 Y=0.25；材质 Mat_House。

**Step 3:** 创建屋顶：`GameObject.CreatePrimitive(PrimitiveType.Cube)`，旋转 45° 绕 Y 形成菱形，再绕 X 倾斜形成三角屋顶；或使用 3 个三角形组成简单屋顶 Mesh。简化方案：用 1 个 Cube scale (0.9, 0.4, 0.9) 位置 Y=0.7，旋转 45° 绕 Y 形成“屋顶”效果。

**Step 4:** 若需更明显三角屋顶，可改用 `CreateRoofMesh()`：4 个顶点组成三角锥，或使用 `Mesh.CombineMeshes` 组合简单几何。

**Step 5:** 将底座、屋顶设为 Block_House 子物体，保存为 Prefab。

**Step 6:** 在 `CreatePrefabs` 中，将 House 的创建改为调用 `CreateHousePrefab()`。

---

## Task 5: 颜色与风格微调

**Files:** Modify: `Assets/LegendaryTerrain/Editor/CreateLegendaryBlockPrefabs.cs`

**Step 1:** 调整颜色为织梦岛风格：草地偏黄绿/青绿，石头偏暖灰，水偏蓝绿半透明。

**Step 2:** 确保树、房子 Prefab 的 pivot 在底部中心，便于生成器放置。

**Step 3:** 运行 `Tools > Legendary > Create Block Prefabs`，确认生成成功。

**Step 4:** 运行 `Tools > Legendary > Generate Terrain from Map`，检查场景中草地、石墙、水、树、房子的外观。

---

## 验证

- 菜单 `Tools > Legendary > Create Block Prefabs` 无报错
- 菜单 `Tools > Legendary > Generate Terrain from Map` 生成成功
- 场景中可见：草地纹理、石墙纹理、半透明水、低多边形树、低多边形房子
