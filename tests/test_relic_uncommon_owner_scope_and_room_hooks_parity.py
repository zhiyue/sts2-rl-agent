"""Seventh batch of uncommon relic parity tests for remaining uncovered relic hooks."""

from types import SimpleNamespace

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CombatSide, MapPointType, PowerId, RoomType
from sts2_env.core.hooks import (
    fire_after_card_exhausted,
    fire_after_card_played,
    fire_after_turn_end,
    fire_before_card_played,
    fire_before_turn_end,
)
from sts2_env.core.rng import Rng
from sts2_env.map.generator import ActMap
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.run.rooms import CombatRoom, RoomVisitContext
from sts2_env.run.run_state import PlayerState, RunState


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


class TestRelicUncommonOwnerScopeAndRoomHooksParity:
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
        """Matches Planisphere.cs (v0.111.0): heal 5 when entering from an Unknown map node."""
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
        assert run_state.player.current_hp == 40

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

    def test_regalite_grants_block_once_per_turn_for_first_generated_card(self):
        """Matches Regalite.cs (v0.111.0): first card generated for the owner each turn grants 4 block."""
        combat = _make_ironclad_combat(["Regalite"], seed=1204)
        player = combat.player
        player.block = 0

        combat.add_generated_card_to_creature_hand(player, create_card(CardId.VOLLEY))
        assert player.block == 4

        combat.add_generated_card_to_creature_hand(player, create_card(CardId.VOLLEY))
        assert player.block == 4

        combat.add_generated_card_to_creature_hand(player, create_card(CardId.STRIKE_IRONCLAD))
        assert player.block == 4

    def test_regalite_resets_each_turn_and_ignores_other_owners_generated_cards(self):
        """Matches Regalite.cs (v0.111.0): counter resets on owner turn start; other owners' cards never trigger."""
        combat = _make_ironclad_combat(["Regalite"], seed=1209)
        player = combat.player
        player.block = 0
        enemy = combat.enemies[0]
        enemy.max_hp = 400
        enemy.current_hp = 400

        combat.add_generated_card_to_creature_hand(enemy, create_card(CardId.VOLLEY))
        assert player.block == 0

        combat.end_player_turn()
        assert combat.round_number == 2

        combat.add_generated_card_to_creature_hand(player, create_card(CardId.VOLLEY))
        assert player.block == 4

    def test_regalite_block_triggers_after_block_gained_hooks(self):
        combat = _make_ironclad_combat(["Regalite"], seed=1209)
        player = combat.player
        player.block = 0
        player.apply_power(PowerId.JUGGERNAUT, 5)
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp

        combat.add_generated_card_to_creature_hand(player, create_card(CardId.VOLLEY))

        assert player.block == 4
        assert enemy.current_hp == start_hp - 5

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
        """Matches StoneCracker.cs: boss combat start upgrades up to 3 random draw-pile cards."""
        deck = [
            create_card(CardId.STRIKE_IRONCLAD),
            create_card(CardId.DEFEND_IRONCLAD),
            create_card(CardId.BASH),
            create_card(CardId.STRIKE_IRONCLAD),
            create_card(CardId.DEFEND_IRONCLAD),
            create_card(CardId.STRIKE_IRONCLAD, upgraded=True),
        ]
        non_boss = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=[create_card(card.card_id, upgraded=card.upgraded) for card in deck],
            rng_seed=1206,
            character_id="Ironclad",
            relics=["STONE_CRACKER"],
            room=CombatRoom(room_type=RoomType.MONSTER),
        )
        creature, ai = create_shrinker_beetle(Rng(1206))
        non_boss.add_enemy(creature, ai)
        non_boss.start_combat()
        assert sum(1 for card in non_boss.combat_player_states[0].starting_deck if card.upgraded) == 1

        boss = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=deck,
            rng_seed=1206,
            character_id="Ironclad",
            relics=["STONE_CRACKER"],
            room=CombatRoom(room_type=RoomType.BOSS, is_boss=True),
        )
        creature, ai = create_shrinker_beetle(Rng(1207))
        boss.add_enemy(creature, ai)
        boss.start_combat()
        assert sum(1 for card in boss.combat_player_states[0].starting_deck if card.upgraded) == 4

    def test_owner_scoped_card_play_relics_ignore_other_players_cards(self):
        """Matches owner checks on common and uncommon card-play relic hooks."""
        combat = _make_ironclad_combat(
            [
                "Permafrost",
                "Kusarigama",
                "LetterOpener",
                "Nunchaku",
                "OrnamentalFan",
                "RippleBasin",
                "TuningFork",
            ],
            seed=1207,
            enemies=2,
        )
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        for enemy in combat.enemies:
            enemy.max_hp = 300
            enemy.current_hp = 300
        combat.player.block = 0

        ally_attack = create_card(CardId.STRIKE_IRONCLAD)
        ally_attack.owner = ally
        ally_skill = create_card(CardId.DEFEND_IRONCLAD)
        ally_skill.owner = ally
        ally_power = create_card(CardId.INFLAME)
        ally_power.owner = ally

        for _ in range(10):
            fire_after_card_played(ally_attack, combat)
        for _ in range(10):
            fire_after_card_played(ally_skill, combat)
        fire_after_card_played(ally_power, combat)

        assert combat.player.block == 0
        assert combat.energy == 3
        assert [enemy.current_hp for enemy in combat.enemies] == [300, 300]

        fire_before_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.block == 4

    def test_pen_nib_ignores_other_players_attacks_for_counter(self):
        """Matches PenNib.cs: only the owner's attacks advance the counter."""
        combat = _make_ironclad_combat(["PenNib"], seed=1208)
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        enemy = combat.enemies[0]
        enemy.max_hp = 300
        enemy.current_hp = 300

        ally_attack = create_card(CardId.STRIKE_IRONCLAD)
        ally_attack.owner = ally
        for _ in range(9):
            fire_before_card_played(ally_attack, combat)
            fire_after_card_played(ally_attack, combat)

        strike = make_strike_ironclad()
        strike.owner = combat.player
        combat.hand = [strike]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 294

    def test_owner_scoped_exhaust_relics_ignore_other_players_cards(self):
        """Matches JossPaper.cs: only owner-card exhausts advance the draw counter."""
        combat = _make_ironclad_combat(["JossPaper"], seed=1209)
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        marker = create_card(CardId.BASH)
        marker.owner = combat.player
        combat.draw_pile = [marker]

        ally_card = create_card(CardId.STRIKE_IRONCLAD)
        ally_card.owner = ally
        owner_card = create_card(CardId.STRIKE_IRONCLAD)
        owner_card.owner = combat.player

        for _ in range(5):
            fire_after_card_exhausted(ally_card, combat)
        assert marker not in combat.hand

        for _ in range(5):
            fire_after_card_exhausted(owner_card, combat)
        assert marker in combat.hand
