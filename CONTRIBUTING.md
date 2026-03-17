# Contributing

Guide for contributing to the STS2 RL Agent project.

---

## Development Environment Setup

### Prerequisites

- **Python 3.11+** (3.12 recommended)
- **Git**
- For training: NVIDIA GPU with CUDA support (optional but recommended)
- For bridge mod development: .NET 9 SDK, Godot 4.5.1 Mono, Slay the Spire 2 (Steam)

### Install

```bash
git clone <repo-url>
cd sts2-rl-agent

# Install in editable mode with dev + training dependencies
pip install -e ".[dev,train]"

# Verify everything works
pytest tests/
python scripts/benchmark.py
```

### Project Layout

```
sts2_env/           Python package (headless simulator + gym envs + bridge)
  core/             Combat engine (no content dependencies)
  cards/            Card definitions (577 cards)
  powers/           Status effects (260 powers)
  monsters/         Monster AI (121 monsters)
  relics/           Relic effects (290 relics)
  potions/          Potion effects (63 potions)
  encounters/       Encounter definitions
  gym_env/          Gymnasium environments
  bridge/           Real-game connection
bridge_mod/         C# bridge mod (Godot project)
scripts/            Training and benchmark scripts
tests/              Test suite (16 test files, 408 test functions)
docs/               Documentation
decompiled/         Decompiled C# source from sts2.dll (reference only)
extracted_pck/      Extracted Godot resources (reference only)
```

---

## Running Tests

```bash
# Run all tests
pytest tests/

# Run with verbose output
pytest tests/ -v

# Run a specific test file
pytest tests/test_combat_flow.py

# Run a specific test function
pytest tests/test_combat_flow.py::test_basic_turn_flow

# Run with coverage
pytest tests/ --cov=sts2_env --cov-report=term-missing
```

The test suite covers:
- `test_damage.py` -- damage/block calculation pipelines
- `test_combat_flow.py` -- turn flow, card play, end turn
- `test_cards.py` -- individual card effects
- `test_powers.py` -- power hook behavior
- `test_monster_ai.py` -- state machine transitions
- `test_encounters.py` -- encounter setup
- `test_gym_env.py` -- Gymnasium environment API
- `test_run_env.py` -- full-run environment
- `test_combat_parity.py` -- simulator vs expected game behavior
- `test_potions.py` -- potion effects
- `test_map_gen.py` -- map generation
- `test_rewards.py` -- reward calculation
- `test_shop.py` -- shop system
- `test_run_flow.py` -- run manager flow

---

## Code Style Conventions

- **Python 3.11+ features are welcome:** `match` statements, `type` aliases, `X | Y` union syntax.
- **Type hints everywhere:** All function signatures should have type annotations. Use `from __future__ import annotations` at the top of every module.
- **Docstrings:** Google-style docstrings for public functions and classes. Module-level docstrings for every file.
- **Imports:** Group as standard library, third-party, local. Use `TYPE_CHECKING` blocks for import-cycle-prone type hints.
- **Constants:** Game constants go in `core/constants.py`. Enum values go in `core/enums.py`.
- **Naming:**
  - Files: `snake_case.py`
  - Classes: `PascalCase`
  - Functions/variables: `snake_case`
  - Constants: `UPPER_SNAKE_CASE`
  - Power/Card/Relic IDs: `UPPER_SNAKE_CASE` enum members matching C# `Id.Entry`
- **No global mutable state** except for the card effect registry and power class registry, which are populated at import time and never modified after.

---

## How to Add New Cards

1. **Identify the card** in `decompiled/MegaCrit.Sts2.Core.Models.Cards/YourCard.cs`. Note the constructor parameters and `OnPlay` method.

2. **Add the CardId enum** in `core/enums.py`:
   ```python
   class CardId(Enum):
       # ... existing entries ...
       YOUR_CARD = auto()
   ```

3. **Register the card effect** in the appropriate character file (e.g., `cards/ironclad.py`):
   ```python
   @register_effect(CardId.YOUR_CARD)
   def _your_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
       deal_damage(combat, combat.player, target, card.base_damage)
       apply_power(combat, target, PowerId.VULNERABLE, card.effect_vars.get("vuln", 2))
   ```

4. **Create the card factory** in the same file or in the appropriate character starter deck:
   ```python
   def make_your_card(upgraded: bool = False) -> CardInstance:
       dmg = 10 if not upgraded else 14
       return CardInstance(
           card_id=CardId.YOUR_CARD,
           cost=2,
           card_type=CardType.ATTACK,
           target_type=TargetType.ANY_ENEMY,
           base_damage=dmg,
           effect_vars={"vuln": 2 if not upgraded else 3},
       )
   ```

5. **Add tests** in `tests/test_cards.py`:
   ```python
   def test_your_card_deals_damage_and_applies_vulnerable():
       combat = make_combat_with_deck([make_your_card()])
       # ... set up and verify
   ```

---

## How to Add New Powers

1. **Study the C# source** in `decompiled/MegaCrit.Sts2.Core.Models.Powers/YourPower.cs`. Identify which hook methods it overrides.

2. **Add the PowerId enum** in `core/enums.py`:
   ```python
   class PowerId(Enum):
       YOUR_POWER = auto()
   ```

3. **Create the PowerInstance subclass** in the appropriate power file (e.g., `powers/common.py`):
   ```python
   class YourPower(PowerInstance):
       power_type = PowerType.BUFF  # or DEBUFF
       stack_type = PowerStackType.COUNTER

       def __init__(self, amount: int):
           super().__init__(PowerId.YOUR_POWER, amount)

       def after_side_turn_start(self, owner, side, combat):
           if side == owner.side:
               combat.draw_cards(owner, self.amount)
   ```

4. **Register the power** at the bottom of the file:
   ```python
   register_power_class(PowerId.YOUR_POWER, YourPower)
   ```

5. **Add tests** in `tests/test_powers.py`.

---

## How to Add New Monsters

1. **Study the C# source** in `decompiled/MegaCrit.Sts2.Core.Models.Monsters/YourMonster.cs`. Note HP ranges, damage values, and `GenerateMoveStateMachine()`.

2. **Create the monster factory** in the appropriate act file (e.g., `monsters/act1.py`):
   ```python
   def create_your_monster(combat: CombatState, rng: Rng) -> tuple[Creature, MonsterAI]:
       creature = Creature(max_hp=rng.next_int(40, 50), monster_id="YOUR_MONSTER")

       # Define moves
       states = {}
       states["Attack"] = MoveState(
           "Attack",
           effect_fn=lambda c: deal_damage(c, creature, c.player, 12),
           intents=[SingleAttackIntent(12)],
           follow_up_id="Defend",
       )
       states["Defend"] = MoveState(
           "Defend",
           effect_fn=lambda c: gain_block_unpowered(creature, 8),
           intents=[BlockIntent(8)],
           follow_up_id="Branch",
       )
       states["Branch"] = RandomBranchState("Branch")
       states["Branch"].add_branch("Attack", repeat_type=MoveRepeatType.CANNOT_REPEAT, weight=60)
       states["Branch"].add_branch("Defend", repeat_type=MoveRepeatType.CANNOT_REPEAT, weight=40)

       ai = MonsterAI(states, initial_state_id="Branch")
       return creature, ai
   ```

3. **Add to an encounter** in `encounters/act1.py`.

4. **Add tests** in `tests/test_monster_ai.py`.

---

## How to Update After Game Patches

When Slay the Spire 2 receives an update:

### 1. Re-decompile sts2.dll

```bash
# Locate the updated DLL
# Windows: "C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll"

ilspycmd -p -o decompiled/ <path-to-sts2.dll>
```

### 2. Re-extract .pck resources (if needed)

```bash
gdre_tools --headless --recover=<path-to-game.pck>
```

### 3. Diff against previous decompilation

```bash
git diff decompiled/
```

Focus on changes in:
- `MegaCrit.Sts2.Core.Models.Cards/` -- new cards, balance changes
- `MegaCrit.Sts2.Core.Models.Powers/` -- new powers, mechanic changes
- `MegaCrit.Sts2.Core.Models.Monsters/` -- new monsters, AI changes
- `MegaCrit.Sts2.Core.Combat/` -- turn flow changes
- `MegaCrit.Sts2.Core.Hooks/` -- new hooks

### 4. Update simulator code

- Add new CardId/PowerId enum values
- Implement new card effects
- Update changed card values (damage, cost, etc.)
- Update monster HP/damage values and AI patterns
- Update power behaviors

### 5. Update reference docs

Regenerate `docs/CARDS_REFERENCE.md`, `docs/POWERS_REFERENCE.md`, etc. from the new decompiled source.

### 6. Run tests

```bash
pytest tests/ -v
```

Fix any failures caused by changed game mechanics.

### 7. Update bridge mod

If the game's C# API changed:
- Update Harmony patch targets in `bridge_mod/MainFile.cs`
- Update state serialization if new fields are needed
- Rebuild with: `dotnet build bridge_mod/ -c Release`

---

## Pull Request Guidelines

1. **One concern per PR.** Don't mix new features with bug fixes.
2. **Tests required** for all new card effects, powers, and monster AI.
3. **Run the full test suite** before submitting: `pytest tests/`
4. **Run the benchmark** to check for performance regressions: `python scripts/benchmark.py`
5. **Update docs** if you add new features or change behavior.
6. **Keep the simulator faithful** to the decompiled C# source. Document any intentional deviations.
