"""Ironclad card-model parity tests for core attack, draw, and power mechanics."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import (
    make_ashen_strike,
    create_ironclad_starter_deck,
    make_battle_trance,
    make_bash,
    make_bloodletting,
    make_bully,
    make_cruelty,
    make_feed,
    make_feel_no_pain,
    make_fiend_fire,
    make_hellraiser,
    make_inflame,
    make_juggernaut,
    make_one_two_punch,
    make_perfected_strike,
    make_pommel_strike,
    make_rage,
    make_rampage,
    make_twin_strike,
    make_vicious,
)
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


REFERENCE_IRONCLAD_HP = 80
REFERENCE_IRONCLAD_SEED = 84
PERFECTED_STRIKE_BASE_DAMAGE = 6
PERFECTED_STRIKE_PLUS_EXTRA_DAMAGE = 3
ASHEN_STRIKE_BASE_DAMAGE = 6
ASHEN_STRIKE_PLUS_EXTRA_DAMAGE = 4
BULLY_BASE_DAMAGE = 4
BULLY_PLUS_EXTRA_DAMAGE = 3
BATTLE_TRANCE_DRAW_COUNT = 3
BATTLE_TRANCE_NO_DRAW_AMOUNT = 1
BLOODLETTING_HP_LOSS = 3
BLOODLETTING_ENERGY_GAIN = 2
POMMEL_STRIKE_DAMAGE = 9
BASH_DAMAGE = 8
INFLAME_PLUS_STRENGTH = 3
PERFECTED_STRIKE_WITH_FIVE_STRIKES_DAMAGE = 16
RAMPAGE_DAMAGE = 9
RAMPAGE_DAMAGE_AFTER_ONE_PLAY = 14
RAMPAGE_DAMAGE_AFTER_TWO_PLAYS = 19
RAMPAGE_PLUS_DAMAGE_INCREASE = 9
FEEL_NO_PAIN_BLOCK = 3
RAGE_BLOCK = 3
# Juggernaut.cs: PowerVar<JuggernautPower>(6); OnUpgrade +2.
JUGGERNAUT_DAMAGE = 6
STRIKE_DAMAGE = 6
FIEND_FIRE_DAMAGE_PER_HIT = 7
TWIN_STRIKE_DAMAGE_PER_HIT = 5
THORNS_LETHAL_DAMAGE = 5
ATTACKER_FATAL_HP = 3
TARGET_HP_FOR_POWER_INTERACTION_TEST = 50
REFERENCE_ENEMY_HP = 100
CRUELTY_POWER_AMOUNT = 25
CRUELTY_PLUS_POWER_AMOUNT = 50
CRUELTY_PLUS_VULNERABLE_STRIKE_DAMAGE = 12
VICIOUS_POWER_AMOUNT = 1
VICIOUS_PLUS_POWER_AMOUNT = 2
ONE_TWO_PUNCH_POWER_AMOUNT = 1
ONE_TWO_PUNCH_PLUS_POWER_AMOUNT = 2
ONE_TWO_PUNCH_STRIKE_DAMAGE = 12
HELLRAISER_POWER_AMOUNT = 1
HELLRAISER_BASE_COST = 2
HELLRAISER_PLUS_COST = 1
VULNERABLE_TRIGGER_AMOUNT = 1
FEED_DAMAGE = 10
FEED_MAX_HP_GAIN = 3
FEED_PLUS_DAMAGE = 12
FEED_PLUS_MAX_HP_GAIN = 4


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=REFERENCE_IRONCLAD_HP,
        player_max_hp=REFERENCE_IRONCLAD_HP,
        deck=create_ironclad_starter_deck(),
        rng_seed=REFERENCE_IRONCLAD_SEED,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(REFERENCE_IRONCLAD_SEED))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestIroncladCoreCardModelParity:
    def test_dynamic_damage_upgrades_only_increase_extra_damage(self):
        """Matches PerfectedStrike/AshenStrike/Bully: upgrade increases ExtraDamage, not CalculationBase."""
        assert make_perfected_strike(upgraded=True).effect_vars == {
            "calc_base": PERFECTED_STRIKE_BASE_DAMAGE,
            "extra_damage": PERFECTED_STRIKE_PLUS_EXTRA_DAMAGE,
        }
        assert make_ashen_strike(upgraded=True).effect_vars == {
            "calc_base": ASHEN_STRIKE_BASE_DAMAGE,
            "extra_damage": ASHEN_STRIKE_PLUS_EXTRA_DAMAGE,
        }
        assert make_bully(upgraded=True).effect_vars == {
            "calc_base": BULLY_BASE_DAMAGE,
            "extra_damage": BULLY_PLUS_EXTRA_DAMAGE,
        }

    def test_battle_trance_draws_then_applies_no_draw_for_future_draws(self):
        """Matches BattleTrance.cs: draw first, then apply No Draw for later non-hand draws."""
        combat = _make_combat()
        draw_a = make_strike_ironclad()
        draw_b = make_defend_ironclad()
        draw_c = make_inflame()
        draw_d = make_rampage()
        combat.hand = [make_battle_trance()]
        combat.draw_pile = [draw_a, draw_b, draw_c, draw_d]
        combat.energy = 0

        assert combat.play_card(0)
        assert len(combat.hand) == BATTLE_TRANCE_DRAW_COUNT
        assert draw_a in combat.hand
        assert draw_b in combat.hand
        assert draw_c in combat.hand
        assert combat.player.get_power_amount(PowerId.NO_DRAW) == BATTLE_TRANCE_NO_DRAW_AMOUNT

        combat.draw_cards(combat.player, 1)
        assert len(combat.hand) == BATTLE_TRANCE_DRAW_COUNT
        assert combat.draw_pile[0] is draw_d

    def test_bloodletting_is_unblockable_self_hp_loss_then_energy_gain(self):
        """Matches Bloodletting.cs: lose 3 HP (unblockable/unpowered) and gain 2 energy."""
        combat = _make_combat()
        start_hp = combat.player.current_hp
        preexisting_block = 20
        combat.player.gain_block(preexisting_block)
        combat.hand = [make_bloodletting()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.current_hp == start_hp - BLOODLETTING_HP_LOSS
        assert combat.player.block == preexisting_block
        assert combat.energy == BLOODLETTING_ENERGY_GAIN

    def test_pommel_strike_deals_damage_and_draws_one(self):
        """Matches PommelStrike.cs: attack target, then draw the configured number of cards."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        drawn = make_inflame()
        combat.hand = [make_pommel_strike()]
        combat.draw_pile = [drawn]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - POMMEL_STRIKE_DAMAGE
        assert len(combat.hand) == 1
        assert combat.hand[0] is drawn

    def test_pommel_strike_does_not_draw_after_ending_combat(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = POMMEL_STRIKE_DAMAGE
        enemy.max_hp = POMMEL_STRIKE_DAMAGE
        drawn = make_inflame()
        combat.hand = [make_pommel_strike()]
        combat.draw_pile = [drawn]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert combat.is_over
        assert combat.hand == []
        assert combat.draw_pile == [drawn]

    def test_bash_does_not_apply_vulnerable_after_ending_combat(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = BASH_DAMAGE
        enemy.max_hp = BASH_DAMAGE
        combat.hand = [make_bash()]
        combat.energy = 2

        assert combat.play_card(0, 0)

        assert combat.is_over
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 0

    def test_bash_does_not_apply_vulnerable_to_removed_target_while_combat_continues(self):
        combat = _make_combat()
        first = combat.enemies[0]
        second, second_ai = create_shrinker_beetle(Rng(85))
        combat.add_enemy(second, second_ai)
        first.current_hp = BASH_DAMAGE
        first.max_hp = BASH_DAMAGE
        second.current_hp = 30
        second.max_hp = 30
        combat.hand = [make_bash()]
        combat.energy = 2

        assert combat.play_card(0, 0)

        assert not combat.is_over
        assert first.escaped
        assert first.get_power_amount(PowerId.VULNERABLE) == 0
        assert second.get_power_amount(PowerId.VULNERABLE) == 0

    def test_feed_gains_reference_max_hp_on_fatal_hit(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = FEED_DAMAGE
        enemy.max_hp = FEED_DAMAGE
        max_hp_before = combat.player.max_hp
        combat.hand = [make_feed()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.player.max_hp == max_hp_before + FEED_MAX_HP_GAIN

        upgraded_combat = _make_combat()
        upgraded_enemy = upgraded_combat.enemies[0]
        upgraded_enemy.current_hp = FEED_PLUS_DAMAGE
        upgraded_enemy.max_hp = FEED_PLUS_DAMAGE
        upgraded_max_hp_before = upgraded_combat.player.max_hp
        upgraded_combat.hand = [make_feed(upgraded=True)]
        upgraded_combat.energy = 1

        assert upgraded_combat.play_card(0, 0)
        assert upgraded_combat.player.max_hp == upgraded_max_hp_before + FEED_PLUS_MAX_HP_GAIN

    def test_inflame_applies_strength_power_amount(self):
        """Matches Inflame.cs: apply StrengthPower using the configured amount."""
        combat = _make_combat()
        combat.hand = [make_inflame(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == INFLAME_PLUS_STRENGTH

    def test_cruelty_applies_reference_power_amounts_and_boosts_vulnerable_damage(self):
        """Matches Cruelty.cs: apply the reference power and raise powered Vulnerable damage."""
        combat = _make_combat()
        base_card = make_cruelty()
        combat.hand = [base_card]
        combat.energy = base_card.cost

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.CRUELTY) == CRUELTY_POWER_AMOUNT

        upgraded_combat = _make_combat()
        enemy = upgraded_combat.enemies[0]
        start_hp = enemy.current_hp
        upgraded_card = make_cruelty(upgraded=True)
        upgraded_combat.hand = [upgraded_card]
        upgraded_combat.energy = upgraded_card.cost

        assert upgraded_combat.play_card(0)
        assert upgraded_combat.player.get_power_amount(PowerId.CRUELTY) == CRUELTY_PLUS_POWER_AMOUNT

        upgraded_combat.apply_power_to(enemy, PowerId.VULNERABLE, VULNERABLE_TRIGGER_AMOUNT)
        strike = make_strike_ironclad()
        upgraded_combat.hand = [strike]
        upgraded_combat.energy = strike.cost

        assert upgraded_combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - CRUELTY_PLUS_VULNERABLE_STRIKE_DAMAGE

    def test_vicious_applies_reference_power_amounts_and_draws_when_owner_applies_vulnerable(self):
        """Matches Vicious.cs: apply the reference power and draw after owner Vulnerable."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        drawn_card = make_strike_ironclad()
        base_card = make_vicious()
        combat.hand = [base_card]
        combat.draw_pile = [drawn_card]
        combat.energy = base_card.cost

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.VICIOUS) == VICIOUS_POWER_AMOUNT

        combat.apply_power_to(enemy, PowerId.VULNERABLE, VULNERABLE_TRIGGER_AMOUNT)
        assert combat.hand == [drawn_card]

        upgraded_combat = _make_combat()
        upgraded_enemy = upgraded_combat.enemies[0]
        first_drawn = make_strike_ironclad()
        second_drawn = make_defend_ironclad()
        upgraded_card = make_vicious(upgraded=True)
        upgraded_combat.hand = [upgraded_card]
        upgraded_combat.draw_pile = [first_drawn, second_drawn]
        upgraded_combat.energy = upgraded_card.cost

        assert upgraded_combat.play_card(0)
        assert upgraded_combat.player.get_power_amount(PowerId.VICIOUS) == VICIOUS_PLUS_POWER_AMOUNT

        upgraded_combat.apply_power_to(upgraded_enemy, PowerId.VULNERABLE, VULNERABLE_TRIGGER_AMOUNT)
        assert upgraded_combat.hand == [first_drawn, second_drawn]

    def test_one_two_punch_applies_reference_power_amounts_and_replays_next_attack(self):
        """Matches OneTwoPunch.cs: apply the reference power and replay the next owner attack."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        base_card = make_one_two_punch()
        combat.hand = [base_card]
        combat.energy = base_card.cost

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ONE_TWO_PUNCH) == ONE_TWO_PUNCH_POWER_AMOUNT

        strike = make_strike_ironclad()
        combat.hand = [strike]
        combat.energy = strike.cost

        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - ONE_TWO_PUNCH_STRIKE_DAMAGE
        assert combat.player.get_power_amount(PowerId.ONE_TWO_PUNCH) == 0

        upgraded_combat = _make_combat()
        upgraded_card = make_one_two_punch(upgraded=True)
        upgraded_combat.hand = [upgraded_card]
        upgraded_combat.energy = upgraded_card.cost

        assert upgraded_combat.play_card(0)
        assert upgraded_combat.player.get_power_amount(PowerId.ONE_TWO_PUNCH) == ONE_TWO_PUNCH_PLUS_POWER_AMOUNT

    def test_hellraiser_applies_reference_power_and_auto_plays_drawn_strike(self):
        """Matches Hellraiser.cs: cost upgrade applies, and drawn Strikes auto-play."""
        assert make_hellraiser().cost == HELLRAISER_BASE_COST
        assert make_hellraiser(upgraded=True).cost == HELLRAISER_PLUS_COST

        combat = _make_combat()
        enemy = combat.enemies[0]
        strike = make_strike_ironclad()
        start_hp = enemy.current_hp
        base_card = make_hellraiser()
        combat.hand = [base_card]
        combat.draw_pile = [strike]
        combat.energy = base_card.cost

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.HELLRAISER) == HELLRAISER_POWER_AMOUNT

        combat.draw_cards(combat.player, 1)
        assert strike not in combat.hand
        assert enemy.current_hp == start_hp - STRIKE_DAMAGE

    def test_perfected_strike_counts_all_strike_cards_including_itself(self):
        """Matches PerfectedStrike.cs: scale from all owner strike cards in combat state."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        perfected = make_perfected_strike()
        combat.hand = [perfected, make_pommel_strike(), make_inflame()]
        combat.draw_pile = [make_strike_ironclad()]
        combat.discard_pile = [make_strike_ironclad()]
        combat.exhaust_pile = [make_strike_ironclad()]
        combat.play_pile = []
        combat.energy = 2

        assert combat.play_card(0, 0)
        # Strikes: Perfected Strike itself + Pommel Strike + draw + discard + exhaust = 5.
        assert enemy.current_hp == start_hp - PERFECTED_STRIKE_WITH_FIVE_STRIKES_DAMAGE

    def test_rampage_increases_its_own_base_damage_each_play(self):
        """Matches Rampage.cs: card mutates its own base damage after each play."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        rampage = make_rampage()
        combat.hand = [rampage]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - RAMPAGE_DAMAGE
        assert rampage.base_damage == RAMPAGE_DAMAGE_AFTER_ONE_PLAY

        combat.hand = [combat.discard_pile.pop(0)]
        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - RAMPAGE_DAMAGE - RAMPAGE_DAMAGE_AFTER_ONE_PLAY
        assert rampage.base_damage == RAMPAGE_DAMAGE_AFTER_TWO_PLAYS

    def test_rampage_upgrade_preserves_grown_damage(self):
        combat = _make_combat()
        rampage = make_rampage()
        combat.hand = [rampage]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert rampage.base_damage == RAMPAGE_DAMAGE_AFTER_ONE_PLAY

        combat.upgrade_card(rampage)

        assert rampage.upgraded is True
        assert rampage.base_damage == RAMPAGE_DAMAGE_AFTER_ONE_PLAY
        assert rampage.effect_vars["increase"] == RAMPAGE_PLUS_DAMAGE_INCREASE

    def test_feel_no_pain_gives_block_when_owner_card_is_exhausted(self):
        """Matches FeelNoPain.cs + FeelNoPainPower.cs: owner gains block per exhausted card."""
        combat = _make_combat()
        strike = make_strike_ironclad()
        combat.hand = [make_feel_no_pain(), strike]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.FEEL_NO_PAIN) == FEEL_NO_PAIN_BLOCK

        combat.exhaust_card(strike)
        assert combat.player.block == FEEL_NO_PAIN_BLOCK

    def test_power_block_gains_trigger_after_block_gained_hooks(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = TARGET_HP_FOR_POWER_INTERACTION_TEST
        enemy.current_hp = TARGET_HP_FOR_POWER_INTERACTION_TEST
        combat.hand = [make_feel_no_pain(), make_juggernaut(), make_rage(), make_strike_ironclad()]
        combat.energy = 4

        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.play_card(0, 0)

        assert combat.player.block == RAGE_BLOCK
        assert enemy.current_hp == TARGET_HP_FOR_POWER_INTERACTION_TEST - STRIKE_DAMAGE - JUGGERNAUT_DAMAGE

        combat.exhaust_card(combat.discard_pile[0])
        assert combat.player.block == RAGE_BLOCK + FEEL_NO_PAIN_BLOCK
        assert enemy.current_hp == (
            TARGET_HP_FOR_POWER_INTERACTION_TEST
            - STRIKE_DAMAGE
            - JUGGERNAUT_DAMAGE
            - JUGGERNAUT_DAMAGE
        )

    def test_fiend_fire_exhausts_hand_for_hits_and_triggers_feel_no_pain_per_exhaust(self):
        """Matches FiendFire.cs with exhaust hooks: exhaust all hand cards, one hit each, hooks fire."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        combat.hand = [make_feel_no_pain(), make_fiend_fire(), strike, defend]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.play_card(0, 0)

        assert enemy.current_hp == start_hp - FIEND_FIRE_DAMAGE_PER_HIT * 2
        assert combat.player.block == FEEL_NO_PAIN_BLOCK * 3
        assert strike in combat.exhaust_pile
        assert defend in combat.exhaust_pile
        assert len(combat.hand) == 0

    def test_fiend_fire_stops_followup_hits_if_attacker_dies_to_thorns(self):
        """Matches AttackCommand.cs: later hits stop when the attacker is dead."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        enemy.apply_power(PowerId.THORNS, THORNS_LETHAL_DAMAGE)
        combat.player.current_hp = ATTACKER_FATAL_HP
        combat.hand = [make_fiend_fire(), make_strike_ironclad(), make_defend_ironclad()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert combat.player.current_hp == 0
        assert enemy.current_hp == REFERENCE_ENEMY_HP - FIEND_FIRE_DAMAGE_PER_HIT
        assert combat.is_over
        assert combat.player_won is False

    def test_twin_strike_stops_second_hit_if_attacker_dies_to_thorns(self):
        """Matches AttackCommand.cs: the next hit is skipped if the attacker died."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        enemy.apply_power(PowerId.THORNS, THORNS_LETHAL_DAMAGE)
        combat.player.current_hp = ATTACKER_FATAL_HP
        combat.hand = [make_twin_strike()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.player.current_hp == 0
        assert enemy.current_hp == REFERENCE_ENEMY_HP - TWIN_STRIKE_DAMAGE_PER_HIT
        assert combat.is_over
        assert combat.player_won is False
