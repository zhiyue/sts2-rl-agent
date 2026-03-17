# Simulator Architecture

Internal architecture of the headless Python combat and run simulator in `sts2_env/`.

---

## Module Dependency Graph

```
core/                       Foundation layer (no game-content dependencies)
  combat.py                 CombatState: turn flow, card play, pile management
  creature.py               Creature: HP, block, powers, power class registry
  hooks.py                  Central hook dispatch (~25 hook points)
  damage.py                 Damage/block calculation pipelines
  enums.py                  CardId, PowerId, IntentType, CardType, etc.
  constants.py              Game constants (MAX_HAND_SIZE=10, BASE_ENERGY=3, etc.)
  rng.py                    Seeded RNG for reproducible simulations

cards/                      Card definitions (depends on core/)
  base.py                   CardInstance dataclass
  effects.py                12 composable effect primitives
  registry.py               CardId -> effect dispatch via @register_effect
  ironclad.py, silent.py, defect.py, necrobinder.py, regent.py, colorless.py, status.py

powers/                     Status effects (depends on core/)
  base.py                   PowerInstance base class with ~30 hook method stubs
  common.py                 Core powers (Strength, Vulnerable, Weak, Frail, etc.)
  damage_modifiers.py       Damage pipeline hooks
  block_modifiers.py        Block pipeline hooks
  card_play_effects.py      On-card-play triggers
  damage_reactions.py       Thorns, reactive powers
  duration.py               Tick-down / duration powers
  turn_effects.py           Start/end of turn triggers
  monster.py                Monster-specific powers

monsters/                   Monster AI (depends on core/)
  state_machine.py          MoveState, RandomBranch, ConditionalBranch, MonsterAI
  intents.py                Intent types (attack, multi-attack, defend, buff, etc.)
  act1_weak.py, act1.py, act2.py, act3.py, act4.py

relics/                     Relic effects (depends on core/)
potions/                    Potion effects (depends on core/)
orbs/                       Orb mechanics for Defect (depends on core/)
characters/                 Character starting states
encounters/                 Encounter definitions (depends on monsters/)
events/                     Event decision trees
map/                        Map generation algorithm

run/                        Full-run state management (depends on all above)
  run_manager.py            Run loop (map -> room -> rewards -> next room)
  run_state.py              Persistent run state (deck, relics, potions, gold, HP)
  rewards.py                Card/gold/potion rewards
  shop.py                   Shop system
  rest_site.py              Rest site (heal/upgrade)
  events.py                 Event handler

gym_env/                    Gymnasium environments (depends on run/, encounters/)
  combat_env.py             Single-combat env (Discrete(115), obs 131-dim)
  run_env.py                Full-run env (Discrete(157), obs 151-dim)
  observation.py            CombatState -> 131-dim float32 vector
  action_space.py           Action encoding + masking
  reward.py                 Reward shaping

bridge/                     Real-game connection (depends on gym_env/)
  client.py                 TCP client
  protocol.py               Message types, phases
  state_adapter.py          Game JSON -> observation vector
  agent_runner.py           Main agent loop
```

Dependency flow is strictly top-down: `core -> cards/powers/monsters/relics -> encounters/events -> run -> gym_env -> bridge`. No circular imports.

---

## Core Engine Design

### CombatState (`core/combat.py`)

The central simulation class. One instance per combat encounter.

**State held:**
- `player: Creature` -- the player entity
- `enemies: list[Creature]` -- enemy entities
- `enemy_ais: dict[int, MonsterAI]` -- AI state machines keyed by combat_id
- Card piles: `hand`, `draw_pile`, `discard_pile`, `exhaust_pile`, `play_pile`
- `energy: int`, `stars: int`, `gold: int`
- `round_number: int`, `current_side: CombatSide`, `is_over: bool`, `player_won: bool`
- `relics: list[RelicInstance]` -- player relics for hook dispatch
- `rng: Rng` -- seeded RNG for shuffle and random effects

**Turn flow** (mirrors decompiled `CombatManager`):

```
start_combat()
  -> fire_before_combat_start (relics trigger, e.g. Anchor gives 10 block)
  -> all enemies roll_move (determine first intent)
  -> _start_player_turn()

_start_player_turn()
  -> fire_before_side_turn_start(PLAYER)
  -> clear player block (round > 1 only, respects Barricade via should_clear_block hook)
  -> reset energy to max_energy (may be modified by relics)
  -> modify_hand_draw (default 5, modified by powers/relics)
  -> handle innate cards (round 1 only: move innate to top of draw pile)
  -> draw cards
  -> fire_after_side_turn_start(PLAYER)

[Player actions: play_card() / end_player_turn()]

end_player_turn()
  -> fire_before_turn_end(PLAYER)
  -> resolve end-of-turn hand: exhaust ethereals, execute turn-end-in-hand effects
     (Burn, Decay, Doubt, etc.), discard non-retained cards
  -> fire_after_turn_end(PLAYER)
  -> check for extra turn (should_take_extra_turn hook)
  -> _execute_enemy_turn()

_execute_enemy_turn()
  -> fire_before_side_turn_start(ENEMY)
  -> clear block for each alive enemy
  -> fire_after_side_turn_start(ENEMY)
  -> each enemy: perform current move, then roll_move for next turn
  -> fire_before_turn_end(ENEMY) / fire_after_turn_end(ENEMY)
  -> round_number++, then _start_player_turn()
```

### Creature (`core/creature.py`)

Represents any combat entity (player or monster).

Uses `__slots__` for memory efficiency: `max_hp`, `current_hp`, `block`, `powers`, `side`, `is_player`, `monster_id`, `combat_id`, `stars`.

Key operations:
- `gain_block(amount)` -- capped at 999
- `damage_block(amount, unblockable)` -- returns amount absorbed
- `lose_hp(amount)` -- returns actual HP lost
- `apply_power(power_id, amount)` -- handles Artifact blocking for debuffs, power stacking, removal at 0
- `clear_block(combat)` -- uses hook dispatch to check Barricade

**Power class registry:** Powers register themselves via `register_power_class(PowerId, cls)` at import time. When `apply_power` is called, it looks up the class, creates an instance, and stores it in `self.powers: dict[PowerId, PowerInstance]`.

### Damage Pipeline (`core/damage.py`)

Two main functions:

1. `calculate_damage(base, dealer, target, props, combat)` -- runs the full modifier pipeline via `hooks.modify_damage()`:
   - Additive pass (Strength adds to dealer's attacks)
   - Multiplicative pass (Vulnerable = 1.5x on target, Weak = 0.75x on dealer)
   - Cap pass (Intangible caps at 1)
   - Floor and clamp to 0

2. `apply_damage(target, damage, props, combat, dealer)` -- applies calculated damage:
   - `fire_before_damage_received` (Thorns, Flame Barrier trigger here)
   - Block absorption (unless Unblockable)
   - HP loss modification via `modify_hp_lost` (Intangible, Tungsten Rod)
   - `target.lose_hp(remaining)`
   - `fire_after_damage_received` / `fire_after_damage_given`
   - Returns `DamageResult(blocked, hp_lost, was_killed, unblocked_damage)`

`ValueProp` flags control which modifiers apply:
- `MOVE (0x8)` -- from a card or monster move
- `UNPOWERED (0x4)` -- bypasses Strength/Weak/Vulnerable
- `UNBLOCKABLE (0x2)` -- bypasses block

Only `IsPoweredAttack()` (has MOVE and lacks UNPOWERED) enables Strength/Weak/Vulnerable.

### Hook System (`core/hooks.py`)

Centralized dispatch matching `CombatState.IterateHookListeners()` from the C# source.

**Dispatch order:** Powers (all creatures) -> Relics (player only).

**~25 hook points** organized as:
- **Modification hooks** (return a value): `modify_damage`, `modify_block`, `modify_hp_lost`, `modify_hand_draw`, `modify_max_energy`, `modify_card_play_count`
- **Condition hooks** (return bool): `should_clear_block`, `should_reset_energy`, `should_flush`, `should_play`, `should_draw`, `should_take_extra_turn`
- **Event hooks** (fire-and-forget): `fire_before_card_played`, `fire_after_card_played`, `fire_after_card_exhausted`, `fire_after_card_discarded`, `fire_after_card_drawn`, `fire_before_turn_end`, `fire_after_turn_end`, `fire_before_side_turn_start`, `fire_after_side_turn_start`, `fire_before_combat_start`, `fire_after_combat_victory`, `fire_after_combat_end`, `fire_before_damage_received`, `fire_after_damage_received`, `fire_after_damage_given`, `fire_after_block_gained`, `fire_after_block_cleared`, `fire_after_energy_reset`, `fire_after_shuffle`, `fire_after_hand_emptied`

---

## How Cards Work

### CardInstance dataclass (`cards/base.py`)

Every card in combat is a `CardInstance` with fields:
- `card_id: CardId` -- enum member identifying the card
- `cost: int`, `card_type: CardType`, `target_type: TargetType`, `rarity: CardRarity`
- `base_damage: int | None`, `base_block: int | None`
- `keywords: frozenset[str]` -- "exhaust", "ethereal", "innate", "retain", "unplayable"
- `tags: frozenset[str]` -- card tags (e.g. "Sly", "Cozy")
- `effect_vars: dict[str, int]` -- additional numeric parameters for effects
- `has_energy_cost_x: bool`, `star_cost: int` -- X-cost and star resource support
- `combat_vars: dict[str, int]` -- persistent per-combat state (Rampage extra damage, etc.)

Properties derived from keywords: `is_attack`, `is_skill`, `is_power`, `exhausts`, `is_unplayable`, `is_ethereal`, `is_innate`, `is_retain`.

### @register_effect decorator (`cards/registry.py`)

Card effects are registered with a decorator pattern:

```python
from sts2_env.cards.registry import register_effect
from sts2_env.core.enums import CardId

@register_effect(CardId.STRIKE_IRONCLAD)
def _strike_ironclad(card, combat, target):
    deal_damage(combat, combat.player, target, card.base_damage)
```

`play_card_effect(card, combat, target)` looks up `_CARD_EFFECTS[card.card_id]` and calls it. The effect function receives the `CardInstance`, `CombatState`, and resolved target `Creature`.

### Effect primitives (`cards/effects.py`)

12 composable building blocks:
- `deal_damage(combat, dealer, target, base_damage, hits=1, props=MOVE)`
- `deal_damage_to_all_enemies(combat, dealer, base_damage, hits=1)`
- `deal_damage_to_random_enemy(combat, dealer, base_damage, hits=1)`
- `gain_block(combat, target, base_block, props=MOVE)`
- `gain_block_unpowered(target, amount)` -- flat block, no modifiers
- `apply_power(combat, target, power_id, amount)`
- `apply_power_to_all_enemies(combat, power_id, amount)`
- `draw_cards(combat, count)`
- `exhaust_card(combat, card)`
- `gain_energy(combat, amount)` / `lose_energy(combat, amount)`
- `lose_hp(combat, target, amount)` -- self-damage (unblockable/unpowered)
- `resolve_x_cost(combat)` -- spend all energy, return amount

---

## How Powers Work

### PowerInstance base class (`powers/base.py`)

All powers inherit from `PowerInstance` and override hook methods.

Class-level attributes define metadata:
- `power_id: PowerId` -- identifies the power
- `power_type: PowerType` -- BUFF, DEBUFF, or NONE
- `stack_type: PowerStackType` -- COUNTER, SINGLE, or NONE
- `allow_negative: bool` -- whether the amount can go below 0

Instance state:
- `amount: int` -- current stacks/counter value
- `skip_next_tick: bool` -- when a debuff is applied to the player, the first tick is skipped (so it lasts the full turn)

Hook methods (all no-ops by default, subclasses override as needed):
- Damage: `modify_damage_additive`, `modify_damage_multiplicative`, `modify_damage_cap`
- Block: `modify_block_additive`, `modify_block_multiplicative`
- HP: `modify_hp_lost`
- Turn: `before_side_turn_start`, `after_side_turn_start`, `before_turn_end`, `after_turn_end`
- Legacy turn: `on_turn_end_enemy_side` (duration tick-down), `on_turn_start_own_side` (Poison, Ritual)
- Card: `before_card_played`, `after_card_played`, `after_card_exhausted`, `on_card_drawn`
- Damage events: `before_damage_received`, `after_damage_received`, `after_damage_given`
- Other: `should_clear_block`, `try_block_debuff`, `modify_hand_draw`, `modify_card_play_count`

### Example: StrengthPower

```python
class StrengthPower(PowerInstance):
    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    allow_negative = True

    def __init__(self, amount):
        super().__init__(PowerId.STRENGTH, amount)

    def modify_damage_additive(self, owner, dealer, target, props):
        if dealer is owner and props.is_powered_attack:
            return self.amount
        return 0
```

### Registration

Powers register at module import time:
```python
register_power_class(PowerId.STRENGTH, StrengthPower)
```

---

## How Monster AI Works

### State Machine (`monsters/state_machine.py`)

Three node types:

**MoveState** -- a concrete monster action:
- `state_id: str` -- unique name (e.g. "Thrash", "Bite")
- `effect_fn: Callable[[CombatState], None]` -- executes the move's combat effects
- `intents: list[Intent]` -- displayed intent (attack, defend, buff, etc.)
- `follow_up_id: str | None` -- fixed next state (for linear/cyclic patterns)
- `must_perform_once: bool` -- cannot transition away until performed

**RandomBranchState** -- weighted random selection:
- Contains a list of `WeightedBranch` entries, each with:
  - `state_id`, `base_weight`, `repeat_type`, `max_times`, `cooldown`
- Repeat rules: `CAN_REPEAT_FOREVER`, `CANNOT_REPEAT` (not twice in a row), `CAN_REPEAT_X_TIMES(n)`, `USE_ONLY_ONCE`
- Weights are dynamically adjusted based on `state_log` history

**ConditionalBranchState** -- first-matching condition:
- Contains `(condition_fn, state_id)` pairs
- Evaluates conditions in order, picks the first that returns True
- Used for HP thresholds, summoning conditions, etc.

### MonsterAI container

Holds the state dictionary and manages transitions:

```
roll_move(rng) -> MoveState
  1. Get next state ID from current state's get_next_state()
  2. Walk through branch states until a MoveState is reached
  3. Record to state_log for repeat-constraint tracking
```

The first move is held until `on_move_performed()` is called. After that, `roll_move()` advances to the next state each time.

### Common AI patterns

- **Fixed cycle:** Thrash -> Enlarge -> Sting -> Adapt -> Guard -> (repeat). Each MoveState's `follow_up_id` points to the next.
- **Alternating:** Clamp -> Screech -> Clamp -> Screech. A -> B -> A via `follow_up_id`.
- **Random:** RandomBranch with weighted choices and repeat constraints.
- **Conditional:** ConditionalBranch checking HP < 50%, can_summon(), etc.
- **Hybrid:** ConditionalBranch -> RandomBranch -> MoveState chains.

---

## How the Gymnasium Environments Work

### CombatEnv (`gym_env/combat_env.py`)

Single-combat training environment.

- **Observation:** `Box(low=-1, high=10, shape=(131,), dtype=float32)`
- **Action space:** `Discrete(115)` = 1 end_turn + 10 untargeted card actions + 50 targeted card actions + 54 potion actions (`9 slots * (1 untargeted + 5 enemy targets)`)
- **Action masking:** `action_masks()` returns `int8[115]` marking legal actions. Required by `MaskablePPO`.

On `reset()`:
1. Create Ironclad starter deck
2. Create `CombatState` with random seed
3. Pick random encounter from Act 1 pool
4. Call `combat.start_combat()`

On `step(action)`:
1. Decode action -> (hand_index, target_index) or end_turn
2. Call `combat.play_card()` or `combat.end_player_turn()`
3. Return (obs, reward, terminated, truncated, info)

**Reward:** Sparse -- +1.0 win, -1.0 loss, 0.0 otherwise.

### Observation encoding (`gym_env/observation.py`)

131-dimensional flat float32 vector:

| Segment | Dims | Encoding |
|---------|------|----------|
| Player state | 4 | hp/max_hp, block/50, energy/10, max_energy/10 |
| Player powers | 6 | str/20, dex/20, vuln/20, weak/20, frail/20, artifact/20 |
| Hand (10 slots) | 50 | 5 features per card: card_id_norm, cost/5, damage/50, block/50, is_attack |
| Pile summaries | 6 | draw/20, discard/20, exhaust/20, draw_attacks/10, draw_skills/10, discard_attacks/10 |
| Enemies (5 slots) | 65 | 13 features per enemy: alive, hp%, block/50, intent_onehot(5), intent_dmg/30, intent_hits/5, vuln/10, weak/10, str/10 |

Card ID is normalized as `(card_index + 1) / (total_card_ids + 1)` to produce a float in (0, 1).

### Action space (`gym_env/action_space.py`)

115 discrete actions:
- `0` -- end turn
- `1..10` -- play card from hand slot 0..9 (self/none/all-enemies target)
- `11..60` -- play card i targeting enemy j: `action = 1 + 10 + hand_idx * 5 + enemy_idx`
- `61..114` -- use potion from slot-major layout: one untargeted action plus five enemy-targeted actions per slot

### RunEnv vs CombatEnv

| | CombatEnv | RunEnv |
|--|-----------|--------|
| Scope | Single combat | Full multi-act run |
| Obs size | 131 | 151 (131 combat + 20 run-level) |
| Action space | Discrete(115) | Discrete(157) |
| Phases | Combat only | Combat + map + card_reward + boss_relic + shop + rest + event + treasure |
| Reward | +1 win / -1 loss | +1 run win / -1 death or timeout |
| Run-level obs | N/A | act/floor/hp_ratio/gold/deck_size/relic_count/potions/phase_onehot(8)/ascension/is_elite/is_boss |

RunEnv action layout (Discrete(157)):
- 0-114: combat actions (same as CombatEnv)
- 115-119: map choices (0-4 paths)
- 120-123: card reward (pick 0-2, or skip)
- 124-126: extra card reward picks when present
- 127-129: boss relic choice (0-2)
- 130-139: shop (leave + buy items)
- 140-144: rest site options
- 145-148: event choices (0-3)
- 149: treasure collect or card-reward reroll, depending on phase
- 150-156: acting-player selection during multiplayer combat

---

## Performance

**Benchmark results** (modern CPU, single thread):
```
Episodes:       1000
Total steps:    28101
Time:           0.78s
Episodes/sec:   1276
Steps/sec:      28101
```

**Bottlenecks:**
1. `inspect.signature()` in `fire_after_card_drawn` -- called on every card draw to determine parameter count of power `on_card_drawn` methods. This is on the hot path and uses reflection. Could be replaced with a pre-computed dispatch table.
2. Power iteration in hook dispatch -- iterates all creatures' powers for every hook call. With many powers active, this is O(creatures * powers * hooks_per_step).
3. Python interpreter overhead -- pure Python is inherently slower than C/C++. A Cython port of the core loop could yield 5-10x improvement.
4. List operations in card piles -- `draw_pile.pop(0)` is O(n). Using `collections.deque` would make it O(1).

---

## Known Bugs Found in Code Review

### Fixed

1. **EchoForm/modify_card_play_count was missing.** The hook for modifying how many times a card is played (needed for EchoForm, Double Tap) was not wired up. Fixed by adding `modify_card_play_count` to the hook dispatch and calling it in `play_card()`.

2. **Enemy round-1 block clear.** Enemies were not clearing block at the start of their turn on round 1. Fixed in `_execute_enemy_turn()` to always clear enemy block.

3. **Energy always 3 in bridge.** The C# bridge mod was using `CardCmd.AutoPlay()` which does not spend energy. Fixed by switching to `PlayCardAction` which properly deducts energy cost.

### Remaining

4. **`inspect.signature` on hot path.** In `fire_after_card_drawn` (hooks.py line 296-305), `inspect.signature(method).parameters` is called every time a card is drawn to determine whether a power's `on_card_drawn` method accepts 3 or 4 parameters. This is a backward-compatibility shim and should be replaced with a uniform 4-parameter signature for all power implementations.

5. **run_env swallows exceptions.** In `STS2RunEnv.step()` (run_env.py line 238), a bare `except Exception` catches all simulation errors and force-ends the run as a loss. While this prevents training crashes, it silently hides bugs. Simulation errors should be logged before being swallowed.

6. **AnimationSpeedPatch fails to apply.** The Harmony patch targeting `MegaAnimationState.SetTimeScale` fails on some game versions because the method signature does not match. The patch is skipped with a log message; the game runs without animation acceleration.

7. **Mod abandon-run popup path may not match.** The Godot node path `/root/Game/RootSceneContainer/MainMenu/...` for the abandon-run confirmation popup may differ across game versions.

8. **Pile summary mismatch in bridge.** The state adapter (`state_adapter.py`) cannot compute draw/discard attack/skill counts because the bridge only sends pile counts, not full pile composition. These 3 features are always 0 in bridge mode but nonzero during simulator training, creating a distribution shift.
