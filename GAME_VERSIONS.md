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

RL-environment scope (verification planned, not yet started): combat engine
semantics (damage pipeline order, hook order, block-clear timing, debuff
decay), run flow (map/events/shop/rest/rewards), observation (131-dim) and
action space (61/100) semantics vs the real game.

### Known remaining gaps (documented, intentional)

- ~~Engine: card-exhausted hook for cards~~ **CLOSED** —
  `registry.register_after_card_exhausted_hook` +
  `hooks.fire_after_card_exhausted` now dispatch to card models (C#
  Hook.AfterCardExhausted → AllCards listener); DrumOfBattle wired
  (ironclad.py:959). Also landed: `AfterAutoPostPlayPhaseEntered` hook
  (HowlFromBeyond replay). Both from interrupted-agent work, reconciled and
  verified 2026-10-24.
- WingedBoots free-travel not wired into
  `run_state.get_available_next_coords` (hook implemented, map layer pending).
- Neow event pools in `sts2_env/events/act3.py` don't offer the 8 new
  relics yet (relic behaviors themselves complete and tested).
- Multiplayer: WitheringPresence uses one shared counter instead of one
  instance per opponent (single-player behavior identical).
- Tainted applier chain: C# reworked VitalSpark into the affliction system
  (CardCmd.Afflict); sim still uses the March semantics for VitalSpark.
  TaintedPower itself implemented and tested.
- Working tree also carries the pre-pull local change to
  `bridge/agent_runner.py` (optional model_path → random agent), restored
  from stash during agent work; not superseded by remote, kept uncommitted.

Next work order: RL-environment scope verification (combat engine semantics,
run flow, observation/action-space), then the remaining gaps above.
