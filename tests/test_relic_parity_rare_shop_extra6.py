"""Focused parity coverage for additional uncovered rare/shop/event relic hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.ironclad import create_ironclad_starter_deck, make_inflame
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.run.run_state import RunState


def _with_owner(cards: list, owner):
    for card in cards:
        card.owner = owner
    return cards


def _starter_deck(character_id: str):
    if character_id == "Defect":
        return create_defect_starter_deck()
    return create_ironclad_starter_deck()


def _make_combat(
    *,
    character_id: str,
    relics: list[str] | None = None,
    seed: int = 1101,
    enemies: int = 1,
    potions: list[object] | None = None,
    start: bool = True,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=_starter_deck(character_id),
        rng_seed=seed,
        character_id=character_id,
        relics=relics or [],
        potions=potions,
    )
    for i in range(enemies):
        if i == 0:
            creature, ai = create_shrinker_beetle(Rng(seed + i))
        else:
            creature, ai = create_twig_slime_s(Rng(seed + i))
        combat.add_enemy(creature, ai)
    if start:
        combat.start_combat()
    return combat


def test_belt_buckle_applies_dex_only_when_no_potions_are_held():
    no_potions = _make_combat(character_id="Ironclad", relics=["BeltBuckle"], seed=1101)
    assert no_potions.player.get_power_amount(PowerId.DEXTERITY) == 2

    with_potions = _make_combat(
        character_id="Ironclad",
        relics=["BeltBuckle"],
        seed=1102,
        potions=[object()],
    )
    assert with_potions.player.get_power_amount(PowerId.DEXTERITY) == 0


def test_brimstone_applies_strength_to_player_and_all_enemies_each_player_turn():
    combat = _make_combat(character_id="Ironclad", relics=["Brimstone"], seed=1103, enemies=2)

    assert combat.player.get_power_amount(PowerId.STRENGTH) == 2
    assert [enemy.get_power_amount(PowerId.STRENGTH) for enemy in combat.enemies] == [1, 1]

    combat.end_player_turn()
    assert combat.player.get_power_amount(PowerId.STRENGTH) == 4
    assert [enemy.get_power_amount(PowerId.STRENGTH) for enemy in combat.enemies] == [2, 2]


def test_burning_sticks_clones_only_first_exhausted_skill_per_combat():
    combat = _make_combat(character_id="Ironclad", relics=["BurningSticks"], seed=1104)
    player = combat.player

    initial_hand_size = len(combat.hand)
    first_skill = make_defend_ironclad()
    first_skill.owner = player
    combat.exhaust_card(first_skill)
    assert len(combat.hand) == initial_hand_size + 1
    assert combat.hand[-1].card_id == first_skill.card_id
    assert combat.hand[-1] is not first_skill

    second_skill = make_defend_ironclad()
    second_skill.owner = player
    combat.exhaust_card(second_skill)
    assert len(combat.hand) == initial_hand_size + 1


def test_game_piece_draws_after_power_play_only():
    combat = _make_combat(character_id="Ironclad", relics=["GamePiece"], seed=1105)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 999
    enemy.current_hp = 999

    drawn_from_power = make_defend_ironclad()
    drawn_from_power.owner = player
    combat.draw_pile = [drawn_from_power]
    combat.hand = _with_owner([make_inflame()], player)
    combat.energy = 3
    assert combat.play_card(0)
    assert len(combat.hand) == 1
    assert combat.hand[0].card_id == drawn_from_power.card_id

    drawn_after_attack = make_defend_ironclad()
    drawn_after_attack.owner = player
    combat.draw_pile = [drawn_after_attack]
    combat.hand = _with_owner([make_strike_ironclad()], player)
    combat.energy = 1
    assert combat.play_card(0, 0)
    assert drawn_after_attack in combat.draw_pile
    assert not combat.hand


def test_runic_capacitor_adds_three_orb_slots_on_round_one_only():
    combat = _make_combat(character_id="Defect", relics=["RunicCapacitor"], seed=1106, start=False)
    assert combat.orb_queue is not None
    base_capacity = combat.orb_queue.capacity

    combat.start_combat()
    assert combat.round_number == 1
    assert combat.orb_queue is not None
    assert combat.orb_queue.capacity == base_capacity + 3

    combat.end_player_turn()
    assert combat.round_number == 2
    assert combat.orb_queue is not None
    assert combat.orb_queue.capacity == base_capacity + 3


def test_the_boot_raises_only_powered_attack_damage_below_five():
    combat = _make_combat(character_id="Ironclad", relics=["TheBoot"], seed=1107)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 200
    enemy.current_hp = 200
    enemy.block = 0

    start_hp = enemy.current_hp
    combat.deal_damage(player, enemy, 1, ValueProp.MOVE)
    assert enemy.current_hp == start_hp - 5

    next_hp = enemy.current_hp
    combat.deal_damage(player, enemy, 1, ValueProp.MOVE | ValueProp.UNPOWERED)
    assert enemy.current_hp == next_hp - 1


def test_touch_of_orobas_upgrades_existing_starter_relic():
    run_state = RunState(seed=1108, character_id="Ironclad")
    assert run_state.player.obtain_relic("BURNING_BLOOD")
    assert "BURNING_BLOOD" in run_state.player.relics

    assert run_state.player.obtain_relic("TOUCH_OF_OROBAS")
    assert "BLACK_BLOOD" in run_state.player.relics
    assert "BURNING_BLOOD" not in run_state.player.relics


def test_unceasing_top_draws_when_hand_is_emptied_during_play_phase():
    combat = _make_combat(character_id="Ironclad", relics=["UnceasingTop"], seed=1109)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 999
    enemy.current_hp = 999

    drawn_card = make_defend_ironclad()
    drawn_card.owner = player
    combat.draw_pile = [drawn_card]
    combat.hand = _with_owner([make_strike_ironclad()], player)
    combat.energy = 1

    assert combat.play_card(0, 0)
    assert len(combat.hand) == 1
    assert combat.hand[0].card_id == drawn_card.card_id
