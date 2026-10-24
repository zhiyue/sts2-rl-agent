# Game Version Tracking

Which Slay the Spire 2 build each decompilation snapshot corresponds to.
The simulator's parity target is the version recorded here.

---

## Current snapshot: v0.107.1 (public branch)

| Field | Value |
|-------|-------|
| Game version | **v0.107.1** |
| Steam appid | 2868840 |
| Steam buildid | 23811903 |
| Build date | 2026-06-18 (release_info.json: `2026-06-18T15:43:56-07:00`) |
| release_info.json commit | `59260271` |
| `main_assembly_hash` | `-1555940892` |
| Decompiled on | 2026-10-24 |
| Decompiled to | `decompiled/` (git-tracked, canonical path) |
| ilspycmd version | 9.1.0.7988 (`C:\Users\zhiyue\.dotnet\tools\ilspycmd.exe`) |
| Decompiled file count | ~3,425 `.cs` files |
| PCK extracted to | `extracted_pck/` (gitignored, ~2.5 GB) |
| GDRE Tools | 2.4.0 (`C:\Users\zhiyue\tools\gdre\gdre_tools.exe`) |
| Extracted file count | ~27,101 files, 0 failed conversions |

### Provenance commands

```bash
# Confirm installed game version
cat "C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\release_info.json"

# Confirm Steam's latest buildid per branch (anonymous)
C:\Users\zhiyue\tools\steamcmd\steamcmd.exe +login anonymous +app_info_print 2868840 +quit

# Re-decompile
C:\Users\zhiyue\.dotnet\tools\ilspycmd.exe -p -o decompiled ^
  "C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll"

# Re-extract PCK
C:\Users\zhiyue\tools\gdre\gdre_tools.exe --headless ^
  --recover="C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck" ^
  --output-dir=extracted_pck
```

### Branch situation (checked 2026-10-24)

| Branch | buildid | timeupdated | Notes |
|--------|---------|-------------|-------|
| public | 23811903 | 2026-06-18 | **= installed version, latest public** |
| public-beta | 24724944 | 2026-08-14 | Newer, NOT used — decision: stay on public |

---

## Historical snapshots

| Directory | Date | Game version | Notes |
|-----------|------|--------------|-------|
| `decompiled_2026-03-15_backup/` | 2026-03-15 | not recorded at the time (pre-v0.107.1 era, ~3,299 files) | Superseded. Kept locally for diffing; also preserved in git history under `decompiled/`. |
| `extracted_pck_2026-03-15_backup/` | 2026-03-15 | same era | Superseded PCK extraction, kept locally. |

### Content delta vs March snapshot (v0.107.1 is NEWER)

New gameplay classes found by diffing the two decompilations:

- Cards: **NotYet, Scare, Wither**
- Relics: **FishingRod, HeftyTablet, Kaleidoscope, NeowsBones, NeowsTalisman, PhialHolster, SilkenTress, WingedBoots**
- Powers: **BorrowedTimePower, NoEnergyGainPower, TaintedPower, WitheringPresencePower**
- Monsters: **Aeonglass** (boss: AeonglassBoss)
- Encounters: **AeonglassBoss, SeapunkNormal**
- Systems: new Badges namespace (~29 classes), new Modding/Debug/Multiplayer surface

Modified-but-existing content: ~2,382 files differ, most heavily
`Models.Cards` (391), `Models.Relics` (174), `Models.Powers` (174),
`Models.Monsters` (111) — balance/API changes between 2026-03 and v0.107.1.

---

## Parity audit vs this snapshot (2026-10-24)

`python scripts/parity_reference_audit.py --show-missing`
(decompiled/ = v0.107.1, synced implementation at commit `1b7e7ce`):

| Surface | Total | Missing implementation | Missing tests |
|---------|-------|------------------------|---------------|
| cards | 577 | NotYet, Scare, Wither | same |
| encounters | 87 | AeonglassBoss, SeapunkNormal | same |
| events | 66 | none | none |
| modifiers | 16 | none | none |
| monsters | 120 | Aeonglass | same |
| potions | 63 | none | none |
| powers | 260 | BorrowedTimePower, NoEnergyGainPower, TaintedPower, WitheringPresencePower | NoEnergyGainPower, TaintedPower, WitheringPresencePower |
| relics | 297 | FishingRod, HeftyTablet, Kaleidoscope, NeowsBones, NeowsTalisman, PhialHolster, SilkenTress, WingedBoots | same |

Full test suite at audit time: **4609 passed, 1 skipped**.

---

## Layered parity verification status (2026-10-24, FINAL — all content layers green)

Verification is layered; each layer compares the Python simulator against the
decompiled v0.107.1 C# source as ground truth.

| Layer | Method | Status |
|-------|--------|--------|
| 0. Content coverage | `scripts/parity_reference_audit.py` | **ALL 8 SURFACES 0 MISSING** — cards 578/578, powers 260/260, relics 297/297 (8 new: FishingRod…WingedBoots), monsters 120/120 (Aeonglass implemented), encounters 87/87 (AeonglassBoss, SeapunkNormal), events/potions/modifiers 100% |
| 1. Static card metadata (cost/type/rarity/target/keywords) | `scripts/audit_card_static_metadata.py` | **PASSED** (fixed 29 stale cards + added 4 missing factories) |
| 2. Reference table | `docs/CARDS_REFERENCE.md` | **Regenerated from C#** via new `scripts/generate_cards_reference.py` (579 entries, was stale March data) |
| 3. Dynamic card vars (damage/block/upgrade numbers) | `scripts/audit_card_dynamic_vars.py` | **PASSED** — all 124 mismatches across 61 cards fixed against C# (incl. real mechanic changes: Begone now Skill/Transform→MinionStrike, Conflagration/Spite fixed-repeat, BorrowedTime 4 energy + 1 extra cost) |
| 4. Card pools order | `tests/test_card_pool_parity.py` | **PASSED** — all 12 pools reordered to match C# pool files exactly; GRAPPLE (unregistered in v0.107.1) and removed FOLLOW_THROUGH dropped from pools |
| 5. Monster HP/stats | `scripts/audit_monster_stats.py` | **PASSED (0 HP / 0 const)** — 53 HP + 27 const mismatches fixed; NOTE: parser originally missed `private int X =>` instance properties (v0.107.1's dominant style), masking 23 real errors; mapping extended by Aeonglass (68 mapped). The "50 unmapped" were audited manually — 8 test fakes, 11 already implemented & verified, rest are pets/noops |
| 6. New-content behavior parity | tests/test_v0_107_1_*.py | NotYet/Scare/Wither (13), 8 relics (31), 4 powers (31 incl. PowerId enum now landed: BORROWED_TIME/TAINTED/WITHERING_PRESENCE), Aeonglass+encounters (13) — all cite C# per assertion |
| 7. Behavior tests | full pytest suite | **4715 passed, 0 failed, 1 skipped** (run with `-p no:cacheprovider`; stale `__pycache__` on /mnt/c can cause ghost failures — `find . -name __pycache__ -not -path "./decompiled*" -exec rm -rf {} +` if seen) |
| 8. Third-source cross-check | extracted_pck/localization/eng/cards.json | 515/579 card titles exact-match; 64 are punctuation variants (Banshee's Cry etc.), spot-checked OK |
| 9. Per-card behavior review (OnPlay semantics) | 3 read-only review agents, 558 cards individually compared against decompiled C# | **DONE** — 25 MISMATCH + 1 HOOK_MISSING + 6 UNCERTAIN found, all fixed against C# with citation (Stoke generates random cards instead of drawing; Murder formula base+extra×drawn not product; Strangle applied wrong power CONSTRICT; EnergySurge/GlimpseBeyond self-inclusion; Soul upgraded-contagion (GraveWarden/Seance); Osty-missing early-return swallowing unconditional effects (SicEm/Snap); GuidingStar immediate draw; Charge transforms to MinionDiveBomb; Royalties gold amount; RocketPunch SetUntilPlayed cost; KnifeTrap full auto-play pipeline; Rattle per-attack-command counting; Reave random pile position; Anointed hand-space cap; NeowsFury player choice; MadScience free-this-turn; HowlFromBeyond replay; DrumOfBattle exhausted-energy hook; PactsEnd always playable; ExpectAFight NoEnergyGainPower) — 2 review leads adjudicated as wrong against C# (WhiteNoise dedup, HeirloomHammer Debris) and correctly NOT changed |
| 10. Engine semantics review (damage pipeline, hooks, phases) | read-only review agent, 15 semantic items vs C# Hook/DamageCmd/CombatManager/CreatureCmd | **DONE** — 5 MISMATCH fixed: PaperPhrog/PaperKrane fold into Vulnerable/Weak power multipliers BEFORE Debilitate doubling (Vuln+Debilitate+Phrog = 10×2.5, was 10×2.25; spot-verified against VulnerablePower.cs/DebilitatePower.cs `amount + (amount-1)`); block pipeline (enchant mult before listeners, BeforeBlockGained hook added, block cap 999→999999999); initial shuffle now goes through ModifyShuffleOrder hook incl. turn-1 Imbued-to-bottom (CombatManager.cs:659-673); Minion victory — win when primary enemies dead + cascade-kill secondaries (CombatManager.cs:192, CreatureCmd.cs:546-551), was unwinnable when only Minions remained. Hook dispatch structure (C# single global listener table) documented in hooks.py as A7-note — currently zero numeric impact, guard test added |
| 11. Run-layer review (map/rewards/shop/rest) | read-only review agent, 29 items vs C# StandardActMap/ActModel/RelicModel/Merchant*/PotionRewardOdds | **DONE** — 11 MISMATCH fixed: elite/rest row bound row<5→row<6; 3-round room-type assignment retry with C# StableShuffle semantics; full MapPathPruning.PruneAndRepair + SpreadAdjacentMapPoints + StraightenPaths ported (new map/path_pruning.py, map/post_processing.py; 120-combo seed test); removed self-invented "gloom" rule; encounter selection via per-act shuffled grab-bag with anti-repeat tags (ActModel.cs:349-416); escaped monsters reduce gold by goldProportion (EncounterModel.cs:373-376); shop relic base costs 150/250/300/150 → **175/225/275/200** (RelicModel.cs:304-312, STS1 values were stale); card-removal price A6+ tier 100+50 (MerchantCardRemovalEntry.cs); potion odds update uses elite-boosted threshold (PotionRewardOdds.cs:62-69); Hive rest/unknown room counts |
| 12. Gym interface review (observation/action space) | read-only review agent + direct env construction | **DONE** — observation missing **Stars**: added as player-state dim, OBS_SIZE 131→**132** (run obs 151→**152**); stale docs corrected (README/SIMULATOR_ARCHITECTURE/TROUBLESHOOTING/TRAINING_GUIDE): action space is Discrete(**115**) combat / Discrete(**157**) full-run, not "61/100". Adjudicated NO-CHANGE: proposed "pay excess energy with Stars" mask fallback — C# `ShouldPayExcessEnergyCostWithStars` defaults false with zero implementers in v0.107.1 (dead code), so Python's strict energy check is the correct parity behavior |

Final regression at layers 10-12 close: **4774+ passed, 0 failed**; all 5 audits green.

RL-environment design limitations — **ALL CLOSED** (2026-10-24, layer 13):

- ~~Observation intent encodes only the first intent~~ **CLOSED** — each enemy
  block now exposes `MAX_INTENTS_PER_MOVE = 3` intent slots (7→8 features
  each: one-hot over 6 types incl. Heal + damage/30 + hits/5). 3 is the
  decompiled maximum (333 MoveState constructions scanned: 4 moves carry 3
  intents — KnowledgeDemon PONDER, InfestedPrism, TheForgotten,
  WaterfallGiant STOMP; 76 carry 2). KnowledgeDemon PONDER was also missing
  its HealIntent in the sim move list (monsters/act2.py) — added. OBS_SIZE
  132 → **217**, RUN_OBS_SIZE 152 → **237**. Slot 0 keeps the legacy offset
  so the bridge encoder stays offset-compatible. IntentType gaps noted, not
  actioned: Python lacks C# `Hidden` (test-monsters only); Python's extra
  `MULTI_ATTACK` models C# `MultiAttackIntent : AttackIntent`.
- ~~Truncation marks but does not end episodes~~ **CLOSED** — both envs set
  an episode-ended flag on `terminated or truncated`; further `step()`
  raises `gymnasium.error.ClosedEnvironmentError`; `reset()` clears.
  combat_env truncation is mutually exclusive with termination (Gymnasium
  TimeLimit convention); run_env truncation applies `lose_run()` so the
  internal run state truly ends (Gymnasium signals stay separated).
- ~~WitheringPresence one shared counter in multiplayer~~ **CLOSED** —
  single power instance now keeps a per-opposing-player counter dict
  (power framework holds one instance per PowerId, so true per-instance
  powers are impossible without core changes; the dict is behaviorally
  equivalent to C#'s Instanced-per-opponent Target semantics,
  WitheringPresencePower.cs:26, Aeonglass.cs:74-84). `amount` stays at the
  applied 6 like C#'s Amount-vs-CardsLeft separation.
- Enemy slots fixed at 5 (MAX_ENEMIES) — kept: no C# encounter exceeds 5
  simultaneous enemies (encounters audit: 87/87 aligned).

Final regression at layer 13 close: **4805 passed, 0 failed**; all 5 audits
green.

### Known remaining gaps (documented, intentional)

- ~~Engine: card-exhausted hook for cards~~ **CLOSED** —
  `registry.register_after_card_exhausted_hook` +
  `hooks.fire_after_card_exhausted` now dispatch to card models (C#
  Hook.AfterCardExhausted → AllCards listener); DrumOfBattle wired
  (ironclad.py:959). Also landed: `AfterAutoPostPlayPhaseEntered` hook
  (HowlFromBeyond replay). Both from interrupted-agent work, reconciled and
  verified 2026-10-24.
- ~~WingedBoots free-travel not wired into `run_state.get_available_next_coords`~~
  **CLOSED** — `RunState.should_allow_free_travel()` (OR of flight modifiers +
  relic hooks, mirroring Hook.cs:2485-2492) consumed by
  `get_available_next_coords`; free travel = whole next row per
  MapTravel.cs:14-21; charge spent only on off-path moves
  (WingedBoots.cs:57-93). Tests cite C# lines.
- ~~Neow event pools don't offer the 8 new relics~~ **CLOSED** —
  `events/act3.py` pools now match Neow.cs PositiveOptions/CurseOptions
  (66-106): positive +FishingRod/Kaleidoscope/PhialHolster/WingedBoots, curse
  +HeftyTablet/NeowsBones/SilkenTress, NeowsTalisman via the
  talisman/pomander mutual-exclusion pair (Neow.cs:242-268). Also fixed en
  route: ScrollBoxes was in the wrong pool (belongs in positive,
  Neow.cs:80), HeftyTablet→remove-ArcaneScroll exclusion (Neow.cs:230-233),
  and the C# rng draw order for the three pairs.
- ~~Multiplayer: WitheringPresence uses one shared counter instead of one
  instance per opponent~~ **CLOSED** — per-opposing-player counter dict in
  the single power instance (see layer 13 above); behaviorally equivalent
  to C#'s Instanced-per-opponent semantics.
- ~~Tainted applier chain: sim still uses the March semantics for VitalSpark~~
  **CLOSED** — full C# chain implemented (VitalSparkPower.cs):
  BeforeCombatStart afflicts all Skills with stackable Tainted
  (CardCmd.Afflict semantics, cards/base.py carries affliction_amount);
  AfterCardEnteredCombat afflicts late entries; playing a Tainted-afflicted
  skill applies TaintedPower to the player's own creature; power removal
  clears the afflictions; amount changes resync. TaintedPower itself was
  already correct (self-removes at enemy side turn end). InfestedPrism
  constants corrected to 2/3 and Pulsate now applies VitalSpark (no
  Strength in C# PulsateMove). 15 parity tests cite C# lines.
- Working tree also carries the pre-pull local change to
  `bridge/agent_runner.py` (optional model_path → random agent), restored
  from stash during agent work; not superseded by remote, kept uncommitted.

Next work order: RL-environment scope verification (combat engine semantics,
run flow, observation/action-space), then the remaining gaps above.
