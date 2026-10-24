"""Parity tests for rare/shop/event relic combat-start, map, and autoplay hooks."""

import pytest

import sts2_env.potions  # noqa: F401
import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import make_lift, make_purity
from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.ironclad import create_ironclad_starter_deck, make_inflame
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.status import make_slimed
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import MapPointType, PowerId, RoomType, ValueProp
from sts2_env.core.rng import Rng, deterministic_hash_code
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.potions.base import create_potion
from sts2_env.run.reward_objects import CARD_REWARD_ALTERNATIVE_LIMIT_MESSAGE
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import PlayerState, RunState, UNLOCK_STATE_NUMBER_OF_RUNS_KEY


CARD_REWARD_ALTERNATIVE_LIMIT_SEED = 1130
DRIFTWOOD_RELIC_ID = "DRIFTWOOD"
IRONCLAD_CHARACTER_ID = "Ironclad"
PAELS_WING_RELIC_ID = "PaelsWing"
PAELS_WING_SACRIFICE_ACTION = "sacrifice_card_reward"
PAELS_WING_SACRIFICE_COUNTER_SEED = 1128
PAELS_WING_ACTIVE_TRACKING_SEED = 1129
REGULAR_CARD_REWARD_CONTEXT = "regular"
WONGO_CUSTOMER_BADGE_RELIC_ID = "WONGO_CUSTOMER_APPRECIATION_BADGE"


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
    max_potion_slots: int = 3,
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
        max_potion_slots=max_potion_slots,
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


def test_belt_buckle_removes_dex_when_petrified_toad_procures_late_potion():
    combat = _make_combat(
        character_id="Ironclad",
        relics=["BeltBuckle", "PetrifiedToad"],
        seed=1121,
        max_potion_slots=1,
    )

    assert [p.potion_id for p in combat.held_potions(combat.player)] == ["PotionShapedRock"]
    assert combat.player.get_power_amount(PowerId.DEXTERITY) == 0


def test_belt_buckle_reapplies_dex_after_last_potion_is_used():
    combat = _make_combat(
        character_id="Ironclad",
        relics=["BeltBuckle"],
        seed=1122,
        potions=[create_potion("FirePotion")],
        max_potion_slots=1,
    )

    assert combat.player.get_power_amount(PowerId.DEXTERITY) == 0
    assert combat.use_potion(0, target_index=0)
    assert combat.player.get_power_amount(PowerId.DEXTERITY) == 2


def test_belt_buckle_applies_dex_when_obtained_during_combat_without_potions():
    combat = _make_combat(character_id="Ironclad", relics=[], seed=1123, max_potion_slots=1)

    assert combat.current_player_state.player_state.obtain_relic("BeltBuckle")

    assert combat.player.get_power_amount(PowerId.DEXTERITY) == 2


def test_fake_snecko_eye_applies_confused_when_obtained_during_combat():
    combat = _make_combat(character_id="Ironclad", relics=[], seed=1124)

    assert combat.current_player_state.player_state.obtain_relic("FakeSneckoEye")

    assert combat.player.get_power_amount(PowerId.CONFUSED) == 1


def test_snecko_eye_applies_confused_when_obtained_during_combat():
    combat = _make_combat(character_id="Ironclad", relics=[], seed=1129)

    assert combat.current_player_state.player_state.obtain_relic("SneckoEye")

    assert combat.player.get_power_amount(PowerId.CONFUSED) == 1


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


def test_the_boot_raises_intangible_damage_cap_to_five():
    combat = _make_combat(character_id="Ironclad", relics=["TheBoot"], seed=1130)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 200
    enemy.current_hp = 200
    enemy.block = 0
    enemy.apply_power(PowerId.INTANGIBLE, 1)

    combat.deal_damage(player, enemy, 10, ValueProp.MOVE)

    assert enemy.current_hp == 195


def test_the_boot_runs_before_tungsten_rod_regardless_of_relic_order():
    from sts2_env.core.hooks import modify_hp_lost

    combat = _make_combat(character_id="Ironclad", relics=["TungstenRod", "TheBoot"], seed=1112)
    player = combat.player

    assert modify_hp_lost(1, player, player, ValueProp.MOVE, combat) == 4


def test_the_boot_covers_owner_osty_dealer_but_not_other_pets():
    """C# TheBoot.cs:28-29: dealer may be the owner's Creature OR the owner's
    Osty pet; unrelated pets do not qualify."""
    combat = _make_combat(character_id="Ironclad", relics=["TheBoot"], seed=1140)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 200
    enemy.current_hp = 200
    enemy.block = 0

    osty = combat.summon_osty(player, 1)
    assert osty is not None

    start_hp = enemy.current_hp
    combat.deal_damage(osty, enemy, 1, ValueProp.MOVE)
    assert enemy.current_hp == start_hp - 5

    # A non-Osty pet owned by the player is not covered by TheBoot.
    pet = combat.summon_event_pet(player, "PAELS_LEGION")
    if pet is not None:
        next_hp = enemy.current_hp
        combat.deal_damage(pet, enemy, 1, ValueProp.MOVE)
        assert enemy.current_hp == next_hp - 1


def test_touch_of_orobas_upgrades_existing_starter_relic():
    run_state = RunState(seed=1108, character_id="Ironclad")
    assert run_state.player.obtain_relic("BURNING_BLOOD")
    assert "BURNING_BLOOD" in run_state.player.relics

    assert run_state.player.obtain_relic("TOUCH_OF_OROBAS")
    assert "BLACK_BLOOD" in run_state.player.relics
    assert "BURNING_BLOOD" not in run_state.player.relics


def test_golden_compass_regenerates_current_act_as_golden_path():
    run_state = RunState(seed=1126, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.unlock_state[UNLOCK_STATE_NUMBER_OF_RUNS_KEY] = 1

    assert run_state.player.obtain_relic("GoldenCompass")

    room_points = run_state.map.room_points()
    assert [point.col for point in room_points] == [3] * 16
    assert [point.point_type for point in room_points] == [
        MapPointType.MONSTER,
        MapPointType.UNKNOWN,
        MapPointType.MONSTER,
        MapPointType.REST_SITE,
        MapPointType.MONSTER,
        MapPointType.REST_SITE,
        MapPointType.UNKNOWN,
        MapPointType.TREASURE,
        MapPointType.UNKNOWN,
        MapPointType.TREASURE,
        MapPointType.UNKNOWN,
        MapPointType.SHOP,
        MapPointType.ELITE,
        MapPointType.REST_SITE,
        MapPointType.ELITE,
        MapPointType.REST_SITE,
    ]
    assert run_state.resolve_room_type(MapPointType.UNKNOWN) == RoomType.EVENT


def test_fur_coat_marks_combat_rooms_and_sets_marked_enemies_to_one_hp():
    run_state = RunState(seed=1127, character_id="Ironclad")
    run_state.initialize_run()

    assert run_state.player.obtain_relic("FurCoat")
    fur_coat = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "FUR_COAT")
    marked = fur_coat._marked_map_coords()
    candidates = [
        point for point in run_state.map.room_points()
        if point.point_type in (MapPointType.MONSTER, MapPointType.ELITE)
    ]
    rng = Rng(run_state.rng.seed + run_state.player.player_id + deterministic_hash_code("FurCoat"))
    rng.shuffle(candidates)
    expected = [(point.col, point.row) for point in candidates[:7]]

    assert len(marked) == 7
    assert [(coord.col, coord.row) for coord in marked] == expected
    assert all(
        run_state.map.get_point(coord).point_type in (MapPointType.MONSTER, MapPointType.ELITE)
        for coord in marked
    )

    run_state.visited_map_coords = [marked[0]]
    combat = CombatState(
        player_hp=run_state.player.current_hp,
        player_max_hp=run_state.player.max_hp,
        deck=list(run_state.player.deck),
        rng_seed=1127,
        character_id="Ironclad",
        player_state=run_state.player,
    )
    enemy, ai = create_shrinker_beetle(Rng(1127))
    enemy.max_hp = 100
    enemy.current_hp = 100
    combat.add_enemy(enemy, ai)

    combat.start_combat()

    assert enemy.current_hp == 1

    # C# FurCoat.cs:130-142 AfterCreatureAddedToCombat: an enemy added to a
    # marked combat mid-fight is also set to 1 HP.
    late_enemy, late_ai = create_twig_slime_s(Rng(1128))
    late_enemy.max_hp = 50
    late_enemy.current_hp = 50
    combat.add_enemy(late_enemy, late_ai)
    assert late_enemy.current_hp == 1

    # Unmarked rooms do not set late arrivals to 1 HP.
    unmarked_point = next(
        point for point in run_state.map.room_points()
        if point.point_type in (MapPointType.MONSTER, MapPointType.ELITE)
        and point.coord != marked[0]
    )
    run_state.visited_map_coords = [unmarked_point.coord]
    other_combat = CombatState(
        player_hp=run_state.player.current_hp,
        player_max_hp=run_state.player.max_hp,
        deck=list(run_state.player.deck),
        rng_seed=1129,
        character_id="Ironclad",
        player_state=run_state.player,
    )
    other_enemy, other_ai = create_shrinker_beetle(Rng(1129))
    other_enemy.max_hp = 50
    other_enemy.current_hp = 50
    other_combat.add_enemy(other_enemy, other_ai)
    other_combat.start_combat()
    late2, late2_ai = create_twig_slime_s(Rng(1130))
    late2.max_hp = 50
    late2.current_hp = 50
    other_combat.add_enemy(late2, late2_ai)
    assert late2.current_hp == 50


def test_paels_wing_sacrifices_card_rewards_and_obtains_relic_every_two():
    mgr = RunManager(seed=PAELS_WING_SACRIFICE_COUNTER_SEED, character_id=IRONCLAD_CHARACTER_ID)
    player = mgr.run_state.player
    assert player.obtain_relic(PAELS_WING_RELIC_ID)
    player.relic_grab_bag = [WONGO_CUSTOMER_BADGE_RELIC_ID]
    player.relic_grab_bag_by_rarity = {}
    player.relic_grab_bag_fallback = [WONGO_CUSTOMER_BADGE_RELIC_ID]

    mgr._enter_card_reward(context=REGULAR_CARD_REWARD_CONTEXT)
    assert any(action["action"] == PAELS_WING_SACRIFICE_ACTION for action in mgr.get_available_actions())
    first = mgr.take_action({"action": PAELS_WING_SACRIFICE_ACTION})

    assert first["success"] is True
    assert first["obtained_relic_id"] is None
    assert WONGO_CUSTOMER_BADGE_RELIC_ID not in player.relics

    mgr._enter_card_reward(context=REGULAR_CARD_REWARD_CONTEXT)
    second = mgr.take_action({"action": PAELS_WING_SACRIFICE_ACTION})

    assert second["success"] is True
    assert second["obtained_relic_id"] == WONGO_CUSTOMER_BADGE_RELIC_ID
    assert WONGO_CUSTOMER_BADGE_RELIC_ID in player.relics


def test_paels_wing_sacrifice_removes_current_card_reward_from_active_tracking():
    mgr = RunManager(seed=PAELS_WING_ACTIVE_TRACKING_SEED, character_id=IRONCLAD_CHARACTER_ID)
    player = mgr.run_state.player
    assert player.obtain_relic(PAELS_WING_RELIC_ID)

    mgr._enter_card_reward(context=REGULAR_CARD_REWARD_CONTEXT)
    sacrificed_reward = mgr._current_reward
    assert sacrificed_reward in mgr.run_state.active_card_rewards

    result = mgr.take_action({"action": PAELS_WING_SACRIFICE_ACTION})

    assert result["success"] is True
    assert sacrificed_reward not in mgr.run_state.active_card_rewards


def test_card_reward_alternative_limit_rejects_skip_reroll_and_paels_wing_sacrifice():
    mgr = RunManager(seed=CARD_REWARD_ALTERNATIVE_LIMIT_SEED, character_id=IRONCLAD_CHARACTER_ID)
    player = mgr.run_state.player
    assert player.obtain_relic(DRIFTWOOD_RELIC_ID)
    assert player.obtain_relic(PAELS_WING_RELIC_ID)

    with pytest.raises(ValueError, match=CARD_REWARD_ALTERNATIVE_LIMIT_MESSAGE):
        mgr._enter_card_reward(context=REGULAR_CARD_REWARD_CONTEXT)


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


def test_whispering_earring_autoplays_opening_hand_and_spends_energy():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[make_strike_ironclad(), make_defend_ironclad()],
        rng_seed=1125,
        character_id="Ironclad",
        relics=["WhisperingEarring"],
    )
    enemy, ai = create_shrinker_beetle(Rng(1125))
    enemy.max_hp = 100
    enemy.current_hp = 100
    combat.add_enemy(enemy, ai)

    combat.start_combat()

    assert enemy.current_hp == 94
    assert combat.player.block == 5
    assert combat.energy == 2
    assert combat.hand == []
    assert len(combat.discard_pile) == 2


def test_whispering_earring_uses_combat_targets_rng_for_ally_targets():
    class SecondChoiceRng:
        def __init__(self):
            self.seen = None

        def choice(self, items):
            self.seen = list(items)
            return self.seen[1]

    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[],
        rng_seed=1126,
        character_id="Ironclad",
        relics=["WhisperingEarring"],
    )
    first_ally = combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60))
    second_ally = combat.add_ally_player(PlayerState(player_id=3, character_id="Ironclad", max_hp=60, current_hp=60))
    card = make_lift()
    card.owner = combat.player
    card.cost = 0
    card.base_block = 8
    combat.hand = [card]
    combat.energy = 0
    combat.rng = SecondChoiceRng()

    combat.relics[0].before_play_phase_start(combat.player, combat.player, combat)

    assert combat.rng.seen == [first_ally, second_ally]
    assert first_ally.block == 0
    assert second_ally.block == 8


def test_whispering_earring_vakuu_card_selector_auto_resolves_card_choices():
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[],
        rng_seed=1127,
        character_id="Ironclad",
        relics=["WhisperingEarring"],
    )
    selected_first = make_defend_ironclad()
    selected_second = make_strike_ironclad()
    selected_third = make_slimed()
    ignored_fourth = make_lift()
    player = combat.player
    combat.hand = _with_owner(
        [make_purity(), selected_first, selected_second, selected_third, ignored_fourth],
        player,
    )
    combat.energy = 1

    combat.relics[0].before_play_phase_start(player, player, combat)

    assert combat.pending_choice is None
    assert selected_first in combat.exhaust_pile
    assert selected_second in combat.exhaust_pile
    assert selected_third in combat.exhaust_pile
    assert ignored_fourth not in combat.exhaust_pile
    assert ignored_fourth in combat.hand
