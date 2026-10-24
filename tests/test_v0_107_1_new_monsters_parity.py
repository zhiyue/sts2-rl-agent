"""Parity tests for the last batch of new monsters/encounters (v0.107.1).

Ground truth: decompiled C# under decompiled/MegaCrit.Sts2.Core.Models.
Covers Aeonglass (act 4 queen-tier boss), the AeonglassBoss and SeapunkNormal
encounter compositions, and pool registration.
"""

from __future__ import annotations

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, PowerId
from sts2_env.core.rng import Rng
from sts2_env.encounters.act4 import (
    ALL_ACT4_ENCOUNTERS,
    BOSS_ENCOUNTERS as ACT4_BOSS,
    NORMAL_ENCOUNTERS as ACT4_NORMAL,
    setup_aeonglass_boss,
    setup_seapunk_normal,
)
from sts2_env.monsters.act4 import (
    AEONGLASS_ARTIFACT,
    AEONGLASS_BASE_EBB_DAMAGE,
    AEONGLASS_BASE_EYE_LASERS_DAMAGE,
    AEONGLASS_BASE_HP,
    AEONGLASS_BASE_INTENSITY_STRENGTH,
    AEONGLASS_BASE_WITHER_AMOUNT,
    AEONGLASS_DEADLY_EBB_DAMAGE,
    AEONGLASS_DEADLY_EYE_LASERS_DAMAGE,
    AEONGLASS_DEADLY_INTENSITY_STRENGTH,
    AEONGLASS_DEADLY_WITHER_AMOUNT,
    AEONGLASS_EBB_BLOCK,
    AEONGLASS_EBB_MOVE,
    AEONGLASS_EYE_LASERS_MOVE,
    AEONGLASS_EYE_LASERS_REPEAT,
    AEONGLASS_INCREASING_INTENSITY_MOVE,
    AEONGLASS_MONSTER_ID,
    AEONGLASS_TOUGH_HP,
    AEONGLASS_WITHERING_PRESENCE_AMOUNT,
    CALCIFIED_CULTIST_MONSTER_ID,
    SEAPUNK_MONSTER_ID,
    create_aeonglass,
)
from sts2_env.powers.monster import WITHERING_PRESENCE_POWER_ID, WitheringPresencePower

DEFAULT_TEST_PLAYER_HP = 80


def _make_combat(seed: int = 7) -> CombatState:
    return CombatState(
        player_hp=DEFAULT_TEST_PLAYER_HP,
        player_max_hp=DEFAULT_TEST_PLAYER_HP,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
    )


def _run_ai(ai, rng: Rng, n: int) -> list[str]:
    moves = [ai.current_move.state_id]
    ai.on_move_performed()
    for _ in range(n - 1):
        ai.roll_move(rng)
        moves.append(ai.current_move.state_id)
        ai.on_move_performed()
    return moves


class TestAeonglassParity:
    """C# ref: decompiled/MegaCrit.Sts2.Core.Models.Monsters/Aeonglass.cs."""

    def test_hp_and_room_setup_powers_match_csharp(self):
        """MinInitialHp => AscensionHelper.GetValueIfAscension(ToughEnemies, 535, 512);
        MaxInitialHp => MinInitialHp (fixed HP). AfterAddedToRoom: WitheringPresence
        (Amount 6) on the opposing players + Artifact(3) on itself."""
        boss, boss_ai = create_aeonglass(Rng(42))
        assert boss.monster_id == AEONGLASS_MONSTER_ID
        assert boss.max_hp == AEONGLASS_BASE_HP
        assert isinstance(boss.powers[WITHERING_PRESENCE_POWER_ID], WitheringPresencePower)
        assert boss.get_power_amount(WITHERING_PRESENCE_POWER_ID) == AEONGLASS_WITHERING_PRESENCE_AMOUNT
        assert boss.get_power_amount(PowerId.ARTIFACT) == AEONGLASS_ARTIFACT

        combat = _make_combat(42)
        combat.add_enemy(boss, boss_ai)
        assert boss.get_power_amount(WITHERING_PRESENCE_POWER_ID) == AEONGLASS_WITHERING_PRESENCE_AMOUNT

        tough, tough_ai = create_aeonglass(Rng(43), ascension_level=8)
        assert tough.max_hp == AEONGLASS_TOUGH_HP

    def test_move_cycle_matches_csharp(self):
        """GenerateMoveStateMachine: EBB -> EYE_LASERS -> INCREASING_INTENSITY -> EBB,
        initial state EBB_MOVE."""
        boss, boss_ai = create_aeonglass(Rng(44))
        combat = _make_combat(44)
        combat.add_enemy(boss, boss_ai)
        assert _run_ai(boss_ai, Rng(44), 7) == [
            AEONGLASS_EBB_MOVE,
            AEONGLASS_EYE_LASERS_MOVE,
            AEONGLASS_INCREASING_INTENSITY_MOVE,
            AEONGLASS_EBB_MOVE,
            AEONGLASS_EYE_LASERS_MOVE,
            AEONGLASS_INCREASING_INTENSITY_MOVE,
            AEONGLASS_EBB_MOVE,
        ]

    def test_intents_match_csharp(self):
        """EBB_MOVE: SingleAttackIntent(26) + DefendIntent. EYE_LASERS_MOVE:
        MultiAttackIntent(11, 2). INCREASING_INTENSITY_MOVE: StatusIntent(1) +
        BuffIntent (base ascension)."""
        boss, boss_ai = create_aeonglass(Rng(45))
        ebb = boss_ai.states[AEONGLASS_EBB_MOVE]
        assert [(i.intent_type.name, i.damage, i.hits) for i in ebb.intents] == [
            ("ATTACK", AEONGLASS_BASE_EBB_DAMAGE, 1),
            ("DEFEND", 0, 1),
        ]
        eye_lasers = boss_ai.states[AEONGLASS_EYE_LASERS_MOVE]
        assert [(i.intent_type.name, i.damage, i.hits) for i in eye_lasers.intents] == [
            ("MULTI_ATTACK", AEONGLASS_BASE_EYE_LASERS_DAMAGE, AEONGLASS_EYE_LASERS_REPEAT),
        ]
        intensity = boss_ai.states[AEONGLASS_INCREASING_INTENSITY_MOVE]
        assert [(i.intent_type.name, i.damage, i.hits) for i in intensity.intents] == [
            ("STATUS_CARD", 0, AEONGLASS_BASE_WITHER_AMOUNT),
            ("BUFF", 0, 1),
        ]

    def test_ebb_move_deals_damage_and_gains_block(self):
        """EbbMove: attack 26 then gain 33 block (no ascension scaling on block)."""
        boss, boss_ai = create_aeonglass(Rng(46))
        combat = _make_combat(46)
        combat.add_enemy(boss, boss_ai)
        hp_before = combat.player.current_hp
        boss_ai.states[AEONGLASS_EBB_MOVE].perform(combat)
        assert combat.player.current_hp == hp_before - AEONGLASS_BASE_EBB_DAMAGE
        assert boss.block == AEONGLASS_EBB_BLOCK

    def test_ebb_move_skips_block_when_lethal(self):
        """The block follows the attack; once the player dies the move ends
        (CreatureCmd.GainBlock never resolves in C#)."""
        boss, boss_ai = create_aeonglass(Rng(47))
        combat = _make_combat(47)
        combat.add_enemy(boss, boss_ai)
        combat.player.current_hp = AEONGLASS_BASE_EBB_DAMAGE
        boss_ai.states[AEONGLASS_EBB_MOVE].perform(combat)
        assert combat.is_over
        assert combat.player_won is False
        assert boss.block == 0

    def test_eye_lasers_move_hits_twice(self):
        """EyeLasersMove: DamageCmd.WithHitCount(2) at 11 damage each."""
        boss, boss_ai = create_aeonglass(Rng(48))
        combat = _make_combat(48)
        combat.add_enemy(boss, boss_ai)
        hp_before = combat.player.current_hp
        boss_ai.states[AEONGLASS_EYE_LASERS_MOVE].perform(combat)
        assert combat.player.current_hp == hp_before - AEONGLASS_BASE_EYE_LASERS_DAMAGE * 2

    def test_increasing_intensity_upgrades_withers_and_ramps_strength(self):
        """IncreasingIntensityMove: FakeUpgrade every Wither in the targets'
        combat card lists, WitherUpgradeCount++, add WitherAmount Withers to the
        discard pile, gain Strength (base 3 + AdditionalStrength), then
        AdditionalStrength++."""
        boss, boss_ai = create_aeonglass(Rng(49))
        combat = _make_combat(49)
        combat.discard_pile.clear()
        combat.add_enemy(boss, boss_ai)

        boss_ai.states[AEONGLASS_INCREASING_INTENSITY_MOVE].perform(combat)
        withers = [c for c in combat.discard_pile if c.card_id is CardId.WITHER]
        assert len(withers) == AEONGLASS_BASE_WITHER_AMOUNT
        # Count 1: damage 3 + 3 * 1 (C# Wither.FakeUpgrade: UpgradeValueBy(3m)).
        assert [c.effect_vars["damage"] for c in withers] == [6]
        assert boss.get_power_amount(PowerId.STRENGTH) == AEONGLASS_BASE_INTENSITY_STRENGTH

        boss_ai.states[AEONGLASS_INCREASING_INTENSITY_MOVE].perform(combat)
        withers = [c for c in combat.discard_pile if c.card_id is CardId.WITHER]
        assert len(withers) == 2 * AEONGLASS_BASE_WITHER_AMOUNT
        # Old and new Withers all carry exactly count 2 of fake upgrades.
        assert [c.effect_vars["damage"] for c in withers] == [9, 9]
        assert boss.get_power_amount(PowerId.STRENGTH) == (
            AEONGLASS_BASE_INTENSITY_STRENGTH + AEONGLASS_BASE_INTENSITY_STRENGTH + 1
        )

        boss_ai.states[AEONGLASS_INCREASING_INTENSITY_MOVE].perform(combat)
        withers = [c for c in combat.discard_pile if c.card_id is CardId.WITHER]
        assert [c.effect_vars["damage"] for c in withers] == [12, 12, 12]
        assert boss.get_power_amount(PowerId.STRENGTH) == (
            AEONGLASS_BASE_INTENSITY_STRENGTH
            + (AEONGLASS_BASE_INTENSITY_STRENGTH + 1)
            + (AEONGLASS_BASE_INTENSITY_STRENGTH + 2)
        )

    def test_increasing_intensity_upgrades_withers_in_all_player_piles(self):
        """C# iterates PlayerCombatState.AllCards, not just the discard pile."""
        boss, boss_ai = create_aeonglass(Rng(50))
        combat = _make_combat(50)
        combat.add_enemy(boss, boss_ai)
        hand_wither = create_card(CardId.WITHER)
        combat.hand = [hand_wither]

        boss_ai.states[AEONGLASS_INCREASING_INTENSITY_MOVE].perform(combat)
        assert hand_wither.effect_vars["damage"] == 6

    def test_generated_withers_match_upgrade_count(self):
        """Aeonglass.AfterCardGeneratedForCombat fires MatchWitherToUpgradeCount
        for every Wither generated in combat, so a WitheringPresence hand Wither
        lands already upgraded to the current count."""
        boss, boss_ai = create_aeonglass(Rng(51))
        combat = _make_combat(51)
        combat.add_enemy(boss, boss_ai)
        boss_ai.states[AEONGLASS_INCREASING_INTENSITY_MOVE].perform(combat)

        combat.hand = [make_strike_ironclad() for _ in range(6)]
        combat.energy = 20
        for _ in range(6):
            assert combat.play_card(0, 0)

        hand_withers = [c for c in combat.hand if c.card_id is CardId.WITHER]
        assert len(hand_withers) == 1
        assert hand_withers[0].effect_vars["damage"] == 6

    def test_ascension_scaling_matches_csharp(self):
        """DeadlyEnemies(9): EbbDamage 32, EyeLasersDamage 12,
        IncreasingIntensityBaseStrength 4, WitherAmount 2."""
        boss, boss_ai = create_aeonglass(Rng(52), ascension_level=9)
        combat = _make_combat(52)
        combat.ascension_level = 9
        combat.discard_pile.clear()
        combat.add_enemy(boss, boss_ai)

        assert boss_ai.states[AEONGLASS_INCREASING_INTENSITY_MOVE].intents[0].hits == (
            AEONGLASS_DEADLY_WITHER_AMOUNT
        )
        boss_ai.states[AEONGLASS_EBB_MOVE].perform(combat)
        assert combat.player.current_hp == DEFAULT_TEST_PLAYER_HP - AEONGLASS_DEADLY_EBB_DAMAGE

        boss_ai.states[AEONGLASS_EYE_LASERS_MOVE].perform(combat)
        assert combat.player.current_hp == (
            DEFAULT_TEST_PLAYER_HP - AEONGLASS_DEADLY_EBB_DAMAGE - AEONGLASS_DEADLY_EYE_LASERS_DAMAGE * 2
        )

        boss_ai.states[AEONGLASS_INCREASING_INTENSITY_MOVE].perform(combat)
        withers = [c for c in combat.discard_pile if c.card_id is CardId.WITHER]
        assert len(withers) == AEONGLASS_DEADLY_WITHER_AMOUNT
        assert [c.effect_vars["damage"] for c in withers] == [6, 6]
        assert boss.get_power_amount(PowerId.STRENGTH) == AEONGLASS_DEADLY_INTENSITY_STRENGTH


class TestNewAct4Encounters:
    """C# refs: Encounters/AeonglassBoss.cs, Encounters/SeapunkNormal.cs."""

    def test_aeonglass_boss_is_single_aeonglass(self):
        """AeonglassBoss.GenerateMonsters: one Aeonglass, RoomType.Boss."""
        combat = _make_combat(53)
        setup_aeonglass_boss(combat, Rng(53))
        assert [enemy.monster_id for enemy in combat.enemies] == [AEONGLASS_MONSTER_ID]
        assert all(combat.enemy_ais[enemy.combat_id].current_move.is_move for enemy in combat.enemies)
        assert combat.enemies[0].max_hp == AEONGLASS_BASE_HP

    def test_seapunk_normal_is_cultist_then_seapunk(self):
        """SeapunkNormal.GenerateMonsters: CalcifiedCultist first, then Seapunk."""
        combat = _make_combat(54)
        setup_seapunk_normal(combat, Rng(54))
        assert [enemy.monster_id for enemy in combat.enemies] == [
            CALCIFIED_CULTIST_MONSTER_ID,
            SEAPUNK_MONSTER_ID,
        ]
        assert all(combat.enemy_ais[enemy.combat_id].current_move.is_move for enemy in combat.enemies)

    def test_new_encounters_registered_in_act4_pools(self):
        assert setup_aeonglass_boss in ACT4_BOSS
        assert setup_seapunk_normal in ACT4_NORMAL
        assert setup_aeonglass_boss in ALL_ACT4_ENCOUNTERS
        assert setup_seapunk_normal in ALL_ACT4_ENCOUNTERS
