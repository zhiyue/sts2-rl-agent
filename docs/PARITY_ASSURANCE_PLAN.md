# Parity Assurance Plan

How we keep the Python simulator logically identical to the game's C#
implementation, and how we notice the moment it drifts. Written 2026-10-24
after the v0.107.1 re-sync; every mechanism below traces to a failure we
actually hit this session.

## 0. Definition: "logically identical" is five verifiable levels

| Level | Meaning | Verified by |
|-------|---------|-------------|
| L1 Content | every game entity exists (cards/powers/relics/monsters/events) | `parity_reference_audit.py` |
| L2 Values | cost/damage/block/upgrade deltas equal per field | `audit_card_static_metadata.py`, `audit_card_dynamic_vars.py`, `audit_monster_stats.py` |
| L3 Mechanics | per-entity behavior semantics (hooks, timing, targeting) | parity tests, one per entity state, each assertion citing the C# file |
| L4 Engine | damage two-pass pipeline, hook dispatch order, debuff tick, phase sequence, RNG streams | engine semantic probe tests |
| L5 Trajectory | **same seed + same action sequence -> identical state trace** | live-game replay diffing via the bridge (the only proof that subsumes all others) |

## 1. Defense lines (continuous, not one-off)

### D1 Audit gates + audit self-checks
The four audit scripts gate every change. Their parsers are themselves
suspect: the monster audit missed the `private int X =>` style (v0.107.1's
dominant form) and silently masked 23 real errors.
- **Golden-sample tests** (hardcoded expectations hand-derived from C#, never
  copied from parser output) fail whenever a parser regex goes blind.
- **Mapping-completeness assertion**: "unmapped" counts must be an explicit
  reviewed list, never a silent skip.

### D2 Generated artifacts (shrink the hand-written surface)
Anything mechanically derivable from C# is generated, never hand-written:
`docs/CARDS_REFERENCE.md` today; next the card-pool order tables, event-pool
membership, encounter tag tables, relic pools, enum alignment tables. Each
generator is a one-way pipeline C# -> data file; after a game update the
regenerate-diff IS the change list.

### D3 Behavior parity test matrix
Every card x {base, upgraded}, monster x {base, A8}, relic x hook, event x
option has at least one assertion, and **every assertion names its C# file
and behavior**. Engine probes pin the phase order, two-pass damage,
round-1 block exemption, and debuff tick rules.

### D4 Live-game replay diffing (L5, the decisive evidence)
`bridge_mod` (AutoSlay + TCP JSON) plays a seeded run; every decision point
`(state, action, next_state)` is recorded; the Python replay harness drives
the same action sequence and diffs every state field (hp/block/piles/power
stacks/gold). Order of rollout: single combat -> full run. Mandatory after
every game update and after every engine-layer change. This is the only
mechanism that exposes RNG-stream and shuffle-order drift end to end.

### D5 Game-update pipeline
```
steamcmd: detect new build -> ilspycmd re-decompile + GDRE re-extract
-> GAME_VERSIONS.md update -> rerun 4 audits + regenerate all artifacts
-> diff = change list -> fix each item citing C# -> D4 replay diff
-> all green -> layered commits
```
Walked manually twice (March -> v0.107.1); script it.

### D6 Governance
- The decompiled C# is the only ground truth. Audits, docs, and old tests
  are all suspects until verified against it.
- Fixer and verifier are different agents/people; the verifier re-derives
  from C# independently.
- One writer session per repo at a time; multi-session work partitions by
  file ownership (two collisions this session).
- CI: pytest + 4 audits on every push; red blocks merge.

## 2. Rollout order

1. **Parser golden samples** (this commit) — close the known trust hole.
2. Bridge single-combat replay diff — makes L5 provable.
3. Data-table generation (pools/events/encounter tags first).
4. Update-pipeline scripting + CI.
5. Replay diff extended to full runs.

## 3. Known residual gaps (tracked in GAME_VERSIONS.md)

Neow event pool (8 new relics + 3 option misplacements), per-act Ancient
opening nodes, observation intent encoding (first intent only), episode
truncation vs termination semantics, fixed 5 enemy slots, dead
`generate_act_room_set` in encounters/room_set.py, A10 second-boss map row,
Underdocks act-0 gating.
