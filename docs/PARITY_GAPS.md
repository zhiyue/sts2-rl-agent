# Parity Gaps to Exact Match

This document tracks the currently confirmed blockers between `sts2_env` and the decompiled game logic under `decompiled/`.

As of 2026-03-17, the correct status is:

- Several gaps called out by earlier audits are now fixed in code.
- The previous version of this document had become stale and is superseded by this rewrite.
- Exact parity is still not guaranteed.

## Recently Fixed

The following items were previously listed as major parity blockers and are now implemented or aligned:

- `sts2_env/core/combat.py`
  - `summon_osty()`
  - `auto_play_from_draw()`
  - `generate_card_to_hand()`
  - `generate_ethereal_cards()`
  - owner-aware Osty lookup / summon / kill for allied player-creatures
- Combat potion support across the RL stack
  - fixed combat action-space layout for potions
  - simulator potion masking / decoding / execution
  - bridge-side potion masking / decoding / execution
- Simulator/bridge observation alignment for pile-summary composition features
  - the simulator now keeps the bridge-only-unavailable composition slots zeroed
- `sts2_env/gym_env/run_env.py`
  - step exceptions are logged instead of being silently converted into losses
- Explicit card-choice parity fixes
  - Silent: `Acrobatics`, `DaggerThrow`, `Prepared`, `HiddenDaggers`
  - Defect: `Scavenge`, `FlakCannon`
- Additional colorless parity fixes and tests
  - `Alchemize`
  - `BeatDown`
  - `HandOfGreed`
- Additional Defect / Silent parity tests
  - `Compact`
  - `WhiteNoise`
  - `TheHunt`
- Additional Regent / Necrobinder parity tests
  - `RefineBlade`
  - `SeekingEdge`
  - `TheSmith`
  - `SolarStrike`
  - `SpoilsOfBattle`
  - `WroughtInWar`
  - `KnockoutBlow`
  - `Charge`
  - `Guards`
  - `BigBang`
  - `HiddenCache`
  - `Resonance`
  - `SummonForth`
  - `Bulwark`
  - `Conqueror`
  - `Convergence`
  - `Glimmer`
  - `DecisionsDecisions`
  - `Quasar`
  - `HeirloomHammer`
  - `CrashLanding`
  - `BlackHole`
  - `Furnace`
  - `Orbit`
  - `PaleBlueDot`
  - `Bodyguard`
  - `Reanimate`
  - `Seance`
  - `Afterlife`
  - `GraveWarden`
  - `PullAggro`
  - `LegionOfBone`
  - `Spur`
  - `NecroMastery`
  - `Dirge`
  - `Eidolon`
  - `Protector`
  - `BoneShards`
  - `Rattle`
  - owner-aware `HighFive`
  - `DrainPower`
  - `SculptingStrike`
  - `EndOfDays`
  - `GlimpseBeyond`
  - `Severance`
  - `SoulStorm`
  - `TheScythe`
  - `TimesUp`
  - `BorrowedTime`
  - `CountdownCard`
  - `DanseMacabre`
  - `DeathMarch`

## Tests Covering Previously Listed Gaps

Recent targeted tests now cover several flows that older parity notes incorrectly described as still missing:

- `tests/test_combat_parity.py`
- `tests/test_card_choice_parity.py`
- `tests/test_silent_choice_parity.py`
- `tests/test_defect_choice_parity.py`
- `tests/test_regent_parity.py`
- `tests/test_necrobinder_parity.py`
- `tests/test_action_space_potions.py`
- `tests/test_bridge_state_adapter.py`
- targeted helper coverage in `tests/test_parity_helpers.py`

In particular, these areas now have direct automated coverage:

- Wish / draw-pile choice ordering
- Secret Weapon / Secret Technique draw-pile filtering
- Discovery / Purity / Dredge / Cleanse choice flows
- Nightmare snapshot behavior
- Osty summon helpers
- focused Regent card flows such as `Begone`, `PhotonCut`, `Largesse`, `ManifestAuthority`, `VoidForm`, `RefineBlade`, `SeekingEdge`, `TheSmith`, `SolarStrike`, `SpoilsOfBattle`, `WroughtInWar`, `KnockoutBlow`, `Charge`, `Guards`, `BigBang`, `HiddenCache`, `Resonance`, `SummonForth`, `Bulwark`, `Conqueror`, `Convergence`, `Glimmer`, `DecisionsDecisions`, `Quasar`, `HeirloomHammer`, `CrashLanding`, `BlackHole`, `Furnace`, `Orbit`, and `PaleBlueDot`
- focused Necrobinder card flows such as `CaptureSpirit`, `Sacrifice`, `Transfigure`, `Undeath`, `Bodyguard`, `Reanimate`, `Seance`, `Afterlife`, `GraveWarden`, `PullAggro`, `LegionOfBone`, `Spur`, `NecroMastery`, `Dirge`, `Eidolon`, `Protector`, `BoneShards`, `Rattle`, owner-aware `HighFive`, `DrainPower`, `SculptingStrike`, `EndOfDays`, `GlimpseBeyond`, `Severance`, `SoulStorm`, `TheScythe`, `TimesUp`, `BorrowedTime`, `CountdownCard`, `DanseMacabre`, and `DeathMarch`
- Entropic Brew and combat potion-slot filling
- Combat potion action decoding and bridge mask generation

## Current Confirmed Blockers

### 1. Exact parity still exceeds the current audited surface

The codebase is no longer blocked on the old “core helper missing” category, but broad exact-match claims still require more decompiled-backed tests across:

- colorless and event cards outside the targeted choice-flow subset
- full Regent and Necrobinder regression coverage
- relic interactions across combat, shop, rewards, and rest-site hooks
- broader bridge smoke testing against a live game

This is primarily a coverage gap, not proof of incorrect behavior, but it prevents claiming an exact match.

### 2. Implemented but not yet fully parity-audited cards

There are still implemented cards that need deeper decompiled-backed coverage, but the previously tracked `Compact`, `WhiteNoise`, and `TheHunt` items are now covered by dedicated parity tests.

### 3. Bridge/runtime validation gap

The Python-side bridge adapter and the C# combat handler now both understand potion actions, but this workspace has not compiled or smoke-tested the bridge mod against a live game session during this audit.

Until that happens, bridge potion support should be considered implemented but not fully field-verified.

### 4. Reachability / semantic audit backlog

Some no-op or deprecated bodies remain and need explicit audit before claiming strict exactness:

- deprecated Act 3 event stubs in `sts2_env/events/act3.py`
- intentionally inert status / curse `OnPlay` bodies in `sts2_env/cards/status.py`

Those may be correct, but they should be documented as “intentionally unreachable” or “semantically handled elsewhere” rather than left ambiguous.

## Standard for Claiming Exact Parity

We should not describe `sts2_env` as an exact match until all of the following are true:

1. The remaining confirmed blockers above are closed.
2. Every gameplay-affecting divergence is either implemented exactly or proven unreachable.
3. The bridge path is smoke-tested against a live game for the combat features we depend on.
4. The remaining no-op markers are limited to base-class defaults or explicitly documented unreachable content.

## Repository Pointers

- Combat core: `sts2_env/core/combat.py`
- Card implementations: `sts2_env/cards/`
- Potion implementations: `sts2_env/potions/`
- RL envs: `sts2_env/gym_env/`
- Bridge adapter: `sts2_env/bridge/state_adapter.py`
- Bridge mod: `bridge_mod/RlCombatHandler.cs`
- Decompiled reference: `decompiled/MegaCrit.Sts2.Core.Models.*`
