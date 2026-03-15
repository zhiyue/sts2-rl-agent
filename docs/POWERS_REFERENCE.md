# Slay the Spire 2 - Powers Reference

> Auto-generated from decompiled source (`MegaCrit.Sts2.Core.Models.Powers`).
> 260 powers total.

---

## Table of Contents

1. [Damage Modifiers](#1-damage-modifiers)
2. [Block Modifiers](#2-block-modifiers)
3. [Turn-Start Effects](#3-turn-start-effects)
4. [Turn-End Effects](#4-turn-end-effects)
5. [Card-Play Triggered](#5-card-play-triggered)
6. [Damage Reaction](#6-damage-reaction)
7. [Card Draw/Exhaust Triggered](#7-card-drawexhaust-triggered)
8. [Block Persistence](#8-block-persistence)
9. [Energy/Cost Modifiers](#9-energycost-modifiers)
10. [Death Prevention](#10-death-prevention)
11. [Special/Unique Mechanics](#11-specialunique-mechanics)

---

## 1. Damage Modifiers

### AccuracyPower
- ID: ACCURACY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageAdditive]
- Logic:
  - ModifyDamageAdditive: Guards: if owner != dealer: return 0/1; if not poweredAttack: return 0/1; if no card: return 0/1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### CalcifyPower
- ID: CALCIFY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageAdditive]
- Logic:
  - ModifyDamageAdditive: Guards: if not poweredAttack: return 0/1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### ColossusPower
- ID: COLOSSUS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns base.DynamicVars["DamageDecrease"].BaseValue
  - AfterTurnEnd: Guards: if side == Enemy | Actions: tick down duration
- Tick: Decrements at end of enemy turn (AfterTurnEnd, side==Enemy)
- Internal State: DynamicVars: DynamicVar

### ConquerorPower
- ID: CONQUEROR
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns 2m
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: tick down duration
- Tick: Decrements at end of owner side turn
- Internal State: None

### CoveredPower
- ID: COVERED
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterApplied, AfterDeath, ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - AfterApplied: Actions: apply InterceptPower(1m) to base.Applier
  - AfterDeath: Actions: remove self
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1
  - AfterTurnEnd: Guards: if side == Enemy | Actions: remove self
- Tick: Removes self on condition
- Internal State: IsInstanced=true; DynamicVars: StringVar

### DiamondDiademPower
- ID: DIAMOND_DIADEM
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns 0.5m
  - AfterTurnEnd: Guards: if side == Enemy | Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### DoubleDamagePower
- ID: DOUBLE_DAMAGE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyDamageMultiplicative: Guards: if not poweredAttack: return 0/1; if no card: return 0/1 | Actions: returns 2m
  - AfterTurnEnd: Actions: tick down duration
- Tick: Decrements via TickDownDuration
- Internal State: None

### FlankingPower
- ID: FLANKING
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - AfterApplied: no-op (completed task)
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns base.Amount
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Removes self on condition
- Internal State: IsInstanced=true; DynamicVars: StringVar

### FlutterPower
- ID: FLUTTER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterDamageReceived]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns base.DynamicVars["DamageDecrease"].BaseValue / 100m
  - AfterDamageReceived: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: DynamicVars: DynamicVar

### GigantificationPower
- ID: GIGANTIFICATION
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeAttack, ModifyDamageMultiplicative, AfterAttack]
- Logic:
  - BeforeAttack: no-op (completed task)
  - ModifyDamageMultiplicative: Guards: if not poweredAttack: return 0/1; if no card: return 0/1
  - AfterAttack: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: Data{ commandToModify: AttackCommand? }

### GuardedPower
- ID: GUARDED
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterApplied, AfterDeath, ModifyDamageMultiplicative]
- Logic:
  - AfterApplied: no-op (completed task)
  - AfterDeath: Actions: remove self
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns 0.5m
- Tick: Removes self on condition
- Internal State: IsInstanced=true; DynamicVars: StringVar

### HangPower
- ID: HANG
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### InterceptPower
- ID: INTERCEPT
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns GetInternalData<Data>().coveredCreatures.Count + 1
  - AfterTurnEnd: Guards: if side == Enemy | Actions: remove self
- Tick: Removes self on condition
- Internal State: Data{ coveredCreatures: List<Creature> }; DynamicVars: StringVar

### KnockdownPower
- ID: KNOCKDOWN
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - AfterApplied: no-op (completed task)
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns base.Amount
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Removes self on condition
- Internal State: IsInstanced=true; DynamicVars: StringVar

### LeadershipPower
- ID: LEADERSHIP
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageAdditive]
- Logic:
  - ModifyDamageAdditive: Guards: if not poweredAttack: return 0/1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### LethalityPower
- ID: LETHALITY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative]
- Logic:
  - ModifyDamageMultiplicative: Guards: if not poweredAttack: return 0/1; if no card: return 0/1 | Actions: returns 1m + (decimal)base.Amount / 100m
- Tick: None
- Internal State: None

### PhantomBladesPower
- ID: PHANTOM_BLADES
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardEnteredCombat, AfterApplied, ModifyDamageAdditive]
- Logic:
  - AfterCardEnteredCombat: no-op (completed task)
  - AfterApplied: no-op (completed task)
  - ModifyDamageAdditive: Guards: if dealer != owner: return 0/1; if not poweredAttack: return 0/1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### ShrinkPower
- ID: SHRINK
- Type: Debuff
- Stack: Unknown
- AllowNegative: true
- Hooks: [AfterApplied, AfterRemoved, AfterTurnEnd, AfterDeath, ModifyDamageMultiplicative]
- Logic:
  - AfterApplied: no-op (completed task)
  - AfterRemoved: no-op (completed task)
  - AfterTurnEnd: Actions: decrement self
  - AfterDeath: Actions: remove self
  - ModifyDamageMultiplicative: Guards: if owner != dealer: return 0/1; if not poweredAttack: return 0/1 | Actions: returns (100m - base.DynamicVars["DamageDecrease"].BaseValue) / 100m
- Tick: Decrements via PowerCmd.Decrement on trigger; Removes self on condition
- Internal State: DynamicVars: DynamicVar, StringVar

### SlowPower
- ID: SLOW
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed, ModifyDamageMultiplicative, AfterModifyingDamageAmount, AfterSideTurnStart]
- Logic:
  - AfterCardPlayed: no-op (completed task)
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns 1m + 0.1m * base.DynamicVars["SlowAmount"].BaseValue
  - AfterModifyingDamageAmount: no-op (completed task)
  - AfterSideTurnStart: Guards: if side != owner.Side: skip
- Tick: None
- Internal State: DynamicVars: DynamicVar

### SoarPower
- ID: SOAR
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns base.DynamicVars["DamageDecrease"].BaseValue / 100m
- Tick: None
- Internal State: DynamicVars: DynamicVar

### StrengthPower
- ID: STRENGTH
- Type: Buff
- Stack: Counter
- AllowNegative: true
- Hooks: [ModifyDamageAdditive]
- Logic:
  - ModifyDamageAdditive: Guards: if owner != dealer: return 0/1; if not poweredAttack: return 0/1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### SurroundedPower
- ID: SURROUNDED
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, BeforeCardPlayed, BeforePotionUsed, AfterDeath]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1 | Actions: returns 1.5m
  - BeforeCardPlayed: see source for complex logic
  - BeforePotionUsed: see source for complex logic
  - AfterDeath: see source for complex logic
- Tick: None
- Internal State: Fields: _facing:Direction

### TankPower
- ID: TANK
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterApplied, ModifyDamageMultiplicative]
- Logic:
  - AfterApplied: Actions: apply GuardedPower(Amount) to item
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns 2m
- Tick: None
- Internal State: None

### TrackingPower
- ID: TRACKING
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative]
- Logic:
  - ModifyDamageMultiplicative: Guards: if not poweredAttack: return 0/1; if no card: return 0/1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### VigorPower
- ID: VIGOR
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeAttack, ModifyDamageAdditive, AfterAttack]
- Logic:
  - BeforeAttack: no-op (completed task)
  - ModifyDamageAdditive: Guards: if owner != dealer: return 0/1; if not poweredAttack: return 0/1 | Actions: returns base.Amount
  - AfterAttack: Actions: modify own amount by -internalData.amountWhenAttackStarted
- Tick: None
- Internal State: Data{ commandToModify: AttackCommand?, amountWhenAttackStarted: int }

### VulnerablePower
- ID: VULNERABLE
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns num
  - AfterTurnEnd: Guards: if side == Enemy | Actions: tick down duration
- Tick: Decrements at end of enemy turn (AfterTurnEnd, side==Enemy)
- Internal State: DynamicVars: DynamicVar

### WeakPower
- ID: WEAK
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyDamageMultiplicative: Guards: if dealer != owner: return 0/1; if not poweredAttack: return 0/1 | Actions: returns num
  - AfterTurnEnd: Guards: if side == Enemy | Actions: tick down duration
- Tick: Decrements at end of enemy turn (AfterTurnEnd, side==Enemy)
- Internal State: DynamicVars: DynamicVar

## 2. Block Modifiers

### DexterityPower
- ID: DEXTERITY
- Type: Buff
- Stack: Counter
- AllowNegative: true
- Hooks: [ModifyBlockAdditive]
- Logic:
  - ModifyBlockAdditive: Guards: if owner != target: return 1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### FastenPower
- ID: FASTEN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyBlockAdditive, AfterModifyingBlockAmount]
- Logic:
  - ModifyBlockAdditive: Guards: if owner != target: return 1 | Actions: returns base.Amount
  - AfterModifyingBlockAmount: no-op (completed task)
- Tick: None
- Internal State: None

### FrailPower
- ID: FRAIL
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyBlockMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyBlockMultiplicative: Guards: if owner != target: return 1 | Actions: returns 0.75m
  - AfterTurnEnd: Guards: if side == Enemy | Actions: tick down duration
- Tick: Decrements at end of enemy turn (AfterTurnEnd, side==Enemy)
- Internal State: None

### NoBlockPower
- ID: NO_BLOCK
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd, ModifyBlockMultiplicative]
- Logic:
  - AfterTurnEnd: Guards: if side == Enemy | Actions: decrement self
  - ModifyBlockMultiplicative: Guards: if target != owner: return 1; if no card: return 0/1
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### ShadowmeldPower
- ID: SHADOWMELD
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyBlockMultiplicative, AfterTurnEnd]
- Logic:
  - ModifyBlockMultiplicative: Guards: if owner != target: return 1 | Actions: returns (decimal)Math.Pow(2.0, base.Amount)
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### UnmovablePower
- ID: UNMOVABLE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyBlockMultiplicative]
- Logic:
  - ModifyBlockMultiplicative: Actions: returns 2m
- Tick: None
- Internal State: None

## 3. Turn-Start Effects

### AggressionPower
- ID: AGGRESSION
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeSideTurnStart]
- Logic:
  - BeforeSideTurnStart: Guards: if side != owner.Side: skip
- Tick: None
- Internal State: None

### BiasedCognitionPower
- ID: BIASED_COGNITION
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: apply FocusPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### BlurPower
- ID: BLUR
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ShouldClearBlock, AfterPreventingBlockClear, AfterSideTurnStart]
- Logic:
  - ShouldClearBlock: see source for complex logic
  - AfterPreventingBlockClear: no-op (completed task)
  - AfterSideTurnStart: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### CoolantPower
- ID: COOLANT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: gain num block on base.Owner
- Tick: None
- Internal State: None

### CountdownPower
- ID: COUNTDOWN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: apply DoomPower(Amount) to creature
- Tick: None
- Internal State: None

### CrimsonMantlePower
- ID: CRIMSON_MANTLE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: Guards: if player == owner.Player | Actions: deal damageVar.BaseValue damage to base.Owner; gain Amount block on base.Owner
- Tick: None
- Internal State: DynamicVars: DamageVar

### DemonFormPower
- ID: DEMON_FORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: apply StrengthPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### EntropyPower
- ID: ENTROPY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: Guards: if player != owner.Player: skip
- Tick: None
- Internal State: None

### FeralPower
- ID: FERAL
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, AfterModifyingCardPlayResultPileOrPosition, AfterSideTurnStart]
- Logic:
  - AfterApplied: no-op (completed task)
  - AfterModifyingCardPlayResultPileOrPosition: no-op (completed task)
  - AfterSideTurnStart: Guards: if side != owner.Side: skip
- Tick: None
- Internal State: Data{ zeroCostAttacksPlayed: int }

### FurnacePower
- ID: FURNACE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side == owner.Side
- Tick: None
- Internal State: None

### LoopPower
- ID: LOOP
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: see source for complex logic
- Tick: None
- Internal State: None

### NeurosurgePower
- ID: NEUROSURGE
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: apply DoomPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### NoxiousFumesPower
- ID: NOXIOUS_FUMES
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side != owner.Side: skip | Actions: apply PoisonPower(Amount) to base.CombatState.HittableEnemies
- Tick: None
- Internal State: None

### PlatingPower
- ID: PLATING
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, BeforeSideTurnStart, BeforeTurnEndEarly, AfterTurnEnd]
- Logic:
  - AfterApplied: no-op (completed task)
  - BeforeSideTurnStart: no-op (completed task)
  - BeforeTurnEndEarly: Guards: if side == owner.Side | Actions: gain Amount block on base.Owner
  - AfterTurnEnd: Guards: if side == Enemy | Actions: decrement self; modify own amount by -base.DynamicVars
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: DynamicVars: DynamicVar

### PoisonPower
- ID: POISON
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side != owner.Side: skip | Actions: deal Amount damage to base.Owner; decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### PrepTimePower
- ID: PREP_TIME
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: apply VigorPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### RampartPower
- ID: RAMPART
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Actions: gain Amount block on item
- Tick: None
- Internal State: None

### RollingBoulderPower
- ID: ROLLING_BOULDER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: Guards: if player != owner.Player: skip
- Tick: None
- Internal State: IsInstanced=true; DynamicVars: DamageVar

### SandpitPower
- ID: SANDPIT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, AfterSideTurnStart, AfterPowerAmountChanged, AfterRemoved, AfterCreatureAddedToCombat, AfterOstyRevived, BeforeTurnEnd]
- Logic:
  - AfterApplied: no-op (completed task)
  - AfterSideTurnStart: Guards: if side == Enemy | Actions: decrement self
  - AfterPowerAmountChanged: see source for complex logic
  - AfterRemoved: see source for complex logic
  - AfterCreatureAddedToCombat: see source for complex logic
  - AfterOstyRevived: see source for complex logic
  - BeforeTurnEnd: Guards: if side == Enemy
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: IsInstanced=true; Fields: _initialAmount:int, _initialTargetPosition:float

### ShadowStepPower
- ID: SHADOW_STEP
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Actions: apply DoubleDamagePower(Amount) to base.Owner; remove self
- Tick: Removes self on condition
- Internal State: None

### SummonNextTurnPower
- ID: SUMMON_NEXT_TURN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### WraithFormPower
- ID: WRAITH_FORM
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: apply DexterityPower(Amount) to base.Owner
- Tick: None
- Internal State: None

## 4. Turn-End Effects

### BattlewornDummyTimeLimitPower
- ID: BATTLEWORN_DUMMY_TIME_LIMIT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Guards: if side != owner.Side: skip | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### ConstrictPower
- ID: CONSTRICT
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd, AfterDeath]
- Logic:
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: deal Amount damage to base.Owner
  - AfterDeath: Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### ConsumingShadowPower
- ID: CONSUMING_SHADOW
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Actions: evoke orb
- Tick: None
- Internal State: None

### DebilitatePower
- ID: DEBILITATE
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### DemisePower
- ID: DEMISE
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: deal Amount damage to base.Owner
- Tick: None
- Internal State: None

### EscapeArtistPower
- ID: ESCAPE_ARTIST
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### GrapplePower
- ID: GRAPPLE
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterBlockGained, AfterTurnEnd]
- Logic:
  - AfterBlockGained: Actions: deal Amount damage to base.Owner
  - AfterTurnEnd: Actions: remove self
- Tick: Removes self on condition
- Internal State: IsInstanced=true

### HatchPower
- ID: HATCH
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Guards: if side == Enemy | Actions: tick down duration
- Tick: Decrements at end of enemy turn (AfterTurnEnd, side==Enemy)
- Internal State: None

### HighVoltagePower
- ID: HIGH_VOLTAGE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: apply StrengthPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### IntangiblePower
- ID: INTANGIBLE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHpLostAfterOsty, AfterModifyingHpLostAfterOsty, ModifyDamageCap, AfterModifyingDamageAmount, AfterTurnEnd]
- Logic:
  - ModifyHpLostAfterOsty: Guards: if target != owner: return 1 | Actions: returns Math.Min(GetDamageCap(dealer), amount)
  - AfterModifyingHpLostAfterOsty: no-op (completed task)
  - ModifyDamageCap: Guards: if target != owner: return 1 | Actions: returns GetDamageCap(dealer)
  - AfterModifyingDamageAmount: no-op (completed task)
  - AfterTurnEnd: Guards: if side == Enemy | Actions: tick down duration
- Tick: Decrements at end of enemy turn (AfterTurnEnd, side==Enemy)
- Internal State: None

### MagicBombPower
- ID: MAGIC_BOMB
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd, AfterDeath]
- Logic:
  - AfterTurnEnd: Actions: deal Amount damage to base.Owner; remove self
  - AfterDeath: Actions: remove self
- Tick: Removes self on condition
- Internal State: IsInstanced=true

### NemesisPower
- ID: NEMESIS
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Guards: if side != owner.Side: skip | Actions: apply IntangiblePower(1m) to base.Owner
- Tick: None
- Internal State: Fields: _shouldApplyIntangible:bool

### NoDrawPower
- ID: NO_DRAW
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [ShouldDraw, AfterTurnEnd]
- Logic:
  - ShouldDraw: Guards: if player != owner.Player: skip
  - AfterTurnEnd: Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### ReboundPower
- ID: REBOUND
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterModifyingCardPlayResultPileOrPosition, AfterTurnEnd]
- Logic:
  - AfterModifyingCardPlayResultPileOrPosition: Guards: if card belongs to owner | Actions: decrement self
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Decrements via PowerCmd.Decrement on trigger; Removes self on condition
- Internal State: None

### RegenPower
- ID: REGEN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Actions: decrement self; heal base.Amount
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### RetainHandPower
- ID: RETAIN_HAND
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ShouldFlush, AfterTurnEnd]
- Logic:
  - ShouldFlush: Guards: if player != owner.Player: skip
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### RitualPower
- ID: RITUAL
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, AfterTurnEnd]
- Logic:
  - AfterApplied: no-op (completed task)
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: apply StrengthPower(Amount) to base.Owner
- Tick: None
- Internal State: Fields: _wasJustAppliedByEnemy:bool

### SicEmPower
- ID: SIC_EM
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageGiven, AfterTurnEnd]
- Logic:
  - AfterDamageGiven: see source for complex logic
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### TemporaryDexterityPower
- ID: TEMPORARY_DEXTERITY
- Type: Unknown
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeApplied, AfterPowerAmountChanged, AfterTurnEnd]
- Logic:
  - BeforeApplied: Actions: apply DexterityPower((decimal)Sign * amount) to target
  - AfterPowerAmountChanged: Actions: apply DexterityPower((decimal)Sign * amount) to base.Owner
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: apply DexterityPower(Amount) to base.Owner; remove self
- Tick: Removes self on condition
- Internal State: Fields: _shouldIgnoreNextInstance:bool

### TemporaryFocusPower
- ID: TEMPORARY_FOCUS
- Type: Unknown
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeApplied, AfterPowerAmountChanged, AfterTurnEnd]
- Logic:
  - BeforeApplied: Actions: apply FocusPower((decimal)Sign * amount) to target
  - AfterPowerAmountChanged: Actions: apply FocusPower((decimal)Sign * amount) to base.Owner
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: apply FocusPower(Amount) to base.Owner; remove self
- Tick: Removes self on condition
- Internal State: Fields: _shouldIgnoreNextInstance:bool

### TemporaryStrengthPower
- ID: TEMPORARY_STRENGTH
- Type: Unknown
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeApplied, AfterPowerAmountChanged, AfterTurnEnd]
- Logic:
  - BeforeApplied: Actions: apply StrengthPower((decimal)Sign * amount) to target
  - AfterPowerAmountChanged: Actions: apply StrengthPower((decimal)Sign * amount) to base.Owner
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: apply StrengthPower(Amount) to base.Owner; remove self
- Tick: Removes self on condition
- Internal State: Fields: _shouldIgnoreNextInstance:bool

### TerritorialPower
- ID: TERRITORIAL
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: apply StrengthPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### WellLaidPlansPower
- ID: WELL_LAID_PLANS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeFlushLate]
- Logic:
  - BeforeFlushLate: see source for complex logic
- Tick: None
- Internal State: None

## 5. Card-Play Triggered

### AfterimagePower
- ID: AFTERIMAGE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: Actions: gain value block on base.Owner
- Tick: None
- Internal State: Data{ int: Dictionary<CardModel, }

### ArsenalPower
- ID: ARSENAL
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: Actions: apply StrengthPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### BlackHolePower
- ID: BLACK_HOLE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed, AfterStarsGained]
- Logic:
  - AfterCardPlayed: see source for complex logic
  - AfterStarsGained: see source for complex logic
- Tick: None
- Internal State: None

### BladeOfInkPower
- ID: BLADE_OF_INK
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed, AfterTurnEnd]
- Logic:
  - AfterCardPlayed: Actions: apply StrengthPower(Amount) to base.Owner
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: apply StrengthPower(-base.DynamicVars["StrengthApplied"].BaseValue) to base.Owner; remove self
- Tick: Removes self on condition
- Internal State: DynamicVars: DynamicVar

### BurstPower
- ID: BURST
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyCardPlayCount, AfterModifyingCardPlayCount, AfterTurnEnd]
- Logic:
  - ModifyCardPlayCount: Guards: if card not owned by owner: skip | Actions: returns playCount + 1
  - AfterModifyingCardPlayCount: Actions: decrement self
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Decrements via PowerCmd.Decrement on trigger; Removes self on condition
- Internal State: None

### CalamityPower
- ID: CALAMITY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: see source for complex logic
- Tick: None
- Internal State: Data{ int: Dictionary<CardModel, }

### ChainsOfBindingPower
- ID: CHAINS_OF_BINDING
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardDrawn, BeforeCardPlayed, ShouldPlay, BeforeTurnEnd]
- Logic:
  - AfterCardDrawn: see source for complex logic
  - BeforeCardPlayed: Guards: if card not owned by owner: skip
  - ShouldPlay: Guards: if card not owned by owner: skip | Actions: returns !GetInternalData<Data>().boundCardPlayed
  - BeforeTurnEnd: no-op (completed task)
- Tick: None
- Internal State: Data{ boundCardPlayed: bool }

### CurlUpPower
- ID: CURL_UP
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived, AfterCardPlayed]
- Logic:
  - AfterDamageReceived: Guards: if target != owner: return 1; if not poweredAttack: return 0/1; if no card: return 0/1
  - AfterCardPlayed: Actions: gain Amount block on base.Owner; remove self
- Tick: Removes self on condition
- Internal State: Data{ playedCard: CardModel? }

### DanseMacabrePower
- ID: DANSE_MACABRE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed]
- Logic:
  - BeforeCardPlayed: Actions: gain Amount block on base.Owner
- Tick: None
- Internal State: DynamicVars: EnergyVar

### DevourLifePower
- ID: DEVOUR_LIFE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: see source for complex logic
- Tick: None
- Internal State: None

### DuplicationPower
- ID: DUPLICATION
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyCardPlayCount, AfterModifyingCardPlayCount, AfterTurnEnd]
- Logic:
  - ModifyCardPlayCount: Guards: if card not owned by owner: skip | Actions: returns playCount + 1
  - AfterModifyingCardPlayCount: Actions: decrement self
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Decrements via PowerCmd.Decrement on trigger; Removes self on condition
- Internal State: None

### EchoFormPower
- ID: ECHO_FORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyCardPlayCount, AfterModifyingCardPlayCount]
- Logic:
  - ModifyCardPlayCount: Guards: if card not owned by owner: skip | Actions: returns playCount + 1
  - AfterModifyingCardPlayCount: no-op (completed task)
- Tick: None
- Internal State: None

### EnragePower
- ID: ENRAGE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: Actions: apply StrengthPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### FreeAttackPower
- ID: FREE_ATTACK
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat, BeforeCardPlayed]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
  - BeforeCardPlayed: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### FreePowerPower
- ID: FREE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat, BeforeCardPlayed]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
  - BeforeCardPlayed: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### FreeSkillPower
- ID: FREE_SKILL
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat, BeforeCardPlayed]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
  - BeforeCardPlayed: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### GalvanicPower
- ID: GALVANIC
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCombatStart, AfterCardEnteredCombat, AfterCardPlayed]
- Logic:
  - BeforeCombatStart: see source for complex logic
  - AfterCardEnteredCombat: see source for complex logic
  - AfterCardPlayed: Actions: deal Amount damage to cardPlay.Card.Owner.Creature
- Tick: None
- Internal State: DynamicVars: StringVar

### GravityPower
- ID: GRAVITY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed, AfterTurnEnd]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: Actions: deal value damage to base.Owner.CombatState.HittableEnemies
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Removes self on condition
- Internal State: Data{ int: Dictionary<CardModel, }

### HauntPower
- ID: HAUNT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: Actions: deal Amount damage to new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(item)
- Tick: None
- Internal State: None

### HellraiserPower
- ID: HELLRAISER
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterCardDrawnEarly, BeforeAttack]
- Logic:
  - AfterCardDrawnEarly: see source for complex logic
  - BeforeAttack: no-op (completed task)
- Tick: None
- Internal State: Fields: _autoplayingCards:HashSet<CardModel>?

### JugglingPower
- ID: JUGGLING
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, AfterCardPlayed, AfterTurnEnd]
- Logic:
  - AfterApplied: no-op (completed task)
  - AfterCardPlayed: see source for complex logic
  - AfterTurnEnd: Guards: if side == owner.Side
- Tick: None
- Internal State: Data{ attacksPlayedThisTurn: int }

### MasterPlannerPower
- ID: MASTER_PLANNER
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: no-op (completed task)
- Tick: None
- Internal State: None

### MonologuePower
- ID: MONOLOGUE
- Type: Buff
- Stack: Unknown
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed, AfterTurnEnd]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: Actions: apply StrengthPower(value) to base.Owner
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: apply StrengthPower(-base.DynamicVars["StrengthApplied"].BaseValue) to base.Owner; remove self
- Tick: Removes self on condition
- Internal State: Data{ int: Dictionary<CardModel, }; IsInstanced=true; DynamicVars: DynamicVar

### OblivionPower
- ID: OBLIVION
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed, AfterTurnEnd]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: Actions: apply DoomPower(value) to base.Owner
  - AfterTurnEnd: Actions: remove self
- Tick: Removes self on condition
- Internal State: Data{ int: Dictionary<CardModel, }

### OneTwoPunchPower
- ID: ONE_TWO_PUNCH
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyCardPlayCount, AfterModifyingCardPlayCount, AfterTurnEnd]
- Logic:
  - ModifyCardPlayCount: Guards: if card not owned by owner: skip | Actions: returns playCount + 1
  - AfterModifyingCardPlayCount: Actions: decrement self
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Decrements via PowerCmd.Decrement on trigger; Removes self on condition
- Internal State: None

### PainfulStabsPower
- ID: PAINFUL_STABS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ShouldPowerBeRemovedAfterOwnerDeath, ShouldCreatureBeRemovedFromCombatAfterDeath, AfterAttack]
- Logic:
  - ShouldPowerBeRemovedAfterOwnerDeath: see source for complex logic
  - ShouldCreatureBeRemovedFromCombatAfterDeath: Actions: returns creature != base.Owner
  - AfterAttack: see source for complex logic
- Tick: None
- Internal State: None

### PanachePower
- ID: PANACHE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed, AfterTurnEnd]
- Logic:
  - AfterCardPlayed: Actions: deal Amount damage to base.CombatState.HittableEnemies
  - AfterTurnEnd: Guards: if side != owner.Side: skip
- Tick: None
- Internal State: Data{ alreadyApplied: bool }; IsInstanced=true; DynamicVars: DynamicVar

### RagePower
- ID: RAGE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed, AfterTurnEnd]
- Logic:
  - AfterCardPlayed: Actions: gain Amount block on base.Owner
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### RupturePower
- ID: RUPTURE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterDamageReceived, AfterCardPlayed]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterDamageReceived: Actions: apply StrengthPower(Amount) to base.Owner
  - AfterCardPlayed: Actions: apply StrengthPower(value) to base.Owner
- Tick: None
- Internal State: Data{ int: Dictionary<CardModel, }

### SerpentFormPower
- ID: SERPENT_FORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: Actions: deal damage damage to creature
- Tick: None
- Internal State: Data{ int: Dictionary<CardModel, }

### SignalBoostPower
- ID: SIGNAL_BOOST
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyCardPlayCount, AfterModifyingCardPlayCount]
- Logic:
  - ModifyCardPlayCount: Guards: if card not owned by owner: skip | Actions: returns playCount + 1
  - AfterModifyingCardPlayCount: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### SkittishPower
- ID: SKITTISH
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterAttack, AfterTurnEnd]
- Logic:
  - AfterAttack: Actions: gain Amount block on base.Owner
  - AfterTurnEnd: Guards: if side != owner.Side: skip
- Tick: None
- Internal State: Data{ hasGainedBlockThisTurn: bool }

### SlothPower
- ID: SLOTH
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [ShouldPlay, BeforeCardPlayed, BeforeSideTurnStart]
- Logic:
  - ShouldPlay: Guards: if card not owned by owner: skip | Actions: returns _cardsPlayedThisTurn < base.Amount
  - BeforeCardPlayed: no-op (completed task)
  - BeforeSideTurnStart: Guards: if side != owner.Side: skip
- Tick: None
- Internal State: Fields: _cardsPlayedThisTurn:int

### SmoggyPower
- ID: SMOGGY
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterCardPlayed, AfterCardEnteredCombat, AfterTurnEnd, ShouldPlay]
- Logic:
  - AfterCardPlayed: see source for complex logic
  - AfterCardEnteredCombat: see source for complex logic
  - AfterTurnEnd: Guards: if side != owner.Side: skip
  - ShouldPlay: Actions: returns !(card.Affliction is Smog)
- Tick: None
- Internal State: None

### SneakyPower
- ID: SNEAKY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: Actions: gain Amount block on base.Owner
- Tick: None
- Internal State: None

### SpiritOfAshPower
- ID: SPIRIT_OF_ASH
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed]
- Logic:
  - BeforeCardPlayed: Actions: gain Amount block on base.Owner
- Tick: None
- Internal State: None

### StormPower
- ID: STORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: see source for complex logic
- Tick: None
- Internal State: Data{ int: Dictionary<CardModel, }

### StranglePower
- ID: STRANGLE
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed, AfterTurnEnd]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: Actions: deal value damage to base.Owner
  - AfterTurnEnd: Actions: remove self
- Tick: Removes self on condition
- Internal State: Data{ int: Dictionary<CardModel, }

### SubroutinePower
- ID: SUBROUTINE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterCardPlayed]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterCardPlayed: Actions: gain 1m energy
- Tick: None
- Internal State: Data{ int: Dictionary<CardModel, }

### SuckPower
- ID: SUCK
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterAttack]
- Logic:
  - AfterAttack: Actions: apply StrengthPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### TagTeamPower
- ID: TAG_TEAM
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, ModifyCardPlayCount, AfterModifyingCardPlayCount]
- Logic:
  - AfterApplied: no-op (completed task)
  - ModifyCardPlayCount: Guards: if target != owner: return 1 | Actions: returns playCount + base.Amount
  - AfterModifyingCardPlayCount: Actions: remove self
- Tick: Removes self on condition
- Internal State: IsInstanced=true; DynamicVars: StringVar

### TenderPower
- ID: TENDER
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardPlayed, AfterTurnEnd]
- Logic:
  - AfterCardPlayed: Actions: apply StrengthPower(-1m) to base.Owner; apply DexterityPower(-1m) to base.Owner
  - AfterTurnEnd: Actions: apply StrengthPower(CardsPlayedThisTurn) to base.Owner; apply DexterityPower(CardsPlayedThisTurn) to base.Owner
- Tick: None
- Internal State: Fields: _cardsPlayedThisTurn:int

### TheSealedThronePower
- ID: THE_SEALED_THRONE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed]
- Logic:
  - BeforeCardPlayed: see source for complex logic
- Tick: None
- Internal State: None

### VeilpiercerPower
- ID: VEILPIERCER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat, BeforeCardPlayed]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
  - BeforeCardPlayed: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### VoidFormPower
- ID: VOID_FORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforePowerAmountChanged, BeforeApplied, TryModifyEnergyCostInCombat, TryModifyStarCost, AfterCardPlayed, BeforeSideTurnStart]
- Logic:
  - BeforePowerAmountChanged: no-op (completed task)
  - BeforeApplied: no-op (completed task)
  - TryModifyEnergyCostInCombat: see source for complex logic
  - TryModifyStarCost: see source for complex logic
  - AfterCardPlayed: no-op (completed task)
  - BeforeSideTurnStart: Guards: if side == owner.Side
- Tick: None
- Internal State: Data{ cardsPlayedThisTurn: int }

## 6. Damage Reaction

### AsleepPower
- ID: ASLEEP
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived, BeforeTurnEndVeryEarly, AfterTurnEnd]
- Logic:
  - AfterDamageReceived: Actions: remove self
  - BeforeTurnEndVeryEarly: see source for complex logic
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger; Removes self on condition
- Internal State: None

### CurlUpPower
- ID: CURL_UP
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived, AfterCardPlayed]
- Logic:
  - AfterDamageReceived: Guards: if target != owner: return 1; if not poweredAttack: return 0/1; if no card: return 0/1
  - AfterCardPlayed: Actions: gain Amount block on base.Owner; remove self
- Tick: Removes self on condition
- Internal State: Data{ playedCard: CardModel? }

### FlameBarrierPower
- ID: FLAME_BARRIER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived, AfterTurnEnd]
- Logic:
  - AfterDamageReceived: Actions: deal Amount damage to dealer
  - AfterTurnEnd: Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### FlutterPower
- ID: FLUTTER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageMultiplicative, AfterDamageReceived]
- Logic:
  - ModifyDamageMultiplicative: Guards: if target != owner: return 1; if not poweredAttack: return 0/1 | Actions: returns base.DynamicVars["DamageDecrease"].BaseValue / 100m
  - AfterDamageReceived: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: DynamicVars: DynamicVar

### HardenedShellPower
- ID: HARDENED_SHELL
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHpLostBeforeOstyLate, AfterModifyingHpLostBeforeOsty, AfterDamageReceived, BeforeSideTurnStart]
- Logic:
  - ModifyHpLostBeforeOstyLate: Guards: if target != owner: return 1 | Actions: returns Math.Min(amount, (decimal)base.Amount - GetInternalData<Data>().damageReceivedThisTurn)
  - AfterModifyingHpLostBeforeOsty: no-op (completed task)
  - AfterDamageReceived: Guards: if target != owner: return 1
  - BeforeSideTurnStart: no-op (completed task)
- Tick: None
- Internal State: Data{ damageReceivedThisTurn: decimal }

### InfernoPower
- ID: INFERNO
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPlayerTurnStart, AfterDamageReceived]
- Logic:
  - AfterPlayerTurnStart: Guards: if player == owner.Player | Actions: deal damageVar.BaseValue damage to base.Owner
  - AfterDamageReceived: Actions: deal Amount damage to base.CombatState.HittableEnemies
- Tick: None
- Internal State: DynamicVars: DamageVar

### PersonalHivePower
- ID: PERSONAL_HIVE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived]
- Logic:
  - AfterDamageReceived: see source for complex logic
- Tick: None
- Internal State: None

### PlowPower
- ID: PLOW
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived]
- Logic:
  - AfterDamageReceived: Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### ReflectPower
- ID: REFLECT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived, AfterSideTurnStart]
- Logic:
  - AfterDamageReceived: Actions: deal result.BlockedDamage damage to dealer
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### RupturePower
- ID: RUPTURE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCardPlayed, AfterDamageReceived, AfterCardPlayed]
- Logic:
  - BeforeCardPlayed: no-op (completed task)
  - AfterDamageReceived: Actions: apply StrengthPower(Amount) to base.Owner
  - AfterCardPlayed: Actions: apply StrengthPower(value) to base.Owner
- Tick: None
- Internal State: Data{ int: Dictionary<CardModel, }

### ShriekPower
- ID: SHRIEK
- Type: Debuff
- Stack: Counter
- AllowNegative: true
- Hooks: [AfterDamageReceived]
- Logic:
  - AfterDamageReceived: Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### SlipperyPower
- ID: SLIPPERY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageCap, AfterDamageReceived]
- Logic:
  - ModifyDamageCap: Guards: if target != owner: return 1
  - AfterDamageReceived: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### SlumberPower
- ID: SLUMBER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived, AfterRemoved, AfterTurnEnd]
- Logic:
  - AfterDamageReceived: Actions: decrement self
  - AfterRemoved: no-op (completed task)
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### TheGambitPower
- ID: THE_GAMBIT
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterDamageReceived]
- Logic:
  - AfterDamageReceived: Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### ThornsPower
- ID: THORNS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeDamageReceived]
- Logic:
  - BeforeDamageReceived: Actions: deal Amount damage to dealer
- Tick: None
- Internal State: None

### VitalSparkPower
- ID: VITAL_SPARK
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageReceived, BeforeSideTurnStart]
- Logic:
  - AfterDamageReceived: Actions: gain base.DynamicVars.Energy.IntValue energy
  - BeforeSideTurnStart: Guards: if side != Enemy: skip
- Tick: None
- Internal State: Data{ playersTriggeredThisTurn: HashSet<Player> }; DynamicVars: EnergyVar

## 7. Card Draw/Exhaust Triggered

### AutomationPower
- ID: AUTOMATION
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardDrawn]
- Logic:
  - AfterCardDrawn: Actions: gain Amount energy
- Tick: None
- Internal State: Data{ cardsLeft: int }; IsInstanced=true; DynamicVars: DynamicVar

### ChainsOfBindingPower
- ID: CHAINS_OF_BINDING
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardDrawn, BeforeCardPlayed, ShouldPlay, BeforeTurnEnd]
- Logic:
  - AfterCardDrawn: see source for complex logic
  - BeforeCardPlayed: Guards: if card not owned by owner: skip
  - ShouldPlay: Guards: if card not owned by owner: skip | Actions: returns !GetInternalData<Data>().boundCardPlayed
  - BeforeTurnEnd: no-op (completed task)
- Tick: None
- Internal State: Data{ boundCardPlayed: bool }

### ConfusedPower
- ID: CONFUSED
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterCardDrawn]
- Logic:
  - AfterCardDrawn: no-op (completed task)
- Tick: None
- Internal State: Fields: _testEnergyCostOverride:int

### CorrosiveWavePower
- ID: CORROSIVE_WAVE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardDrawn, AfterTurnEnd]
- Logic:
  - AfterCardDrawn: Guards: if card not owned by owner: skip | Actions: apply PoisonPower(Amount) to base.CombatState.HittableEnemies
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### DarkEmbracePower
- ID: DARK_EMBRACE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardExhausted, AfterTurnEnd]
- Logic:
  - AfterCardExhausted: Guards: if card belongs to owner | Actions: draw Amount cards
  - AfterTurnEnd: Actions: draw Amount cards
- Tick: None
- Internal State: Data{ etherealCount: int }

### FeelNoPainPower
- ID: FEEL_NO_PAIN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardExhausted]
- Logic:
  - AfterCardExhausted: Guards: if card belongs to owner | Actions: gain Amount block on base.Owner
- Tick: None
- Internal State: None

### GalvanicPower
- ID: GALVANIC
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeCombatStart, AfterCardEnteredCombat, AfterCardPlayed]
- Logic:
  - BeforeCombatStart: see source for complex logic
  - AfterCardEnteredCombat: see source for complex logic
  - AfterCardPlayed: Actions: deal Amount damage to cardPlay.Card.Owner.Creature
- Tick: None
- Internal State: DynamicVars: StringVar

### HexPower
- ID: HEX
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterApplied, AfterCardEnteredCombat, AfterDeath, AfterRemoved]
- Logic:
  - AfterApplied: see source for complex logic
  - AfterCardEnteredCombat: see source for complex logic
  - AfterDeath: Actions: remove self
  - AfterRemoved: no-op (completed task)
- Tick: Removes self on condition
- Internal State: None

### IterationPower
- ID: ITERATION
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardDrawn]
- Logic:
  - AfterCardDrawn: Actions: draw Amount cards
- Tick: None
- Internal State: None

### PagestormPower
- ID: PAGESTORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardDrawn]
- Logic:
  - AfterCardDrawn: Actions: draw Amount cards
- Tick: None
- Internal State: None

### PhantomBladesPower
- ID: PHANTOM_BLADES
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardEnteredCombat, AfterApplied, ModifyDamageAdditive]
- Logic:
  - AfterCardEnteredCombat: no-op (completed task)
  - AfterApplied: no-op (completed task)
  - ModifyDamageAdditive: Guards: if dealer != owner: return 0/1; if not poweredAttack: return 0/1 | Actions: returns base.Amount
- Tick: None
- Internal State: None

### PillarOfCreationPower
- ID: PILLAR_OF_CREATION
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardGeneratedForCombat]
- Logic:
  - AfterCardGeneratedForCombat: Actions: gain Amount block on base.Owner
- Tick: None
- Internal State: None

### RingingPower
- ID: RINGING
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterApplied, AfterCardEnteredCombat, AfterTurnEnd, AfterRemoved, ShouldPlay]
- Logic:
  - AfterApplied: see source for complex logic
  - AfterCardEnteredCombat: see source for complex logic
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
  - AfterRemoved: no-op (completed task)
  - ShouldPlay: Guards: if card not owned by owner: skip | Actions: returns !CombatManager.Instance.History.CardPlaysStarted.Any((CardPlayStartedEntry e) => e.HappenedThisTurn(base.CombatState) && e.CardPlay.Card.Owner.Creature == base.Owner)
- Tick: Removes self on condition
- Internal State: None

### SmoggyPower
- ID: SMOGGY
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterCardPlayed, AfterCardEnteredCombat, AfterTurnEnd, ShouldPlay]
- Logic:
  - AfterCardPlayed: see source for complex logic
  - AfterCardEnteredCombat: see source for complex logic
  - AfterTurnEnd: Guards: if side != owner.Side: skip
  - ShouldPlay: Actions: returns !(card.Affliction is Smog)
- Tick: None
- Internal State: None

### SmokestackPower
- ID: SMOKESTACK
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardGeneratedForCombat]
- Logic:
  - AfterCardGeneratedForCombat: Actions: deal Amount damage to base.CombatState.HittableEnemies
- Tick: None
- Internal State: None

### SpeedsterPower
- ID: SPEEDSTER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardDrawn]
- Logic:
  - AfterCardDrawn: Actions: deal Amount damage to base.CombatState.HittableEnemies
- Tick: None
- Internal State: None

### SwordSagePower
- ID: SWORD_SAGE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPowerAmountChanged, AfterCardEnteredCombat, AfterRemoved, TryModifyEnergyCostInCombat]
- Logic:
  - AfterPowerAmountChanged: no-op (completed task)
  - AfterCardEnteredCombat: no-op (completed task)
  - AfterRemoved: no-op (completed task)
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
- Tick: None
- Internal State: None

### TangledPower
- ID: TANGLED
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, AfterCardEnteredCombat, AfterTurnEnd, AfterRemoved, TryModifyEnergyCostInCombat]
- Logic:
  - AfterApplied: see source for complex logic
  - AfterCardEnteredCombat: see source for complex logic
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
  - AfterRemoved: no-op (completed task)
  - TryModifyEnergyCostInCombat: see source for complex logic
- Tick: Removes self on condition
- Internal State: DynamicVars: EnergyVar

### TrashToTreasurePower
- ID: TRASH_TO_TREASURE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCardGeneratedForCombat]
- Logic:
  - AfterCardGeneratedForCombat: Actions: channel orb
- Tick: None
- Internal State: None

## 8. Block Persistence

### BlockNextTurnPower
- ID: BLOCK_NEXT_TURN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterBlockCleared]
- Logic:
  - AfterBlockCleared: Guards: if creature == owner | Actions: gain Amount block on base.Owner; remove self
- Tick: Removes self on condition
- Internal State: None

### SelfFormingClayPower
- ID: SELF_FORMING_CLAY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterBlockCleared]
- Logic:
  - AfterBlockCleared: Guards: if creature == owner | Actions: gain Amount block on base.Owner; remove self
- Tick: Removes self on condition
- Internal State: None

### ToricToughnessPower
- ID: TORIC_TOUGHNESS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterBlockCleared]
- Logic:
  - AfterBlockCleared: Guards: if creature == owner | Actions: gain base.DynamicVars.Block block on base.Owner; decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: IsInstanced=true; DynamicVars: BlockVar

## 9. Energy/Cost Modifiers

### ClarityPower
- ID: CLARITY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHandDraw, AfterSideTurnStart]
- Logic:
  - ModifyHandDraw: Guards: if player != owner.Player: skip | Actions: returns count + 1m
  - AfterSideTurnStart: Guards: if side == owner.Side | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### CorruptionPower
- ID: CORRUPTION
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat]
- Logic:
  - TryModifyEnergyCostInCombat: see source for complex logic
- Tick: None
- Internal State: None

### CuriousPower
- ID: CURIOUS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
- Tick: None
- Internal State: None

### DemesnePower
- ID: DEMESNE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHandDraw, ModifyMaxEnergy]
- Logic:
  - ModifyHandDraw: Guards: if player != owner.Player: skip | Actions: returns count + (decimal)base.Amount
  - ModifyMaxEnergy: Guards: if player != owner.Player: skip | Actions: returns amount + (decimal)base.Amount
- Tick: None
- Internal State: None

### DrawCardsNextTurnPower
- ID: DRAW_CARDS_NEXT_TURN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHandDraw, AfterSideTurnStart]
- Logic:
  - ModifyHandDraw: Guards: if player != owner.Player: skip | Actions: returns count + (decimal)base.Amount
  - AfterSideTurnStart: Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### FreeAttackPower
- ID: FREE_ATTACK
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat, BeforeCardPlayed]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
  - BeforeCardPlayed: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### FreePowerPower
- ID: FREE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat, BeforeCardPlayed]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
  - BeforeCardPlayed: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### FreeSkillPower
- ID: FREE_SKILL
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat, BeforeCardPlayed]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
  - BeforeCardPlayed: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### FriendshipPower
- ID: FRIENDSHIP
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyMaxEnergy]
- Logic:
  - ModifyMaxEnergy: Guards: if player != owner.Player: skip | Actions: returns amount + (decimal)base.Amount
- Tick: None
- Internal State: None

### MachineLearningPower
- ID: MACHINE_LEARNING
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHandDraw]
- Logic:
  - ModifyHandDraw: Guards: if player != owner.Player: skip | Actions: returns count + (decimal)base.Amount
- Tick: None
- Internal State: None

### MindRotPower
- ID: MIND_ROT
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHandDraw, AfterModifyingHandDraw]
- Logic:
  - ModifyHandDraw: Guards: if player != owner.Player: skip | Actions: returns Math.Max(0m, count - (decimal)base.Amount)
  - AfterModifyingHandDraw: no-op (completed task)
- Tick: None
- Internal State: None

### OrbitPower
- ID: ORBIT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterEnergySpent]
- Logic:
  - AfterEnergySpent: Actions: gain Amount energy
- Tick: None
- Internal State: Data{ energySpent: int, triggerCount: int }; IsInstanced=true; DynamicVars: EnergyVar

### PaleBlueDotPower
- ID: PALE_BLUE_DOT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHandDraw, AfterModifyingHandDraw]
- Logic:
  - ModifyHandDraw: Guards: if player != owner.Player: skip | Actions: returns count + (decimal)base.Amount
  - AfterModifyingHandDraw: no-op (completed task)
- Tick: None
- Internal State: DynamicVars: DynamicVar

### PyrePower
- ID: PYRE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyMaxEnergy]
- Logic:
  - ModifyMaxEnergy: Guards: if player != owner.Player: skip | Actions: returns amount + (decimal)base.Amount
- Tick: None
- Internal State: None

### SwordSagePower
- ID: SWORD_SAGE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPowerAmountChanged, AfterCardEnteredCombat, AfterRemoved, TryModifyEnergyCostInCombat]
- Logic:
  - AfterPowerAmountChanged: no-op (completed task)
  - AfterCardEnteredCombat: no-op (completed task)
  - AfterRemoved: no-op (completed task)
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
- Tick: None
- Internal State: None

### TangledPower
- ID: TANGLED
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterApplied, AfterCardEnteredCombat, AfterTurnEnd, AfterRemoved, TryModifyEnergyCostInCombat]
- Logic:
  - AfterApplied: see source for complex logic
  - AfterCardEnteredCombat: see source for complex logic
  - AfterTurnEnd: Guards: if side == owner.Side | Actions: remove self
  - AfterRemoved: no-op (completed task)
  - TryModifyEnergyCostInCombat: see source for complex logic
- Tick: Removes self on condition
- Internal State: DynamicVars: EnergyVar

### ToolsOfTheTradePower
- ID: TOOLS_OF_THE_TRADE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHandDraw, AfterPlayerTurnStart]
- Logic:
  - ModifyHandDraw: Guards: if player != owner.Player: skip | Actions: returns count + (decimal)base.Amount
  - AfterPlayerTurnStart: Guards: if player == owner.Player | Actions: discard cards
- Tick: None
- Internal State: None

### TyrannyPower
- ID: TYRANNY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHandDraw, AfterPlayerTurnStart]
- Logic:
  - ModifyHandDraw: Guards: if player != owner.Player: skip | Actions: returns count + (decimal)base.Amount
  - AfterPlayerTurnStart: Guards: if player != owner.Player: skip | Actions: exhaust cards
- Tick: None
- Internal State: None

### VeilpiercerPower
- ID: VEILPIERCER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyEnergyCostInCombat, BeforeCardPlayed]
- Logic:
  - TryModifyEnergyCostInCombat: Guards: if card not owned by owner: skip
  - BeforeCardPlayed: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### VoidFormPower
- ID: VOID_FORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforePowerAmountChanged, BeforeApplied, TryModifyEnergyCostInCombat, TryModifyStarCost, AfterCardPlayed, BeforeSideTurnStart]
- Logic:
  - BeforePowerAmountChanged: no-op (completed task)
  - BeforeApplied: no-op (completed task)
  - TryModifyEnergyCostInCombat: see source for complex logic
  - TryModifyStarCost: see source for complex logic
  - AfterCardPlayed: no-op (completed task)
  - BeforeSideTurnStart: Guards: if side == owner.Side
- Tick: None
- Internal State: Data{ cardsPlayedThisTurn: int }

### WasteAwayPower
- ID: WASTE_AWAY
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyMaxEnergy]
- Logic:
  - ModifyMaxEnergy: Guards: if player != owner.Player: skip | Actions: returns amount - (decimal)base.Amount
- Tick: None
- Internal State: None

## 10. Death Prevention

### AdaptablePower
- ID: ADAPTABLE
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterDeath, ShouldAllowHitting, ShouldStopCombatFromEnding, ShouldCreatureBeRemovedFromCombatAfterDeath, ShouldPowerBeRemovedAfterOwnerDeath]
- Logic:
  - AfterDeath: see source for complex logic
  - ShouldAllowHitting: Guards: if creature != owner: skip | Actions: returns !IsReviving
  - ShouldStopCombatFromEnding: see source for complex logic
  - ShouldCreatureBeRemovedFromCombatAfterDeath: Guards: if creature != owner: skip
  - ShouldPowerBeRemovedAfterOwnerDeath: see source for complex logic
- Tick: None
- Internal State: Data{ isReviving: bool }

### CrabRagePower
- ID: CRAB_RAGE
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterDeath]
- Logic:
  - AfterDeath: Actions: gain base.DynamicVars.Block block on base.Owner; apply StrengthPower(base.DynamicVars.Strength.IntValue) to base.Owner; remove self
- Tick: Removes self on condition
- Internal State: DynamicVars: BlockVar

### DampenPower
- ID: DAMPEN
- Type: Debuff
- Stack: None
- AllowNegative: false
- Hooks: [AfterApplied, AfterDeath, AfterRemoved]
- Logic:
  - AfterApplied: no-op (completed task)
  - AfterDeath: Actions: remove self
  - AfterRemoved: no-op (completed task)
- Tick: Removes self on condition
- Internal State: Data{ casters: HashSet<Creature>, int: Dictionary<CardModel, }

### DieForYouPower
- ID: DIE_FOR_YOU
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [ModifyUnblockedDamageTarget, ShouldAllowHitting, ShouldCreatureBeRemovedFromCombatAfterDeath, ShouldPowerBeRemovedAfterOwnerDeath]
- Logic:
  - ModifyUnblockedDamageTarget: Guards: if not poweredAttack: return 0/1 | Actions: returns base.Owner
  - ShouldAllowHitting: Actions: returns creature.IsAlive
  - ShouldCreatureBeRemovedFromCombatAfterDeath: Guards: if creature != owner: skip
  - ShouldPowerBeRemovedAfterOwnerDeath: see source for complex logic
- Tick: None
- Internal State: None

### DoorRevivalPower
- ID: DOOR_REVIVAL
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [BeforeDeath, AfterDeath, ShouldAllowHitting, ShouldStopCombatFromEnding, ShouldCreatureBeRemovedFromCombatAfterDeath, ShouldPowerBeRemovedAfterOwnerDeath]
- Logic:
  - BeforeDeath: Guards: if creature != owner: skip
  - AfterDeath: Guards: if creature == owner
  - ShouldAllowHitting: Guards: if creature != owner: skip | Actions: returns !IsHalfDead
  - ShouldStopCombatFromEnding: Actions: returns door.Doormaker.IsAlive
  - ShouldCreatureBeRemovedFromCombatAfterDeath: Guards: if creature == owner
  - ShouldPowerBeRemovedAfterOwnerDeath: see source for complex logic
- Tick: None
- Internal State: Data{ isHalfDead: bool }

### IllusionPower
- ID: ILLUSION
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [ShouldPowerBeRemovedOnDeath, AfterApplied, AfterDeath, ShouldAllowHitting, ShouldCreatureBeRemovedFromCombatAfterDeath, AfterCombatEnd]
- Logic:
  - ShouldPowerBeRemovedOnDeath: Actions: returns power.Type == PowerType.Debuff
  - AfterApplied: Actions: apply MinionPower(1m) to base.Owner
  - AfterDeath: see source for complex logic
  - ShouldAllowHitting: Guards: if creature != owner: skip
  - ShouldCreatureBeRemovedFromCombatAfterDeath: Guards: if creature != owner: skip
  - AfterCombatEnd: see source for complex logic
- Tick: None
- Internal State: Data{ isReviving: bool }; Fields: _followUpStateId:string?

### InfestedPower
- ID: INFESTED
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterDeath, ShouldStopCombatFromEnding]
- Logic:
  - AfterDeath: see source for complex logic
  - ShouldStopCombatFromEnding: see source for complex logic
- Tick: None
- Internal State: None

### MinionPower
- ID: MINION
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [ShouldPowerBeRemovedAfterOwnerDeath, ShouldOwnerDeathTriggerFatal]
- Logic:
  - ShouldPowerBeRemovedAfterOwnerDeath: see source for complex logic
  - ShouldOwnerDeathTriggerFatal: see source for complex logic
- Tick: None
- Internal State: None

### PossessSpeedPower
- ID: POSSESS_SPEED
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterPowerAmountChanged, AfterDeath]
- Logic:
  - AfterPowerAmountChanged: no-op (completed task)
  - AfterDeath: Actions: apply DexterityPower(-item.Value) to item.Key
- Tick: None
- Internal State: Data{ decimal: Dictionary<Creature, }

### PossessStrengthPower
- ID: POSSESS_STRENGTH
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterPowerAmountChanged, AfterDeath]
- Logic:
  - AfterPowerAmountChanged: no-op (completed task)
  - AfterDeath: Actions: apply StrengthPower(-item.Value) to item.Key
- Tick: None
- Internal State: Data{ decimal: Dictionary<Creature, }

### RavenousPower
- ID: RAVENOUS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDeath]
- Logic:
  - AfterDeath: Actions: apply StrengthPower(Amount) to base.Owner
- Tick: None
- Internal State: None

### ReattachPower
- ID: REATTACH
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterDeath, ShouldAllowHitting, ShouldCreatureBeRemovedFromCombatAfterDeath, ShouldPowerBeRemovedAfterOwnerDeath, ShouldOwnerDeathTriggerFatal]
- Logic:
  - AfterDeath: see source for complex logic
  - ShouldAllowHitting: Guards: if creature != owner: skip
  - ShouldCreatureBeRemovedFromCombatAfterDeath: Guards: if creature != owner: skip
  - ShouldPowerBeRemovedAfterOwnerDeath: see source for complex logic
  - ShouldOwnerDeathTriggerFatal: Actions: returns AreAllOtherSegmentsDead()
- Tick: None
- Internal State: Data{ isReviving: bool }

### SteamEruptionPower
- ID: STEAM_ERUPTION
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDeath, ShouldStopCombatFromEnding, ShouldCreatureBeRemovedFromCombatAfterDeath, ShouldPowerBeRemovedAfterOwnerDeath]
- Logic:
  - AfterDeath: see source for complex logic
  - ShouldStopCombatFromEnding: see source for complex logic
  - ShouldCreatureBeRemovedFromCombatAfterDeath: Guards: if creature != owner: skip
  - ShouldPowerBeRemovedAfterOwnerDeath: see source for complex logic
- Tick: None
- Internal State: None

### StockPower
- ID: STOCK
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDeath, ShouldStopCombatFromEnding]
- Logic:
  - AfterDeath: see source for complex logic
  - ShouldStopCombatFromEnding: see source for complex logic
- Tick: None
- Internal State: None

### SurprisePower
- ID: SURPRISE
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterDeath, ShouldStopCombatFromEnding]
- Logic:
  - AfterDeath: see source for complex logic
  - ShouldStopCombatFromEnding: see source for complex logic
- Tick: None
- Internal State: None

## 11. Special/Unique Mechanics

### AccelerantPower
- ID: ACCELERANT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### AnticipatePower
- ID: ANTICIPATE
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### ArtifactPower
- ID: ARTIFACT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [TryModifyPowerAmountReceived, AfterModifyingPowerAmountReceived]
- Logic:
  - TryModifyPowerAmountReceived: Guards: if target != owner: return 1
  - AfterModifyingPowerAmountReceived: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### BackAttackLeftPower
- ID: BACK_ATTACK_LEFT
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### BackAttackRightPower
- ID: BACK_ATTACK_RIGHT
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### BarricadePower
- ID: BARRICADE
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterApplied, ShouldClearBlock]
- Logic:
  - AfterApplied: no-op (completed task)
  - ShouldClearBlock: see source for complex logic
- Tick: None
- Internal State: DynamicVars: StringVar

### BeaconOfHopePower
- ID: BEACON_OF_HOPE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterBlockGained]
- Logic:
  - AfterBlockGained: Actions: gain amountToGive block on item
- Tick: None
- Internal State: None

### BufferPower
- ID: BUFFER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyHpLostAfterOstyLate, AfterModifyingHpLostAfterOsty]
- Logic:
  - ModifyHpLostAfterOstyLate: Guards: if target != owner: return 1
  - AfterModifyingHpLostAfterOsty: Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### BurrowedPower
- ID: BURROWED
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [ShouldClearBlock, AfterBlockBroken, AfterRemoved]
- Logic:
  - ShouldClearBlock: see source for complex logic
  - AfterBlockBroken: Guards: if creature == owner
  - AfterRemoved: see source for complex logic
- Tick: None
- Internal State: None

### CallOfTheVoidPower
- ID: CALL_OF_THE_VOID
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: Guards: if player != owner.Player: skip
- Tick: None
- Internal State: None

### ChildOfTheStarsPower
- ID: CHILD_OF_THE_STARS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterStarsSpent]
- Logic:
  - AfterStarsSpent: Actions: gain Amount block on base.Owner
- Tick: None
- Internal State: None

### CoordinatePower
- ID: COORDINATE
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### CreativeAiPower
- ID: CREATIVE_AI
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: Guards: if player != owner.Player: skip
- Tick: None
- Internal State: None

### CrueltyPower
- ID: CRUELTY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### CrushUnderPower
- ID: CRUSH_UNDER
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### DarkShacklesPower
- ID: DARK_SHACKLES
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### DisintegrationPower
- ID: DISINTEGRATION
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterTurnEndLate]
- Logic:
  - AfterTurnEndLate: Guards: if side == owner.Side | Actions: deal Amount damage to base.Owner
- Tick: None
- Internal State: None

### DoomPower
- ID: DOOM
- Type: Debuff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeTurnEnd]
- Logic:
  - BeforeTurnEnd: see source for complex logic
- Tick: None
- Internal State: None

### DrumOfBattlePower
- ID: DRUM_OF_BATTLE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDrawLate]
- Logic:
  - BeforeHandDrawLate: Actions: exhaust cards
- Tick: None
- Internal State: None

### DyingStarPower
- ID: DYING_STAR
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### EnergyNextTurnPower
- ID: ENERGY_NEXT_TURN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterEnergyReset]
- Logic:
  - AfterEnergyReset: Guards: if player == owner.Player | Actions: remove self; gain Amount energy
- Tick: Removes self on condition
- Internal State: None

### EnfeeblingTouchPower
- ID: ENFEEBLING_TOUCH
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### EnvenomPower
- ID: ENVENOM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageGiven]
- Logic:
  - AfterDamageGiven: Actions: apply PoisonPower(Amount) to target
- Tick: None
- Internal State: None

### FanOfKnivesPower
- ID: FAN_OF_KNIVES
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### FeedingFrenzyPower
- ID: FEEDING_FRENZY
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### FlexPotionPower
- ID: FLEX_POTION
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### FocusPower
- ID: FOCUS
- Type: Buff
- Stack: Counter
- AllowNegative: true
- Hooks: [ModifyOrbValue]
- Logic:
  - ModifyOrbValue: Actions: returns Math.Max(value + (decimal)base.Amount, 0m)
- Tick: None
- Internal State: None

### FocusedStrikePower
- ID: FOCUSED_STRIKE
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### ForbiddenGrimoirePower
- ID: FORBIDDEN_GRIMOIRE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCombatEnd]
- Logic:
  - AfterCombatEnd: no-op (completed task)
- Tick: None
- Internal State: None

### ForegoneConclusionPower
- ID: FOREGONE_CONCLUSION
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: Guards: if player == owner.Player | Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### GenesisPower
- ID: GENESIS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterEnergyReset]
- Logic:
  - AfterEnergyReset: Guards: if player == owner.Player
- Tick: None
- Internal State: None

### HailstormPower
- ID: HAILSTORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeTurnEnd]
- Logic:
  - BeforeTurnEnd: Guards: if side == owner.Side | Actions: deal Amount damage to base.CombatState.HittableEnemies
- Tick: None
- Internal State: DynamicVars: DynamicVar

### HammerTimePower
- ID: HAMMER_TIME
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterForge]
- Logic:
  - AfterForge: see source for complex logic
- Tick: None
- Internal State: None

### HardToKillPower
- ID: HARD_TO_KILL
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [ModifyDamageCap, AfterModifyingDamageAmount]
- Logic:
  - ModifyDamageCap: Guards: if target != owner: return 1 | Actions: returns base.Amount
  - AfterModifyingDamageAmount: no-op (completed task)
- Tick: None
- Internal State: None

### HeistPower
- ID: HEIST
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeDeath]
- Logic:
  - BeforeDeath: Guards: if owner != target: return 1
- Tick: None
- Internal State: IsInstanced=true

### HelicalDartPower
- ID: HELICAL_DART
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### HelloWorldPower
- ID: HELLO_WORLD
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: see source for complex logic
- Tick: None
- Internal State: None

### HotfixPower
- ID: HOTFIX
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### ImbalancedPower
- ID: IMBALANCED
- Type: Debuff
- Stack: Single
- AllowNegative: false
- Hooks: [AfterDamageGiven]
- Logic:
  - AfterDamageGiven: see source for complex logic
- Tick: None
- Internal State: None

### ImprovementPower
- ID: IMPROVEMENT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCombatEnd]
- Logic:
  - AfterCombatEnd: no-op (completed task)
- Tick: None
- Internal State: None

### InfiniteBladesPower
- ID: INFINITE_BLADES
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: Guards: if player == owner.Player
- Tick: None
- Internal State: None

### JuggernautPower
- ID: JUGGERNAUT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterBlockGained]
- Logic:
  - AfterBlockGained: Actions: deal Amount damage to target
- Tick: None
- Internal State: None

### LightningRodPower
- ID: LIGHTNING_ROD
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterEnergyReset]
- Logic:
  - AfterEnergyReset: Guards: if player == owner.Player | Actions: decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: None

### ManglePower
- ID: MANGLE
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### MayhemPower
- ID: MAYHEM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDrawLate]
- Logic:
  - BeforeHandDrawLate: Guards: if player == owner.Player
- Tick: None
- Internal State: None

### MonarchsGazePower
- ID: MONARCHS_GAZE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageGiven]
- Logic:
  - AfterDamageGiven: Actions: apply MonarchsGazeStrengthDownPower(Amount) to target
- Tick: None
- Internal State: None

### MonarchsGazeStrengthDownPower
- ID: MONARCHS_GAZE_STRENGTH_DOWN
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### NecroMasteryPower
- ID: NECRO_MASTERY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCurrentHpChanged]
- Logic:
  - AfterCurrentHpChanged: see source for complex logic
- Tick: None
- Internal State: None

### NightmarePower
- ID: NIGHTMARE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: Guards: if player == owner.Player | Actions: remove self
- Tick: Removes self on condition
- Internal State: Data{ selectedCard: CardModel? }; IsInstanced=true; DynamicVars: StringVar

### NostalgiaPower
- ID: NOSTALGIA
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterModifyingCardPlayResultPileOrPosition]
- Logic:
  - AfterModifyingCardPlayResultPileOrPosition: Guards: if card not owned by owner: skip
- Tick: None
- Internal State: None

### OutbreakPower
- ID: OUTBREAK
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPowerAmountChanged]
- Logic:
  - AfterPowerAmountChanged: Actions: deal Amount damage to base.Owner.CombatState.HittableEnemies
- Tick: None
- Internal State: Data{ timesPoisoned: int }; DynamicVars: RepeatVar

### PaperCutsPower
- ID: PAPER_CUTS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageGiven]
- Logic:
  - AfterDamageGiven: see source for complex logic
- Tick: None
- Internal State: None

### ParryPower
- ID: PARRY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### PiercingWailPower
- ID: PIERCING_WAIL
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### RadiancePower
- ID: RADIANCE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterEnergyReset]
- Logic:
  - AfterEnergyReset: Guards: if player == owner.Player | Actions: decrement self; gain base.DynamicVars.Energy.IntValue energy
- Tick: Decrements via PowerCmd.Decrement on trigger
- Internal State: DynamicVars: EnergyVar

### ReaperFormPower
- ID: REAPER_FORM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterDamageGiven]
- Logic:
  - AfterDamageGiven: Actions: apply DoomPower(Amount) to target
- Tick: None
- Internal State: None

### ReptileTrinketPower
- ID: REPTILE_TRINKET
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### RoyaltiesPower
- ID: ROYALTIES
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterCombatEnd]
- Logic:
  - AfterCombatEnd: no-op (completed task)
- Tick: None
- Internal State: None

### SeekingEdgePower
- ID: SEEKING_EDGE
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### SentryModePower
- ID: SENTRY_MODE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: Guards: if player == owner.Player
- Tick: None
- Internal State: None

### SetupStrikePower
- ID: SETUP_STRIKE
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### ShacklingPotionPower
- ID: SHACKLING_POTION
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### ShroudPower
- ID: SHROUD
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPowerAmountChanged]
- Logic:
  - AfterPowerAmountChanged: Actions: gain Amount block on base.Owner
- Tick: None
- Internal State: None

### SleightOfFleshPower
- ID: SLEIGHT_OF_FLESH
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPowerAmountChanged]
- Logic:
  - AfterPowerAmountChanged: Actions: deal Amount damage to power.Owner
- Tick: None
- Internal State: None

### SpectrumShiftPower
- ID: SPECTRUM_SHIFT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: Guards: if player == owner.Player
- Tick: None
- Internal State: None

### SpeedPotionPower
- ID: SPEED_POTION
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### SpinnerPower
- ID: SPINNER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterEnergyReset]
- Logic:
  - AfterEnergyReset: Guards: if player == owner.Player
- Tick: None
- Internal State: None

### StampedePower
- ID: STAMPEDE
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeTurnEnd]
- Logic:
  - BeforeTurnEnd: Guards: if side != owner.Side: skip
- Tick: None
- Internal State: None

### StarNextTurnPower
- ID: STAR_NEXT_TURN
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterEnergyReset]
- Logic:
  - AfterEnergyReset: Guards: if player == owner.Player | Actions: remove self
- Tick: Removes self on condition
- Internal State: None

### StratagemPower
- ID: STRATAGEM
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterShuffle]
- Logic:
  - AfterShuffle: Guards: if player != owner.Player: skip
- Tick: None
- Internal State: None

### SwipePower
- ID: SWIPE
- Type: Buff
- Stack: Single
- AllowNegative: false
- Hooks: [BeforeDeath]
- Logic:
  - BeforeDeath: Guards: if owner != target: return 1
- Tick: None
- Internal State: IsInstanced=true; Fields: _stolenCard:CardModel?

### SynchronizePower
- ID: SYNCHRONIZE
- Type: Unknown
- Stack: Unknown
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### TheBombPower
- ID: THE_BOMB
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [BeforeTurnEnd]
- Logic:
  - BeforeTurnEnd: Guards: if side != owner.Side: skip | Actions: deal base.DynamicVars.Damage damage to base.CombatState.HittableEnemies; remove self; decrement self
- Tick: Decrements via PowerCmd.Decrement on trigger; Removes self on condition
- Internal State: IsInstanced=true; DynamicVars: DamageVar

### TheHuntPower
- ID: THE_HUNT
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: None

### ThieveryPower
- ID: THIEVERY
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [None]
- Logic:
  - No hook overrides (passive/marker power)
- Tick: None
- Internal State: IsInstanced=true; DynamicVars: GoldVar

### ThunderPower
- ID: THUNDER
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterOrbEvoked]
- Logic:
  - AfterOrbEvoked: Actions: deal Amount damage to livingTargets
- Tick: None
- Internal State: None

### ViciousPower
- ID: VICIOUS
- Type: Buff
- Stack: Counter
- AllowNegative: false
- Hooks: [AfterPowerAmountChanged]
- Logic:
  - AfterPowerAmountChanged: Actions: draw Amount cards
- Tick: None
- Internal State: None

---

## Appendix: All Powers Alphabetical Index

| Power | ID | Type | Stack | AllowNeg | Hooks |
|-------|-----|------|-------|----------|-------|
| AccelerantPower | ACCELERANT | Buff | Counter | false | (none) |
| AccuracyPower | ACCURACY | Buff | Counter | false | ModifyDamageAdditive |
| AdaptablePower | ADAPTABLE | Buff | Single | false | AfterDeath, ShouldAllowHitting, ShouldStopCombatFromEnding +2 more |
| AfterimagePower | AFTERIMAGE | Buff | Counter | false | BeforeCardPlayed, AfterCardPlayed |
| AggressionPower | AGGRESSION | Buff | Counter | false | BeforeSideTurnStart |
| AnticipatePower | ANTICIPATE | Unknown | Unknown | false | (none) |
| ArsenalPower | ARSENAL | Buff | Counter | false | AfterCardPlayed |
| ArtifactPower | ARTIFACT | Buff | Counter | false | TryModifyPowerAmountReceived, AfterModifyingPowerAmountReceived |
| AsleepPower | ASLEEP | Buff | Counter | false | AfterDamageReceived, BeforeTurnEndVeryEarly, AfterTurnEnd |
| AutomationPower | AUTOMATION | Buff | Counter | false | AfterCardDrawn |
| BackAttackLeftPower | BACK_ATTACK_LEFT | Buff | Single | false | (none) |
| BackAttackRightPower | BACK_ATTACK_RIGHT | Buff | Single | false | (none) |
| BarricadePower | BARRICADE | Buff | Single | false | AfterApplied, ShouldClearBlock |
| BattlewornDummyTimeLimitPower | BATTLEWORN_DUMMY_TIME_LIMIT | Buff | Counter | false | AfterTurnEnd |
| BeaconOfHopePower | BEACON_OF_HOPE | Buff | Counter | false | AfterBlockGained |
| BiasedCognitionPower | BIASED_COGNITION | Debuff | Counter | false | AfterSideTurnStart |
| BlackHolePower | BLACK_HOLE | Buff | Counter | false | AfterCardPlayed, AfterStarsGained |
| BladeOfInkPower | BLADE_OF_INK | Buff | Counter | false | AfterCardPlayed, AfterTurnEnd |
| BlockNextTurnPower | BLOCK_NEXT_TURN | Buff | Counter | false | AfterBlockCleared |
| BlurPower | BLUR | Buff | Counter | false | ShouldClearBlock, AfterPreventingBlockClear, AfterSideTurnStart |
| BufferPower | BUFFER | Buff | Counter | false | ModifyHpLostAfterOstyLate, AfterModifyingHpLostAfterOsty |
| BurrowedPower | BURROWED | Buff | Single | false | ShouldClearBlock, AfterBlockBroken, AfterRemoved |
| BurstPower | BURST | Buff | Counter | false | ModifyCardPlayCount, AfterModifyingCardPlayCount, AfterTurnEnd |
| CalamityPower | CALAMITY | Buff | Counter | false | BeforeCardPlayed, AfterCardPlayed |
| CalcifyPower | CALCIFY | Buff | Counter | false | ModifyDamageAdditive |
| CallOfTheVoidPower | CALL_OF_THE_VOID | Buff | Counter | false | BeforeHandDraw |
| ChainsOfBindingPower | CHAINS_OF_BINDING | Debuff | Counter | false | AfterCardDrawn, BeforeCardPlayed, ShouldPlay +1 more |
| ChildOfTheStarsPower | CHILD_OF_THE_STARS | Buff | Counter | false | AfterStarsSpent |
| ClarityPower | CLARITY | Buff | Counter | false | ModifyHandDraw, AfterSideTurnStart |
| ColossusPower | COLOSSUS | Buff | Counter | false | ModifyDamageMultiplicative, AfterTurnEnd |
| ConfusedPower | CONFUSED | Debuff | Single | false | AfterCardDrawn |
| ConquerorPower | CONQUEROR | Debuff | Counter | false | ModifyDamageMultiplicative, AfterTurnEnd |
| ConstrictPower | CONSTRICT | Debuff | Counter | false | AfterTurnEnd, AfterDeath |
| ConsumingShadowPower | CONSUMING_SHADOW | Buff | Counter | false | AfterTurnEnd |
| CoolantPower | COOLANT | Buff | Counter | false | AfterSideTurnStart |
| CoordinatePower | COORDINATE | Unknown | Unknown | false | (none) |
| CorrosiveWavePower | CORROSIVE_WAVE | Buff | Counter | false | AfterCardDrawn, AfterTurnEnd |
| CorruptionPower | CORRUPTION | Buff | Single | false | TryModifyEnergyCostInCombat |
| CountdownPower | COUNTDOWN | Buff | Counter | false | AfterSideTurnStart |
| CoveredPower | COVERED | Buff | Single | false | AfterApplied, AfterDeath, ModifyDamageMultiplicative +1 more |
| CrabRagePower | CRAB_RAGE | Buff | Single | false | AfterDeath |
| CreativeAiPower | CREATIVE_AI | Buff | Counter | false | BeforeHandDraw |
| CrimsonMantlePower | CRIMSON_MANTLE | Buff | Counter | false | AfterPlayerTurnStart |
| CrueltyPower | CRUELTY | Buff | Counter | false | (none) |
| CrushUnderPower | CRUSH_UNDER | Unknown | Unknown | false | (none) |
| CuriousPower | CURIOUS | Buff | Counter | false | TryModifyEnergyCostInCombat |
| CurlUpPower | CURL_UP | Buff | Counter | false | AfterDamageReceived, AfterCardPlayed |
| DampenPower | DAMPEN | Debuff | None | false | AfterApplied, AfterDeath, AfterRemoved |
| DanseMacabrePower | DANSE_MACABRE | Buff | Counter | false | BeforeCardPlayed |
| DarkEmbracePower | DARK_EMBRACE | Buff | Counter | false | AfterCardExhausted, AfterTurnEnd |
| DarkShacklesPower | DARK_SHACKLES | Unknown | Unknown | false | (none) |
| DebilitatePower | DEBILITATE | Debuff | Counter | false | AfterTurnEnd |
| DemesnePower | DEMESNE | Buff | Counter | false | ModifyHandDraw, ModifyMaxEnergy |
| DemisePower | DEMISE | Debuff | Counter | false | AfterTurnEnd |
| DemonFormPower | DEMON_FORM | Buff | Counter | false | AfterSideTurnStart |
| DevourLifePower | DEVOUR_LIFE | Buff | Counter | false | AfterCardPlayed |
| DexterityPower | DEXTERITY | Buff | Counter | true | ModifyBlockAdditive |
| DiamondDiademPower | DIAMOND_DIADEM | Buff | Single | false | ModifyDamageMultiplicative, AfterTurnEnd |
| DieForYouPower | DIE_FOR_YOU | Buff | Single | false | ModifyUnblockedDamageTarget, ShouldAllowHitting, ShouldCreatureBeRemovedFromCombatAfterDeath +1 more |
| DisintegrationPower | DISINTEGRATION | Debuff | Counter | false | AfterTurnEndLate |
| DoomPower | DOOM | Debuff | Counter | false | BeforeTurnEnd |
| DoorRevivalPower | DOOR_REVIVAL | Buff | Single | false | BeforeDeath, AfterDeath, ShouldAllowHitting +3 more |
| DoubleDamagePower | DOUBLE_DAMAGE | Buff | Counter | false | ModifyDamageMultiplicative, AfterTurnEnd |
| DrawCardsNextTurnPower | DRAW_CARDS_NEXT_TURN | Buff | Counter | false | ModifyHandDraw, AfterSideTurnStart |
| DrumOfBattlePower | DRUM_OF_BATTLE | Buff | Counter | false | BeforeHandDrawLate |
| DuplicationPower | DUPLICATION | Buff | Counter | false | ModifyCardPlayCount, AfterModifyingCardPlayCount, AfterTurnEnd |
| DyingStarPower | DYING_STAR | Unknown | Unknown | false | (none) |
| EchoFormPower | ECHO_FORM | Buff | Counter | false | ModifyCardPlayCount, AfterModifyingCardPlayCount |
| EnergyNextTurnPower | ENERGY_NEXT_TURN | Buff | Counter | false | AfterEnergyReset |
| EnfeeblingTouchPower | ENFEEBLING_TOUCH | Unknown | Unknown | false | (none) |
| EnragePower | ENRAGE | Buff | Counter | false | AfterCardPlayed |
| EntropyPower | ENTROPY | Buff | Counter | false | AfterPlayerTurnStart |
| EnvenomPower | ENVENOM | Buff | Counter | false | AfterDamageGiven |
| EscapeArtistPower | ESCAPE_ARTIST | Buff | Counter | false | AfterTurnEnd |
| FanOfKnivesPower | FAN_OF_KNIVES | Buff | Single | false | (none) |
| FastenPower | FASTEN | Buff | Counter | false | ModifyBlockAdditive, AfterModifyingBlockAmount |
| FeedingFrenzyPower | FEEDING_FRENZY | Unknown | Unknown | false | (none) |
| FeelNoPainPower | FEEL_NO_PAIN | Buff | Counter | false | AfterCardExhausted |
| FeralPower | FERAL | Buff | Counter | false | AfterApplied, AfterModifyingCardPlayResultPileOrPosition, AfterSideTurnStart |
| FlameBarrierPower | FLAME_BARRIER | Buff | Counter | false | AfterDamageReceived, AfterTurnEnd |
| FlankingPower | FLANKING | Debuff | Counter | false | AfterApplied, ModifyDamageMultiplicative, AfterTurnEnd |
| FlexPotionPower | FLEX_POTION | Unknown | Unknown | false | (none) |
| FlutterPower | FLUTTER | Buff | Counter | false | ModifyDamageMultiplicative, AfterDamageReceived |
| FocusPower | FOCUS | Buff | Counter | true | ModifyOrbValue |
| FocusedStrikePower | FOCUSED_STRIKE | Unknown | Unknown | false | (none) |
| ForbiddenGrimoirePower | FORBIDDEN_GRIMOIRE | Buff | Counter | false | AfterCombatEnd |
| ForegoneConclusionPower | FOREGONE_CONCLUSION | Buff | Counter | false | BeforeHandDraw |
| FrailPower | FRAIL | Debuff | Counter | false | ModifyBlockMultiplicative, AfterTurnEnd |
| FreeAttackPower | FREE_ATTACK | Buff | Counter | false | TryModifyEnergyCostInCombat, BeforeCardPlayed |
| FreePowerPower | FREE | Buff | Counter | false | TryModifyEnergyCostInCombat, BeforeCardPlayed |
| FreeSkillPower | FREE_SKILL | Buff | Counter | false | TryModifyEnergyCostInCombat, BeforeCardPlayed |
| FriendshipPower | FRIENDSHIP | Buff | Counter | false | ModifyMaxEnergy |
| FurnacePower | FURNACE | Buff | Counter | false | AfterSideTurnStart |
| GalvanicPower | GALVANIC | Buff | Counter | false | BeforeCombatStart, AfterCardEnteredCombat, AfterCardPlayed |
| GenesisPower | GENESIS | Buff | Counter | false | AfterEnergyReset |
| GigantificationPower | GIGANTIFICATION | Buff | Counter | false | BeforeAttack, ModifyDamageMultiplicative, AfterAttack |
| GrapplePower | GRAPPLE | Debuff | Counter | false | AfterBlockGained, AfterTurnEnd |
| GravityPower | GRAVITY | Buff | Counter | false | BeforeCardPlayed, AfterCardPlayed, AfterTurnEnd |
| GuardedPower | GUARDED | Buff | Single | false | AfterApplied, AfterDeath, ModifyDamageMultiplicative |
| HailstormPower | HAILSTORM | Buff | Counter | false | BeforeTurnEnd |
| HammerTimePower | HAMMER_TIME | Buff | Single | false | AfterForge |
| HangPower | HANG | Debuff | Counter | false | ModifyDamageMultiplicative |
| HardToKillPower | HARD_TO_KILL | Buff | Counter | false | ModifyDamageCap, AfterModifyingDamageAmount |
| HardenedShellPower | HARDENED_SHELL | Buff | Counter | false | ModifyHpLostBeforeOstyLate, AfterModifyingHpLostBeforeOsty, AfterDamageReceived +1 more |
| HatchPower | HATCH | Buff | Counter | false | AfterTurnEnd |
| HauntPower | HAUNT | Buff | Counter | false | AfterCardPlayed |
| HeistPower | HEIST | Buff | Counter | false | BeforeDeath |
| HelicalDartPower | HELICAL_DART | Unknown | Unknown | false | (none) |
| HelloWorldPower | HELLO_WORLD | Buff | Counter | false | BeforeHandDraw |
| HellraiserPower | HELLRAISER | Buff | Single | false | AfterCardDrawnEarly, BeforeAttack |
| HexPower | HEX | Debuff | Single | false | AfterApplied, AfterCardEnteredCombat, AfterDeath +1 more |
| HighVoltagePower | HIGH_VOLTAGE | Buff | Counter | false | AfterTurnEnd |
| HotfixPower | HOTFIX | Unknown | Unknown | false | (none) |
| IllusionPower | ILLUSION | Buff | Single | false | ShouldPowerBeRemovedOnDeath, AfterApplied, AfterDeath +3 more |
| ImbalancedPower | IMBALANCED | Debuff | Single | false | AfterDamageGiven |
| ImprovementPower | IMPROVEMENT | Buff | Counter | false | AfterCombatEnd |
| InfernoPower | INFERNO | Buff | Counter | false | AfterPlayerTurnStart, AfterDamageReceived |
| InfestedPower | INFESTED | Buff | Single | false | AfterDeath, ShouldStopCombatFromEnding |
| InfiniteBladesPower | INFINITE_BLADES | Buff | Counter | false | BeforeHandDraw |
| IntangiblePower | INTANGIBLE | Buff | Counter | false | ModifyHpLostAfterOsty, AfterModifyingHpLostAfterOsty, ModifyDamageCap +2 more |
| InterceptPower | INTERCEPT | Buff | Single | false | ModifyDamageMultiplicative, AfterTurnEnd |
| IterationPower | ITERATION | Buff | Counter | false | AfterCardDrawn |
| JuggernautPower | JUGGERNAUT | Buff | Counter | false | AfterBlockGained |
| JugglingPower | JUGGLING | Buff | Counter | false | AfterApplied, AfterCardPlayed, AfterTurnEnd |
| KnockdownPower | KNOCKDOWN | Debuff | Counter | false | AfterApplied, ModifyDamageMultiplicative, AfterTurnEnd |
| LeadershipPower | LEADERSHIP | Buff | Counter | false | ModifyDamageAdditive |
| LethalityPower | LETHALITY | Buff | Counter | false | ModifyDamageMultiplicative |
| LightningRodPower | LIGHTNING_ROD | Buff | Counter | false | AfterEnergyReset |
| LoopPower | LOOP | Buff | Counter | false | AfterPlayerTurnStart |
| MachineLearningPower | MACHINE_LEARNING | Buff | Counter | false | ModifyHandDraw |
| MagicBombPower | MAGIC_BOMB | Debuff | Counter | false | AfterTurnEnd, AfterDeath |
| ManglePower | MANGLE | Unknown | Unknown | false | (none) |
| MasterPlannerPower | MASTER_PLANNER | Buff | Single | false | AfterCardPlayed |
| MayhemPower | MAYHEM | Buff | Counter | false | BeforeHandDrawLate |
| MindRotPower | MIND_ROT | Debuff | Counter | false | ModifyHandDraw, AfterModifyingHandDraw |
| MinionPower | MINION | Buff | Single | false | ShouldPowerBeRemovedAfterOwnerDeath, ShouldOwnerDeathTriggerFatal |
| MonarchsGazePower | MONARCHS_GAZE | Buff | Counter | false | AfterDamageGiven |
| MonarchsGazeStrengthDownPower | MONARCHS_GAZE_STRENGTH_DOWN | Unknown | Unknown | false | (none) |
| MonologuePower | MONOLOGUE | Buff | Unknown | false | BeforeCardPlayed, AfterCardPlayed, AfterTurnEnd |
| NecroMasteryPower | NECRO_MASTERY | Buff | Counter | false | AfterCurrentHpChanged |
| NemesisPower | NEMESIS | Buff | Single | false | AfterTurnEnd |
| NeurosurgePower | NEUROSURGE | Debuff | Counter | false | AfterSideTurnStart |
| NightmarePower | NIGHTMARE | Buff | Counter | false | BeforeHandDraw |
| NoBlockPower | NO_BLOCK | Debuff | Counter | false | AfterTurnEnd, ModifyBlockMultiplicative |
| NoDrawPower | NO_DRAW | Debuff | Single | false | ShouldDraw, AfterTurnEnd |
| NostalgiaPower | NOSTALGIA | Buff | Counter | false | AfterModifyingCardPlayResultPileOrPosition |
| NoxiousFumesPower | NOXIOUS_FUMES | Buff | Counter | false | AfterSideTurnStart |
| OblivionPower | OBLIVION | Debuff | Counter | false | BeforeCardPlayed, AfterCardPlayed, AfterTurnEnd |
| OneTwoPunchPower | ONE_TWO_PUNCH | Buff | Counter | false | ModifyCardPlayCount, AfterModifyingCardPlayCount, AfterTurnEnd |
| OrbitPower | ORBIT | Buff | Counter | false | AfterEnergySpent |
| OutbreakPower | OUTBREAK | Buff | Counter | false | AfterPowerAmountChanged |
| PagestormPower | PAGESTORM | Buff | Counter | false | AfterCardDrawn |
| PainfulStabsPower | PAINFUL_STABS | Buff | Counter | false | ShouldPowerBeRemovedAfterOwnerDeath, ShouldCreatureBeRemovedFromCombatAfterDeath, AfterAttack |
| PaleBlueDotPower | PALE_BLUE_DOT | Buff | Counter | false | ModifyHandDraw, AfterModifyingHandDraw |
| PanachePower | PANACHE | Buff | Counter | false | AfterCardPlayed, AfterTurnEnd |
| PaperCutsPower | PAPER_CUTS | Buff | Counter | false | AfterDamageGiven |
| ParryPower | PARRY | Buff | Counter | false | (none) |
| PersonalHivePower | PERSONAL_HIVE | Buff | Counter | false | AfterDamageReceived |
| PhantomBladesPower | PHANTOM_BLADES | Buff | Counter | false | AfterCardEnteredCombat, AfterApplied, ModifyDamageAdditive |
| PiercingWailPower | PIERCING_WAIL | Unknown | Unknown | false | (none) |
| PillarOfCreationPower | PILLAR_OF_CREATION | Buff | Counter | false | AfterCardGeneratedForCombat |
| PlatingPower | PLATING | Buff | Counter | false | AfterApplied, BeforeSideTurnStart, BeforeTurnEndEarly +1 more |
| PlowPower | PLOW | Debuff | Counter | false | AfterDamageReceived |
| PoisonPower | POISON | Debuff | Counter | false | AfterSideTurnStart |
| PossessSpeedPower | POSSESS_SPEED | Buff | Single | false | AfterPowerAmountChanged, AfterDeath |
| PossessStrengthPower | POSSESS_STRENGTH | Buff | Single | false | AfterPowerAmountChanged, AfterDeath |
| PrepTimePower | PREP_TIME | Buff | Counter | false | AfterSideTurnStart |
| PyrePower | PYRE | Buff | Counter | false | ModifyMaxEnergy |
| RadiancePower | RADIANCE | Buff | Counter | false | AfterEnergyReset |
| RagePower | RAGE | Buff | Counter | false | AfterCardPlayed, AfterTurnEnd |
| RampartPower | RAMPART | Buff | Counter | false | AfterSideTurnStart |
| RavenousPower | RAVENOUS | Buff | Counter | false | AfterDeath |
| ReaperFormPower | REAPER_FORM | Buff | Counter | false | AfterDamageGiven |
| ReattachPower | REATTACH | Buff | Single | false | AfterDeath, ShouldAllowHitting, ShouldCreatureBeRemovedFromCombatAfterDeath +2 more |
| ReboundPower | REBOUND | Buff | Counter | false | AfterModifyingCardPlayResultPileOrPosition, AfterTurnEnd |
| ReflectPower | REFLECT | Buff | Counter | false | AfterDamageReceived, AfterSideTurnStart |
| RegenPower | REGEN | Buff | Counter | false | AfterTurnEnd |
| ReptileTrinketPower | REPTILE_TRINKET | Unknown | Unknown | false | (none) |
| RetainHandPower | RETAIN_HAND | Buff | Counter | false | ShouldFlush, AfterTurnEnd |
| RingingPower | RINGING | Debuff | Single | false | AfterApplied, AfterCardEnteredCombat, AfterTurnEnd +2 more |
| RitualPower | RITUAL | Buff | Counter | false | AfterApplied, AfterTurnEnd |
| RollingBoulderPower | ROLLING_BOULDER | Buff | Counter | false | AfterPlayerTurnStart |
| RoyaltiesPower | ROYALTIES | Buff | Counter | false | AfterCombatEnd |
| RupturePower | RUPTURE | Buff | Counter | false | BeforeCardPlayed, AfterDamageReceived, AfterCardPlayed |
| SandpitPower | SANDPIT | Buff | Counter | false | AfterApplied, AfterSideTurnStart, AfterPowerAmountChanged +4 more |
| SeekingEdgePower | SEEKING_EDGE | Buff | Single | false | (none) |
| SelfFormingClayPower | SELF_FORMING_CLAY | Buff | Counter | false | AfterBlockCleared |
| SentryModePower | SENTRY_MODE | Buff | Counter | false | BeforeHandDraw |
| SerpentFormPower | SERPENT_FORM | Buff | Counter | false | BeforeCardPlayed, AfterCardPlayed |
| SetupStrikePower | SETUP_STRIKE | Unknown | Unknown | false | (none) |
| ShacklingPotionPower | SHACKLING_POTION | Unknown | Unknown | false | (none) |
| ShadowStepPower | SHADOW_STEP | Buff | Counter | false | AfterSideTurnStart |
| ShadowmeldPower | SHADOWMELD | Buff | Counter | false | ModifyBlockMultiplicative, AfterTurnEnd |
| ShriekPower | SHRIEK | Debuff | Counter | true | AfterDamageReceived |
| ShrinkPower | SHRINK | Debuff | Unknown | true | AfterApplied, AfterRemoved, AfterTurnEnd +2 more |
| ShroudPower | SHROUD | Buff | Counter | false | AfterPowerAmountChanged |
| SicEmPower | SIC_EM | Debuff | Counter | false | AfterDamageGiven, AfterTurnEnd |
| SignalBoostPower | SIGNAL_BOOST | Buff | Counter | false | ModifyCardPlayCount, AfterModifyingCardPlayCount |
| SkittishPower | SKITTISH | Buff | Counter | false | AfterAttack, AfterTurnEnd |
| SleightOfFleshPower | SLEIGHT_OF_FLESH | Buff | Counter | false | AfterPowerAmountChanged |
| SlipperyPower | SLIPPERY | Buff | Counter | false | ModifyDamageCap, AfterDamageReceived |
| SlothPower | SLOTH | Debuff | Counter | false | ShouldPlay, BeforeCardPlayed, BeforeSideTurnStart |
| SlowPower | SLOW | Debuff | Counter | false | AfterCardPlayed, ModifyDamageMultiplicative, AfterModifyingDamageAmount +1 more |
| SlumberPower | SLUMBER | Buff | Counter | false | AfterDamageReceived, AfterRemoved, AfterTurnEnd |
| SmoggyPower | SMOGGY | Debuff | Single | false | AfterCardPlayed, AfterCardEnteredCombat, AfterTurnEnd +1 more |
| SmokestackPower | SMOKESTACK | Buff | Counter | false | AfterCardGeneratedForCombat |
| SneakyPower | SNEAKY | Buff | Counter | false | AfterCardPlayed |
| SoarPower | SOAR | Buff | Single | false | ModifyDamageMultiplicative |
| SpectrumShiftPower | SPECTRUM_SHIFT | Buff | Counter | false | BeforeHandDraw |
| SpeedPotionPower | SPEED_POTION | Unknown | Unknown | false | (none) |
| SpeedsterPower | SPEEDSTER | Buff | Counter | false | AfterCardDrawn |
| SpinnerPower | SPINNER | Buff | Counter | false | AfterEnergyReset |
| SpiritOfAshPower | SPIRIT_OF_ASH | Buff | Counter | false | BeforeCardPlayed |
| StampedePower | STAMPEDE | Buff | Counter | false | BeforeTurnEnd |
| StarNextTurnPower | STAR_NEXT_TURN | Buff | Counter | false | AfterEnergyReset |
| SteamEruptionPower | STEAM_ERUPTION | Buff | Counter | false | AfterDeath, ShouldStopCombatFromEnding, ShouldCreatureBeRemovedFromCombatAfterDeath +1 more |
| StockPower | STOCK | Buff | Counter | false | AfterDeath, ShouldStopCombatFromEnding |
| StormPower | STORM | Buff | Counter | false | BeforeCardPlayed, AfterCardPlayed |
| StranglePower | STRANGLE | Debuff | Counter | false | BeforeCardPlayed, AfterCardPlayed, AfterTurnEnd |
| StratagemPower | STRATAGEM | Buff | Counter | false | AfterShuffle |
| StrengthPower | STRENGTH | Buff | Counter | true | ModifyDamageAdditive |
| SubroutinePower | SUBROUTINE | Buff | Counter | false | BeforeCardPlayed, AfterCardPlayed |
| SuckPower | SUCK | Buff | Counter | false | AfterAttack |
| SummonNextTurnPower | SUMMON_NEXT_TURN | Buff | Counter | false | AfterPlayerTurnStart |
| SurprisePower | SURPRISE | Buff | Single | false | AfterDeath, ShouldStopCombatFromEnding |
| SurroundedPower | SURROUNDED | Debuff | Single | false | ModifyDamageMultiplicative, BeforeCardPlayed, BeforePotionUsed +1 more |
| SwipePower | SWIPE | Buff | Single | false | BeforeDeath |
| SwordSagePower | SWORD_SAGE | Buff | Counter | false | AfterPowerAmountChanged, AfterCardEnteredCombat, AfterRemoved +1 more |
| SynchronizePower | SYNCHRONIZE | Unknown | Unknown | false | (none) |
| TagTeamPower | TAG_TEAM | Debuff | Counter | false | AfterApplied, ModifyCardPlayCount, AfterModifyingCardPlayCount |
| TangledPower | TANGLED | Debuff | Counter | false | AfterApplied, AfterCardEnteredCombat, AfterTurnEnd +2 more |
| TankPower | TANK | Buff | Single | false | AfterApplied, ModifyDamageMultiplicative |
| TemporaryDexterityPower | TEMPORARY_DEXTERITY | Unknown | Counter | false | BeforeApplied, AfterPowerAmountChanged, AfterTurnEnd |
| TemporaryFocusPower | TEMPORARY_FOCUS | Unknown | Counter | false | BeforeApplied, AfterPowerAmountChanged, AfterTurnEnd |
| TemporaryStrengthPower | TEMPORARY_STRENGTH | Unknown | Counter | false | BeforeApplied, AfterPowerAmountChanged, AfterTurnEnd |
| TenderPower | TENDER | Debuff | Counter | false | AfterCardPlayed, AfterTurnEnd |
| TerritorialPower | TERRITORIAL | Buff | Counter | false | AfterTurnEnd |
| TheBombPower | THE_BOMB | Buff | Counter | false | BeforeTurnEnd |
| TheGambitPower | THE_GAMBIT | Debuff | Single | false | AfterDamageReceived |
| TheHuntPower | THE_HUNT | Buff | Counter | false | (none) |
| TheSealedThronePower | THE_SEALED_THRONE | Buff | Counter | false | BeforeCardPlayed |
| ThieveryPower | THIEVERY | Buff | Counter | false | (none) |
| ThornsPower | THORNS | Buff | Counter | false | BeforeDamageReceived |
| ThunderPower | THUNDER | Buff | Counter | false | AfterOrbEvoked |
| ToolsOfTheTradePower | TOOLS_OF_THE_TRADE | Buff | Counter | false | ModifyHandDraw, AfterPlayerTurnStart |
| ToricToughnessPower | TORIC_TOUGHNESS | Buff | Counter | false | AfterBlockCleared |
| TrackingPower | TRACKING | Buff | Counter | false | ModifyDamageMultiplicative |
| TrashToTreasurePower | TRASH_TO_TREASURE | Buff | Counter | false | AfterCardGeneratedForCombat |
| TyrannyPower | TYRANNY | Buff | Counter | false | ModifyHandDraw, AfterPlayerTurnStart |
| UnmovablePower | UNMOVABLE | Buff | Counter | false | ModifyBlockMultiplicative |
| VeilpiercerPower | VEILPIERCER | Buff | Counter | false | TryModifyEnergyCostInCombat, BeforeCardPlayed |
| ViciousPower | VICIOUS | Buff | Counter | false | AfterPowerAmountChanged |
| VigorPower | VIGOR | Buff | Counter | false | BeforeAttack, ModifyDamageAdditive, AfterAttack |
| VitalSparkPower | VITAL_SPARK | Buff | Counter | false | AfterDamageReceived, BeforeSideTurnStart |
| VoidFormPower | VOID_FORM | Buff | Counter | false | BeforePowerAmountChanged, BeforeApplied, TryModifyEnergyCostInCombat +3 more |
| VulnerablePower | VULNERABLE | Debuff | Counter | false | ModifyDamageMultiplicative, AfterTurnEnd |
| WasteAwayPower | WASTE_AWAY | Debuff | Counter | false | ModifyMaxEnergy |
| WeakPower | WEAK | Debuff | Counter | false | ModifyDamageMultiplicative, AfterTurnEnd |
| WellLaidPlansPower | WELL_LAID_PLANS | Buff | Counter | false | BeforeFlushLate |
| WraithFormPower | WRAITH_FORM | Debuff | Counter | false | AfterSideTurnStart |

---
*End of reference.*