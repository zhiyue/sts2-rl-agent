"""Focused parity coverage for additional rare/shop/event relic hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CombatSide, PowerId, ValueProp
from sts2_env.core.hooks import (
    fire_after_block_cleared,
    fire_after_card_discarded,
    fire_before_side_turn_start,
)
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.reward_objects import RemoveCardReward
from sts2_env.run.run_state import RunState


def _with_owner(cards: list, owner):
    for card in cards:
        card.owner = owner
    return cards


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1001,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def test_beating_remnant_caps_damage_per_turn_and_resets_next_player_turn():
    combat = _make_ironclad_combat(["BeatingRemnant"], seed=1001)
    player = combat.player
    enemy = combat.enemies[0]
    player.block = 0
    start_hp = player.current_hp

    combat.deal_damage(enemy, player, 12, ValueProp.UNPOWERED)
    combat.deal_damage(enemy, player, 20, ValueProp.UNPOWERED)
    assert player.current_hp == start_hp - 20

    fire_before_side_turn_start(CombatSide.PLAYER, combat)
    combat.deal_damage(enemy, player, 7, ValueProp.UNPOWERED)
    assert player.current_hp == start_hp - 27


def test_captains_wheel_grants_block_only_on_round_three_player_block_clear():
    combat = _make_ironclad_combat(["CaptainsWheel"], seed=1002)
    player = combat.player
    enemy = combat.enemies[0]
    player.block = 0

    combat.round_number = 1
    fire_after_block_cleared(player, combat)
    assert player.block == 0

    combat.round_number = 3
    fire_after_block_cleared(enemy, combat)
    assert player.block == 0

    fire_after_block_cleared(player, combat)
    assert player.block == 18


def test_tough_bandages_only_grants_block_when_discard_happens_on_player_side():
    combat = _make_ironclad_combat(["ToughBandages"], seed=1003)
    player = combat.player
    player.block = 0

    combat.current_side = CombatSide.PLAYER
    fire_after_card_discarded(object(), combat)
    assert player.block == 3

    combat.current_side = CombatSide.ENEMY
    fire_after_card_discarded(object(), combat)
    assert player.block == 3


def test_the_abacus_gains_block_when_draw_triggers_shuffle():
    combat = _make_ironclad_combat(["TheAbacus"], seed=1004)
    player = combat.player
    player.block = 0
    recycled = make_strike_ironclad()
    recycled.owner = player
    combat.draw_pile = []
    combat.discard_pile = [recycled]

    combat.draw_cards(player, 1)
    assert player.block == 6


def test_booming_conch_adds_draw_only_for_round_one_elite():
    combat = _make_ironclad_combat(["BoomingConch"], seed=1005)
    relic = next(relic for relic in combat.relics if relic.relic_id.name == "BOOMING_CONCH")

    combat.round_number = 1
    combat.is_elite = True
    assert relic.modify_hand_draw(combat.player, 5, combat) == 7

    combat.round_number = 2
    assert relic.modify_hand_draw(combat.player, 5, combat) == 5

    combat.round_number = 1
    combat.is_elite = False
    assert relic.modify_hand_draw(combat.player, 5, combat) == 5


def test_empty_cage_removes_two_cards_immediately_or_queues_choice_reward():
    immediate = RunState(seed=1006, character_id="Ironclad")
    immediate.player.deck = create_ironclad_starter_deck()
    immediate_start = len(immediate.player.deck)
    assert immediate.player.obtain_relic("EMPTY_CAGE")
    assert len(immediate.player.deck) == immediate_start - 2

    deferred = RunState(seed=1007, character_id="Ironclad")
    deferred.player.deck = create_ironclad_starter_deck()
    deferred.enable_deck_choice_requests = True
    deferred_start = len(deferred.player.deck)
    assert deferred.player.obtain_relic("EMPTY_CAGE")
    assert len(deferred.player.deck) == deferred_start

    remove_rewards = [reward for reward in deferred.pending_rewards if isinstance(reward, RemoveCardReward)]
    assert len(remove_rewards) == 1
    assert remove_rewards[0].count == 2
    assert remove_rewards[0].cards is not None
    assert len(remove_rewards[0].cards) >= 2


def test_alchemical_coffer_adds_slots_and_fills_new_potion_slots():
    run_state = RunState(seed=1008, character_id="Ironclad")
    start_slots = run_state.player.max_potion_slots
    start_held = len(run_state.player.held_potions())

    assert run_state.player.obtain_relic("ALCHEMICAL_COFFER")
    assert run_state.player.max_potion_slots == start_slots + 4
    assert len(run_state.player.held_potions()) == start_held + 4


def test_hand_drill_applies_vulnerable_when_attack_breaks_enemy_block():
    combat = _make_ironclad_combat(["HandDrill"], seed=1009)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 60
    enemy.current_hp = 60
    enemy.block = 5

    combat.hand = _with_owner([make_strike_ironclad()], player)
    combat.energy = 1
    assert combat.play_card(0, 0)
    assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

    combat.hand = _with_owner([make_strike_ironclad()], player)
    combat.energy = 1
    assert combat.play_card(0, 0)
    assert enemy.get_power_amount(PowerId.VULNERABLE) == 2
