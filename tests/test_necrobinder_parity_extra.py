"""Additional Necrobinder parity tests backed by decompiled card models."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.necrobinder import (
    create_necrobinder_starter_deck,
    make_drain_power,
    make_end_of_days,
    make_glimpse_beyond,
    make_severance,
    make_soul_storm,
)
from sts2_env.cards.status import make_soul
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=create_necrobinder_starter_deck(),
        rng_seed=42,
        character_id="Necrobinder",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestNecrobinderParityExtra:
    def test_drain_power_attacks_then_upgrades_random_upgradable_discard_cards(self):
        """Matches DrainPower.cs: attack target, then upgrade random upgradable discard cards."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        strike_a = make_strike_ironclad()
        defend = make_defend_ironclad()
        strike_b = make_strike_ironclad()
        combat.discard_pile = [strike_a, defend, strike_b]
        combat.hand = [make_drain_power()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 10
        assert sum(1 for card in combat.discard_pile if card.upgraded) == 2

    def test_end_of_days_applies_doom_to_all_enemies_then_kills_only_doomed_targets(self):
        """Matches EndOfDays.cs: apply Doom to all hittable enemies, then doom-kill."""
        combat = _make_combat()
        low_hp_enemy = combat.enemies[0]
        low_hp_enemy.current_hp = 20
        low_hp_enemy.max_hp = 20
        extra_enemy, extra_ai = create_shrinker_beetle(Rng(99))
        combat.add_enemy(extra_enemy, extra_ai)
        extra_enemy.current_hp = 40
        extra_enemy.max_hp = 40
        combat.hand = [make_end_of_days()]
        combat.energy = 3

        assert combat.play_card(0)
        assert low_hp_enemy.is_dead
        assert extra_enemy.is_alive
        assert extra_enemy.get_power_amount(PowerId.DOOM) == 29

    def test_glimpse_beyond_adds_souls_to_each_alive_teammate_draw_pile(self):
        """Matches GlimpseBeyond.cs: generate Soul cards into every alive teammate draw pile."""
        combat = _make_combat()
        ally_state = PlayerState(player_id=2, character_id="Necrobinder", max_hp=45, current_hp=45)
        ally = combat.add_ally_player(ally_state)
        ally_combat_state = combat.combat_player_state_for(ally)
        assert ally_combat_state is not None

        combat.hand = [make_glimpse_beyond(upgraded=True)]
        combat.energy = 1
        ally_start = len(ally_combat_state.draw)
        owner_start = len(combat.draw_pile)

        assert combat.play_card(0)

        ally_souls = [card for card in ally_combat_state.draw if card.card_id == CardId.SOUL]
        owner_souls = [card for card in combat.draw_pile if card.card_id == CardId.SOUL]
        assert len(ally_combat_state.draw) == ally_start + 4
        assert len(ally_souls) == 4
        assert len(combat.draw_pile) == owner_start
        assert len(owner_souls) == 0

    def test_severance_deals_damage_and_places_souls_in_draw_discard_and_hand(self):
        """Matches Severance.cs: attack, then create one Soul in draw/discard/hand respectively."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.draw_pile.clear()
        combat.discard_pile.clear()
        combat.hand = [make_severance()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 13
        assert sum(1 for card in combat.draw_pile if card.card_id == CardId.SOUL) == 1
        assert sum(1 for card in combat.discard_pile if card.card_id == CardId.SOUL) == 1
        assert sum(1 for card in combat.hand if card.card_id == CardId.SOUL) == 1

    def test_soul_storm_scales_damage_with_exhausted_soul_count(self):
        """Matches SoulStorm.cs: damage uses base + extra * exhausted Soul count."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.exhaust_pile = [make_soul(), make_soul(), make_strike_ironclad()]
        combat.hand = [make_soul_storm(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 15
