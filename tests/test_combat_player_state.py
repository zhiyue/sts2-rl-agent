"""Tests for multi-player combat state containers."""

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.colorless import make_discovery
from sts2_env.cards.defect import make_charge_battery
from sts2_env.cards.silent import make_backflip
from sts2_env.cards.regent import make_gather_light
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.potions.base import create_potion
from sts2_env.run.run_state import PlayerState


def test_add_ally_player_state_creates_full_combat_player_state():
    ally_state = PlayerState(
        player_id=2,
        character_id="Silent",
        max_hp=70,
        current_hp=65,
        deck=create_ironclad_starter_deck(),
        relics=["RING_OF_THE_SNAKE"],
        max_potion_slots=2,
    )
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=7,
        character_id="Ironclad",
    )

    ally_creature = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally_creature)

    assert ally_combat_state is not None
    assert ally_combat_state.player_state is ally_state
    assert ally_combat_state.character_id == "Silent"
    assert ally_combat_state.max_potion_slots == 2
    assert len(ally_combat_state.relics) == 1
    assert combat._ally_player_zones[ally_creature] is ally_combat_state.zone_map  # noqa: SLF001


def test_ally_player_state_tracks_independent_resources_and_potions():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=9,
        character_id="Ironclad",
    )
    ally = Creature(max_hp=50, current_hp=50, side=CombatSide.PLAYER, is_player=True)
    combat.add_ally_player(ally)

    combat.gain_energy(ally, 2)
    combat.gain_stars(ally, 3)
    assert combat.add_potion(create_potion("FirePotion"), owner=ally)

    ally_state = combat.combat_player_state_for(ally)
    assert ally_state is not None
    assert ally_state.energy == 2
    assert ally_state.stars == 3
    assert len(combat.held_potions(ally)) == 1
    assert combat.energy == 0
    assert combat.stars == 0


def test_ally_card_play_uses_acting_owner_for_player_and_resources():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=13,
        character_id="Ironclad",
    )
    ally_state = PlayerState(
        player_id=2,
        character_id="Regent",
        max_hp=60,
        current_hp=60,
    )
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None
    card = make_gather_light()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 1

    assert combat.play_card_from_creature(ally, 0)
    assert ally.block == 7
    assert ally.stars == 1
    assert ally_combat_state.stars == 1
    assert combat.primary_player.stars == 0
    assert card in ally_combat_state.discard


def test_ally_generated_card_choice_uses_ally_character_pool_and_hand():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=17,
        character_id="Ironclad",
    )
    ally_state = PlayerState(
        player_id=2,
        character_id="Silent",
        max_hp=70,
        current_hp=70,
    )
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None
    discovery = make_discovery()
    discovery.owner = ally
    ally_combat_state.hand = [discovery]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 1

    assert combat.play_card_from_creature(ally, 0)
    assert combat.pending_choice is not None
    assert combat.resolve_pending_choice(0)
    assert len(ally_combat_state.hand) == 1
    assert ally_combat_state.hand[0].owner is ally
    assert ally_combat_state.hand[0].cost == 0
    assert ally_combat_state.hand[0].card_id != discovery.card_id


def test_ally_silent_backflip_gains_block_and_draws_to_ally_hand():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=31,
        character_id="Ironclad",
    )
    ally_state = PlayerState(player_id=2, character_id="Silent", max_hp=70, current_hp=70)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None
    card = make_backflip()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.draw = [make_strike_ironclad(), make_strike_ironclad()]
    ally_combat_state.zone_map["draw"] = ally_combat_state.draw
    ally_combat_state.energy = 1

    assert combat.play_card_from_creature(ally, 0)
    assert ally.block == 5
    assert len(ally_combat_state.hand) == 2
    assert combat.primary_player.block == 0


def test_ally_defect_charge_battery_applies_energy_next_turn_to_ally():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=33,
        character_id="Ironclad",
    )
    ally_state = PlayerState(player_id=2, character_id="Defect", max_hp=75, current_hp=75)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None
    card = make_charge_battery()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 1

    assert combat.play_card_from_creature(ally, 0)
    assert ally.block == 7
    assert ally.has_power(PowerId.ENERGY_NEXT_TURN)
    assert not combat.primary_player.has_power(PowerId.ENERGY_NEXT_TURN)


def test_start_combat_initializes_ally_energy_and_opening_hand():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=23,
        character_id="Ironclad",
    )
    ally_state = PlayerState(
        player_id=2,
        character_id="Ironclad",
        max_hp=60,
        current_hp=60,
        deck=[make_strike_ironclad() for _ in range(5)],
    )
    ally = combat.add_ally_player(ally_state)
    creature, ai = create_shrinker_beetle(Rng(23))
    combat.add_enemy(creature, ai)

    combat.start_combat()

    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None
    assert ally_combat_state.energy == ally_combat_state.base_max_energy
    assert len(ally_combat_state.hand) == 5


def test_end_player_turn_resets_ally_energy_next_round():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=25,
        character_id="Ironclad",
    )
    ally_state = PlayerState(
        player_id=2,
        character_id="Ironclad",
        max_hp=60,
        current_hp=60,
        deck=[make_strike_ironclad() for _ in range(5)],
    )
    ally = combat.add_ally_player(ally_state)
    creature, ai = create_shrinker_beetle(Rng(25))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None
    ally_combat_state.energy = 0

    combat.end_player_turn()

    assert ally_combat_state.energy == ally_combat_state.base_max_energy
