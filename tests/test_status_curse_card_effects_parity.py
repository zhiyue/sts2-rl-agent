"""Focused parity tests for status/curse cards backed by decompiled models."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.status import (
    make_bad_luck,
    make_beckon,
    make_brightest_flame,
    make_burn,
    make_caltrops,
    make_debris,
    make_debt,
    make_deprecated_card,
    make_decay,
    make_doubt,
    make_entrench,
    make_enthralled,
    make_exterminate,
    make_feeding_frenzy,
    make_fuel,
    make_frantic_escape,
    make_infection,
    make_mad_science,
    make_maul,
    make_neows_fury,
    make_normality,
    make_outmaneuver,
    make_peck,
    make_relax,
    make_regret,
    make_rip_and_tear,
    make_shame,
    make_shiv,
    make_sovereign_blade,
    make_spore_mind,
    make_sweeping_gaze,
    make_toxic,
    make_toric_toughness,
    make_void,
    make_whistle,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.constants import MAX_HAND_SIZE
from sts2_env.core.enums import CardId, CardType, CombatSide, PowerId, TargetType
from sts2_env.core.hooks import fire_after_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.powers.base import PowerInstance
from sts2_env.powers.remaining_c import SandpitPower
from sts2_env.run.run_state import PlayerState


def _make_combat(gold: int = 0, *, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        character_id="Ironclad",
        gold=gold,
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(100 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class _CannotHitPower(PowerInstance):
    def __init__(self):
        super().__init__(PowerId.COVERED, 1)

    def should_allow_hitting(self, owner, combat):
        return False


class _FirstRng:
    def sample(self, lst, k):
        return list(lst)[:k]

    def shuffle(self, seq) -> None:
        return None

    def choice(self, lst):
        return list(lst)[0]


class TestStatusCurseCardEffectsParity:
    def test_caltrops_applies_thorns_power(self):
        combat = _make_combat()
        combat.hand = [make_caltrops(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.THORNS) == 5

    def test_entrench_doubles_current_block_without_block_modifiers(self):
        combat = _make_combat()
        combat.player.block = 6
        combat.player.apply_power(PowerId.NO_BLOCK, 1)
        combat.hand = [make_entrench(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 12

    def test_outmaneuver_applies_energy_next_turn(self):
        combat = _make_combat()
        combat.hand = [make_outmaneuver(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 3

    def test_relax_gains_block_and_next_turn_draw_and_energy(self):
        combat = _make_combat()
        combat.hand = [make_relax(upgraded=True)]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.player.block == 17
        assert combat.player.get_power_amount(PowerId.DRAW_CARDS_NEXT_TURN) == 3
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 3

    def test_whistle_deals_damage_stuns_target_and_exhausts(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        card = make_whistle(upgraded=True)
        combat.hand = [card]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 56
        assert combat.enemy_ais[enemy.combat_id].current_move.state_id == "STUNNED"
        assert card in combat.exhaust_pile

    def test_maul_play_grows_all_owner_maul_copies_and_upgrade_preserves_growth(self):
        combat = _make_combat()
        played = make_maul()
        in_draw = make_maul()
        in_discard = make_maul()
        combat.hand = [played]
        combat.draw_pile = [in_draw]
        combat.discard_pile = [in_discard]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert played.base_damage == 6
        assert in_draw.base_damage == 6
        assert in_discard.base_damage == 6

        combat.upgrade_card(in_draw)

        assert in_draw.upgraded is True
        assert in_draw.base_damage == 7
        assert in_draw.effect_vars["increase"] == 2

    def test_neows_fury_lets_player_select_discard_cards_up_to_cards_var(self):
        # NeowsFury.cs OnPlay (lines 36-40): after the attack the player selects
        # 0..num cards from the discard pile via CardSelectCmd.FromCombatPile
        # with CardSelectorPrefs(prompt, 0, num) — an explicit selection, not an
        # automatic shuffle-and-take.
        combat = _make_combat()
        first = make_strike_ironclad()
        second = make_strike_ironclad()
        third = make_strike_ironclad()
        fury = make_neows_fury()
        combat.hand = [fury]
        combat.discard_pile = [first, second, third]
        combat.energy = 1

        assert combat.play_card(0, 0)

        choice = combat.pending_choice
        assert choice is not None
        assert choice.min_choices == 0
        assert choice.max_choices == 2  # base CardsVar
        assert [option.card for option in choice.options] == [first, second, third]

        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)

        assert first in combat.hand
        assert second in combat.hand
        assert third in combat.discard_pile
        assert combat.pending_choice is None

    def test_neows_fury_caps_selection_at_free_hand_slots(self):
        # NeowsFury.cs line 36: num = min(CardsVar, MaxCardsInHand - hand size),
        # so with 9 cards already in hand (fury left the hand when played) only
        # 1 of the 2 discard cards may be taken.
        combat = _make_combat()
        discard_cards = [make_strike_ironclad() for _ in range(3)]
        fury = make_neows_fury()
        filler = [make_strike_ironclad() for _ in range(MAX_HAND_SIZE - 1)]
        combat.hand = [fury, *filler]
        combat.discard_pile = list(discard_cards)
        combat.energy = 1

        assert combat.play_card(0, 0)

        choice = combat.pending_choice
        assert choice is not None
        assert choice.max_choices == 1

        assert combat.resolve_pending_choice(0)
        assert not combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)

        assert sum(1 for c in combat.hand if c in discard_cards) == 1
        assert len(combat.hand) == MAX_HAND_SIZE

    def test_neows_fury_allows_selecting_no_cards(self):
        # NeowsFury.cs CardSelectorPrefs(prompt, 0, num): selecting zero cards
        # is a legal resolution.
        combat = _make_combat()
        fury = make_neows_fury()
        combat.hand = [fury]
        combat.discard_pile = [make_strike_ironclad() for _ in range(3)]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(None)
        assert combat.pending_choice is None
        assert len(combat.discard_pile) == 3
        assert combat.hand == []

    def test_deprecated_card_draws_one_and_removes_deck_version(self):
        # DeprecatedCard.cs OnPlay (lines 26-34): CardPileCmd.Draw(1, owner)
        # then CardPileCmd.RemoveFromDeck(DeckVersion) — playing the save-file
        # placeholder draws a card and removes the deck counterpart from the
        # run deck. The sim has no per-instance DeckVersion tracking, so the
        # deck copy sharing the card id is removed; the card is unreachable in
        # normal play (excluded from generation pools).
        combat = _make_combat()
        placeholder = make_deprecated_card()
        deck_copy = make_deprecated_card()
        state = combat.combat_player_state_for(combat.player)
        assert state is not None
        state.player_state.deck.append(deck_copy)
        combat.hand = [placeholder]
        combat.draw_pile = [make_strike_ironclad()]
        combat.energy = 1

        assert combat.play_card(0)

        drawn = next(hand_card for hand_card in combat.hand if hand_card is not placeholder)
        assert drawn.card_id is CardId.STRIKE_IRONCLAD
        assert deck_copy not in state.player_state.deck
        assert placeholder in combat.exhaust_pile

    def test_feeding_frenzy_applies_temporary_strength_then_restores(self):
        combat = _make_combat()
        combat.hand = [make_feeding_frenzy(upgraded=True)]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.FEEDING_FRENZY) == 7
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 7

        fire_after_turn_end(CombatSide.PLAYER, combat)

        assert combat.player.get_power_amount(PowerId.FEEDING_FRENZY) == 0
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 0

    def test_toric_toughness_stores_actual_block_gained(self):
        combat = _make_combat()
        combat.player.apply_power(PowerId.DEXTERITY, 2)
        combat.hand = [make_toric_toughness()]
        combat.energy = 2

        assert combat.play_card(0)
        power = combat.player.powers[PowerId.TORIC_TOUGHNESS]
        assert power._block_value == 7  # noqa: SLF001

    def test_burn_deals_turn_end_in_hand_damage_then_discards(self):
        """Matches Burn.cs: in-hand turn-end burn deals 2 and follows turn cleanup."""
        combat = _make_combat()
        burn = make_burn()
        combat.hand = [burn]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 2
        assert burn in combat.discard_pile

    def test_void_loses_energy_on_draw(self):
        """Matches Void.cs: drawing Void immediately loses configured energy."""
        combat = _make_combat()
        combat.hand.clear()
        void = make_void()
        combat.draw_pile = [void]
        combat.energy = 3

        combat.draw_cards(combat.player, 1)

        assert combat.energy == 2
        assert void in combat.hand

    def test_void_does_not_lose_energy_after_combat_ending(self):
        combat = _make_combat()
        combat.hand.clear()
        void = make_void()
        combat.draw_pile = [void]
        combat.energy = 3
        combat.is_over = True

        combat.draw_cards(combat.player, 1)

        assert combat.energy == 3

    def test_fuel_gains_energy_before_drawing_void(self):
        """Matches Fuel.cs: gain energy before Draw, so drawn Void removes one."""
        combat = _make_combat()
        combat.hand = [make_fuel()]
        combat.draw_pile = [make_void()]
        combat.energy = 0

        assert combat.play_card(0)

        assert combat.energy == 0

    def test_brightest_flame_gains_energy_before_drawing_void(self):
        """Matches BrightestFlame.cs: gain energy before Draw, so drawn Void removes one."""
        combat = _make_combat()
        max_hp_before = combat.player.max_hp
        combat.hand = [make_brightest_flame()]
        combat.draw_pile = [make_void()]
        combat.energy = 0

        assert combat.play_card(0)

        assert combat.energy == 1
        assert combat.player.max_hp == max_hp_before - 1

    def test_infection_deals_turn_end_in_hand_damage_then_discards(self):
        combat = _make_combat()
        infection = make_infection()
        combat.hand = [infection]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 3
        assert infection in combat.discard_pile

    def test_mad_science_sapping_vulnerable_uses_owner_applier(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        card = make_mad_science()
        card.effect_vars["rider"] = 1
        drawn = make_strike_ironclad()
        combat.hand = [card]
        combat.draw_pile = [drawn]
        combat.energy = 1
        combat.apply_power_to(combat.player, PowerId.VICIOUS, 1)

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.WEAK) == 2
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2
        assert drawn in combat.hand

    def test_mad_science_chaos_uses_combat_generation_pool(self):
        combat = _make_combat()
        card = make_mad_science()
        card.card_type = CardType.SKILL
        card.base_damage = None
        card.base_block = 8
        card.effect_vars["rider"] = 6
        combat.hand = [card]
        combat.energy = 1
        combat.rng = _FirstRng()

        assert combat.play_card(0)

        generated = next(hand_card for hand_card in combat.hand if hand_card is not card)
        assert generated.card_id is not CardId.STRIKE_IRONCLAD
        # MadScience.cs ExecuteRider Chaos (lines 466-471) uses
        # SetToFreeThisTurn() (CardModel.cs lines 1266-1270), which is
        # SetThisTurnOrUntilPlayed(0) — the generated card is free now but its
        # cost returns at end of turn (or when played).
        assert generated.cost == 0
        generated.end_of_turn_cleanup()
        assert generated.cost == generated.original_cost

    def test_mad_science_power_type_does_not_require_target_selection(self):
        combat = _make_combat()
        card = make_mad_science()
        card.card_type = CardType.POWER
        card.base_damage = None
        card.effect_vars["rider"] = 7
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 2
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 2

    def test_distraction_uses_combat_generation_pool(self):
        combat = _make_combat()
        card = create_card(CardId.DISTRACTION)
        combat.hand = [card]
        combat.energy = 1
        combat.rng = _FirstRng()

        assert combat.play_card(0)

        generated = combat.hand[0]
        assert generated.card_id is not CardId.DEFEND_IRONCLAD
        assert generated.card_type is CardType.SKILL
        assert generated.cost == 0

    def test_toxic_deals_turn_end_in_hand_damage_but_playing_it_exhausts_safely(self):
        combat = _make_combat()
        toxic = make_toxic()
        combat.hand = [toxic]
        combat.energy = 1
        starting_hp = combat.player.current_hp

        assert combat.play_card(0)
        assert combat.player.current_hp == starting_hp
        assert toxic in combat.exhaust_pile

        toxic = make_toxic()
        combat.hand = [toxic]
        combat.energy = 0
        starting_hp = combat.player.current_hp
        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 5
        assert toxic in combat.discard_pile

    def test_regret_uses_pre_flush_hand_size_for_hp_loss(self):
        """Matches Regret.cs: hp loss equals hand size at turn end before flush."""
        combat = _make_combat()
        regret = make_regret()
        filler_a = make_strike_ironclad()
        filler_b = make_strike_ironclad()
        combat.hand = [regret, filler_a, filler_b]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 3

    def test_debt_loses_up_to_ten_gold_without_going_negative(self):
        """Matches Debt.cs: turn-end gold loss is clamped by current gold."""
        combat = _make_combat(gold=7)
        combat.hand = [make_debt()]

        combat.end_player_turn()

        assert combat.gold == 0

    def test_beckon_and_bad_luck_use_unblocked_turn_end_hp_loss(self):
        combat = _make_combat()
        combat.player.block = 99
        combat.hand = [make_beckon(), make_bad_luck()]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 19

    def test_decay_doubt_and_shame_turn_end_effects_are_handled_by_combat_hooks(self):
        combat = _make_combat()
        combat.hand = [make_decay(), make_doubt(), make_shame()]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 2
        assert combat.player.get_power_amount(PowerId.WEAK) == 1
        assert combat.player.get_power_amount(PowerId.FRAIL) == 1

    def test_playable_noop_status_and_curses_only_clear_from_hand(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_player_hp = combat.player.current_hp
        starting_enemy_hp = enemy.current_hp

        debris = make_debris()
        spore_mind = make_spore_mind()
        combat.hand = [debris, spore_mind]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.play_card(0)
        assert debris in combat.exhaust_pile
        assert spore_mind in combat.exhaust_pile
        assert combat.energy == 0
        assert combat.player.current_hp == starting_player_hp
        assert enemy.current_hp == starting_enemy_hp

    def test_enthralled_blocks_other_manual_cards_until_played(self):
        combat = _make_combat()
        enthralled = make_enthralled()
        strike = make_strike_ironclad()
        combat.hand = [enthralled, strike]
        combat.energy = 3

        assert combat.can_play_card(strike) is False
        assert combat.play_card(1, 0) is False
        assert combat.play_card(0) is True
        assert enthralled in combat.discard_pile
        assert combat.can_play_card(strike) is True

    def test_frantic_escape_increases_matching_sandpit_and_its_own_cost_per_play(self):
        """Matches FranticEscape.cs: increment matching Sandpit then increase own cost."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        sandpit = SandpitPower(4)
        sandpit.set_target(combat.player)
        enemy.powers[PowerId.SANDPIT] = sandpit

        frantic = make_frantic_escape()
        combat.hand = [frantic]
        combat.energy = 10

        assert combat.play_card(0)
        assert sandpit.amount == 5
        assert frantic.cost == 2

        # Replay the same instance to validate accumulating combat-time cost increase.
        combat.hand = [frantic]
        assert combat.play_card(0)
        assert sandpit.amount == 6
        assert frantic.cost == 3

    def test_frantic_escape_increases_only_matching_sandpit_instance(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        ally = combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        sandpit = SandpitPower(4)
        sandpit.set_target(combat.player)
        sandpit.add_instance(4, ally)
        enemy.powers[PowerId.SANDPIT] = sandpit
        ally_card = make_frantic_escape()
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None
        ally_state.hand = [ally_card]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1

        assert combat.play_card_from_creature(ally, 0)

        assert sandpit._instances == [(4, combat.player), (5, ally)]  # noqa: SLF001

    def test_normality_blocks_playing_a_fourth_card_while_it_remains_in_hand(self):
        """Matches Normality.cs: while in hand, the player cannot play more than 3 cards per turn."""
        combat = _make_combat()
        shiv_a = make_shiv()
        shiv_b = make_shiv()
        shiv_c = make_shiv()
        shiv_d = make_shiv()
        combat.hand = [make_normality(), shiv_a, shiv_b, shiv_c, shiv_d]
        combat.energy = 10

        assert combat.play_card(1, 0)
        assert combat.play_card(1, 0)
        assert combat.play_card(1, 0)
        assert combat.count_cards_played_this_turn(combat.player) == 3
        assert combat.can_play_card(shiv_d) is False
        assert combat.play_card(1, 0) is False

    def test_normality_uses_card_dynamic_limit_for_should_play(self):
        combat = _make_combat()
        normality = make_normality()
        normality.effect_vars["calc_base"] = 1
        first = make_shiv()
        second = make_shiv()
        combat.hand = [normality, first, second]
        combat.energy = 10

        assert combat.play_card(1, 0)
        assert combat.can_play_card(second) is False

    def test_upgraded_peck_hits_four_times(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_peck(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 8

    def test_sweeping_gaze_uses_osty_damage_and_dealer(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        starting_hp = enemy.current_hp
        combat.hand = [make_sweeping_gaze()]

        assert combat.play_card(0)
        assert enemy.current_hp == starting_hp - 10
        assert combat.count_powered_hits_by_dealer_this_turn(osty) == 1

    def test_exterminate_hits_only_hittable_enemies_four_times(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_exterminate()]
        combat.energy = 1

        assert combat.play_card(0)
        assert blocked.current_hp == 100
        assert hittable.current_hp == 88

    def test_rip_and_tear_rerolls_only_hittable_enemies_after_kill(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 7
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_rip_and_tear()]
        combat.energy = 1

        assert combat.play_card(0)
        assert blocked.current_hp == 100
        assert hittable.is_dead

    def test_shiv_fan_of_knives_hits_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.apply_power_to(combat.player, PowerId.FAN_OF_KNIVES, 1)
        combat.hand = [make_shiv()]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert blocked.current_hp == 100
        assert hittable.current_hp == 96

    def test_shiv_fan_of_knives_does_not_require_target_selection(self):
        combat = _make_combat(extra_enemies=1)
        for enemy in combat.enemies:
            enemy.current_hp = enemy.max_hp = 100
        combat.apply_power_to(combat.player, PowerId.FAN_OF_KNIVES, 1)
        combat.hand = [make_shiv()]
        combat.energy = 0

        assert combat.play_card(0)
        assert [enemy.current_hp for enemy in combat.enemies] == [96, 96]

    def test_sovereign_blade_seeking_edge_hits_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.apply_power_to(combat.player, PowerId.SEEKING_EDGE, 1)
        blade = make_sovereign_blade()
        combat.hand = [blade]
        combat.energy = blade.cost

        assert combat.play_card(0, 0)
        assert blocked.current_hp == 100
        assert hittable.current_hp == 90

    def test_sovereign_blade_seeking_edge_does_not_require_target_selection(self):
        combat = _make_combat(extra_enemies=1)
        for enemy in combat.enemies:
            enemy.current_hp = enemy.max_hp = 100
        combat.apply_power_to(combat.player, PowerId.SEEKING_EDGE, 1)
        blade = make_sovereign_blade()
        combat.hand = [blade]
        combat.energy = blade.cost

        assert combat.play_card(0)
        assert [enemy.current_hp for enemy in combat.enemies] == [90, 90]
