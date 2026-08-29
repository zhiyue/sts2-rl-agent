"""Parity tests for starter/common relic rest and upgrade hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import (
    create_ironclad_starter_deck,
    make_defend_ironclad,
    make_strike_ironclad,
)
from sts2_env.cards.necrobinder import create_necrobinder_starter_deck
from sts2_env.cards.regent import create_regent_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CardType, CombatSide, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.relics.common import BookOfFiveRings
from sts2_env.run.rest_site import HealOption
from sts2_env.run.reward_objects import PotionReward
from sts2_env.run.run_state import RunState


class _FailingShuffleRng:
    def shuffle(self, values):
        raise AssertionError("wrong RNG stream")


class _ReverseShuffleRng:
    def shuffle(self, values):
        values.reverse()


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1600,
    player_hp: int = 80,
    player_max_hp: int = 80,
) -> CombatState:
    combat = CombatState(
        player_hp=player_hp,
        player_max_hp=player_max_hp,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _make_necrobinder_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1700,
) -> CombatState:
    combat = CombatState(
        player_hp=72,
        player_max_hp=72,
        deck=create_necrobinder_starter_deck(),
        rng_seed=seed,
        character_id="Necrobinder",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _make_regent_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1800,
) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_regent_starter_deck(),
        rng_seed=seed,
        character_id="Regent",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicStarterCommonRestUpgradeHooksParity:
    def test_burning_blood_heals_six_after_combat_victory(self):
        """Matches BurningBlood.cs: heal 6 HP after winning combat."""
        combat = _make_ironclad_combat(["BurningBlood"], seed=1601, player_hp=50, player_max_hp=80)
        enemy = combat.enemies[0]

        combat.deal_damage(combat.player, enemy, 999, ValueProp.UNPOWERED)

        assert combat.is_over
        assert combat.player_won
        assert combat.player.current_hp == 56

    def test_phylactery_unbound_summons_five_at_start_then_two_each_player_turn(self):
        """Matches PhylacteryUnbound.cs: summon 5 pre-combat and +2 each player turn."""
        combat = _make_necrobinder_combat(["PhylacteryUnbound"], seed=1602)
        osty = combat.get_osty(combat.player)
        assert osty is not None

        # 5 from before_combat_start + 2 from first player-turn start.
        assert osty.max_hp == 7
        assert osty.current_hp == 7

        relic = next(
            relic for relic in combat.relics
            if relic.relic_id.name == "PHYLACTERY_UNBOUND"
        )
        relic.after_side_turn_start(combat.player, CombatSide.ENEMY, combat)
        assert osty.max_hp == 7

        relic.after_side_turn_start(combat.player, CombatSide.PLAYER, combat)
        assert osty.max_hp == 9
        assert osty.current_hp == 9

    def test_fencing_manual_forges_ten_on_first_player_turn_start(self):
        """Matches FencingManual.cs: first player turn start forges 10."""
        combat = _make_regent_combat(["FencingManual"], seed=1603)
        blades = [card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE]

        assert len(blades) == 1
        assert blades[0].base_damage == 20

    def test_pendulum_draws_one_extra_card_every_third_turn(self):
        """Matches Pendulum.cs: every 3rd hand draw gains +1 card (counter cycles mod 3)."""
        combat = _make_ironclad_combat(["Pendulum"], seed=1604)

        # Turn 1: counter 0 -> 1, no bonus.
        assert len(combat.hand) == 5

        combat.end_player_turn()
        assert combat.round_number == 2
        # Turn 2: counter 1 -> 2, no bonus.
        assert len(combat.hand) == 5

        combat.end_player_turn()
        assert combat.round_number == 3
        # Turn 3: counter 2 -> 0, +1 card.
        assert len(combat.hand) == 6

        combat.end_player_turn()
        assert combat.round_number == 4
        # Cycle restarts: counter 0 -> 1, no bonus.
        assert len(combat.hand) == 5

    def test_regal_pillow_adds_fifteen_to_rest_site_heal_amount(self):
        """Matches RegalPillow.cs: rest-site heal amount gains +15."""
        run_state = RunState(seed=1605, character_id="Ironclad")
        run_state.player.max_hp = 80
        run_state.player.current_hp = 20
        assert run_state.player.obtain_relic("REGAL_PILLOW")

        result = HealOption().execute(run_state.player)

        assert result == "Healed 39 HP"
        assert run_state.player.current_hp == 59

    def test_tiny_mailbox_adds_potion_reward_to_rest_site_heal(self):
        """Matches TinyMailbox.cs: healing at rest site adds a Potion reward."""
        run_state = RunState(seed=1606, character_id="Ironclad")
        assert run_state.player.obtain_relic("TINY_MAILBOX")
        run_state.pending_rewards.clear()

        HealOption().execute(run_state.player)

        potion_rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, PotionReward)]
        assert len(potion_rewards) == 1

    def test_war_paint_upgrades_two_random_skill_cards_on_obtain(self):
        """Matches WarPaint.cs: obtain upgrades 2 Skill cards."""
        run_state = RunState(seed=1607, character_id="Ironclad")
        run_state.player.deck = create_ironclad_starter_deck()
        skills_upgraded_before = sum(
            1 for card in run_state.player.deck if card.card_type == CardType.SKILL and card.upgraded
        )
        attacks_upgraded_before = sum(
            1 for card in run_state.player.deck if card.card_type == CardType.ATTACK and card.upgraded
        )

        assert run_state.player.obtain_relic("WAR_PAINT")

        skills_upgraded_after = sum(
            1 for card in run_state.player.deck if card.card_type == CardType.SKILL and card.upgraded
        )
        attacks_upgraded_after = sum(
            1 for card in run_state.player.deck if card.card_type == CardType.ATTACK and card.upgraded
        )

        assert skills_upgraded_after - skills_upgraded_before == 2
        assert attacks_upgraded_after == attacks_upgraded_before

    def test_war_paint_uses_niche_rng_for_random_upgrades(self):
        """Matches WarPaint.cs: random upgrade selection is from RunState.Rng.Niche."""
        run_state = RunState(seed=1607, character_id="Ironclad")
        run_state.player.deck = [
            create_card(CardId.BLOOD_WALL),
            create_card(CardId.DEFEND_IRONCLAD),
            create_card(CardId.ARMAMENTS),
        ]
        run_state.rng.rewards = _FailingShuffleRng()
        run_state.rng.niche = _ReverseShuffleRng()

        assert run_state.player.obtain_relic("WAR_PAINT")

        upgraded = {card.card_id for card in run_state.player.deck if card.upgraded}
        assert upgraded == {CardId.BLOOD_WALL, CardId.DEFEND_IRONCLAD}

    def test_book_of_five_rings_heals_on_every_fifth_card_added_and_keeps_total_counter(self):
        """Matches BookOfFiveRings.cs: CardsAdded is cumulative; trigger uses modulo 5."""
        run_state = RunState(seed=1608, character_id="Ironclad")
        run_state.player.max_hp = 80
        run_state.player.current_hp = 20
        assert run_state.player.obtain_relic("BOOK_OF_FIVE_RINGS")
        relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "BOOK_OF_FIVE_RINGS")

        for _ in range(4):
            run_state.player.add_card_instance_to_deck(make_strike_ironclad())
        assert run_state.player.current_hp == 20
        assert relic._cards_added == BookOfFiveRings.CARDS_THRESHOLD - 1

        run_state.player.add_card_instance_to_deck(make_strike_ironclad())
        assert run_state.player.current_hp == 40
        assert relic._cards_added == BookOfFiveRings.CARDS_THRESHOLD

        for _ in range(5):
            run_state.player.add_card_instance_to_deck(make_strike_ironclad())
        assert run_state.player.current_hp == 60
        assert relic._cards_added == BookOfFiveRings.CARDS_THRESHOLD * 2
