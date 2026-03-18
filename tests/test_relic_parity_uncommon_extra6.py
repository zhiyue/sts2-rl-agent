"""Sixth batch of uncommon relic parity tests for remaining uncovered hooks."""

from types import SimpleNamespace

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CardType, CombatSide, OrbType, PowerId
from sts2_env.core.hooks import (
    fire_after_block_cleared,
    fire_after_card_discarded,
    fire_after_card_played,
    fire_after_side_turn_start,
)
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.run.run_state import RunState


def _with_owner(cards: list, owner):
    for card in cards:
        card.owner = owner
    return cards


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1101,
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


def _make_defect_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1151,
) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=seed,
        character_id="Defect",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicParityUncommonExtra6:
    def test_candelabra_grants_energy_only_on_player_round_two_start(self):
        """Matches Candelabra.cs: +2 energy only on round 2 player-side turn start."""
        combat = _make_ironclad_combat(["Candelabra"], seed=1101)
        combat.energy = 1

        combat.round_number = 2
        fire_after_side_turn_start(CombatSide.ENEMY, combat)
        assert combat.energy == 1

        fire_after_side_turn_start(CombatSide.PLAYER, combat)
        assert combat.energy == 3

        combat.round_number = 3
        fire_after_side_turn_start(CombatSide.PLAYER, combat)
        assert combat.energy == 3

    def test_pen_nib_doubles_only_every_tenth_attack_then_deactivates(self):
        """Matches PenNib.cs: every 10th attack deals double; subsequent attack returns to normal."""
        combat = _make_ironclad_combat(["PenNib"], seed=1102)
        enemy = combat.enemies[0]
        enemy.max_hp = 600
        enemy.current_hp = 600
        enemy.block = 0
        combat.hand = _with_owner([make_strike_ironclad() for _ in range(11)], combat.player)
        combat.energy = 11

        hits: list[int] = []
        for _ in range(11):
            hp_before = enemy.current_hp
            assert combat.play_card(0, 0)
            hits.append(hp_before - enemy.current_hp)

        assert hits[8] == 6
        assert hits[9] == 12
        assert hits[10] == 6

    def test_lucky_fysh_gains_gold_each_time_a_card_is_added_to_deck(self):
        """Matches LuckyFysh.cs: every on-card-added-to-deck event gives +15 gold."""
        run_state = RunState(seed=1103, character_id="Ironclad")
        run_state.player.gold = 0
        assert run_state.player.obtain_relic("LUCKY_FYSH")

        run_state.player.add_card_instance_to_deck(create_card(CardId.STRIKE_IRONCLAD))
        assert run_state.player.gold == 15

        run_state.player.add_card_instance_to_deck(create_card(CardId.DEFEND_IRONCLAD))
        assert run_state.player.gold == 30

    def test_sparkling_rouge_triggers_only_on_owner_block_clear_during_round_three(self):
        """Matches SparklingRouge.cs: round-3 owner block clear grants Strength(1)+Dexterity(1)."""
        combat = _make_ironclad_combat(["SparklingRouge"], seed=1104)
        player = combat.player
        enemy = combat.enemies[0]

        combat.round_number = 2
        fire_after_block_cleared(player, combat)
        assert player.get_power_amount(PowerId.STRENGTH) == 0
        assert player.get_power_amount(PowerId.DEXTERITY) == 0

        combat.round_number = 3
        fire_after_block_cleared(enemy, combat)
        assert player.get_power_amount(PowerId.STRENGTH) == 0
        assert player.get_power_amount(PowerId.DEXTERITY) == 0

        fire_after_block_cleared(player, combat)
        assert player.get_power_amount(PowerId.STRENGTH) == 1
        assert player.get_power_amount(PowerId.DEXTERITY) == 1

    def test_tuning_fork_gains_block_on_tenth_skill_with_counter_carryover(self):
        """Matches TuningFork.cs: every 10 skills played gains 7 block, including across turns."""
        combat = _make_ironclad_combat(["TuningFork"], seed=1105)
        combat.player.block = 0
        skill_card = SimpleNamespace(card_type=CardType.SKILL)

        for _ in range(9):
            fire_after_card_played(skill_card, combat)
        assert combat.player.block == 0

        combat.end_player_turn()
        assert combat.round_number == 2

        fire_after_card_played(skill_card, combat)
        assert combat.player.block == 7

        for _ in range(10):
            fire_after_card_played(skill_card, combat)
        assert combat.player.block == 14

    def test_tingsha_only_deals_damage_for_discards_during_player_side(self):
        """Matches Tingsha.cs: discard trigger damages random enemy only on player side."""
        combat = _make_ironclad_combat(["Tingsha"], seed=1106, enemies=2)
        for enemy in combat.enemies:
            enemy.max_hp = 120
            enemy.current_hp = 120
            enemy.block = 0

        before = sum(enemy.current_hp for enemy in combat.enemies)
        combat.current_side = CombatSide.PLAYER
        fire_after_card_discarded(object(), combat)
        after_player_side = sum(enemy.current_hp for enemy in combat.enemies)
        assert before - after_player_side == 3

        combat.current_side = CombatSide.ENEMY
        fire_after_card_discarded(object(), combat)
        assert sum(enemy.current_hp for enemy in combat.enemies) == after_player_side

    def test_vambrace_doubles_first_card_block_once_per_combat(self):
        """Matches Vambrace.cs: first card-sourced block in combat is doubled once."""
        combat = _make_ironclad_combat(["Vambrace"], seed=1107)
        combat.player.block = 0
        combat.hand = _with_owner([make_defend_ironclad(), make_defend_ironclad()], combat.player)
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == 10

        assert combat.play_card(0)
        assert combat.player.block == 15

        combat2 = _make_ironclad_combat(["Vambrace"], seed=1108)
        combat2.player.block = 0
        combat2.player.gain_block(3, unpowered=True)
        combat2.hand = _with_owner([make_defend_ironclad()], combat2.player)
        combat2.energy = 1

        assert combat2.play_card(0)
        assert combat2.player.block == 13

    def test_symbiotic_virus_channels_dark_only_on_round_one_player_start(self):
        """Matches SymbioticVirus.cs: on round-1 player start, channel one Dark orb."""
        combat = _make_defect_combat(["SymbioticVirus"], seed=1109)
        assert combat.orb_queue is not None
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.DARK

        combat.orb_queue.orbs.clear()
        combat.round_number = 2
        fire_after_side_turn_start(CombatSide.PLAYER, combat)
        assert len(combat.orb_queue.orbs) == 0
