"""Focused owner-semantics regressions for Silent and Defect helper-style cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import make_sweeping_beam, make_turbo
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.silent import make_acrobatics, make_adrenaline, make_tactician
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


def _make_combat(character_id: str = "Ironclad") -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[make_strike_ironclad() for _ in range(6)],
        rng_seed=1001,
        character_id=character_id,
    )
    creature, ai = create_shrinker_beetle(Rng(1001))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def test_ally_acrobatics_draws_to_ally_hand_and_discards_from_ally_choice():
    combat = _make_combat("Silent")
    ally_state = PlayerState(player_id=2, character_id="Silent", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_acrobatics()
    card.owner = ally
    kept = make_strike_ironclad()
    drawn_a = make_strike_ironclad()
    drawn_b = make_strike_ironclad()
    drawn_c = make_strike_ironclad()
    ally_combat_state.hand = [card, kept]
    for current in ally_combat_state.hand:
        current.owner = ally
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.draw = [drawn_a, drawn_b, drawn_c]
    ally_combat_state.zone_map["draw"] = ally_combat_state.draw
    ally_combat_state.energy = 1
    starting_primary_hand = len(combat.hand)

    assert combat.play_card_from_creature(ally, 0)
    assert combat.pending_choice is not None
    assert all(option.card.owner is ally for option in combat.pending_choice.options)
    assert len(combat.hand) == starting_primary_hand

    assert combat.resolve_pending_choice(0)
    assert len(ally_combat_state.hand) == 3
    assert len(ally_combat_state.discard) == 2


def test_ally_adrenaline_draws_and_gains_energy_for_ally_only():
    combat = _make_combat("Silent")
    ally_state = PlayerState(player_id=2, character_id="Silent", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_adrenaline()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.draw = [make_strike_ironclad(), make_strike_ironclad()]
    ally_combat_state.zone_map["draw"] = ally_combat_state.draw
    ally_combat_state.energy = 0
    starting_primary_hand = len(combat.hand)
    starting_primary_energy = combat.energy

    assert combat.play_card_from_creature(ally, 0)
    assert len(ally_combat_state.hand) == 2
    assert ally_combat_state.energy == 1
    assert len(combat.hand) == starting_primary_hand
    assert combat.energy == starting_primary_energy


def test_ally_tactician_grants_energy_to_ally_only():
    combat = _make_combat("Silent")
    ally_state = PlayerState(player_id=2, character_id="Silent", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_tactician()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 3
    starting_primary_energy = combat.energy

    assert combat.play_card_from_creature(ally, 0)
    assert ally_combat_state.energy == 1
    assert combat.energy == starting_primary_energy


def test_ally_sweeping_beam_draws_to_ally_hand_only():
    combat = _make_combat("Defect")
    ally_state = PlayerState(player_id=2, character_id="Defect", max_hp=75, current_hp=75)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_sweeping_beam()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.draw = [make_strike_ironclad()]
    ally_combat_state.zone_map["draw"] = ally_combat_state.draw
    starting_primary_hand = len(combat.hand)
    ally_combat_state.energy = 1

    assert combat.play_card_from_creature(ally, 0)
    assert len(ally_combat_state.hand) == 1
    assert len(combat.hand) == starting_primary_hand


def test_ally_turbo_grants_energy_and_void_to_ally_only():
    combat = _make_combat("Defect")
    ally_state = PlayerState(player_id=2, character_id="Defect", max_hp=75, current_hp=75)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_turbo()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 0
    starting_primary_energy = combat.energy

    assert combat.play_card_from_creature(ally, 0)
    assert ally_combat_state.energy == 2
    assert any(current.card_id.name == "VOID" for current in ally_combat_state.discard)
    assert combat.energy == starting_primary_energy
