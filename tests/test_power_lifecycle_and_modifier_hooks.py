"""Tests for power mechanics: ticking, persistence, damage/block modification."""

import pytest

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CombatSide, PowerId, RoomType, ValueProp
from sts2_env.core.damage import apply_damage, calculate_block, calculate_damage
from sts2_env.core.combat import CombatState
from sts2_env.core.hooks import (
    fire_after_block_cleared,
    fire_after_block_gained,
    fire_after_player_turn_start,
    fire_after_side_turn_start,
    fire_after_turn_end,
    fire_before_combat_start,
    fire_before_turn_end,
)
from sts2_env.cards.defect import create_defect_starter_deck, make_beam_cell, make_feral, make_genetic_algorithm, make_subroutine
from sts2_env.cards.ironclad import (
    create_ironclad_starter_deck,
    make_demon_form,
    make_anger,
    make_inflame,
    make_juggling,
    make_sword_boomerang,
)
from sts2_env.cards.ironclad_basic import make_bash, make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.silent import _make_shiv, make_afterimage, make_deflect, make_serpent_form
from sts2_env.cards.status import make_burn, make_dazed, make_rebound, make_sovereign_blade
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.monsters.state_machine import MonsterAI, MoveState
from sts2_env.monsters.intents import attack_intent
from sts2_env.monsters.act3 import create_turret_operator
from sts2_env.powers.base import PowerInstance
from sts2_env.powers.monster import BurrowedPower, CrabRagePower, SkittishPower, SmoggyPower
from sts2_env.powers.remaining_b import RampartPower
from sts2_env.powers.remaining_c import SandpitPower, ToricToughnessPower
from sts2_env.relics.registry import create_relic_by_name
from sts2_env.run.reward_objects import GoldReward, RemoveCardReward
from sts2_env.run.rooms import CombatRoom
from sts2_env.run.run_state import PlayerState, RunState


TEMPORARY_STRENGTH_AMOUNT = 3
TEMPORARY_DEXTERITY_AMOUNT = 4
TEMPORARY_FOCUS_AMOUNT = 2
STARTING_ACT_INDEX = 0
MULTIPLAYER_ALLY_CHARACTER_ID = "Ironclad"
MULTIPLAYER_TEST_ENEMY_HP = 20
MULTIPLAYER_ALLY_HP = 70
ACT_THREE_INDEX = 2
CARD_DRAW_HOOK_PROBE_DRAW_COUNT = 1


def _noop_move(combat: CombatState) -> None:
    pass


def _noop_monster_ai() -> MonsterAI:
    return MonsterAI(
        {"NOOP": MoveState("NOOP", _noop_move, [attack_intent(1)], follow_up_id="NOOP")},
        "NOOP",
    )


def _add_multiplayer_ally(combat: CombatState, player_id: int = 2) -> Creature:
    return combat.add_ally_player(
        PlayerState(
            player_id=player_id,
            character_id=MULTIPLAYER_ALLY_CHARACTER_ID,
            max_hp=MULTIPLAYER_ALLY_HP,
            current_hp=MULTIPLAYER_ALLY_HP,
        )
    )


def _expected_multiplayer_enemy_hp(combat: CombatState, base_hp: int) -> int:
    from sts2_env.core.constants import MULTIPLAYER_ACT_3_BOSS_SCALING, MULTIPLAYER_ACT_SCALING

    run_state = getattr(combat.current_player_state.player_state, "run_state", None)
    act_index = getattr(run_state, "current_act_index", STARTING_ACT_INDEX)
    room_type = getattr(getattr(combat, "room", None), "room_type", None)
    if act_index == ACT_THREE_INDEX and room_type == RoomType.BOSS:
        scaling = MULTIPLAYER_ACT_3_BOSS_SCALING
    else:
        scaling = MULTIPLAYER_ACT_SCALING[act_index]
    return int(base_hp * len(combat.combat_player_states) * scaling)


class _FirstChoiceRng:
    def choice(self, values):
        return list(values)[0]


class _BlockHookCounterPower(PowerInstance):
    def __init__(self, observed: Creature | None = None):
        super().__init__(PowerId.JUGGERNAUT, 0)
        self.observed = observed
        self.calls: list[int] = []

    def after_block_gained(self, owner, creature, amount, combat):
        if creature is (self.observed or owner):
            self.calls.append(amount)


class _CardDrawHookProbePower(PowerInstance):
    def __init__(self):
        super().__init__(PowerId.SPEEDSTER, 1)
        self.events: list[tuple[Creature, object, bool, CombatState]] = []

    def on_card_drawn(
        self,
        owner: Creature,
        card: object,
        from_hand_draw: bool,
        combat: CombatState,
    ) -> None:
        self.events.append((owner, card, from_hand_draw, combat))


class _TurnEndHpProbePower(PowerInstance):
    def __init__(self):
        super().__init__(PowerId.ACCURACY, 1)
        self.hp_seen: int | None = None

    def after_turn_end(self, owner, side, combat):
        if side == owner.side:
            self.hp_seen = owner.current_hp


class TestPowerApplication:
    """Stacking, negative amounts, Artifact blocking."""

    def test_strength_stacks(self, player):
        player.apply_power(PowerId.STRENGTH, 2)
        player.apply_power(PowerId.STRENGTH, 3)
        assert player.get_power_amount(PowerId.STRENGTH) == 5

    def test_vulnerable_stacks(self, enemy):
        enemy.apply_power(PowerId.VULNERABLE, 2)
        enemy.apply_power(PowerId.VULNERABLE, 3)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 5

    def test_strength_allows_negative(self, player):
        player.apply_power(PowerId.STRENGTH, 3)
        player.apply_power(PowerId.STRENGTH, -5)
        assert player.get_power_amount(PowerId.STRENGTH) == -2

    def test_artifact_blocks_debuff(self, player):
        player.apply_power(PowerId.ARTIFACT, 1)
        player.apply_power(PowerId.VULNERABLE, 3)
        assert not player.has_power(PowerId.VULNERABLE)
        assert not player.has_power(PowerId.ARTIFACT)  # consumed

    def test_artifact_blocks_multiple(self, player):
        player.apply_power(PowerId.ARTIFACT, 2)
        player.apply_power(PowerId.VULNERABLE, 1)
        player.apply_power(PowerId.WEAK, 1)
        assert not player.has_power(PowerId.VULNERABLE)
        assert not player.has_power(PowerId.WEAK)
        assert not player.has_power(PowerId.ARTIFACT)  # 2 consumed

    def test_artifact_blocks_negative_strength(self, player):
        player.apply_power(PowerId.ARTIFACT, 1)
        player.apply_power(PowerId.STRENGTH, -2)

        assert not player.has_power(PowerId.STRENGTH)
        assert not player.has_power(PowerId.ARTIFACT)

    def test_multiplayer_scaling_applies_to_primary_enemy_power_amount(self, simple_combat):
        simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]

        simple_combat.apply_power_to(enemy, PowerId.SKITTISH, 6, applier=enemy)

        assert enemy.get_power_amount(PowerId.SKITTISH) == 13

    def test_multiplayer_scaling_uses_charge_rule_for_artifact_like_powers(self, simple_combat):
        simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        simple_combat.add_ally_player(PlayerState(player_id=3, character_id="Silent", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]

        simple_combat.apply_power_to(enemy, PowerId.ARTIFACT, 1, applier=enemy)

        assert enemy.get_power_amount(PowerId.ARTIFACT) == 5

    def test_multiplayer_scaling_applies_to_secondary_enemy_targets_but_not_players(self, simple_combat):
        simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        secondary_enemy = Creature(max_hp=20, current_hp=20, side=CombatSide.ENEMY, monster_id="MINION")
        simple_combat.add_enemy(secondary_enemy, simple_combat.enemy_ais[simple_combat.enemies[0].combat_id])
        simple_combat.apply_power_to(secondary_enemy, PowerId.MINION, 1, applier=secondary_enemy)

        simple_combat.apply_power_to(simple_combat.player, PowerId.REGEN, 5, applier=simple_combat.player)
        simple_combat.apply_power_to(secondary_enemy, PowerId.SKITTISH, 6, applier=secondary_enemy)

        assert simple_combat.player.get_power_amount(PowerId.REGEN) == 5
        assert secondary_enemy.get_power_amount(PowerId.SKITTISH) == 13

    def test_multiplayer_scaling_normalizes_enemy_powers_added_before_combat(self, simple_combat):
        simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = Creature(max_hp=20, current_hp=20, side=CombatSide.ENEMY, monster_id="PREPOWERED")
        enemy.apply_power(PowerId.SKITTISH, 6, applier=enemy)

        simple_combat.add_enemy(enemy, simple_combat.enemy_ais[simple_combat.enemies[0].combat_id])

        assert enemy.get_power_amount(PowerId.SKITTISH) == 13

    def test_multiplayer_scaling_normalizes_enemy_hp_on_add(self, simple_combat):
        _add_multiplayer_ally(simple_combat)
        enemy = Creature(
            max_hp=MULTIPLAYER_TEST_ENEMY_HP,
            current_hp=MULTIPLAYER_TEST_ENEMY_HP,
            side=CombatSide.ENEMY,
            monster_id="HP_SCALED",
        )

        simple_combat.add_enemy(enemy, _noop_monster_ai())

        expected_hp = _expected_multiplayer_enemy_hp(simple_combat, MULTIPLAYER_TEST_ENEMY_HP)
        assert enemy.max_hp == expected_hp
        assert enemy.current_hp == expected_hp

    def test_multiplayer_scaling_uses_act_three_boss_hp_multiplier(self):
        run_state = RunState(seed=304, character_id="Ironclad")
        run_state.current_act_index = ACT_THREE_INDEX
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=42,
            player_state=run_state.player,
            room=CombatRoom(room_type=RoomType.BOSS),
        )
        _add_multiplayer_ally(combat)
        enemy = Creature(
            max_hp=MULTIPLAYER_TEST_ENEMY_HP,
            current_hp=MULTIPLAYER_TEST_ENEMY_HP,
            side=CombatSide.ENEMY,
            monster_id="BOSS_HP_SCALED",
        )

        combat.add_enemy(enemy, _noop_monster_ai())

        expected_hp = _expected_multiplayer_enemy_hp(combat, MULTIPLAYER_TEST_ENEMY_HP)
        assert enemy.max_hp == expected_hp
        assert enemy.current_hp == expected_hp

    def test_multiplayer_scaling_does_not_change_single_player_enemy_hp(self, simple_combat):
        enemy = Creature(
            max_hp=MULTIPLAYER_TEST_ENEMY_HP,
            current_hp=MULTIPLAYER_TEST_ENEMY_HP,
            side=CombatSide.ENEMY,
            monster_id="SOLO_HP",
        )

        simple_combat.add_enemy(enemy, _noop_monster_ai())

        assert enemy.max_hp == MULTIPLAYER_TEST_ENEMY_HP
        assert enemy.current_hp == MULTIPLAYER_TEST_ENEMY_HP

    def test_artifact_does_not_block_positive_strength(self, player):
        player.apply_power(PowerId.ARTIFACT, 1)
        player.apply_power(PowerId.STRENGTH, 2)

        assert player.get_power_amount(PowerId.STRENGTH) == 2
        assert player.get_power_amount(PowerId.ARTIFACT) == 1

    def test_call_of_the_void_uses_combat_card_pool(self):
        run_state = RunState(seed=304, character_id="Ironclad")
        run_state.player.deck = create_ironclad_starter_deck()
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=run_state.player.deck,
            rng_seed=304,
            character_id="Ironclad",
            player_state=run_state.player,
        )
        creature, ai = create_shrinker_beetle(Rng(304))
        combat.add_enemy(creature, ai)
        combat.player.apply_power(PowerId.CALL_OF_THE_VOID, 1)

        combat.start_combat()

        assert combat.hand[0].card_id != CardId.FEED

    def test_curl_up_triggers_after_multi_hit_card_finishes(self, simple_combat):
        enemy = simple_combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 39
        simple_combat.apply_power_to(enemy, PowerId.CURL_UP, 12)
        simple_combat.hand = [make_sword_boomerang()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0)
        assert enemy.current_hp == 30
        assert enemy.block == 12

    def test_curl_up_block_triggers_after_block_gained_hooks(self, simple_combat):
        enemy = simple_combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 39
        simple_combat.apply_power_to(enemy, PowerId.CURL_UP, 12)
        calls: list[int] = []

        class BlockProbePower(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.JUGGERNAUT, 0)

            def after_block_gained(self, owner, creature, amount, combat):
                if creature is owner:
                    calls.append(amount)

        enemy.powers[PowerId.JUGGERNAUT] = BlockProbePower()
        simple_combat.hand = [make_sword_boomerang()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0)

        assert enemy.block == 12
        assert calls == [12]

    def test_curl_up_does_not_trigger_when_hit_kills_owner(self, simple_combat):
        enemy = simple_combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 6
        simple_combat.apply_power_to(enemy, PowerId.CURL_UP, 12)
        simple_combat.hand = [make_strike_ironclad()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0, 0)
        assert enemy.is_dead
        assert enemy.block == 0

    def test_sloth_limits_owner_card_plays_and_resets_on_player_turn_start(self, simple_combat):
        simple_combat.hand = [make_strike_ironclad() for _ in range(4)]
        simple_combat.energy = 10
        simple_combat.apply_power_to(simple_combat.player, PowerId.SLOTH, 3)

        assert simple_combat.play_card(0, 0)
        assert simple_combat.play_card(0, 0)
        assert simple_combat.play_card(0, 0)
        assert simple_combat.can_play_card(simple_combat.hand[0]) is False
        assert simple_combat.play_card(0, 0) is False

        sloth = simple_combat.player.powers[PowerId.SLOTH]
        sloth.before_side_turn_start(simple_combat.player, CombatSide.PLAYER, simple_combat)

        assert simple_combat.can_play_card(simple_combat.hand[0]) is True

    def test_ringing_allows_only_one_owner_card_until_turn_end(self, simple_combat):
        simple_combat.hand = [make_strike_ironclad() for _ in range(2)]
        simple_combat.energy = 10
        simple_combat.apply_power_to(simple_combat.player, PowerId.RINGING, 1)

        assert simple_combat.play_card(0, 0)
        assert simple_combat.can_play_card(simple_combat.hand[0]) is False
        assert simple_combat.play_card(0, 0) is False

        ringing = simple_combat.player.powers[PowerId.RINGING]
        ringing.after_turn_end(simple_combat.player, CombatSide.PLAYER, simple_combat)

        assert simple_combat.can_play_card(simple_combat.hand[0]) is True

    def test_entropy_requests_hand_transform_with_combat_selection_rng(self, simple_combat):
        card = make_strike_ironclad()
        simple_combat.hand = [card]
        simple_combat.rng = _FirstChoiceRng()
        simple_combat.apply_power_to(simple_combat.player, PowerId.ENTROPY, 1)

        simple_combat.player.powers[PowerId.ENTROPY].after_player_turn_start(simple_combat.player, simple_combat)

        assert simple_combat.pending_choice is None
        assert simple_combat.hand[0] is not card
        assert simple_combat.hand[0].card_id != CardId.STRIKE_IRONCLAD

    def test_ringing_counts_cards_played_before_it_was_applied(self, simple_combat):
        simple_combat.hand = [make_strike_ironclad() for _ in range(2)]
        simple_combat.energy = 10

        assert simple_combat.play_card(0, 0)

        simple_combat.apply_power_to(simple_combat.player, PowerId.RINGING, 1)

        assert simple_combat.can_play_card(simple_combat.hand[0]) is False
        assert simple_combat.play_card(0, 0) is False

    def test_ringing_counts_card_play_starts_before_after_card_played(self, simple_combat):
        first = make_anger()
        second = make_strike_ironclad()
        second.afflict("ringing")
        simple_combat.hand = [first, second]
        simple_combat.energy = 1

        assert simple_combat.play_card(0, 0)

        simple_combat.apply_power_to(simple_combat.player, PowerId.RINGING, 1)

        assert simple_combat.can_play_card(second) is False

    def test_ringing_only_blocks_cards_it_afflicted(self, simple_combat):
        ringed = make_strike_ironclad()
        already_afflicted = make_strike_ironclad()
        already_afflicted.afflict("hexed")
        simple_combat.hand = [ringed, already_afflicted]
        simple_combat.energy = 10

        simple_combat.apply_power_to(simple_combat.player, PowerId.RINGING, 1)

        assert ringed.has_affliction("ringing")
        assert already_afflicted.has_affliction("hexed")
        assert simple_combat.play_card(0, 0)
        assert simple_combat.can_play_card(simple_combat.hand[0]) is True

    def test_tangled_increases_attack_energy_cost_for_playability_and_spend(self, simple_combat):
        simple_combat.hand = [make_strike_ironclad()]
        simple_combat.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.TANGLED, 1)

        assert simple_combat.modified_card_cost(simple_combat.player, simple_combat.hand[0]) == 2
        assert simple_combat.can_play_card(simple_combat.hand[0]) is False

        simple_combat.energy = 2

        assert simple_combat.play_card(0, 0)
        assert simple_combat.energy == 0

    def test_tangled_only_increases_cards_it_afflicted(self, simple_combat):
        already_afflicted = make_strike_ironclad()
        already_afflicted.afflict("hexed")
        simple_combat.hand = [already_afflicted]
        simple_combat.energy = 1

        simple_combat.apply_power_to(simple_combat.player, PowerId.TANGLED, 1)

        assert already_afflicted.has_affliction("hexed")
        assert simple_combat.modified_card_cost(simple_combat.player, already_afflicted) == 1
        assert simple_combat.can_play_card(already_afflicted) is True

    def test_tangled_stack_does_not_afflict_cards_skipped_by_initial_application(self, simple_combat):
        """Matches TangledPower.cs: only AfterApplied scans existing Attack cards."""
        skipped_attack = make_strike_ironclad()
        skipped_attack.afflict("hexed")
        simple_combat.hand = [skipped_attack]

        simple_combat.apply_power_to(simple_combat.player, PowerId.TANGLED, 1)
        skipped_attack.clear_affliction("hexed")
        simple_combat.apply_power_to(simple_combat.player, PowerId.TANGLED, 1)

        assert skipped_attack.affliction is None
        assert simple_combat.modified_card_cost(simple_combat.player, skipped_attack) == skipped_attack.cost

    def test_corruption_makes_owner_skills_free_and_exhaust_after_play(self, simple_combat):
        defend = make_defend_ironclad()
        simple_combat.hand = [defend]
        simple_combat.energy = 0
        simple_combat.apply_power_to(simple_combat.player, PowerId.CORRUPTION, 1)

        assert simple_combat.modified_card_cost(simple_combat.player, defend) == 0
        assert simple_combat.play_card(0)
        assert defend in simple_combat.exhaust_pile
        assert defend not in simple_combat.discard_pile

    def test_curious_power_reduces_owner_power_card_cost_only(self, simple_combat):
        curious_amount = 2
        ally_max_hp = 70
        ally = simple_combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=ally_max_hp, current_hp=ally_max_hp)
        )
        owner_power = make_inflame()
        ally_power = make_demon_form()
        attack = make_strike_ironclad()
        owner_power.owner = simple_combat.player
        ally_power.owner = ally
        attack.owner = simple_combat.player

        simple_combat.apply_power_to(simple_combat.player, PowerId.CURIOUS, curious_amount)

        assert simple_combat.modified_card_cost(simple_combat.player, owner_power) == 0
        assert simple_combat.modified_card_cost(ally, ally_power) == 3
        assert simple_combat.modified_card_cost(simple_combat.player, attack) == 1

    def test_iteration_uses_turn_draw_history_for_first_status(self, simple_combat):
        iteration_amount = 2
        first_status = make_dazed()
        second_status = make_burn()
        next_turn_status = make_dazed()
        first_bonus = make_strike_ironclad()
        second_bonus = make_defend_ironclad()
        next_card = make_bash()
        simple_combat.hand.clear()
        simple_combat.draw_pile = [first_status, second_status, next_turn_status, first_bonus, second_bonus, next_card]

        simple_combat.draw_cards(simple_combat.player, 1)
        simple_combat.apply_power_to(simple_combat.player, PowerId.ITERATION, iteration_amount)
        simple_combat.draw_cards(simple_combat.player, 1)

        assert simple_combat.hand == [first_status, second_status]
        assert simple_combat.draw_pile == [next_turn_status, first_bonus, second_bonus, next_card]

        simple_combat._start_player_turn()  # noqa: SLF001

        assert simple_combat.hand == [first_status, second_status, next_turn_status, first_bonus, second_bonus, next_card]

    def test_card_draw_power_hook_receives_from_hand_draw(self, simple_combat):
        probe = _CardDrawHookProbePower()
        first_card = make_strike_ironclad()
        second_card = make_defend_ironclad()
        simple_combat.player.powers[probe.power_id] = probe
        simple_combat.hand.clear()
        simple_combat.draw_pile = [first_card, second_card]

        simple_combat.draw_cards(simple_combat.player, CARD_DRAW_HOOK_PROBE_DRAW_COUNT)
        simple_combat._draw_cards(CARD_DRAW_HOOK_PROBE_DRAW_COUNT, from_hand_draw=True)  # noqa: SLF001

        assert probe.events == [
            (simple_combat.player, first_card, False, simple_combat),
            (simple_combat.player, second_card, True, simple_combat),
        ]


class TestDebuffTicking:
    """Vulnerable/Weak/Frail tick down at enemy turn end only; Strength does not tick."""

    def test_vulnerable_ticks_down(self, enemy):
        enemy.apply_power(PowerId.VULNERABLE, 3)
        enemy.tick_down_power(PowerId.VULNERABLE)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

    def test_vulnerable_removed_at_zero(self, enemy):
        enemy.apply_power(PowerId.VULNERABLE, 1)
        enemy.tick_down_power(PowerId.VULNERABLE)
        assert not enemy.has_power(PowerId.VULNERABLE)

    def test_skip_first_tick(self, enemy):
        """Debuffs applied during player turn skip first tick."""
        enemy.apply_power(PowerId.VULNERABLE, 2)
        p = enemy.powers[PowerId.VULNERABLE]
        p.skip_next_tick = True
        enemy.tick_down_power(PowerId.VULNERABLE)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2  # skipped
        enemy.tick_down_power(PowerId.VULNERABLE)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 1  # now ticked

    def test_weak_ticks_down(self, player):
        player.apply_power(PowerId.WEAK, 2)
        player.tick_down_power(PowerId.WEAK)
        assert player.get_power_amount(PowerId.WEAK) == 1

    def test_frail_ticks_down(self, player):
        player.apply_power(PowerId.FRAIL, 2)
        player.tick_down_power(PowerId.FRAIL)
        assert player.get_power_amount(PowerId.FRAIL) == 1

    def test_strength_does_not_tick(self, player):
        """Strength is permanent -- tick_down_power is a no-op."""
        player.apply_power(PowerId.STRENGTH, 3)
        player.tick_down_power(PowerId.STRENGTH)
        assert player.get_power_amount(PowerId.STRENGTH) == 3


class TestDebuffTickTiming:
    """Debuffs tick at the end of the ENEMY turn, not the player turn.

    When a debuff is applied via combat.apply_power_to() during the player
    turn, skip_next_tick is set, so the first enemy-turn-end tick is skipped.
    """

    def test_debuffs_tick_after_enemy_turn(self, simple_combat):
        """Vulnerable applied to ENEMY during player turn.

        Per C# PowerCmd.Apply: skip_next_tick is only set when
        target.Side == CombatSide.Player. Since the target is an enemy,
        skip is NOT set, so the debuff ticks normally.
        """
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(enemy, PowerId.VULNERABLE, 2)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

        # End turn 1: enemy is not player-side, no skip -> ticks 2->1
        simple_combat.end_player_turn()
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 1

        # End turn 2: ticks 1->0 (removed)
        simple_combat.end_player_turn()
        assert not enemy.has_power(PowerId.VULNERABLE)

    def test_vulnerable_is_removed_when_duration_reaches_zero(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy.apply_power(PowerId.VULNERABLE, 1)

        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 15

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert PowerId.VULNERABLE not in enemy.powers
        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 10

    def test_strength_persists_across_turns(self, simple_combat):
        """Strength never ticks down across multiple turns."""
        simple_combat.player.apply_power(PowerId.STRENGTH, 5)
        assert simple_combat.player.get_power_amount(PowerId.STRENGTH) == 5
        simple_combat.end_player_turn()
        assert simple_combat.player.get_power_amount(PowerId.STRENGTH) == 5
        simple_combat.end_player_turn()
        assert simple_combat.player.get_power_amount(PowerId.STRENGTH) == 5

    def test_debuff_on_player_skips_first_tick(self, simple_combat):
        """Debuffs applied TO the player skip first tick (C# checks target.Side == Player)."""
        player = simple_combat.player
        simple_combat.apply_power_to(player, PowerId.VULNERABLE, 2)
        assert player.get_power_amount(PowerId.VULNERABLE) == 2
        # End turn 1: player is player-side, skip IS set -> stays at 2
        simple_combat.end_player_turn()
        assert player.get_power_amount(PowerId.VULNERABLE) == 2
        # End turn 2: now ticks 2->1
        simple_combat.end_player_turn()
        assert player.get_power_amount(PowerId.VULNERABLE) == 1


class TestDamageModifierInteractions:
    LETHALITY_PERCENT = 50
    PERCENT_SCALE = 100

    def test_lethality_boosts_only_first_attack_started_this_turn(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        first_attack = make_strike_ironclad()
        second_attack = make_strike_ironclad()
        first_attack_damage = int(first_attack.base_damage * (1 + self.LETHALITY_PERCENT / self.PERCENT_SCALE))
        second_attack_damage = second_attack.base_damage
        simple_combat.hand = [first_attack, second_attack]
        simple_combat.energy = 2
        simple_combat.apply_power_to(simple_combat.player, PowerId.LETHALITY, self.LETHALITY_PERCENT)

        assert simple_combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - first_attack_damage

        assert simple_combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - first_attack_damage - second_attack_damage

    def test_lethality_only_boosts_first_play_in_replayed_attack_series(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        strike = make_strike_ironclad()
        first_hit_damage = int(strike.base_damage * (1 + self.LETHALITY_PERCENT / self.PERCENT_SCALE))
        replay_hit_damage = strike.base_damage
        simple_combat.hand = [strike]
        simple_combat.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.LETHALITY, self.LETHALITY_PERCENT)
        simple_combat.apply_power_to(simple_combat.player, PowerId.ECHO_FORM, 1)

        assert simple_combat.play_card(0, 0)

        assert enemy.current_hp == starting_hp - first_hit_damage - replay_hit_damage

    def test_cruelty_increases_vulnerable_multiplier(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.CRUELTY, 25)
        enemy.apply_power(PowerId.VULNERABLE, 1)

        damage = calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat)
        assert damage == 17

    def test_debilitate_modifies_weak_and_vulnerable_excess(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]

        enemy.apply_power(PowerId.DEBILITATE, 1)
        enemy.apply_power(PowerId.VULNERABLE, 1)
        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 20

        enemy.powers.pop(PowerId.DEBILITATE, None)
        enemy.powers.pop(PowerId.VULNERABLE, None)
        player.apply_power(PowerId.DEBILITATE, 1)
        player.apply_power(PowerId.WEAK, 1)
        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 5

    def test_vulnerable_debilitate_paper_phrog_folds_phrog_in_before_doubling(self, simple_combat):
        """C# VulnerablePower.cs:35-46 calls PaperPhrog.ModifyVulnerableMultiplier
        from inside the power's own multiplicative hook, so Phrog's +0.25 patches
        the base 1.5 BEFORE Debilitate doubles the excess: (1.5 + 0.25) widened
        = 2.5x.  The previous Python ratio-compensation listener applied Phrog
        after Debilitate and produced 2.25x."""
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.current_player_state.relics.append(create_relic_by_name("PaperPhrog"))

        enemy.apply_power(PowerId.DEBILITATE, 1)
        enemy.apply_power(PowerId.VULNERABLE, 1)

        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 25

    def test_weak_debilitate_paper_krane_folds_krane_in_before_widening(self, simple_combat):
        """C# WeakPower.cs:33-40 calls PaperKrane.ModifyWeakMultiplier from
        inside the power's own multiplicative hook, so Krane's -0.15 patches the
        base 0.75 BEFORE Debilitate widens the shortfall: (0.75 - 0.15) widened
        = 0.20x.  The previous Python ratio-compensation listener applied Krane
        after Debilitate and produced 0.35x."""
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.current_player_state.relics.append(create_relic_by_name("PaperKrane"))

        enemy.apply_power(PowerId.DEBILITATE, 1)
        enemy.apply_power(PowerId.WEAK, 1)

        assert calculate_damage(10, enemy, player, ValueProp.MOVE, simple_combat) == 2

    def test_debilitate_decrements_on_owner_turn_end_without_skip(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(player, PowerId.DEBILITATE, 2, applier=enemy)

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert player.get_power_amount(PowerId.DEBILITATE) == 2

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)

        assert player.get_power_amount(PowerId.DEBILITATE) == 1

    def test_disintegration_deals_damage_after_normal_turn_end_hooks(self, simple_combat):
        player = simple_combat.player
        probe = _TurnEndHpProbePower()
        player.powers[PowerId.ACCURACY] = probe
        simple_combat.apply_power_to(player, PowerId.DISINTEGRATION, 7)

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)

        assert probe.hp_seen == 80
        assert player.current_hp == 73

    def test_shrink_reduces_owner_powered_attack_damage_by_thirty_percent_and_ticks(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(player, PowerId.SHRINK, 1, applier=enemy)

        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 7
        assert calculate_damage(10, enemy, player, ValueProp.MOVE, simple_combat) == 10

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)

        assert PowerId.SHRINK not in player.powers
        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 10

    def test_infinite_shrink_does_not_tick_down(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(player, PowerId.SHRINK, -1, applier=enemy)

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)

        assert player.get_power_amount(PowerId.SHRINK) == -1

    def test_flanking_is_removed_at_target_turn_end(self, simple_combat):
        player = simple_combat.player
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(enemy, PowerId.FLANKING, 2, applier=player)

        assert calculate_damage(10, ally, enemy, ValueProp.MOVE, simple_combat) == 20

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert PowerId.FLANKING not in enemy.powers
        assert calculate_damage(10, ally, enemy, ValueProp.MOVE, simple_combat) == 10

    def test_flanking_keeps_separate_appliers_like_reference(self, simple_combat):
        player = simple_combat.player
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(enemy, PowerId.FLANKING, 2, applier=player)
        simple_combat.apply_power_to(enemy, PowerId.FLANKING, 3, applier=ally)

        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 30
        assert calculate_damage(10, ally, enemy, ValueProp.MOVE, simple_combat) == 20

    def test_knockdown_is_removed_at_target_turn_end(self, simple_combat):
        player = simple_combat.player
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(enemy, PowerId.KNOCKDOWN, 2, applier=player)

        assert calculate_damage(10, ally, enemy, ValueProp.MOVE, simple_combat) == 20

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert PowerId.KNOCKDOWN not in enemy.powers
        assert calculate_damage(10, ally, enemy, ValueProp.MOVE, simple_combat) == 10

    def test_knockdown_keeps_separate_appliers_like_reference(self, simple_combat):
        player = simple_combat.player
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(enemy, PowerId.KNOCKDOWN, 2, applier=player)
        simple_combat.apply_power_to(enemy, PowerId.KNOCKDOWN, 3, applier=ally)

        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 30
        assert calculate_damage(10, ally, enemy, ValueProp.MOVE, simple_combat) == 20

    def test_guarded_keeps_separate_appliers_like_reference(self, simple_combat):
        player = simple_combat.player
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(player, PowerId.GUARDED, 1, applier=ally)
        simple_combat.apply_power_to(player, PowerId.GUARDED, 1, applier=enemy)

        assert calculate_damage(20, enemy, player, ValueProp.MOVE, simple_combat) == 5

        simple_combat.kill_creature(enemy)

        assert player.has_power(PowerId.GUARDED)
        assert calculate_damage(20, ally, player, ValueProp.MOVE, simple_combat) == 10

    def test_diamond_diadem_is_removed_at_enemy_turn_end(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(player, PowerId.DIAMOND_DIADEM, 1)

        assert calculate_damage(10, enemy, player, ValueProp.MOVE, simple_combat) == 5

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert PowerId.DIAMOND_DIADEM not in player.powers
        assert calculate_damage(10, enemy, player, ValueProp.MOVE, simple_combat) == 10

    def test_leadership_power_adds_damage_to_allied_powered_attacks_only(self, simple_combat):
        base_damage = 10
        leadership_amount = 3
        ally_max_hp = 70
        player = simple_combat.player
        ally = simple_combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=ally_max_hp, current_hp=ally_max_hp)
        )
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(player, PowerId.LEADERSHIP, leadership_amount)

        assert calculate_damage(base_damage, ally, enemy, ValueProp.MOVE, simple_combat) == base_damage + leadership_amount
        assert calculate_damage(base_damage, player, enemy, ValueProp.MOVE, simple_combat) == base_damage
        assert calculate_damage(base_damage, ally, enemy, ValueProp.UNPOWERED, simple_combat) == base_damage

    def test_back_attack_right_power_marks_surrounded_back_damage_side(self, simple_combat):
        base_damage = 10
        back_attack_multiplier = 1.5
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(player, PowerId.SURROUNDED, 1)
        simple_combat.apply_power_to(enemy, PowerId.BACK_ATTACK_RIGHT, 1)
        surrounded = player.powers[PowerId.SURROUNDED]
        surrounded.facing = surrounded.FACING_LEFT

        assert calculate_damage(base_damage, enemy, player, ValueProp.MOVE, simple_combat) == int(
            base_damage * back_attack_multiplier
        )

        surrounded.facing = surrounded.FACING_RIGHT

        assert calculate_damage(base_damage, enemy, player, ValueProp.MOVE, simple_combat) == base_damage

    def test_imbalanced_triggers_only_when_damage_was_fully_blocked(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy_ai = simple_combat.enemy_ais[enemy.combat_id]
        starting_move_id = enemy_ai.current_move.state_id
        stunned_move_id = "STUNNED"
        enemy.apply_power(PowerId.IMBALANCED, 1)

        simple_combat.deal_damage(enemy, player, 0, ValueProp.MOVE)
        assert enemy_ai.current_move.state_id == starting_move_id

        player.block = 5
        simple_combat.deal_damage(enemy, player, 3, ValueProp.MOVE)
        assert enemy_ai.current_move.state_id == stunned_move_id

    def test_slippery_decrements_when_damage_is_blocked(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy.apply_power(PowerId.SLIPPERY, 2)
        enemy.block = 5

        simple_combat.deal_damage(player, enemy, 3, ValueProp.MOVE)

        assert enemy.get_power_amount(PowerId.SLIPPERY) == 1

    def test_vital_spark_triggers_on_non_fully_blocked_zero_damage(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy.apply_power(PowerId.VITAL_SPARK, 1)
        simple_combat.energy = 0

        simple_combat.deal_damage(player, enemy, 0, ValueProp.MOVE)

        assert simple_combat.energy == 1

    def test_vital_spark_does_not_trigger_on_fully_blocked_damage(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy.apply_power(PowerId.VITAL_SPARK, 1)
        enemy.block = 5
        simple_combat.energy = 0

        simple_combat.deal_damage(player, enemy, 3, ValueProp.MOVE)

        assert simple_combat.energy == 0

    def test_vital_spark_triggers_once_per_player_each_turn(self, simple_combat):
        player = simple_combat.player
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        enemy = simple_combat.enemies[0]
        enemy.apply_power(PowerId.VITAL_SPARK, 1)
        simple_combat.energy = 0
        ally_state.energy = 0

        simple_combat.deal_damage(player, enemy, 0, ValueProp.MOVE)
        simple_combat.deal_damage(player, enemy, 0, ValueProp.MOVE)
        simple_combat.deal_damage(ally, enemy, 0, ValueProp.MOVE)

        assert simple_combat.energy == 1
        assert ally_state.energy == 1

    def test_vital_spark_grants_energy_to_osty_owner(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy.apply_power(PowerId.VITAL_SPARK, 1)
        osty = simple_combat.summon_osty(player, 5)
        assert osty is not None
        simple_combat.energy = 0

        simple_combat.deal_damage(osty, enemy, 0, ValueProp.MOVE)

        assert simple_combat.energy == 1

    def test_personal_hive_adds_dazed_to_osty_owner_draw_pile(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy.apply_power(PowerId.PERSONAL_HIVE, 2)
        osty = simple_combat.summon_osty(player, 5)
        assert osty is not None

        simple_combat.deal_damage(osty, enemy, 1, ValueProp.MOVE)

        dazed_count = sum(card.card_id == CardId.DAZED for card in simple_combat.draw_pile)
        assert dazed_count == 2

    def test_personal_hive_does_not_assign_event_pet_dazed_to_owner(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        personal_hive_amount = 2
        enemy.apply_power(PowerId.PERSONAL_HIVE, personal_hive_amount)
        pet = simple_combat.summon_event_pet(player, "PAELS_LEGION")
        assert pet is not None

        simple_combat.deal_damage(pet, enemy, 1, ValueProp.MOVE)

        owner_dazed_count = sum(
            card.card_id == CardId.DAZED and card.owner is player
            for card in simple_combat.draw_pile
        )
        pet_generated_count = sum(
            1
            for owner, _ in simple_combat._generated_cards_combat  # noqa: SLF001
            if owner is pet
        )
        assert owner_dazed_count == 0
        assert pet_generated_count == personal_hive_amount


class TestMinionVictoryAndCascade:
    """C# CombatManager.cs:192: the fight is won once no *primary* enemy is
    alive — living secondary (Minion) enemies do not keep it going.
    CreatureCmd.cs:546-551: when a primary enemy dies and every living
    teammate is secondary, the whole team is cascade-killed with it."""

    def _add_minion(self, combat: CombatState) -> Creature:
        minion = Creature(max_hp=10, current_hp=10, side=CombatSide.ENEMY, monster_id="TEST_MINION")
        minion.apply_power(PowerId.MINION, 1)
        combat.add_enemy(minion, _noop_monster_ai())
        return minion

    def test_killing_last_primary_enemy_wins_and_cascades_minion(self, simple_combat):
        primary = simple_combat.enemies[0]
        primary.max_hp = 50
        primary.current_hp = 50
        minion = self._add_minion(simple_combat)

        assert primary.is_primary_enemy
        assert minion.is_secondary_enemy
        assert not simple_combat.is_over

        simple_combat.kill_creature(primary)

        assert simple_combat.is_over
        assert simple_combat.player_won
        # The minion cannot outlive its primary: it was cascade-killed.
        assert not minion.is_alive

    def test_living_minion_alone_does_not_block_victory(self, simple_combat):
        """Direct check of the victory predicate (CombatManager.cs:192): a
        living Minion secondary does not count towards the enemy-alive check."""
        primary = simple_combat.enemies[0]
        minion = self._add_minion(simple_combat)
        primary.current_hp = 0

        assert not primary.is_alive
        assert minion.is_alive

        simple_combat._check_combat_end()  # noqa: SLF001

        assert simple_combat.is_over
        assert simple_combat.player_won

    def test_living_primary_enemy_still_blocks_victory(self, simple_combat):
        """Control: a living primary enemy keeps the fight going even when a
        minion teammate is also alive."""
        primary = simple_combat.enemies[0]
        minion = self._add_minion(simple_combat)
        minion.current_hp = 1

        simple_combat._check_combat_end()  # noqa: SLF001

        assert not simple_combat.is_over


class TestSandpitPower:
    def test_sandpit_kills_target_player_when_count_reaches_zero(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        sandpit = SandpitPower(1)
        sandpit.set_target(player)
        enemy.powers[PowerId.SANDPIT] = sandpit

        sandpit.after_side_turn_start(enemy, CombatSide.ENEMY, simple_combat)

        assert player.is_dead
        assert PowerId.SANDPIT not in enemy.powers


class TestBurrowedPower:
    def test_burrowed_clears_block_when_removed(self, simple_combat):
        enemy = simple_combat.enemies[0]
        enemy.block = 12
        enemy.powers[PowerId.BURROWED] = BurrowedPower()

        simple_combat._remove_power(enemy, PowerId.BURROWED)

        assert enemy.block == 0
        assert PowerId.BURROWED not in enemy.powers

    def test_burrowed_removes_itself_when_block_is_broken(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy.block = 5
        enemy.powers[PowerId.BURROWED] = BurrowedPower()

        apply_damage(enemy, 5, ValueProp.MOVE, simple_combat, player)

        assert enemy.block == 0
        assert PowerId.BURROWED not in enemy.powers


class TestDieForYouPower:
    def test_osty_takes_unblocked_attack_damage_for_owner(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.summon_osty(player, 3)
        osty = simple_combat.get_osty(player)
        assert osty is not None
        player.block = 2

        apply_damage(player, 5, ValueProp.MOVE, simple_combat, enemy)

        assert player.current_hp == player.max_hp
        assert osty.is_dead
        assert not osty.escaped

    def test_osty_overkill_damage_spills_back_to_owner(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.summon_osty(player, 2)
        osty = simple_combat.get_osty(player)
        assert osty is not None
        player.block = 1

        apply_damage(player, 5, ValueProp.MOVE, simple_combat, enemy)

        assert osty.is_dead
        assert player.current_hp == player.max_hp - 2

    def test_osty_does_not_take_unpowered_damage_for_owner(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        osty_hp = 5
        unpowered_damage = 3
        simple_combat.summon_osty(player, osty_hp)
        osty = simple_combat.get_osty(player)
        assert osty is not None

        apply_damage(player, unpowered_damage, ValueProp.MOVE | ValueProp.UNPOWERED, simple_combat, enemy)

        assert player.current_hp == player.max_hp - unpowered_damage
        assert osty.current_hp == osty.max_hp

    def test_hardened_shell_caps_damage_before_osty_redirect(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.summon_osty(player, 10)
        osty = simple_combat.get_osty(player)
        assert osty is not None
        player.apply_power(PowerId.HARDENED_SHELL, 3)

        apply_damage(player, 8, ValueProp.MOVE, simple_combat, enemy)

        assert osty.current_hp == 7
        assert player.current_hp == player.max_hp

    def test_revived_osty_can_die_again(self, simple_combat):
        player = simple_combat.player
        simple_combat.summon_osty(player, 2)
        osty = simple_combat.get_osty(player)
        assert osty is not None

        assert simple_combat.kill_osty(player)
        simple_combat.summon_osty(player, 4)

        assert osty.is_alive
        assert not osty.escaped
        assert not osty._death_processed
        assert simple_combat.kill_osty(player)


class TestSkittishPower:
    def test_skittish_does_not_trigger_from_non_card_damage(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        enemy.powers[PowerId.SKITTISH] = SkittishPower(6)

        simple_combat.deal_damage(player, enemy, 3, ValueProp.MOVE)

        assert enemy.block == 0

    def test_skittish_triggers_after_card_attack_damage(self, simple_combat):
        enemy = simple_combat.enemies[0]
        simple_combat.hand = [make_strike_ironclad()]
        simple_combat.energy = 1
        enemy.powers[PowerId.SKITTISH] = SkittishPower(6)

        assert simple_combat.play_card(0, 0)

        assert enemy.block == 6

    def test_skittish_block_triggers_after_block_gained_hooks(self, simple_combat):
        enemy = simple_combat.enemies[0]
        counter = _BlockHookCounterPower()
        enemy.powers[PowerId.JUGGERNAUT] = counter
        enemy.powers[PowerId.SKITTISH] = SkittishPower(6)
        simple_combat.hand = [make_strike_ironclad()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0, 0)

        assert enemy.block == 6
        assert counter.calls == [6]


class TestPowerAmountChangedHooks:
    def test_temporary_strength_power_applies_strength_and_expires_on_owner_turn_end(self, simple_combat):
        player = simple_combat.player

        simple_combat.apply_power_to(player, PowerId.TEMPORARY_STRENGTH, TEMPORARY_STRENGTH_AMOUNT)

        assert player.get_power_amount(PowerId.TEMPORARY_STRENGTH) == TEMPORARY_STRENGTH_AMOUNT
        assert player.get_power_amount(PowerId.STRENGTH) == TEMPORARY_STRENGTH_AMOUNT

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)
        assert player.get_power_amount(PowerId.TEMPORARY_STRENGTH) == TEMPORARY_STRENGTH_AMOUNT
        assert player.get_power_amount(PowerId.STRENGTH) == TEMPORARY_STRENGTH_AMOUNT

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)
        assert player.get_power_amount(PowerId.TEMPORARY_STRENGTH) == 0
        assert player.get_power_amount(PowerId.STRENGTH) == 0

    def test_temporary_dexterity_power_applies_dexterity_and_expires_on_owner_turn_end(self, simple_combat):
        player = simple_combat.player

        simple_combat.apply_power_to(player, PowerId.TEMPORARY_DEXTERITY, TEMPORARY_DEXTERITY_AMOUNT)

        assert player.get_power_amount(PowerId.TEMPORARY_DEXTERITY) == TEMPORARY_DEXTERITY_AMOUNT
        assert player.get_power_amount(PowerId.DEXTERITY) == TEMPORARY_DEXTERITY_AMOUNT

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)
        assert player.get_power_amount(PowerId.TEMPORARY_DEXTERITY) == TEMPORARY_DEXTERITY_AMOUNT
        assert player.get_power_amount(PowerId.DEXTERITY) == TEMPORARY_DEXTERITY_AMOUNT

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)
        assert player.get_power_amount(PowerId.TEMPORARY_DEXTERITY) == 0
        assert player.get_power_amount(PowerId.DEXTERITY) == 0

    def test_temporary_focus_power_applies_focus_and_expires_on_owner_turn_end(self, simple_combat):
        player = simple_combat.player

        simple_combat.apply_power_to(player, PowerId.TEMPORARY_FOCUS, TEMPORARY_FOCUS_AMOUNT)

        assert player.get_power_amount(PowerId.TEMPORARY_FOCUS) == TEMPORARY_FOCUS_AMOUNT
        assert player.get_power_amount(PowerId.FOCUS) == TEMPORARY_FOCUS_AMOUNT

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)
        assert player.get_power_amount(PowerId.TEMPORARY_FOCUS) == TEMPORARY_FOCUS_AMOUNT
        assert player.get_power_amount(PowerId.FOCUS) == TEMPORARY_FOCUS_AMOUNT

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)
        assert player.get_power_amount(PowerId.TEMPORARY_FOCUS) == 0
        assert player.get_power_amount(PowerId.FOCUS) == 0

    def test_shroud_gains_block_when_owner_applies_doom(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.SHROUD, 3)

        simple_combat.apply_power_to(enemy, PowerId.DOOM, 2)

        assert player.block == 3

    def test_shroud_block_triggers_after_block_gained_hooks(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        counter = _BlockHookCounterPower()
        player.powers[PowerId.JUGGERNAUT] = counter
        player.apply_power(PowerId.SHROUD, 3)

        simple_combat.apply_power_to(enemy, PowerId.DOOM, 2)

        assert player.block == 3
        assert counter.calls == [3]

    def test_vicious_draws_when_owner_applies_vulnerable(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.VICIOUS, 2)
        simple_combat.hand.clear()

        simple_combat.apply_power_to(enemy, PowerId.VULNERABLE, 1)

        assert len(simple_combat.hand) == 2

    def test_outbreak_triggers_every_third_poison_application(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.OUTBREAK, 4)
        starting_hp = enemy.current_hp

        simple_combat.apply_power_to(enemy, PowerId.POISON, 1)
        simple_combat.apply_power_to(enemy, PowerId.POISON, 1)
        assert enemy.current_hp == starting_hp

        simple_combat.apply_power_to(enemy, PowerId.POISON, 1)
        assert enemy.current_hp == starting_hp - 4

    def test_monarchs_gaze_strength_down_is_temporary(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.MONARCHS_GAZE, 2)
        player.apply_power(PowerId.SLEIGHT_OF_FLESH, 5)
        starting_hp = enemy.current_hp

        simple_combat.deal_damage(
            dealer=player,
            target=enemy,
            amount=4,
            props=ValueProp.MOVE,
        )

        assert enemy.current_hp == starting_hp - 9
        assert enemy.get_power_amount(PowerId.MONARCHS_GAZE_STRENGTH_DOWN) == 2
        assert enemy.get_power_amount(PowerId.STRENGTH) == -2

        power = enemy.powers[PowerId.MONARCHS_GAZE_STRENGTH_DOWN]
        power.after_turn_end(enemy, CombatSide.ENEMY, simple_combat)
        assert enemy.get_power_amount(PowerId.STRENGTH) == 0

    def test_sleight_of_flesh_triggers_for_duration_and_negative_strength_debuffs(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.SLEIGHT_OF_FLESH, 5)
        starting_hp = enemy.current_hp

        simple_combat.apply_power_to(enemy, PowerId.WEAK, 1, applier=player)
        assert enemy.current_hp == starting_hp - 5

        simple_combat.apply_power_to(enemy, PowerId.STRENGTH, -1, applier=player)
        assert enemy.current_hp == starting_hp - 10

    def test_sleight_of_flesh_ignores_temporary_power_but_sees_internal_strength_down(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.SLEIGHT_OF_FLESH, 5)
        starting_hp = enemy.current_hp

        simple_combat.apply_power_to(enemy, PowerId.MANGLE, 3, applier=player)

        assert enemy.get_power_amount(PowerId.MANGLE) == 3
        assert enemy.get_power_amount(PowerId.STRENGTH) == -3
        assert enemy.current_hp == starting_hp - 5

    def test_enemy_ritual_skips_first_turn_end_after_application(self, simple_combat):
        enemy = simple_combat.enemies[0]

        enemy.apply_power(PowerId.RITUAL, 3)
        fire_after_turn_end(CombatSide.ENEMY, simple_combat)
        assert enemy.get_power_amount(PowerId.STRENGTH) == 0

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)
        assert enemy.get_power_amount(PowerId.STRENGTH) == 3

    def test_enemy_ritual_does_not_skip_again_when_existing_ritual_stacks(self, simple_combat):
        enemy = simple_combat.enemies[0]

        enemy.apply_power(PowerId.RITUAL, 3)
        fire_after_turn_end(CombatSide.ENEMY, simple_combat)
        enemy.apply_power(PowerId.RITUAL, 2)

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert enemy.get_power_amount(PowerId.RITUAL) == 5
        assert enemy.get_power_amount(PowerId.STRENGTH) == 5

    def test_tank_applies_guarded_to_other_player_teammates(self, simple_combat):
        ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70)
        ally = simple_combat.add_ally_player(ally_state)

        simple_combat.apply_power_to(simple_combat.player, PowerId.TANK, 1)

        assert ally.get_power_amount(PowerId.GUARDED) == 1
        assert simple_combat.player.get_power_amount(PowerId.GUARDED) == 0

    def test_tank_does_not_reapply_guarded_when_existing_tank_stacks(self, simple_combat):
        ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70)
        ally = simple_combat.add_ally_player(ally_state)

        simple_combat.apply_power_to(simple_combat.player, PowerId.TANK, 1)
        simple_combat.apply_power_to(simple_combat.player, PowerId.TANK, 1)

        assert simple_combat.player.get_power_amount(PowerId.TANK) == 2
        assert ally.get_power_amount(PowerId.GUARDED) == 1
        assert getattr(ally.powers[PowerId.GUARDED], "_appliers") == [simple_combat.player]

    def test_grapple_keeps_separate_appliers_like_reference(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.apply_power_to(enemy, PowerId.GRAPPLE, 5, applier=simple_combat.player)
        simple_combat.apply_power_to(enemy, PowerId.GRAPPLE, 7, applier=ally)

        simple_combat.player.gain_block(4)
        fire_after_block_gained(simple_combat.player, 4, simple_combat)
        assert enemy.current_hp == starting_hp - 5

        ally.gain_block(4)
        fire_after_block_gained(ally, 4, simple_combat)
        assert enemy.current_hp == starting_hp - 12

    def test_rolling_boulder_keeps_separate_instances_like_reference(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.apply_power_to(simple_combat.player, PowerId.ROLLING_BOULDER, 5)
        simple_combat.apply_power_to(simple_combat.player, PowerId.ROLLING_BOULDER, 10)

        fire_after_player_turn_start(simple_combat.player, simple_combat)
        assert enemy.current_hp == starting_hp - 15

        fire_after_player_turn_start(simple_combat.player, simple_combat)
        assert enemy.current_hp == starting_hp - 40

    def test_enemy_plating_decrements_by_player_count(self, simple_combat):
        enemy = simple_combat.enemies[0]
        simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy.apply_power(PowerId.PLATING, 6)

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert enemy.get_power_amount(PowerId.PLATING) == 4

    def test_plating_block_triggers_after_block_gained_hooks(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        start_hp = enemy.current_hp
        player.apply_power(PowerId.PLATING, 4)
        player.apply_power(PowerId.JUGGERNAUT, 5)
        player.block = 0

        fire_before_turn_end(CombatSide.PLAYER, simple_combat)

        assert player.block == 4
        assert enemy.current_hp == start_hp - 5

    def test_beacon_of_hope_shared_block_triggers_after_block_gained_hooks(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        counter = _BlockHookCounterPower()
        ally.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.player.apply_power(PowerId.BEACON_OF_HOPE, 1)
        simple_combat.hand = [make_defend_ironclad()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0)

        assert simple_combat.player.block == 5
        assert ally.block == 2
        assert counter.calls == [2]

    def test_child_of_the_stars_block_triggers_after_block_gained_hooks(self, simple_combat):
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.player.apply_power(PowerId.CHILD_OF_THE_STARS, 2)
        simple_combat.gain_stars(simple_combat.player, 3)

        simple_combat.spend_stars(simple_combat.player, 2)

        assert simple_combat.player.block == 4
        assert counter.calls == [4]

    def test_coolant_counts_orb_types_and_triggers_after_block_gained_hooks(self):
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
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "FROST")
        combat.channel_orb(combat.player, "FROST")
        counter = _BlockHookCounterPower()
        combat.player.powers[PowerId.JUGGERNAUT] = counter
        combat.player.apply_power(PowerId.COOLANT, 2)

        fire_after_side_turn_start(CombatSide.PLAYER, combat)

        assert combat.player.block == 4
        assert counter.calls == [4]

    def test_crimson_mantle_block_triggers_after_block_gained_hooks(self, simple_combat):
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.player.apply_power(PowerId.CRIMSON_MANTLE, 6)

        fire_after_player_turn_start(simple_combat.player, simple_combat)

        assert simple_combat.player.block == 6
        assert counter.calls == [6]

    def test_danse_macabre_block_triggers_after_block_gained_hooks(self, simple_combat):
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.player.apply_power(PowerId.DANSE_MACABRE, 3)
        simple_combat.hand = [make_bash()]
        simple_combat.energy = 2

        assert simple_combat.play_card(0, 0)

        assert simple_combat.player.block == 3
        assert counter.calls == [3]

    def test_parry_block_triggers_after_block_gained_hooks(self, simple_combat):
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.player.apply_power(PowerId.PARRY, 4)
        simple_combat.hand = [make_sovereign_blade()]
        simple_combat.energy = 2

        assert simple_combat.play_card(0, 0)

        assert simple_combat.player.block == 4
        assert counter.calls == [4]

    def test_pillar_of_creation_block_triggers_after_block_gained_hooks(self, simple_combat):
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.player.apply_power(PowerId.PILLAR_OF_CREATION, 3)

        simple_combat.add_generated_card_to_creature_hand(simple_combat.player, make_rebound())

        assert simple_combat.player.block == 3
        assert counter.calls == [3]

    def test_rampart_targets_turret_operator_and_triggers_after_block_gained_hooks(self, simple_combat):
        shield = simple_combat.enemies[0]
        turret, turret_ai = create_turret_operator(Rng(43))
        simple_combat.add_enemy(turret, turret_ai)
        counter = _BlockHookCounterPower(observed=turret)
        shield.powers[PowerId.JUGGERNAUT] = counter
        shield.powers[PowerId.RAMPART] = RampartPower(25)

        fire_after_side_turn_start(CombatSide.PLAYER, simple_combat)

        assert shield.block == 0
        assert turret.block == 25
        assert counter.calls == [25]

    def test_sneaky_block_triggers_after_block_gained_hooks(self, simple_combat):
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.player.apply_power(PowerId.SNEAKY, 2)
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        ally_state.hand = [make_strike_ironclad()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1

        assert simple_combat.play_card_from_creature(ally, 0, 0)

        assert simple_combat.player.block == 2
        assert counter.calls == [2]

    def test_crab_rage_block_triggers_after_block_gained_hooks(self, simple_combat):
        owner = simple_combat.enemies[0]
        ally, ally_ai = create_twig_slime_s(Rng(100))
        simple_combat.add_enemy(ally, ally_ai)
        counter = _BlockHookCounterPower()
        owner.powers[PowerId.JUGGERNAUT] = counter
        owner.powers[PowerId.CRAB_RAGE] = CrabRagePower()

        assert simple_combat.kill_creature(ally)

        assert owner.block == 99
        assert counter.calls == [99]

    def test_juggling_counts_attacks_played_before_it_was_applied(self, simple_combat):
        simple_combat.hand = [
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_juggling(),
            make_strike_ironclad(),
        ]
        simple_combat.energy = 10

        assert simple_combat.play_card(0, 0)
        assert simple_combat.play_card(0, 0)
        assert simple_combat.play_card(0)
        assert simple_combat.play_card(0, 0)

        assert len(simple_combat.hand) == 1
        assert simple_combat.hand[0].card_id == CardId.STRIKE_IRONCLAD

    def test_magic_bomb_without_live_applier_is_removed_without_damage(self, simple_combat):
        player = simple_combat.player
        player_start_hp = player.current_hp
        orphan_bomb_damage = 7

        player.apply_power(PowerId.MAGIC_BOMB, orphan_bomb_damage)
        fire_after_turn_end(CombatSide.PLAYER, simple_combat)

        assert player.current_hp == player_start_hp
        assert PowerId.MAGIC_BOMB not in player.powers

    def test_magic_bomb_keeps_multiple_applications_as_separate_instances(self, simple_combat):
        player = simple_combat.player
        applier = simple_combat.enemies[0]
        player_start_hp = player.current_hp
        first_bomb_damage = 3
        second_bomb_damage = 4

        simple_combat.apply_power_to(player, PowerId.MAGIC_BOMB, first_bomb_damage, applier=applier)
        simple_combat.apply_power_to(player, PowerId.MAGIC_BOMB, second_bomb_damage, applier=applier)
        fire_after_turn_end(CombatSide.PLAYER, simple_combat)

        assert player.current_hp == player_start_hp - first_bomb_damage - second_bomb_damage
        assert PowerId.MAGIC_BOMB not in player.powers
        damage_events = [
            event
            for event in simple_combat._damage_events_combat
            if event[0] is player and event[1] is player and event[2] == ValueProp.UNPOWERED
        ]
        assert [event[3] for event in damage_events] == [first_bomb_damage, second_bomb_damage]

    def test_feral_returns_limited_zero_cost_attacks_to_hand(self, simple_combat):
        simple_combat.hand = [make_feral(), make_beam_cell(), make_beam_cell()]
        simple_combat.energy = 10

        assert simple_combat.play_card(0)
        assert simple_combat.play_card(0, 0)
        assert len(simple_combat.hand) == 2

        assert simple_combat.play_card(0, 0)
        assert len(simple_combat.hand) == 1
        assert simple_combat.hand[0].card_id == CardId.BEAM_CELL

    def test_feral_counts_zero_cost_attacks_played_before_it_was_applied(self, simple_combat):
        simple_combat.hand = [make_beam_cell(), make_feral(), make_beam_cell()]
        simple_combat.energy = 10

        assert simple_combat.play_card(0, 0)
        assert simple_combat.play_card(0)
        assert simple_combat.play_card(0, 0)

        assert simple_combat.hand == []

    def test_rebound_moves_its_card_to_draw_top_after_play(self, simple_combat):
        rebound = make_rebound()
        simple_combat.hand = [rebound]
        simple_combat.draw_pile = []
        simple_combat.discard_pile = []
        simple_combat.energy = 1

        assert simple_combat.play_card(0, 0)

        assert simple_combat.draw_pile == [rebound]
        assert simple_combat.discard_pile == []
        assert PowerId.REBOUND not in simple_combat.player.powers

    def test_nostalgia_moves_first_qualifying_card_to_draw_top(self, simple_combat):
        first = make_strike_ironclad()
        second = make_defend_ironclad()
        simple_combat.hand = [first, second]
        simple_combat.draw_pile = []
        simple_combat.discard_pile = []
        simple_combat.energy = 2
        simple_combat.apply_power_to(simple_combat.player, PowerId.NOSTALGIA, 1)

        assert simple_combat.play_card(0, 0)
        assert simple_combat.draw_pile == [first]
        assert first not in simple_combat.discard_pile

        assert simple_combat.play_card(0)
        assert second in simple_combat.discard_pile
        assert simple_combat.draw_pile == [first]

    def test_nostalgia_counts_qualifying_cards_played_before_it_was_applied(self, simple_combat):
        first = make_strike_ironclad()
        second = make_defend_ironclad()
        simple_combat.hand = [first, second]
        simple_combat.draw_pile = []
        simple_combat.discard_pile = []
        simple_combat.energy = 2

        assert simple_combat.play_card(0, 0)

        simple_combat.apply_power_to(simple_combat.player, PowerId.NOSTALGIA, 1)

        assert simple_combat.play_card(0)
        assert second in simple_combat.discard_pile
        assert second not in simple_combat.draw_pile

    def test_nostalgia_moves_card_when_under_card_play_start_limit(self, simple_combat):
        strike = make_strike_ironclad()
        simple_combat.hand = [strike]
        simple_combat.draw_pile = []
        simple_combat.discard_pile = []
        simple_combat.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.NOSTALGIA, 2)

        assert simple_combat.play_card(0, 0)

        assert simple_combat.draw_pile == [strike]

    def test_block_next_turn_triggers_on_block_cleared_hook(self, simple_combat):
        simple_combat.player.apply_power(PowerId.BLOCK_NEXT_TURN, 6)

        fire_after_block_cleared(simple_combat.player, simple_combat)

        assert simple_combat.player.block == 6
        assert simple_combat.player.get_power_amount(PowerId.BLOCK_NEXT_TURN) == 0

    def test_block_next_turn_block_triggers_after_block_gained_hooks(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        start_hp = enemy.current_hp
        player.apply_power(PowerId.BLOCK_NEXT_TURN, 6)
        player.apply_power(PowerId.JUGGERNAUT, 5)

        fire_after_block_cleared(player, simple_combat)

        assert player.block == 6
        assert enemy.current_hp == start_hp - 5

    def test_regen_power_heals_on_owner_side_turn_end_and_decrements(self, simple_combat):
        damaged_hp = 70
        regen_amount = 4
        player = simple_combat.player
        player.current_hp = damaged_hp
        simple_combat.apply_power_to(player, PowerId.REGEN, regen_amount)

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert player.current_hp == damaged_hp
        assert player.get_power_amount(PowerId.REGEN) == regen_amount

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)

        assert player.current_hp == damaged_hp + regen_amount
        assert player.get_power_amount(PowerId.REGEN) == regen_amount - 1

    def test_territorial_power_gains_strength_on_owner_side_turn_end(self, simple_combat):
        territorial_amount = 2
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(enemy, PowerId.TERRITORIAL, territorial_amount)

        fire_after_turn_end(CombatSide.PLAYER, simple_combat)

        assert enemy.get_power_amount(PowerId.STRENGTH) == 0

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert enemy.get_power_amount(PowerId.STRENGTH) == territorial_amount

    def test_toric_toughness_triggers_on_block_cleared_hook(self, simple_combat):
        power = ToricToughnessPower(1)
        power.set_block(7)
        simple_combat.player.powers[PowerId.TORIC_TOUGHNESS] = power

        fire_after_block_cleared(simple_combat.player, simple_combat)

        assert simple_combat.player.block == 7
        assert PowerId.TORIC_TOUGHNESS not in simple_combat.player.powers

    def test_toric_toughness_block_triggers_after_block_gained_hooks(self, simple_combat):
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        power = ToricToughnessPower(1)
        power.set_block(7)
        simple_combat.player.powers[PowerId.TORIC_TOUGHNESS] = power

        fire_after_block_cleared(simple_combat.player, simple_combat)

        assert simple_combat.player.block == 7
        assert counter.calls == [7]

    def test_toric_toughness_keeps_separate_block_values_like_reference(self, simple_combat):
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.apply_power_to(simple_combat.player, PowerId.TORIC_TOUGHNESS, 2)
        power = simple_combat.player.powers[PowerId.TORIC_TOUGHNESS]
        assert isinstance(power, ToricToughnessPower)
        power.set_block(5)
        simple_combat.apply_power_to(simple_combat.player, PowerId.TORIC_TOUGHNESS, 1)
        power.set_block(7)

        fire_after_block_cleared(simple_combat.player, simple_combat)

        assert simple_combat.player.block == 12
        assert counter.calls == [5, 7]
        assert simple_combat.player.get_power_amount(PowerId.TORIC_TOUGHNESS) == 1

    def test_self_forming_clay_power_triggers_on_block_cleared_hook(self, simple_combat):
        simple_combat.player.apply_power(PowerId.SELF_FORMING_CLAY, 4)

        fire_after_block_cleared(simple_combat.player, simple_combat)

        assert simple_combat.player.block == 4
        assert PowerId.SELF_FORMING_CLAY not in simple_combat.player.powers

    def test_self_forming_clay_block_triggers_after_block_gained_hooks(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        start_hp = enemy.current_hp
        player.apply_power(PowerId.SELF_FORMING_CLAY, 4)
        player.apply_power(PowerId.JUGGERNAUT, 5)

        fire_after_block_cleared(player, simple_combat)

        assert player.block == 4
        assert enemy.current_hp == start_hp - 5

    def test_after_energy_reset_runs_before_hand_draw_modifiers(self, simple_combat):
        class DrawProbePower(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.ACCURACY, 1)
                self.energy_reset_seen = False

            def after_energy_reset(self, owner, combat):
                self.energy_reset_seen = True

            def modify_hand_draw(self, owner, draw):
                return draw + 1 if self.energy_reset_seen else draw

        probe = DrawProbePower()
        simple_combat.player.powers[PowerId.ACCURACY] = probe
        simple_combat.hand = []
        simple_combat.draw_pile = [make_strike_ironclad() for _ in range(6)]

        simple_combat._start_player_turn()

        assert probe.energy_reset_seen is True
        assert len(simple_combat.hand) == 6

    def test_energy_and_star_next_turn_trigger_once_during_energy_reset(self, simple_combat):
        simple_combat.hand = []
        simple_combat.draw_pile = []
        simple_combat.energy = 0
        simple_combat.stars = 0
        simple_combat.apply_power_to(simple_combat.player, PowerId.ENERGY_NEXT_TURN, 2)
        simple_combat.apply_power_to(simple_combat.player, PowerId.STAR_NEXT_TURN, 3)

        simple_combat._start_player_turn()

        assert simple_combat.energy == simple_combat.max_energy + 2
        assert simple_combat.stars == 3
        assert PowerId.ENERGY_NEXT_TURN not in simple_combat.player.powers
        assert PowerId.STAR_NEXT_TURN not in simple_combat.player.powers

    def test_genesis_gains_stars_once_during_energy_reset(self, simple_combat):
        simple_combat.hand = []
        simple_combat.draw_pile = []
        simple_combat.stars = 0
        simple_combat.apply_power_to(simple_combat.player, PowerId.GENESIS, 2)

        simple_combat._start_player_turn()

        assert simple_combat.stars == 2
        assert simple_combat.player.get_power_amount(PowerId.GENESIS) == 2

    def test_combat_end_runs_before_combat_victory(self, simple_combat):
        calls: list[str] = []

        class RecordingPower(PowerInstance):
            def after_combat_end(self, owner, combat):
                calls.append("end")

            def after_combat_victory(self, owner, combat):
                calls.append("victory")

        simple_combat.player.powers[PowerId.STRENGTH] = RecordingPower(PowerId.STRENGTH, 1)

        simple_combat._end_combat(player_won=True)

        assert calls == ["end", "victory"]

    def test_royalties_adds_gold_reward_after_combat_end(self, simple_combat):
        room = CombatRoom(room_type=RoomType.MONSTER)
        simple_combat.room = room
        simple_combat.apply_power_to(simple_combat.player, PowerId.ROYALTIES, 17)

        simple_combat._end_combat(player_won=True)

        rewards = room.extra_rewards[simple_combat.player_id]
        assert len(rewards) == 1
        assert isinstance(rewards[0], GoldReward)
        assert rewards[0].min_gold == 17
        assert rewards[0].max_gold == 17

    def test_forbidden_grimoire_adds_card_removal_rewards_after_combat_end(self, simple_combat):
        room = CombatRoom(room_type=RoomType.MONSTER)
        simple_combat.room = room
        simple_combat.apply_power_to(simple_combat.player, PowerId.FORBIDDEN_GRIMOIRE, 2)

        simple_combat._end_combat(player_won=True)

        rewards = room.extra_rewards[simple_combat.player_id]
        assert len(rewards) == 2
        assert all(isinstance(reward, RemoveCardReward) for reward in rewards)

    def test_improvement_upgrades_random_deck_cards_after_combat_end(self, simple_combat):
        first = make_bash()
        second = make_strike_ironclad()
        simple_combat.current_player_state.player_state.deck = [first, second]
        simple_combat.apply_power_to(simple_combat.player, PowerId.IMPROVEMENT, 1)

        simple_combat._end_combat(player_won=True)

        assert sum(card.upgraded for card in [first, second]) == 1

    def test_orbit_gains_energy_after_every_four_energy_spent(self, simple_combat):
        simple_combat.hand = [make_defend_ironclad() for _ in range(4)]
        simple_combat.energy = 4
        simple_combat.apply_power_to(simple_combat.player, PowerId.ORBIT, 1)

        assert simple_combat.play_card(0)
        assert simple_combat.play_card(0)
        assert simple_combat.play_card(0)
        assert simple_combat.play_card(0)

        assert simple_combat.energy == 1

    def test_orbit_keeps_separate_energy_counters_like_reference(self, simple_combat):
        card = make_defend_ironclad()
        card.owner = simple_combat.player
        simple_combat.apply_power_to(simple_combat.player, PowerId.ORBIT, 1)
        orbit = simple_combat.player.powers[PowerId.ORBIT]

        orbit.after_energy_spent(simple_combat.player, card, 3, simple_combat)
        simple_combat.apply_power_to(simple_combat.player, PowerId.ORBIT, 1)
        start_energy = simple_combat.energy

        orbit.after_energy_spent(simple_combat.player, card, 1, simple_combat)

        assert simple_combat.energy == start_energy + 1

    def test_chains_of_binding_allows_only_one_bound_card_per_turn(self, simple_combat):
        first = make_strike_ironclad()
        second = make_strike_ironclad()
        first.bound = True
        second.bound = True
        simple_combat.hand = [first, second]
        simple_combat.energy = 10
        simple_combat.apply_power_to(simple_combat.player, PowerId.CHAINS_OF_BINDING, 2)

        assert simple_combat.play_card(0, 0)
        assert simple_combat.can_play_card(simple_combat.hand[0]) is False
        assert simple_combat.play_card(0, 0) is False

    def test_chains_of_binding_clears_bound_cards_at_turn_end(self, simple_combat):
        card = make_strike_ironclad()
        card.owner = simple_combat.player
        card.bound = True
        simple_combat.hand = [card]
        simple_combat.apply_power_to(simple_combat.player, PowerId.CHAINS_OF_BINDING, 2)

        power = simple_combat.player.powers[PowerId.CHAINS_OF_BINDING]
        power.before_turn_end(simple_combat.player, CombatSide.PLAYER, simple_combat)

        assert card.bound is False
        assert simple_combat.can_play_card(card) is True

    def test_chains_of_binding_clears_bound_cards_at_any_turn_end_like_reference(self, simple_combat):
        card = make_strike_ironclad()
        card.owner = simple_combat.player
        card.bound = True
        simple_combat.hand = [card]
        simple_combat.apply_power_to(simple_combat.player, PowerId.CHAINS_OF_BINDING, 2)

        power = simple_combat.player.powers[PowerId.CHAINS_OF_BINDING]
        power.before_turn_end(simple_combat.player, CombatSide.ENEMY, simple_combat)

        assert card.bound is False
        assert simple_combat.can_play_card(card) is True

    def test_smoggy_only_tracks_owners_skill_plays(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        primary_skill = make_defend_ironclad()
        ally_skill = make_defend_ironclad()
        primary_skill.owner = simple_combat.player
        ally_skill.owner = ally
        simple_combat.hand = [primary_skill]
        simple_combat.energy = 1
        ally_state.hand = [ally_skill]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        simple_combat.player.powers[PowerId.SMOGGY] = SmoggyPower()

        assert simple_combat.play_card_from_creature(ally, 0)

        assert simple_combat.can_play_card(primary_skill) is True

    def test_smoggy_does_not_block_teammate_skills(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        primary_skill = make_defend_ironclad()
        ally_skill = make_defend_ironclad()
        primary_skill.owner = simple_combat.player
        ally_skill.owner = ally
        simple_combat.hand = [primary_skill]
        simple_combat.energy = 1
        ally_state.hand = [ally_skill]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        simple_combat.player.powers[PowerId.SMOGGY] = SmoggyPower()

        assert simple_combat.play_card(0)

        assert simple_combat.can_play_card(ally_skill) is True

    def test_smoggy_marks_new_skills_after_prior_skill_play_this_turn(self, simple_combat):
        first_skill = make_defend_ironclad()
        existing_skill = make_defend_ironclad()
        first_skill.owner = simple_combat.player
        existing_skill.owner = simple_combat.player
        simple_combat.hand = [first_skill, existing_skill]
        simple_combat.energy = 2

        assert simple_combat.play_card(0)

        simple_combat.player.powers[PowerId.SMOGGY] = SmoggyPower()

        assert simple_combat.can_play_card(existing_skill) is True

        new_skill = make_defend_ironclad()
        simple_combat.move_card_to_hand(new_skill)

        assert simple_combat.can_play_card(new_skill) is False

    def test_smoggy_marks_new_skills_after_skill_play_started(self, simple_combat):
        started_skill = make_deflect()
        new_skill = make_defend_ironclad()
        simple_combat.player.powers[PowerId.SMOGGY] = SmoggyPower()
        simple_combat.hand = [started_skill]
        simple_combat.energy = 0

        assert simple_combat.play_card(0)

        simple_combat.move_card_to_hand(new_skill)

        assert new_skill.has_affliction("smog")

    def test_smoggy_does_not_block_new_skill_with_existing_affliction(self, simple_combat):
        first_skill = make_defend_ironclad()
        first_skill.owner = simple_combat.player
        simple_combat.hand = [first_skill]
        simple_combat.energy = 2

        assert simple_combat.play_card(0)

        simple_combat.player.powers[PowerId.SMOGGY] = SmoggyPower()
        new_skill = make_defend_ironclad()
        new_skill.afflict("hexed")
        simple_combat.move_card_to_hand(new_skill)

        assert new_skill.has_affliction("hexed")
        assert simple_combat.can_play_card(new_skill) is True

    def test_free_card_powers_only_decrement_for_owner_cards(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        ally_state.hand = [make_strike_ironclad(), make_defend_ironclad(), make_afterimage()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 3
        simple_combat.apply_power_to(simple_combat.player, PowerId.FREE_ATTACK, 1)
        simple_combat.apply_power_to(simple_combat.player, PowerId.FREE_SKILL, 1)
        simple_combat.apply_power_to(simple_combat.player, PowerId.FREE_POWER, 1)

        assert simple_combat.play_card_from_creature(ally, 0, 0)
        assert simple_combat.play_card_from_creature(ally, 0)
        assert simple_combat.play_card_from_creature(ally, 0)

        assert ally_state.energy == 0
        assert simple_combat.player.get_power_amount(PowerId.FREE_ATTACK) == 1
        assert simple_combat.player.get_power_amount(PowerId.FREE_SKILL) == 1
        assert simple_combat.player.get_power_amount(PowerId.FREE_POWER) == 1

    def test_spirit_of_ash_triggers_for_string_ethereal_keyword(self, simple_combat):
        ethereal = make_defend_ironclad()
        ethereal.keywords = frozenset({"ethereal"})
        simple_combat.hand = [ethereal]
        simple_combat.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.SPIRIT_OF_ASH, 4)

        assert simple_combat.play_card(0)

        assert simple_combat.player.block == 9

    def test_spirit_of_ash_block_triggers_after_block_gained_hooks(self, simple_combat):
        ethereal = make_defend_ironclad()
        ethereal.keywords = frozenset({"ethereal"})
        simple_combat.hand = [ethereal]
        simple_combat.energy = 1
        counter = _BlockHookCounterPower()
        simple_combat.player.powers[PowerId.JUGGERNAUT] = counter
        simple_combat.apply_power_to(simple_combat.player, PowerId.SPIRIT_OF_ASH, 4)

        assert simple_combat.play_card(0)

        assert simple_combat.player.block == 9
        assert counter.calls == [4, 5]

    def test_tag_team_replays_non_applier_attack_against_target(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_strike_ironclad()]
        simple_combat.energy = 1
        ally_state.hand = [make_strike_ironclad()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        simple_combat.apply_power_to(enemy, PowerId.TAG_TEAM, 1, applier=simple_combat.player)

        assert simple_combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 6
        assert enemy.get_power_amount(PowerId.TAG_TEAM) == 1

        assert simple_combat.play_card_from_creature(ally, 0, 0)

        assert enemy.current_hp == starting_hp - 18
        assert not enemy.has_power(PowerId.TAG_TEAM)

    def test_tag_team_keeps_separate_appliers_like_reference(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_strike_ironclad()]
        simple_combat.energy = 1
        ally_state.hand = [make_strike_ironclad()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        simple_combat.apply_power_to(enemy, PowerId.TAG_TEAM, 1, applier=simple_combat.player)
        simple_combat.apply_power_to(enemy, PowerId.TAG_TEAM, 1, applier=ally)

        assert simple_combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 12
        assert enemy.get_power_amount(PowerId.TAG_TEAM) == 1

        assert simple_combat.play_card_from_creature(ally, 0, 0)
        assert enemy.current_hp == starting_hp - 24
        assert not enemy.has_power(PowerId.TAG_TEAM)

    def test_echo_form_ignores_teammate_cards(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_strike_ironclad()]
        simple_combat.energy = 2
        ally_state.hand = [make_strike_ironclad()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.ECHO_FORM, 1)

        assert simple_combat.play_card_from_creature(ally, 0, 0)
        assert enemy.current_hp == starting_hp - 6

        assert simple_combat.play_card(0, 0)

        assert enemy.current_hp == starting_hp - 18

    def test_echo_form_counts_cards_played_before_it_was_applied(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_strike_ironclad(), make_strike_ironclad()]
        simple_combat.energy = 10

        assert simple_combat.play_card(0, 0)

        simple_combat.apply_power_to(simple_combat.player, PowerId.ECHO_FORM, 1)

        assert simple_combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 12

    def test_echo_form_only_replays_first_started_card_each_turn(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_strike_ironclad(), make_strike_ironclad()]
        simple_combat.energy = 10
        simple_combat.apply_power_to(simple_combat.player, PowerId.ECHO_FORM, 1)

        assert simple_combat.play_card(0, 0)
        assert simple_combat.play_card(0, 0)

        assert enemy.current_hp == starting_hp - 18

    def test_replay_powers_do_not_affect_teammate_attacks(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        ally_state.hand = [make_strike_ironclad()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.ONE_TWO_PUNCH, 1)
        simple_combat.apply_power_to(simple_combat.player, PowerId.DUPLICATION, 1)

        assert simple_combat.play_card_from_creature(ally, 0, 0)

        assert enemy.current_hp == starting_hp - 6
        assert simple_combat.player.get_power_amount(PowerId.ONE_TWO_PUNCH) == 1
        assert simple_combat.player.get_power_amount(PowerId.DUPLICATION) == 1

    def test_burst_does_not_decrement_for_teammate_skills(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        ally_state.hand = [make_defend_ironclad()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.BURST, 1)

        assert simple_combat.play_card_from_creature(ally, 0)

        assert ally.block == 5
        assert simple_combat.player.get_power_amount(PowerId.BURST) == 1

    def test_phantom_blades_ignores_teammate_shivs_for_first_shiv_bonus(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Silent", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [_make_shiv()]
        simple_combat.energy = 0
        ally_state.hand = [_make_shiv()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 0
        simple_combat.apply_power_to(simple_combat.player, PowerId.PHANTOM_BLADES, 3)

        assert simple_combat.play_card_from_creature(ally, 0, 0)
        assert enemy.current_hp == starting_hp - 4

        assert simple_combat.play_card(0, 0)

        assert enemy.current_hp == starting_hp - 11

    def test_phantom_blades_counts_shivs_played_before_it_was_applied(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [_make_shiv(), _make_shiv()]
        simple_combat.energy = 0

        assert simple_combat.play_card(0, 0)

        simple_combat.apply_power_to(simple_combat.player, PowerId.PHANTOM_BLADES, 3)

        assert simple_combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 8

    def test_phantom_blades_gives_existing_owner_shivs_retain_on_application(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Silent", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        owner_shiv = _make_shiv()
        ally_shiv = _make_shiv()
        owner_shiv.owner = simple_combat.player
        ally_shiv.owner = ally
        simple_combat.hand = [owner_shiv]
        ally_state.hand = [ally_shiv]
        ally_state.zone_map["hand"] = ally_state.hand

        simple_combat.apply_power_to(simple_combat.player, PowerId.PHANTOM_BLADES, 3)

        assert owner_shiv.is_retain
        assert not ally_shiv.is_retain

    def test_rage_ignores_teammate_attacks(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        ally_state.hand = [make_strike_ironclad()]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.RAGE, 5)

        assert simple_combat.play_card_from_creature(ally, 0, 0)

        assert simple_combat.player.block == 0

    def test_star_powers_ignore_teammate_star_changes(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Regent", max_hp=60, current_hp=60))
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.apply_power_to(simple_combat.player, PowerId.BLACK_HOLE, 3)
        simple_combat.apply_power_to(simple_combat.player, PowerId.CHILD_OF_THE_STARS, 2)

        simple_combat.gain_stars(ally, 2)
        simple_combat.spend_stars(ally, 2)

        assert enemy.current_hp == starting_hp
        assert simple_combat.player.block == 0

    def test_gigantification_tracks_attack_context_and_decrements_after_attack(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        card = make_strike_ironclad()
        card.owner = player
        simple_combat.apply_power_to(player, PowerId.GIGANTIFICATION, 1)
        starting_hp = enemy.current_hp

        with simple_combat.attack_context(player, enemy, ValueProp.MOVE, model_source=card):
            damage = calculate_damage(3, player, enemy, ValueProp.MOVE, simple_combat)
            apply_damage(enemy, damage, ValueProp.MOVE, simple_combat, player)

        assert enemy.current_hp == starting_hp - 9
        assert not player.has_power(PowerId.GIGANTIFICATION)

    def test_black_hole_damages_only_after_star_cost_card_play(self, simple_combat):
        enemy = simple_combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        simple_combat.apply_power_to(simple_combat.player, PowerId.BLACK_HOLE, 3)
        simple_combat.stars = 1
        simple_combat.player.stars = 1

        assert simple_combat.spend_stars(simple_combat.player, 1) == 1

        assert enemy.current_hp == 100

        card = make_defend_ironclad()
        card.star_cost = 1
        simple_combat.hand = [card]
        simple_combat.energy = 1
        simple_combat.stars = 1
        simple_combat.player.stars = 1

        assert simple_combat.play_card(0)

        assert enemy.current_hp == 97

    def test_dampen_downgrades_cards_then_restores_when_caster_dies(self, simple_combat):
        enemy = simple_combat.enemies[0]
        bash = make_bash()
        simple_combat.upgrade_card(bash)
        simple_combat.hand = [bash]

        simple_combat.apply_power_to(simple_combat.player, PowerId.DAMPEN, 1, applier=enemy)
        dampen = simple_combat.player.powers[PowerId.DAMPEN]
        dampen.add_caster(enemy)

        assert bash.upgraded is False
        assert bash.base_damage == 8
        assert bash.effect_vars["vulnerable"] == 2

        assert simple_combat.kill_creature(enemy)

        assert PowerId.DAMPEN not in simple_combat.player.powers
        assert bash.upgraded is True
        assert bash.base_damage == 10
        assert bash.effect_vars["vulnerable"] == 3

    def test_hex_applies_ethereal_to_existing_and_new_cards_then_restores(self, simple_combat):
        enemy = simple_combat.enemies[0]
        normal = make_defend_ironclad()
        already_ethereal = make_defend_ironclad()
        already_ethereal.keywords = frozenset({"ethereal"})
        simple_combat.hand = [normal, already_ethereal]

        simple_combat.apply_power_to(simple_combat.player, PowerId.HEX, 2, applier=enemy)

        assert normal.is_ethereal
        assert already_ethereal.is_ethereal

        new_card = make_defend_ironclad()
        simple_combat.move_card_to_hand(new_card)

        assert new_card.is_ethereal

        assert simple_combat.kill_creature(enemy)

        assert PowerId.HEX not in simple_combat.player.powers
        assert not normal.is_ethereal
        assert not new_card.is_ethereal
        assert already_ethereal.is_ethereal

    def test_hex_skips_cards_with_existing_affliction(self, simple_combat):
        enemy = simple_combat.enemies[0]
        blocked = make_defend_ironclad()
        normal = make_defend_ironclad()
        blocked.afflict("smog")
        simple_combat.hand = [blocked, normal]

        simple_combat.apply_power_to(simple_combat.player, PowerId.HEX, 2, applier=enemy)

        assert blocked.has_affliction("smog")
        assert not blocked.is_ethereal
        assert normal.has_affliction("hexed")
        assert normal.is_ethereal

    def test_dampen_restore_keeps_hex_ethereal_until_hex_is_removed(self, simple_combat):
        enemy = simple_combat.enemies[0]
        bash = make_bash()
        simple_combat.upgrade_card(bash)
        simple_combat.hand = [bash]
        simple_combat.apply_power_to(simple_combat.player, PowerId.HEX, 1, applier=enemy)
        simple_combat.apply_power_to(simple_combat.player, PowerId.DAMPEN, 1, applier=enemy)

        assert not bash.upgraded
        assert bash.is_ethereal

        simple_combat._remove_power(simple_combat.player, PowerId.DAMPEN)

        assert bash.upgraded
        assert bash.is_ethereal

        simple_combat._remove_power(simple_combat.player, PowerId.HEX)

        assert not bash.is_ethereal

    def test_dampen_preserves_self_mutating_card_growth_when_downgrading_and_restoring(self, simple_combat):
        enemy = simple_combat.enemies[0]
        card = make_genetic_algorithm()
        simple_combat.hand = [card]
        simple_combat.energy = 1

        assert simple_combat.play_card(0)
        assert card.base_block == 4
        assert card.effect_vars["block"] == 4

        simple_combat.upgrade_card(card)
        simple_combat.hand = [card]

        simple_combat.apply_power_to(simple_combat.player, PowerId.DAMPEN, 1, applier=enemy)

        assert card.upgraded is False
        assert card.base_block == 4
        assert card.effect_vars["block"] == 4
        assert card.effect_vars["increase"] == 3

        simple_combat._remove_power(simple_combat.player, PowerId.DAMPEN)

        assert card.upgraded is True
        assert card.base_block == 4
        assert card.effect_vars["block"] == 4
        assert card.effect_vars["increase"] == 4

    def test_galvanic_damages_only_galvanized_power_card_owner(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        starting_hp = player.current_hp
        existing_power = make_inflame()
        simple_combat.hand = [existing_power]
        simple_combat.energy = 1

        simple_combat.apply_power_to(enemy, PowerId.GALVANIC, 3)

        assert simple_combat.play_card(0)
        assert player.current_hp == starting_hp

        new_power = make_inflame()
        simple_combat.move_card_to_hand(new_power)
        simple_combat.energy = 1

        assert simple_combat.play_card(0)

        assert player.current_hp == starting_hp - 3

    def test_galvanic_marks_existing_power_cards_before_combat_start(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        starting_hp = player.current_hp
        existing_power = make_inflame()
        simple_combat.hand = [existing_power]
        simple_combat.energy = 1
        simple_combat.apply_power_to(enemy, PowerId.GALVANIC, 3)

        fire_before_combat_start(simple_combat)

        assert simple_combat.play_card(0)
        assert player.current_hp == starting_hp - 3

    def test_galvanic_skips_power_cards_with_existing_affliction(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        starting_hp = player.current_hp
        existing_power = make_inflame()
        existing_power.afflict("hexed")
        simple_combat.hand = [existing_power]
        simple_combat.energy = 1
        simple_combat.apply_power_to(enemy, PowerId.GALVANIC, 3)

        fire_before_combat_start(simple_combat)

        assert existing_power.has_affliction("hexed")
        assert simple_combat.play_card(0)
        assert player.current_hp == starting_hp

    def test_dark_embrace_draws_for_owner_exhausts_only(self, simple_combat):
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        ally_state = simple_combat.combat_player_state_for(ally)
        assert ally_state is not None
        owner_card = make_defend_ironclad()
        owner_card.owner = simple_combat.player
        ally_card = make_defend_ironclad()
        ally_card.owner = ally
        owner_draw = make_bash()
        ally_ignored_draw = make_strike_ironclad()
        simple_combat.hand = [owner_card]
        ally_state.hand = [ally_card]
        ally_state.zone_map["hand"] = ally_state.hand
        simple_combat.draw_pile = [owner_draw, ally_ignored_draw]
        simple_combat.apply_power_to(simple_combat.player, PowerId.DARK_EMBRACE, 1)

        simple_combat.exhaust_card(ally_card)

        assert owner_draw in simple_combat.draw_pile

        simple_combat.exhaust_card(owner_card)

        assert owner_draw in simple_combat.hand
        assert ally_ignored_draw in simple_combat.draw_pile

    def test_dark_embrace_defers_ethereal_exhaust_draw_until_after_hand_flush(self, simple_combat):
        ethereal = make_defend_ironclad()
        ethereal.owner = simple_combat.player
        ethereal.keywords = frozenset({"ethereal"})
        marker = make_bash()
        simple_combat.hand = [ethereal]
        simple_combat.draw_pile = [marker]
        simple_combat.discard_pile = []
        simple_combat.apply_power_to(simple_combat.player, PowerId.DARK_EMBRACE, 1)

        simple_combat.end_player_turn()

        assert ethereal in simple_combat.exhaust_pile
        assert marker in simple_combat.hand
        assert marker not in simple_combat.discard_pile

    def test_gravity_uses_amount_from_before_card_played(self, simple_combat):
        class GravityMutator(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.VIGOR, 1)

            def before_card_played(self, owner: Creature, card: object, combat) -> None:
                owner.powers[PowerId.GRAVITY].amount = 10

        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_defend_ironclad()]
        simple_combat.energy = 1
        simple_combat.apply_power_to(simple_combat.player, PowerId.GRAVITY, 3)
        simple_combat.player.powers[PowerId.VIGOR] = GravityMutator()

        assert simple_combat.play_card(0)

        assert enemy.current_hp == starting_hp - 3

    def test_strangle_damages_owner_when_any_card_is_played(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_defend_ironclad()]
        simple_combat.energy = 1
        simple_combat.apply_power_to(enemy, PowerId.STRANGLE, 2, applier=simple_combat.player)

        assert simple_combat.play_card(0)

        assert enemy.current_hp == starting_hp - 2

    def test_strangle_uses_amount_from_before_card_played(self, simple_combat):
        class StrangleMutator(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.VIGOR, 1)

            def before_card_played(self, owner: Creature, card: object, combat) -> None:
                owner.powers[PowerId.STRANGLE].amount = 8

        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_defend_ironclad()]
        simple_combat.energy = 1
        simple_combat.apply_power_to(enemy, PowerId.STRANGLE, 2, applier=simple_combat.player)
        enemy.powers[PowerId.VIGOR] = StrangleMutator()

        assert simple_combat.play_card(0)

        assert enemy.current_hp == starting_hp - 2

    def test_subroutine_does_not_refund_itself_when_first_played(self, simple_combat):
        simple_combat.hand = [make_subroutine()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0)

        assert simple_combat.energy == 0
        assert simple_combat.player.get_power_amount(PowerId.SUBROUTINE) == 1

    def test_subroutine_uses_amount_from_before_card_played(self, simple_combat):
        simple_combat.apply_power_to(simple_combat.player, PowerId.SUBROUTINE, 2)
        simple_combat.hand = [make_subroutine()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0)

        assert simple_combat.energy == 2
        assert simple_combat.player.get_power_amount(PowerId.SUBROUTINE) == 3

    def test_afterimage_does_not_trigger_from_first_afterimage_play(self, simple_combat):
        simple_combat.hand = [make_afterimage()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0)

        assert simple_combat.player.block == 0
        assert simple_combat.player.get_power_amount(PowerId.AFTERIMAGE) == 1

    def test_afterimage_uses_amount_from_before_card_played(self, simple_combat):
        simple_combat.apply_power_to(simple_combat.player, PowerId.AFTERIMAGE, 2)
        simple_combat.hand = [make_afterimage()]
        simple_combat.energy = 1

        assert simple_combat.play_card(0)

        assert simple_combat.player.block == 2
        assert simple_combat.player.get_power_amount(PowerId.AFTERIMAGE) == 3

    def test_serpent_form_does_not_trigger_from_first_serpent_form_play(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.hand = [make_serpent_form()]
        simple_combat.energy = 3

        assert simple_combat.play_card(0)

        assert enemy.current_hp == starting_hp
        assert simple_combat.player.get_power_amount(PowerId.SERPENT_FORM) == 4

    def test_serpent_form_uses_amount_from_before_card_played(self, simple_combat):
        enemy = simple_combat.enemies[0]
        starting_hp = enemy.current_hp
        simple_combat.apply_power_to(simple_combat.player, PowerId.SERPENT_FORM, 2)
        simple_combat.hand = [make_serpent_form()]
        simple_combat.energy = 3

        assert simple_combat.play_card(0)

        assert enemy.current_hp == starting_hp - 2
        assert simple_combat.player.get_power_amount(PowerId.SERPENT_FORM) == 6

    def test_painful_stabs_adds_wounds_to_player_discard_on_unblocked_hit(self, simple_combat):
        enemy = simple_combat.enemies[0]
        player = simple_combat.player
        enemy.apply_power(PowerId.PAINFUL_STABS, 2)

        simple_combat.deal_damage(
            dealer=enemy,
            target=player,
            amount=5,
            props=ValueProp.MOVE,
        )

        wounds = [card for card in simple_combat.discard_pile if card.card_id == CardId.WOUND]
        assert len(wounds) == 2

    def test_painful_stabs_keeps_owner_on_field_after_death(self, simple_combat):
        enemy = simple_combat.enemies[0]
        enemy.apply_power(PowerId.PAINFUL_STABS, 2)

        assert simple_combat.kill_creature(enemy)

        assert PowerId.PAINFUL_STABS in enemy.powers
        assert enemy.escaped is False

    def test_poison_triggers_multiple_times_with_opponent_accelerant(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.ACCELERANT, 2)
        enemy.apply_power(PowerId.POISON, 3)

        poison = enemy.powers[PowerId.POISON]
        starting_hp = enemy.current_hp
        poison.after_side_turn_start(enemy, CombatSide.ENEMY, simple_combat)

        assert enemy.current_hp == starting_hp - 6
        assert not enemy.has_power(PowerId.POISON)

    def test_reflect_deals_blocked_damage_back_to_attacker(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.REFLECT, 1)
        player.block = 3
        starting_enemy_hp = enemy.current_hp

        simple_combat.deal_damage(
            dealer=enemy,
            target=player,
            amount=5,
            props=ValueProp.MOVE,
        )

        assert enemy.current_hp == starting_enemy_hp - 3

    def test_reflect_runs_in_normal_after_damage_received_order(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        observed_enemy_hp = []

        class ObserveBeforeReflectPower(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.STRENGTH, 1)

            def after_damage_received(self, owner, target, dealer, damage, props, combat):
                if target is owner and dealer is not None:
                    observed_enemy_hp.append(dealer.current_hp)

        player.powers[PowerId.STRENGTH] = ObserveBeforeReflectPower()
        player.apply_power(PowerId.REFLECT, 1)
        player.block = 3
        starting_enemy_hp = enemy.current_hp

        simple_combat.deal_damage(
            dealer=enemy,
            target=player,
            amount=5,
            props=ValueProp.MOVE,
        )

        assert observed_enemy_hp == [starting_enemy_hp]
        assert enemy.current_hp == starting_enemy_hp - 3

    def test_reflect_does_not_trigger_when_owner_dies_to_same_hit(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.REFLECT, 1)
        player.current_hp = 2
        player.block = 3
        starting_enemy_hp = enemy.current_hp

        simple_combat.deal_damage(
            dealer=enemy,
            target=player,
            amount=5,
            props=ValueProp.MOVE,
        )

        assert player.is_dead
        assert enemy.current_hp == starting_enemy_hp

    def test_reflect_does_not_trigger_when_teammate_blocks_damage(self, simple_combat):
        player = simple_combat.player
        ally = simple_combat.add_ally_player(PlayerState(player_id=2, character_id="Ironclad", max_hp=70, current_hp=70))
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.REFLECT, 1)
        ally.block = 3
        starting_enemy_hp = enemy.current_hp

        simple_combat.deal_damage(
            dealer=enemy,
            target=ally,
            amount=5,
            props=ValueProp.MOVE,
        )

        assert enemy.current_hp == starting_enemy_hp

    def test_the_gambit_runs_full_death_flow(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        events = []

        class DeathObserverPower(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.STRENGTH, 1)

            def after_current_hp_changed(self, owner, creature, delta, combat):
                if creature is owner:
                    events.append(("hp", delta))

            def after_death(self, owner, creature, combat, was_removal_prevented):
                if creature is owner and not was_removal_prevented:
                    events.append(("death", creature))

        player.powers[PowerId.STRENGTH] = DeathObserverPower()
        player.apply_power(PowerId.THE_GAMBIT, 1)

        simple_combat.deal_damage(
            dealer=enemy,
            target=player,
            amount=1,
            props=ValueProp.MOVE,
        )

        assert player.is_dead
        assert events == [("hp", -1), ("hp", -79), ("death", player)]
        assert not player.has_power(PowerId.THE_GAMBIT)

    def test_no_block_only_blocks_card_sourced_block(self, simple_combat):
        player = simple_combat.player
        player.apply_power(PowerId.NO_BLOCK, 1)

        card_block = calculate_block(8, player, ValueProp.MOVE, simple_combat, card_source=object())
        non_card_block = calculate_block(8, player, ValueProp.MOVE, simple_combat, card_source=None)

        assert card_block == 0
        assert non_card_block == 8

    def test_no_block_decrements_at_enemy_turn_end_without_skip(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(player, PowerId.NO_BLOCK, 2, applier=enemy)

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert player.get_power_amount(PowerId.NO_BLOCK) == 1

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert PowerId.NO_BLOCK not in player.powers
        assert calculate_block(8, player, ValueProp.MOVE, simple_combat, card_source=object()) == 8

    def test_intangible_is_removed_when_duration_reaches_zero(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.INTANGIBLE, 1)

        assert calculate_damage(10, enemy, player, ValueProp.MOVE, simple_combat) == 1

        fire_after_turn_end(CombatSide.ENEMY, simple_combat)

        assert PowerId.INTANGIBLE not in player.powers
        assert calculate_damage(10, enemy, player, ValueProp.MOVE, simple_combat) == 10


class TestDexterityBlock:
    """Dexterity adds to block; Frail multiplies block by 0.75."""

    def test_dexterity_adds_to_block(self, player):
        player.apply_power(PowerId.DEXTERITY, 3)
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        assert block == 8  # 5 + 3

    def test_frail_reduces_block(self, player):
        """Frail x0.75 block."""
        player.apply_power(PowerId.FRAIL, 2)
        block = calculate_block(8, player, ValueProp.MOVE, [player])
        assert block == 6  # floor(8 * 0.75) = 6

    def test_dexterity_and_frail_combined(self, player):
        """Dexterity additive first, then Frail multiplicative."""
        player.apply_power(PowerId.DEXTERITY, 3)
        player.apply_power(PowerId.FRAIL, 2)
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        # (5 + 3) * 0.75 = 6.0 -> 6
        assert block == 6


class TestCreatureBlock:
    """Block cap, clear, and heal cap."""

    def test_clear_block(self, player):
        player.gain_block(10)
        player.clear_block()
        assert player.block == 0

    def test_block_capped_at_999999999(self, player):
        """C# Creature.cs:465 GainBlockInternal caps block at 999999999."""
        player.gain_block(1000)
        assert player.block == 1000

        player.gain_block(999999999)
        assert player.block == 999999999

    def test_heal_capped_at_max(self, player):
        player.current_hp = 50
        healed = player.heal(100)
        assert player.current_hp == 80  # max_hp
        assert healed == 30
