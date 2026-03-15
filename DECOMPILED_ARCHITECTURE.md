# STS2 反编译源码架构 — 模拟器实现指南

本文档基于对 `sts2.dll` 反编译后 C# 源码的深度分析，记录构建 Headless 模拟器所需的核心游戏机制。

---

## 1. 关键命名空间导航

```
extraction/decompiled/
├── MegaCrit.Sts2.Core.Combat/              战斗管理器、战斗状态、阵营
├── MegaCrit.Sts2.Core.Commands/            底层命令（伤害、格挡、卡牌、状态效果）
│   └── Builders/                           AttackCommand 构建器模式
├── MegaCrit.Sts2.Core.GameActions/         高层动作（打牌、结束回合）
├── MegaCrit.Sts2.Core.Entities.Creatures/  Creature 基类（HP、Block、Powers）
├── MegaCrit.Sts2.Core.Entities.Players/    PlayerCombatState（能量、星星、牌堆）
├── MegaCrit.Sts2.Core.Entities.Cards/      TargetType、CardKeyword、PileType
├── MegaCrit.Sts2.Core.Hooks/              Hook 静态类（中央事件分发）
├── MegaCrit.Sts2.Core.Models/             基类：AbstractModel、CardModel、MonsterModel、PowerModel、RelicModel
├── MegaCrit.Sts2.Core.Models.Cards/       全部卡牌实现（~576 张）
├── MegaCrit.Sts2.Core.Models.Monsters/    全部怪物 AI 实现（~111 个）
├── MegaCrit.Sts2.Core.Models.Powers/      全部状态效果实现（~260 个）
├── MegaCrit.Sts2.Core.Models.Relics/      全部遗物实现（~289 个）
├── MegaCrit.Sts2.Core.Models.Encounters/  遭遇定义（怪物分组）
├── MegaCrit.Sts2.Core.Models.Acts/        幕定义（Overgrowth, Underdocks, Hive, Glory）
├── MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine/  怪物 AI 状态机
├── MegaCrit.Sts2.Core.MonsterMoves.Intents/                  意图类型
├── MegaCrit.Sts2.Core.Map/               地图生成（StandardActMap, MapPoint）
└── MegaCrit.Sts2.Core.ValueProps/        ValueProp 标志位
```

---

## 2. 战斗系统

### 2.1 核心类关系

```
CombatManager (单例)
  └── CombatState
       ├── CombatSide (Player / Enemy)
       ├── RoundNumber (从 1 开始)
       ├── PlayerCombatState
       │    ├── Energy / MaxEnergy
       │    ├── Stars (STS2 新资源)
       │    ├── Hand, DrawPile, DiscardPile, ExhaustPile, PlayPile
       │    └── HasEnoughResourcesFor(card) — 检查能量+星星
       └── Creatures[] (怪物列表)
            ├── HP / MaxHP / Block
            └── Powers[] (状态效果列表)
```

### 2.2 回合流程

```
StartCombat()
  └── Hook.BeforeCombatStart (遗物触发，如 Anchor 获得 10 格挡)

循环每回合 {
  ┌─ 玩家回合 ────────────────────────────────────────┐
  │ StartTurn()                                        │
  │   ├── 清除格挡 (第 1 回合除外，受 Hook.ShouldClearBlock 控制) │
  │   ├── ResetEnergy() → 能量回满 MaxEnergy            │
  │   ├── Hook.ModifyHandDraw → 决定抽牌数量 (默认 5)    │
  │   └── 抽牌                                         │
  │                                                    │
  │ 玩家操作阶段（打牌 / 使用药水 / 结束回合）              │
  │                                                    │
  │ EndPlayerTurnPhaseOne()                            │
  │   ├── 弃置虚无(Ethereal)牌                          │
  │   └── 触发回合结束效果                               │
  │ EndPlayerTurnPhaseTwo()                            │
  │   ├── 弃置手牌 (保留 Retain 牌)                     │
  │   └── 状态效果 tick down                            │
  └────────────────────────────────────────────────────┘

  ┌─ 敌方回合 ────────────────────────────────────────┐
  │ SwitchFromPlayerToEnemySide()                      │
  │ ExecuteEnemyTurn()                                 │
  │   ├── 每个怪物清除格挡                               │
  │   ├── 每个怪物执行当前招式                            │
  │   └── 每个怪物滚动下一招式 (RollMove)                │
  │ EndEnemyTurn()                                     │
  │   └── 状态效果 tick down (Vulnerable, Weak, Frail)  │
  └────────────────────────────────────────────────────┘

  SwitchSides() → RoundNumber++
}
```

### 2.3 牌堆管理

```
5 个牌堆：
  Hand          — 手牌
  DrawPile      — 抽牌堆（打乱顺序）
  DiscardPile   — 弃牌堆
  ExhaustPile   — 消耗堆
  PlayPile      — 正在打出的牌（临时）

抽牌流程：
  DrawPile 空时 → DiscardPile 洗入 DrawPile → 继续抽

打牌后去向：
  默认 → DiscardPile
  有 Exhaust 关键词 → ExhaustPile
  状态牌(Status)/诅咒(Curse) → 取决于卡牌定义
```

---

## 3. 伤害计算公式

### 3.1 攻击伤害管线 (`Hook.ModifyDamageInternal`)

```python
def calculate_damage(base_damage, dealer, target, card_source, value_props):
    damage = base_damage

    # 第 1 步：附魔修正（加法 → 乘法）
    if card_source and card_source.enchantment:
        damage += enchantment.additive_modifier
        damage *= enchantment.multiplicative_modifier

    # 第 2 步：加法修正（遍历所有 hook listeners）
    # 例如：StrengthPower → 攻击者拥有力量时 damage += strength.amount
    # 仅对 "powered attack" 生效 (有 Move 标志 且 非 Unpowered)
    for listener in iterate_hook_listeners():
        damage += listener.modify_damage_additive(dealer, target, value_props)

    # 第 3 步：乘法修正（遍历所有 hook listeners）
    # 例如：VulnerablePower → 目标有易伤时 damage *= 1.5
    #       WeakPower → 攻击者有虚弱时 damage *= 0.75
    for listener in iterate_hook_listeners():
        damage *= listener.modify_damage_multiplicative(dealer, target, value_props)

    # 第 4 步：伤害上限检查
    # 例如：IntangiblePower → 将伤害限制为 1
    for listener in iterate_hook_listeners():
        damage = listener.modify_damage_cap(damage, dealer, target)

    # 第 5 步：下限 0
    damage = max(0, floor(damage))

    return damage
```

### 3.2 伤害结算 (`CreatureCmd.Damage`)

```python
def apply_damage(creature, damage, value_props):
    # 计算最终伤害
    final_damage = Hook.ModifyDamage(damage, dealer, creature, card_source, value_props)

    # 减去格挡（除非 Unblockable）
    if not value_props.has(Unblockable):
        blocked = min(creature.block, final_damage)
        creature.block -= blocked
        final_damage -= blocked

    # Osty 宠物重定向（STS2 新机制）
    final_damage = Hook.ModifyHpLostBeforeOsty(final_damage)

    # 其他未格挡伤害重定向
    final_damage = Hook.ModifyUnblockedDamageTarget(final_damage)

    # 最终 HP 修正
    final_damage = Hook.ModifyHpLostAfterOsty(final_damage)

    # 扣血
    creature.lose_hp(final_damage)
```

### 3.3 格挡计算 (`Hook.ModifyBlock`)

```python
def calculate_block(base_block, creature, card_source, value_props):
    block = base_block

    # 加法修正
    # 例如：DexterityPower → block += dexterity.amount
    for listener in iterate_hook_listeners():
        block += listener.modify_block_additive(creature, value_props)

    # 乘法修正
    # 例如：FrailPower → block *= 0.75
    for listener in iterate_hook_listeners():
        block *= listener.modify_block_multiplicative(creature, value_props)

    block = max(0, floor(block))
    return block
```

### 3.4 ValueProp 标志位

```
Move       (0x8)  — 来自卡牌或怪物招式
Unpowered  (0x4)  — 不受力量/虚弱/易伤影响
Unblockable(0x2)  — 绕过格挡

IsPoweredAttack() = 有 Move 标志 AND 没有 Unpowered 标志
```

只有 `IsPoweredAttack()` 为 true 时，Strength、Weak、Vulnerable 才生效。

---

## 4. 核心状态效果实现

### 4.1 伤害/格挡修正类

| 状态效果 | 类型 | Hook 方法 | 修正值 | 说明 |
|----------|------|-----------|--------|------|
| **Strength** | Buff | `ModifyDamageAdditive` | `+Amount` | 仅对攻击者的 powered attack 生效；允许负值 |
| **Dexterity** | Buff | `ModifyBlockAdditive` | `+Amount` | 允许负值 |
| **Vulnerable** | Debuff | `ModifyDamageMultiplicative` | `×1.5` | 目标受到更多伤害；可被 PaperPhrog 遗物增强为 +0.25 (即 ×1.75) |
| **Weak** | Debuff | `ModifyDamageMultiplicative` | `×0.75` | 攻击者造成更少伤害；可被 PaperKrane 遗物增强 |
| **Frail** | Debuff | `ModifyBlockMultiplicative` | `×0.75` | 获得更少格挡 |
| **Intangible** | Buff | `ModifyDamageCap` | 上限 1 | 所有伤害被限制为 1 |

### 4.2 状态效果的生命周期

```
Vulnerable, Weak, Frail:
  - 类型: Counter（计数器叠加）
  - 消退时机: AfterTurnEnd on CombatSide.Enemy（敌方回合结束时 -1）
  - 实际效果: 持续整个玩家回合 + 敌方回合结束时才消退

Strength, Dexterity:
  - 类型: Counter（计数器叠加）
  - 允许负值
  - 永久持续（不会自然消退）

Intangible:
  - 通常每回合消退
```

### 4.3 Power 基类结构

```csharp
abstract class PowerModel : AbstractModel
{
    PowerType Type;                    // Buff / Debuff / None
    PowerStackType StackType;          // Counter / Single / None
    bool AllowNegative;                // 是否允许负值叠加
    int Amount;                        // 当前叠加层数

    // ~100+ 可重写的 hook 方法（与 AbstractModel 共享）:
    virtual ModifyDamageAdditive(...)
    virtual ModifyDamageMultiplicative(...)
    virtual ModifyDamageCap(...)
    virtual ModifyBlockAdditive(...)
    virtual ModifyBlockMultiplicative(...)
    virtual AfterSideTurnStart(...)
    virtual AfterTurnEnd(...)
    virtual BeforeAttack(...)
    virtual AfterDamageGiven(...)
    virtual OnDeath(...)
    // ... 更多
}
```

---

## 5. 怪物 AI 系统

### 5.1 架构概览

```
MonsterModel (抽象基类)
  ├── GenerateMoveStateMachine() → 定义 AI 行为
  ├── 属性: MinInitialHp, MaxInitialHp (普通/高难分别定义)
  ├── 伤害值: XxxDamage => AscensionHelper.GetValueIfAscension(level, asc, normal)
  └── 招式: PerformMove(targets) 异步委托

MonsterMoveStateMachine
  └── Dictionary<string, MonsterState>
       ├── MoveState          — 实际招式（有 Intent[] 和 PerformMove 委托）
       ├── RandomBranchState  — 加权随机选择
       └── ConditionalBranchState — 条件分支（取第一个匹配）
```

### 5.2 状态机节点类型

**MoveState（招式节点）**：
```python
MoveState(
    name="Thrash",                    # 招式名
    intents=[SingleAttackIntent(...)], # 意图显示
    perform_move=lambda targets: ..., # 执行逻辑
    follow_up_state="NextState"       # 固定后继状态（可选）
)
```

**RandomBranchState（随机分支）**：
```python
RandomBranchState(
    branches=[
        (weight=75, state="Attack", repeat_rule=CannotRepeat),
        (weight=25, state="Defend", repeat_rule=CanRepeatForever),
    ]
)
```

重复规则：
- `CannotRepeat` — 不能连续选同一招
- `CanRepeatXTimes(n)` — 最多连续 n 次
- `UseOnlyOnce` — 整场战斗只用一次
- `CanRepeatForever` — 无限制
- Cooldown 冷却回合数

权重在选择时会根据 `StateLog`（历史记录）动态调整。

**ConditionalBranchState（条件分支）**：
```python
ConditionalBranchState(
    branches=[
        (condition=lambda: can_summon(), state="Summon"),
        (condition=lambda: True, state="Attack"),  # fallback
    ]
)
```

### 5.3 招式选择流程 (`RollMove`)

```python
def roll_move(state_machine):
    current = state_machine.current_state

    # 沿着状态链前进，直到到达一个 MoveState
    while True:
        next_state = current.get_next_state()
        if isinstance(next_state, MoveState):
            state_machine.current_move = next_state
            return
        elif isinstance(next_state, RandomBranchState):
            # 按权重随机选择，考虑重复约束和冷却
            current = weighted_random_select(next_state.branches, state_log)
        elif isinstance(next_state, ConditionalBranchState):
            # 取第一个条件为 true 的分支
            current = first_matching_branch(next_state.branches)
```

### 5.4 怪物 AI 模式示例

**固定循环型（Crusher Boss）**：
```
Thrash → Enlarging Strike → Bug Sting → Adapt → Guarded Strike → (循环)
实现方式：每个 MoveState 的 follow_up_state 指向下一个
```

**交替型（Chomper）**：
```
Clamp → Screech → Clamp → Screech → ...
实现方式：A.follow_up = B, B.follow_up = A
```

**随机型（TwoTailedRat）**：
```
RandomBranch:
  75% → Summon (条件: can_summon(), UseOnlyOnce)
  25% → Bite (CanRepeatForever)
  50% → Scratch (CannotRepeat)
```

**条件型**：
```
ConditionalBranch:
  HP < 50% → 使用治疗招式
  有小怪存活 → 使用增益招式
  default → 普通攻击
```

### 5.5 高难度 (Ascension) 修正

```python
# HP 使用 AscensionLevel.ToughEnemies
min_hp = AscensionHelper.GetValueIfAscension(
    AscensionLevel.ToughEnemies, ascension_value, normal_value
)

# 伤害使用 AscensionLevel.DeadlyEnemies
damage = AscensionHelper.GetValueIfAscension(
    AscensionLevel.DeadlyEnemies, ascension_value, normal_value
)
```

### 5.6 意图系统 (Intents)

```
SingleAttackIntent(damage)        — 单次攻击
MultiAttackIntent(damage, count)  — 多段攻击
BlockIntent(block)                — 获得格挡
BuffIntent                        — 增益
DebuffIntent                      — 减益
StrategicIntent                   — 策略（召唤、特殊）
EscapeIntent                      — 逃跑
SleepIntent                       — 休眠
UnknownIntent                     — 未知意图
```

---

## 6. 遗物触发系统

### 6.1 架构

```
RelicModel : AbstractModel
  ├── DynamicVars (参数化数值，如 BlockVar(10m))
  ├── Flash() — 触发视觉效果
  ├── Rarity: Starter, Common, Uncommon, Rare, Shop, Event, Ancient, None
  └── 重写 AbstractModel 的 hook 方法来实现触发逻辑
```

### 6.2 Hook 触发时机分类

| 时机 | Hook 方法 | 示例遗物 |
|------|-----------|---------|
| 战斗开始 | `BeforeCombatStart` | Anchor（获得 10 格挡） |
| 回合开始 | `AfterSideTurnStart` | Akabeko（第 1 回合获得 8 活力） |
| 打牌前 | `BeforeCardPlayed` | — |
| 打牌后 | `AfterCardPlayed` | — |
| 攻击前 | `BeforeAttack` | — |
| 造成伤害后 | `AfterDamageGiven` | — |
| 受到伤害后 | `AfterDamageTaken` | — |
| 伤害加法修正 | `ModifyDamageAdditive` | — |
| 伤害乘法修正 | `ModifyDamageMultiplicative` | PaperPhrog（Vulnerable ×1.75 而非 ×1.5） |
| 格挡修正 | `ModifyBlock` | — |
| 抽牌数修正 | `ModifyHandDraw` | — |
| 格挡清除决定 | `ShouldClearBlock` | — |
| 死亡时 | `OnDeath` | — |
| 治疗时 | `ModifyHeal` | — |
| 获得金币时 | `ModifyGoldGain` | — |

### 6.3 中央事件分发机制 (`Hook` 静态类)

```python
# 所有 AbstractModel 的子类（卡牌、遗物、状态效果、怪物、修饰符）
# 都可以注册为 hook listener

def hook_modify_damage(base, dealer, target, card_source, value_props):
    damage = base
    # 遍历所有已注册的 hook listeners
    for listener in CombatState.iterate_hook_listeners():
        # listener 可以是任何 AbstractModel 子类
        damage += listener.modify_damage_additive(dealer, target, value_props)
    for listener in CombatState.iterate_hook_listeners():
        damage *= listener.modify_damage_multiplicative(dealer, target, value_props)
    return damage
```

关键洞察：**卡牌、遗物、状态效果、怪物** 全部通过同一个 Hook 系统交互，它们都是 `AbstractModel` 的子类，共享相同的 ~100+ 虚方法。

---

## 7. 卡牌效果系统

### 7.1 卡牌基类结构

```csharp
abstract class CardModel : AbstractModel
{
    // 构造函数
    base(energyCost, CardType, CardRarity, TargetType)

    // 核心属性
    CardType Type;        // Attack, Skill, Power, Status, Curse, Quest
    TargetType Target;    // None, Self, AnyEnemy, AllEnemies, RandomEnemy, AnyAlly, AllAllies...
    CardRarity Rarity;    // Basic, Common, Uncommon, Rare, Ancient, Event, Token, Status, Curse, Quest
    int EnergyCost;
    int? StarCost;        // STS2 新资源消耗
    DynamicVars Vars;     // DamageVar, BlockVar, CardsVar 等

    // 核心方法
    abstract OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay);
    virtual OnUpgrade();
    virtual CanPlay();
    virtual IsValidTarget(target);
}
```

### 7.2 效果实现模式

**攻击牌**：
```csharp
// Strike 类
OnPlay(ctx, cardPlay) {
    DamageCmd.Attack(dynamicVars.Damage)
        .FromCard(this)
        .Targeting(target)
        .WithHitFx(...)
        .Execute(ctx);
}
```

**防御牌**：
```csharp
// Defend 类
OnPlay(ctx, cardPlay) {
    CreatureCmd.GainBlock(owner.Creature, dynamicVars.Block, cardPlay);
}
```

**状态效果牌**：
```csharp
// 施加力量
OnPlay(ctx, cardPlay) {
    PowerCmd.Apply<StrengthPower>(creature, amount, applier, cardSource);
}
```

**抽牌**：
```csharp
CardPileCmd.Draw(ctx, count, owner);
```

**生成状态牌**：
```csharp
// 给敌人弃牌堆加 Dazed
CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.DiscardPile, count);
```

**X 费牌**：
```csharp
int x = ResolveEnergyXValue();  // 消耗所有能量
// 用 x 作为重复次数或效果倍率
for (int i = 0; i < x; i++) { ... }
```

### 7.3 打牌完整流程 (`PlayCardAction`)

```python
def play_card(card, target, ctx):
    # 1. 验证
    assert card.can_play()
    assert card.is_valid_target(target)

    # 2. 消耗资源
    card.spend_resources()  # 扣能量 + 扣星星

    # 3. 执行
    Hook.before_card_played(card, target)
    card.on_play(ctx, card_play)
    if card.enchantment:
        card.enchantment.on_play(ctx, card_play)
    Hook.after_card_played(card, target)

    # 4. 检查重复打出（如 Double Tap 效果）
    play_count = Hook.modify_card_play_count(card)
    for i in range(1, play_count):
        card.on_play(ctx, card_play)  # 额外执行

    # 5. 牌的去向
    if card.has_keyword(Exhaust):
        move_to(ExhaustPile)
    else:
        move_to(DiscardPile)  # 默认
```

### 7.4 升级系统

```python
def upgrade_card(card):
    # 常见升级方式：
    card.damage_var.upgrade_value_by(3)      # 伤害 +3
    card.block_var.upgrade_value_by(3)        # 格挡 +3
    card.energy_cost.upgrade_by(-1)           # 费用 -1
    card.add_keyword(CardKeyword.Innate)      # 添加关键词
    card.remove_keyword(CardKeyword.Ethereal) # 移除关键词
```

---

## 8. 地图生成算法

### 8.1 地图结构 (`StandardActMap`)

```python
# 7 列 × mapLength 行 的网格
map_grid = MapPoint[7][map_length]   # map_grid[column][row]

# 特殊节点
boss_point = BossMapPoint           # 最后一行之后
second_boss_point = optional        # 某些情况有第二 Boss
```

### 8.2 路径生成算法

```python
def generate_paths(map_grid, num_paths=7):
    for i in range(num_paths):
        # 从第 1 行的随机列开始
        col = random_column(0..6)
        row = 1

        while row < map_length:
            # 创建节点
            map_grid[col][row] = MapPoint()

            # 连接到上一行的节点
            connect_to_parent(col, row)

            # 前进一行，随机偏移列（左/中/右）
            direction = random_choice([-1, 0, +1])
            new_col = clamp(col + direction, 0, 6)

            # 防止路径交叉
            if would_cross_another_path(col, new_col, row):
                new_col = col  # 保持直行

            col = new_col
            row += 1

        # 最后一行连接到 Boss 节点
        connect_to_boss(col)
```

### 8.3 房间类型分配

**固定分配**：
```
第 1 行          → Monster（首次遭遇）
倒数第 7 行      → Treasure（宝箱）或 Elite（高难模式替换）
Boss 前最后一行  → RestSite（篝火）
```

**随机分配池**（高斯分布）：
```
Elite:    ~5 个 (高难 ×1.6)
Shop:     3 个
Unknown:  ~12 个（问号房，可触发事件/战斗/商店）
RestSite: ~5 个
```

**放置规则**：
- 同类型特殊节点不能相邻（父子/兄弟关系）
- RestSite 和 Elite 不能出现在前 4 行
- RestSite 不能出现在后 3 行

### 8.4 房间类型枚举

```
Unassigned, Unknown, Shop, Treasure, RestSite, Monster, Elite, Boss, Ancient
```

### 8.5 各幕配置

| 幕 | 名称 | 房间数 | 特色 |
|-----|------|--------|------|
| Act 1 | Overgrowth | 15 | 入门遭遇 |
| Act 2 | Hive | — | 中期挑战 |
| Act 3 | Glory | — | 高难遭遇 |
| Act 4 | Underdocks | — | 最终挑战 |

每幕定义自己的：Boss 池、遭遇池、事件池、Ancient NPC

---

## 9. 能量与星星系统

### 9.1 能量 (Energy)

```python
# 每回合开始时
player.energy = player.max_energy     # 默认 3
player.energy += Hook.modify_energy() # 遗物/状态效果修正

# 打牌消耗
player.energy -= card.energy_cost

# X 费牌
x = player.energy  # 消耗所有剩余能量
player.energy = 0
```

### 9.2 星星 (Stars) — STS2 新机制

```python
# 某些卡牌需要同时消耗能量和星星
def has_enough_resources(card):
    return (player.energy >= card.energy_cost and
            player.stars >= card.star_cost)
```

---

## 10. 模拟器实现建议

### 10.1 优先级排序

```
P0 — 必须首先实现（最小可战斗原型）:
  ├── Creature (HP, Block, Powers)
  ├── 5 个牌堆 (Hand, Draw, Discard, Exhaust, Play)
  ├── 能量系统
  ├── 伤害公式 (含 Strength, Weak, Vulnerable)
  ├── 格挡公式 (含 Dexterity, Frail)
  ├── 基础卡牌效果 (Strike, Defend 类)
  ├── 回合流程 (玩家→敌方→循环)
  └── 怪物 AI 状态机框架

P1 — 核心扩展:
  ├── 完整的 Hook 事件系统
  ├── 所有 Ironclad 卡牌（从单角色开始）
  ├── Act 1 全部怪物 AI
  ├── 关键遗物 (Starter + Common)
  ├── 关键状态效果 (~20 个高频 Power)
  └── 卡牌关键词 (Exhaust, Ethereal, Innate, Retain)

P2 — 完整战斗:
  ├── 所有状态效果 (260 个)
  ├── 所有遗物触发
  ├── 药水系统
  ├── 附魔系统（STS2 新增）
  ├── 星星资源系统
  └── X 费牌

P3 — 地图与非战斗:
  ├── 地图生成算法
  ├── 事件决策树
  ├── 商店系统
  ├── 篝火（休息/升级/回忆）
  ├── 卡牌奖励选择
  └── Boss 宝箱遗物选择

P4 — 完整游戏:
  ├── 全部 5 个角色
  ├── 全部 4 幕
  ├── 高难度 (Ascension) 系统
  └── 解锁系统
```

### 10.2 从 C# 提取逻辑的工作流

```
1. 定位目标文件
   例如: extraction/decompiled/MegaCrit.Sts2.Core.Models.Monsters/Chomper.cs

2. 阅读 C# 源码，理解逻辑
   - 构造函数 → HP, 伤害值
   - GenerateMoveStateMachine() → AI 模式
   - PerformMove 委托 → 具体效果

3. 用 Python 重新实现
   - 无需 1:1 翻译，只需行为等价
   - 利用 spire-codex 的 JSON 数据获取静态数值
   - 自行实现动态逻辑（AI 决策、效果触发）

4. 单元测试验证
   - 对照真实游戏行为
   - 验证伤害公式、状态效果交互
```

### 10.3 与 spire-codex 数据的对接

```python
# spire-codex data/*.json 提供静态数据
cards_data = load_json("cards.json")        # 576 张卡牌的数值
monsters_data = load_json("monsters.json")  # 111 个怪物的 HP/伤害
powers_data = load_json("powers.json")      # 260 个状态效果的元信息
encounters_data = load_json("encounters.json")  # 87 个遭遇的怪物组合
characters_data = load_json("characters.json")  # 5 个角色的初始状态

# 需要从 C# 源码自行提取的动态逻辑
# → 卡牌的 OnPlay() 效果
# → 怪物的 GenerateMoveStateMachine() AI
# → 遗物的各种 Hook 重写
# → 状态效果的 Hook 重写
# → 地图生成的 StandardActMap 算法
```

### 10.4 关键陷阱与注意事项

1. **Hook 执行顺序**：多个 listener 的执行顺序可能影响最终结果（先加法后乘法是硬编码的，但同类修正的 listener 遍历顺序需要确认）

2. **Vulnerable/Weak/Frail 消退时机**：在 **敌方回合结束时** 消退，而非玩家回合结束。这意味着玩家施加的 1 层 Vulnerable 实际上覆盖了玩家的一整个回合

3. **第 1 回合格挡不清除**：玩家第 1 回合开始时不清除格挡（例如 Anchor 遗物在战斗开始时给 10 格挡，第 1 回合不会被清掉）

4. **Powered vs Unpowered 攻击**：力量/虚弱/易伤只对 "powered attack" 生效。一些特殊伤害源（如 Thorns 荆棘、Poison 毒）标记为 Unpowered，不受这些修正影响

5. **怪物 AI 的随机性**：RandomBranchState 的权重和重复约束需要精确复现，否则怪物行为模式会偏离真实游戏

6. **异步执行模型**：原版代码使用 C# async/await，模拟器可以简化为同步执行，但需要注意有些效果依赖执行顺序（如多段攻击中间触发的效果）
