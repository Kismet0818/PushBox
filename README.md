# PushBox

一个使用 Unity 6 制作的 2D 网格推箱子原型。关卡由 Tilemap 搭建，游戏运行时会读取墙壁、目标点和出生点并生成玩家与箱子。

## 已实现功能

- 回合制网格移动，可用 WASD 或方向键操作
- 墙体和箱子碰撞判定
- 箱子只能被推动一格，不能拉回或同时推动两个箱子
- 目标点高亮反馈
- 步数统计
- 撤销与重新开始
- 全部箱子到达目标点后显示过关信息
- 支持通过 Tilemap 快速搭建新关卡

## 环境要求

- Unity Editor `6000.6.3f1`
- Unity Input System `1.20.0`
- Universal Render Pipeline `17.6.0`

项目使用的完整依赖列表位于 `Packages/manifest.json`。

## 运行项目

1. 在 Unity Hub 中添加本项目目录。
2. 使用 Unity `6000.6.3f1` 打开项目，等待资源导入和脚本编译完成。
3. 打开 `Assets/Scenes/Level01.unity` 或 `Assets/Scenes/Level02.unity`。
4. 点击编辑器顶部的 Play 按钮。

`SampleScene` 是 Unity 模板示例场景，不包含当前项目的游戏逻辑。

## 操作方式

| 按键 | 功能 |
| --- | --- |
| `W` / `↑` | 向上移动 |
| `S` / `↓` | 向下移动 |
| `A` / `←` | 向左移动 |
| `D` / `→` | 向右移动 |
| `Z` | 撤销上一步 |
| `R` | 重新开始当前关卡 |

移动动画播放期间会忽略新的移动输入。过关后仍可撤销或重新开始。

## 游戏规则

- 玩家每次只能移动或推动一格。
- 玩家不能穿过墙壁或箱子。
- 当玩家前方是箱子时，箱子后方的格子必须同时满足不是墙、没有其他箱子。
- 当所有箱子都位于目标点上时，当前关卡完成。
- 撤销会恢复玩家和箱子的位置，并减少一步步数。

## 项目结构

```text
Assets/
  Art/                  项目使用的贴图与字体
  Editor/               编辑器扩展预留目录
  Palettes/             Tile Palette 相关资源
  Prefabs/              Box 和 Player 预制体
  Scenes/               SampleScene、Level01、Level02
  Scripts/              游戏运行时代码
  Settings/             URP、Input System 与场景模板设置
  Tiles/                Tile 资源
Packages/               Unity 包依赖
ProjectSettings/        Unity 项目设置
美术资源/               导入的像素美术素材包
_script_backup/         早期脚本与场景备份
```

主要脚本：

| 文件 | 职责 |
| --- | --- |
| `Assets/Scripts/Board.cs` | 保存墙壁、目标点、箱子占位和格子边界 |
| `Assets/Scripts/Box.cs` | 箱子落格、平滑移动和目标点状态刷新 |
| `Assets/Scripts/PlayerController.cs` | 读取方向输入、处理推箱判定、执行玩家和箱子动画 |
| `Assets/Scripts/LevelBuilder.cs` | 从 Tilemap 读取关卡并生成玩家和箱子 |
| `Assets/Scripts/GameManager.cs` | 管理步数、历史记录、撤销、重开和过关状态 |

## 关卡制作

正式关卡场景需要包含以下内容：

- 一个 `Grid` 对象。
- 挂在 `Grid` 对象上的 `LevelBuilder` 组件。
- 一个 `GameManager` 对象。
- `Wall`、`Target`、`SpawnBox`、`SpawnPlayer` 和 `Floor` Tilemap。
- 已配置的 `Box` 和 `PlayerController` 预制体。

在 `LevelBuilder` 组件中配置对应 Tilemap 和预制体后，运行场景即可自动生成关卡。

Tilemap 用途：

| Tilemap | 用途 |
| --- | --- |
| `Wall` | 墙壁和不可通行区域 |
| `Target` | 箱子目标点 |
| `SpawnBox` | 箱子初始位置 |
| `SpawnPlayer` | 玩家初始位置 |
| `Floor` | 地面视觉层 |

当前实现有以下约束：

- `Wall` Tilemap 的 `cellBounds` 决定棋盘范围。
- 目标点和箱子必须位于墙壁 Tilemap 的边界内。
- `SpawnPlayer` 中应只放置一个 Tile；存在多个时会使用遍历到的第一个位置。
- 推荐箱子数量与目标点数量一致。
- `SpawnBox` 和 `SpawnPlayer` 图层会在运行时自动隐藏。

## 构建项目

当前仓库的 `EditorBuildSettings` 仍只登记了 `SampleScene`。构建可执行版本前，需要在 Build Settings 或 Build Profiles 中手动加入正式关卡：

1. 打开 `File > Build Settings` 或 Unity 6 的 Build Profiles。
2. 添加 `Assets/Scenes/Level01.unity`。
3. 添加 `Assets/Scenes/Level02.unity`。
4. 将 `Level01` 调整为索引 `0`。
5. 禁用或移除 `SampleScene`。
6. 选择目标平台并执行 Build。

## 当前限制

- 过关后没有自动进入下一关的流程。
- 没有主菜单、选关界面或存档系统。
- 没有移动端触屏操作。
- 相机使用固定正交尺寸，新增不同尺寸的关卡时可能需要调整。
- 尚未提供自动化测试。
- `_script_backup` 保留了旧版实现，不属于当前可编译源码。

## 后续可改进项

- 增加关卡流程控制，在 `Level01` 完成后进入 `Level02`。
- 为 `LevelBuilder` 增加关卡数据校验和编辑器检查工具。
- 增加 Board、推箱、撤销和过关逻辑的 EditMode/PlayMode 测试。
- 根据棋盘尺寸自动调整相机。
- 增加动画、音效、移动反馈和过关界面。
- 增加移动端虚拟方向键。

## 资源说明

- 项目字体位于 `Assets/Art/Fonts/`。
- 项目贴图位于 `Assets/Art/`。
- `美术资源/2D Pixel Dungeon Asset Pack/` 为导入的像素地牢素材包。

正式发布前请确认所有第三方素材的授权和使用条款。
