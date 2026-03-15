# Decompilation Guide

How to decompile Slay the Spire 2 for analysis and simulator development. Covers tools, process, key findings, and lessons learned.

---

## Overview

STS2 is built on Godot 4 with C# / .NET 9. The game logic resides in a single DLL (`sts2.dll`), and resources are packaged in Godot `.pck` files. Unlike some games, the code is not obfuscated -- class names, method names, and game logic are fully readable after decompilation.

Two extraction steps are needed:

1. **DLL decompilation** (game logic): `sts2.dll` -> ~3,300 C# source files
2. **PCK extraction** (resources): `.pck` files -> ~24,000 resource files (images, animations, localization, scenes)

---

## Tools

| Tool | Version Used | Purpose | Install |
|------|-------------|---------|---------|
| [ILSpy](https://github.com/icsharpcode/ILSpy) | 10.0 | GUI decompiler for .NET DLLs | Download release from GitHub |
| [ilspycmd](https://github.com/icsharpcode/ILSpy) | 9.1.0 | CLI decompiler (batch extraction) | `dotnet tool install -g ilspycmd` |
| [GDRE Tools](https://github.com/GDRETools/gdsdecomp) | 2.4.0 | Godot PCK extraction | Download release from GitHub |
| [dnSpy](https://github.com/dnSpy/dnSpy) | -- | Alternative .NET decompiler (archived) | GitHub releases |
| [dotPeek](https://www.jetbrains.com/decompiler/) | -- | JetBrains .NET decompiler (free) | JetBrains website |

**Recommended:** ILSpy for interactive browsing, ilspycmd for bulk extraction, GDRE Tools for PCK files.

---

## Step 1: Locate Game Files

After installing STS2 via Steam, the game directory is typically:

- **Windows:** `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\`
- **Linux:** `~/.local/share/Steam/steamapps/common/Slay the Spire 2/`
- **macOS:** `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/`

Key files:

```
Slay the Spire 2/
  sts2.exe                              # Godot launcher
  sts2.pck                              # Main resource pack
  data_sts2_windows_x86_64/
    sts2.dll                            # All game logic (~15 MB)
    0Harmony.dll                        # Harmony library (for modding)
    GodotSharp.dll                      # Godot C# bindings
    GodotSharpEditor.dll                # Godot editor bindings
```

---

## Step 2: Decompile sts2.dll

### Using ilspycmd (recommended for bulk extraction)

```bash
# Install the CLI tool
dotnet tool install -g ilspycmd

# Decompile to a directory of C# source files
ilspycmd -p -o decompiled/ "C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll"
```

The `-p` flag writes one file per class, organized by namespace. This produces approximately 3,300 `.cs` files:

```
decompiled/
  MegaCrit.Sts2.Core.Combat/
    CombatManager.cs
    CombatState.cs
    ...
  MegaCrit.Sts2.Core.Commands/
    DamageCmd.cs
    CreatureCmd.cs
    CardPileCmd.cs
    ...
  MegaCrit.Sts2.Core.GameActions/
    PlayCardAction.cs
    EndTurnAction.cs
    ...
  MegaCrit.Sts2.Core.Hooks/
    Hook.cs                             # Central event dispatch
  MegaCrit.Sts2.Core.Models.Cards/      # 577 card implementations
  MegaCrit.Sts2.Core.Models.Monsters/   # 121 monster AI
  MegaCrit.Sts2.Core.Models.Powers/     # 260 status effects
  MegaCrit.Sts2.Core.Models.Relics/     # 290 relics
  MegaCrit.Sts2.Core.Models.Potions/    # 63 potions
  MegaCrit.Sts2.Core.Models.Encounters/ # 88 encounters
  MegaCrit.Sts2.Core.Models.Events/     # 68 events
  MegaCrit.Sts2.Core.Models.Acts/       # 4 acts
  MegaCrit.Sts2.Core.Models.Characters/ # 5 characters
  MegaCrit.Sts2.Core.Map/              # Map generation
  MegaCrit.Sts2.Core.AutoSlay/         # Built-in automation system
  ... (~40 more namespaces)
```

### Using ILSpy GUI (recommended for interactive analysis)

1. Download the self-contained ILSpy release from GitHub
2. Open `sts2.dll` in ILSpy
3. Browse the type tree on the left panel
4. Right-click any class/namespace and "Save Code" to export

The GUI is useful for:
- Following cross-references (Ctrl+Click on a type to navigate)
- Searching across the entire assembly (Ctrl+Shift+F)
- Viewing IL alongside decompiled C#
- Understanding inheritance hierarchies

---

## Step 3: Extract PCK Resources

```bash
# Using GDRE Tools headless mode
gdre_tools.exe --headless --recover="C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\sts2.pck"
```

This extracts approximately 24,000 files:

```
extracted_pck/
  images/                    # Card art, relic icons, UI elements (PNG)
  animations/                # Spine skeletal animations (.skel + .atlas + .png)
  localization/
    eng/                     # English text (JSON)
      cards.json             # Card names and descriptions
      relics.json            # Relic names and descriptions
      powers.json            # Power names and descriptions
      monsters.json          # Monster names
      events.json            # Event text
      ...
  scenes/                    # Godot scene files (.tscn)
  resources/                 # Godot resources (.tres)
  scripts/                   # GDScript files (.gd) -- UI and glue logic
```

The most valuable extractions for simulator development:
- **Localization files:** Contain all user-facing text with SmartFormat templates
- **Images:** Card portraits and relic icons (useful for debugging/visualization)
- **Spine animations:** Monster animations (not needed for headless sim)

---

## Key Namespace Guide

The decompiled source is organized by namespace. Here is a guide to the most important ones for simulator development:

### Core Engine

| Namespace | Files | Purpose |
|-----------|-------|---------|
| `MegaCrit.Sts2.Core.Combat` | ~10 | CombatManager, CombatState, CombatSide |
| `MegaCrit.Sts2.Core.Commands` | ~15 | Low-level commands (DamageCmd, CreatureCmd, CardPileCmd, PowerCmd) |
| `MegaCrit.Sts2.Core.Commands.Builders` | ~5 | AttackCommand builder pattern |
| `MegaCrit.Sts2.Core.GameActions` | ~10 | High-level actions (PlayCardAction, EndTurnAction) |
| `MegaCrit.Sts2.Core.Hooks` | 1 | Central Hook static class (~100 hook methods) |
| `MegaCrit.Sts2.Core.Entities.Creatures` | ~5 | Creature base class (HP, Block, Powers) |
| `MegaCrit.Sts2.Core.Entities.Players` | ~5 | PlayerCombatState (Energy, Stars, Piles) |
| `MegaCrit.Sts2.Core.ValueProps` | ~3 | ValueProp flags (Move, Unpowered, Unblockable) |

### Game Content

| Namespace | Files | Purpose |
|-----------|-------|---------|
| `MegaCrit.Sts2.Core.Models.Cards` | ~577 | One file per card |
| `MegaCrit.Sts2.Core.Models.Powers` | ~260 | One file per status effect |
| `MegaCrit.Sts2.Core.Models.Monsters` | ~121 | One file per monster |
| `MegaCrit.Sts2.Core.Models.Relics` | ~290 | One file per relic |
| `MegaCrit.Sts2.Core.Models.Potions` | ~63 | One file per potion |
| `MegaCrit.Sts2.Core.Models.Encounters` | ~88 | Encounter compositions |
| `MegaCrit.Sts2.Core.Models.Events` | ~68 | Event decision trees |
| `MegaCrit.Sts2.Core.Models.Characters` | ~7 | Character definitions |
| `MegaCrit.Sts2.Core.Models.Acts` | ~4 | Act definitions |

### Supporting Systems

| Namespace | Purpose |
|-----------|---------|
| `MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine` | Monster AI state machine |
| `MegaCrit.Sts2.Core.MonsterMoves.Intents` | Intent types |
| `MegaCrit.Sts2.Core.Map` | Map generation (StandardActMap, MapPoint) |
| `MegaCrit.Sts2.Core.AutoSlay` | Built-in automation (debug/test tool) |
| `MegaCrit.Sts2.Core.Modding` | Mod loading infrastructure |

---

## Key Patterns in the Decompiled Code

### Pattern 1: AbstractModel and Hook System

All game entities (cards, powers, relics, monsters) inherit from `AbstractModel`, which defines ~100+ virtual methods that correspond to hook points:

```csharp
abstract class AbstractModel
{
    virtual int ModifyDamageAdditive(Creature dealer, Creature target, ValueProp vp) => 0;
    virtual float ModifyDamageMultiplicative(...) => 1.0f;
    virtual int ModifyDamageCap(...) => damage;
    virtual int ModifyBlockAdditive(...) => 0;
    virtual float ModifyBlockMultiplicative(...) => 1.0f;
    virtual void AfterSideTurnStart(CombatSide side) {}
    virtual void AfterTurnEnd(CombatSide side) {}
    virtual void BeforeCardPlayed(CardModel card, ...) {}
    virtual void AfterCardPlayed(CardModel card, ...) {}
    // ... ~100 more hooks
}
```

The `Hook` static class iterates over all registered `AbstractModel` instances and calls these methods at the appropriate time.

### Pattern 2: Monster State Machine

Every monster defines its AI via `GenerateMoveStateMachine()`:

```csharp
class Chomper : MonsterModel
{
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var clamp = new MoveState("Clamp", new[] { new SingleAttackIntent(ClampDamage) },
            targets => DamageCmd.Attack(ClampDamage).Targeting(targets).Execute());
        var screech = new MoveState("Screech", new[] { new DebuffIntent() },
            targets => PowerCmd.Apply<VulnerablePower>(targets, 2));

        clamp.FollowUpState = screech;
        screech.FollowUpState = clamp;

        return new MonsterMoveStateMachine("Start", clamp);
    }
}
```

Three state types: `MoveState` (concrete action), `RandomBranchState` (weighted random), `ConditionalBranchState` (condition-based branching).

### Pattern 3: Card Effects

Cards implement `OnPlay()` using composable command methods:

```csharp
class Bash : CardModel
{
    public Bash() : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        DynamicVars.DamageVar = new DamageVar(8m);
        DynamicVars.PowerVar = new PowerVar(2m);
    }

    override void OnPlay(ctx, cardPlay)
    {
        DamageCmd.Attack(DynamicVars.Damage).FromCard(this).Targeting(target).Execute(ctx);
        PowerCmd.Apply<VulnerablePower>(target, DynamicVars.Power, player, this);
    }
}
```

### Pattern 4: DynamicVars and Upgrades

Numeric values use `DynamicVar` types (DamageVar, BlockVar, PowerVar, CardsVar) that support upgrade modifications:

```csharp
override void OnUpgrade()
{
    DynamicVars.DamageVar.UpgradeValueBy(3);  // Damage 8 -> 11
    DynamicVars.PowerVar.UpgradeValueBy(1);   // Vuln 2 -> 3
}
```

### Pattern 5: Ascension Scaling

Monster stats scale with ascension level:

```csharp
int ClampDamage => AscensionHelper.GetValueIfAscension(
    AscensionLevel.DeadlyEnemies, 14, 11);  // Asc: 14, Normal: 11

int MinInitialHp => AscensionHelper.GetValueIfAscension(
    AscensionLevel.ToughEnemies, 28, 25);
```

---

## Lessons Learned

### 1. Hook execution order matters

The game iterates hook listeners in a specific order (player powers, then relics, then enemy powers). When multiple modifiers stack (e.g., Strength + Pen Nib + Vulnerable), the additive pass happens first, then the multiplicative pass. This two-pass design is hardcoded in `Hook.ModifyDamageInternal()`.

### 2. Powered vs Unpowered attacks

Not all damage is affected by Strength/Weak/Vulnerable. The `ValueProp` flags determine this:

- `Move` flag = damage from a card or monster move
- `Unpowered` flag = immune to Strength/Weak/Vulnerable (e.g., Thorns, Poison)
- Only `IsPoweredAttack()` (has Move, lacks Unpowered) applies these modifiers

This is a common source of simulator bugs.

### 3. Debuff decay timing

Vulnerable, Weak, and Frail decay at the end of the **enemy** turn, not the player turn. This means applying 1 stack of Vulnerable covers the entire player turn. The `AfterTurnEnd(CombatSide.Enemy)` handler decrements the counter.

### 4. Block is not cleared on turn 1

The game does not clear player block at the start of turn 1. This allows relics like Anchor (gain 10 block at combat start) to persist into the first turn. The `ShouldClearBlock` hook check includes a round-1 exemption.

### 5. AutoSlay is a goldmine

STS2 ships with a complete `AutoSlay` system in `MegaCrit.Sts2.Core.AutoSlay`. It is gated behind `NGame.IsReleaseGame()` returning true (which it does not in the shipping build). This system:

- Handles all UI navigation automatically
- Has room-type-specific handlers (Combat, Map, Shop, Rest, Event, CardReward)
- Includes error recovery via a Watchdog
- Supports card selection interfaces (`ICardSelector`)

By patching `IsReleaseGame()` to return false, we get a complete automation backbone. The v2 bridge mod uses this approach.

### 6. Localization files contain game data

The extracted localization JSONs (`localization/eng/*.json`) contain not just display text but also SmartFormat templates with numeric values. These can be cross-referenced with decompiled code to verify damage/block values:

```json
"BASH_DESC": "Deal {DamageVar} damage.\nApply {VulnerablePower:diff()} [gold]Vulnerable[/gold]."
```

### 7. spire-codex saves work

The [spire-codex](https://github.com/ptrlrd/spire-codex) project has already extracted structured JSON data for all cards, monsters, powers, relics, potions, encounters, and events. The `data/*.json` files can be used directly instead of re-parsing the decompiled source.

However, spire-codex only extracts static data (names, costs, damage values). Dynamic logic (card effects, monster AI, relic triggers) must still be read from the decompiled C# source.

### 8. STS2 vs STS1 key differences

| Aspect | STS1 | STS2 |
|--------|------|------|
| Engine | libGDX (Java) | Godot 4 (C#/.NET 9) |
| Decompiler | CFR / JD-GUI | ILSpy / ilspycmd |
| Resource format | JAR (ZIP with .class) | PCK + DLL |
| New resource: Stars | N/A | Second resource type (some cards cost stars) |
| New mechanic: Enchantments | N/A | 23 enchantments modify card behavior |
| New mechanic: Orbs (expanded) | Lightning/Frost/Dark/Plasma | 5 orb types |
| Characters | 4 | 5 playable + 2 additional |
| Acts | 3 + optional 4th | 4 (Overgrowth, Hive, Glory, Underdocks) |

---

## Workflow for Adding New Content to the Simulator

When the game updates and adds new cards, monsters, or powers:

1. **Re-decompile:** Run `ilspycmd -p -o decompiled_new/ sts2.dll` on the updated DLL
2. **Diff:** Compare with previous decompilation to find new/changed files
3. **Re-extract PCK:** Run GDRE Tools on the updated PCK for new localization
4. **Implement:** Read the new decompiled class and add a Python equivalent
5. **Test:** Write a unit test that verifies the behavior matches the C# source
6. **Register:** Add the new content to the appropriate registry

For bulk updates, the [spire-codex](https://github.com/ptrlrd/spire-codex) parsers can be re-run to generate updated JSON data files.
