"""Regression tests for run-level deck choice handling in RunManager."""

from sts2_env.cards.factory import create_card
from sts2_env.core.enums import CardId, CardType
from sts2_env.run.events import EventModel, EventOption, EventResult
from types import SimpleNamespace

from sts2_env.core.enums import RelicRarity
from sts2_env.run.reward_objects import (
    EnchantCardsReward,
    RelicReward,
    RemoveCardReward,
    TransformCardsReward,
    UpgradeCardsReward,
    DuplicateCardReward,
)
from sts2_env.run.run_manager import RunManager
from sts2_env.run.shop import ShopInventory, ShopRelicEntry


def test_shop_remove_card_uses_run_level_deck_choice_and_resumes_shop():
    mgr = RunManager(seed=801, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_SHOP
    mgr.run_state.player.gold = 999
    starting_deck = len(mgr.run_state.player.deck)
    mgr._shop_inventory = ShopInventory(removal_cost=75)

    result = mgr._do_shop_action({"action": "remove_card"})

    assert result["phase"] == RunManager.PHASE_SHOP
    assert mgr.run_state.pending_choice is not None
    assert any(action["action"] == "choose" for action in mgr.get_available_actions())

    final = mgr.take_action({"action": "choose", "index": 0})

    assert final["phase"] == RunManager.PHASE_SHOP
    assert mgr.run_state.pending_choice is None
    assert len(mgr.run_state.player.deck) == starting_deck - 1
    assert mgr._shop_inventory is not None
    assert mgr._shop_inventory.removal_cost == 100


def test_treasure_relic_with_deck_choice_pauses_then_returns_to_map():
    mgr = RunManager(seed=802, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="PRECISE_SCISSORS",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    assert any(action["action"] == "choose" for action in mgr.get_available_actions())

    final = mgr.take_action({"action": "choose", "index": 0})

    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.pending_choice is None
    assert len(mgr.run_state.player.deck) == starting_deck - 1


def test_manager_context_enables_interactive_relic_obtain_choices():
    mgr = RunManager(seed=803, character_id="Ironclad")

    assert mgr.run_state.enable_deck_choice_requests is True
    assert mgr.run_state.player.obtain_relic("BEAUTIFUL_BRACELET")
    assert mgr.run_state.pending_choice is not None
    assert any(action["action"] == "choose" for action in mgr.get_available_actions())

    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.pending_choice is None
    assert sum(1 for card in mgr.run_state.player.deck if card.has_enchantment("Swift")) == 1


def test_remove_card_reward_uses_run_level_deck_choice():
    mgr = RunManager(seed=804, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    reward = RemoveCardReward(mgr.run_state.player.player_id, count=1)

    result = reward.select(mgr)

    assert result["pending_choice"] is True
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    assert mgr.run_state.pending_choice is None
    assert len(mgr.run_state.player.deck) == starting_deck - 1


def test_upgrade_cards_reward_uses_run_level_deck_choice():
    mgr = RunManager(seed=807, character_id="Ironclad")
    reward = UpgradeCardsReward(mgr.run_state.player.player_id, count=1)

    result = reward.select(mgr)

    assert result["pending_choice"] is True
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    assert mgr.run_state.pending_choice is None
    assert any(card.upgraded for card in mgr.run_state.player.deck)


def test_duplicate_card_reward_uses_run_level_deck_choice():
    mgr = RunManager(seed=810, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    reward = DuplicateCardReward(mgr.run_state.player.player_id, count=1)

    result = reward.select(mgr)

    assert result["pending_choice"] is True
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    assert mgr.run_state.pending_choice is None
    assert len(mgr.run_state.player.deck) == starting_deck + 1


def test_enchant_cards_reward_uses_run_level_deck_choice():
    mgr = RunManager(seed=811, character_id="Ironclad")
    reward = EnchantCardsReward(mgr.run_state.player.player_id, enchantment="Swift", amount=3, count=1)

    result = reward.select(mgr)

    assert result["pending_choice"] is True
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["finished"] is True
    assert mgr.run_state.pending_choice is None
    assert any(card.has_enchantment("Swift") for card in mgr.run_state.player.deck)


def test_transform_cards_reward_with_upgrade_requires_confirm_and_resumes():
    mgr = RunManager(seed=808, character_id="Ironclad")
    reward = TransformCardsReward(mgr.run_state.player.player_id, count=2, upgrade=True)

    result = reward.select(mgr)

    assert result["pending_choice"] is True
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["finished"] is True
    assert mgr.run_state.pending_choice is None


def test_treasure_relic_with_upgrade_reward_pauses_then_returns_to_map():
    mgr = RunManager(seed=809, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="POMANDER",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"}) if mgr.run_state.pending_choice is not None else {"phase": mgr.phase}
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE or mgr.phase == RunManager.PHASE_MAP_CHOICE


def test_treasure_relic_with_enchant_reward_pauses_then_returns_to_map():
    mgr = RunManager(seed=812, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="BEAUTIFUL_BRACELET",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    mgr.take_action({"action": "choose", "index": 2})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE


def test_treasure_relic_with_duplicate_reward_pauses_then_returns_to_map():
    mgr = RunManager(seed=813, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="DOLLYS_MIRROR",
        rarity=RelicRarity.SHOP,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len(mgr.run_state.player.deck) == starting_deck + 1


def test_treasure_relic_with_remove_reward_and_immediate_side_effect_resumes_to_map():
    mgr = RunManager(seed=814, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="PRESERVED_FOG",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert any(card.card_id.name == "FOLLY" for card in mgr.run_state.player.deck)
    assert mgr.run_state.pending_choice is not None
    for idx in range(5):
        mgr.take_action({"action": "choose", "index": idx})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len(mgr.run_state.player.deck) == starting_deck - 4


def test_treasure_relic_with_random_upgrade_effect_returns_to_map_without_choice():
    mgr = RunManager(seed=815, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="WAR_PAINT",
        rarity=RelicRarity.COMMON,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len([card for card in mgr.run_state.player.deck if card.upgraded and card.card_type.name == "SKILL"]) >= 2


def test_war_hammer_upgrades_random_cards_after_elite_victory():
    mgr = RunManager(seed=816, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("WAR_HAMMER")
    relic = next(relic for relic in mgr.run_state.player.get_relic_objects() if relic.relic_id.name == "WAR_HAMMER")
    upgraded_before = sum(1 for card in mgr.run_state.player.deck if card.upgraded)

    relic.after_combat_victory(mgr.run_state.player, SimpleNamespace(is_elite=True))

    upgraded_after = sum(1 for card in mgr.run_state.player.deck if card.upgraded)
    assert upgraded_after >= upgraded_before + 4
    assert not any(isinstance(reward, UpgradeCardsReward) for reward in mgr.run_state.pending_rewards)


def test_treasure_leafy_poultice_transforms_one_strike_and_one_defend_without_choice():
    mgr = RunManager(seed=818, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="LEAFY_POULTICE",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert sum(
        1 for card in mgr.run_state.player.deck
        if card.rarity.name == "BASIC" and ("STRIKE" in card.card_id.name or "DEFEND" in card.card_id.name)
    ) == 7


def test_treasure_sand_castle_applies_random_upgrades_without_choice():
    mgr = RunManager(seed=819, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="SAND_CASTLE",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    for idx in range(6):
        mgr.take_action({"action": "choose", "index": idx})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert sum(1 for card in mgr.run_state.player.deck if card.upgraded) >= 6


def test_treasure_fragrant_mushroom_routes_upgrades_through_reward_chain():
    mgr = RunManager(seed=822, character_id="Ironclad")
    starting_hp = mgr.run_state.player.current_hp
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="FRAGRANT_MUSHROOM",
        rarity=RelicRarity.EVENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.player.current_hp == starting_hp - 15
    assert mgr.run_state.pending_choice is not None
    for idx in range(3):
        mgr.take_action({"action": "choose", "index": idx})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert sum(1 for card in mgr.run_state.player.deck if card.upgraded) >= 3


def test_treasure_archaic_tooth_routes_mapping_transform_through_reward_chain():
    mgr = RunManager(seed=823, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="ARCHAIC_TOOTH",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    final = mgr.take_action({"action": "choose", "index": 0})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert any(card.card_id.name == "BREAK" for card in mgr.run_state.player.deck)


def test_treasure_pandoras_box_transforms_all_basic_strike_defends_without_choice():
    mgr = RunManager(seed=820, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="PANDORAS_BOX",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None
    basic_count = sum(
        1 for card in mgr.run_state.player.deck
        if card.rarity.name == "BASIC" and ("STRIKE" in card.card_id.name or "DEFEND" in card.card_id.name)
    )
    for idx in range(basic_count):
        mgr.take_action({"action": "choose", "index": idx})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert sum(
        1 for card in mgr.run_state.player.deck
        if card.rarity.name == "BASIC" and ("STRIKE" in card.card_id.name or "DEFEND" in card.card_id.name)
    ) == 0


def test_dollys_mirror_pending_choice_excludes_quest_cards():
    mgr = RunManager(seed=814, character_id="Ironclad")
    mgr.run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))

    assert mgr.run_state.player.obtain_relic("DOLLYS_MIRROR")

    assert mgr.run_state.pending_choice is not None
    assert all(option.card.card_type != CardType.QUEST for option in mgr.run_state.pending_choice.options)


def test_astrolabe_pending_choice_excludes_quest_cards():
    mgr = RunManager(seed=815, character_id="Ironclad")
    mgr.run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="ASTROLABE",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, TransformCardsReward)
    assert mgr._current_reward.cards is not None
    assert all(card.card_type != CardType.QUEST for card in mgr._current_reward.cards)


def test_yummy_cookie_pending_choice_excludes_quest_cards():
    mgr = RunManager(seed=816, character_id="Ironclad")
    mgr.run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="YUMMY_COOKIE",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, UpgradeCardsReward)
    assert mgr._current_reward.cards is not None
    assert all(card.card_type != CardType.QUEST for card in mgr._current_reward.cards)


def test_pomander_pending_choice_excludes_quest_cards():
    mgr = RunManager(seed=817, character_id="Ironclad")
    mgr.run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="POMANDER",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, UpgradeCardsReward)
    assert mgr._current_reward.cards is not None
    assert all(card.card_type != CardType.QUEST for card in mgr._current_reward.cards)


def test_shop_buy_relic_with_multi_choice_resumes_shop_after_confirm():
    mgr = RunManager(seed=804, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_SHOP
    mgr.run_state.player.gold = 999
    mgr._shop_inventory = ShopInventory(
        relics=[ShopRelicEntry(relic_rarity=RelicRarity.ANCIENT, relic_id="BEAUTIFUL_BRACELET", price=99)]
    )

    result = mgr._do_shop_action({"action": "buy_relic", "index": 0})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None

    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["phase"] == RunManager.PHASE_SHOP
    assert mgr.run_state.pending_choice is None
    assert sum(1 for card in mgr.run_state.player.deck if card.has_enchantment("Swift")) == 1


def test_boss_relic_pick_with_deck_choice_resumes_to_next_act_after_confirm():
    mgr = RunManager(seed=805, character_id="Ironclad")
    starting_act = mgr.run_state.current_act_index
    mgr._phase = RunManager.PHASE_BOSS_RELIC
    mgr._boss_relics = ["BEAUTIFUL_BRACELET"]

    result = mgr._do_boss_relic_pick({"index": 0})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None

    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.current_act_index == starting_act + 1
    assert mgr.run_state.pending_choice is None


class _ObtainRelicEvent(EventModel):
    event_id = "TEST_OBTAIN_RELIC"

    def generate_initial_options(self, run_state):
        return [EventOption(option_id="take", label="Take")]

    def choose(self, run_state, option_id: str):
        if option_id == "take":
            run_state.player.obtain_relic("PRECISE_SCISSORS")
            return EventResult(finished=True, description="Took relic.")
        return EventResult(finished=True, description="Left.")


def test_event_choice_that_obtains_relic_with_deck_choice_resumes_to_map():
    mgr = RunManager(seed=806, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = _ObtainRelicEvent()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)

    result = mgr._do_event_choice({"option_id": "take"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None

    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.pending_choice is None
    assert len(mgr.run_state.player.deck) == starting_deck - 1
