"""Parity tests for rare/shop/event relic block, discard, and potion hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CombatSide, PowerId, ValueProp
from sts2_env.core.hooks import (
    fire_after_block_cleared,
    fire_after_card_discarded,
    fire_after_shuffle,
    fire_after_side_turn_start,
    fire_before_side_turn_start,
)
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.potions.base import create_potion
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


def test_beating_remnant_counts_final_hp_loss_after_other_reductions():
    combat = _make_ironclad_combat(["BeatingRemnant", "TungstenRod"], seed=1016)
    player = combat.player
    enemy = combat.enemies[0]
    player.block = 0
    start_hp = player.current_hp

    combat.deal_damage(enemy, player, 12, ValueProp.UNPOWERED)
    combat.deal_damage(enemy, player, 20, ValueProp.UNPOWERED)

    assert player.current_hp == start_hp - 19


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


def test_captains_wheel_block_triggers_after_block_gained_hooks():
    combat = _make_ironclad_combat(["CaptainsWheel"], seed=1030)
    player = combat.player
    enemy = combat.enemies[0]
    start_hp = enemy.current_hp
    player.block = 0
    player.apply_power(PowerId.JUGGERNAUT, 5)

    combat.round_number = 3
    fire_after_block_cleared(player, combat)

    assert player.block == 18
    assert enemy.current_hp == start_hp - 5


def test_tough_bandages_only_grants_block_when_discard_happens_on_owner_side():
    combat = _make_ironclad_combat(["ToughBandages"], seed=1003)
    player = combat.player
    player.block = 0

    combat.current_side = CombatSide.PLAYER
    card = make_strike_ironclad()
    card.owner = player
    fire_after_card_discarded(card, combat)
    assert player.block == 3

    combat.current_side = CombatSide.ENEMY
    fire_after_card_discarded(card, combat)
    assert player.block == 3


def test_tough_bandages_block_triggers_after_block_gained_hooks():
    combat = _make_ironclad_combat(["ToughBandages"], seed=1031)
    player = combat.player
    enemy = combat.enemies[0]
    start_hp = enemy.current_hp
    player.block = 0
    player.apply_power(PowerId.JUGGERNAUT, 5)
    card = make_strike_ironclad()
    card.owner = player

    combat.current_side = CombatSide.PLAYER
    fire_after_card_discarded(card, combat)

    assert player.block == 3
    assert enemy.current_hp == start_hp - 5


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


def test_the_abacus_block_triggers_after_block_gained_hooks():
    combat = _make_ironclad_combat(["TheAbacus"], seed=1032)
    player = combat.player
    enemy = combat.enemies[0]
    start_hp = enemy.current_hp
    player.block = 0
    player.apply_power(PowerId.JUGGERNAUT, 5)

    fire_after_shuffle(combat)

    assert player.block == 6
    assert enemy.current_hp == start_hp - 5


def test_sturdy_clamp_caps_block_when_it_prevents_clear():
    combat = _make_ironclad_combat(["SturdyClamp"], seed=1014)
    player = combat.player
    player.block = 18

    player.clear_block(combat)
    fire_after_block_cleared(player, combat)

    assert player.block == 10


def test_sturdy_clamp_does_not_cap_when_blur_prevents_clear_first():
    combat = _make_ironclad_combat(["SturdyClamp"], seed=1015)
    player = combat.player
    player.block = 18
    player.apply_power(PowerId.BLUR, 1)

    player.clear_block(combat)
    fire_after_block_cleared(player, combat)

    assert player.block == 18


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


def test_booming_conch_grants_one_energy_on_round_one_elite_side_turn_start():
    """C# BoomingConch.cs:45-55 AfterSideTurnStart: on turn <= 1 of an elite
    combat the owner's side gains EnergyVar(1)."""
    combat = _make_ironclad_combat(["BoomingConch"], seed=1015)
    relic = next(relic for relic in combat.relics if relic.relic_id.name == "BOOMING_CONCH")
    combat.is_elite = True
    combat.round_number = 1
    energy_before = combat.current_player_state.energy

    fire_after_side_turn_start(CombatSide.PLAYER, combat)
    assert combat.current_player_state.energy == energy_before + 1

    # Only the first turn of the elite combat grants energy.
    combat.round_number = 2
    energy_before = combat.current_player_state.energy
    fire_after_side_turn_start(CombatSide.PLAYER, combat)
    assert combat.current_player_state.energy == energy_before


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


def test_alchemical_coffer_fills_added_slots_not_existing_empty_slots():
    run_state = RunState(seed=1010, character_id="Ironclad")
    run_state.player.max_potion_slots = 3
    run_state.player.potions = [create_potion("FirePotion"), None, create_potion("FlexPotion")]

    assert run_state.player.obtain_relic("ALCHEMICAL_COFFER")

    assert run_state.player.potions[1] is None
    assert run_state.player.max_potion_slots == 7
    assert all(potion is not None for potion in run_state.player.potions[3:7])


def test_hand_drill_applies_vulnerable_when_attack_breaks_enemy_block():
    combat = _make_ironclad_combat(["HandDrill"], seed=1009)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 60
    enemy.current_hp = 60
    enemy.block = 5

    drawn = make_strike_ironclad()
    combat.draw_pile = _with_owner([drawn], player)
    combat.apply_power_to(player, PowerId.VICIOUS, 1)
    combat.hand = _with_owner([make_strike_ironclad()], player)
    combat.energy = 1
    assert combat.play_card(0, 0)
    assert enemy.get_power_amount(PowerId.VULNERABLE) == 2
    assert drawn in combat.hand

    combat.hand = _with_owner([make_strike_ironclad()], player)
    combat.energy = 1
    assert combat.play_card(0, 0)
    assert enemy.get_power_amount(PowerId.VULNERABLE) == 2


def test_hand_drill_counts_pet_owned_damage_when_breaking_block():
    combat = _make_ironclad_combat(["HandDrill"], seed=1024)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.block = 5
    pet = combat.summon_event_pet(player, "PAELS_LEGION")
    assert pet is not None

    combat.deal_damage(pet, enemy, 6, ValueProp.MOVE)

    assert enemy.get_power_amount(PowerId.VULNERABLE) == 2


def test_vitruvian_minion_doubles_owned_minion_card_damage_and_block():
    combat = _make_ironclad_combat(["VitruvianMinion"], seed=1025)
    player = combat.player
    enemy = combat.enemies[0]
    relic = next(relic for relic in combat.current_player_state.relics if relic.relic_id.name == "VITRUVIAN_MINION")
    minion_attack = create_card(CardId.MINION_STRIKE)
    minion_attack.owner = player
    minion_skill = create_card(CardId.MINION_SACRIFICE)
    minion_skill.owner = player
    plain_attack = make_strike_ironclad()
    plain_attack.owner = player

    assert relic.modify_damage_multiplicative(player, player, enemy, ValueProp.MOVE, minion_attack) == 2.0
    assert relic.modify_block_multiplicative(player, player, ValueProp.MOVE, card_source=minion_skill) == 2.0
    assert relic.modify_damage_multiplicative(player, player, enemy, ValueProp.MOVE, plain_attack) == 1.0


def test_strike_dummy_relics_count_owned_strike_card_even_from_pet_dealer():
    combat = _make_ironclad_combat(["StrikeDummy", "FakeStrikeDummy"], seed=1026)
    player = combat.player
    enemy = combat.enemies[0]
    pet = combat.summon_event_pet(player, "PAELS_LEGION")
    strike = make_strike_ironclad()
    strike.owner = player

    by_id = {relic.relic_id.name: relic for relic in combat.current_player_state.relics}

    assert by_id["STRIKE_DUMMY"].modify_damage_additive(player, pet, enemy, ValueProp.MOVE, strike) == 3
    assert by_id["FAKE_STRIKE_DUMMY"].modify_damage_additive(player, pet, enemy, ValueProp.MOVE, strike) == 1
    assert by_id["FAKE_STRIKE_DUMMY"].modify_damage_additive(player, pet, enemy, ValueProp.UNPOWERED, strike) == 0


def test_mystic_lighter_counts_owned_enchanted_card_and_requires_powered_damage():
    combat = _make_ironclad_combat(["MysticLighter"], seed=1027)
    player = combat.player
    enemy = combat.enemies[0]
    pet = combat.summon_event_pet(player, "PAELS_LEGION")
    strike = make_strike_ironclad()
    strike.owner = player
    strike.add_enchantment("Glam", 1)
    relic = next(relic for relic in combat.current_player_state.relics if relic.relic_id.name == "MYSTIC_LIGHTER")

    assert relic.modify_damage_additive(player, pet, enemy, ValueProp.MOVE, strike) == 9
    assert relic.modify_damage_additive(player, pet, enemy, ValueProp.MOVE | ValueProp.UNPOWERED, strike) == 0


def test_undying_sigil_halves_powered_enemy_damage_when_enemy_hp_is_at_doom():
    combat = _make_ironclad_combat(["UndyingSigil"], seed=1028)
    player = combat.player
    enemy = combat.enemies[0]
    relic = next(relic for relic in combat.current_player_state.relics if relic.relic_id.name == "UNDYING_SIGIL")
    enemy.current_hp = 5
    enemy.apply_power(PowerId.DOOM, 5)

    assert relic.modify_damage_multiplicative(player, enemy, player, ValueProp.MOVE) == 0.5
    assert relic.modify_damage_multiplicative(player, enemy, player, ValueProp.UNPOWERED) == 1.0
    assert relic.modify_damage_multiplicative(player, player, player, ValueProp.MOVE) == 1.0
