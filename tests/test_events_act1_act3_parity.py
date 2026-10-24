"""Parity tests for Act 1 and Act 3 events."""

import sts2_env.events.act1  # noqa: F401
import sts2_env.events.act3  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.characters.all import get_character
from sts2_env.core.enums import CardId, CardRarity, CardType, MapPointType, PowerId, RoomType
from sts2_env.core.rng import Rng
from sts2_env.events.act3 import (
    DeprecatedAncientEvent,
    DeprecatedEvent,
    Neow,
    WarHistorianRepy,
)
from sts2_env.monsters.act1_weak import create_twig_slime_s
from sts2_env.relics.base import RelicId, RelicRarity
from sts2_env.run.modifiers import (
    AllStarModifier,
    BigGameHunterModifier,
    CharacterCardsModifier,
    CursedRunModifier,
    DeadlyEventsModifier,
    DEPRECATED_MODIFIER_ID,
    DeprecatedModifier,
    DraftModifier,
    InsanityModifier,
    MidasModifier,
    ModifierModel,
    MurderousModifier,
    NightTerrorsModifier,
    PANDORAS_BOX_RELIC_ID,
    SealedDeckModifier,
    SEALED_DECK_RARITY_SORT_ORDER,
    TerminalModifier,
    VintageModifier,
)
from sts2_env.run.rest_site import generate_rest_site_options
from sts2_env.run.reward_objects import CardReward, GoldReward, PotionReward, RelicReward, RewardsSet
from sts2_env.run.rewards import CardRewardGenerationOptions
from sts2_env.run.rooms import RoomVisitContext, create_room
from sts2_env.run.events import pick_event
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import PlayerState, RunState, UNLOCK_STATE_NUMBER_OF_RUNS_KEY
from sts2_env.run.shop import _create_character_shop_card


IRONCLAD_CHARACTER_ID = "Ironclad"
INSANITY_PARITY_SEED = 818
INSANITY_EXPECTED_CARD_COUNT = 30
INSANITY_EXPECTED_DECK_SIZE = INSANITY_EXPECTED_CARD_COUNT


class _NeowChoiceRng:
    def __init__(self, preferred_relic: str):
        self._preferred_relic = preferred_relic
        self.choice_seqs: list[list[str]] = []
        self.shuffled_positive_pool: list[str] | None = None

    def choice(self, seq):
        self.choice_seqs.append(list(seq))
        if self._preferred_relic in seq:
            return self._preferred_relic
        return seq[0]

    def shuffle(self, seq) -> None:
        self.shuffled_positive_pool = list(seq)


def test_neow_is_not_pool_allowed_but_exposes_three_choices():
    run_state = RunState(seed=804, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()

    assert event.is_allowed(run_state) is False
    up_front_counter = run_state.rng.up_front.counter
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["positive_1", "positive_2", "cursed"]
    assert run_state.rng.up_front.counter == up_front_counter

    relics_before = len(run_state.player.relics)
    cursed = event.choose(run_state, "cursed")
    assert cursed.finished
    assert "cursed relic" in cursed.description.lower()
    assert len(run_state.player.relics) == relics_before + 1

    relics_before = len(run_state.player.relics)
    positive = event.choose(run_state, "positive_1")
    assert positive.finished
    assert "positive relic" in positive.description.lower()
    assert len(run_state.player.relics) == relics_before + 1


def test_neow_positive_pool_respects_cursed_choice_conflicts():
    run_state = RunState(seed=807, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()
    event.rng = _NeowChoiceRng(RelicId.LEAFY_POULTICE.name)

    options = event.generate_initial_options(run_state)

    labels = {option.label for option in options}
    assert any("LEAFY POULTICE" in label.upper() for label in labels)
    assert not any("NEW LEAF" in option.label.upper() for option in options if option.option_id != "cursed")


def test_neow_positive_pool_offers_scroll_boxes_when_bundles_are_possible_and_skips_silver_crucible_in_multiplayer():
    # C# Neow.cs:80 lists ScrollBoxes among PositiveOptions, gated by
    # CanGenerateBundles via IsAllowedAtNeow (C# ScrollBoxes.cs:23-28);
    # C# Neow.cs:96-106 has no ScrollBoxes in CurseOptions.
    # C# SilverCrucible.cs:81 IsAllowed requires Players.Count == 1
    # (filtered at Neow via IsAllowedAtNeow, C# RelicModel.cs:443).
    run_state = RunState(seed=809, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()
    recorder = _NeowChoiceRng(RelicId.CURSED_PEARL.name)
    event.rng = recorder

    event.generate_initial_options(run_state)
    cursed_pool = recorder.choice_seqs[0]
    assert RelicId.SILVER_CRUCIBLE.name in cursed_pool  # single player
    assert RelicId.SCROLL_BOXES.name not in cursed_pool
    assert RelicId.SCROLL_BOXES.name in (recorder.shuffled_positive_pool or [])

    multiplayer = RunState(seed=810, character_id="Ironclad")
    multiplayer.initialize_run()
    multiplayer.players.append(multiplayer.player)
    multiplayer_event = Neow()
    multiplayer_recorder = _NeowChoiceRng(RelicId.CURSED_PEARL.name)
    multiplayer_event.rng = multiplayer_recorder
    multiplayer_event.generate_initial_options(multiplayer)
    assert RelicId.SILVER_CRUCIBLE.name not in multiplayer_recorder.choice_seqs[0]
    # C# MassiveScroll.cs:19 IsAllowed requires Players.Count > 1.
    assert RelicId.MASSIVE_SCROLL.name in (multiplayer_recorder.shuffled_positive_pool or [])
    assert RelicId.MASSIVE_SCROLL.name not in (recorder.shuffled_positive_pool or [])


def test_neow_static_pool_matches_csharp_neow_options():
    # C# Neow.cs:66-82 PositiveOptions and Neow.cs:96-106 CurseOptions.
    # MassiveScroll (multiplayer only, C# MassiveScroll.cs:19) and
    # SilverCrucible (single player only, C# SilverCrucible.cs:81) are
    # appended per run after the IsAllowedAtNeow filter
    # (C# RelicModel.cs:443), so they are absent from the static pools here.
    # ScrollBoxes is gated on CanGenerateBundles (C# ScrollBoxes.cs:23-28)
    # and appended per run for the same reason.
    assert Neow._POSITIVE_POOL == (
        RelicId.ARCANE_SCROLL.name,
        RelicId.BOOMING_CONCH.name,
        RelicId.FISHING_ROD.name,
        RelicId.GOLDEN_PEARL.name,
        RelicId.KALEIDOSCOPE.name,
        RelicId.LEAD_PAPERWEIGHT.name,
        RelicId.LOST_COFFER.name,
        RelicId.NEW_LEAF.name,
        RelicId.NEOWS_TORMENT.name,
        RelicId.PHIAL_HOLSTER.name,
        RelicId.PRECISE_SCISSORS.name,
        RelicId.WINGED_BOOTS.name,
    )
    assert Neow._CURSED_POOL == (
        RelicId.CURSED_PEARL.name,
        RelicId.HEFTY_TABLET.name,
        RelicId.LARGE_CAPSULE.name,
        RelicId.LEAFY_POULTICE.name,
        RelicId.NEOWS_BONES.name,
        RelicId.PRECARIOUS_SHEARS.name,
        RelicId.SILKEN_TRESS.name,
    )


def test_neow_hefty_tablet_cursed_choice_excludes_arcane_scroll():
    # C# Neow.cs:230-233: if the cursed option is HeftyTablet,
    # ArcaneScroll is removed from the positive list.
    run_state = RunState(seed=813, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()
    recorder = _NeowChoiceRng(RelicId.HEFTY_TABLET.name)
    event.rng = recorder

    options = event.generate_initial_options(run_state)

    labels = {option.label for option in options}
    assert any("HEFTY TABLET" in label.upper() for label in labels)
    assert not any("ARCANE SCROLL" in option.label.upper() for option in options)
    assert RelicId.ARCANE_SCROLL.name not in (recorder.shuffled_positive_pool or [])


def test_neow_appends_neows_talisman_xor_pomander_pair():
    # C# Neow.cs:261-268: after the LavaRock/SmallCapsule and
    # NutritiousOyster/StoneHumidifier pairs, exactly one of
    # NeowsTalisman or Pomander is appended to the positive list
    # (NeowsTalisman is new in v0.107.1, C# Neow.cs:57/86).
    run_state = RunState(seed=814, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()
    recorder = _NeowChoiceRng(RelicId.CURSED_PEARL.name)
    event.rng = recorder

    event.generate_initial_options(run_state)

    positive_pool = recorder.shuffled_positive_pool or []
    assert RelicId.NEOWS_TALISMAN.name in positive_pool
    assert RelicId.POMANDER.name not in positive_pool
    # C# draw order for the exclusive pairs (Neow.cs:242-268): capsule pair,
    # then oyster pair, then talisman pair.
    pair_choices = [seq for seq in recorder.choice_seqs if len(seq) == 2]
    assert pair_choices == [
        [RelicId.LAVA_ROCK.name, RelicId.SMALL_CAPSULE.name],
        [RelicId.NUTRITIOUS_OYSTER.name, RelicId.STONE_HUMIDIFIER.name],
        [RelicId.NEOWS_TALISMAN.name, RelicId.POMANDER.name],
    ]


def test_neow_modifier_path_exposes_sequential_modifier_options():
    run_state = RunState(seed=811, character_id="Ironclad")
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.modifiers = [AllStarModifier(), DraftModifier()]
    run_state.initialize_run()
    event = Neow()

    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["modifier_0"]

    first = event.choose(run_state, "modifier_0")
    assert first.finished is False
    assert [option.option_id for option in first.next_options] == ["modifier_1"]
    assert len(run_state.player.deck) == 5

    second = event.choose(run_state, "modifier_1")
    rewards = second.rewards["reward_objects"]
    assert len(rewards) == 10
    assert all(isinstance(reward, CardReward) and reward.skippable is False for reward in rewards)


def test_neow_modifiers_without_neow_option_do_not_fall_back_to_relic_choices():
    run_state = RunState(seed=8111, character_id="Ironclad")
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.modifiers = [MurderousModifier()]
    run_state.initialize_run()
    event = Neow()

    options = event.generate_initial_options(run_state)

    assert options == []


def test_neow_sealed_deck_modifier_requests_ten_of_thirty_cards():
    run_state = RunState(seed=812, character_id="Ironclad")
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.modifiers = [SealedDeckModifier()]
    run_state.initialize_run()
    event = Neow()

    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["modifier_0"]

    result = event.choose(run_state, "modifier_0")
    assert result.finished is False
    assert run_state.pending_choice is not None
    assert len(run_state.pending_choice.options) == 30
    assert run_state.pending_choice.min_choices == 10
    assert run_state.pending_choice.max_choices == 10
    offered_cards = [option.card for option in run_state.pending_choice.options]
    assert offered_cards == sorted(
        offered_cards,
        key=lambda card: (SEALED_DECK_RARITY_SORT_ORDER[card.rarity], card.card_id.name),
    )


def test_neow_all_star_modifier_adds_five_colorless_cards():
    run_state = RunState(seed=815, character_id="Ironclad")
    run_state.player.deck = create_ironclad_starter_deck()
    modifier = AllStarModifier()
    modifier.on_run_created(run_state)

    result = modifier.generate_neow_event_result(run_state)
    assert result.finished
    assert len(run_state.player.deck) == 15
    assert all(card.card_id not in {CardId.BASH, CardId.STRIKE_IRONCLAD, CardId.DEFEND_IRONCLAD} for card in run_state.player.deck[-5:])


def test_neow_insanity_modifier_adds_thirty_random_cards_and_removes_pandoras_box():
    run_state = RunState(seed=INSANITY_PARITY_SEED, character_id=IRONCLAD_CHARACTER_ID)
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.relic_grab_bag = [PANDORAS_BOX_RELIC_ID]
    run_state.player.relic_grab_bag_by_rarity = {RelicRarity.ANCIENT: [PANDORAS_BOX_RELIC_ID]}
    run_state.player.relic_grab_bag_fallback = [PANDORAS_BOX_RELIC_ID]
    modifier = InsanityModifier()
    modifier.on_run_created(run_state)

    result = modifier.generate_neow_event_result(run_state)

    assert result.finished
    assert len(run_state.player.deck) == INSANITY_EXPECTED_DECK_SIZE
    assert all(card.card_id in get_character(IRONCLAD_CHARACTER_ID).card_pool for card in run_state.player.deck)
    assert PANDORAS_BOX_RELIC_ID not in run_state.player.relic_grab_bag
    assert all(PANDORAS_BOX_RELIC_ID not in relic_ids for relic_ids in run_state.player.relic_grab_bag_by_rarity.values())
    assert PANDORAS_BOX_RELIC_ID not in run_state.player.relic_grab_bag_fallback


def test_murderous_modifier_applies_strength_to_combat_creatures_and_added_enemies():
    mgr = RunManager(seed=2401, character_id="Ironclad")
    mgr.run_state.modifiers = [MurderousModifier()]

    mgr._enter_combat(RoomType.MONSTER)

    combat = mgr.get_combat_state()
    assert combat is not None
    assert combat.player.get_power_amount(PowerId.STRENGTH) == 3
    assert all(enemy.get_power_amount(PowerId.STRENGTH) == 3 for enemy in combat.enemies)

    added_enemy, ai = create_twig_slime_s(Rng(2402))
    combat.add_enemy(added_enemy, ai)

    assert added_enemy.get_power_amount(PowerId.STRENGTH) == 3


def test_deprecated_modifier_is_no_op_save_placeholder():
    run_state = RunState(seed=2400, character_id="Ironclad")
    modifier = DeprecatedModifier()

    modifier.on_run_created(run_state)
    result = modifier.generate_neow_event_result(run_state)

    assert modifier.modifier_id == DEPRECATED_MODIFIER_ID
    assert modifier.clears_player_deck is False
    assert result is None


def test_terminal_modifier_adds_player_plating_in_combat_and_drains_base_room_hp():
    mgr = RunManager(seed=2403, character_id="Ironclad")
    mgr.run_state.modifiers = [TerminalModifier()]

    mgr._enter_combat(RoomType.MONSTER)

    combat = mgr.get_combat_state()
    assert combat is not None
    assert combat.player.get_power_amount(PowerId.PLATING) == 5

    run_state = RunState(seed=2404, character_id="Ironclad")
    modifier = TerminalModifier()
    base_room = RoomVisitContext(RoomType.EVENT)
    run_state.base_room = base_room
    modifier.after_room_entered(run_state, base_room)

    assert run_state.player.max_hp == 79
    assert run_state.player.current_hp == 79


def test_midas_modifier_doubles_gold_rewards_and_removes_smith():
    run_state = RunState(seed=2405, character_id="Ironclad")
    run_state.modifiers = [MidasModifier()]
    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(create_room(RoomType.MONSTER), run_state)

    generated = rewards.generate_without_offering(run_state)

    gold = next(reward for reward in generated if isinstance(reward, GoldReward))
    assert 20 <= gold.amount <= 40
    assert gold.min_gold == gold.max_gold == gold.amount
    assert not any(option.option_id == "SMITH" for option in generate_rest_site_options(run_state.player))


def test_night_terrors_modifier_full_rests_then_loses_max_hp():
    run_state = RunState(seed=2406, character_id="Ironclad")
    run_state.modifiers = [NightTerrorsModifier()]
    run_state.player.current_hp = 10

    heal = next(option for option in generate_rest_site_options(run_state.player) if option.option_id == "HEAL")
    result = heal.execute(run_state.player)

    assert result == "Healed 70 HP"
    assert run_state.player.max_hp == 75
    assert run_state.player.current_hp == 75


def test_vintage_modifier_replaces_regular_combat_card_reward_with_relic_reward():
    run_state = RunState(seed=2407, character_id="Ironclad")
    run_state.modifiers = [VintageModifier()]

    monster_rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(
        create_room(RoomType.MONSTER),
        run_state,
    ).generate_without_offering(run_state)
    elite_rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(
        create_room(RoomType.ELITE),
        run_state,
    ).generate_without_offering(run_state)

    assert not any(isinstance(reward, CardReward) for reward in monster_rewards)
    assert any(isinstance(reward, RelicReward) for reward in monster_rewards)
    assert any(isinstance(reward, CardReward) for reward in elite_rewards)


def test_big_game_hunter_modifier_increases_map_elites():
    base = RunState(seed=2408, character_id="Ironclad")
    base.initialize_run()
    hunter = RunState(seed=2408, character_id="Ironclad")
    hunter.modifiers = [BigGameHunterModifier()]
    hunter.initialize_run()

    base_elites = sum(1 for point in base.map.room_points() if point.point_type == MapPointType.ELITE)
    hunter_elites = sum(1 for point in hunter.map.room_points() if point.point_type == MapPointType.ELITE)

    assert hunter_elites > base_elites


def test_big_game_hunter_modifier_makes_elite_card_rewards_rare():
    run_state = RunState(seed=2409, character_id="Ironclad")
    run_state.modifiers = [BigGameHunterModifier()]
    reward = CardReward(run_state.player.player_id, context="elite")

    reward.populate(run_state, create_room(RoomType.ELITE))

    assert reward.cards
    assert reward.forced_rarities == ()
    assert reward.has_custom_card_pool is True
    assert all(card.card_id in reward.custom_card_ids for card in reward.cards)
    assert all(card.rarity == CardRarity.RARE for card in reward.cards)


def test_big_game_hunter_modifier_falls_back_to_character_rare_pool_when_candidates_have_no_rares():
    run_state = RunState(seed=2412, character_id="Ironclad")
    run_state.modifiers = [BigGameHunterModifier()]
    reward = CardReward(
        run_state.player.player_id,
        context="elite",
        custom_card_ids=(CardId.ANGER,),
        has_custom_card_pool=True,
    )

    reward.populate(run_state, create_room(RoomType.ELITE))

    assert reward.cards
    assert all(card.rarity == CardRarity.RARE for card in reward.cards)
    assert all(card.card_id != CardId.ANGER for card in reward.cards)


def test_big_game_hunter_modifier_keeps_colorless_rare_candidates():
    run_state = RunState(seed=2413, character_id="Ironclad")
    run_state.modifiers = [BigGameHunterModifier()]
    assert run_state.player.obtain_relic("DINGY_RUG")
    reward = CardReward(run_state.player.player_id, context="elite")

    reward.populate(run_state, create_room(RoomType.ELITE))

    ironclad_pool = set(get_character("Ironclad").card_pool)
    assert any(card_id not in ironclad_pool for card_id in reward.custom_card_ids)
    assert all(card.rarity == CardRarity.RARE for card in reward.cards)


def test_big_game_hunter_modifier_respects_locked_reward_rarities():
    run_state = RunState(seed=2410, character_id="Ironclad")
    run_state.modifiers = [BigGameHunterModifier()]
    reward = CardReward(
        run_state.player.player_id,
        context="elite",
        forced_rarities=(CardRarity.COMMON, CardRarity.COMMON, CardRarity.COMMON),
    )

    reward.populate(run_state, create_room(RoomType.ELITE))

    assert reward.cards
    assert all(card.rarity == CardRarity.COMMON for card in reward.cards)


def test_big_game_hunter_modifier_respects_no_rarity_modification_flag():
    run_state = RunState(seed=2414, character_id="Ironclad")
    run_state.modifiers = [BigGameHunterModifier()]
    reward = CardReward(
        run_state.player.player_id,
        context="elite",
        allow_rarity_modifications=False,
    )

    reward.populate(run_state, create_room(RoomType.ELITE))

    assert reward.cards
    assert reward.has_custom_card_pool is False
    assert any(card.rarity is not CardRarity.RARE for card in reward.cards)


def test_character_cards_modifier_expands_card_reward_pool():
    run_state = RunState(seed=2411, character_id="Ironclad")
    modifier = CharacterCardsModifier("Silent")
    options = CardRewardGenerationOptions()

    modified = modifier.modify_card_reward_creation_options(run_state.player, options, None, None, run_state)

    assert modified.character_ids == ()
    assert modified.has_custom_card_pool is True
    assert modified.use_default_character_pool is True
    assert modified.custom_card_ids
    ironclad_pool = set(get_character("Ironclad").card_pool)
    silent_pool = set(get_character("Silent").card_pool)
    assert any(card_id in ironclad_pool for card_id in modified.custom_card_ids)
    assert any(card_id in silent_pool for card_id in modified.custom_card_ids)

    locked = CardRewardGenerationOptions(allow_card_pool_modifications=False)
    assert modifier.modify_card_reward_creation_options(run_state.player, locked, None, None, run_state) is locked


def test_character_cards_modifier_expands_merchant_card_pool():
    class LastChoiceRng:
        def choice(self, values):
            return list(values)[-1]

        def next_float_range(self, low, high):
            return 1.0

    run_state = RunState(seed=2412, character_id="Ironclad")
    run_state.modifiers = [CharacterCardsModifier("Silent")]

    entry = _create_character_shop_card(
        run_state,
        LastChoiceRng(),
        "Skill",
        CardRarity.RARE,
    )

    assert entry.card.card_id in get_character("Silent").card_pool
    assert entry.card.card_type == CardType.SKILL
    assert entry.rarity == CardRarity.RARE


def test_cursed_run_modifier_adds_random_curse_after_each_act_entered():
    run_state = RunState(seed=2413, character_id="Ironclad")
    run_state.modifiers = [CursedRunModifier()]

    run_state.initialize_run()

    first_act_curses = [card for card in run_state.player.deck if card.card_type == CardType.CURSE]
    assert len(first_act_curses) == 1
    assert first_act_curses[0].can_be_generated_by_modifiers

    run_state.enter_act(1)

    assert len([card for card in run_state.player.deck if card.card_type == CardType.CURSE]) == 2


def test_deadly_events_modifier_updates_unknown_odds_and_removes_juzu():
    run_state = RunState(seed=2414, character_id="Ironclad")
    run_state.modifiers = [DeadlyEventsModifier()]

    run_state.initialize_run()
    run_state.player.unlock_state[UNLOCK_STATE_NUMBER_OF_RUNS_KEY] = 1

    assert "JUZU_BRACELET" not in run_state.player.relic_grab_bag
    assert run_state.unknown_odds._base[RoomType.ELITE] == 0.1
    assert run_state.unknown_odds._current[RoomType.ELITE] == 0.1

    run_state.unknown_odds._current = {
        RoomType.MONSTER: 0.0,
        RoomType.ELITE: 0.0,
        RoomType.TREASURE: 0.0,
        RoomType.SHOP: 0.0,
    }
    run_state.unknown_odds.roll(run_state.rng.unknown_map_point, run_state)

    assert run_state.unknown_odds._current[RoomType.TREASURE] == 0.04


def test_neow_draft_modifier_clears_deck_and_queues_ten_non_skippable_rewards():
    run_state = RunState(seed=816, character_id="Ironclad")
    run_state.player.deck = create_ironclad_starter_deck()
    modifier = DraftModifier()
    modifier.on_run_created(run_state)

    assert run_state.player.deck == []
    result = modifier.generate_neow_event_result(run_state)
    rewards = result.rewards["reward_objects"]
    assert len(rewards) == 10
    assert all(isinstance(reward, CardReward) and reward.skippable is False for reward in rewards)


def test_neow_specialized_modifier_adds_five_copies_of_same_card():
    from sts2_env.run.modifiers import SpecializedModifier

    run_state = RunState(seed=817, character_id="Ironclad")
    run_state.player.deck = create_ironclad_starter_deck()
    modifier = SpecializedModifier()

    result = modifier.generate_neow_event_result(run_state)
    assert result.finished
    assert len(run_state.player.deck) == 15
    added_ids = {card.card_id for card in run_state.player.deck[-5:]}
    assert len(added_ids) == 1
    assert len({card.instance_id for card in run_state.player.deck}) == len(run_state.player.deck)


def test_neow_modifier_pending_choice_resumes_to_next_modifier_in_run_manager():
    mgr = RunManager(seed=813, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr.run_state.modifiers = [SealedDeckModifier(), AllStarModifier()]
    for modifier in mgr.run_state.modifiers:
        modifier.on_run_created(mgr.run_state)
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = Neow()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)

    first = mgr._do_event_choice({"option_id": "modifier_0"})
    assert first["phase"] == RunManager.PHASE_EVENT
    assert mgr.run_state.pending_choice is not None

    for index in range(10):
        mgr.take_action({"action": "choose", "index": index})
    resolved = mgr.take_action({"action": "confirm_choice"})
    assert resolved["phase"] == RunManager.PHASE_EVENT
    assert [option.option_id for option in mgr._event_options] == ["modifier_1"]

    second = mgr._do_event_choice({"option_id": "modifier_1"})
    assert second["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len(mgr.run_state.player.deck) == 15


def test_neow_modifier_reward_chain_resumes_to_next_modifier_in_run_manager():
    mgr = RunManager(seed=814, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr.run_state.modifiers = [DraftModifier(), AllStarModifier()]
    for modifier in mgr.run_state.modifiers:
        modifier.on_run_created(mgr.run_state)
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = Neow()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)

    first = mgr._do_event_choice({"option_id": "modifier_0"})
    assert first["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, CardReward)
    assert mgr._current_reward.skippable is False

    for _ in range(10):
        result = mgr.take_action({"action": "pick_card", "index": 0})

    assert result["phase"] == RunManager.PHASE_EVENT
    assert [option.option_id for option in mgr._event_options] == ["modifier_1"]


def test_war_historian_repy_is_disabled_but_choice_results_are_stable():
    run_state = RunState(seed=805, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck.append(create_card(CardId.LANTERN_KEY))
    event = WarHistorianRepy()

    assert event.is_allowed(run_state) is False
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["unlock_cage", "unlock_chest"]

    unlock_cage = event.choose(run_state, "unlock_cage")
    assert unlock_cage.finished
    assert "history course" in unlock_cage.description.lower()
    assert run_state.extra_fields.get("freed_repy") is True
    assert CardId.LANTERN_KEY not in [card.card_id for card in run_state.player.deck]
    assert "HISTORY_COURSE" in run_state.player.relics

    run_state.player.deck.append(create_card(CardId.LANTERN_KEY))
    unlock_chest = event.choose(run_state, "unlock_chest")
    assert unlock_chest.finished
    assert "2 potions" in unlock_chest.description.lower()
    rewards = unlock_chest.rewards["reward_objects"]
    assert len([reward for reward in rewards if isinstance(reward, PotionReward)]) == 2
    assert len([reward for reward in rewards if isinstance(reward, RelicReward)]) == 2


def test_lantern_key_forces_act3_unknown_rooms_into_war_historian_repy():
    mgr = RunManager(seed=809, character_id="Ironclad")
    mgr.run_state.initialize_run()
    mgr.run_state.current_act_index = 2
    mgr.run_state.player.unlock_state[UNLOCK_STATE_NUMBER_OF_RUNS_KEY] = 1
    mgr.run_state.player.deck.append(create_card(CardId.LANTERN_KEY))

    assert mgr.run_state.resolve_room_type(MapPointType.UNKNOWN) is RoomType.EVENT

    mgr._enter_event()

    assert isinstance(mgr._event_model, WarHistorianRepy)


def test_lantern_key_card_hook_replaces_next_act3_event():
    run_state = RunState(seed=810, character_id="Ironclad")
    run_state.initialize_run()
    run_state.current_act_index = 2
    run_state.player.deck.append(create_card(CardId.LANTERN_KEY))

    event = pick_event(run_state, pool=["Neow"])

    assert isinstance(event, WarHistorianRepy)


def test_lantern_key_card_hook_filters_act3_unknown_room_types():
    run_state = RunState(seed=811, character_id="Ironclad")
    run_state.initialize_run()
    lantern_key = create_card(CardId.LANTERN_KEY)
    room_types = {RoomType.EVENT, RoomType.MONSTER, RoomType.SHOP}

    run_state.current_act_index = 1
    assert lantern_key.modify_unknown_map_point_room_types(run_state, room_types) is room_types

    run_state.current_act_index = 2
    assert lantern_key.modify_unknown_map_point_room_types(run_state, room_types) == {RoomType.EVENT}


def test_war_historian_repy_removes_lantern_key_on_both_paths():
    run_state = RunState(seed=808, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck.extend([create_card(CardId.LANTERN_KEY), create_card(CardId.LANTERN_KEY)])
    event = WarHistorianRepy()

    event.choose(run_state, "unlock_cage")
    assert CardId.LANTERN_KEY not in [card.card_id for card in run_state.player.deck]


def test_deprecated_act3_events_are_unreachable_and_optionless():
    run_state = RunState(seed=806, character_id="Ironclad")
    run_state.initialize_run()

    for event in (DeprecatedEvent(), DeprecatedAncientEvent()):
        assert event.is_allowed(run_state) is False
        assert event.generate_initial_options(run_state) == []
        result = event.choose(run_state, "anything")
        assert result.finished
        assert result.next_options == []
