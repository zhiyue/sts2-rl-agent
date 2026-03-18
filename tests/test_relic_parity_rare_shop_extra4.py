"""Focused parity coverage for remaining high-signal rare and event relic hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck, make_inflame
from sts2_env.cards.ironclad_basic import make_bash, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.run.reward_objects import AddCardsReward, RemoveCardReward
from sts2_env.run.run_state import RunState


def _with_owner(cards: list, owner):
    for card in cards:
        card.owner = owner
    return cards


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 901,
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


def test_art_of_war_grants_energy_only_after_turn_without_attacks():
    combat = _make_ironclad_combat(["ArtOfWar"], seed=901)
    enemy = combat.enemies[0]
    enemy.max_hp = 999
    enemy.current_hp = 999

    combat.end_player_turn()
    assert combat.round_number == 2
    assert combat.energy == 4

    combat.hand = _with_owner([make_strike_ironclad()], combat.player)
    combat.energy = 1
    assert combat.play_card(0, 0)

    combat.end_player_turn()
    assert combat.round_number == 3
    assert combat.energy == 3


def test_charons_ashes_deals_damage_to_all_enemies_on_card_exhaust():
    combat = _make_ironclad_combat(["CharonsAshes"], seed=902, enemies=2)
    for enemy in combat.enemies:
        enemy.max_hp = 50
        enemy.current_hp = 50

    card = make_strike_ironclad()
    card.owner = combat.player
    combat.hand = [card]
    combat.exhaust_card(card)

    assert [enemy.current_hp for enemy in combat.enemies] == [47, 47]


def test_demon_tongue_heals_first_unblocked_hit_each_player_turn():
    combat = _make_ironclad_combat(["DemonTongue"], seed=903)
    enemy = combat.enemies[0]
    enemy.max_hp = 999
    enemy.current_hp = 999
    player = combat.player
    player.block = 0

    start_hp = player.current_hp
    combat.deal_damage(enemy, player, 6, ValueProp.UNPOWERED)
    assert player.current_hp == start_hp

    combat.deal_damage(enemy, player, 4, ValueProp.UNPOWERED)
    assert player.current_hp == start_hp - 4

    hp_before_next_turn_hit = player.current_hp
    combat.end_player_turn()
    assert combat.round_number == 2

    player.block = 0
    combat.deal_damage(enemy, player, 3, ValueProp.UNPOWERED)
    assert player.current_hp == hp_before_next_turn_hit


def test_mummified_hand_sets_random_remaining_hand_card_cost_to_zero_on_power_play():
    combat = _make_ironclad_combat(["MummifiedHand"], seed=904)
    enemy = combat.enemies[0]
    enemy.max_hp = 999
    enemy.current_hp = 999

    power_card = make_inflame()
    target_card = make_bash()
    combat.hand = _with_owner([power_card, target_card], combat.player)
    combat.energy = 3
    assert target_card.cost > 0

    assert combat.play_card(0)
    assert target_card.cost == 0


def test_pocketwatch_draw_bonus_depends_on_previous_turn_card_count():
    combat = _make_ironclad_combat(["Pocketwatch"], seed=905)
    enemy = combat.enemies[0]
    enemy.max_hp = 999
    enemy.current_hp = 999

    combat.end_player_turn()
    assert combat.round_number == 2
    assert len(combat.hand) == 8

    combat.hand = _with_owner([make_strike_ironclad() for _ in range(4)], combat.player)
    combat.energy = 4
    for _ in range(4):
        assert combat.play_card(0, 0)

    combat.end_player_turn()
    assert combat.round_number == 3
    assert len(combat.hand) == 5


def test_tungsten_rod_reduces_each_hp_loss_instance_by_one():
    combat = _make_ironclad_combat(["TungstenRod"], seed=906)
    enemy = combat.enemies[0]
    enemy.max_hp = 999
    enemy.current_hp = 999
    player = combat.player
    player.block = 0

    start_hp = player.current_hp
    combat.deal_damage(enemy, player, 1, ValueProp.UNPOWERED)
    combat.deal_damage(enemy, player, 3, ValueProp.UNPOWERED)
    assert player.current_hp == start_hp - 2


def test_arcane_scroll_deferred_followup_queues_add_card_reward():
    run_state = RunState(seed=907, character_id="Ironclad")
    run_state.defer_followup_rewards = True
    starting_deck_size = len(run_state.player.deck)

    assert run_state.player.obtain_relic("ARCANE_SCROLL")
    assert len(run_state.player.deck) == starting_deck_size

    add_rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, AddCardsReward)]
    assert len(add_rewards) == 1
    assert len(add_rewards[0].cards) == 1
    assert add_rewards[0].cards[0].rarity.name == "RARE"


def test_precarious_shears_deck_choice_mode_queues_remove_reward_and_applies_damage():
    run_state = RunState(seed=908, character_id="Ironclad")
    run_state.enable_deck_choice_requests = True
    run_state.player.deck = create_ironclad_starter_deck()
    start_hp = run_state.player.current_hp
    starting_deck_size = len(run_state.player.deck)

    assert run_state.player.obtain_relic("PRECARIOUS_SHEARS")
    assert run_state.player.current_hp == start_hp - 13
    assert len(run_state.player.deck) == starting_deck_size

    remove_rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, RemoveCardReward)]
    assert len(remove_rewards) == 1
    assert remove_rewards[0].count == 2
    assert remove_rewards[0].cards is not None
    assert len(remove_rewards[0].cards) >= 2
