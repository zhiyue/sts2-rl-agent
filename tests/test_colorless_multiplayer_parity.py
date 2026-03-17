"""Focused multiplayer parity tests for owner-aware Colorless cards."""

from sts2_env.cards.base import CardInstance
from sts2_env.cards.colorless import make_believe_in_you, make_huddle_up, make_lift, make_rally
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CardRarity, CardType, CombatSide, TargetType
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[make_strike_ironclad() for _ in range(6)],
        rng_seed=901,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(901))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def test_ally_believe_in_you_grants_energy_to_target_ally_only():
    combat = _make_combat()
    ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_believe_in_you()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 1
    starting_primary_energy = combat.energy
    starting_ally_energy = ally_combat_state.energy

    assert combat.play_card_from_creature(ally, 0, 0)
    assert ally_combat_state.energy == starting_ally_energy
    assert combat.energy == starting_primary_energy + 3


def test_ally_lift_targets_primary_player_block_without_affecting_ally():
    combat = _make_combat()
    ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_lift()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 1

    assert combat.play_card_from_creature(ally, 0, 0)
    assert combat.primary_player.block == 11
    assert ally.block == 0


def test_ally_huddle_up_draws_cards_to_primary_player_hand():
    combat = _make_combat()
    ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_huddle_up()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.draw = [make_strike_ironclad(), make_strike_ironclad()]
    ally_combat_state.zone_map["draw"] = ally_combat_state.draw
    combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad()]
    starting_primary_hand = len(combat.hand)
    ally_combat_state.energy = 1

    assert combat.play_card_from_creature(ally, 0)
    assert len(combat.hand) == starting_primary_hand + 2
    assert len(ally_combat_state.hand) == 0


def test_ally_rally_grants_block_to_primary_player_only():
    combat = _make_combat()
    ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = make_rally()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 2

    assert combat.play_card_from_creature(ally, 0)
    assert combat.primary_player.block == 12
    assert ally.block == 0


def test_any_ally_targeting_indices_resolve_from_ally_owner_perspective():
    combat = _make_combat()
    ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None

    card = CardInstance(
        card_id=CardId.LIFT,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.ANY_ALLY,
        rarity=CardRarity.UNCOMMON,
        base_block=8,
    )
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 1

    assert combat.play_card_from_creature(ally, 0, 0)
    assert combat.primary_player.block == 8
