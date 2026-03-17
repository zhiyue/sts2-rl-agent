# Known Issues and Limitations

Current known issues, bugs, and limitations of the STS2 RL Agent project.

---

## Fixed Issues

### 1. Energy always displayed as 3 with CardCmd.AutoPlay

**Status:** Fixed

**Problem:** The C# bridge mod initially used `CardCmd.AutoPlay()` to execute card plays. This method bypasses the normal energy deduction, so the player's energy always stayed at 3 (max) regardless of cards played. The agent could play unlimited cards per turn.

**Fix:** Switched to `PlayCardAction` which properly spends energy:
```csharp
var playAction = new PlayCardAction(card, target);
RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(playAction);
```

**Location:** `bridge_mod/RlCombatHandler.cs` line 187-188

### 2. EchoForm / modify_card_play_count was missing

**Status:** Fixed

**Problem:** The hook for modifying how many times a card is played was not implemented. Powers like EchoForm (play each card twice) had no effect.

**Fix:** Added `modify_card_play_count` to `core/hooks.py` and wired it into `CombatState.play_card()`.

**Location:** `sts2_env/core/hooks.py` lines 189-200, `sts2_env/core/combat.py` line 255

### 3. Enemy round-1 block not cleared

**Status:** Fixed

**Problem:** Enemies that gained block before their first turn (from combat-start effects) were not having their block cleared at the start of the enemy turn on round 1.

**Fix:** The enemy turn now always clears block for each alive enemy, regardless of round number.

**Location:** `sts2_env/core/combat.py` `_execute_enemy_turn()`

### 4. State adapter and action mask protocol mismatches

**Status:** Fixed

**Problem:** The Python `StateAdapter` was expecting different field names and formats than what the C# mod was actually sending. For example, target type strings like `"AnyEnemy"` vs `"ANY_ENEMY"`, and power list format differences.

**Fix:** Updated `state_adapter.py` to handle both formats:
```python
_UNTARGETED_TYPES = {TargetTypeName.SELF, TargetTypeName.NONE, TargetTypeName.ALL_ENEMIES,
                     "SELF", "NONE", "ALL_ENEMIES", "Self", "None", "AllEnemies"}
```

**Location:** `sts2_env/bridge/state_adapter.py` lines 69-71

---

## Open Issues

### 5. AnimationSpeedPatch fails to apply

**Severity:** Low (affects real-game speed only)

**Problem:** The Harmony patch targeting `MegaAnimationState.SetTimeScale` fails on some game versions because the method signature changed between updates. The patch is skipped with a log message.

**Impact:** The game runs at normal animation speed instead of 5x. The `WaitSpeedPatch` (which reduces timed delays by 10x) still applies successfully, providing some speedup.

**Workaround:** None currently. The animation patch needs to be updated when the game's `MegaAnimationState` API changes.

**Location:** `bridge_mod/MainFile.cs` `AnimationSpeedPatch` class

### 6. Mod abandon-run popup path may not match all versions

**Severity:** Low

**Problem:** The Godot scene tree paths used to find the abandon-run confirmation popup (`VerticalPopup/YesButton`) may not match all game versions. If the path is wrong, the mod cannot automatically abandon an existing run before starting a new one.

**Impact:** If there is already a run in progress when the mod starts, it may fail to abandon it cleanly.

**Workaround:** Manually abandon the run from the main menu before starting the agent.

**Location:** `bridge_mod/RlAutoSlayer.cs` `PlayMainMenuAsync()` lines 455-472

### 7. Full-run training needs significantly more steps and better reward shaping

**Severity:** High (fundamental training challenge)

**Problem:** The full-run environment produces 0% win rate even after 1M training steps. The agent learns to progress further through Act 1 (avg 8.9 floors vs 3.9 for random) but cannot complete a run.

**Root causes:**
- Sparse reward: only +1 at run victory, -1 at death. No intermediate signal.
- Long episodes: a full run spans thousands of steps.
- Multi-phase action space: Discrete(100) covering 8 different game phases.
- Compounding decisions: bad deck choices early doom later combats.

**Mitigation:** Reward shaping is available (`--reward-shaping` flag) but only provides small floor-progression bonuses. A fundamental redesign of the reward function or training approach (hierarchical RL, curriculum learning) is needed.

### 8. Only Ironclad combat model trained

**Severity:** Medium

**Problem:** The combat training pipeline only creates Ironclad starter decks. All training and evaluation use the Ironclad character.

**Impact:** The trained model is specific to Ironclad. It cannot play Silent, Defect, Necrobinder, or Regent effectively because:
- Different starter decks and starting HP
- Character-specific mechanics (orbs, stars, pets)
- Different card pools with different effect distributions

**Workaround:** The simulator supports all 5 characters (cards, powers, monsters are all implemented). Training scripts need to be extended to support character selection.

### 9. Potion actions not in combat action space

**Severity:** Medium

**Problem:** The combat action space `Discrete(61)` only covers card plays and end turn. There are no actions for using potions during combat.

**Impact:** The agent cannot use potions strategically. In the bridge mod, potions are not offered as an option.

**Fix needed:** Extend the action space to include potion usage: `play_potion(slot, target_index)`. This would increase the action space by `max_potion_slots * (1 + max_enemies)`.

### 10. Some card effects may not match the real game exactly

**Severity:** Medium (simulator fidelity)

**Problem:** The headless simulator reimplements card effects based on the decompiled C# source, but some effects are approximated:
- `generate_card_to_hand()`, `auto_play_from_draw()`, and `generate_ethereal_cards()` are stub implementations that do nothing.
- Osty (Necrobinder pet) mechanics are not implemented.
- Some complex card interactions (particularly involving the play pile and card movement during effects) may differ from the real game.

**Impact:** The trained model may develop strategies that exploit simulator inaccuracies and fail to transfer to the real game. The bridge mod's real-game evaluation is the ground truth.

### 11. Reconnection timing issues

**Severity:** Low

**Problem:** If the Python agent connects before the game has finished loading and the AutoSlayer has started, there can be a race condition where the first state message arrives before the agent is ready.

**Workaround:** Start the game first, wait for the main menu to appear, then start the Python agent. The agent runner has reconnection retry logic (`_reconnect_with_retry` with 10 attempts, 3s delay).

**Location:** `sts2_env/bridge/agent_runner.py` lines 288-309

### 12. `inspect.signature` on hot path

**Severity:** Low (performance)

**Problem:** In `hooks.py` `fire_after_card_drawn`, `inspect.signature(method).parameters` is called for every card draw to determine the parameter count of each power's `on_card_drawn` method. This is slow.

**Impact:** Adds unnecessary overhead to every card draw. With many powers active, this can be measurable.

**Fix needed:** Standardize all `on_card_drawn` implementations to accept 4 parameters `(owner, card, from_hand_draw, combat)` and remove the signature inspection.

**Location:** `sts2_env/core/hooks.py` lines 295-305

### 13. run_env swallows simulation exceptions

**Severity:** Medium (debugging difficulty)

**Problem:** `STS2RunEnv.step()` wraps all RunManager actions in a bare `except Exception` that force-ends the run as a loss. This silently hides simulation bugs.

```python
try:
    if phase == RunManager.PHASE_COMBAT:
        self._step_combat(action)
    # ...
except Exception:
    if not self._mgr.is_over:
        self._mgr.run_state.lose_run()
```

**Impact:** Bugs in the run manager, encounter setup, or card effects are converted into silent losses rather than visible errors. This makes debugging difficult.

**Fix needed:** Add logging before swallowing the exception:
```python
except Exception as e:
    logger.error("RunEnv step failed: %s", e, exc_info=True)
    if not self._mgr.is_over:
        self._mgr.run_state.lose_run()
```

**Location:** `sts2_env/gym_env/run_env.py` lines 238-242

### 14. Pile summary distribution shift between simulator and bridge

**Severity:** Low

**Problem:** The observation vector includes 3 pile-composition features (draw_attacks, draw_skills, discard_attacks) that are computed from full pile contents in the simulator but are always 0 in bridge mode (the game only sends pile counts, not composition).

**Impact:** The trained model learns to use these features during training, but they are absent during real-game evaluation. This creates a distribution shift that may degrade bridge performance.

**Fix options:**
- Extend the C# serializer to send pile composition counts.
- Remove these features from the observation space entirely (reduces obs from 131 to 128 dims).
- Zero them out during training as well (easiest but wastes 3 dims).
