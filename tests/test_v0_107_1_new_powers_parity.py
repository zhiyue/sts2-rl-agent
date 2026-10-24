"""Parity tests for the four powers added in game v0.107.1.

Ground truth (decompiled, v0.107.1 working tree — do NOT use git show, HEAD
holds the March build):
- decompiled/MegaCrit.Sts2.Core.Models.Powers/BorrowedTimePower.cs
- decompiled/MegaCrit.Sts2.Core.Models.Powers/NoEnergyGainPower.cs
- decompiled/MegaCrit.Sts2.Core.Models.Powers/TaintedPower.cs
- decompiled/MegaCrit.Sts2.Core.Models.Powers/WitheringPresencePower.cs
Appliers: MegaCrit.Sts2.Core.Models.Cards/BorrowedTime.cs,
MegaCrit.Sts2.Core.Models.Cards/ExpectAFight.cs,
MegaCrit.Sts2.Core.Models.Powers/VitalSparkPower.cs (Tainted affliction),
MegaCrit.Sts2.Core.Models.Monsters/Aeonglass.cs.

PowerId.BORROWED_TIME / PowerId.TAINTED / PowerId.WITHERING_PRESENCE are not
yet members of sts2_env.core.enums.PowerId (gap reported, enums.py is out of
scope for this change), so those powers register under stable string fallback
keys until the enum lands.
"""

import sts2_env.powers  # noqa: F401  (trigger power registration)

import pytest

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import make_expect_a_fight
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import (
    CardId,
    CardType,
    CombatSide,
    PowerId,
    ValueProp,
)
from sts2_env.core.hooks import fire_after_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.powers.card_play_effects import (
    BORROWED_TIME_POWER_ID,
    BorrowedTimePower,
    NoEnergyGainPower,
)
from sts2_env.powers.damage_modifiers import TAINTED_POWER_ID, TaintedPower
from sts2_env.powers.monster import WITHERING_PRESENCE_POWER_ID, WitheringPresencePower
from sts2_env.core.creature import get_power_class


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[make_strike_ironclad() for _ in range(10)],
        rng_seed=42,
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


# ---------------------------------------------------------------------------
# BorrowedTimePower (decompiled/MegaCrit.Sts2.Core.Models.Powers/BorrowedTimePower.cs)
# ---------------------------------------------------------------------------
class TestBorrowedTimePowerParity:
    def test_applied_by_borrowed_time_card_with_energy_gain(self):
        """BorrowedTime.cs OnPlay: `PlayerCmd.GainEnergy(Energy)` (4; 6 upgraded,
        OnUpgrade `Energy.UpgradeValueBy(2m)`) then
        `PowerCmd.Apply<BorrowedTimePower>(ExtraCost = 1)` on Owner.Creature."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.BORROWED_TIME)]
        combat.energy = 1

        assert combat.play_card(0)
        # energy: 1 - 1 (play cost) + 4 (EnergyVar) == 4.
        assert combat.energy == 4
        assert combat.player.get_power_amount(BORROWED_TIME_POWER_ID) == 1

    def test_upgraded_card_gain(self):
        """BorrowedTime.cs OnUpgrade: Energy.UpgradeValueBy(2m) -> 6."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.BORROWED_TIME, upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.energy == 6
        assert combat.player.get_power_amount(BORROWED_TIME_POWER_ID) == 1

    def test_raises_energy_cost_of_owned_cards(self):
        """BorrowedTimePower.cs TryModifyEnergyCostInCombat (lines 21-30):
        `modifiedCost = originalCost + Amount` when `card.Owner.Creature == Owner`;
        the sim surfaces this via CombatState.modified_card_cost ->
        PowerInstance.modify_card_cost (used for both the can-play check and the
        energy spent, combat.py:1170/1384/1412)."""
        combat = _make_combat()
        strike = make_strike_ironclad()
        combat.player.apply_power(BORROWED_TIME_POWER_ID, 1)

        assert combat.modified_card_cost(combat.player, strike) == strike.cost + 1

    def test_stacks_add_up(self):
        """PowerStackType.Counter: one stack per ExtraCost; playing BorrowedTime
        while the power is active would add another stack
        (PowerCmd.Apply stacks Counter powers)."""
        combat = _make_combat()
        strike = make_strike_ironclad()
        combat.player.apply_power(BORROWED_TIME_POWER_ID, 1)
        combat.player.apply_power(BORROWED_TIME_POWER_ID, 2)

        assert combat.player.get_power_amount(BORROWED_TIME_POWER_ID) == 3
        assert combat.modified_card_cost(combat.player, strike) == strike.cost + 3

    def test_only_the_cards_owner_is_affected(self):
        """BorrowedTimePower.cs line 24: `if (card.Owner.Creature != base.Owner)
        return false;` — a power on one creature must not raise another
        creature's card costs (the sim dispatcher only consults the card
        owner's own powers)."""
        combat = _make_combat()
        combat.enemies[0].apply_power(BORROWED_TIME_POWER_ID, 1)
        strike = make_strike_ironclad()

        assert combat.modified_card_cost(combat.player, strike) == strike.cost

    def test_cards_generated_after_application_are_affected(self):
        """The C# power modifies cost at play time, so cards not yet owned when
        BorrowedTime was played (drawn or generated later) are also raised —
        the previous per-card temporary override emulation missed this."""
        combat = _make_combat()
        combat.player.apply_power(BORROWED_TIME_POWER_ID, 1)
        late_card = make_strike_ironclad()  # "drawn" after the power landed

        assert combat.modified_card_cost(combat.player, late_card) == late_card.cost + 1

    def test_x_cost_cards_untouched(self):
        """X-cost cards have no fixed cost to raise; C#
        BorrowedTimePower does not override TryModifyStarCost and the sim
        dispatcher (CombatState.modified_card_cost) skips has_energy_cost_x."""
        combat = _make_combat()
        x_card = create_card(CardId.WHIRLWIND)
        assert x_card.has_energy_cost_x
        combat.player.apply_power(BORROWED_TIME_POWER_ID, 1)

        assert combat.modified_card_cost(combat.player, x_card) == 0

    def test_removed_at_end_of_own_side_turn_only(self):
        """BorrowedTimePower.cs AfterSideTurnEnd (lines 32-38): removed when the
        owner is among the participants of the side that just ended — i.e. at
        the end of the owner's own side turn; an enemy-side turn end leaves it
        alone."""
        combat = _make_combat()
        combat.player.apply_power(BORROWED_TIME_POWER_ID, 1)

        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert combat.player.has_power(BORROWED_TIME_POWER_ID)

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert not combat.player.has_power(BORROWED_TIME_POWER_ID)

    def test_full_turn_flow_removes_power_and_unbumps_cost(self):
        """end_player_turn runs the player side end (AfterSideTurnEnd with the
        player as participant) -> the power is gone for the next turn."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.BORROWED_TIME)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.has_power(BORROWED_TIME_POWER_ID)

        combat.end_player_turn()
        assert not combat.player.has_power(BORROWED_TIME_POWER_ID)
        strike = make_strike_ironclad()
        assert combat.modified_card_cost(combat.player, strike) == strike.cost

    def test_artifact_blocks_the_debuff_but_not_the_energy(self):
        """PowerType.Debuff (BorrowedTimePower.cs line 15) -> PowerCmd.Apply is
        subject to Artifact; BorrowedTime.cs OnPlay gains the energy *before*
        applying the power, so the energy stands even when the power is blocked."""
        combat = _make_combat()
        combat.player.apply_power(PowerId.ARTIFACT, 1)
        combat.hand = [create_card(CardId.BORROWED_TIME)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.energy == 4
        assert not combat.player.has_power(BORROWED_TIME_POWER_ID)
        assert not combat.player.has_power(PowerId.ARTIFACT)

    def test_power_id_enum_gap_is_documented(self):
        """PowerId.BORROWED_TIME landed in enums.py; the fallback constant now
        resolves to the real enum member and the registry lookup follows it."""
        assert "BORROWED_TIME" in PowerId.__members__
        assert BORROWED_TIME_POWER_ID is PowerId.BORROWED_TIME
        assert get_power_class(BORROWED_TIME_POWER_ID) is BorrowedTimePower


# ---------------------------------------------------------------------------
# NoEnergyGainPower (decompiled/MegaCrit.Sts2.Core.Models.Powers/NoEnergyGainPower.cs)
# ---------------------------------------------------------------------------
class TestNoEnergyGainPowerParity:
    def test_blocks_all_energy_gain_for_owner(self):
        """NoEnergyGainPower.cs ModifyEnergyGain (lines 19-26): `if (player !=
        base.Owner.Player) return amount; return 0m;` — the owner gains no
        energy. The sim consults only the gaining creature's own powers
        (combat.gain_energy, combat.py:2155)."""
        combat = _make_combat()
        combat.player.apply_power(PowerId.NO_ENERGY_GAIN, 1)
        energy_before = combat.energy

        combat.gain_energy(combat.player, 3)
        assert combat.energy == energy_before

    def test_power_type_and_stack(self):
        """NoEnergyGainPower.cs: PowerType.Debuff (line 15),
        PowerStackType.Single (line 17) — reapplying cannot stack it past a
        single instance's amount semantics."""
        from sts2_env.core.enums import PowerStackType, PowerType

        assert NoEnergyGainPower.power_type is PowerType.DEBUFF
        assert NoEnergyGainPower.stack_type is PowerStackType.SINGLE

    def test_removed_at_end_of_own_side_turn(self):
        """NoEnergyGainPower.cs AfterSideTurnEnd (lines 34-40): removed when the
        owner participates in the side that just ended."""
        combat = _make_combat()
        combat.player.apply_power(PowerId.NO_ENERGY_GAIN, 1)

        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert combat.player.has_power(PowerId.NO_ENERGY_GAIN)

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert not combat.player.has_power(PowerId.NO_ENERGY_GAIN)

        # Energy gain works again after removal.
        energy_before = combat.energy
        combat.gain_energy(combat.player, 2)
        assert combat.energy == energy_before + 2

    def test_applied_by_expect_a_fight(self):
        """ExpectAFight.cs OnPlay: gain CalculatedEnergy (= Attacks in hand),
        then `PowerCmd.Apply<NoEnergyGainPower>(1)` on the owner — the gained
        energy stands, later gains this turn are blocked."""
        combat = _make_combat()
        combat.hand = [make_expect_a_fight(), make_strike_ironclad(), make_strike_ironclad()]
        combat.energy = 2

        # 2 (cost) paid, +2 gained (two Attacks in hand incl. ExpectAFight? No:
        # ExpectAFight is a Skill -> CalculatedEnergy counts the 2 Strikes).
        assert combat.play_card(0)
        assert combat.energy == 2
        assert combat.player.has_power(PowerId.NO_ENERGY_GAIN)

        combat.gain_energy(combat.player, 1)
        assert combat.energy == 2


# ---------------------------------------------------------------------------
# TaintedPower (decompiled/MegaCrit.Sts2.Core.Models.Powers/TaintedPower.cs)
# ---------------------------------------------------------------------------
class TestTaintedPowerParity:
    def test_powered_attack_on_owner_adds_amount(self):
        """TaintedPower.cs ModifyDamageAdditive (lines 18-29): `target == Owner`
        and `props.IsPoweredAttack()` (ValuePropExtensions.cs: Move set,
        Unpowered clear) -> `return Amount`; the sim hook is
        PowerInstance.modify_damage_additive inside calculate_damage."""
        combat = _make_combat()
        player = combat.player
        player.apply_power(TAINTED_POWER_ID, 2, applier=combat.enemies[0])
        hp_before = player.current_hp

        combat.deal_damage(combat.enemies[0], player, 10, ValueProp.MOVE)
        assert player.current_hp == hp_before - 12

    def test_unpowered_and_non_move_damage_ignored(self):
        """IsPoweredAttack requires the Move flag and no Unpowered flag;
        unpowered hits (e.g. Wither's DamageVar(3, Unpowered | Move)) and
        non-move HP loss get no bonus."""
        combat = _make_combat()
        player = combat.player
        player.apply_power(TAINTED_POWER_ID, 2, applier=combat.enemies[0])
        hp_before = player.current_hp

        combat.deal_damage(combat.enemies[0], player, 10, ValueProp.MOVE | ValueProp.UNPOWERED)
        assert player.current_hp == hp_before - 10

        combat.deal_damage(combat.enemies[0], player, 10, ValueProp.NONE)
        assert player.current_hp == hp_before - 20

    def test_other_targets_get_no_bonus(self):
        """TaintedPower.cs line 20: `if (target != base.Owner) return 0m;`."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        player = combat.player
        player.apply_power(TAINTED_POWER_ID, 2, applier=combat.enemies[0])
        hp_before = enemy.current_hp

        combat.deal_damage(player, enemy, 10, ValueProp.MOVE)
        assert enemy.current_hp == hp_before - 10

    def test_stacks_add_up(self):
        """PowerStackType.Counter (TaintedPower.cs line 16): VitalSpark applies
        one stack per afflicted Skill played (VitalSparkPower.cs line 54)."""
        combat = _make_combat()
        player = combat.player
        player.apply_power(TAINTED_POWER_ID, 1, applier=combat.enemies[0])
        player.apply_power(TAINTED_POWER_ID, 2, applier=combat.enemies[0])
        hp_before = player.current_hp

        combat.deal_damage(combat.enemies[0], player, 10, ValueProp.MOVE)
        assert player.current_hp == hp_before - 13

    def test_survives_player_turn_end_removed_at_enemy_turn_end(self):
        """TaintedPower.cs AfterSideTurnEnd (lines 31-38):
        `if (side == CombatSide.Enemy) { Flash(); Remove; }` — no participant
        check, so a Tainted applied during the player's turn survives the
        player turn end and is removed when the enemy side turn ends. This is
        the opposite timing of BorrowedTimePower/NoEnergyGainPower."""
        combat = _make_combat()
        player = combat.player
        player.apply_power(TAINTED_POWER_ID, 2, applier=combat.enemies[0])

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert player.has_power(TAINTED_POWER_ID)

        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert not player.has_power(TAINTED_POWER_ID)

    def test_power_id_enum_gap_is_documented(self):
        """PowerId.TAINTED landed in enums.py; the fallback constant now
        resolves to the real enum member and the registry lookup follows it."""
        assert "TAINTED" in PowerId.__members__
        assert TAINTED_POWER_ID is PowerId.TAINTED
        assert get_power_class(TAINTED_POWER_ID) is TaintedPower


# ---------------------------------------------------------------------------
# WitheringPresencePower
# (decompiled/MegaCrit.Sts2.Core.Models.Powers/WitheringPresencePower.cs)
# ---------------------------------------------------------------------------
class TestWitheringPresencePowerParity:
    def test_one_wither_per_six_cards_played(self):
        """WitheringPresencePower.cs AfterCardPlayed (lines 46-61): every card
        played by the tracked player decrements CardsLeft (canonical var 6,
        line 28 / `private const int _baseCardsLeft = 6`, line 16); at <= 0 a
        Wither is added to that player's hand
        (`CardPileCmd.AddToCombatAndPreview<Wither>(..., PileType.Hand, 1)`)
        and CardsLeft resets to 6. In C# the power sits on Aeonglass with
        Target set to the opposing player (Aeonglass.cs AfterAddedToRoom)."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.apply_power(WITHERING_PRESENCE_POWER_ID, 6)
        combat.hand = [make_strike_ironclad() for _ in range(6)]
        combat.energy = 20

        for _ in range(6):
            assert combat.play_card(0, 0)

        withers = [c for c in combat.hand if c.card_id == CardId.WITHER]
        assert len(withers) == 1
        assert enemy.get_power_amount(WITHERING_PRESENCE_POWER_ID) == 6

    def test_five_cards_do_not_trigger(self):
        """CardsLeft only triggers at <= 0 (WitheringPresencePower.cs line 52)."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.apply_power(WITHERING_PRESENCE_POWER_ID, 6)
        combat.hand = [make_strike_ironclad() for _ in range(5)]
        combat.energy = 20

        for _ in range(5):
            assert combat.play_card(0, 0)

        assert not any(c.card_id == CardId.WITHER for c in combat.hand)
        assert enemy.get_power_amount(WITHERING_PRESENCE_POWER_ID) == 1

    def test_counter_restarts_after_trigger(self):
        """After the trigger the count restarts from 6
        (WitheringPresencePower.cs line 57: `CardsLeft.BaseValue = 6m`)."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.apply_power(WITHERING_PRESENCE_POWER_ID, 6)
        combat.hand = [make_strike_ironclad() for _ in range(12)]
        combat.energy = 40

        for _ in range(12):
            # Keep the beetle alive: 12 strikes would otherwise kill it and end
            # the combat mid-loop.
            enemy.current_hp = enemy.max_hp
            # Skip the unplayable Wither once it lands in hand.
            index = next(
                i for i, c in enumerate(combat.hand) if c.card_id != CardId.WITHER
            )
            assert combat.play_card(index, 0)

        withers = [c for c in combat.hand if c.card_id == CardId.WITHER]
        assert len(withers) == 2

    def test_non_opposing_side_cards_do_not_count(self):
        """C# counts only `cardPlay.Card.Owner == base.Target.Player` — cards
        played by the power holder's own side (Aeonglass itself / its minions)
        never decrement CardsLeft."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        power = WitheringPresencePower(6)
        enemy.powers[WITHERING_PRESENCE_POWER_ID] = power

        # A card "played" by the holder's own side: no decrement.
        enemy_card = make_strike_ironclad()
        enemy_card.owner = enemy
        power.after_card_played(enemy, enemy_card, combat)
        assert power.amount == 6

    def test_generated_wither_matches_csharp_card(self):
        """Wither.cs: cost -1 (X? no: `base(-1, ...)`), CardType.Status,
        Unplayable, MaxUpgradeLevel 0, DamageVar(3, Unpowered | Move)."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.apply_power(WITHERING_PRESENCE_POWER_ID, 6)
        combat.hand = [make_strike_ironclad() for _ in range(6)]
        combat.energy = 20

        for _ in range(6):
            assert combat.play_card(0, 0)

        wither = next(c for c in combat.hand if c.card_id == CardId.WITHER)
        assert wither.cost == -1
        assert wither.card_type is CardType.STATUS
        assert wither.is_unplayable
        assert wither.effect_vars.get("damage") == 3

    def test_power_id_enum_gap_is_documented(self):
        """PowerId.WITHERING_PRESENCE landed in enums.py; the fallback constant
        now resolves to the real enum member and the registry lookup follows
        it. Its applier (Aeonglass.AfterAddedToRoom) lives with the
        unimplemented monster layer."""
        assert "WITHERING_PRESENCE" in PowerId.__members__
        assert WITHERING_PRESENCE_POWER_ID is PowerId.WITHERING_PRESENCE
        assert get_power_class(WITHERING_PRESENCE_POWER_ID) is WitheringPresencePower


@pytest.mark.parametrize(
    "factory_id",
    [
        PowerId.NO_ENERGY_GAIN,
        PowerId.BORROWED_TIME,
        PowerId.TAINTED,
        PowerId.WITHERING_PRESENCE,
    ],
)
def test_all_four_powers_are_registered(factory_id):
    """All four v0.107.1 powers resolve through get_power_class, i.e.
    Creature.apply_power can instantiate them."""
    assert get_power_class(factory_id) is not None
