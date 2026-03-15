# Slay the Spire 2 RL Agent — Research Notes

## 1. STS2 技术架构

| 项目 | 详情 |
|------|------|
| 游戏引擎 | Godot 4（从 Unity 迁移，因 Unity 2023 年 per-install 收费政策） |
| 编程语言 | C# / .NET 8 |
| 核心文件 | `sts2.dll`（所有游戏逻辑） |
| 代码混淆 | 无，类名/方法名/游戏逻辑完全可读 |
| 资源格式 | `.pck` 文件（Godot 标准打包格式） |
| 怪物动画 | Spine 骨骼动画（`.skel` + `.atlas` + `.png`） |

与 STS1 对比：

| 方面 | STS1 | STS2 |
|------|------|------|
| 引擎 | libGDX | Godot 4 |
| 语言 | Java | C# / .NET 8 |
| 游戏文件 | `.jar`（ZIP 包含 `.class`） | `.pck` + `sts2.dll` |
| 反编译工具 | CFR / IntelliJ / JD-GUI | ILSpy + GDRE Tools |
| Mod 加载 | ModTheSpire + BaseMod | 原生 mods 文件夹 + BaseLib-StS2 |
| 自动化接口 | CommunicationMod（stdin/stdout JSON） | 暂无 |
| Bot 框架 | spirecomm (Python), bottled_ai | 暂无 |

## 2. 反编译方法

### 2.1 提取资源（.pck 文件）

工具：[GDRE Tools (gdsdecomp)](https://github.com/GDRETools/gdsdecomp)

```bash
gdre_tools --headless --recover=<path-to-pck>
```

- 提取约 9,947 个文件：图片、Spine 骨骼动画、本地化数据、Godot 场景/资源文件
- 备选工具：[Godot PCK Explorer](https://dmitriysalnikov.itch.io/godot-pck-explorer)、[godotdec](https://github.com/Bioruebe/godotdec)

### 2.2 反编译 C# DLL（游戏逻辑）

工具：[ILSpy](https://github.com/icsharpcode/ILSpy)（推荐）或其命令行版本 `ilspycmd`

```bash
ilspycmd -p -o <output-dir> sts2.dll
```

- 产出约 3,300 个可读 C# 源文件
- 关键命名空间：`MegaCrit.Sts2.Core.Models.Powers/`、卡牌定义、遗物定义、怪物 AI 等
- 备选工具：dnSpy、dotPeek (JetBrains)

## 3. 现有 Modding 生态

### 3.1 Mod 安装方式

- 在游戏目录创建 `mods` 文件夹，放入 `.dll` + `.pck` 文件
- 游戏会显示 "Running Modded"，使用独立存档
- Steam Workshop 支持已确认/计划中

### 3.2 关键 Modding 工具

| 工具 | 说明 |
|------|------|
| [BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) | Mod 基础库（类似 STS1 的 BaseMod） |
| [Harmony](https://github.com/pardeike/Harmony) | C# 运行时方法 hook 库（注意 Mac ARM64 有 bug） |
| GUMM | 另一种 Mod 加载方案 |
| R2Modman / Thunderstore | 社区 Mod 管理器 |

### 3.3 有用的现有项目

| 项目 | 说明 |
|------|------|
| [spire-codex](https://github.com/ptrlrd/spire-codex) | 从反编译代码提取结构化数据（卡牌/遗物/怪物/药水），含 20 个 Python 解析器 |
| [BetterSpire2](https://www.nexusmods.com/slaythespire2/mods/2) | QoL mod（伤害计数器、自动确认、快速模式等） |
| DevConsole mod | 启用内置开发者控制台（`card`、`block`、`act`、`afflict` 等命令） |
| [Nexus Mods (STS2)](https://www.nexusmods.com/slaythespire2) | Mod 托管平台 |

## 4. RL 环境搭建方案

### 4.1 路线 A：Mod 桥接（Prototype / 验证用）

构建一个 C# mod，模仿 STS1 的 [CommunicationMod](https://github.com/ForgottenArbiter/CommunicationMod) 方案：

1. 使用 **Harmony** hook 游戏状态更新方法（`sts2.dll` 中的关键类）
2. 将游戏状态序列化为 JSON
3. 通过 TCP socket / named pipe 暴露给外部 Python 进程
4. Python 端运行 RL agent，发送动作指令

**优点**：直接与真实游戏交互，状态准确
**缺点**：速度受限于游戏渲染帧率，无法满足 RL 训练需要的百万级对局

### 4.2 路线 B：Headless 模拟器（训练用，推荐）

基于反编译的 C# 源码，用 Python 重现核心游戏逻辑，提供 Gymnasium 接口。

STS1 的先例项目：

| 项目 | 语言 | 说明 |
|------|------|------|
| [decapitate-the-spire](https://github.com/jahabrewer/decapitate-the-spire) | Python | 无头模拟器，gym-style `step()` API，专注 Silent/Exordium |
| [conquer-the-spire](https://github.com/utilForever/conquer-the-spire) | C++ | 跨平台模拟器 |
| [MiniStS](https://github.com/iambb5445/MiniSTS) | Python | 简化版实现，AAAI/AIIDE 2024 发表 |
| Miles Oram 的 C++ 重制 | C++ | 完整 Ironclad 4 幕体验 + Deep RL |

**关键经验**（来自 decapitate-the-spire 作者）：直接连接游戏做 RL 训练不可行，必须用无头模拟器才能达到每秒数千局的训练速度。

### 4.3 推荐方案

两条路线结合使用：

```
路线 B（Headless 模拟器）——用于大规模 RL 训练
         ↓ 训练完成后
路线 A（Mod 桥接）——用于在真实游戏中验证 agent 效果
```

## 5. RL 算法选择

### 5.1 推荐：PPO + Invalid Action Masking

| 算法 | 适用性 | 说明 |
|------|--------|------|
| **PPO** | **推荐** | 策略梯度方法，稳定性好，已在多个 STS 项目验证有效 |
| DQN | 不推荐 | 在大动作空间下不稳定，研究表明在 UNO/麻将/斗地主中效果差 |
| AlphaZero | 不直接适用 | 设计用于完全信息博弈，STS 有隐藏信息（牌序、敌人意图） |
| NFSP / CFR | 不推荐 | 计算成本高，更适合多人对抗博弈（如德州扑克） |
| Two-Step RL | 值得探索 | 将问题分为构建期和战斗期两个阶段 |

关键论文：[A Closer Look at Invalid Action Masking in Policy Gradient Algorithms](https://arxiv.org/abs/2006.14171)

### 5.2 动作掩码（Action Masking）

STS 每回合合法动作不同（手牌不同、目标不同、药水等），必须使用动作掩码：

- 定义最大可能动作空间
- 每步通过 `action_masks()` 返回布尔数组，将非法动作概率置零后重新归一化
- sb3-contrib 提供 `MaskablePPO` 开箱即用

注意：动作空间超过 ~1400 时可能有数值精度问题。

## 6. RL 框架选择

| 框架 | 推荐度 | 说明 |
|------|--------|------|
| **[SB3](https://github.com/DLR-RM/stable-baselines3) + [sb3-contrib](https://sb3-contrib.readthedocs.io/en/master/modules/ppo_mask.html)** | **入门首选** | 内置 MaskablePPO，API 简洁，PyTorch 后端，95% 代码覆盖率 |
| [Ray RLlib](https://docs.ray.io/en/latest/rllib/) | 大规模训练 | 分布式训练，原生动作掩码支持，学习曲线较陡 |
| [RLCard](https://rlcard.org/) | 参考设计 | 卡牌游戏 RL 专用工具包，状态格式标准化 |
| [OpenSpiel](https://github.com/google-deepmind/open_spiel) | 研究参考 | DeepMind 出品，70+ 游戏环境，支持不完全信息博弈 |
| [Gymnasium](https://gymnasium.farama.org/) | 接口标准 | 所有自定义环境都应实现 Gymnasium 接口 |

## 7. 观测空间设计

### 7.1 需要编码的状态组件

```
玩家状态：HP / 最大 HP / 格挡 / 能量
手牌：按卡牌类型计数（bag-of-cards 编码）
抽牌堆：已知卡牌组成（未知顺序）
弃牌堆：卡牌组成
消耗堆：卡牌组成
敌人：HP / 格挡 / 意图（伤害/增益/减益） / 状态效果
玩家状态效果：力量、敏捷、易伤、虚弱等
遗物：二值向量（持有/未持有）
药水：当前持有
楼层 / 幕数
金币
地图状态（路径选择用）
```

### 7.2 编码方式参考

| 方式 | 说明 | 来源 |
|------|------|------|
| One-hot 卡牌向量 | 每张卡映射为二值向量（1x234） | LearnTheSpire |
| Bag-of-cards | 每种卡牌类型的数量 | 多个 ML 项目通用 |
| 二值遗物向量 | 每个遗物 0/1 | Tilburg University 研究 |
| 降维 | 压缩为抽象概念（如"持续伤害能力"） | decapitate-the-spire |

### 7.3 已知陷阱

- **手牌索引问题**：「手牌位置 4」每回合含义不同，agent 必须学习索引与实际卡牌效果的映射关系。建议编码卡牌语义而非原始索引。
- **状态空间巨大**：需要仔细选择特征，避免维度灾难。

## 8. 动作空间设计

固定大小离散空间，覆盖所有可能动作：

```
战斗中：
  - 打出卡牌 N（目标 M）   — N ∈ [0, max_hand_size), M ∈ [0, max_enemies)
  - 使用药水 P（目标 M）    — P ∈ [0, max_potions)
  - 结束回合

非战斗：
  - 选择卡牌奖励 K          — K ∈ [0, max_card_choices) 或跳过
  - 地图节点导航
  - 商店购买/移除
  - 事件选项选择
  - 使用/丢弃药水
```

每步用 `action_masks()` 屏蔽当前不可用的动作。

## 9. Reward Shaping

| 信号 | 说明 |
|------|------|
| 通关 | 大正奖励 |
| 死亡 | 大负奖励 |
| 击杀怪物 | 小正奖励 |
| 推进楼层 | 小正奖励 |
| 损失 HP | 小负奖励（鼓励无伤通关） |
| 获得遗物/升级卡牌 | 可选小正奖励 |

**警告**：过度 reward shaping 可能导致 agent 利用奖励漏洞（如故意拖延战斗以获取更多击杀奖励）。需要谨慎平衡。

## 10. 关键论文与资源

### 学术论文

| 论文 | 会议/来源 | 链接 |
|------|-----------|------|
| Language-Driven Play: LLMs as Game-Playing Agents in StS | FDG 2024 | [ACM](https://dl.acm.org/doi/10.1145/3649921.3650013) |
| MiniStS: A Testbed for Dynamic Rule Exploration | AAAI AIIDE/EXAG 2024 | [GitHub](https://github.com/iambb5445/MiniSTS) |
| Strategic Delegation: A Modular and Hybrid Agent | agents4science 2025 | [OpenReview](https://openreview.net/pdf?id=gC3D2ESSyK) |
| LLMs May Not Be Human-Level Players, But They Can Be Testers | arXiv 2024 | [arXiv:2410.02829](https://arxiv.org/html/2410.02829v1) |
| Predicting a Successful Run in StS | Tilburg University | [论文](http://arno.uvt.nl/show.cgi?fid=169629) |
| A Closer Look at Invalid Action Masking | arXiv 2020 | [arXiv:2006.14171](https://arxiv.org/abs/2006.14171) |
| Two-Step RL for Multistage Strategy Card Game | arXiv 2023 | [arXiv:2311.17305](https://arxiv.org/html/2311.17305v1) |
| Playing Non-Embedded Card-Based Games with RL | arXiv 2025 | [arXiv:2504.04783](https://arxiv.org/html/2504.04783v1) |

### 博客

| 文章 | 链接 |
|------|------|
| Creating an AI for Slay the Spire（PPO/A2C 实战） | [toypiper.com](https://www.toypiper.com/creating-an-ai-for-slay-the-spire/) |
| Tackling UNO with RL | [Towards Data Science](https://towardsdatascience.com/tackling-uno-card-game-with-reinforcement-learning-fad2fc19355c/) |
| Training an AI for Dominion（deck-building RL） | [ianwdavis.com](https://ianwdavis.com/dominion2.html) |

### GitHub 项目

| 项目 | 说明 |
|------|------|
| [decapitate-the-spire](https://github.com/jahabrewer/decapitate-the-spire) | STS1 Python 无头模拟器 |
| [conquer-the-spire](https://github.com/utilForever/conquer-the-spire) | STS1 C++ 模拟器 |
| [MiniStS](https://github.com/iambb5445/MiniSTS) | STS 简化版 Python 实现 |
| [spire-codex](https://github.com/ptrlrd/spire-codex) | STS2 反编译数据提取 |
| [CommunicationMod](https://github.com/ForgottenArbiter/CommunicationMod) | STS1 游戏-外部进程通信桥 |
| [spirecomm](https://github.com/ForgottenArbiter/spirecomm) | STS1 Python 通信库 |
| [bottled_ai](https://github.com/xaved88/bottled_ai) | STS1 bot（52% Watcher 胜率） |
| [BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) | STS2 Mod 基础库 |
| [GDRE Tools](https://github.com/GDRETools/gdsdecomp) | Godot 反编译工具 |

## 11. spire-codex 项目深度分析

[spire-codex](https://github.com/ptrlrd/spire-codex) 是一个完整的 STS2 反编译数据管道 + REST API + 前端，线上地址 [spire-codex.com](https://spire-codex.com)。

### 11.1 架构总览

```
spire-codex/
├── backend/
│   ├── app/
│   │   ├── main.py                 FastAPI 应用（CORS、限速、静态文件）
│   │   ├── models/schemas.py       Pydantic 数据模型（19 种实体）
│   │   ├── parsers/                20 个 Python 正则解析器 + orchestrator
│   │   ├── routers/                22 个 FastAPI 路由模块
│   │   └── services/data_service.py  lru_cache JSON 加载器
│   ├── static/images/              卡牌/遗物/药水/怪物图片 PNG
│   └── requirements.txt
├── frontend/                       Next.js 16 + TypeScript + Tailwind CSS
├── tools/
│   ├── spine-renderer/             Spine 骨骼动画无头渲染器（Node.js）
│   ├── diff_data.py                版本变更日志生成器
│   └── update.py                   全流程驱动脚本
├── data/                           输出：20 个解析后的 JSON 文件
│   ├── cards.json, monsters.json, relics.json, ...
│   └── changelogs/
└── extraction/                     未提交到 git，需自行生成
    ├── raw/                        GDRE 提取的 Godot 资源（~9,947 文件）
    │   ├── images/                 卡牌肖像、遗物、药水图片
    │   ├── animations/             Spine .skel + .atlas + .png
    │   └── localization/eng/       JSON 本地化文件（名称、描述、SmartFormat 模板）
    └── decompiled/                 ILSpy 反编译的 C#（~3,300 .cs 文件）
        └── MegaCrit.Sts2.Core.Models.{Cards,Characters,Monsters,...}/
```

### 11.2 已提取的游戏数据

`data/` 目录下已有完整的结构化 JSON，**无需运行解析器即可直接使用**：

| 类别 | 文件 | 数量 | 对 RL 的价值 |
|------|------|------|-------------|
| 卡牌 | `cards.json` | 576 | **核心** — 费用/类型/稀有度/伤害/格挡/命中次数/施加状态/关键词/升级增量/所属角色 |
| 怪物 | `monsters.json` | 111 | **核心** — HP 范围（普通+高难）/招式列表/每招伤害值（普通+高难）/格挡值 |
| 遭遇 | `encounters.json` | 87 | **核心** — 怪物组合/房间类型/所属幕数 |
| 状态效果 | `powers.json` | 260 | **核心** — Buff/Debuff 类型/叠加方式（Counter/Single/None） |
| 遗物 | `relics.json` | 289 | **重要** — 稀有度/角色池/描述 |
| 角色 | `characters.json` | 5 | **重要** — 初始 HP/金币/能量/牌组/遗物 |
| 药水 | `potions.json` | 63 | **重要** — 稀有度/角色池 |
| 幕 | `acts.json` | 4 | **重要** — Boss 池/遭遇池/事件池/古代 NPC |
| 事件 | `events.json` | 66 | **有用** — 选项/结果/完整决策树 |
| 附魔 | `enchantments.json` | 22 | **有用** — 卡牌类型限制/是否可叠加 |
| 关键词 | `keywords.json` | 8 | 参考 |
| 意图 | `intents.json` | 14 | 参考 |
| 宝珠 | `orbs.json` | 5 | 参考 |
| 苦难 | `afflictions.json` | 9 | 参考 |
| 修饰符 | `modifiers.json` | 16 | 参考 |
| 成就 | `achievements.json` | 33 | 无关 |
| 高难等级 | `ascensions.json` | 11 | 参数化难度 |
| 纪元 | `epochs.json` | 若干 | 解锁系统 |
| 故事 | `stories.json` | 若干 | 解锁系统 |

角色列表：**Ironclad、Silent、Defect、Necrobinder、Regent**（5 个可玩角色）
幕：**Overgrowth（Act 1）、Hive（Act 2）、Glory（Act 3）、Underdocks**

### 11.3 JSON 数据结构示例

**卡牌** (cards.json)：
```json
{
  "id": "ABRASIVE",
  "name": "Abrasive",
  "description": "Gain 1 [gold]Dexterity[/gold].\nGain 4 [gold]Thorns[/gold].",
  "description_raw": "Gain {DexterityPower:diff()} ...",
  "cost": 3,
  "type": "Power",
  "rarity": "Rare",
  "target": "Self",
  "color": "silent",
  "damage": null,
  "block": null,
  "hit_count": null,
  "powers_applied": [
    {"power": "Thorns", "amount": 4},
    {"power": "Dexterity", "amount": 1}
  ],
  "keywords": ["Sly"],
  "tags": null,
  "vars": {"ThornsPower": 4, "Thorns": 4, "DexterityPower": 1, "Dexterity": 1},
  "upgrade": {"thornspower": "+2"}
}
```

**怪物** (monsters.json)：
```json
{
  "id": "ASSASSIN_RUBY_RAIDER",
  "name": "Assassin Raider",
  "type": "Normal",
  "min_hp": 18, "max_hp": 23,
  "min_hp_ascension": 19, "max_hp_ascension": 24,
  "moves": [{"id": "KILLSHOT", "name": "Killshot"}],
  "damage_values": {"Killshot": {"normal": 11, "ascension": 12}},
  "block_values": null
}
```

**富文本格式**：描述中使用 `[gold]text[/gold]`、`[red]`、`[energy:N]`、`[star:N]` 等标记。清理用正则：`\[/?[a-z]+(?::\d+)?\]`

### 11.4 解析器工作原理

所有解析器从两个来源提取数据：
- **反编译 C#**：`extraction/decompiled/MegaCrit.Sts2.Core.Models.{Category}/`
- **本地化 JSON**：`extraction/raw/localization/eng/{category}.json`

**ID 生成规则**（所有实体统一）：
```python
def class_name_to_id(name: str) -> str:
    # PascalCase → UPPER_SNAKE_CASE
    s = re.sub(r'(?<=[a-z0-9])(?=[A-Z])', '_', name)
    s = re.sub(r'(?<=[A-Z])(?=[A-Z][a-z])', '_', s)
    return s.upper()
# FrogKnight → FROG_KNIGHT
```

**核心解析器详解**：

| 解析器 | 关键正则/逻辑 |
|--------|--------------|
| `card_parser.py` | 从构造函数提取 `base(cost, CardType, CardRarity, TargetType)`；扫描 `DamageVar`/`BlockVar` 提取数值；从 `CanonicalKeywords` 提取关键词；从 pool 文件确定角色颜色 |
| `monster_parser.py` | 从 `MinInitialHp => AscensionHelper.GetValueIfAscension(level, asc, normal)` 提取 HP；从 `new MoveState("NAME", ...)` 提取招式；从 `(\w+)Damage =>` 提取伤害值 |
| `description_resolver.py` | **共享核心**：`extract_vars_from_source()` 从 C# 提取所有数值变量；`resolve_description()` 解析 SmartFormat 模板（条件、复数、图标等） |
| `event_parser.py` | 最复杂 — 解析多页事件决策树、StringVar 解析、随机范围值、古代 NPC 对话 |

**解析器依赖关系**：
```
pool_parser → 必须在 potion_parser 之后运行（读取 data/potions.json 补充 pool 信息）
event_parser → 读取 data/relics.json 丰富遗物描述
encounter_parser ↔ monster_parser → 互相引用确定怪物类型和幕数归属
```

### 11.5 API 接口

FastAPI 后端（端口 8000），限速 60 req/min/IP：

```
GET /api/{category}          — 列表（支持过滤）
GET /api/{category}/{id}     — 单个实体
GET /api/stats               — 各类实体数量统计
```

过滤参数：
- 卡牌：`color`, `type`, `rarity`, `keyword`, `search`
- 遗物/药水：`rarity`, `pool`, `search`
- 怪物：`type`（Normal/Elite/Boss）, `search`
- 遭遇：`room_type`, `act`
- 事件：`type`, `act`
- 状态效果：`type`, `stack_type`

也可直接访问 https://spire-codex.com/api/

### 11.6 依赖与环境

**后端**：FastAPI + uvicorn + Pydantic（SQLAlchemy 在 requirements.txt 中但未使用，计划未来迁移数据库）
**前端**：Next.js 16 + TypeScript + Tailwind CSS
**提取工具**（外部）：GDRE Tools v2.4.0、ILSpy CLI v9.1.0（`dotnet tool install ilspycmd -g`）、Python 3.10+、Node.js 20+

### 11.7 对 RL Agent 项目的价值

**可直接利用的部分**：

1. **`data/*.json`** — 576 张卡牌、111 个怪物、87 个遭遇、260 个状态效果的完整结构化数据，可直接用于构建 headless 模拟器的静态数据层
2. **解析器代码** — 如果游戏更新，可重新运行解析器获取最新数据
3. **ID 命名约定** — UPPER_SNAKE_CASE，可作为模拟器的统一 ID 系统
4. **描述解析器** — `description_resolver.py` 的 SmartFormat 解析逻辑可复用于理解卡牌效果
5. **角色初始数据** — `characters.json` 提供了完整的初始状态（HP/金币/能量/牌组/遗物）

**仍需自行实现的部分**：

1. **战斗逻辑** — spire-codex 只提取静态数据，不包含战斗流程（回合结构、伤害计算公式、状态效果触发时机）
2. **怪物 AI** — `monsters.json` 只有招式名和伤害值，不包含 AI 决策逻辑（需从反编译 C# 中提取）
3. **遗物触发逻辑** — 只有描述文本，需从 C# 源码中理解触发条件和效果
4. **地图生成** — 需从反编译代码中理解地图生成算法
5. **事件效果** — `events.json` 有选项文本但缺少具体数值效果的结构化表示

### 11.8 C# 命名空间 → 解析器映射

构建模拟器时，需要重点阅读的反编译目录：

| C# 命名空间目录 | 对应解析器 | 模拟器用途 |
|----------------|-----------|-----------|
| `MegaCrit.Sts2.Core.Models.Cards/` | card_parser | 卡牌效果实现 |
| `MegaCrit.Sts2.Core.Models.Monsters/` | monster_parser | 怪物 AI 逻辑 |
| `MegaCrit.Sts2.Core.Models.Powers/` | power_parser | 状态效果逻辑 |
| `MegaCrit.Sts2.Core.Models.Relics/` | relic_parser | 遗物触发逻辑 |
| `MegaCrit.Sts2.Core.Models.Encounters/` | encounter_parser | 遭遇组合 |
| `MegaCrit.Sts2.Core.Models.Events/` | event_parser | 事件决策树 |
| `MegaCrit.Sts2.Core.Models.Characters/` | character_parser | 角色初始状态 |
| `MegaCrit.Sts2.Core.Models.Potions/` | potion_parser | 药水效果 |
| `MegaCrit.Sts2.Core.Models.{Card,Relic,Potion}Pools/` | pool_parser | 角色→物品映射 |

## 12. 建议的实施路线图

### Phase 1：反编译与理解
1. 安装 ILSpy，反编译 `sts2.dll`
2. 研究游戏状态模型（`MegaCrit.Sts2.Core.Models.*`）
3. 参考 spire-codex 的数据提取方式
4. 整理卡牌/遗物/怪物/状态效果的完整列表

### Phase 2：Headless 模拟器
1. 用 Python 实现核心战斗逻辑（参考 decapitate-the-spire 架构）
2. 先实现单角色、第一幕
3. 实现 Gymnasium 接口（`reset()`, `step()`, `action_masks()`）
4. 编写单元测试，与真实游戏行为对照验证

### Phase 3：RL 训练
1. 使用 SB3 + sb3-contrib 的 MaskablePPO
2. 设计观测空间（flat vector，分段编码）
3. 设计动作空间（固定大小 + 动作掩码）
4. 实验 reward shaping 方案
5. 先在简化场景训练（固定 deck、单场战斗），再扩展

### Phase 4：真实游戏验证
1. 构建 C# Mod（Harmony hook + JSON 序列化 + TCP 通信）
2. 在真实游戏中运行训练好的 agent
3. 对比模拟器与真实游戏的行为差异，迭代修正
