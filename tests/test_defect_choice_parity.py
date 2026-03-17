"""Defect parity tests for decompiled selection and exhaust flows."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import (
    make_compact,
    create_defect_starter_deck,
    make_defend_defect,
    make_flak_cannon,
    make_scavenge,
    make_strike_defect,
    make_white_noise,
)
from sts2_env.cards.status import make_burn, make_dazed, make_void, make_wound
from sts2_env.core.enums import CardId, CardType, PowerId
from sts2_env.core.combat import CombatState
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=42,
        character_id="Defect",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestDefectChoiceParity:
    def test_scavenge_exhausts_the_selected_hand_card_before_granting_energy_next_turn(self):
        """Matches Scavenge.cs: choose one hand card to exhaust, then apply EnergyNextTurn."""
        combat = _make_combat()
        strike = make_strike_defect()
        defend = make_defend_defect()
        combat.hand = [make_scavenge(), strike, defend]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [strike, defend]
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 0

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert defend in combat.exhaust_pile
        assert defend not in combat.hand
        assert strike in combat.hand
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 2

    def test_flak_cannon_exhausts_all_non_exhausted_status_cards_and_hits_per_status(self):
        """Matches FlakCannon.cs: exhaust every non-exhausted Status, then attack that many times."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        status_hand = make_dazed()
        status_draw = make_burn()
        status_discard = make_wound()
        already_exhausted = make_void()
        strike = make_strike_defect()

        combat.hand = [make_flak_cannon(), status_hand, strike]
        combat.draw_pile = [status_draw]
        combat.discard_pile = [status_discard]
        combat.exhaust_pile = [already_exhausted]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.pending_choice is None
        assert enemy.current_hp == starting_hp - 24

        assert status_hand in combat.exhaust_pile
        assert status_draw in combat.exhaust_pile
        assert status_discard in combat.exhaust_pile
        assert already_exhausted in combat.exhaust_pile

        assert status_draw not in combat.draw_pile
        assert status_discard not in combat.discard_pile
        assert strike in combat.hand

    def test_compact_transforms_only_status_cards_in_hand_into_fuel(self):
        """Matches Compact.cs: gain block, then transform hand Status cards into Fuel."""
        combat = _make_combat()
        strike = make_strike_defect()
        status_a = make_dazed()
        status_b = make_void()
        draw_status = make_burn()
        combat.hand = [make_compact(), strike, status_a, status_b]
        combat.draw_pile = [draw_status]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 6
        assert combat.pending_choice is None

        hand_ids = [card.card_id for card in combat.hand]
        assert hand_ids.count(CardId.FUEL) == 2
        assert strike in combat.hand
        assert draw_status in combat.draw_pile

    def test_white_noise_generates_free_power_card_in_hand(self):
        """Matches WhiteNoise.cs: generate one combat-distinct Power card and set it free this turn."""
        combat = _make_combat()
        combat.hand = [make_white_noise()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is None
        assert len(combat.hand) == 1

        generated = combat.hand[0]
        assert generated.card_id != CardId.WHITE_NOISE
        assert generated.card_type == CardType.POWER
        assert generated.cost == 0
