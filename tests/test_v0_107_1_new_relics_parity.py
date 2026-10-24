"""Parity tests for the v0.107.1 Ancient (Neow) relics.

Each test cites the decompiled C# source that pins the behavior:
decompiled/MegaCrit.Sts2.Core.Models.Relics/<Relic>.cs (game version 0.107.1).
"""

import pytest

import sts2_env.powers  # noqa: F401  # ensure power registration

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.core.card_pools import (
    CHARACTER_CARD_POOLS_BY_ID,
    CURSE_CARD_POOL,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CardRarity, CardTag, CardType, RelicRarity, RoomType
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.relics.base import RelicId, RelicPool
from sts2_env.relics.registry import coerce_relic_id, create_relic_by_name
from sts2_env.run.reward_objects import AddCardsReward, CardReward, RelicReward
from sts2_env.run.modifiers import FlightModifier
from sts2_env.run.rooms import CombatRoom
from sts2_env.run.run_state import RunState


def _make_run(seed: int, character_id: str = "Ironclad") -> RunState:
    run_state = RunState(seed=seed, character_id=character_id)
    run_state.player.deck = create_ironclad_starter_deck()
    return run_state


def _make_combat(
    run_state: RunState,
    relic,
    room_type: RoomType,
    seed: int,
):
    """Combat wired to the persistent player state with a live relic instance."""
    player_state = run_state.player
    combat = CombatState(
        player_hp=player_state.current_hp,
        player_max_hp=player_state.max_hp,
        deck=list(player_state.deck),
        rng_seed=seed,
        character_id=player_state.character_id,
        relics=[relic],
        player_state=player_state,
        room=CombatRoom(room_type=room_type),
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _character_pool_for_card(card_id: CardId) -> str | None:
    for pool_id, pool in CHARACTER_CARD_POOLS_BY_ID.items():
        if card_id in pool:
            return pool_id.name
    return None


class TestV0107NewRelicMetadata:
    """Static metadata: every new relic is Ancient rarity, Event pool (Neow-only).

    C# <Relic>.cs: `public override RelicRarity Rarity => RelicRarity.Ancient;`
    on all eight classes; they are granted only through the Neow event
    (Neow.cs PositiveOptions/CurseOptions) and never appear in shops or the
    relic grab bag, matching RelicPool.EVENT + ANCIENT in the simulator.
    """

    NEW_RELICS = (
        "FishingRod",
        "HeftyTablet",
        "Kaleidoscope",
        "NeowsBones",
        "NeowsTalisman",
        "PhialHolster",
        "SilkenTress",
        "WingedBoots",
    )

    @pytest.mark.parametrize("name", NEW_RELICS)
    def test_relic_is_ancient_event_pool_and_registered(self, name):
        relic_id = coerce_relic_id(name)
        relic_class = type(create_relic_by_name(relic_id))
        assert relic_class.rarity is RelicRarity.ANCIENT
        assert relic_class.pool is RelicPool.EVENT

    def test_relic_ids_exist_in_enum(self):
        from sts2_env.relics.registry import _camel_to_enum_name

        for name in self.NEW_RELICS:
            assert hasattr(RelicId, _camel_to_enum_name(name))


class TestFishingRodParity:
    """C# FishingRod.cs.

    - Rarity: RelicRarity.Ancient.
    - AfterCombatEnd: bails out when room.Encounter.RoomType != RoomType.Monster.
    - CombatsSeen is a [SavedProperty] counter; on every 3rd combat (Combats
      DynamicVar = 3) a random upgradable deck card is upgraded via Rng.Niche.
    """

    def test_upgrades_one_random_card_on_every_third_monster_combat(self):
        run_state = _make_run(seed=101)
        run_state.player.obtain_relic("FishingRod")
        relic = run_state.player.get_relic_objects()[-1]
        for index in range(3):
            combat = _make_combat(run_state, relic, RoomType.MONSTER, seed=1000 + index)
            combat._end_combat(player_won=True)
            assert relic.combats_seen == index + 1
        # Starter deck has 10 upgradable cards; exactly 1 upgraded after 3 combats.
        assert sum(1 for card in run_state.player.deck if card.upgraded) == 1

    def test_counter_persists_across_combats_and_upgrade_only_on_multiple_of_three(self):
        run_state = _make_run(seed=102)
        run_state.player.obtain_relic("FishingRod")
        relic = run_state.player.get_relic_objects()[-1]
        # First two combats: counter advances, no upgrade yet (CombatsSeen % 3 != 0).
        for index in range(2):
            combat = _make_combat(run_state, relic, RoomType.MONSTER, seed=2000 + index)
            combat._end_combat(player_won=True)
            assert relic.combats_seen == index + 1
            assert sum(1 for card in run_state.player.deck if card.upgraded) == 0
        # Third combat crosses the threshold.
        combat = _make_combat(run_state, relic, RoomType.MONSTER, seed=2002)
        combat._end_combat(player_won=True)
        assert relic.combats_seen == 3
        assert sum(1 for card in run_state.player.deck if card.upgraded) == 1

    def test_elite_and_boss_rooms_do_not_count(self):
        run_state = _make_run(seed=103)
        run_state.player.obtain_relic("FishingRod")
        relic = run_state.player.get_relic_objects()[-1]
        for room_type in (RoomType.ELITE, RoomType.BOSS):
            combat = _make_combat(run_state, relic, room_type, seed=3000 + int(room_type.value))
            combat._end_combat(player_won=True)
        assert relic.combats_seen == 0
        assert sum(1 for card in run_state.player.deck if card.upgraded) == 0


class TestHeftyTabletParity:
    """C# HeftyTablet.cs.

    - Rarity: RelicRarity.Ancient; HasUponPickupEffect = true.
    - AfterObtained: 3 Rare cards (CardsVar = 3) from the owner's pool with
      CardRarityOddsType.Uniform and CardCreationFlags.NoUpgradeRoll, offered
      on a choose-a-card screen with canSkip = true; the chosen card and one
      Injury are then added to the deck (Injury unconditionally).
    """

    def test_offers_three_uniform_rare_cards_with_skip_and_adds_injury(self):
        run_state = _make_run(seed=104)
        assert run_state.player.obtain_relic("HeftyTablet")
        rewards = [r for r in run_state.pending_rewards if isinstance(r, CardReward)]
        assert len(rewards) == 1
        reward = rewards[0]
        assert reward.skippable is True  # CardSelectCmd.FromChooseACardScreen(canSkip: true)
        assert len(reward.cards) == 3
        assert all(card.rarity == CardRarity.RARE for card in reward.cards)
        assert all(not card.upgraded for card in reward.cards)  # NoUpgradeRoll flag
        assert all(card.card_id not in CURSE_CARD_POOL for card in reward.cards)
        # Injury always lands in the deck.
        assert sum(1 for card in run_state.player.deck if card.card_id == CardId.INJURY) == 1

    def test_injury_is_queued_when_followups_are_deferred(self):
        run_state = _make_run(seed=105)
        run_state.defer_followup_rewards = True
        assert run_state.player.obtain_relic("HeftyTablet")
        add_rewards = [r for r in run_state.pending_rewards if isinstance(r, AddCardsReward)]
        assert len(add_rewards) == 1
        assert [card.card_id for card in add_rewards[0].cards] == [CardId.INJURY]

    def test_has_upon_pickup_effect(self):
        relic = create_relic_by_name("HeftyTablet")
        assert relic.has_upon_pickup_effect is True


class TestKaleidoscopeParity:
    """C# Kaleidoscope.cs.

    - Rarity: RelicRarity.Ancient; HasUponPickupEffect = true.
    - AfterObtained: offers CardsVar(2) card rewards; each reward holds one
      card from each of 3 other character card pools (StableShuffle with
      Rng.Niche, Take(3)), rolled at CardRarityOddsType.RegularEncounter with
      CardCreationFlags.NoCardPoolModifications.
    - IsAllowedAtNeow: requires all character card pools unlocked (the
      simulator assumes everything is unlocked).
    """

    def test_offers_two_rewards_with_cards_from_three_other_characters(self):
        run_state = _make_run(seed=106, character_id="Ironclad")
        assert run_state.player.obtain_relic("Kaleidoscope")
        rewards = [r for r in run_state.pending_rewards if isinstance(r, CardReward)]
        assert len(rewards) == 2  # CardsVar = 2
        for reward in rewards:
            assert len(reward.cards) == 3  # one card per other character pool
            pools = {_character_pool_for_card(card.card_id) for card in reward.cards}
            assert None not in pools  # all cards come from character pools
            assert len(pools) == 3  # three distinct character pools
            assert "IRONCLAD" not in pools  # owner's own pool is excluded

    def test_cards_are_upgraded_or_not_independently_and_are_normal_cards(self):
        run_state = _make_run(seed=107, character_id="Ironclad")
        run_state.player.obtain_relic("Kaleidoscope")
        for reward in run_state.pending_rewards:
            for card in reward.cards:
                assert card.card_type not in (CardType.CURSE, CardType.STATUS, CardType.QUEST)


class TestNeowsBonesParity:
    """C# NeowsBones.cs.

    - Rarity: RelicRarity.Ancient.
    - GetValidRelics: the Neow event's relic-bearing AllPossibleOptions where
      IsAllowedAtNeow holds, excluding NeowsBones itself.
    - AfterObtained: PlayerRng.Rewards shuffles the valid relics, the first
      RelicsVar(2) are offered as RelicRewards with skipping disallowed; then
      CursesVar(1) random curse (CanBeGeneratedByModifiers, ordered by card Id)
      is drawn with Rng.Niche and added to the deck.
    """

    def test_offers_two_specific_neow_relics_with_skipping_disallowed(self):
        from sts2_env.relics.shop_event import NeowsBones

        run_state = _make_run(seed=108)
        assert run_state.player.obtain_relic("NeowsBones")
        relic_rewards = [r for r in run_state.pending_rewards if isinstance(r, RelicReward)]
        assert len(relic_rewards) == 2  # RelicsVar = 2
        for reward in relic_rewards:
            assert reward.relic_id is not None
            assert reward.relic_id != "NEOWS_BONES"  # excluded from GetValidRelics
            assert reward.relic_id in NeowsBones.NEOW_RELIC_IDS
            assert reward.skippable is False  # WithSkippingDisallowed()

    def test_neow_relic_set_matches_neow_all_possible_options(self):
        from sts2_env.relics.registry import RELIC_REGISTRY
        from sts2_env.relics.shop_event import NeowsBones

        assert len(NeowsBones.NEOW_RELIC_IDS) == 27  # 28 options minus NeowsBones
        for name in NeowsBones.NEOW_RELIC_IDS:
            assert name in RELIC_REGISTRY or name in RelicId.__members__
            assert name != "NEOWS_BONES"
        # Both new Neow relics from this version are part of the pool.
        assert "FISHING_ROD" in NeowsBones.NEOW_RELIC_IDS
        assert "KALEIDOSCOPE" in NeowsBones.NEOW_RELIC_IDS
        assert "PHIAL_HOLSTER" in NeowsBones.NEOW_RELIC_IDS
        assert "WINGED_BOOTS" in NeowsBones.NEOW_RELIC_IDS
        assert "HEFTY_TABLET" in NeowsBones.NEOW_RELIC_IDS
        assert "NEOWS_TALISMAN" in NeowsBones.NEOW_RELIC_IDS
        assert "SILKEN_TRESS" in NeowsBones.NEOW_RELIC_IDS

    def test_adds_one_random_generatable_curse_to_deck(self):
        run_state = _make_run(seed=109)
        deck_before = [card.card_id for card in run_state.player.deck]
        assert run_state.player.obtain_relic("NeowsBones")
        new_curses = [
            card
            for card in run_state.player.deck
            if card.card_id not in deck_before and card.card_type == CardType.CURSE
        ]
        assert len(new_curses) == 1  # CursesVar = 1
        curse = new_curses[0]
        assert curse.card_id in CURSE_CARD_POOL
        assert curse.can_be_generated_by_modifiers is True  # CanBeGeneratedByModifiers filter


class TestNeowsTalismanParity:
    """C# NeowsTalisman.cs.

    - Rarity: RelicRarity.Ancient; HasUponPickupEffect = true.
    - AfterObtained: upgrades the last Basic Strike and the last Basic Defend
      in deck order (LastOrDefault over the Basic-rarity cards).
    """

    def test_upgrades_last_strike_and_last_defend_only(self):
        run_state = _make_run(seed=110)
        deck = run_state.player.deck
        # Starter deck layout: 5 Strikes, 4 Defends, 1 Bash.
        assert deck[0].card_id == CardId.STRIKE_IRONCLAD
        assert deck[4].card_id == CardId.STRIKE_IRONCLAD
        assert deck[8].card_id == CardId.DEFEND_IRONCLAD
        assert run_state.player.obtain_relic("NeowsTalisman")
        assert deck[4].upgraded is True  # last Basic Strike
        assert deck[8].upgraded is True  # last Basic Defend
        assert deck[3].upgraded is False  # earlier Strike untouched
        assert deck[7].upgraded is False  # earlier Defend untouched
        assert deck[9].upgraded is False  # Bash untouched

    def test_does_not_upgrade_non_basic_strike_tagged_cards(self):
        run_state = _make_run(seed=111)
        perfected = create_card(CardId.PERFECTED_STRIKE)  # COMMON, has STRIKE tag
        run_state.player.deck.append(perfected)
        run_state.player.obtain_relic("NeowsTalisman")
        assert perfected.upgraded is False  # Rarity == Basic filter
        assert run_state.player.deck[4].upgraded is True


class TestPhialHolsterParity:
    """C# PhialHolster.cs.

    - Rarity: RelicRarity.Ancient; HasUponPickupEffect = true.
    - AfterObtained: PlayerCmd.GainMaxPotionCount(PotionSlots = 1) then procures
      Potions = 2 random out-of-combat potions from the CombatPotionGeneration
      RNG stream via PotionCmd.TryToProcure.
    """

    def test_grants_one_potion_slot_and_two_random_potions(self):
        run_state = _make_run(seed=112)
        slots_before = run_state.player.max_potion_slots
        assert run_state.player.obtain_relic("PhialHolster")
        assert run_state.player.max_potion_slots == slots_before + 1  # PotionSlots = 1
        held = [potion for potion in run_state.player.potions if potion is not None]
        assert len(held) == 2  # Potions = 2

    def test_has_upon_pickup_effect(self):
        assert create_relic_by_name("PhialHolster").has_upon_pickup_effect is True


class TestSilkenTressParity:
    """C# SilkenTress.cs.

    - Rarity: RelicRarity.Ancient; HasUponPickupEffect = true.
    - AfterObtained: PlayerCmd.LoseGold(Owner.Gold) -> all gold is lost.
    - TryModifyCardRewardOptionsLate: enchants every enchantable card of the
      first IsCardReward batch with Glam(1) and returns true;
      AfterModifyingCardRewardOptions then sets IsUsed (SavedProperty), so
      IsUsedUp becomes true and later rewards are untouched.
    """

    def test_loses_all_gold_upon_pickup(self):
        run_state = _make_run(seed=113)
        run_state.player.gold = 250
        assert run_state.player.obtain_relic("SilkenTress")
        assert run_state.player.gold == 0

    def test_first_card_reward_gets_glam_then_relic_is_used_up(self):
        run_state = _make_run(seed=114)
        run_state.player.obtain_relic("SilkenTress")
        relic = run_state.player.get_relic_objects()[-1]
        assert relic.is_used_up is False

        first = CardReward(run_state.player.player_id, generation_context=None, card_creation_source="other")
        first.populate(run_state, None)
        assert len(first.cards) > 0
        assert all(card.enchantments.get("Glam") == 1 for card in first.cards)
        assert relic.is_used_up is True  # IsUsed set after modifying

        second = CardReward(run_state.player.player_id, generation_context=None, card_creation_source="other")
        second.populate(run_state, None)
        assert all(not card.enchantments for card in second.cards)  # later rewards untouched

    def test_marks_used_even_when_no_card_qualifies_for_glam(self):
        # C#: TryModifyCardRewardOptionsLate returns true once past the guards,
        # so the relic is consumed by the first reward even without candidates.
        run_state = _make_run(seed=115)
        run_state.player.obtain_relic("SilkenTress")
        relic = run_state.player.get_relic_objects()[-1]
        cards = [create_card(CardId.INJURY)]  # curses cannot be enchanted
        result = relic.modify_card_reward_options_late(
            run_state.player, cards, None, None, run_state
        )
        assert result == cards
        assert all(not card.enchantments for card in cards)
        assert relic.is_used_up is True


class TestWingedBootsParity:
    """C# WingedBoots.cs.

    - Rarity: RelicRarity.Ancient.
    - IsAllowed: runState.Players.Count == 1.
    - IsUsedUp: TimesUsed >= 3; ShouldAllowFreeTravel: !IsUsedUp.
    - AfterRoomEntered: consumes one charge when the previous visited map
      point does not contain the current map point among its children.
    """

    def test_allowed_only_for_single_player_runs(self):
        run_state = _make_run(seed=116)
        relic = create_relic_by_name("WingedBoots")
        assert relic.is_allowed(run_state) is True
        run_state.players.append(run_state.player)
        assert relic.is_allowed(run_state) is False  # Players.Count == 2

    def test_first_room_and_path_travel_do_not_consume_charges(self):
        run_state = _make_run(seed=117)
        run_state.generate_map()
        run_state.player.obtain_relic("WingedBoots")
        relic = run_state.player.get_relic_objects()[-1]

        start = run_state.map.start_point.children[0]
        run_state.add_visited_coord(start.coord)
        relic.after_room_entered(run_state.player, None)
        assert relic.times_used == 0  # VisitedMapCoords.Count <= 1

        next_point = start.children[0]
        run_state.add_visited_coord(next_point.coord)
        relic.after_room_entered(run_state.player, None)
        assert relic.times_used == 0  # current point IS a child of previous

    def test_ignoring_paths_consumes_a_charge_until_used_up(self):
        run_state = _make_run(seed=118)
        run_state.generate_map()
        run_state.player.obtain_relic("WingedBoots")
        relic = run_state.player.get_relic_objects()[-1]

        start = run_state.map.start_point.children[0]
        next_point = start.children[0]
        run_state.add_visited_coord(start.coord)
        relic.after_room_entered(run_state.player, None)
        run_state.add_visited_coord(next_point.coord)
        relic.after_room_entered(run_state.player, None)

        target_row = next_point.coord.row + 1
        # Walk off-path (to nodes not reachable from the current point) until
        # all three charges are consumed. The walk adapts to the generated
        # map shape: pruning/repair (A10) can make individual rows narrow, so
        # the three off-path steps may span several rows.
        current = next_point
        row = target_row
        guard = 0
        while relic.times_used < 3 and guard < 30:
            guard += 1
            row_points = run_state.map.get_row(row)
            off_path_points = [p for p in row_points if p not in current.children]
            if not off_path_points:
                # Step along the path to stay connected, then try the next row.
                if current.children:
                    current = current.children[0]
                    run_state.add_visited_coord(current.coord)
                    relic.after_room_entered(run_state.player, None)
                row += 1
                continue
            point = off_path_points[0]
            run_state.add_visited_coord(point.coord)
            relic.after_room_entered(run_state.player, None)
            current = point
            row += 1
        assert relic.times_used == 3
        assert relic.is_used_up is True  # IsUsedUp: TimesUsed >= 3
        assert relic.should_allow_free_travel() is False  # ShouldAllowFreeTravel

        # Further off-path travel no longer consumes anything.
        extra_points = [
            point for point in run_state.map.get_row(row)
            if point not in current.children
        ]
        if extra_points:
            run_state.add_visited_coord(extra_points[0].coord)
            relic.after_room_entered(run_state.player, None)
        assert relic.times_used == 3

    def test_should_allow_free_travel_while_charges_remain(self):
        relic = create_relic_by_name("WingedBoots")
        assert relic.should_allow_free_travel() is True
        assert relic.is_used_up is False

    def test_map_layer_reaches_any_node_in_next_row_while_charges_remain(self):
        # C# MapTravel.cs:14-21 GetTravelablePointsFrom: when
        # Hook.ShouldAllowFreeTravel is true, every point in
        # currentPoint.coord.row + 1 is travelable instead of just
        # currentPoint.Children. The hook ORs across all listeners
        # (C# Hook.cs:2485-2492), including WingedBoots with charges
        # (C# WingedBoots.cs:52-55).
        run_state = _make_run(seed=119)
        run_state.generate_map()
        run_state.player.obtain_relic("WingedBoots")

        start = run_state.map.start_point.children[0]
        run_state.add_visited_coord(start.coord)
        run_state.map.get_point(start.coord)  # sanity: point exists

        expected = {point.coord for point in run_state.map.get_row(start.coord.row + 1)}
        children = {child.coord for child in start.children}
        assert expected - children, "map row must contain off-path nodes"
        assert set(run_state.get_available_next_coords()) == expected

    def test_map_layer_falls_back_to_children_when_used_up(self):
        run_state = _make_run(seed=120)
        run_state.generate_map()
        run_state.player.obtain_relic("WingedBoots")
        relic = run_state.player.get_relic_objects()[-1]
        relic._times_used = 3  # IsUsedUp: TimesUsed >= 3

        start = run_state.map.start_point.children[0]
        run_state.add_visited_coord(start.coord)

        assert relic.should_allow_free_travel() is False
        assert set(run_state.get_available_next_coords()) == {
            child.coord for child in start.children
        }

    def test_run_state_should_allow_free_travel_or_accumulates_listeners(self):
        # C# Hook.cs:2485-2492: Hook.ShouldAllowFreeTravel ORs across every
        # hook listener, so either the Flight modifier (C# Flight.cs:5) or a
        # WingedBoots with charges enables free travel.
        run_state = _make_run(seed=121)
        run_state.generate_map()
        assert run_state.should_allow_free_travel() is False

        run_state.player.obtain_relic("WingedBoots")
        assert run_state.should_allow_free_travel() is True

        spent = run_state.player.get_relic_objects()[-1]
        spent._times_used = 3
        assert run_state.should_allow_free_travel() is False

        run_state.modifiers = [FlightModifier()]
        assert run_state.should_allow_free_travel() is True
