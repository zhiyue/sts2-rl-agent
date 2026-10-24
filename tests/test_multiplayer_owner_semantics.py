"""Focused multiplayer owner-semantics regressions across card modules."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import make_energy_surge, make_ignition, make_sweeping_beam, make_turbo
from sts2_env.cards.ironclad import (
    make_battle_trance,
    make_bloodletting,
    make_burning_pact,
    make_offering,
)
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.regent import make_big_bang, make_gather_light, make_make_it_so, make_venerate
from sts2_env.cards.status import make_dazed
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide, OrbType, PowerId
from sts2_env.core.hooks import fire_before_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.relics.registry import create_relic_by_name
from sts2_env.run.run_state import PlayerState


def _make_combat(character_id: str = "Ironclad") -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[make_strike_ironclad() for _ in range(6)],
        rng_seed=951,
        character_id=character_id,
    )
    creature, ai = create_shrinker_beetle(Rng(951))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def test_ally_turbo_grants_energy_and_void_to_ally_only():
    combat = _make_combat("Defect")
    ally_state = PlayerState(player_id=2, character_id="Defect", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    card = make_turbo()
    card.owner = ally
    ally_state_combat.hand = [card]
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand
    ally_state_combat.energy = 0
    starting_primary_energy = combat.energy

    assert combat.play_card_from_creature(ally, 0)
    assert ally_state_combat.energy == 2
    assert combat.energy == starting_primary_energy
    assert len(ally_state_combat.discard) == 2


def test_owner_draw_reaction_powers_ignore_ally_drawn_cards():
    combat = _make_combat()
    ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None
    marker = make_strike_ironclad()
    ally_status = make_dazed()
    ally_status.owner = ally
    combat.draw_pile = [marker]
    combat.hand.clear()
    ally_state_combat.draw = [ally_status]
    ally_state_combat.hand = []
    combat.player.apply_power(PowerId.ITERATION, 1)
    combat.player.apply_power(PowerId.PAGESTORM, 1)

    combat.draw_cards(ally, 1)

    assert ally_status in ally_state_combat.hand
    assert marker in combat.draw_pile
    assert marker not in combat.hand


def test_ally_sweeping_beam_draws_to_ally_hand_only():
    combat = _make_combat("Defect")
    ally_state = PlayerState(player_id=2, character_id="Defect", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    card = make_sweeping_beam()
    card.owner = ally
    ally_state_combat.hand = [card]
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand
    ally_state_combat.draw = [make_strike_ironclad()]
    ally_state_combat.zone_map["draw"] = ally_state_combat.draw
    starting_primary_hand = len(combat.hand)
    ally_state_combat.energy = 1

    assert combat.play_card_from_creature(ally, 0)
    assert len(ally_state_combat.hand) == 1
    assert len(combat.hand) == starting_primary_hand


def test_energy_surge_grants_energy_to_every_teammate_including_caster():
    # EnergySurge.cs OnPlay: GetTeammatesOf(Owner.Creature) (CombatState.cs:397)
    # covers the whole side filtered to alive players — the caster AND allies.
    combat = _make_combat("Defect")
    ally_state = PlayerState(player_id=2, character_id="Defect", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    card = make_energy_surge()
    combat.hand = [card]
    combat.energy = 1
    ally_state_combat.energy = 0

    assert combat.play_card(0)
    assert combat.energy == 2  # 1 - 1 cost + 2 gained
    assert ally_state_combat.energy == 2


def test_ally_ice_cream_preserves_ally_energy_only():
    combat = _make_combat()
    combat.relics.append(create_relic_by_name("Ectoplasm"))
    ally_state = PlayerState(
        player_id=2,
        character_id="Ironclad",
        max_hp=70,
        current_hp=70,
        relics=["IceCream"],
    )
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    combat.energy = 1
    ally_state_combat.energy = 2

    combat.end_player_turn()

    assert combat.round_number == 2
    assert combat.energy == 4
    assert ally_state_combat.energy == ally_state_combat.base_max_energy + 2


def test_ignition_channels_plasma_to_target_ally_orb_queue():
    combat = _make_combat("Defect")
    ally_state = PlayerState(player_id=2, character_id="Defect", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None
    assert combat.orb_queue is not None
    assert ally_state_combat.orb_queue is not None

    card = make_ignition()
    combat.hand = [card]
    combat.energy = 1

    assert combat.play_card(0, 0)
    assert not combat.orb_queue.orbs
    assert len(ally_state_combat.orb_queue.orbs) == 1
    assert ally_state_combat.orb_queue.orbs[0].orb_type == OrbType.PLASMA


def test_ally_orb_overflow_evoke_uses_ally_state():
    combat = _make_combat("Defect")
    ally_state = PlayerState(player_id=2, character_id="Defect", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None
    assert ally_state_combat.orb_queue is not None

    combat.player.block = 0
    ally.block = 0
    for _ in range(4):
        combat.channel_orb(ally, "FROST")

    assert ally.block == 5
    assert combat.player.block == 0


def test_ally_hand_based_relics_use_ally_hand():
    combat = _make_combat()
    ally_state = PlayerState(
        player_id=2,
        character_id="Ironclad",
        max_hp=70,
        current_hp=70,
        relics=["BoneTea", "CloakClasp"],
    )
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    primary_card = make_strike_ironclad()
    combat.hand = [primary_card]
    ally_cards = [make_strike_ironclad(), make_defend_ironclad()]
    for card in ally_cards:
        card.owner = ally
    ally_state_combat.hand = ally_cards
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand

    for relic in ally_state_combat.relics:
        if relic.relic_id.name == "BONE_TEA":
            relic.after_side_turn_start(ally, CombatSide.PLAYER, combat)

    assert primary_card.upgraded is False
    assert all(card.upgraded for card in ally_cards)

    combat.player.block = 0
    ally.block = 0
    fire_before_turn_end(CombatSide.PLAYER, combat)

    assert combat.player.block == 0
    assert ally.block == 2


def test_ally_ironclad_draw_and_energy_cards_use_ally_state():
    combat = _make_combat("Ironclad")
    ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    battle_trance = make_battle_trance()
    battle_trance.owner = ally
    ally_state_combat.hand = [battle_trance]
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand
    ally_state_combat.draw = [make_strike_ironclad(), make_strike_ironclad(), make_strike_ironclad()]
    ally_state_combat.zone_map["draw"] = ally_state_combat.draw
    ally_state_combat.energy = 0
    primary_hand_count = len(combat.hand)

    assert combat.play_card_from_creature(ally, 0)
    assert len(ally_state_combat.hand) == 3
    assert len(combat.hand) == primary_hand_count
    assert ally.get_power_amount(PowerId.NO_DRAW) == 1
    ally.powers.pop(PowerId.NO_DRAW, None)

    bloodletting = make_bloodletting()
    bloodletting.owner = ally
    ally_state_combat.hand = [bloodletting]
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand
    ally_state_combat.energy = 0
    ally_hp_before = ally.current_hp
    primary_energy_before = combat.energy

    assert combat.play_card_from_creature(ally, 0)
    assert ally_state_combat.energy == 2
    assert ally.current_hp == ally_hp_before - 3
    assert combat.energy == primary_energy_before

    offering = make_offering()
    offering.owner = ally
    ally_state_combat.hand = [offering]
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand
    ally_state_combat.draw = [make_strike_ironclad(), make_strike_ironclad(), make_strike_ironclad()]
    ally_state_combat.zone_map["draw"] = ally_state_combat.draw
    ally_state_combat.energy = 0
    ally_hp_before = ally.current_hp
    primary_hand_count = len(combat.hand)

    assert combat.play_card_from_creature(ally, 0)
    assert ally_state_combat.energy == 2
    assert ally.current_hp == ally_hp_before - 6
    assert len(ally_state_combat.hand) == 3
    assert len(combat.hand) == primary_hand_count


def test_ally_burning_pact_draws_to_ally_after_choice():
    combat = _make_combat("Ironclad")
    ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    pact = make_burning_pact()
    pact.owner = ally
    strike = make_strike_ironclad()
    defend = make_defend_ironclad()
    draw_a = make_strike_ironclad()
    draw_b = make_strike_ironclad()
    ally_state_combat.hand = [pact, strike, defend]
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand
    ally_state_combat.draw = [draw_a, draw_b]
    ally_state_combat.zone_map["draw"] = ally_state_combat.draw
    ally_state_combat.energy = 1
    primary_hand_count = len(combat.hand)

    assert combat.play_card_from_creature(ally, 0)
    assert combat.pending_choice is not None
    assert combat.resolve_pending_choice(1)

    assert defend in ally_state_combat.exhaust
    assert draw_a in ally_state_combat.hand
    assert draw_b in ally_state_combat.hand
    assert len(combat.hand) == primary_hand_count


def test_ally_big_bang_draws_gains_energy_stars_and_forges_for_ally_owner():
    combat = _make_combat("Regent")
    ally_state = PlayerState(player_id=2, character_id="Regent", max_hp=60, current_hp=60)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    card = make_big_bang()
    card.owner = ally
    ally_state_combat.hand = [card]
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand
    ally_state_combat.draw = [make_strike_ironclad()]
    ally_state_combat.zone_map["draw"] = ally_state_combat.draw
    ally_state_combat.energy = 0

    assert combat.play_card_from_creature(ally, 0)
    assert ally_state_combat.energy == 1
    assert ally_state_combat.stars == 1
    assert combat.primary_player.stars == 0
    assert any(owner_card.card_id.name == "SOVEREIGN_BLADE" for owner_card in ally_state_combat.hand)


def test_make_it_so_late_effect_tracks_owner_not_primary_player():
    combat = _make_combat("Regent")
    ally_state = PlayerState(player_id=2, character_id="Regent", max_hp=60, current_hp=60)
    ally = combat.add_ally_player(ally_state)
    ally_state_combat = combat.combat_player_state_for(ally)
    assert ally_state_combat is not None

    watcher = make_make_it_so()
    watcher.owner = ally
    ally_state_combat.discard = [watcher]
    ally_state_combat.zone_map["discard"] = ally_state_combat.discard
    ally_state_combat.hand = [make_gather_light(), make_venerate(), make_venerate()]
    for card in ally_state_combat.hand:
        card.owner = ally
    ally_state_combat.zone_map["hand"] = ally_state_combat.hand
    ally_state_combat.energy = 3

    combat.hand = [make_gather_light(), make_venerate(), make_venerate()]
    combat.energy = 3

    assert combat.play_card(0)
    assert watcher in ally_state_combat.discard
    assert combat.play_card(0)
    assert watcher in ally_state_combat.discard
    assert combat.play_card(0)
    assert watcher in ally_state_combat.discard

    assert combat.play_card_from_creature(ally, 0)
    assert watcher in ally_state_combat.discard
    assert combat.play_card_from_creature(ally, 0)
    assert watcher in ally_state_combat.discard
    assert combat.play_card_from_creature(ally, 0)
    assert watcher in ally_state_combat.hand
