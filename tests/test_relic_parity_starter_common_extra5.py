"""Fifth batch of focused parity tests for starter/common relic hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck, make_bash, make_strike_ironclad
from sts2_env.cards.necrobinder import create_necrobinder_starter_deck
from sts2_env.cards.regent import create_regent_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1200,
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
    seed: int = 1300,
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
    seed: int = 1400,
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


class TestRelicParityStarterCommonExtra5:
    def test_black_blood_heals_twelve_after_combat_victory(self):
        """Matches BlackBlood.cs: after winning combat, heal 12 HP."""
        combat = _make_ironclad_combat(["BlackBlood"], seed=1201, player_hp=60, player_max_hp=80)
        enemy = combat.enemies[0]

        combat.deal_damage(combat.player, enemy, 999, ValueProp.UNPOWERED)

        assert combat.is_over
        assert combat.player_won
        assert combat.player.current_hp == 72

    def test_bound_phylactery_summons_once_before_combat_and_then_each_turn_after_first(self):
        """Matches BoundPhylactery.cs: summon 1 before combat and +1 each turn from round 2."""
        combat = _make_necrobinder_combat(["BoundPhylactery"], seed=1202)
        osty = combat.get_osty(combat.player)

        assert osty is not None
        osty.max_hp = 50
        osty.current_hp = 50

        combat.end_player_turn()
        assert combat.round_number == 2
        assert osty.max_hp == 51

        combat.end_player_turn()
        assert combat.round_number == 3
        assert osty.max_hp == 52

    def test_divine_right_grants_three_stars_at_first_player_turn_start(self):
        """Matches DivineRight.cs: gain 3 Stars on round 1 only."""
        combat = _make_regent_combat(["DivineRight"], seed=1203)

        assert combat.stars == 3
        assert combat.player.stars == 3

        combat.end_player_turn()
        assert combat.round_number == 2
        assert combat.stars == 3

    def test_divine_destiny_grants_six_stars_at_first_player_turn_start(self):
        """Matches DivineDestiny.cs: gain 6 Stars on round 1 only."""
        combat = _make_regent_combat(["DivineDestiny"], seed=1204)

        assert combat.stars == 6
        assert combat.player.stars == 6

        combat.end_player_turn()
        assert combat.round_number == 2
        assert combat.stars == 6

    def test_bone_flute_triggers_block_only_when_own_osty_attacks(self):
        """Matches BoneFlute.cs: gain 2 block when owner's Osty performs an attack."""
        combat = _make_necrobinder_combat(["BoneFlute"], seed=1205)
        enemy = combat.enemies[0]
        start_block = combat.player.block

        combat.deal_damage(combat.player, enemy, 4, ValueProp.MOVE)
        assert combat.player.block == start_block

        combat.summon_osty(combat.player, 5)
        assert combat.osty is not None
        combat.deal_damage(combat.osty, enemy, 4, ValueProp.MOVE)
        assert combat.player.block == start_block + 2

    def test_oddly_smooth_stone_applies_one_dexterity_at_combat_start(self):
        """Matches OddlySmoothStone.cs: gain 1 Dexterity at combat start."""
        combat = _make_ironclad_combat(["OddlySmoothStone"], seed=1206)
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 1

    def test_strike_dummy_only_buffs_strike_tagged_attacks(self):
        """Matches StrikeDummy.cs: Strikes gain +3 damage; non-Strike attacks do not."""
        combat = _make_ironclad_combat(["StrikeDummy"], seed=1207)
        enemy = combat.enemies[0]
        combat.hand = [make_strike_ironclad(), make_bash()]
        combat.energy = 3

        strike_start = enemy.current_hp
        assert combat.play_card(0, 0)
        strike_damage = strike_start - enemy.current_hp

        bash_start = enemy.current_hp
        assert combat.play_card(0, 0)
        bash_damage = bash_start - enemy.current_hp

        assert strike_damage == 9
        assert bash_damage == 8
