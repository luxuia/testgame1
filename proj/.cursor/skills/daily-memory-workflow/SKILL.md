---
name: daily-memory-workflow
description: Use when the user asks to save daily memory, record today's work, update memory, 记录到今天的memory, 总结今天做的事情, or update long-term memory. Applies to projects with memory/ and MEMORY.md structure.
---

# 每日记忆与长期记忆更新

## 概述

本 skill 指导 Agent 按项目的「热-温-冷」三层记忆结构，保存当日记录并更新长期记忆。项目结构见 `MEMORY.md` 与 `memory/topics/memory-system.md`。

## 三层结构速查

| 层级 | 路径 | 作用 |
|------|------|------|
| L1 热 | `memory/YYYY-MM-DD.md` | 当日流水，全量、不筛选 |
| L2 温 | `memory/topics/<主题>.md` | 按主题沉淀高价值内容 |
| L3 冷 | `MEMORY.md` | 长期有效 P0/P1 原则 |

## 工作流

### 1. 保存每日记忆 (L1)

当用户要求「记录今天」「总结今天」「保存到 memory」时：

1. 获取今日日期：`YYYY-MM-DD`（如 2025-03-02）
2. 若 `memory/YYYY-MM-DD.md` 不存在，则创建：
   ```markdown
   # YYYY-MM-DD 当日记录 (L1)

   ## 决策
   - [用户/会话中提到的决策]

   ## 执行
   - [完成的任务、修改的文件]

   ## 结果
   - [关键结果、报错与修复]

   ## 补充
   - [其他值得记录的内容]
   ```
3. 若已存在，则**追加**新内容到对应小节或新增「补充」小节
4. **L1 不筛选**：全量写入，不做价值判断

### 2. 更新长期记忆 (L2/L3)

**L2 主题提炼**：当用户要求「提炼」「沉淀」「整理到主题」时：

- 从最近的 `memory/YYYY-MM-DD.md` 中抽取高价值、可复用内容
- 写入或追加到 `memory/topics/<主题>.md`

**L3 原则升级**：当用户要求「更新长期记忆」「更新 MEMORY」时：

- 仅将跨项目适用、长期有效的 P0/P1 级原则、事实、关键结论写入 `MEMORY.md`
- 保持 `MEMORY.md` 极简、稳定、低噪音

## 快速参考

| 用户意图 | 行为 |
|----------|------|
| 记录今天 / 总结今天 | 创建或追加 `memory/YYYY-MM-DD.md` |
| 提炼到主题 | 从 daily 抽取 → `memory/topics/<主题>.md` |
| 更新长期记忆 | 提炼 P0/P1 → `MEMORY.md` |

## 常见错误

- **不要**在 L1 做价值筛选：全量写入
- **不要**把琐碎细节写入 L3：MEMORY.md 只保留原则
- **不要**覆盖已有 daily 文件：追加内容，除非用户明确要求重写
