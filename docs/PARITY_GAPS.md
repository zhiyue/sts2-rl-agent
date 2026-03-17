# Parity Gaps to Exact Match

This document tracks the currently known blockers that prevent `sts2_env` from being described as fully identical to the decompiled game logic under `decompiled/`.

As of 2026-03-16, the correct status is:

- Core combat flow is close to the decompiled implementation.
- The currently covered subset is stable.
- Exact parity is not guaranteed.

## Evidence Collected

- Verified passing tests:
  - `tests/test_combat_parity.py`: 20 passed
  - `tests/test_run_flow.py tests/test_rewards.py tests/test_shop.py`: 110 passed
  - `tests/test_cards.py tests/test_powers.py tests/test_monster_ai.py tests/test_potions.py`: 92 passed
- Static scan of `sts2_env/` found 113 `stub` / `placeholder` / `not implemented` / `no-op` markers.
- High-risk uncovered domains still include Regent, Necrobinder, complex card-selection/generation effects, potion-slot logic, and helper methods in `core/combat.py`.
- Text search over `tests/` found no direct coverage for `Regent`, `Necrobinder`, `Osty`, `Venerate`, `Bodyguard`, `Wish`, `DualWield`, `Metamorphosis`, or `Transfigure`.

## Exact-Parity Standard

We should not claim exact parity until all of the following are true:

1. Every gameplay-affecting `stub` / `placeholder` / `pass` listed below is either implemented or proven unreachable.
2. The foundational helper methods in `sts2_env/core/combat.py` are implemented where the decompiled game has real behavior.
3. Every listed gap has at least one parity test tied to the relevant decompiled class.
4. The remaining `stub` markers are limited to:
   - base-class no-op hook defaults
   - deprecated content that is provably never reachable in normal gameplay
5. Regent, Necrobinder, colorless/event cards, and potion-slot behavior have direct test coverage.

## Blocking Gaps

### P0: Foundational Helpers

These are root-cause gaps. As long as these methods are incomplete, multiple cards, powers, relics, and potions cannot be exact.

| Python location | Current status | Affected decompiled classes |
|---|---|---|
| `sts2_env/core/combat.py::summon_osty()` | Unimplemented | `MegaCrit.Sts2.Core.Models.Cards/Bodyguard.cs`, `Afterlife.cs`, `Reanimate.cs`; `MegaCrit.Sts2.Core.Models.Powers/SummonNextTurnPower.cs` |
| `sts2_env/core/combat.py::auto_play_from_draw()` | Unimplemented | `MegaCrit.Sts2.Core.Models.Powers/MayhemPower.cs`; `MegaCrit.Sts2.Core.Models.Cards/Catastrophe.cs`, `BeatDown.cs`, `Uproar.cs` |
| `sts2_env/core/combat.py::generate_card_to_hand()` | Unimplemented | `MegaCrit.Sts2.Core.Models.Powers/CreativeAiPower.cs`, `HelloWorldPower.cs`; `MegaCrit.Sts2.Core.Models.Cards/NeowsFury.cs`, `JackOfAllTrades.cs`, `WhiteNoise.cs` |
| `sts2_env/core/combat.py::generate_ethereal_cards()` | Unimplemented | `MegaCrit.Sts2.Core.Models.Relics/BigHat.cs` |
| `sts2_env/cards/effects.py::summon_osty()` | Placeholder wrapper | Same gap surface as `summon_osty()` above |
| `sts2_env/cards/effects.py::channel_orb()` | Marked placeholder, partial behavior only | Defect orb-related logic depends on exact passive/evoke ordering in decompiled combat flow |

### P0: Regent System Gaps

`sts2_env/cards/regent.py` contains the highest concentration of gameplay-affecting placeholders in the repository.

| Python location | Current status | Decompiled class |
|---|---|---|
| `VENERATE` | `pass`, Stars not granted | `MegaCrit.Sts2.Core.Models.Cards/Venerate.cs` |
| `BEGONE` | transform/create follow-up card stub | `MegaCrit.Sts2.Core.Models.Cards/Begone.cs` |
| `PHOTON_CUT` | put-back-on-draw-pile stub | `MegaCrit.Sts2.Core.Models.Cards/PhotonCut.cs` |
| `REFINE_BLADE` | forge/upgrade stub | `MegaCrit.Sts2.Core.Models.Cards/RefineBlade.cs` |
| `SOLAR_STRIKE` | Stars gain stub | `MegaCrit.Sts2.Core.Models.Cards/SolarStrike.cs` |
| `SPOILS_OF_BATTLE` | forge/upgrade stub | `MegaCrit.Sts2.Core.Models.Cards/SpoilsOfBattle.cs` |
| `WROUGHT_IN_WAR` | forge stub | `MegaCrit.Sts2.Core.Models.Cards/WroughtInWar.cs` |
| `KNOCKOUT_BLOW` | Stars gain stub | `MegaCrit.Sts2.Core.Models.Cards/KnockoutBlow.cs` |
| `LARGESSE` | generate upgraded cards stub | `MegaCrit.Sts2.Core.Models.Cards/Largesse.cs` |
| `MANIFEST_AUTHORITY` | generate upgraded cards stub | `MegaCrit.Sts2.Core.Models.Cards/ManifestAuthority.cs` |
| `SEEKING_EDGE` | immediate forge stub | `MegaCrit.Sts2.Core.Models.Cards/SeekingEdge.cs` |
| `THE_SMITH` | forge stub | `MegaCrit.Sts2.Core.Models.Cards/TheSmith.cs` |
| `VOID_FORM` | end-turn rider stub | `MegaCrit.Sts2.Core.Models.Cards/VoidForm.cs` |

Notes:

- Many Regent gaps are not cosmetic. They alter Stars, Forge, card generation, card movement, or end-turn sequencing.
- Until these are implemented, Regent cannot be considered parity-complete.

### P0: Necrobinder and Osty Gaps

These are gameplay-critical because the character revolves around Osty, souls, summons, and card-state mutation.

| Python location | Current status | Decompiled class |
|---|---|---|
| `BODYGUARD` | summon stub | `MegaCrit.Sts2.Core.Models.Cards/Bodyguard.cs` |
| `AFTERLIFE` | summon stub | `MegaCrit.Sts2.Core.Models.Cards/Afterlife.cs` |
| `GRAVE_WARDEN` | create Soul cards stub | `MegaCrit.Sts2.Core.Models.Cards/GraveWarden.cs` |
| `GRAVEBLAST` | exhaust-pile selection stub | `MegaCrit.Sts2.Core.Models.Cards/Graveblast.cs` |
| `CAPTURE_SPIRIT` | non-attack damage plus Soul generation stub | `MegaCrit.Sts2.Core.Models.Cards/CaptureSpirit.cs` |
| `CLEANSE` | exhaust/select/summon stub | `MegaCrit.Sts2.Core.Models.Cards/Cleanse.cs` |
| `EIDOLON` | exhaust-from-hand selection stub | `MegaCrit.Sts2.Core.Models.Cards/Eidolon.cs` |
| `END_OF_DAYS` | kill-minion rider stub | `MegaCrit.Sts2.Core.Models.Cards/EndOfDays.cs` |
| `REANIMATE` | powerful summon stub | `MegaCrit.Sts2.Core.Models.Cards/Reanimate.cs` |
| `SACRIFICE` | kill-minion rider stub | `MegaCrit.Sts2.Core.Models.Cards/Sacrifice.cs` |
| `SEANCE` | transform/select stub | `MegaCrit.Sts2.Core.Models.Cards/Seance.cs` |
| `TRANSFIGURE` | Replay grant stub | `MegaCrit.Sts2.Core.Models.Cards/Transfigure.cs` |
| `UNDEATH` | generated-card-to-discard stub | `MegaCrit.Sts2.Core.Models.Cards/Undeath.cs` |

Notes:

- This file alone still contains 23 explicit placeholder markers.
- Many of these depend on the unimplemented `summon_osty()` helper, so parity work must start from the combat core.

### P1: Card Selection, Copying, and Generation

These are common enough that they materially affect deck quality and combat outcomes.

| Python location | Current status | Decompiled class |
|---|---|---|
| `sts2_env/cards/status.py::WISH` | draw-pile grid selection stub | `MegaCrit.Sts2.Core.Models.Cards/Wish.cs` |
| `sts2_env/cards/status.py::DUAL_WIELD` | copy selected card stub | `MegaCrit.Sts2.Core.Models.Cards/DualWield.cs` |
| `sts2_env/cards/status.py::METAMORPHOSIS` | generate free attacks to draw pile stub | `MegaCrit.Sts2.Core.Models.Cards/Metamorphosis.cs` |
| `sts2_env/cards/status.py::NEOWS_FURY` | add-card-to-hand rider stub | `MegaCrit.Sts2.Core.Models.Cards/NeowsFury.cs` |
| `sts2_env/cards/status.py::WHISTLE` | stun rider stub | `MegaCrit.Sts2.Core.Models.Cards/Whistle.cs` |
| `sts2_env/cards/status.py::DISTRACTION` | cost-0 generation stub | `MegaCrit.Sts2.Core.Models.Cards/Distraction.cs` |

### P1: Colorless and Event Cards

This area contains many selection/generation effects that are currently simplified away.

| Python location | Current status | Decompiled class |
|---|---|---|
| `CATASTROPHE` | auto-play stub | `MegaCrit.Sts2.Core.Models.Cards/Catastrophe.cs` |
| `DISCOVERY` | choose-and-add-cost-0 stub | `MegaCrit.Sts2.Core.Models.Cards/Discovery.cs` |
| `JACK_OF_ALL_TRADES` | generated colorless card stub | `MegaCrit.Sts2.Core.Models.Cards/JackOfAllTrades.cs` |
| `PURITY` | exhaust selected cards stub | `MegaCrit.Sts2.Core.Models.Cards/Purity.cs` |
| `SEEKER_STRIKE` | choose-from-draw stub | `MegaCrit.Sts2.Core.Models.Cards/SeekerStrike.cs` |
| `SPLASH` | upgrade/select/add-to-hand stub | `MegaCrit.Sts2.Core.Models.Cards/Splash.cs` |
| `ALCHEMIZE` | generate random potion stub | `MegaCrit.Sts2.Core.Models.Cards/Alchemize.cs` |
| `ANOINTED` | add card from exhaust pile stub | `MegaCrit.Sts2.Core.Models.Cards/Anointed.cs` |
| `BEAT_DOWN` | auto-play stub | `MegaCrit.Sts2.Core.Models.Cards/BeatDown.cs` |
| `HAND_OF_GREED` | gold-on-kill rider stub | `MegaCrit.Sts2.Core.Models.Cards/HandOfGreed.cs` |
| `HIDDEN_GEM` | Replay grant stub | `MegaCrit.Sts2.Core.Models.Cards/HiddenGem.cs` |
| `JACKPOT` | upgraded card generation stub | `MegaCrit.Sts2.Core.Models.Cards/Jackpot.cs` |
| `SECRET_TECHNIQUE` | select Skill from draw stub | `MegaCrit.Sts2.Core.Models.Cards/SecretTechnique.cs` |
| `SECRET_WEAPON` | select Attack from draw stub | `MegaCrit.Sts2.Core.Models.Cards/SecretWeapon.cs` |

### P1: Silent and Defect Simplifications

These are not always full no-ops, but they still diverge from the decompiled selection and sequencing behavior.

| Python location | Current status | Decompiled class |
|---|---|---|
| `sts2_env/cards/silent.py::SURVIVOR` | discards last card instead of player-selected card | `MegaCrit.Sts2.Core.Models.Cards/Survivor.cs` |
| `sts2_env/cards/silent.py::HAND_TRICK` | Sly application stub | `MegaCrit.Sts2.Core.Models.Cards/HandTrick.cs` |
| `sts2_env/cards/silent.py::NIGHTMARE` | approximated with generic power | `MegaCrit.Sts2.Core.Models.Cards/Nightmare.cs` |
| `sts2_env/cards/silent.py::THE_HUNT` | extra reward rider stub | `MegaCrit.Sts2.Core.Models.Cards/TheHunt.cs` |
| `sts2_env/cards/defect.py::HOLOGRAM` | takes top discard instead of selected discard | `MegaCrit.Sts2.Core.Models.Cards/Hologram.cs` |
| `sts2_env/cards/defect.py::UPROAR` | auto-play top draw approximated as discard move | `MegaCrit.Sts2.Core.Models.Cards/Uproar.cs` |
| `sts2_env/cards/defect.py::COMPACT` | transform/create rider stub | `MegaCrit.Sts2.Core.Models.Cards/Compact.cs` |
| `sts2_env/cards/defect.py::WHITE_NOISE` | random Power generation stub | `MegaCrit.Sts2.Core.Models.Cards/WhiteNoise.cs` |

### P1: Potion-Slot and Procure Logic

| Python location | Current status | Decompiled class |
|---|---|---|
| `sts2_env/potions/effects.py::_entropic_brew()` | no-op placeholder because potion slots are not fully modeled | `MegaCrit.Sts2.Core.Models.Potions/EntropicBrew.cs` |

### P2: Reachability and Content-Completeness Notes

These do not by themselves prove gameplay drift, but they show the codebase is not yet in an “every gameplay class audited” state.

| Python location | Current status | Decompiled counterpart |
|---|---|---|
| `sts2_env/monsters/act3.py` `ConstructMenagerie` comment block | encounter placeholder note | Related decompiled encounter configuration must still be verified |
| `sts2_env/events/act3.py::DeprecatedEvent` | explicit deprecated stub, intentionally unreachable | Deprecated content; acceptable only if proven unreachable |
| `sts2_env/events/act3.py::DeprecatedAncientEvent` | explicit deprecated stub, intentionally unreachable | Deprecated content; acceptable only if proven unreachable |

## Test Coverage Gaps

The current tests are useful, but they do not justify exact-parity claims in the highest-risk domains.

High-priority missing coverage areas:

- Regent card effects and Stars / Forge interactions
- Necrobinder and Osty summon flow
- Card selection from draw, discard, and exhaust piles
- Replay-granting effects
- Free-cost generated cards
- Potion-slot filling and potion procurement
- Silent and Defect cards currently implemented with simplified selection heuristics

## Recommended Order to Reach Exact Match

1. Implement the four foundational helper gaps in `sts2_env/core/combat.py`.
2. Finish Regent and Necrobinder, because they currently contain the largest concentration of gameplay-affecting stubs.
3. Implement card-selection and card-generation logic for `Wish`, `DualWield`, `Metamorphosis`, `Transfigure`, and similar effects.
4. Complete potion-slot state and `EntropicBrew`.
5. Add decompiled-backed parity tests for every entry in this document.
6. Re-run the static scan and only claim exact parity when gameplay-affecting stub markers are gone.

## Repository Pointers

- Combat core: `sts2_env/core/combat.py`
- Card implementations: `sts2_env/cards/`
- Power implementations: `sts2_env/powers/`
- Relics: `sts2_env/relics/`
- Potions: `sts2_env/potions/`
- Decompiled reference: `decompiled/MegaCrit.Sts2.Core.Models.*`
