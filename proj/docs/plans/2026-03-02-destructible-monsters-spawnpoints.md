# 可破坏交互、怪物 Prefab、出生点与多地图 实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 实现可破坏方块攻击交互、完善怪物 Prefab 映射、解析 SafeZones 作为出生点，并为多地图切换打基础。

**Architecture:** 在 LegendaryCharacterController 中增加攻击输入与射线检测，命中可破坏块时调用 BlockController.TakeDamage；MirDBParser 解析 SafeZones 并写入 Mir2MapInfo；SceneManager 使用首个 StartPoint 作为出生点；MonsterPrefabConfig 扩展常见 MonsterIndex 映射，占位 Cube 由 Resources 预置 Prefab 替代。

**Tech Stack:** Unity 2022+, Physics.Raycast, BlockRegistry, MirDBParser, MonsterPrefabConfig

---

## Task 1: 可破坏交互 - 攻击射线检测与 TakeDamage

**Files:**
- Modify: `Assets/LegendaryTerrain/Runtime/LegendaryCharacterController.cs`
- Modify: `Assets/LegendaryTerrain/Runtime/BlockRegistry.cs`（如需 GetBlockAt 查询）

**Step 1: 在 LegendaryCharacterController 中添加攻击输入**

在 `Update` 中检测攻击键（如 `Fire1` / 鼠标左键），调用新方法 `TryAttack()`。

**Step 2: 实现 TryAttack 射线检测**

- 从角色位置 + 高度偏移，沿当前朝向（或移动方向，无移动时用 lastDir）发射射线
- 射线长度约 2 格（2 * BlockSize）
- 使用 `Physics.Raycast`，Layer 包含可破坏块
- 命中 `BlockController` 时调用 `TakeDamage(damageAmount)`，默认 25

**Step 3: 确保 BlockController 有 Collider**

- Block Prefab 已有 Collider（BlockController 有 `[RequireComponent(typeof(Collider))]`）
- 确认射线可命中（Layer、LayerMask）

**Step 4: 提交**

```bash
git add Assets/LegendaryTerrain/Runtime/LegendaryCharacterController.cs
git commit -m "feat: 攻击射线检测，命中可破坏块调用 BlockController.TakeDamage"
```

---

## Task 2: 怪物 Prefab - 完善 MonsterIndex 映射

**Files:**
- Modify: `Assets/LegendaryTerrain/Resources/LegendaryTerrain/MonsterPrefabConfig.asset`（或通过 Editor 扩展）
- Create: `Assets/LegendaryTerrain/Resources/LegendaryTerrain/Monster_*.prefab`（占位 Prefab，可先用 Capsule/简单模型）

**Step 1: 确定常见 MonsterIndex**

参考 Crystal.Database Envir 或 Server.MirDB 中的 Monster 列表。常见索引示例：0=鸡、1=鹿、2=狼等。可先映射 0–10 作为占位。

**Step 2: 创建占位怪物 Prefab**

- 在 `Resources/LegendaryTerrain/` 下创建 `Monster_0.prefab`、`Monster_1.prefab` 等
- 使用 Capsule 或简单模型，带 MonsterController、DistanceCulling

**Step 3: 配置 MonsterPrefabConfig**

在 `MonsterPrefabConfig.asset` 的 `_entries` 中添加：
- MonsterIndex: 0, ResourcesPath: "LegendaryTerrain/Monster_0"
- MonsterIndex: 1, ResourcesPath: "LegendaryTerrain/Monster_1"
- …（至少 5–10 个常见索引）

**Step 4: 验证 MonsterSpawner**

运行场景，确认怪物刷新点显示对应 Prefab 而非 Cube。

**Step 5: 提交**

```bash
git add Assets/LegendaryTerrain/Resources/LegendaryTerrain/
git commit -m "feat: MonsterPrefabConfig 映射常见 MonsterIndex，占位 Prefab 替代 Cube"
```

---

## Task 3: 出生点与 SafeZones 解析

**Files:**
- Modify: `Assets/LegendaryTerrain/Runtime/Mir2RespawnData.cs`（添加 SafeZone 结构）
- Modify: `Assets/LegendaryTerrain/Runtime/MirDBParser.cs`
- Modify: `Assets/LegendaryTerrain/Runtime/SceneManager.cs`（LegendarySceneManager）

**Step 1: 添加 Mir2SafeZone 与 Mir2MapInfo.SafeZones**

```csharp
// Mir2RespawnData.cs
[Serializable]
public class Mir2SafeZone
{
    public Vector2Int Location;
    public ushort Size;
    public bool StartPoint;
}
// Mir2MapInfo 添加:
public List<Mir2SafeZone> SafeZones = new List<Mir2SafeZone>();
```

**Step 2: MirDBParser 解析 SafeZones**

在 `ReadMapInfo` 中，将 SafeZones 循环从 `reader.ReadInt32(); reader.ReadInt32()` 改为：
```csharp
info.SafeZones.Add(new Mir2SafeZone
{
    Location = new Vector2Int(reader.ReadInt32(), reader.ReadInt32()),
    Size = reader.ReadUInt16(),
    StartPoint = reader.ReadBoolean()
});
```

**Step 3: SceneManager 使用 SafeZones 作为出生点**

在 `GetSpawnPosition()` 中：
- 若 `mapInfo?.SafeZones` 非空，优先取 `StartPoint == true` 的首个 SafeZone 的 Location
- 转换为世界坐标：`Location.x * BlockSize, 0.5f, Location.y * BlockSize`
- 若无 StartPoint，取首个 SafeZone 的 Location
- 若无 SafeZones，退回 `(0, 0.5f, 0)`

**Step 4: 提交**

```bash
git add Assets/LegendaryTerrain/Runtime/Mir2RespawnData.cs MirDBParser.cs SceneManager.cs
git commit -m "feat: 解析 SafeZones，玩家出生点使用 StartPoint"
```

---

## Task 4: 多地图切换占位（可选，为后续扩展）

**Files:**
- Modify: `Assets/LegendaryTerrain/Runtime/SceneManager.cs`
- Create: `Assets/LegendaryTerrain/Runtime/MapTransitionTrigger.cs`（占位）

**Step 1: 解析 Movements 为传送门数据（可选）**

MirDBParser 已跳过 Movements。若需多地图，可添加 `Mir2Movement` 与 `MapInfo.Movements`，解析 Source/Destination 地图与坐标。

**Step 2: 创建 MapTransitionTrigger 占位组件**

- 挂载到场景中的传送点 GameObject
- 字段：`TargetMapFileName`、`TargetLocation`
- 玩家进入 Trigger 时，调用 `LegendarySceneManager.LoadMap(targetMap, targetLocation)`（待实现）

**Step 3: SceneManager 预留 LoadMap 接口**

```csharp
public void LoadMap(string mapFileName, Vector2Int? spawnLocation = null)
{
    // 卸载当前地形/怪物，加载新地图，设置出生点
    // 本任务可仅预留方法签名与 TODO
}
```

**Step 4: 提交**

```bash
git add Assets/LegendaryTerrain/Runtime/
git commit -m "feat: 多地图切换占位，MapTransitionTrigger 与 LoadMap 接口"
```

---

## 建议执行顺序

| 顺序 | Task | 依赖 |
|------|------|------|
| 1 | Task 1 可破坏交互 | 无 |
| 2 | Task 3 SafeZones | 无 |
| 3 | Task 2 怪物 Prefab | 无 |
| 4 | Task 4 多地图占位 | Task 3 |

---

## 参考

- Crystal Map.cs: `Server/MirEnvir/Map.cs`
- MirDB 格式: `MirDBParser.cs` 注释
- roadmap-next.md 短期规划
