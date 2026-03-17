"""Focused tests for persistent card enchantment support."""

from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.enchantments import can_enchant_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.rest_site import CloneOption
from sts2_env.run.reward_objects import RelicReward
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import PlayerState


def _make_combat(deck, character_id="Ironclad") -> CombatState:
    combat = CombatState(player_hp=80, player_max_hp=80, deck=deck, rng_seed=42, character_id=character_id)
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    return combat


def test_player_state_enchant_helpers_mark_cards():
    player = PlayerState(character_id="Ironclad", deck=create_ironclad_starter_deck())

    enchanted = player.enchant_cards("Clone", 4, 1)
    assert enchanted == 1
    assert any(card.has_enchantment("Clone") for card in player.deck)

    basic_strikes = [card for card in player.deck if card.rarity.name == "BASIC" and "STRIKE" in card.card_id.name]
    player.enchant_basic_strikes("TezcatarasEmber")
    assert all(card.has_enchantment("TezcatarasEmber") for card in basic_strikes)


def test_clone_rest_option_duplicates_clone_enchanted_cards():
    player = PlayerState(character_id="Ironclad", deck=create_ironclad_starter_deck())
    player.enchant_cards("Clone", 4, 2)
    original_size = len(player.deck)

    result = CloneOption().execute(player)

    assert "Cloned" in result
    assert len(player.deck) == original_size + 2


def test_obtain_relic_triggers_after_obtained_enchantments():
    player = PlayerState(character_id="Ironclad", deck=create_ironclad_starter_deck())
    from sts2_env.run.run_state import RunState

    run_state = RunState(seed=53, character_id="Ironclad")
    run_state.player = player
    player.run_state = run_state
    run_state.players = [player]

    assert player.obtain_relic("PAELS_GROWTH")
    assert any(card.has_enchantment("Clone") for card in player.deck)


def test_shop_enchant_relics_apply_expected_enchantments():
    from sts2_env.run.run_state import RunState

    run_state = RunState(seed=61, character_id="Ironclad")
    run_state.initialize_run()
    player = run_state.player
    player.deck = create_ironclad_starter_deck()

    assert player.obtain_relic("BEAUTIFUL_BRACELET")
    assert sum(1 for card in player.deck if card.has_enchantment("Swift")) == 3

    assert player.obtain_relic("KIFUDA")
    assert sum(1 for card in player.deck if card.has_enchantment("Adroit")) == 3

    assert player.obtain_relic("PUNCH_DAGGER")
    assert sum(1 for card in player.deck if card.has_enchantment("Momentum")) == 1

    assert player.obtain_relic("ROYAL_STAMP")
    assert sum(1 for card in player.deck if card.has_enchantment("RoyallyApproved")) == 1


def test_event_enchant_relics_apply_expected_enchantments():
    from sts2_env.run.run_state import RunState

    run_state = RunState(seed=67, character_id="Ironclad")
    run_state.initialize_run()
    player = run_state.player
    player.deck = create_ironclad_starter_deck()

    assert player.obtain_relic("NUTRITIOUS_SOUP")
    basic_strikes = [card for card in player.deck if card.rarity.name == "BASIC" and "STRIKE" in card.card_id.name]
    assert all(card.has_enchantment("TezcatarasEmber") for card in basic_strikes)

    assert player.obtain_relic("PAELS_CLAW")
    assert all(card.has_enchantment("Goopy") for card in player.deck)


def test_relic_reward_can_enqueue_followup_rewards_via_after_obtained():
    mgr = RunManager(seed=59, character_id="Ironclad")
    reward = RelicReward(mgr.run_state.player.player_id, relic_id="CAULDRON")
    mgr._current_reward = reward
    mgr._phase = RunManager.PHASE_CARD_REWARD

    result = mgr._do_relic_reward_pick()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    actions = mgr.get_available_actions()
    assert any(action["action"] == "pick_potion" for action in actions)


def test_relic_reward_can_pause_for_run_level_card_choice():
    mgr = RunManager(seed=83, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    reward = RelicReward(mgr.run_state.player.player_id, relic_id="PAELS_GROWTH")
    mgr._current_reward = reward
    mgr._phase = RunManager.PHASE_CARD_REWARD

    result = mgr._do_relic_reward_pick()
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    actions = mgr.get_available_actions()
    assert any(action["action"] == "choose" for action in actions)

    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["success"] is True
    assert any(card.has_enchantment("Clone") for card in mgr.run_state.player.deck)


def test_shop_relic_purchase_can_pause_for_run_level_card_choice_and_resume_shop():
    mgr = RunManager(seed=89, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_SHOP
    from sts2_env.run.shop import ShopInventory, ShopRelicEntry
    from sts2_env.core.enums import RelicRarity

    mgr._shop_inventory = ShopInventory(
        relics=[ShopRelicEntry(relic_rarity=RelicRarity.RARE, relic_id="PAELS_GROWTH", price=1)]
    )
    mgr.run_state.player.gold = 100

    result = mgr._do_shop_action({"action": "buy_relic", "index": 0})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    actions = mgr.get_available_actions()
    assert any(action["action"] == "choose" for action in actions)

    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_SHOP
    assert any(card.has_enchantment("Clone") for card in mgr.run_state.player.deck)


def test_archaic_tooth_transforms_starter_card_to_mapped_ancient_card():
    from sts2_env.run.run_state import RunState
    from sts2_env.core.enums import CardId

    run_state = RunState(seed=97, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()

    run_state.player.obtain_relic("ARCHAIC_TOOTH")
    assert run_state.pending_choice is None

    assert any(card.card_id == CardId.BREAK for card in run_state.player.deck)


def test_claws_preserves_upgrade_and_enchantments_when_transforming_to_maul():
    from sts2_env.run.run_state import RunState

    run_state = RunState(seed=101, character_id="Ironclad")
    run_state.initialize_run()
    run_state.enable_deck_choice_requests = True
    card = make_strike_ironclad()
    card.upgraded = True
    card.add_enchantment("Clone", 4)
    run_state.player.deck = [card]

    run_state.player.obtain_relic("CLAWS")
    assert run_state.pending_choice is not None
    run_state.resolve_pending_choice(0)
    run_state.resolve_pending_choice(None)

    transformed = run_state.player.deck[0]
    assert transformed.card_id.name == "MAUL"
    assert transformed.upgraded
    assert transformed.has_enchantment("Clone")


def test_run_level_relic_hooks_fire_for_curse_add_and_potion_procure():
    player = PlayerState(character_id="Ironclad", deck=create_ironclad_starter_deck())
    from sts2_env.run.run_state import RunState

    run_state = RunState(seed=79, character_id="Ironclad")
    run_state.player = player
    player.run_state = run_state
    run_state.players = [player]

    start_max_hp = player.max_hp
    player.obtain_relic("DARKSTONE_PERIAPT")
    player.add_random_curses(1)
    assert player.max_hp == start_max_hp + 6

    player.obtain_relic("SOZU")
    held_before = len(player.held_potions())
    assert not player.procure_potion("FirePotion")
    assert len(player.held_potions()) == held_before


def test_runtime_enchantments_modify_combat_behavior():
    combat = _make_combat(create_ironclad_starter_deck())
    enemy = combat.enemies[0]

    strike = make_strike_ironclad()
    strike.add_enchantment("Sharp", 3)
    strike.add_enchantment("Corrupted", 1)
    strike.add_enchantment("Vigorous", 8)
    combat.hand = [strike]
    combat.energy = 1
    starting_hp = combat.player.current_hp

    assert combat.play_card(0, 0)
    assert enemy.current_hp == enemy.max_hp - 25
    assert combat.player.current_hp == starting_hp - 2

    defend = make_defend_ironclad()
    defend.add_enchantment("Nimble", 2)
    defend.add_enchantment("Swift", 2)
    combat.hand = [defend]
    combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad()]
    combat.energy = 1

    assert combat.play_card(0)
    assert combat.player.block >= 7
    assert len(combat.hand) == 2


def test_runtime_enchantments_cover_draw_energy_autoplay_and_replay():
    draw_combat = _make_combat(create_ironclad_starter_deck())
    slither_card = make_strike_ironclad()
    slither_card.add_enchantment("Slither", 1)
    draw_combat.hand.clear()
    draw_combat.draw_pile = [slither_card]
    draw_combat.energy = 3
    draw_combat.draw_cards(draw_combat.player, 1)
    assert 0 <= slither_card.cost <= 3

    sown_combat = _make_combat(create_ironclad_starter_deck())
    sown_card = make_defend_ironclad()
    sown_card.add_enchantment("Sown", 2)
    sown_combat.hand = [sown_card]
    sown_combat.energy = 1
    assert sown_combat.play_card(0)
    assert sown_combat.energy == 2

    imbued_deck = [make_defend_ironclad() for _ in range(5)] + [make_defend_ironclad()]
    imbued_deck[-1].add_enchantment("Imbued", 1)
    imbued_combat = _make_combat(imbued_deck)
    imbued_combat.start_combat()
    assert imbued_combat.player.block > 0

    spiral_combat = _make_combat(create_ironclad_starter_deck())
    spiral_card = make_strike_ironclad()
    spiral_card.add_enchantment("Spiral", 1)
    spiral_combat.hand = [spiral_card]
    spiral_combat.energy = 1
    assert spiral_combat.play_card(0, 0)
    assert spiral_combat.enemies[0].current_hp == spiral_combat.enemies[0].max_hp - 12

    glam_combat = _make_combat(create_ironclad_starter_deck())
    glam_card = make_strike_ironclad()
    glam_card.add_enchantment("Glam", 1)
    glam_combat.hand = [glam_card]
    glam_combat.energy = 1
    assert glam_combat.play_card(0, 0)
    assert glam_combat.enemies[0].current_hp == glam_combat.enemies[0].max_hp - 12
    glam_combat.hand = [glam_card]
    glam_combat.energy = 1
    glam_combat.enemies[0].current_hp = glam_combat.enemies[0].max_hp
    assert glam_combat.play_card(0, 0)
    assert glam_combat.enemies[0].current_hp == glam_combat.enemies[0].max_hp - 6


def test_eternal_cards_are_not_removable_and_perfect_fit_moves_to_top_on_shuffle():
    player = PlayerState(character_id="Ironclad", deck=create_ironclad_starter_deck())
    player.deck[0].add_enchantment("TezcatarasEmber", 1)
    removed = player.remove_cards_from_deck(1)
    assert removed == 1
    assert any(card.has_enchantment("TezcatarasEmber") for card in player.deck)

    combat = _make_combat(create_ironclad_starter_deck())
    perfect_fit = make_strike_ironclad()
    perfect_fit.add_enchantment("PerfectFit", 1)
    combat.draw_pile = []
    combat.discard_pile = [make_defend_ironclad(), perfect_fit, make_strike_ironclad()]
    combat._shuffle_if_needed()
    assert combat.draw_pile[0] is perfect_fit


def test_can_enchant_card_filters_candidates_like_decompiled_rules():
    attack = make_strike_ironclad()
    defend = make_defend_ironclad()
    assert can_enchant_card(attack, "Sharp")
    assert not can_enchant_card(defend, "Sharp")
    assert can_enchant_card(defend, "Nimble")
    assert not can_enchant_card(attack, "Nimble")
    assert can_enchant_card(attack, "Spiral")
    assert can_enchant_card(defend, "Spiral")
    assert can_enchant_card(attack, "TezcatarasEmber")
    assert not can_enchant_card(defend, "TezcatarasEmber")


def test_favored_doubles_powered_attack_damage():
    combat = _make_combat(create_ironclad_starter_deck())
    enemy = combat.enemies[0]
    strike = make_strike_ironclad()
    strike.add_enchantment("Favored", 1)
    combat.hand = [strike]
    combat.energy = 1
    enemy_hp = enemy.current_hp

    assert combat.play_card(0, 0)

    assert enemy.current_hp == enemy_hp - 12


def test_slumbering_essence_reduces_cost_before_flush():
    combat = _make_combat(create_ironclad_starter_deck())
    card = make_strike_ironclad()
    card.add_enchantment("SlumberingEssence", 1)
    combat.hand = [card]

    combat._resolve_end_of_turn_hand()

    assert combat.hand == []
    assert combat.discard_pile[-1] is card
    assert card.cost == 0
