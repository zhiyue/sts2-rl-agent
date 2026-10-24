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
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
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
from sts2_env.monsters.act2 import create_infested_prism
from sts2_env.powers.card_play_effects import (
    BORROWED_TIME_POWER_ID,
    BorrowedTimePower,
    NoEnergyGainPower,
)
from sts2_env.powers.damage_modifiers import TAINTED_POWER_ID, TaintedPower
from sts2_env.powers.monster import WITHERING_PRESENCE_POWER_ID, WitheringPresencePower
from sts2_env.powers.remaining_c import VitalSparkPower
from sts2_env.run.run_state import PlayerState
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
# VitalSparkPower
# (decompiled/MegaCrit.Sts2.Core.Models.Powers/VitalSparkPower.cs)
# ---------------------------------------------------------------------------
# Applied by InfestedPrism.AfterAddedToRoom (InfestedPrism.cs:56-60) and
# PulsateMove (InfestedPrism.cs:114); carries the Tainted affliction
# (Models.Afflictions/Tainted.cs) on player Skill cards via CardCmd.Afflict
# (Commands/CardCmd.cs:625-659).


def _make_prism_combat(deck):
    """Combat where the enemy holds VitalSpark(2) BEFORE start_combat, the C#
    order: InfestedPrism.AfterAddedToRoom applies the power, then CombatState
    StartCombat runs BeforeCombatStart hooks (VitalSparkPower.cs:25-39)."""
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=list(deck),
        rng_seed=42,
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    creature.apply_power(PowerId.VITAL_SPARK, 2)
    combat.start_combat()
    return combat


def _player_pile_cards(combat):
    """All unique card instances the primary player owns in combat piles."""
    state = combat.combat_player_state_for(combat.player)
    return list(combat.unique_cards_in_piles(state.all_piles))


class TestVitalSparkPowerParity:
    def test_power_type_and_stack(self):
        """VitalSparkPower.cs lines 17-19: PowerType.Buff,
        PowerStackType.Counter (PulsateMove re-applies stack on top)."""
        from sts2_env.core.enums import PowerStackType, PowerType

        assert VitalSparkPower.power_type is PowerType.BUFF
        assert VitalSparkPower.stack_type is PowerStackType.COUNTER
        assert get_power_class(PowerId.VITAL_SPARK) is VitalSparkPower

    def test_before_combat_start_afflicts_every_player_skill(self):
        """VitalSparkPower.cs BeforeCombatStart (lines 25-39): for each player
        ally, `CardCmd.Afflict<Tainted>(card, Amount)` for every card with
        `Type == CardType.Skill`; non-Skills are never afflicted."""
        deck = [make_strike_ironclad() for _ in range(3)] + [
            create_card(CardId.BORROWED_TIME) for _ in range(2)
        ]
        combat = _make_prism_combat(deck)

        all_cards = _player_pile_cards(combat)
        strikes = [c for c in all_cards if c.is_attack]
        skills = [c for c in all_cards if c.is_skill]
        assert strikes and len(skills) == 2
        assert all(c.affliction == "tainted" for c in skills)
        assert all(c.affliction_amount == 2 for c in skills)
        assert not any(c.affliction == "tainted" for c in strikes)

    def test_before_combat_start_covers_all_player_allies(self):
        """VitalSparkPower.cs lines 27-32: the loop runs over
        `Owner.CombatState.Allies` filtered to IsPlayer — every player's cards,
        not just the primary player's."""
        deck = [create_card(CardId.BORROWED_TIME)]
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=list(deck),
            rng_seed=42,
        )
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70)
        )
        ally_skills = [create_card(CardId.BORROWED_TIME) for _ in range(2)]
        combat.combat_player_state_for(ally).starting_deck.extend(ally_skills)
        creature, ai = create_shrinker_beetle(Rng(42))
        combat.add_enemy(creature, ai)
        creature.apply_power(PowerId.VITAL_SPARK, 2)
        combat.start_combat()

        ally_state = combat.combat_player_state_for(ally)
        primary_state = combat.combat_player_state_for(combat.player)
        # Opening draw moves most cards to hand; check every pile.
        ally_cards = list(combat.unique_cards_in_piles(ally_state.all_piles))
        primary_cards = list(combat.unique_cards_in_piles(primary_state.all_piles))
        assert ally_cards and primary_cards
        assert all(c.affliction == "tainted" for c in ally_cards)
        assert all(c.affliction == "tainted" for c in primary_cards)

    def test_afflict_restacks_amount_on_already_tainted_cards(self):
        """CardCmd.cs Afflict (lines 645-658) with Tainted.IsStackable=true
        (Tainted.cs:13): `card.Affliction.Amount += amount` — re-afflicting an
        already Tainted card stacks instead of failing."""
        combat = _make_prism_combat([create_card(CardId.BORROWED_TIME)])
        skill = next(c for c in _player_pile_cards(combat) if c.is_skill)
        assert skill.affliction_amount == 2

        # A second Afflict<Tainted> from any source stacks.
        assert skill.afflict("tainted", stackable=True, amount=2)
        assert skill.affliction_amount == 4

    def test_cards_with_other_afflictions_are_skipped(self):
        """CardCmd.cs lines 641-643 + 652-654: CanAfflict fails for a card
        already holding a different affliction, so Afflict<Tainted> is a
        no-op there."""
        skill = create_card(CardId.BORROWED_TIME)
        skill.afflict("ringing")

        VitalSparkPower(2)._afflict_card(skill)

        assert skill.affliction == "ringing"

    def test_after_card_entered_combat_afflicts_new_skills_only(self):
        """VitalSparkPower.cs AfterCardEnteredCombat (lines 41-47):
        `if (card.Affliction == null && card.Type == CardType.Skill)` —
        newly entering Skills get Tainted(Amount); attacks and pre-afflicted
        cards are left alone."""
        combat = _make_prism_combat([make_strike_ironclad()])

        new_skill = create_card(CardId.BORROWED_TIME)
        combat.add_generated_card_to_creature_hand(combat.player, new_skill)
        assert new_skill.affliction == "tainted"
        assert new_skill.affliction_amount == 2

        pre_afflicted = create_card(CardId.BORROWED_TIME)
        pre_afflicted.afflict("tainted", stackable=True, amount=1)
        combat.add_generated_card_to_creature_hand(combat.player, pre_afflicted)
        assert pre_afflicted.affliction_amount == 1  # not re-stamped

        new_attack = make_strike_ironclad()
        combat.add_generated_card_to_creature_hand(combat.player, new_attack)
        assert new_attack.affliction is None

    def test_after_card_played_applies_tainted_power_to_card_owner(self):
        """VitalSparkPower.cs AfterCardPlayed (lines 49-56):
        `if (cardPlay.Card.Affliction is Tainted)` ->
        `PowerCmd.Apply<TaintedPower>(cardPlay.Card.Owner.Creature, Amount)` —
        the card's OWNER gets the debuff (the player self-debuffs)."""
        combat = _make_prism_combat([])
        skill = create_card(CardId.BORROWED_TIME)
        skill.afflict("tainted", stackable=True, amount=2)
        combat.hand = [skill]
        combat.energy = 5

        assert combat.play_card(0)
        assert combat.player.get_power_amount(TAINTED_POWER_ID) == 2

    def test_after_card_played_ignores_unafflicted_cards(self):
        """VitalSparkPower.cs line 51: only `Affliction is Tainted` cards
        trigger the TaintedPower application."""
        combat = _make_prism_combat([])
        skill = create_card(CardId.BORROWED_TIME)  # no affliction
        combat.hand = [skill]
        combat.energy = 5

        assert combat.play_card(0)
        assert not combat.player.has_power(TAINTED_POWER_ID)

    def test_each_afflicted_play_stacks_one_counter(self):
        """PowerStackType.Counter (VitalSparkPower.cs line 19 /
        TaintedPower.cs line 16): two afflicted Skills played in one turn ->
        TaintedPower stacks to 4 (2 + 2) and hits for +4."""
        combat = _make_prism_combat([])
        skill_a = create_card(CardId.BORROWED_TIME)
        skill_a.afflict("tainted", stackable=True, amount=2)
        skill_b = create_card(CardId.BORROWED_TIME)
        skill_b.afflict("tainted", stackable=True, amount=2)
        combat.hand = [skill_a, skill_b]
        combat.energy = 20

        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.player.get_power_amount(TAINTED_POWER_ID) == 4

        hp_before = combat.player.current_hp
        combat.deal_damage(combat.enemies[0], combat.player, 10, ValueProp.MOVE)
        assert combat.player.current_hp == hp_before - 14

    def test_artifact_blocks_the_played_card_debuff(self):
        """TaintedPower is a Debuff (TaintedPower.cs line 14), so C#
        PowerCmd.Apply is subject to Artifact; the affliction on the deck
        itself is untouched. Uses Defend (a Skill with no OnPlay debuff of
        its own, unlike BorrowedTime) so Artifact is free for the Tainted
        application."""
        combat = _make_prism_combat([])
        combat.player.apply_power(PowerId.ARTIFACT, 1)
        skill = make_defend_ironclad()
        skill.afflict("tainted", stackable=True, amount=2)
        combat.hand = [skill]
        combat.energy = 5

        assert combat.play_card(0)
        assert not combat.player.has_power(TAINTED_POWER_ID)
        assert not combat.player.has_power(PowerId.ARTIFACT)

    def test_tainted_power_chain_removed_at_enemy_side_turn_end(self):
        """Full chain timing: playing an afflicted Skill applies TaintedPower
        (VitalSparkPower.cs:49-56), which survives the player side turn end
        and self-removes when the ENEMY side turn ends
        (TaintedPower.cs AfterSideTurnEnd, lines 31-38)."""
        combat = _make_prism_combat([])
        skill = create_card(CardId.BORROWED_TIME)
        skill.afflict("tainted", stackable=True, amount=2)
        combat.hand = [skill]
        combat.energy = 5

        assert combat.play_card(0)
        assert combat.player.has_power(TAINTED_POWER_ID)

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.has_power(TAINTED_POWER_ID)

        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert not combat.player.has_power(TAINTED_POWER_ID)

    def test_after_removed_clears_tainted_from_player_cards(self):
        """VitalSparkPower.cs AfterRemoved (lines 58-77): when the power is
        removed, `CardCmd.ClearAffliction` runs for every player-ally card
        whose `Affliction is Tainted`; other afflictions survive."""
        ringing_card = create_card(CardId.BORROWED_TIME)
        ringing_card.afflict("ringing")  # stamped before combat: VitalSpark skips it
        combat = _make_prism_combat([ringing_card, create_card(CardId.BORROWED_TIME)])
        prism = combat.enemies[0]
        assert ringing_card.affliction == "ringing"
        tainted_card = next(
            c for c in _player_pile_cards(combat) if c.affliction == "tainted"
        )

        combat._remove_power(prism, PowerId.VITAL_SPARK)

        assert tainted_card.affliction is None
        assert ringing_card.affliction == "ringing"

    def test_after_power_amount_changed_resyncs_card_affliction_amounts(self):
        """VitalSparkPower.cs AfterPowerAmountChanged (lines 79-98): any change
        of the power's Amount sets every existing Tainted card affliction's
        Amount to the power's new total (PulsateMove re-apply path)."""
        combat = _make_prism_combat([create_card(CardId.BORROWED_TIME)])
        skill = next(c for c in _player_pile_cards(combat) if c.is_skill)
        assert skill.affliction_amount == 2

        prism = combat.enemies[0]
        prism.apply_power(PowerId.VITAL_SPARK, 3)  # stacks to 5, resyncs cards

        assert prism.get_power_amount(PowerId.VITAL_SPARK) == 5
        assert skill.affliction_amount == 5

    def test_infested_prism_applies_vital_spark_2_deadly_3(self):
        """InfestedPrism.cs line 40 (`VitalSparkAmount => GetValueIfAscension(
        DeadlyEnemies, 3, 2)`) and line 59 (AfterAddedToRoom applies it on room
        entry). The PulsateMove re-apply (line 114) is covered by
        test_monster_ai_state_machine_parity."""
        prism, _prism_ai = create_infested_prism(Rng(1))
        assert prism.get_power_amount(PowerId.VITAL_SPARK) == 2

        deadly_prism, _deadly_ai = create_infested_prism(Rng(2), ascension_level=9)
        assert deadly_prism.get_power_amount(PowerId.VITAL_SPARK) == 3


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
