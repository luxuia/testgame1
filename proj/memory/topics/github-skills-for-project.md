# 主题：适合本项目的 GitHub Skills 与资源

针对「传奇数值 + 动森渲染 + 我的世界沙盒」Unity 项目，整理的 GitHub 资源与 Cursor Skills。

---

## 一、Cursor / AI 开发辅助

### 1. Unity-MCP（强烈推荐）

- **仓库**：https://github.com/IvanMurzak/Unity-MCP
- **Stars**：1100+
- **作用**：通过 MCP 连接 Cursor/Claude/Windsurf 与 Unity Editor，50+ 内置工具
- **能力**：资产操作、场景/层级管理、GameObject 创建/修改、Prefab、包管理、运行时调试
- **安装**：`openupm add com.ivanmurzak.unity.mcp` 或下载 .unitypackage
- **适配**：沙盒建造、资产管线、关卡编辑、AI 辅助调试

### 2. CursorLink-Unity

- **仓库**：https://github.com/Cocolab-AI/CusorLink-Unity
- **作用**：将 Unity Console 输出接入 Cursor 上下文（Markdown）
- **能力**：实时日志、剪贴板集成、可过滤，便于 AI 辅助排错
- **适配**：调试、报错分析

### 3. Cursor Skills 合集（通用）

- **awesome-agent-skills**：https://github.com/VoltAgent/awesome-agent-skills（380+ skills）
- **aussiegingersnap/cursor-skills**：feature-build、documentation、versioning、skill-creator
- **安装**：Cursor 设置 → Rules → Add Rule → Remote Rule (GitHub)，输入仓库 URL
- **适配**：功能开发流程、文档、版本管理、自建 skill

---

## 二、沙盒 / 方块世界（我的世界向）

### 1. Minecraft4Unity

- **仓库**：https://github.com/paternostrox/Minecraft4Unity
- **Stars**：153
- **特点**：程序化世界生成、Greedy Meshing、Job System、存档/背包
- **流程**：3D Simplex Noise → Block Data → Mesh/Collider → Object Spawn
- **适配**：Phase 1 方块世界、地形生成、物品系统参考

### 2. Vektor Voxels

- **仓库**：https://github.com/VektorKnight/vektor-voxels
- **特点**：RGB 光照、多线程 Chunk、DDA 射线放置、自定义 Shader
- **适配**：光照系统、体素放置、性能优化

### 3. VoxelsV2

- **仓库**：https://github.com/KryKomDev/VoxelsV2
- **特点**：可配置程序化地形、GUI、Minecraft 风格世界生成
- **适配**：生物群系、噪声配置

---

## 三、卡通渲染（动森向）

### 1. BOTW 风格 Cel Shading

- **仓库**：https://github.com/daniel-ilett/shaders-botw-cel-shading
- **Stars**：113，MIT
- **特点**：塞尔达风格卡通着色，轮廓、渐变
- **适配**：动森风格渲染参考

### 2. URP Cel Shade Lit

- **仓库**：https://github.com/Robinseibold/Unity-URP-CelShadeLit
- **Stars**：117
- **特点**：URP 卡通着色、多光源、阴影
- **适配**：现代渲染管线

### 3. Anime-Style Cel Shader

- **仓库**：https://github.com/hatfullr/UnityURP-AnimeStyleCelShader
- **特点**：URP 动漫风格、后处理
- **适配**：温馨治愈风格

---

## 四、装备 / 数值系统（传奇向）

### 1. Player Inventory System

- **仓库**：https://github.com/fideltfg/Player-Inventory-System
- **特点**：背包、装备、合成、箱子、耐久、存档加密
- **要求**：Unity 2022.3+、Input System
- **适配**：装备、背包、合成系统

### 2. Smith Equipments

- **仓库**：https://github.com/Unity-LRTools/Smith-Equipments
- **特点**：装备创建、随机生成、品质、套装、编辑器工具
- **适配**：装备生成、品质、套装

### 3. Unity-RPG-Inventory

- **仓库**：https://github.com/rito15/Unity-RPG-Inventory
- **Stars**：18
- **适配**：RPG 背包基础实现

---

## 五、网络系统（多人）

### 1. Mirror

- **仓库**：https://github.com/MirrorNetworking/Mirror
- **Stars**：6000+
- **特点**：Unity 主流开源网络库，多传输、RPC、SyncVar、兴趣管理
- **适配**：计划中「权威服务器」网络方案

### 2. Netcode for GameObjects

- **仓库**：Unity 官方包
- **特点**：官方网络方案，与 Unity 服务集成
- **适配**：备选网络方案

---

## 六、推荐使用优先级

| 优先级 | 资源 | 阶段 | 用途 |
|-------|------|------|------|
| P0 | Unity-MCP | Phase 1 | AI 辅助开发、资产/场景操作 |
| P0 | Minecraft4Unity | Phase 1 | 方块世界、地形生成参考 |
| P0 | Mirror | Phase 2+ | 多人网络 |
| P1 | URP Cel Shade / BOTW Shader | Phase 1 | 动森风格渲染 |
| P1 | Player Inventory System | Phase 2 | 装备/背包 |
| P2 | CursorLink-Unity | 全阶段 | 调试辅助 |
| P2 | awesome-agent-skills | 全阶段 | 通用开发流程 |

---

## 七、Cursor Skill 安装方式

1. **远程 Rule**：Cursor 设置 → Rules → Add Rule → Remote Rule (GitHub) → 输入 URL
2. **本地复制**：`git clone <repo>` 后，将 `skills/<skill-name>/` 复制到 `.cursor/skills/`
3. **Unity-MCP**：在 Unity 中安装插件，在 Cursor 的 MCP 配置中连接

---

*最后更新：2025-03-02*
