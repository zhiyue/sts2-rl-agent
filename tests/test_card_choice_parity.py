"""Parity tests for card-selection flows backed by decompiled card models."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import make_secret_technique, make_secret_weapon, make_thinking_ahead
from sts2_env.cards.factory import create_card
from sts2_env.cards.defect import create_defect_starter_deck, make_hologram
from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck, make_bash
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.necrobinder import create_necrobinder_starter_deck, make_cleanse, make_dredge
from sts2_env.cards.silent import (
    create_silent_starter_deck,
    make_grand_finale,
    make_defend_silent,
    make_hand_trick,
    make_nightmare,
    make_strike_silent,
    make_survivor,
)
from sts2_env.cards.status import make_clash, make_dual_wield, make_enthralled, make_regret, make_wish
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CombatSide, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat(deck, character_id: str) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=deck,
        rng_seed=42,
        character_id=character_id,
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    return combat


class TestSingleChoiceParity:
    def test_survivor_discards_the_selected_hand_card(self):
        """Matches Survivor.cs: gain block, then discard exactly one chosen hand card."""
        combat = _make_combat(create_silent_starter_deck(), "Silent")
        strike = make_strike_silent()
        defend = make_defend_silent()
        combat.hand = [make_survivor(), strike, defend]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 8
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [strike, defend]

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert strike in combat.hand
        assert defend not in combat.hand
        assert defend in combat.discard_pile

    def test_hologram_returns_the_selected_discard_card_to_hand(self):
        """Matches Hologram.cs: gain block, then move one chosen discard card to hand."""
        combat = _make_combat(create_defect_starter_deck(), "Defect")
        strike = make_strike_ironclad()
        bash = make_bash()
        combat.hand = [make_hologram()]
        combat.discard_pile = [strike, bash]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 3
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [strike, bash]

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert bash in combat.hand
        assert bash not in combat.discard_pile
        assert strike in combat.discard_pile

    def test_wish_orders_draw_pile_by_rarity_then_id_before_selection(self):
        """Matches Wish.cs simple-grid ordering over the draw pile."""
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        draw_cards = [
            make_regret(),
            make_dual_wield(),
            make_strike_ironclad(),
            make_wish(),
        ]
        combat.hand = [make_wish()]
        combat.draw_pile = list(draw_cards)
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None

        expected = sorted(draw_cards, key=lambda current: (current.rarity.value, current.card_id.value))
        assert [option.card for option in combat.pending_choice.options] == expected

    def test_grand_finale_cannot_be_played_while_draw_pile_is_non_empty(self):
        """Matches GrandFinale.cs: legality depends on draw pile being empty."""
        combat = _make_combat(create_silent_starter_deck(), "Silent")
        combat.hand = [make_grand_finale()]
        combat.draw_pile = [make_strike_silent()]

        assert combat.can_play_card(combat.hand[0]) is False
        assert combat.play_card(0) is False

    def test_clash_cannot_be_played_if_non_attack_is_in_hand(self):
        """Matches Clash.cs: every card in hand must be an Attack."""
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.hand = [make_clash(), make_defend_ironclad()]

        assert combat.can_play_card(combat.hand[0]) is False
        assert combat.play_card(0, 0) is False

    def test_enthralled_blocks_other_manual_plays_but_can_play_itself(self):
        """Matches Enthralled.cs ShouldPlay: only Enthralled itself is playable from hand."""
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enthralled = make_enthralled()
        strike = make_strike_ironclad()
        combat.hand = [enthralled, strike]
        combat.energy = 3

        assert combat.can_play_card(strike) is False
        assert combat.play_card(1, 0) is False
        assert combat.can_play_card(enthralled) is True

    def test_secret_weapon_only_exposes_attacks_from_draw_pile(self):
        """Matches SecretWeapon.cs: filter draw pile by Attack and preserve pile order."""
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        bash = make_bash()
        combat.hand = [make_secret_weapon()]
        combat.draw_pile = [defend, strike, bash]

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [strike, bash]

    def test_secret_technique_only_exposes_skills_from_draw_pile(self):
        """Matches SecretTechnique.cs: filter draw pile by Skill and preserve pile order."""
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        bash = make_bash()
        combat.hand = [make_secret_technique()]
        combat.draw_pile = [strike, defend, bash]

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [defend]

    def test_hand_trick_only_targets_non_sly_skills_in_hand(self):
        """Matches HandTrick.cs filtering for Skill cards that are not already Sly."""
        combat = _make_combat(create_silent_starter_deck(), "Silent")
        defend = make_defend_silent()
        already_sly = make_defend_silent()
        already_sly.combat_vars["sly_this_turn"] = 1
        strike = make_strike_silent()
        combat.hand = [make_hand_trick(), defend, already_sly, strike]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 7
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [defend]

        assert combat.resolve_pending_choice(0)
        assert defend.combat_vars["sly_this_turn"] == 1


class TestGeneratedChoiceParity:
    def test_discovery_generates_three_distinct_options_and_selected_card_costs_zero(self):
        """Matches Discovery.cs distinct-card choice plus temporary zero cost on selection."""
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        discovery = create_card(CardId.DISCOVERY)
        combat.hand = [discovery]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.allow_skip is True
        assert len(combat.pending_choice.options) == 3

        generated = [option.card for option in combat.pending_choice.options]
        assert len({card.card_id for card in generated}) == 3

        selected = generated[0]
        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None
        assert selected in combat.hand
        assert selected.cost == 0


class TestMultiChoiceParity:
    def test_purity_allows_multi_select_and_exhausts_only_confirmed_cards(self):
        """Matches Purity.cs: choose zero-to-N hand cards, then exhaust the confirmed set."""
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        bash = make_bash()
        combat.hand = [create_card(CardId.PURITY), strike, defend, bash]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi is True
        assert combat.pending_choice.min_choices == 0
        assert combat.pending_choice.max_choices == 3
        assert combat.pending_choice.can_confirm() is True

        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(2)
        assert combat.resolve_pending_choice(None)

        assert combat.pending_choice is None
        assert strike in combat.exhaust_pile
        assert bash in combat.exhaust_pile
        assert defend in combat.hand

    def test_dredge_requires_exact_number_of_discard_cards_before_confirm(self):
        """Matches Dredge.cs: select exactly N discard cards, bounded by current hand space."""
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        bash = make_bash()
        combat.hand = [make_dredge()]
        combat.discard_pile = [strike, defend, bash]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi is True
        assert combat.pending_choice.min_choices == 3
        assert combat.pending_choice.max_choices == 3
        assert combat.pending_choice.can_confirm() is False

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice.can_confirm() is False
        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice.can_confirm() is False
        assert combat.resolve_pending_choice(2)
        assert combat.pending_choice.can_confirm() is True

        assert combat.resolve_pending_choice(None)
        assert combat.pending_choice is None
        assert combat.hand == [strike, defend, bash]
        assert not combat.discard_pile

    def test_cleanse_summons_osty_and_exhausts_the_selected_sorted_draw_card(self):
        """Matches Cleanse.cs: summon Osty first, then exhaust one sorted draw-pile card."""
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        draw_cards = [
            make_regret(),
            make_dual_wield(),
            make_strike_ironclad(),
        ]
        combat.hand = [make_cleanse()]
        combat.draw_pile = list(draw_cards)
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.current_hp == 3
        assert combat.pending_choice is not None

        expected = sorted(draw_cards, key=lambda current: (current.rarity.value, current.card_id.value))
        assert [option.card for option in combat.pending_choice.options] == expected

        chosen = expected[1]
        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert chosen in combat.exhaust_pile
        assert chosen not in combat.draw_pile


class TestDeferredChoiceParity:
    def test_thinking_ahead_draws_before_choice_and_puts_selected_card_on_top(self):
        """Matches ThinkingAhead.cs: draw first, then choose a hand card to put on top."""
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        retained = make_bash()
        first_draw = make_strike_ironclad()
        second_draw = make_defend_ironclad()
        combat.hand = [make_thinking_ahead(), retained]
        combat.draw_pile = [first_draw, second_draw]

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [retained, first_draw, second_draw]

        assert combat.resolve_pending_choice(2)
        assert combat.pending_choice is None
        assert combat.draw_pile[0] is second_draw
        assert second_draw not in combat.hand
        assert retained in combat.hand
        assert first_draw in combat.hand

    def test_nightmare_snapshots_selected_card_at_choice_time(self):
        """Matches NightmarePower.cs SetSelectedCard: store a clone, not a live reference."""
        combat = _make_combat([], "Silent")
        combat.start_combat()

        selected = make_strike_silent()
        combat.hand = [make_nightmare(), selected]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [selected]

        assert combat.resolve_pending_choice(0)
        power = combat.player.powers[PowerId.NIGHTMARE]
        assert getattr(power, "selected_card", None) is not None
        assert power.selected_card is not selected
        assert power.selected_card.base_damage == 6

        selected.base_damage = 99
        power.before_side_turn_start(combat.player, CombatSide.PLAYER, combat)

        generated = [card for card in combat.hand if card.card_id == CardId.STRIKE_SILENT]
        assert len(generated) == 4
        assert sum(1 for card in generated if card.base_damage == 6) == 3
        assert sum(1 for card in generated if card.base_damage == 99) == 1
