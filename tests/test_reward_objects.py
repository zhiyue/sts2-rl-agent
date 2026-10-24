"""Tests for reward objects and rewards-set assembly."""

import pytest

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_curse_of_the_bell
from sts2_env.characters.all import IRONCLAD
from sts2_env.core.enums import CardId, CardRarity, CardTag, CardType, MapPointType, RoomType
from sts2_env.relics.base import RelicId
from sts2_env.run.reward_objects import (
    AddCardsReward,
    CardReward,
    CARD_REMOVAL_REWARD_SET_INDEX,
    CARD_REWARD_SET_INDEX,
    ENCOUNTER_GOLD_REWARD_RANGES,
    GOLD_REWARD_SET_INDEX,
    GoldReward,
    POTION_REWARD_SET_INDEX,
    PotionReward,
    RELIC_REWARD_SET_INDEX,
    RelicReward,
    RemoveCardReward,
    RewardsSet,
    SPECIAL_REWARD_SET_INDEX,
    TUTORIAL_BLOCK_POTION_ID,
    TUTORIAL_MONSTER_CARD_REWARDS,
    TUTORIAL_MONSTER_POTION_REWARDS,
)
from sts2_env.run.modifiers import ModifierModel
from sts2_env.run.rooms import CombatRoom, Room, TreasureRoom
from sts2_env.run.run_state import (
    RunState,
    UNLOCK_STATE_EPOCH_UNLOCK_COUNT_KEY,
    UNLOCK_STATE_NUMBER_OF_RUNS_KEY,
)


class _ExtraRewardCardRelic:
    def modify_card_reward_creation_options(self, player, options, reward, room, run_state):
        return options

    def modify_card_reward_options_late(self, player, cards, reward, room, run_state):
        return [*cards, create_card(CardId.SHRUG_IT_OFF)]

    def allow_card_reward_reroll(self, player, reward, room, run_state):
        return False


class _EarlyExtraCardRewardModifier(ModifierModel):
    def __init__(self):
        super().__init__("early_extra_card_reward")
        self.modified_rewards = []
        self.after_saw_all_populated = False

    def modify_rewards(self, player, rewards, room, run_state):
        self.modified_rewards = [*rewards, CardReward(player.player_id, context="boss")]
        return self.modified_rewards

    def modify_rewards_late(self, player, rewards, room, run_state):
        for reward in rewards:
            if isinstance(reward, CardReward):
                reward.include_colorless = True
        return rewards

    def after_modifying_rewards(self, player, run_state):
        self.after_saw_all_populated = all(
            not isinstance(reward, CardReward) or reward.is_populated
            for reward in self.modified_rewards
        )


def test_rewards_set_merges_combat_room_extra_rewards_for_player():
    run_state = RunState(seed=42, character_id="Ironclad")
    room = CombatRoom(room_type=RoomType.MONSTER)
    extra = CardReward(run_state.player.player_id, context="regular")
    room.add_extra_reward(run_state.player.player_id, extra)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
    generated = rewards.generate_without_offering(run_state)

    assert any(reward is extra for reward in generated)


def test_reward_set_indices_match_reference_reward_order():
    player_id = 1

    rewards = [
        GoldReward(player_id, 1, 1),
        PotionReward(player_id),
        RelicReward(player_id),
        AddCardsReward(player_id, [create_card(CardId.ANGER)]),
        CardReward(player_id),
        RemoveCardReward(player_id),
    ]

    assert [reward.rewards_set_index for reward in rewards] == [
        GOLD_REWARD_SET_INDEX,
        POTION_REWARD_SET_INDEX,
        RELIC_REWARD_SET_INDEX,
        SPECIAL_REWARD_SET_INDEX,
        CARD_REWARD_SET_INDEX,
        CARD_REMOVAL_REWARD_SET_INDEX,
    ]


def test_reward_modification_runs_early_then_late_before_populating_new_rewards():
    run_state = RunState(seed=47, character_id="Ironclad")
    modifier = _EarlyExtraCardRewardModifier()
    run_state.modifiers = [modifier]
    room = CombatRoom(room_type=RoomType.MONSTER)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
    generated = rewards.generate_without_offering(run_state)

    card_rewards = [reward for reward in generated if isinstance(reward, CardReward)]
    assert len(card_rewards) == 2
    assert all(reward.include_colorless is True for reward in card_rewards)
    assert all(reward.is_populated for reward in card_rewards)
    assert modifier.after_saw_all_populated is True


def test_combat_room_gold_ranges_match_encounter_model_defaults():
    assert ENCOUNTER_GOLD_REWARD_RANGES == {
        RoomType.MONSTER: (10, 20),
        RoomType.ELITE: (35, 45),
        RoomType.BOSS: (100, 100),
    }


def test_rewards_set_uses_encounter_gold_ranges_for_combat_rooms():
    run_state = RunState(seed=43, character_id="Ironclad")
    run_state.initialize_run()

    for room_type, expected_range in ENCOUNTER_GOLD_REWARD_RANGES.items():
        room = CombatRoom(room_type=room_type)
        rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
        gold_reward = next(reward for reward in rewards.rewards if isinstance(reward, GoldReward))
        assert (gold_reward.min_gold, gold_reward.max_gold) == expected_range


def test_rewards_set_scales_monster_gold_by_gold_proportion_only():
    run_state = RunState(seed=44, character_id="Ironclad")
    run_state.initialize_run()

    monster_room = CombatRoom(room_type=RoomType.MONSTER, gold_proportion=0.5)
    monster_rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(monster_room, run_state)
    monster_gold = next(reward for reward in monster_rewards.rewards if isinstance(reward, GoldReward))

    elite_room = CombatRoom(room_type=RoomType.ELITE, gold_proportion=0.5)
    elite_rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(elite_room, run_state)
    elite_gold = next(reward for reward in elite_rewards.rewards if isinstance(reward, GoldReward))

    assert (monster_gold.min_gold, monster_gold.max_gold) == (5, 10)
    assert (elite_gold.min_gold, elite_gold.max_gold) == (35, 45)


def test_rewards_set_omits_monster_gold_when_gold_proportion_is_zero():
    run_state = RunState(seed=45, character_id="Ironclad")
    run_state.initialize_run()
    room = CombatRoom(room_type=RoomType.MONSTER, gold_proportion=0)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)

    assert not any(isinstance(reward, GoldReward) for reward in rewards.rewards)


def test_rewards_set_applies_poverty_ascension_to_encounter_gold_ranges():
    run_state = RunState(seed=46, character_id="Ironclad", ascension_level=3)
    run_state.initialize_run()

    # C# EncounterModel.Min/MaxGoldReward (Poverty): multiply by 0.75 then
    # `return (int)num` -- truncation: 10->7, 20->15, 35->26, 45->33, 100->75.
    expected = {
        RoomType.MONSTER: (7, 15),
        RoomType.ELITE: (26, 33),
        RoomType.BOSS: (75, 75),
    }
    for room_type, expected_range in expected.items():
        room = CombatRoom(room_type=room_type)
        rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
        gold_reward = next(reward for reward in rewards.rewards if isinstance(reward, GoldReward))
        assert (gold_reward.min_gold, gold_reward.max_gold) == expected_range


def _tutorial_run_state(seed: int = 48) -> RunState:
    run_state = RunState(seed=seed, character_id=IRONCLAD.character_id)
    run_state.player.unlock_state[UNLOCK_STATE_NUMBER_OF_RUNS_KEY] = 0
    run_state.player.unlock_state[UNLOCK_STATE_EPOCH_UNLOCK_COUNT_KEY] = 0
    return run_state


def _set_tutorial_map_point_history(
    run_state: RunState,
    room_type: RoomType,
    map_point_type: MapPointType,
    count: int,
) -> None:
    run_state.map_point_history.clear()
    for _ in range(count):
        run_state.append_to_map_point_history(map_point_type, room_type)


def test_tutorial_monster_rewards_use_fixed_card_sets_and_potions():
    run_state = _tutorial_run_state()

    for count, expected_cards in enumerate(TUTORIAL_MONSTER_CARD_REWARDS, start=1):
        _set_tutorial_map_point_history(run_state, RoomType.MONSTER, MapPointType.MONSTER, count)
        rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(
            CombatRoom(room_type=RoomType.MONSTER),
            run_state,
        )

        card_reward = next(reward for reward in rewards.rewards if isinstance(reward, CardReward))
        assert [card.card_id for card in card_reward.cards] == list(expected_cards)
        potions = [reward.potion_id for reward in rewards.rewards if isinstance(reward, PotionReward)]
        expected_potion = TUTORIAL_MONSTER_POTION_REWARDS.get(count)
        assert potions == ([expected_potion] if expected_potion else [])


def test_tutorial_elite_and_boss_rewards_match_reference_fixed_rewards():
    run_state = _tutorial_run_state(seed=49)

    _set_tutorial_map_point_history(run_state, RoomType.ELITE, MapPointType.ELITE, 1)
    first_elite = RewardsSet(run_state.player.player_id).with_rewards_from_room(
        CombatRoom(room_type=RoomType.ELITE),
        run_state,
    )
    first_elite_cards = next(reward for reward in first_elite.rewards if isinstance(reward, CardReward))
    first_elite_relic = next(reward for reward in first_elite.rewards if isinstance(reward, RelicReward))
    first_elite_potion = next(reward for reward in first_elite.rewards if isinstance(reward, PotionReward))
    assert [card.card_id for card in first_elite_cards.cards] == [CardId.BLUDGEON, CardId.PYRE, CardId.EVIL_EYE]
    assert first_elite_relic.relic_id == RelicId.VAJRA.name
    assert first_elite_potion.potion_id == TUTORIAL_BLOCK_POTION_ID

    _set_tutorial_map_point_history(run_state, RoomType.ELITE, MapPointType.ELITE, 2)
    second_elite = RewardsSet(run_state.player.player_id).with_rewards_from_room(
        CombatRoom(room_type=RoomType.ELITE),
        run_state,
    )
    second_elite_cards = next(reward for reward in second_elite.rewards if isinstance(reward, CardReward))
    second_elite_relic = next(reward for reward in second_elite.rewards if isinstance(reward, RelicReward))
    assert [card.card_id for card in second_elite_cards.cards] == [
        CardId.PILLAGE,
        CardId.RAMPAGE,
        CardId.FLAME_BARRIER_CARD,
    ]
    assert second_elite_relic.relic_id == RelicId.ORNAMENTAL_FAN.name
    assert not any(isinstance(reward, PotionReward) for reward in second_elite.rewards)

    _set_tutorial_map_point_history(run_state, RoomType.BOSS, MapPointType.BOSS, 1)
    boss = RewardsSet(run_state.player.player_id).with_rewards_from_room(
        CombatRoom(room_type=RoomType.BOSS),
        run_state,
    )
    boss_cards = next(reward for reward in boss.rewards if isinstance(reward, CardReward))
    assert [card.card_id for card in boss_cards.cards] == [
        CardId.PRIMAL_FORCE,
        CardId.DEMON_FORM_CARD,
        CardId.THRASH,
    ]


def test_tutorial_rewards_require_explicit_first_run_unlock_state():
    run_state = RunState(seed=50, character_id=IRONCLAD.character_id)
    _set_tutorial_map_point_history(run_state, RoomType.MONSTER, MapPointType.MONSTER, 1)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(
        CombatRoom(room_type=RoomType.MONSTER),
        run_state,
    )
    card_reward = next(reward for reward in rewards.rewards if isinstance(reward, CardReward))

    assert not card_reward.cards


def test_tutorial_rewards_require_combat_room_instance():
    run_state = _tutorial_run_state(seed=51)
    _set_tutorial_map_point_history(run_state, RoomType.MONSTER, MapPointType.MONSTER, 1)

    with pytest.raises(ValueError, match="invalid room type"):
        RewardsSet(run_state.player.player_id).with_rewards_from_room(
            Room(room_type=RoomType.MONSTER),
            run_state,
        )


def test_rewards_set_allows_treasure_room_without_default_rewards():
    run_state = RunState(seed=52, character_id=IRONCLAD.character_id)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(
        TreasureRoom(),
        run_state,
    )

    assert rewards.rewards == []


def test_cauldron_after_obtained_queues_five_potion_rewards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()

    assert run_state.player.obtain_relic("CAULDRON")

    assert len(run_state.pending_rewards) == 5
    assert all(isinstance(reward, PotionReward) for reward in run_state.pending_rewards)


def test_orrery_after_obtained_queues_five_card_rewards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()

    assert run_state.player.obtain_relic("ORRERY")

    assert len(run_state.pending_rewards) == 5
    assert all(isinstance(reward, CardReward) for reward in run_state.pending_rewards)


def test_card_reward_can_upgrade_cards_after_generation():
    run_state = RunState(seed=45, character_id="Ironclad")
    run_state.initialize_run()
    reward = CardReward(
        run_state.player.player_id,
        option_count=3,
        forced_rarities=(CardRarity.COMMON, CardRarity.COMMON, CardRarity.COMMON),
        generation_context=None,
        roll_upgrade=False,
        card_type=CardType.ATTACK,
        upgrade_after_generation=True,
    )

    reward.populate(run_state, None)

    assert len(reward.cards) == 3
    assert all(card.card_type == CardType.ATTACK for card in reward.cards)
    assert all(card.rarity == CardRarity.COMMON for card in reward.cards)
    assert all(card.upgraded for card in reward.cards)


def test_card_reward_upgrade_after_generation_runs_after_late_reward_modifiers():
    run_state = RunState(seed=46, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.get_relic_objects = lambda: [_ExtraRewardCardRelic()]
    reward = CardReward(
        run_state.player.player_id,
        option_count=1,
        forced_rarities=(CardRarity.COMMON,),
        generation_context=None,
        roll_upgrade=False,
        card_type=CardType.ATTACK,
        upgrade_after_generation=True,
    )

    reward.populate(run_state, None)

    added = next(card for card in reward.cards if card.card_id == CardId.SHRUG_IT_OFF)
    assert added.upgraded


def test_other_source_card_reward_uses_noncombat_base_odds_without_changing_pity():
    run_state = RunState(seed=2, character_id="Ironclad")
    run_state.initialize_run()
    run_state.card_rarity_odds.current_value = 0.4
    reward = CardReward(
        run_state.player.player_id,
        option_count=1,
        generation_context=None,
        card_creation_source="other",
    )

    reward.populate(run_state, None)

    assert len(reward.cards) == 1
    assert reward.cards[0].rarity == CardRarity.UNCOMMON
    assert run_state.card_rarity_odds.current_value == 0.4


def test_other_source_card_reward_filters_custom_pool_before_selection():
    run_state = RunState(seed=1, character_id="Ironclad")
    run_state.initialize_run()
    reward = CardReward(
        run_state.player.player_id,
        option_count=1,
        forced_rarities=(CardRarity.COMMON,),
        generation_context=None,
        card_creation_source="other",
        use_default_character_pool=False,
        has_custom_card_pool=True,
        custom_card_ids=(CardId.BASH, CardId.BLOODLETTING),
    )

    reward.populate(run_state, None)

    assert [card.card_id for card in reward.cards] == [CardId.BLOODLETTING]


def test_calling_bell_adds_curse_and_queues_three_relic_rewards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()

    assert run_state.player.obtain_relic("CALLING_BELL")

    assert any(card.card_id == make_curse_of_the_bell().card_id for card in run_state.player.deck)
    assert len(run_state.pending_rewards) == 3
    assert all(isinstance(reward, RelicReward) for reward in run_state.pending_rewards)


def test_war_paint_upgrades_two_skills_on_obtain():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()

    assert run_state.player.obtain_relic("WAR_PAINT")

    upgraded_skills = [card for card in run_state.player.deck if card.upgraded and card.card_type.name == "SKILL"]
    assert len(upgraded_skills) >= 2


def test_whetstone_upgrades_two_attacks_on_obtain():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()

    assert run_state.player.obtain_relic("WHETSTONE")

    upgraded_attacks = [card for card in run_state.player.deck if card.upgraded and card.card_type.name == "ATTACK"]
    assert len(upgraded_attacks) >= 2


def test_archaic_tooth_transforms_a_basic_card():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    before = [card.card_id for card in run_state.player.deck]

    assert run_state.player.obtain_relic("ARCHAIC_TOOTH")

    after = [card.card_id for card in run_state.player.deck]
    assert before != after


def test_astrolabe_transforms_and_upgrades_three_cards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    before = [card.card_id for card in run_state.player.deck[:3]]

    assert run_state.player.obtain_relic("ASTROLABE")

    after = [card.card_id for card in run_state.player.deck[:3]]
    assert before != after
    assert sum(1 for card in run_state.player.deck if card.upgraded) >= 3


def test_precise_scissors_removes_one_card_from_deck():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    starting_deck = len(run_state.player.deck)

    assert run_state.player.obtain_relic("PRECISE_SCISSORS")

    assert len(run_state.player.deck) == starting_deck - 1


def test_pandoras_box_only_transforms_basic_strike_defend_cards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    bash_count_before = sum(1 for card in run_state.player.deck if card.card_id == CardId.BASH)

    assert run_state.player.obtain_relic("PANDORAS_BOX")

    assert sum(1 for card in run_state.player.deck if card.card_id == CardId.BASH) == bash_count_before
    assert sum(
        1
        for card in run_state.player.deck
        if card.rarity == CardRarity.BASIC and (CardTag.STRIKE in card.tags or CardTag.DEFEND in card.tags)
    ) == 0


def test_duplicate_card_helper_excludes_quest_cards_from_candidates():
    run_state = RunState(seed=52, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))

    assert run_state.player.duplicate_card_from_deck(cards=run_state.player.duplicable_deck_cards())

    assert sum(1 for card in run_state.player.deck if card.card_id == CardId.BYRDONIS_EGG) == 1
