# Lua 架构与配置/逻辑分离

## 配置与逻辑分离

项目采用**配置与逻辑分离**的设计：

| 类型 | 位置 | 格式 | 用途 |
|------|------|------|------|
| **配置（数据）** | `Data/Prototypes/*.lua` | `return { TagName = { ... } }` | 原型定义：家具、物品、需求等静态数据 |
| **逻辑（函数）** | `LUA/*.lua` | `function Foo(...) ... end` | 可执行逻辑：门动画、建造判定、事件处理等 |

### 原则

- **配置**：只返回 Lua 表，不包含函数或复杂逻辑；由 `LuaPrototypeConverter` 转为 JToken 供 C# 解析
- **逻辑**：定义函数，通过 `FunctionsManager.{Domain}.Call(functionName, args)` 由 C# 调用

### 扩展 Mod 时

- 新增原型：在 Mod 的 `Prototypes/` 下添加 `.lua` 配置
- 新增逻辑：在 Mod 的 `LUA/` 下添加 `.lua` 脚本，或通过 `LoadFunctions` 加载

## 统一调用入口

所有 Lua 调用均通过 `FunctionsManager` 集中管理：

```csharp
FunctionsManager.Furniture.Call("IsEnterable_Door", furniture);
FunctionsManager.TileType.Call("CanBuildHere_Ladder", tile);
```

## 错误与日志

- Lua 调用统一在 `LuaFunctions.Call` 内 try-catch
- 错误日志格式：`[脚本名] 函数名(参数类型列表) 错误信息`
- 便于定位脚本、函数和参数类型

## 沙箱

- Lua 脚本使用 `CoreModules.Preset_SoftSandbox`
- 禁用：`io`、`os.execute`、`load`/`loadfile`/`require`
- 保留：string、math、table、coroutine、pcall、时间函数等
- 降低 Mod 脚本执行危险操作的风险
