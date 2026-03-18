"""Regression tests for run-level deck choice handling in RunManager."""

from sts2_env.cards.factory import create_card
from sts2_env.cards.silent import make_backstab
from sts2_env.core.enums import CardId, CardType, RoomType
from sts2_env.events.act2 import CrystalSphere, FakeMerchant, FieldOfManSizedHoles, LuminousChoir
from sts2_env.events.shared import Bugslayer, DrowningBeacon, GraveOfTheForgotten, Orobas, PunchOff, Reflections, ThisOrThat, Trial
from sts2_env.run.events import EventModel, EventOption, EventResult
from types import SimpleNamespace

from sts2_env.core.enums import RelicRarity
from sts2_env.run.reward_objects import (
    AddCardsReward,
    EnchantCardsReward,
    ObtainRelicsReward,
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


def test_add_cards_reward_auto_applies_without_pending_choice():
    mgr = RunManager(seed=825, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    reward = AddCardsReward(mgr.run_state.player.player_id, [create_card(CardId.NEOWS_FURY)])

    result = reward.select(mgr)

    assert result["added"] == 1
    assert mgr.run_state.pending_choice is None
    assert len(mgr.run_state.player.deck) == starting_deck + 1


def test_obtain_relics_reward_auto_applies_random_relics():
    mgr = RunManager(seed=826, character_id="Ironclad")
    starting_relics = len(mgr.run_state.player.relics)
    reward = ObtainRelicsReward(mgr.run_state.player.player_id, count=2)

    result = reward.select(mgr)

    assert len(result["obtained"]) == 2
    assert mgr.run_state.pending_choice is None
    assert len(mgr.run_state.player.relics) == starting_relics + 2


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


def test_treasure_relic_with_remove_reward_and_followup_add_card_resumes_to_map():
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
    assert not any(card.card_id.name == "FOLLY" for card in mgr.run_state.player.deck)
    assert mgr.run_state.pending_choice is not None
    for idx in range(5):
        mgr.take_action({"action": "choose", "index": idx})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len(mgr.run_state.player.deck) == starting_deck - 4
    assert any(card.card_id.name == "FOLLY" for card in mgr.run_state.player.deck)


def test_treasure_calling_bell_queues_curse_reward_while_surfacing_relic_rewards():
    mgr = RunManager(seed=830, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="CALLING_BELL",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, RelicReward)
    assert any(
        isinstance(reward, AddCardsReward)
        and [card.card_id.name for card in reward.cards] == ["CURSE_OF_THE_BELL"]
        for reward in mgr._pending_rewards
    )


def test_treasure_cursed_pearl_auto_applies_curse_and_gold_then_returns_to_map():
    mgr = RunManager(seed=831, character_id="Ironclad")
    starting_gold = mgr.run_state.player.gold
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="CURSED_PEARL",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.player.gold == starting_gold + 333
    assert any(card.card_id.name == "GREED" for card in mgr.run_state.player.deck)


def test_treasure_large_capsule_auto_applies_random_relics_and_added_cards():
    mgr = RunManager(seed=832, character_id="Ironclad")
    starting_relics = len(mgr.run_state.player.relics)
    starting_strikes = sum(1 for card in mgr.run_state.player.deck if "STRIKE" in card.card_id.name)
    starting_defends = sum(1 for card in mgr.run_state.player.deck if "DEFEND" in card.card_id.name)
    rolled = iter(("ANCHOR", "LANTERN"))
    mgr.run_state.player.roll_relic_reward_id = lambda **_: next(rolled, None)
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="LARGE_CAPSULE",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len(mgr.run_state.player.relics) == starting_relics + 3
    assert sum(1 for card in mgr.run_state.player.deck if "STRIKE" in card.card_id.name) == starting_strikes + 1
    assert sum(1 for card in mgr.run_state.player.deck if "DEFEND" in card.card_id.name) == starting_defends + 1


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
    combat = SimpleNamespace(
        is_elite=True,
        combat_player_state_for=lambda owner: SimpleNamespace(player_state=mgr.run_state.player),
    )

    relic.after_combat_victory(mgr.run_state.player, combat)

    rewards = [reward for reward in mgr.run_state.pending_rewards if isinstance(reward, UpgradeCardsReward)]
    assert len(rewards) == 1
    assert rewards[0].count == 4


def test_after_card_reward_consumes_followup_pending_rewards_before_leaving_map():
    mgr = RunManager(seed=824, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_CARD_REWARD
    mgr._current_room_type = RoomType.MONSTER
    mgr.run_state.pending_rewards.append(UpgradeCardsReward(mgr.run_state.player.player_id, count=1))

    mgr._after_card_reward()

    assert mgr.phase == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None


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


def test_treasure_storybook_auto_adds_card_and_returns_to_map():
    mgr = RunManager(seed=826, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="STORYBOOK",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.pending_choice is None
    assert len(mgr.run_state.player.deck) == starting_deck + 1
    assert any(card.card_id.name == "BRIGHTEST_FLAME" for card in mgr.run_state.player.deck)


def test_treasure_cursed_pearl_auto_adds_greed_reward_and_gold_before_returning_to_map():
    mgr = RunManager(seed=828, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    starting_gold = mgr.run_state.player.gold
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="CURSED_PEARL",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.pending_choice is None
    assert mgr.run_state.player.gold == starting_gold + 333
    assert len(mgr.run_state.player.deck) == starting_deck + 1
    assert any(card.card_id.name == "GREED" for card in mgr.run_state.player.deck)


def test_treasure_distinguished_cape_auto_adds_apparitions_and_reduces_max_hp():
    mgr = RunManager(seed=827, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    starting_max_hp = mgr.run_state.player.max_hp
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(
        mgr.run_state.player.player_id,
        relic_id="DISTINGUISHED_CAPE",
        rarity=RelicRarity.ANCIENT,
    )

    result = mgr._do_treasure_collect()

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.pending_choice is None
    assert mgr.run_state.player.max_hp == starting_max_hp - 9
    assert len(mgr.run_state.player.deck) == starting_deck + 3
    assert sum(1 for card in mgr.run_state.player.deck if card.card_id.name == "APPARITION") == 3


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


def test_event_fixed_add_card_reward_auto_applies_and_returns_to_map():
    mgr = RunManager(seed=833, character_id="Ironclad")
    starting_deck = len(mgr.run_state.player.deck)
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = Bugslayer()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)

    result = mgr._do_event_choice({"option_id": "exterminate"})

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len(mgr.run_state.player.deck) == starting_deck + 1
    assert any(card.card_id.name == "EXTERMINATE" for card in mgr.run_state.player.deck)


def test_event_fixed_relic_reward_surfaces_reward_screen_then_resumes_to_map():
    mgr = RunManager(seed=834, character_id="Ironclad")
    starting_max_hp = mgr.run_state.player.max_hp
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = DrowningBeacon()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)

    result = mgr._do_event_choice({"option_id": "climb"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.player.max_hp == starting_max_hp - 13
    assert isinstance(mgr._current_reward, RelicReward)
    assert mgr._current_reward.relic_id == "FRESNEL_LENS"

    final = mgr.take_action({"action": "pick_relic_reward", "relic_id": "FRESNEL_LENS"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.player.relics[-1] == "FRESNEL_LENS"


def test_event_random_relic_plus_add_card_surfaces_relic_then_keeps_add_card_pending():
    mgr = RunManager(seed=835, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = ThisOrThat()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)
    mgr.run_state.player.roll_relic_reward_id = lambda **_: "ANCHOR"

    result = mgr._do_event_choice({"option_id": "ornate"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, RelicReward)
    assert mgr._current_reward.relic_id == "ANCHOR"
    assert any(
        isinstance(reward, AddCardsReward)
        and [card.card_id.name for card in reward.cards] == ["CLUMSY"]
        for reward in mgr._pending_rewards
    )


def test_event_grave_of_the_forgotten_confront_routes_to_enchant_reward_chain():
    mgr = RunManager(seed=836, character_id="Ironclad")
    mgr.run_state.player.deck.append(make_backstab())
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = GraveOfTheForgotten()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)

    result = mgr._do_event_choice({"option_id": "confront"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, EnchantCardsReward)
    assert mgr.run_state.pending_choice is not None
    assert any(card.card_id.name == "DECAY" for card in mgr.run_state.player.deck)


def test_event_trial_merchant_innocent_routes_to_upgrade_reward_chain():
    mgr = RunManager(seed=837, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = Trial()
    mgr._event_options = []

    result = mgr._do_event_choice({"option_id": "merchant_innocent"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, UpgradeCardsReward)
    assert mgr.run_state.pending_choice is not None
    assert any(card.card_id.name == "SHAME" for card in mgr.run_state.player.deck)


def test_event_trial_nondescript_innocent_routes_to_transform_reward_chain():
    mgr = RunManager(seed=838, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = Trial()
    mgr._event_options = []

    result = mgr._do_event_choice({"option_id": "nondescript_innocent"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, TransformCardsReward)
    assert mgr.run_state.pending_choice is not None
    assert any(card.card_id.name == "DOUBT" for card in mgr.run_state.player.deck)


def test_event_reflections_shatter_auto_applies_bulk_add_cards_reward_and_returns_to_map():
    mgr = RunManager(seed=839, character_id="Ironclad")
    mgr.run_state.player.deck = [create_card(CardId.BASH), create_card(CardId.ANGER)]
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = Reflections()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)

    result = mgr._do_event_choice({"option_id": "shatter"})

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len(mgr.run_state.player.deck) == 5
    assert sum(1 for card in mgr.run_state.player.deck if card.card_id == CardId.BASH) == 2
    assert sum(1 for card in mgr.run_state.player.deck if card.card_id == CardId.ANGER) == 2
    assert any(card.card_id.name == "BAD_LUCK" for card in mgr.run_state.player.deck)


def test_event_random_relic_helper_in_act2_event_surfaces_reward_screen():
    mgr = RunManager(seed=840, character_id="Ironclad")
    mgr.run_state.player.gold = 999
    mgr.run_state.player.roll_relic_reward_id = lambda **_: "ANCHOR"
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = LuminousChoir()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)

    result = mgr._do_event_choice({"option_id": "tribute"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, RelicReward)
    assert mgr._current_reward.relic_id == "ANCHOR"


def test_event_fake_merchant_buy_surfaces_reward_screen_and_resumes_event_options():
    mgr = RunManager(seed=841, character_id="Ironclad")
    mgr.run_state.player.gold = 200
    mgr._phase = RunManager.PHASE_EVENT
    event = FakeMerchant()
    event._inventories[id(mgr.run_state)] = ["FAKE_ANCHOR", "FAKE_MANGO"]
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)

    first = mgr._do_event_choice({"option_id": "buy"})
    assert first["phase"] == RunManager.PHASE_EVENT

    second = mgr._do_event_choice({"option_id": "buy_0"})
    assert second["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, RelicReward)
    assert mgr._current_reward.relic_id == "FAKE_ANCHOR"

    final = mgr.take_action({"action": "pick_relic_reward", "relic_id": "FAKE_ANCHOR"})
    assert final["phase"] == RunManager.PHASE_EVENT
    assert any(option.option_id == "buy_0" for option in mgr._event_options)


def test_event_orobas_configured_relic_reward_preserves_setup_attrs():
    mgr = RunManager(seed=842, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    event = Orobas()
    event._choices = {"relic_1": ("SEA_GLASS", {"_character_id": "Silent"})}
    mgr._event_model = event
    mgr._event_options = [EventOption("relic_1", "Sea Glass", "Gain a relic")]

    result = mgr._do_event_choice({"option_id": "relic_1"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, RelicReward)
    assert mgr._current_reward.relic_id == "SEA_GLASS"

    final = mgr.take_action({"action": "pick_relic_reward", "relic_id": "SEA_GLASS"})
    assert final["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.player.relics[-1] == "SEA_GLASS"
    assert mgr.run_state.player.relic_objects[-1]._character_id == "Silent"
    assert mgr._current_reward.character_ids == ("Silent",)


def test_event_fixed_card_and_remove_plus_card_paths_use_reward_chain():
    mgr = RunManager(seed=843, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT

    crystal = CrystalSphere()
    mgr._event_model = crystal
    mgr._event_options = crystal.generate_initial_options(mgr.run_state)
    result = mgr._do_event_choice({"option_id": "debt"})
    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert any(card.card_id.name == "DEBT" for card in mgr.run_state.player.deck)

    mgr._phase = RunManager.PHASE_EVENT
    holes = FieldOfManSizedHoles()
    mgr._event_model = holes
    mgr._event_options = holes.generate_initial_options(mgr.run_state)
    result = mgr._do_event_choice({"option_id": "resist"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, RemoveCardReward)
    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert any(card.card_id.name == "NORMALITY" for card in mgr.run_state.player.deck)

    mgr._phase = RunManager.PHASE_EVENT
    punch = PunchOff()
    mgr._event_model = punch
    mgr._event_options = punch.generate_initial_options(mgr.run_state)
    result = mgr._do_event_choice({"option_id": "nab"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, RelicReward)
    assert any(
        isinstance(reward, AddCardsReward)
        and [card.card_id.name for card in reward.cards] == ["INJURY"]
        for reward in mgr._pending_rewards
    )
