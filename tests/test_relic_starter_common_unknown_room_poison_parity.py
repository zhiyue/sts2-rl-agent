"""Parity tests for starter/common relic unknown-room and poison hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.silent import create_silent_starter_deck, make_deadly_poison
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import MapPointType, PowerId, RoomType
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import RunState, UNLOCK_STATE_NUMBER_OF_RUNS_KEY


def _make_silent_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1700,
) -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=create_silent_starter_deck(),
        rng_seed=seed,
        character_id="Silent",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicStarterCommonUnknownRoomPoisonParity:
    def test_juzu_bracelet_removes_monsters_from_unknown_room_rolls(self):
        """Matches JuzuBracelet.cs: unknown rooms can no longer resolve to Monster."""
        baseline = RunState(seed=1701, character_id="Ironclad")
        baseline.player.unlock_state[UNLOCK_STATE_NUMBER_OF_RUNS_KEY] = 1
        baseline.unknown_odds._current = {
            RoomType.MONSTER: 1.0,
            RoomType.ELITE: -1.0,
            RoomType.TREASURE: 0.0,
            RoomType.SHOP: 0.0,
        }
        assert baseline.resolve_room_type(MapPointType.UNKNOWN) == RoomType.MONSTER

        run_state = RunState(seed=1702, character_id="Ironclad")
        run_state.player.unlock_state[UNLOCK_STATE_NUMBER_OF_RUNS_KEY] = 1
        assert run_state.player.obtain_relic("JUZU_BRACELET")
        run_state.unknown_odds._current = {
            RoomType.MONSTER: 1.0,
            RoomType.ELITE: -1.0,
            RoomType.TREASURE: 0.0,
            RoomType.SHOP: 0.0,
        }

        assert run_state.resolve_room_type(MapPointType.UNKNOWN) == RoomType.EVENT

    def test_snecko_skull_adds_one_extra_poison_to_owned_applications(self):
        """Matches SneckoSkull.cs:19-29 ModifyPowerAmountGivenAdditive: any
        owner-applied Poison gains +1 — there is no amount > 0 guard."""
        combat = _make_silent_combat(["SneckoSkull"], seed=1703)
        enemy = combat.enemies[0]
        deadly_poison = make_deadly_poison()
        deadly_poison.owner = combat.player
        combat.hand = [deadly_poison]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.POISON) == 6

    def test_snecko_skull_applies_even_to_zero_amount_poison(self):
        """C# SneckoSkull.cs has no `amount > 0` guard: a 0-amount owner Poison
        application still gains +1."""
        combat = _make_silent_combat(["SneckoSkull"], seed=1713)
        relic = next(r for r in combat.relics if r.relic_id.name == "SNECKO_SKULL")
        enemy = combat.enemies[0]

        modified = relic.modify_power_amount_given(
            combat.player, PowerId.POISON, 0, combat.player, enemy, None, combat,
        )
        assert modified == 1
