"""Regression tests for relic-driven reward and card-reward hooks."""

from sts2_env.core.combat import CombatState
from sts2_env.cards.factory import create_card
from sts2_env.cards.factory import eligible_character_cards, eligible_registered_cards
from sts2_env.characters.all import get_character
from sts2_env.core.card_pools import CardPoolId
from sts2_env.core.enums import CardId, CardRarity, CardType, RelicRarity, RoomType, ValueProp
from sts2_env.run.reward_objects import (
    AddCardsReward,
    CardBundlesReward,
    CardReward,
    GoldReward,
    ObtainRelicsReward,
    PotionReward,
    RelicReward,
    RewardsSet,
)
from sts2_env.run.rooms import create_room
from sts2_env.run.shop import generate_shop_inventory
from sts2_env.run.run_state import PlayerState, RunState
from sts2_env.run.run_manager import RunManager
from sts2_env.run.modifiers import CharacterCardsModifier
from sts2_env.run.rewards import CARD_CREATION_SOURCE_OTHER


IRONCLAD_CHARACTER_ID = "Ironclad"
SILENT_CHARACTER_ID = "Silent"
FROZEN_EGG_RELIC_NAME = "FROZEN_EGG"
LASTING_CANDY_RELIC_NAME = "LASTING_CANDY"
SILVER_CRUCIBLE_RELIC_NAME = "SILVER_CRUCIBLE"
LASTING_CANDY_TRIGGERING_COMBATS_SEEN = 2
FIRST_TREASURE_ROOM_COUNT = 1
MANUAL_REWARD_PARITY_SEED = 218
EMPTY_MANUAL_REWARD_CARD_COUNT = 0
ACTIVE_REWARD_RELIC_UPDATE_SEED = 107
DRIFTWOOD_REWARD_SET_HOOK_SEED = 206
SILVER_CRUCIBLE_TREASURE_SKIP_SEED = 210
SILVER_CRUCIBLE_CARD_REWARD_LIMIT_SEED = 211
SILVER_CRUCIBLE_REROLL_SEED = 212
FRESNEL_LENS_RELIC_NAME = "FRESNEL_LENS"
FRESNEL_LENS_NIMBLE_AMOUNT = 2
ALLY_PLAYER_ID = 2


def test_prayer_wheel_adds_one_extra_card_reward_only_once():
    run_state = RunState(seed=101, character_id="Ironclad")
    assert run_state.player.obtain_relic("PRAYER_WHEEL")
    room = create_room(RoomType.MONSTER)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
    generated_once = rewards.generate_without_offering(run_state)
    generated_twice = rewards.generate_without_offering(run_state)

    card_rewards = [reward for reward in generated_once if isinstance(reward, CardReward)]
    assert len(card_rewards) == 2
    assert len([reward for reward in generated_twice if isinstance(reward, CardReward)]) == 2


def test_white_star_adds_boss_card_reward_after_elite():
    run_state = RunState(seed=102, character_id="Ironclad")
    assert run_state.player.obtain_relic("WHITE_STAR")
    room = create_room(RoomType.ELITE)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
    generated = rewards.generate_without_offering(run_state)

    card_contexts = [reward.context for reward in generated if isinstance(reward, CardReward)]
    assert "elite" in card_contexts
    assert "boss" in card_contexts


def test_black_star_adds_extra_relic_reward_after_elite():
    run_state = RunState(seed=103, character_id="Ironclad")
    assert run_state.player.obtain_relic("BLACK_STAR")
    room = create_room(RoomType.ELITE)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
    generated = rewards.generate_without_offering(run_state)

    assert len([reward for reward in generated if isinstance(reward, RelicReward)]) == 2


def test_white_beast_statue_forces_potion_reward():
    run_state = RunState(seed=104, character_id="Ironclad")
    assert run_state.player.obtain_relic("WHITE_BEAST_STATUE")
    run_state.potion_reward_odds.current_value = -1.0
    room = create_room(RoomType.MONSTER)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
    generated = rewards.generate_without_offering(run_state)

    assert any(isinstance(reward, PotionReward) for reward in generated)
    assert run_state.potion_reward_odds.current_value == -1.1


def test_white_beast_statue_does_not_force_noncombat_potion_rewards():
    run_state = RunState(seed=1041, character_id="Ironclad")
    assert run_state.player.obtain_relic("WHITE_BEAST_STATUE")
    relic = run_state.player.relic_objects[-1]

    assert relic.should_force_potion_reward(run_state.player, create_room(RoomType.TREASURE)) is False


def test_glass_eye_enqueues_five_single_rarity_card_rewards():
    run_state = RunState(seed=105, character_id="Ironclad")
    assert run_state.player.obtain_relic("GLASS_EYE")

    assert len(run_state.pending_rewards) == 5
    assert all(isinstance(reward, CardReward) for reward in run_state.pending_rewards)

    expected_rarities = (
        CardRarity.COMMON,
        CardRarity.COMMON,
        CardRarity.UNCOMMON,
        CardRarity.UNCOMMON,
        CardRarity.RARE,
    )
    for reward, rarity in zip(run_state.pending_rewards, expected_rarities):
        assert reward.forced_rarities == ()
        assert reward.generation_context is None
        assert reward.roll_upgrade is False
        assert reward.card_creation_source == CARD_CREATION_SOURCE_OTHER
        assert reward.allow_rarity_modifications is False
        assert reward.card_pool_rarity_filter is rarity
        assert reward.use_uniform_noncombat_odds is True
        reward.populate(run_state, None)
        assert len(reward.cards) == 3
        assert all(card.rarity == rarity for card in reward.cards)
        assert all(card.upgraded is False for card in reward.cards)


def test_lava_rock_adds_two_relic_rewards_only_to_act_one_boss_once():
    run_state = RunState(seed=109, character_id="Ironclad")
    assert run_state.player.obtain_relic("LAVA_ROCK")
    room = create_room(RoomType.BOSS)

    first = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state).generate_without_offering(run_state)
    second = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state).generate_without_offering(run_state)

    assert len([reward for reward in first if isinstance(reward, RelicReward)]) == 2
    assert len([reward for reward in second if isinstance(reward, RelicReward)]) == 0


def test_silver_crucible_upgrades_three_card_rewards_and_skips_first_treasure():
    mgr = RunManager(seed=SILVER_CRUCIBLE_TREASURE_SKIP_SEED, character_id=IRONCLAD_CHARACTER_ID)
    run_state = mgr.run_state
    assert run_state.player.obtain_relic(SILVER_CRUCIBLE_RELIC_NAME)
    crucible = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == SILVER_CRUCIBLE_RELIC_NAME)

    for _ in range(3):
        reward = CardReward(
            run_state.player.player_id,
            cards=[
                create_card(CardId.ANGER),
                create_card(CardId.SHRUG_IT_OFF),
                create_card(CardId.INFLAME),
            ],
        )
        reward.populate(run_state, None)
        assert all(card.upgraded for card in reward.cards)

    mgr._enter_room(RoomType.TREASURE)

    assert mgr.phase == RunManager.PHASE_MAP_CHOICE
    assert crucible.enabled is False


def test_silver_crucible_counts_treasure_room_type_without_string_name_lookup():
    run_state = RunState(seed=SILVER_CRUCIBLE_TREASURE_SKIP_SEED, character_id=IRONCLAD_CHARACTER_ID)
    assert run_state.player.obtain_relic(SILVER_CRUCIBLE_RELIC_NAME)
    crucible = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == SILVER_CRUCIBLE_RELIC_NAME)

    crucible.after_room_entered(run_state.player, RoomType.TREASURE)

    assert crucible._treasure_rooms_entered == FIRST_TREASURE_ROOM_COUNT
    assert crucible.should_generate_treasure(run_state.player) is False


def test_toy_box_wax_rewards_melt_after_three_combats():
    mgr = RunManager(seed=211, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("TOY_BOX")

    wax_rewards = [reward for reward in mgr.run_state.pending_rewards if isinstance(reward, RelicReward)]
    assert len(wax_rewards) == 4
    assert all(reward.is_wax for reward in wax_rewards)

    first_reward = wax_rewards[0]
    first_reward.populate(mgr.run_state, None)
    reward_result = first_reward.select(mgr)
    assert reward_result["relic_id"]

    wax_relic = mgr.run_state.player.relic_objects[-1]
    assert getattr(wax_relic, "is_wax", False) is True
    assert getattr(wax_relic, "is_melted", False) is False

    for combat_seed in (212, 213, 214):
        combat = CombatState(
            player_hp=mgr.run_state.player.current_hp,
            player_max_hp=mgr.run_state.player.max_hp,
            deck=list(mgr.run_state.player.deck),
            rng_seed=combat_seed,
            character_id=mgr.run_state.player.character_id,
            player_state=mgr.run_state.player,
        )
        toy_box = next(relic for relic in combat.current_player_state.relics if relic.relic_id.name == "TOY_BOX")
        toy_box.after_combat_end(combat.player, combat)

    assert getattr(wax_relic, "is_melted", False) is True
    assert wax_relic.enabled is False


def test_rest_site_heal_relics_enqueue_rewards_and_modify_heal():
    mgr = RunManager(seed=205, character_id="Ironclad")
    mgr.run_state.player.current_hp = 20
    assert mgr.run_state.player.obtain_relic("DREAM_CATCHER")
    assert mgr.run_state.player.obtain_relic("TINY_MAILBOX")
    assert mgr.run_state.player.obtain_relic("REGAL_PILLOW")
    assert mgr.run_state.player.obtain_relic("STONE_HUMIDIFIER")

    mgr._enter_rest_site()
    result = mgr._do_rest_site({"action": "rest_option", "option_id": "HEAL"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.player.current_hp == 64
    assert mgr.run_state.player.max_hp == 85
    actions = mgr.get_available_actions()
    assert any(action["action"] == "pick_card" for action in actions)

    mgr.take_action({"action": "skip"})
    actions = mgr.get_available_actions()
    assert any(action["action"] == "pick_potion" for action in actions)


def test_molten_egg_upgrades_attack_reward_cards_and_future_deck_additions():
    run_state = RunState(seed=106, character_id="Ironclad")
    assert run_state.player.obtain_relic("MOLTEN_EGG")

    reward = CardReward(
        run_state.player.player_id,
        cards=[
            create_card(CardId.ANGER),
            create_card(CardId.SHRUG_IT_OFF),
            create_card(CardId.INFLAME),
        ],
    )
    reward.populate(run_state, None)

    assert reward.cards[0].upgraded
    assert not reward.cards[1].upgraded
    assert not reward.cards[2].upgraded

    added = create_card(CardId.ANGER)
    run_state.player.add_card_instance_to_deck(added)
    assert run_state.player.deck[-1].upgraded


def test_fresnel_lens_and_glitter_modify_card_reward_options_late():
    run_state = RunState(seed=107, character_id="Ironclad")
    assert run_state.player.obtain_relic("FRESNEL_LENS")
    assert run_state.player.obtain_relic("GLITTER")

    reward = CardReward(
        run_state.player.player_id,
        cards=[
            create_card(CardId.SHRUG_IT_OFF),
            create_card(CardId.ANGER),
        ],
    )
    reward.populate(run_state, None)

    by_id = {card.card_id: card for card in reward.cards}
    assert by_id[CardId.SHRUG_IT_OFF].enchantments["Nimble"] == 2
    assert "Glam" not in by_id[CardId.SHRUG_IT_OFF].enchantments
    assert "Nimble" not in by_id[CardId.ANGER].enchantments
    assert by_id[CardId.ANGER].enchantments["Glam"] == 1


def test_new_relic_updates_already_populated_card_rewards():
    mgr = RunManager(seed=ACTIVE_REWARD_RELIC_UPDATE_SEED, character_id=IRONCLAD_CHARACTER_ID)
    card_reward = CardReward(
        mgr.run_state.player.player_id,
        option_count=1,
        forced_rarities=(CardRarity.COMMON,),
        custom_card_ids=(CardId.SHRUG_IT_OFF,),
        has_custom_card_pool=True,
    )
    card_reward.populate(mgr.run_state, None)
    mgr._pending_rewards = [card_reward]
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id=FRESNEL_LENS_RELIC_NAME,
    )
    mgr._phase = RunManager.PHASE_CARD_REWARD
    mgr._prime_next_relic_reward()

    assert "Nimble" not in card_reward.cards[0].enchantments

    mgr.take_action({"action": "pick_relic_reward", "relic_id": FRESNEL_LENS_RELIC_NAME})

    assert isinstance(mgr._current_reward, CardReward)
    assert mgr._offered_cards[0].enchantments["Nimble"] == FRESNEL_LENS_NIMBLE_AMOUNT


def test_new_relic_only_updates_same_players_active_card_rewards():
    run_state = RunState(seed=ACTIVE_REWARD_RELIC_UPDATE_SEED, character_id=IRONCLAD_CHARACTER_ID)
    ally = run_state.add_player(PlayerState(player_id=ALLY_PLAYER_ID, character_id=IRONCLAD_CHARACTER_ID))
    card_reward = CardReward(
        run_state.player.player_id,
        option_count=1,
        forced_rarities=(CardRarity.COMMON,),
        custom_card_ids=(CardId.SHRUG_IT_OFF,),
        has_custom_card_pool=True,
    )
    card_reward.populate(run_state, None)

    assert ally.obtain_relic(FRESNEL_LENS_RELIC_NAME)

    assert "Nimble" not in card_reward.cards[0].enchantments


def test_driftwood_allows_rerolling_card_rewards():
    mgr = RunManager(seed=206, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("DRIFTWOOD")
    mgr._enter_card_reward(context="regular")

    original_ids = [card.card_id for card in mgr._offered_cards]
    actions = mgr.get_available_actions()
    assert any(action["action"] == "reroll_card_reward" for action in actions)

    result = mgr.take_action({"action": "reroll_card_reward"})

    assert result["success"] is True
    assert [card.card_id for card in mgr._offered_cards] != original_ids
    assert not any(action["action"] == "reroll_card_reward" for action in mgr.get_available_actions())


def test_driftwood_reroll_is_added_by_rewards_set_late_hook_not_direct_populate():
    run_state = RunState(seed=DRIFTWOOD_REWARD_SET_HOOK_SEED, character_id=IRONCLAD_CHARACTER_ID)
    assert run_state.player.obtain_relic("DRIFTWOOD")
    room = create_room(RoomType.MONSTER)
    direct_reward = CardReward(run_state.player.player_id)

    direct_reward.populate(run_state, room)

    assert direct_reward.rerolls_remaining == 0

    rewards = RewardsSet(run_state.player.player_id, room=room).with_custom_rewards([
        CardReward(run_state.player.player_id),
    ])
    generated = rewards.generate_without_offering(run_state)
    set_reward = next(reward for reward in generated if isinstance(reward, CardReward))

    assert set_reward.rerolls_remaining == 1


def test_miniature_tent_keeps_rest_site_open_after_heal_reward_resolution():
    mgr = RunManager(seed=207, character_id="Ironclad")
    mgr.run_state.player.current_hp = 30
    assert mgr.run_state.player.obtain_relic("MINIATURE_TENT")
    assert mgr.run_state.player.obtain_relic("DREAM_CATCHER")

    mgr._enter_rest_site()
    result = mgr._do_rest_site({"action": "rest_option", "option_id": "HEAL"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    mgr.take_action({"action": "skip"})
    assert mgr.phase == RunManager.PHASE_REST_SITE
    remaining_ids = {action["option_id"] for action in mgr.get_available_actions()}
    assert "HEAL" not in remaining_ids
    assert "SMITH" in remaining_ids


def test_prismatic_gem_expands_card_rewards_beyond_owner_character_pool():
    run_state = RunState(seed=108, character_id="Ironclad")
    assert run_state.player.obtain_relic("PRISMATIC_GEM")

    reward = CardReward(run_state.player.player_id, option_count=12)
    reward.populate(run_state, None)

    ironclad_pool = set(get_character("Ironclad").card_pool)
    assert any(card.card_id not in ironclad_pool for card in reward.cards)


def test_driftwood_allows_single_card_reward_reroll():
    from sts2_env.run.run_manager import RunManager

    mgr = RunManager(seed=401, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("DRIFTWOOD")
    mgr._enter_card_reward(context="regular")

    before = [card.card_id.name for card in mgr._offered_cards]
    assert any(action["action"] == "reroll_card_reward" for action in mgr.get_available_actions())

    result = mgr.take_action({"action": "reroll_card_reward"})

    after = [card.card_id.name for card in mgr._offered_cards]
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert result["rerolls_remaining"] == 0
    assert before != after


def test_merchant_card_creation_hooks_upgrade_and_enchant_cards_for_sale():
    run_state = RunState(seed=200, character_id="Ironclad")
    assert run_state.player.obtain_relic("MOLTEN_EGG")
    assert run_state.player.obtain_relic("FRESNEL_LENS")

    inventory = generate_shop_inventory(run_state)
    attack_entries = [entry for entry in inventory.cards if entry.card is not None and entry.card.card_type.name == "ATTACK"]
    block_entries = [entry for entry in inventory.cards if entry.card is not None and entry.card.base_block is not None]
    nonblock_entries = [entry for entry in inventory.cards if entry.card is not None and entry.card.base_block is None]

    assert attack_entries
    assert any(entry.card.upgraded for entry in attack_entries)
    assert block_entries
    assert all(entry.card.enchantments.get("Nimble") == 2 for entry in block_entries)
    assert all("Nimble" not in entry.card.enchantments for entry in nonblock_entries)


def test_scroll_boxes_enqueues_choose_one_card_bundle_reward():
    run_state = RunState(seed=213, character_id="Ironclad")
    starting_gold = run_state.player.gold

    assert run_state.player.obtain_relic("SCROLL_BOXES")

    bundles = [reward for reward in run_state.pending_rewards if isinstance(reward, CardBundlesReward)]
    assert len(bundles) == 1
    assert run_state.player.gold == starting_gold
    assert len(bundles[0].bundles) == 2
    assert all(len(bundle) == 3 for bundle in bundles[0].bundles)
    assert all(
        [card.rarity for card in bundle] == [CardRarity.COMMON, CardRarity.COMMON, CardRarity.UNCOMMON]
        for bundle in bundles[0].bundles
    )
    assert len({card.card_id for bundle in bundles[0].bundles for card in bundle}) == 6


def test_scroll_boxes_applies_dingy_rug_card_pool_hook_to_bundle_candidates():
    run_state = RunState(seed=2, character_id="Ironclad")
    assert run_state.player.obtain_relic("DINGY_RUG")
    assert run_state.player.obtain_relic("SCROLL_BOXES")

    bundle_reward = next(reward for reward in run_state.pending_rewards if isinstance(reward, CardBundlesReward))
    ironclad_pool = set(get_character("Ironclad").card_pool)
    uncommon_cards = [
        card
        for bundle in bundle_reward.bundles
        for card in bundle
        if card.rarity == CardRarity.UNCOMMON
    ]
    assert any(card.card_id not in ironclad_pool for card in uncommon_cards)


def test_scroll_boxes_applies_prismatic_gem_card_pool_hook_to_bundle_candidates():
    run_state = RunState(seed=213, character_id="Ironclad")
    assert run_state.player.obtain_relic("PRISMATIC_GEM")
    assert run_state.player.obtain_relic("SCROLL_BOXES")

    bundle_reward = next(reward for reward in run_state.pending_rewards if isinstance(reward, CardBundlesReward))
    ironclad_pool = set(get_character("Ironclad").card_pool)
    assert any(
        card.card_id not in ironclad_pool
        for bundle in bundle_reward.bundles
        for card in bundle
    )


def test_scroll_boxes_dingy_rug_freezes_pool_before_prismatic_gem():
    run_state = RunState(seed=213, character_id="Ironclad")
    assert run_state.player.obtain_relic("DINGY_RUG")
    assert run_state.player.obtain_relic("PRISMATIC_GEM")
    assert run_state.player.obtain_relic("SCROLL_BOXES")

    bundle_reward = next(reward for reward in run_state.pending_rewards if isinstance(reward, CardBundlesReward))
    ironclad_pool = set(get_character("Ironclad").card_pool)
    colorless_uncommon_pool = set(
        eligible_registered_cards(
            card_pool=CardPoolId.COLORLESS,
            rarity=CardRarity.UNCOMMON,
            generation_context=None,
        )
    )
    assert all(
        card.card_id in ironclad_pool or card.card_id in colorless_uncommon_pool
        for bundle in bundle_reward.bundles
        for card in bundle
    )


def test_character_cards_modifier_appends_full_extra_pool_after_rarity_filter():
    run_state = RunState(seed=213, character_id="Ironclad")
    run_state.modifiers = [CharacterCardsModifier("Silent")]

    common_ids = run_state.player._card_bundle_candidate_ids(CardRarity.COMMON)
    uncommon_ids = run_state.player._card_bundle_candidate_ids(CardRarity.UNCOMMON)
    silent_pool = set(get_character("Silent").card_pool)

    assert any(card_id in silent_pool for card_id in common_ids)
    assert any(create_card(card_id).rarity is not CardRarity.COMMON for card_id in common_ids if card_id in silent_pool)
    assert any(card_id in silent_pool for card_id in uncommon_ids)
    assert any(create_card(card_id).rarity is not CardRarity.UNCOMMON for card_id in uncommon_ids if card_id in silent_pool)


def test_scroll_boxes_card_bundle_pick_adds_entire_selected_bundle():
    mgr = RunManager(seed=213, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)

    assert mgr.run_state.player.obtain_relic("SCROLL_BOXES")
    mgr._consume_run_pending_rewards()

    actions = mgr.get_available_actions()
    bundle_actions = [action for action in actions if action["action"] == "pick_card_bundle"]
    assert len(bundle_actions) == 2
    picked_ids = bundle_actions[0]["card_ids"]
    result = mgr.take_action({"action": "pick_card_bundle", "index": 0})

    assert result["success"] is True
    assert result["card_ids"] == picked_ids
    assert len(mgr.run_state.player.deck) == starting_deck + 3


def test_sea_glass_enqueues_fifteen_cards_from_assigned_character_pool():
    run_state = RunState(seed=214, character_id="Ironclad")

    assert run_state.player.obtain_relic("SEA_GLASS")
    sea_glass = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "SEA_GLASS")
    rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, CardReward)]

    assert sea_glass._character_id == "Ironclad"
    assert len(rewards) == 1
    reward = rewards[0]
    assert reward.option_count == 15
    assert reward.cards_to_pick == 15
    assert reward.character_ids == (sea_glass._character_id,)
    assert reward.generation_context is None
    assert reward.roll_upgrade is False
    assert reward.forced_rarities == ()
    assert reward.card_creation_source == CARD_CREATION_SOURCE_OTHER
    assert reward.allow_card_pool_modifications is False

    reward.populate(run_state, None)
    assigned_pool = set(get_character(sea_glass._character_id).card_pool)
    assert len(reward.cards) == 15
    assert all(card.card_id in assigned_pool for card in reward.cards)
    assert [card.rarity for card in reward.cards].count(CardRarity.COMMON) == 5
    assert [card.rarity for card in reward.cards].count(CardRarity.UNCOMMON) == 5
    assert [card.rarity for card in reward.cards].count(CardRarity.RARE) == 5


def test_sea_glass_card_reward_can_pick_multiple_cards_before_skip():
    mgr = RunManager(seed=214, character_id="Ironclad")
    starting_deck_size = len(mgr.run_state.player.deck)

    assert mgr.run_state.player.obtain_relic("SEA_GLASS")
    mgr._consume_run_pending_rewards()

    assert mgr.phase == RunManager.PHASE_CARD_REWARD
    assert len([action for action in mgr.get_available_actions() if action["action"] == "pick_card"]) == 15
    first = mgr.take_action({"action": "pick_card", "index": 0})
    assert first["pending_more_picks"] is True
    assert first["phase"] == RunManager.PHASE_CARD_REWARD
    second = mgr.take_action({"action": "pick_card", "index": 0})
    assert second["pending_more_picks"] is True
    assert second["phase"] == RunManager.PHASE_CARD_REWARD
    assert len(mgr.run_state.player.deck) == starting_deck_size + 2

    skipped = mgr.take_action({"action": "skip"})
    assert skipped["phase"] != RunManager.PHASE_CARD_REWARD


def test_sea_glass_ignores_dingy_rug_and_prismatic_pool_modifiers():
    run_state = RunState(seed=214, character_id="Ironclad")
    assert run_state.player.obtain_relic("DINGY_RUG")
    assert run_state.player.obtain_relic("PRISMATIC_GEM")
    assert run_state.player.obtain_relic_with_setup("SEA_GLASS", setup_attrs={"_character_id": "Silent"})

    rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, CardReward)]
    reward = rewards[-1]
    assert reward.character_ids == ("Silent",)
    assert reward.include_colorless is False
    assert reward.allow_card_pool_modifications is False

    reward.populate(run_state, None)
    silent_pool = set(get_character("Silent").card_pool)
    assert all(card.card_id in silent_pool for card in reward.cards)


def test_massive_scroll_enqueues_multiplayer_reward_pool():
    run_state = RunState(seed=215, character_id="Ironclad")

    assert run_state.player.obtain_relic("MASSIVE_SCROLL")
    rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, CardReward)]

    assert len(rewards) == 1
    reward = rewards[0]
    assert reward.option_count == 3
    assert reward.include_colorless is False
    assert reward.character_ids == ()
    assert reward.generation_context is None
    assert reward.has_custom_card_pool is True
    expected_ids = {
        CardId.BEACON_OF_HOPE,
        CardId.BELIEVE_IN_YOU,
        CardId.COORDINATE_CARD,
        CardId.DEMONIC_SHIELD,
        CardId.GANG_UP,
        CardId.HUDDLE_UP,
        CardId.INTERCEPT_CARD,
        CardId.KNOCKDOWN,
        CardId.LIFT,
        CardId.MIMIC,
        CardId.RALLY,
        CardId.TAG_TEAM,
        CardId.TANK_CARD,
    }
    assert set(reward.custom_card_ids) == expected_ids
    reward.populate(run_state, None)
    assert len(reward.cards) == 3
    assert all(card.card_id in expected_ids for card in reward.cards)


def test_lead_paperweight_enqueues_colorless_other_source_reward():
    run_state = RunState(seed=216, character_id="Ironclad")

    assert run_state.player.obtain_relic("LEAD_PAPERWEIGHT")
    rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, CardReward)]

    assert len(rewards) == 1
    reward = rewards[0]
    assert reward.option_count == 2
    assert reward.include_colorless is True
    assert reward.use_default_character_pool is False
    assert reward.generation_context is None
    assert reward.has_custom_card_pool is True


def test_prismatic_gem_does_not_expand_lead_paperweight_colorless_pool():
    run_state = RunState(seed=216, character_id="Ironclad")
    assert run_state.player.obtain_relic("PRISMATIC_GEM")
    assert run_state.player.obtain_relic("LEAD_PAPERWEIGHT")

    reward = [reward for reward in run_state.pending_rewards if isinstance(reward, CardReward)][0]
    reward.populate(run_state, None)

    ironclad_pool = set(get_character("Ironclad").card_pool)
    assert all(card.card_id not in ironclad_pool for card in reward.cards)


def test_orrery_and_lost_coffer_card_rewards_use_other_source_pool():
    orrery_state = RunState(seed=217, character_id="Ironclad")
    assert orrery_state.player.obtain_relic("ORRERY")
    orrery_rewards = [reward for reward in orrery_state.pending_rewards if isinstance(reward, CardReward)]
    assert len(orrery_rewards) == 5
    assert all(reward.generation_context is None for reward in orrery_rewards)

    lost_coffer_state = RunState(seed=218, character_id="Ironclad")
    assert lost_coffer_state.player.obtain_relic("LOST_COFFER")
    lost_coffer_rewards = [reward for reward in lost_coffer_state.pending_rewards if isinstance(reward, CardReward)]
    assert len(lost_coffer_rewards) == 1
    assert lost_coffer_rewards[0].generation_context is None


def test_treasure_room_uses_relic_reward_object_and_followup_rewards():
    mgr = RunManager(seed=216, character_id="Ironclad")
    mgr._enter_treasure()

    assert isinstance(mgr._current_reward, RelicReward)
    assert mgr.get_available_actions()[0]["action"] == "collect"

    mgr._current_reward = RelicReward(mgr.run_state.player.player_id, relic_id="SMALL_CAPSULE", rng_stream="treasure_room")
    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert any(action["action"] == "pick_relic_reward" for action in mgr.get_available_actions())


def test_calling_bell_enqueues_common_uncommon_rare_relic_rewards():
    run_state = RunState(seed=221, character_id="Ironclad")

    assert run_state.player.obtain_relic("CALLING_BELL")
    relic_rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, RelicReward)]

    assert len(relic_rewards) == 3
    assert [reward.rarity for reward in relic_rewards] == [
        RelicRarity.COMMON,
        RelicRarity.UNCOMMON,
        RelicRarity.RARE,
    ]
    assert any(card.card_id.name == "CURSE_OF_THE_BELL" for card in run_state.player.deck)


def test_calling_bell_deferred_followups_queue_curse_reward_before_relic_rewards():
    run_state = RunState(seed=224, character_id="Ironclad")
    run_state.defer_followup_rewards = True

    assert run_state.player.obtain_relic("CALLING_BELL")

    assert len(run_state.pending_rewards) == 4
    assert isinstance(run_state.pending_rewards[0], AddCardsReward)
    assert [card.card_id.name for card in run_state.pending_rewards[0].cards] == ["CURSE_OF_THE_BELL"]
    relic_rewards = [reward for reward in run_state.pending_rewards[1:] if isinstance(reward, RelicReward)]
    assert [reward.rarity for reward in relic_rewards] == [
        RelicRarity.COMMON,
        RelicRarity.UNCOMMON,
        RelicRarity.RARE,
    ]
    assert not any(card.card_id.name == "CURSE_OF_THE_BELL" for card in run_state.player.deck)


def test_cursed_pearl_deferred_followups_queue_greed_reward_and_gain_gold_immediately():
    run_state = RunState(seed=225, character_id="Ironclad")
    starting_gold = run_state.player.gold
    run_state.defer_followup_rewards = True

    assert run_state.player.obtain_relic("CURSED_PEARL")

    assert run_state.player.gold == starting_gold + 333
    assert len(run_state.pending_rewards) == 1
    assert isinstance(run_state.pending_rewards[0], AddCardsReward)
    assert [card.card_id.name for card in run_state.pending_rewards[0].cards] == ["GREED"]
    assert not any(card.card_id.name == "GREED" for card in run_state.player.deck)


def test_large_capsule_deferred_followups_queue_strike_and_defend_reward():
    run_state = RunState(seed=226, character_id="Ironclad")
    run_state.defer_followup_rewards = True

    assert run_state.player.obtain_relic("LARGE_CAPSULE")

    obtain_rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, ObtainRelicsReward)]
    assert len(obtain_rewards) == 1
    assert len(obtain_rewards[0].relic_ids) == 2
    assert len(set(obtain_rewards[0].relic_ids)) == 2

    strike_defend_rewards = [
        reward
        for reward in run_state.pending_rewards
        if isinstance(reward, AddCardsReward)
        and len(reward.cards) == 2
        and any("STRIKE" in card.card_id.name for card in reward.cards)
        and any("DEFEND" in card.card_id.name for card in reward.cards)
    ]
    assert len(strike_defend_rewards) == 1


def test_lead_paperweight_uses_colorless_only_regenerable_reward_and_supports_reroll():
    mgr = RunManager(seed=222, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("DRIFTWOOD")
    assert mgr.run_state.player.obtain_relic("LEAD_PAPERWEIGHT")

    rewards = [reward for reward in mgr.run_state.pending_rewards if isinstance(reward, CardReward)]
    assert len(rewards) == 1
    reward = rewards[0]
    assert reward.option_count == 2
    assert reward.include_colorless is True
    assert reward.use_default_character_pool is False
    assert reward.character_ids == ()

    mgr._consume_run_pending_rewards()
    assert mgr.phase == RunManager.PHASE_CARD_REWARD
    first_ids = [card.card_id for card in mgr._offered_cards]
    ironclad_pool = set(get_character("Ironclad").card_pool)
    assert all(card_id not in ironclad_pool for card_id in first_ids)
    assert any(action["action"] == "reroll_card_reward" for action in mgr.get_available_actions())

    mgr.take_action({"action": "reroll_card_reward"})
    second_ids = [card.card_id for card in mgr._offered_cards]
    assert all(card_id not in ironclad_pool for card_id in second_ids)
    assert first_ids != second_ids


def test_amethyst_aubergine_adds_bonus_gold_reward_object():
    run_state = RunState(seed=217, character_id="Ironclad")
    assert run_state.player.obtain_relic("AMETHYST_AUBERGINE")

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(create_room(RoomType.MONSTER), run_state)
    generated = rewards.generate_without_offering(run_state)

    gold_rewards = [reward for reward in generated if isinstance(reward, GoldReward)]
    assert len(gold_rewards) == 2
    assert any(reward.min_gold == 15 and reward.max_gold == 15 for reward in gold_rewards)

    run_state.current_act_index = 0
    boss_rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(create_room(RoomType.BOSS), run_state)
    boss_generated = boss_rewards.generate_without_offering(run_state)
    assert any(
        isinstance(reward, GoldReward) and reward.min_gold == 15 and reward.max_gold == 15
        for reward in boss_generated
    )

    run_state.current_act_index = len(run_state.acts) - 1
    final_boss_rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(
        create_room(RoomType.BOSS),
        run_state,
    )
    final_boss_generated = final_boss_rewards.generate_without_offering(run_state)
    assert not any(
        isinstance(reward, GoldReward) and reward.min_gold == 15 and reward.max_gold == 15
        for reward in final_boss_generated
    )


def test_lasting_candy_adds_extra_power_reward_every_second_combat():
    run_state = RunState(seed=218, character_id="Ironclad")
    assert run_state.player.obtain_relic("LASTING_CANDY")
    relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "LASTING_CANDY")
    relic._combats_seen = 2

    reward = CardReward(
        run_state.player.player_id,
        option_count=2,
        forced_rarities=(CardRarity.COMMON, CardRarity.COMMON),
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))

    assert len(reward.cards) == 3
    assert any(card.card_type.name == "POWER" for card in reward.cards)


def test_lasting_candy_extra_power_is_visible_to_late_reward_modifiers():
    run_state = RunState(seed=218, character_id="Ironclad")
    assert run_state.player.obtain_relic("FROZEN_EGG")
    assert run_state.player.obtain_relic("LASTING_CANDY")
    relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "LASTING_CANDY")
    relic._combats_seen = 2

    reward = CardReward(
        run_state.player.player_id,
        option_count=2,
        forced_rarities=(CardRarity.COMMON, CardRarity.COMMON),
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))

    added_power = next(card for card in reward.cards if card.card_type == CardType.POWER)
    assert added_power.upgraded


def test_lasting_candy_does_not_modify_other_source_card_rewards():
    run_state = RunState(seed=218, character_id="Ironclad")
    assert run_state.player.obtain_relic("LASTING_CANDY")
    relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "LASTING_CANDY")
    relic._combats_seen = 2

    assert run_state.player.obtain_relic("ORRERY")
    reward = [reward for reward in run_state.pending_rewards if isinstance(reward, CardReward)][0]
    reward.populate(run_state, None)

    assert reward.card_creation_source == "other"
    assert len(reward.cards) == 3


def test_lasting_candy_does_not_duplicate_existing_power_reward_options():
    run_state = RunState(seed=218, character_id="Ironclad")
    assert run_state.player.obtain_relic("LASTING_CANDY")
    relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "LASTING_CANDY")
    relic._combats_seen = 2
    existing_powers = [
        create_card(card_id)
        for card_id in eligible_character_cards(
            "Ironclad",
            card_type=CardType.POWER,
            generation_context="modifier",
        )
    ]

    reward = CardReward(run_state.player.player_id, cards=existing_powers)
    reward.populate(run_state, create_room(RoomType.MONSTER))

    assert len(reward.cards) == len(existing_powers)
    assert len({card.card_id for card in reward.cards}) == len(existing_powers)


def test_manual_card_reward_skips_card_pool_creation_modifiers_but_keeps_reward_hooks():
    run_state = RunState(seed=MANUAL_REWARD_PARITY_SEED, character_id=IRONCLAD_CHARACTER_ID)
    run_state.modifiers = [CharacterCardsModifier(SILENT_CHARACTER_ID)]
    assert run_state.player.obtain_relic(FROZEN_EGG_RELIC_NAME)
    assert run_state.player.obtain_relic(LASTING_CANDY_RELIC_NAME)
    relic = next(
        relic
        for relic in run_state.player.get_relic_objects()
        if relic.relic_id.name == LASTING_CANDY_RELIC_NAME
    )
    relic._combats_seen = LASTING_CANDY_TRIGGERING_COMBATS_SEEN

    reward = CardReward(
        run_state.player.player_id,
        cards=[create_card(CardId.INFLAME)],
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))

    assert [card.card_id for card in reward.cards] == [CardId.INFLAME]
    assert reward.cards[0].upgraded
    assert reward.allow_card_pool_modifications is False
    assert reward.custom_card_ids == ()


def test_empty_manual_card_reward_stays_empty_after_populate():
    run_state = RunState(seed=MANUAL_REWARD_PARITY_SEED, character_id=IRONCLAD_CHARACTER_ID)

    reward = CardReward(run_state.player.player_id, cards=[])
    reward.populate(run_state, create_room(RoomType.MONSTER))

    assert reward.cards == []
    assert reward.option_count == EMPTY_MANUAL_REWARD_CARD_COUNT


def test_lasting_candy_stays_inside_custom_card_reward_pool():
    run_state = RunState(seed=218, character_id="Ironclad")
    assert run_state.player.obtain_relic("LASTING_CANDY")
    relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "LASTING_CANDY")
    relic._combats_seen = 2

    reward = CardReward(
        run_state.player.player_id,
        option_count=2,
        forced_rarities=(CardRarity.COMMON, CardRarity.COMMON),
        use_default_character_pool=False,
        has_custom_card_pool=True,
        custom_card_ids=(CardId.ANGER, CardId.SHRUG_IT_OFF),
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))

    assert len(reward.cards) == 2
    assert {card.card_id for card in reward.cards} <= {CardId.ANGER, CardId.SHRUG_IT_OFF}


def test_wing_charm_enchants_one_random_reward_card_with_swift():
    run_state = RunState(seed=219, character_id="Ironclad")
    assert run_state.player.obtain_relic("WING_CHARM")
    anger = create_card(CardId.ANGER)
    anger.add_enchantment("Glam", 1)
    inflame = create_card(CardId.INFLAME)
    inflame.add_enchantment("Glam", 1)

    reward = CardReward(
        run_state.player.player_id,
        cards=[anger, create_card(CardId.SHRUG_IT_OFF), inflame],
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))

    swift_cards = [card for card in reward.cards if card.enchantments.get("Swift") == 1]
    assert len(swift_cards) == 1
    assert swift_cards[0].card_id == CardId.SHRUG_IT_OFF


def test_wongos_mystery_ticket_adds_three_relic_rewards_once_after_threshold():
    run_state = RunState(seed=220, character_id="Ironclad")
    assert run_state.player.obtain_relic("WONGOS_MYSTERY_TICKET")
    relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "WONGOS_MYSTERY_TICKET")

    relic._combats_finished = 4
    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(create_room(RoomType.MONSTER), run_state)
    generated = rewards.generate_without_offering(run_state)
    relic_rewards = [reward for reward in generated if isinstance(reward, RelicReward)]
    assert len(relic_rewards) == 0

    relic._combats_finished = 5

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(create_room(RoomType.MONSTER), run_state)
    generated = rewards.generate_without_offering(run_state)
    relic_rewards = [reward for reward in generated if isinstance(reward, RelicReward)]
    assert len(relic_rewards) == 3
    assert relic._gave_relic is True

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(create_room(RoomType.MONSTER), run_state)
    generated = rewards.generate_without_offering(run_state)
    relic_rewards = [reward for reward in generated if isinstance(reward, RelicReward)]
    assert len(relic_rewards) == 0


def test_relic_grab_bag_respects_before_act3_treasure_chest_filters():
    run_state = RunState(seed=221, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.relic_grab_bag_by_rarity = {
        RelicRarity.COMMON: ["AMETHYST_AUBERGINE"],
        RelicRarity.UNCOMMON: [],
        RelicRarity.RARE: [],
        RelicRarity.SHOP: [],
    }
    run_state.player.relic_grab_bag = ["AMETHYST_AUBERGINE"]
    run_state.player.relic_grab_bag_fallback = []

    run_state.total_floor = 40
    assert run_state.player.has_available_relics() is True
    assert run_state.player.pull_next_relic_reward_id(rarity=RelicRarity.COMMON) == "AMETHYST_AUBERGINE"

    run_state.player.relic_grab_bag_by_rarity[RelicRarity.COMMON] = ["AMETHYST_AUBERGINE"]
    run_state.player.relic_grab_bag = ["AMETHYST_AUBERGINE"]
    run_state.total_floor = 41
    assert run_state.player.has_available_relics() is False
    assert run_state.player.pull_next_relic_reward_id(rarity=RelicRarity.COMMON) == "CIRCLET"


def test_dingy_rug_adds_colorless_cards_to_reward_pool():
    run_state = RunState(seed=209, character_id="Ironclad")
    assert run_state.player.obtain_relic("DINGY_RUG")

    reward = CardReward(
        run_state.player.player_id,
        forced_rarities=(CardRarity.RARE,) * 40,
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))

    ironclad_pool = set(get_character("Ironclad").card_pool)
    assert any(card.card_id not in ironclad_pool for card in reward.cards)


def test_dingy_rug_freezes_card_reward_pool_before_prismatic_gem():
    run_state = RunState(seed=209, character_id="Ironclad")
    assert run_state.player.obtain_relic("DINGY_RUG")
    assert run_state.player.obtain_relic("PRISMATIC_GEM")

    reward = CardReward(
        run_state.player.player_id,
        forced_rarities=(CardRarity.UNCOMMON,) * 20,
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))

    ironclad_pool = set(get_character("Ironclad").card_pool)
    colorless_uncommon_pool = set(
        eligible_registered_cards(
            card_pool=CardPoolId.COLORLESS,
            rarity=CardRarity.UNCOMMON,
            generation_context="combat",
        )
    )
    assert all(
        card.card_id in ironclad_pool or card.card_id in colorless_uncommon_pool
        for card in reward.cards
    )


def test_lava_lamp_upgrades_only_no_damage_combat_rewards():
    run_state = RunState(seed=210, character_id="Ironclad")
    assert run_state.player.obtain_relic("LAVA_LAMP")
    lava_lamp = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "LAVA_LAMP")

    reward = CardReward(
        run_state.player.player_id,
        cards=[create_card(CardId.ANGER), create_card(CardId.SHRUG_IT_OFF)],
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))
    assert all(card.upgraded for card in reward.cards)

    lava_lamp._took_damage = True
    reward = CardReward(
        run_state.player.player_id,
        cards=[create_card(CardId.ANGER), create_card(CardId.SHRUG_IT_OFF)],
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))
    assert not any(card.upgraded for card in reward.cards)


def test_lava_lamp_ignores_unblockable_damage_for_reward_upgrade():
    run_state = RunState(seed=210, character_id="Ironclad")
    assert run_state.player.obtain_relic("LAVA_LAMP")
    lava_lamp = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "LAVA_LAMP")

    lava_lamp.after_damage_received(
        run_state.player,
        run_state.player,
        None,
        5,
        ValueProp.UNBLOCKABLE,
        None,
    )
    reward = CardReward(
        run_state.player.player_id,
        cards=[create_card(CardId.ANGER), create_card(CardId.SHRUG_IT_OFF)],
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))

    assert all(card.upgraded for card in reward.cards)


def test_silver_crucible_upgrades_first_three_card_rewards_only():
    run_state = RunState(seed=SILVER_CRUCIBLE_CARD_REWARD_LIMIT_SEED, character_id=IRONCLAD_CHARACTER_ID)
    assert run_state.player.obtain_relic(SILVER_CRUCIBLE_RELIC_NAME)
    silver = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == SILVER_CRUCIBLE_RELIC_NAME)

    upgraded_flags: list[bool] = []
    for _ in range(4):
        reward = CardReward(
            run_state.player.player_id,
            cards=[create_card(CardId.ANGER), create_card(CardId.SHRUG_IT_OFF)],
        )
        reward.populate(run_state, create_room(RoomType.MONSTER))
        upgraded_flags.append(all(card.upgraded for card in reward.cards))

    assert upgraded_flags == [True, True, True, False]
    assert silver._times_used == 3


def test_silver_crucible_reroll_consumes_next_card_reward_upgrade():
    mgr = RunManager(seed=SILVER_CRUCIBLE_REROLL_SEED, character_id=IRONCLAD_CHARACTER_ID)
    assert mgr.run_state.player.obtain_relic(SILVER_CRUCIBLE_RELIC_NAME)
    assert mgr.run_state.player.obtain_relic("DRIFTWOOD")
    silver = next(relic for relic in mgr.run_state.player.get_relic_objects() if relic.relic_id.name == SILVER_CRUCIBLE_RELIC_NAME)

    mgr._enter_card_reward(context="regular")
    assert all(card.upgraded for card in mgr._offered_cards)
    assert silver._times_used == 1

    mgr.take_action({"action": "reroll_card_reward"})

    assert all(card.upgraded for card in mgr._offered_cards)
    assert silver._times_used == 2


def test_calling_bell_enqueues_common_uncommon_rare_relic_rewards():
    run_state = RunState(seed=223, character_id="Ironclad")
    assert run_state.player.obtain_relic("CALLING_BELL")

    relic_rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, RelicReward)]
    assert len(relic_rewards) == 3
    assert [reward.rarity for reward in relic_rewards] == [
        RelicRarity.COMMON,
        RelicRarity.UNCOMMON,
        RelicRarity.RARE,
    ]
    assert any(card.card_id.name == "CURSE_OF_THE_BELL" for card in run_state.player.deck)
