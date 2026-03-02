# 传奇地形项目 - 后续规划

> 基于当前完成状态与 game-plan 的 Phase 对齐。

## 当前状态（2026-03-02）

- 场景管理：地形、怪物、角色、可破坏块
- 性能：分块、网格合并、GPU Instancing、远距剔除
- 视觉：织梦岛风格贴图与低多边形树/房子
- 数据：.map v0–v7/v100、RespawnInfo、FishingCell

---

## 短期（1–2 周）

### 1. 可破坏交互闭环
- 角色攻击：射线检测前方方块，调用 `BlockController.TakeDamage`
- 或 `DestructibleTool` 组件，供后续武器系统调用
- 可选：破坏掉落（占位物、经验）

### 2. 怪物 Prefab 完善
- 从 Crystal.Database Envir 提取怪物贴图/模型
- 完善 `MonsterPrefabConfig` 的 MonsterIndex 映射
- 怪物占位 Cube 替换为实际 Prefab

### 3. 出生点与多地图
- 解析 `MapInfo.SafeZones` 作为玩家出生点
- 多地图切换：场景加载/卸载、传送门占位

---

## 中期（1–2 月）

### 4. WIL/WIX 贴图转换
- 解析 Envir 下 WIL/WIX 资源
- 将 Back/Middle/Front 索引映射到真实贴图
- 替换程序化贴图为传奇原版风格（可选）

### 5. 怪物 AI 基础
- 简单移动/巡逻
- 玩家进入范围追击
- 基础攻击与受击

### 6. 角色与战斗
- 角色属性（生命、攻击、防御）
- 普攻与技能占位
- 与怪物交互（受击、死亡）

---

## 长期（对齐 game-plan Phase）

### Phase 1 收尾（MVP）
- 单地图可玩：移动、破坏、怪物、简单战斗
- 存档：场景破坏状态、角色位置
- 输入：键鼠 + 手柄抽象层

### Phase 2 数值架构
- 经验、等级、属性
- 装备、强化、镶嵌
- 经济与交易

### Phase 3 内容与玩法
- 副本、BOSS
- 社交、组队、公会
- 创造模式

### Phase 4 优化与发布
- 性能基准、自动化测试
- 合规、上架、本地化

---

## 建议优先级

| 优先级 | 任务 | 理由 |
|--------|------|------|
| P0 | 可破坏交互 | 闭环核心玩法，依赖少 |
| P0 | 出生点/SafeZones | 多地图基础 |
| P1 | 怪物 Prefab 映射 | 提升观感，数据已有 |
| P1 | 多地图切换 | 扩展内容 |
| P2 | WIL 贴图 | 美术升级，工作量大 |
| P2 | 怪物 AI | 玩法深度 |

---

## 文件与产出

- 本规划：`docs/plans/roadmap-next.md`
- 参考：`docs/plans/2026-03-02-legendary-terrain-design.md`、`docs/plans/game-plan.md`
