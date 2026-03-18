"""Seventh batch of uncommon relic parity tests for remaining uncovered relic hooks."""

from types import SimpleNamespace

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CombatSide, MapPointType, PowerId, RoomType
from sts2_env.core.hooks import fire_after_turn_end
from sts2_env.core.rng import Rng
from sts2_env.map.generator import ActMap
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.run.rooms import RoomVisitContext
from sts2_env.run.run_state import RunState


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1201,
    enemies: int = 1,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    for i in range(enemies):
        if i == 0:
            creature, ai = create_shrinker_beetle(Rng(seed + i))
        else:
            creature, ai = create_twig_slime_s(Rng(seed + i))
        combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _combat_relic(combat: CombatState, relic_name: str):
    return next(relic for relic in combat.relics if relic.relic_id.name == relic_name)


def _run_relic(run_state: RunState, relic_name: str):
    return next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == relic_name)


class TestRelicParityUncommonExtra7:
    def test_book_repair_knife_heals_per_qualifying_doom_death_only(self):
        """Matches BookRepairKnife.cs: heal 3 for each non-owner doomed death that still counts as fatal."""
        combat = _make_ironclad_combat(["BookRepairKnife"], seed=1201, enemies=2)
        relic = _combat_relic(combat, "BOOK_REPAIR_KNIFE")
        player = combat.player
        enemy_fatal, enemy_minion = combat.enemies
        player.current_hp = 40

        enemy_fatal.current_hp = 0
        enemy_minion.current_hp = 0
        enemy_minion.apply_power(PowerId.MINION, 1)

        relic.after_died_to_doom(player, [player, enemy_fatal, enemy_minion], combat)
        assert player.current_hp == 43

        relic.after_died_to_doom(player, [player, enemy_minion], combat)
        assert player.current_hp == 43

    def test_planisphere_heals_when_last_map_point_was_unknown(self):
        """Matches Planisphere.cs: heal when entering from an Unknown map node."""
        run_state = RunState(seed=1202, character_id="Ironclad")
        run_state.player.current_hp = 35
        assert run_state.player.obtain_relic("PLANISPHERE")
        relic = _run_relic(run_state, "PLANISPHERE")

        run_state.map = ActMap(num_rooms=2)
        unknown = run_state.map.get_or_create(0, 1)
        unknown.point_type = MapPointType.UNKNOWN
        run_state.visited_map_coords.clear()
        run_state.add_visited_coord(unknown.coord)

        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.MONSTER))
        assert run_state.player.current_hp == 39

    def test_planisphere_does_not_heal_on_non_unknown_or_when_owner_dead(self):
        """Planisphere should not heal for known nodes and should never revive a dead owner."""
        run_state = RunState(seed=1203, character_id="Ironclad")
        run_state.player.current_hp = 35
        assert run_state.player.obtain_relic("PLANISPHERE")
        relic = _run_relic(run_state, "PLANISPHERE")

        run_state.map = ActMap(num_rooms=2)
        point = run_state.map.get_or_create(0, 1)
        point.point_type = MapPointType.MONSTER
        run_state.visited_map_coords.clear()
        run_state.add_visited_coord(point.coord)

        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.MONSTER))
        assert run_state.player.current_hp == 35

        run_state.player.current_hp = 0
        point.point_type = MapPointType.UNKNOWN
        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.MONSTER))
        assert run_state.player.current_hp == 0

    def test_regalite_gains_block_only_for_owner_colorless_card_entering_combat(self):
        """Matches Regalite.cs: owner gains 2 block when a colorless card enters combat."""
        combat = _make_ironclad_combat(["Regalite"], seed=1204)
        relic = _combat_relic(combat, "REGALITE")
        player = combat.player
        player.block = 0

        colorless = create_card(CardId.VOLLEY)
        colorless.owner = player
        relic.after_card_entered_combat(player, colorless, combat)
        assert player.block == 2

        non_colorless = create_card(CardId.STRIKE_IRONCLAD)
        non_colorless.owner = player
        relic.after_card_entered_combat(player, non_colorless, combat)
        assert player.block == 2

        other_owner_card = create_card(CardId.VOLLEY)
        other_owner_card.owner = combat.enemies[0]
        relic.after_card_entered_combat(player, other_owner_card, combat)
        assert player.block == 2

    def test_reptile_trinket_applies_temporary_strength_on_owned_potion_use(self):
        """Matches ReptileTrinket.cs: owned potion use in active combat grants temporary Strength(3)."""
        combat = _make_ironclad_combat(["ReptileTrinket"], seed=1205)
        relic = _combat_relic(combat, "REPTILE_TRINKET")
        player = combat.player

        relic.after_potion_used(player, SimpleNamespace(owner=player), None, combat)
        assert player.get_power_amount(PowerId.STRENGTH) == 3
        assert player.get_power_amount(PowerId.REPTILE_TRINKET) == 3

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert player.get_power_amount(PowerId.STRENGTH) == 0
        assert player.get_power_amount(PowerId.REPTILE_TRINKET) == 0

        combat.is_over = True
        relic.after_potion_used(player, SimpleNamespace(owner=player), None, combat)
        assert player.get_power_amount(PowerId.STRENGTH) == 0

    def test_stone_cracker_upgrades_three_cards_only_on_boss_room_entry(self):
        """Matches StoneCracker.cs: on boss room entry, upgrade up to 3 random upgradable deck cards."""
        run_state = RunState(seed=1206, character_id="Ironclad")
        run_state.player.deck = [
            create_card(CardId.STRIKE_IRONCLAD),
            create_card(CardId.DEFEND_IRONCLAD),
            create_card(CardId.BASH),
            create_card(CardId.STRIKE_IRONCLAD),
            create_card(CardId.DEFEND_IRONCLAD),
            create_card(CardId.STRIKE_IRONCLAD, upgraded=True),
        ]
        assert run_state.player.obtain_relic("STONE_CRACKER")
        relic = _run_relic(run_state, "STONE_CRACKER")
        upgraded_before = sum(1 for card in run_state.player.deck if card.upgraded)

        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.MONSTER))
        assert sum(1 for card in run_state.player.deck if card.upgraded) == upgraded_before

        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.BOSS))
        assert sum(1 for card in run_state.player.deck if card.upgraded) == upgraded_before + 3
