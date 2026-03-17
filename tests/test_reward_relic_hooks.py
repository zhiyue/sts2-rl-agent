"""Regression tests for relic-driven reward and card-reward hooks."""

from sts2_env.core.combat import CombatState
from sts2_env.cards.factory import create_card
from sts2_env.characters.all import get_character
from sts2_env.core.enums import CardId, CardRarity, RoomType
from sts2_env.run.reward_objects import CardReward, PotionReward, RelicReward, RewardsSet
from sts2_env.run.rooms import create_room
from sts2_env.run.shop import generate_shop_inventory
from sts2_env.run.run_state import RunState
from sts2_env.run.run_manager import RunManager


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
        assert reward.forced_rarities == (rarity, rarity, rarity)
        reward.populate(run_state, None)
        assert len(reward.cards) == 3
        assert all(card.rarity == rarity for card in reward.cards)


def test_lava_rock_adds_two_relic_rewards_only_to_act_one_boss_once():
    run_state = RunState(seed=109, character_id="Ironclad")
    assert run_state.player.obtain_relic("LAVA_ROCK")
    room = create_room(RoomType.BOSS)

    first = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state).generate_without_offering(run_state)
    second = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state).generate_without_offering(run_state)

    assert len([reward for reward in first if isinstance(reward, RelicReward)]) == 2
    assert len([reward for reward in second if isinstance(reward, RelicReward)]) == 0


def test_silver_crucible_upgrades_three_card_rewards_and_skips_first_treasure():
    mgr = RunManager(seed=210, character_id="Ironclad")
    run_state = mgr.run_state
    assert run_state.player.obtain_relic("SILVER_CRUCIBLE")
    crucible = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "SILVER_CRUCIBLE")

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

    reward = CardReward(run_state.player.player_id, cards=[create_card(CardId.ANGER)])
    reward.populate(run_state, None)

    assert reward.cards[0].enchantments["Nimble"] == 2
    assert reward.cards[0].enchantments["Glam"] == 1


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
    run_state = RunState(seed=208, character_id="Ironclad")
    assert run_state.player.obtain_relic("MOLTEN_EGG")
    assert run_state.player.obtain_relic("FRESNEL_LENS")

    inventory = generate_shop_inventory(run_state)
    attack_entries = [entry for entry in inventory.cards if entry.card is not None and entry.card.card_type.name == "ATTACK"]

    assert attack_entries
    assert any(entry.card.upgraded for entry in attack_entries)
    assert all("Nimble" in entry.card.enchantments for entry in inventory.cards if entry.card is not None)


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


def test_silver_crucible_upgrades_first_three_card_rewards_only():
    run_state = RunState(seed=211, character_id="Ironclad")
    assert run_state.player.obtain_relic("SILVER_CRUCIBLE")
    silver = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "SILVER_CRUCIBLE")

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


def test_silver_crucible_reroll_keeps_same_reward_screen_upgraded_without_double_counting():
    mgr = RunManager(seed=212, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("SILVER_CRUCIBLE")
    assert mgr.run_state.player.obtain_relic("DRIFTWOOD")
    silver = next(relic for relic in mgr.run_state.player.get_relic_objects() if relic.relic_id.name == "SILVER_CRUCIBLE")

    mgr._enter_card_reward(context="regular")
    assert all(card.upgraded for card in mgr._offered_cards)
    assert silver._times_used == 1

    mgr.take_action({"action": "reroll_card_reward"})

    assert all(card.upgraded for card in mgr._offered_cards)
    assert silver._times_used == 1
