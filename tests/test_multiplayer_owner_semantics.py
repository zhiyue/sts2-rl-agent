"""Focused multiplayer owner-semantics regressions across card modules."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import make_sweeping_beam, make_turbo
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.regent import make_big_bang, make_gather_light, make_make_it_so, make_venerate
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
