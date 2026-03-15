"""Tests for individual card effects: Strike, Defend, Bash (base and upgraded)."""

import pytest

from sts2_env.cards.base import CardInstance
from sts2_env.cards.ironclad_basic import (
    make_bash,
    make_defend_ironclad,
    make_strike_ironclad,
)
from sts2_env.cards.registry import play_card_effect
from sts2_env.core.constants import BASE_DRAW
from sts2_env.core.creature import Creature
from sts2_env.core.damage import calculate_damage
from sts2_env.core.enums import (
    CardId,
    CardType,
    CombatSide,
    PowerId,
    TargetType,
    ValueProp,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.rng import Rng

import sts2_env.powers  # noqa: F401


@pytest.fixture
def small_combat() -> CombatState:
    """Minimal combat with a standard Ironclad starter deck vs ShrinkerBeetle."""
    from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck
    from sts2_env.monsters.act1_weak import create_shrinker_beetle

    deck = create_ironclad_starter_deck()
    combat = CombatState(player_hp=80, player_max_hp=80, deck=deck, rng_seed=42)
    rng = Rng(42)
    creature, ai = create_shrinker_beetle(rng)
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


# ---------------------------------------------------------------------------
#  Strike tests
# ---------------------------------------------------------------------------

class TestStrike:
    def test_strike_deals_6_damage(self, small_combat):
        """Strike deals 6 damage to an enemy."""
        enemy = small_combat.enemies[0]
        initial_hp = enemy.current_hp
        strike = make_strike_ironclad()
        play_card_effect(strike, small_combat, enemy)
        assert enemy.current_hp == initial_hp - 6

    def test_strike_upgraded_deals_9_damage(self, small_combat):
        """Upgraded Strike deals 9 damage."""
        from sts2_env.cards.ironclad import make_strike_ironclad as make_strike_upgraded
        enemy = small_combat.enemies[0]
        initial_hp = enemy.current_hp
        strike = make_strike_upgraded(upgraded=True)
        play_card_effect(strike, small_combat, enemy)
        assert enemy.current_hp == initial_hp - 9

    def test_strike_with_strength(self, small_combat):
        """Strength adds to Strike damage."""
        enemy = small_combat.enemies[0]
        initial_hp = enemy.current_hp
        small_combat.player.apply_power(PowerId.STRENGTH, 3)
        strike = make_strike_ironclad()
        play_card_effect(strike, small_combat, enemy)
        assert enemy.current_hp == initial_hp - 9  # 6 + 3


# ---------------------------------------------------------------------------
#  Defend tests
# ---------------------------------------------------------------------------

class TestDefend:
    def test_defend_gains_5_block(self, small_combat):
        """Defend gives 5 block to the player."""
        assert small_combat.player.block == 0
        defend = make_defend_ironclad()
        play_card_effect(defend, small_combat, None)
        assert small_combat.player.block == 5

    def test_defend_upgraded_gains_8_block(self, small_combat):
        """Upgraded Defend gives 8 block."""
        from sts2_env.cards.ironclad import make_defend_ironclad as make_defend_upgraded
        defend = make_defend_upgraded(upgraded=True)
        play_card_effect(defend, small_combat, None)
        assert small_combat.player.block == 8

    def test_defend_with_dexterity(self, small_combat):
        """Dexterity adds to Defend block."""
        small_combat.player.apply_power(PowerId.DEXTERITY, 2)
        defend = make_defend_ironclad()
        play_card_effect(defend, small_combat, None)
        assert small_combat.player.block == 7  # 5 + 2

    def test_defend_stacks_block(self, small_combat):
        """Playing Defend twice gives 10 block total."""
        d1 = make_defend_ironclad()
        d2 = make_defend_ironclad()
        play_card_effect(d1, small_combat, None)
        play_card_effect(d2, small_combat, None)
        assert small_combat.player.block == 10


# ---------------------------------------------------------------------------
#  Bash tests
# ---------------------------------------------------------------------------

class TestBash:
    def test_bash_deals_8_damage(self, small_combat):
        """Bash deals 8 damage."""
        enemy = small_combat.enemies[0]
        initial_hp = enemy.current_hp
        bash = make_bash()
        play_card_effect(bash, small_combat, enemy)
        assert enemy.current_hp == initial_hp - 8

    def test_bash_applies_2_vulnerable(self, small_combat):
        """Bash applies 2 Vulnerable to the target."""
        enemy = small_combat.enemies[0]
        bash = make_bash()
        play_card_effect(bash, small_combat, enemy)
        assert enemy.has_power(PowerId.VULNERABLE)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

    def test_bash_damage_then_vulnerable(self, small_combat):
        """After Bash, subsequent attacks deal 1.5x to the Vulnerable target."""
        enemy = small_combat.enemies[0]
        bash = make_bash()
        play_card_effect(bash, small_combat, enemy)
        hp_after_bash = enemy.current_hp
        strike = make_strike_ironclad()
        play_card_effect(strike, small_combat, enemy)
        # 6 * 1.5 = 9
        assert enemy.current_hp == hp_after_bash - 9

    def test_bash_upgraded_deals_10_damage_and_3_vuln(self, small_combat):
        """Upgraded Bash deals 10 damage and applies 3 Vulnerable."""
        from sts2_env.cards.ironclad import make_bash as make_bash_upgraded
        enemy = small_combat.enemies[0]
        initial_hp = enemy.current_hp
        bash = make_bash_upgraded(upgraded=True)
        play_card_effect(bash, small_combat, enemy)
        assert enemy.current_hp == initial_hp - 10
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 3


# ---------------------------------------------------------------------------
#  Upgrade factory tests
# ---------------------------------------------------------------------------

class TestUpgrade:
    def test_strike_base_damage_6(self):
        strike = make_strike_ironclad()
        assert strike.base_damage == 6
        assert not strike.upgraded

    def test_strike_upgraded_damage_9(self):
        from sts2_env.cards.ironclad import make_strike_ironclad as mk
        strike = mk(upgraded=True)
        assert strike.base_damage == 9
        assert strike.upgraded

    def test_defend_base_block_5(self):
        defend = make_defend_ironclad()
        assert defend.base_block == 5
        assert not defend.upgraded

    def test_defend_upgraded_block_8(self):
        from sts2_env.cards.ironclad import make_defend_ironclad as mk
        defend = mk(upgraded=True)
        assert defend.base_block == 8
        assert defend.upgraded

    def test_bash_base_damage_8(self):
        bash = make_bash()
        assert bash.base_damage == 8
        assert bash.effect_vars["vulnerable"] == 2

    def test_bash_upgraded_damage_10_vuln_3(self):
        from sts2_env.cards.ironclad import make_bash as mk
        bash = mk(upgraded=True)
        assert bash.base_damage == 10
        assert bash.effect_vars["vulnerable"] == 3
        assert bash.upgraded
