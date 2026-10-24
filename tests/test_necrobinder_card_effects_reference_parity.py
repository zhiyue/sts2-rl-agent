"""Reference parity tests for Necrobinder card effects."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.necrobinder import (
    HANG_STACK_CAP,
    make_banshees_cry,
    create_necrobinder_starter_deck,
    make_blight_strike,
    make_bodyguard,
    make_bury,
    make_calcify_card,
    make_call_of_the_void,
    make_deathbringer,
    make_deaths_door,
    make_debilitate_card,
    make_defy,
    make_defend_necrobinder,
    make_demesne,
    make_devour_life_card,
    make_enfeebling_touch,
    make_end_of_days,
    make_fear,
    make_fetch,
    make_flatten,
    make_friendship,
    make_hang,
    make_high_five,
    make_invoke,
    make_lethality_card,
    make_misery,
    make_negative_pulse,
    make_neurosurge,
    make_pagestorm,
    make_parse,
    make_poke,
    make_pull_from_below,
    make_sculpting_strike,
    make_scourge,
    make_sentry_mode,
    make_shared_fate,
    make_sic_em,
    make_snap,
    make_sow,
    make_squeeze,
    make_strike_necrobinder,
    make_times_up,
    make_unleash,
    make_veilpiercer,
)
from sts2_env.cards.status import make_soul, make_void
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CombatSide, PowerId, ValueProp
from sts2_env.core.hooks import fire_after_side_turn_start, fire_after_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.powers.base import PowerInstance


REFERENCE_ENEMY_HP = 100
DELAY_BLOCK = 11
DELAY_ENERGY_NEXT_TURN = 1
DELAY_UPGRADED_BLOCK = 13
DELAY_UPGRADED_ENERGY_NEXT_TURN = 2
SOW_DAMAGE = 8
SOW_UPGRADED_DAMAGE = 11
STRIKE_NECROBINDER_DAMAGE = 6
STRIKE_NECROBINDER_UPGRADED_DAMAGE = 9
# C# Debilitate.cs: DamageVar(10), PowerVar<DebilitatePower>(2);
# OnUpgrade: Damage +2 (12), DebilitatePower +1 (3).
DEBILITATE_DAMAGE = 10
DEBILITATE_POWER_AMOUNT = 2
DEBILITATE_UPGRADED_DAMAGE = 12
DEBILITATE_UPGRADED_POWER_AMOUNT = 3
PARSE_DRAW_COUNT = 3
PARSE_UPGRADED_DRAW_COUNT = 4
DEATHS_DOOR_REPEAT = 2
DEATHS_DOOR_BLOCK_GAIN = 6
NEUROSURGE_POWER_AMOUNT = 3
NEUROSURGE_UPGRADED_ENERGY = 4
SENTRY_MODE_POWER_AMOUNT = 1
SENTRY_MODE_UPGRADED_COST = 1


class _CannotHitPower(PowerInstance):
    def __init__(self):
        super().__init__(PowerId.COVERED, 1)

    def should_allow_hitting(self, owner, combat):
        return False


def _make_combat(*, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=create_necrobinder_starter_deck(),
        rng_seed=9090,
        character_id="Necrobinder",
    )
    creature, ai = create_shrinker_beetle(Rng(9090))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(9200 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class TestNecrobinderCardEffectsReferenceParity:
    def test_strike_necrobinder_deals_single_target_damage(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        combat.hand = [make_strike_necrobinder()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == REFERENCE_ENEMY_HP - STRIKE_NECROBINDER_DAMAGE

    def test_upgraded_strike_necrobinder_damage_matches_reference(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        combat.hand = [make_strike_necrobinder(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == REFERENCE_ENEMY_HP - STRIKE_NECROBINDER_UPGRADED_DAMAGE

    def test_sow_hits_all_enemies_and_retains(self):
        combat = _make_combat(extra_enemies=1)
        for enemy in combat.enemies:
            enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        card = make_sow()
        combat.hand = [card]
        combat.energy = 1

        assert card.is_retain
        assert combat.play_card(0)
        assert [enemy.current_hp for enemy in combat.enemies] == [
            REFERENCE_ENEMY_HP - SOW_DAMAGE,
            REFERENCE_ENEMY_HP - SOW_DAMAGE,
        ]

    def test_upgraded_sow_damage_matches_reference(self):
        combat = _make_combat(extra_enemies=1)
        for enemy in combat.enemies:
            enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        combat.hand = [make_sow(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert [enemy.current_hp for enemy in combat.enemies] == [
            REFERENCE_ENEMY_HP - SOW_UPGRADED_DAMAGE,
            REFERENCE_ENEMY_HP - SOW_UPGRADED_DAMAGE,
        ]

    def test_delay_gains_block_and_applies_energy_next_turn(self):
        combat = _make_combat()
        combat.hand = [create_card(CardId.DELAY)]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == DELAY_BLOCK
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == DELAY_ENERGY_NEXT_TURN

    def test_upgraded_delay_values_match_reference(self):
        combat = _make_combat()
        combat.hand = [create_card(CardId.DELAY, upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == DELAY_UPGRADED_BLOCK
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == DELAY_UPGRADED_ENERGY_NEXT_TURN

    def test_bury_deals_large_single_target_damage(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        combat.hand = [make_bury()]
        combat.energy = 4

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 48

    def test_upgraded_bury_damage_matches_reference(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        combat.hand = [make_bury(upgraded=True)]
        combat.energy = 4

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 37

    def test_calcify_applies_owner_power_amount(self):
        combat = _make_combat()
        combat.hand = [make_calcify_card(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.CALCIFY) == 6

    def test_calcify_adds_damage_to_owner_osty_attacks(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        osty = combat.summon_osty(combat.player, 5)
        assert osty is not None
        combat.player.apply_power(PowerId.CALCIFY, 4)

        combat.deal_damage(osty, enemy, 3, ValueProp.MOVE)

        assert enemy.current_hp == enemy.max_hp - 7

    def test_defy_gains_block_and_applies_weak(self):
        """Matches Defy.cs: BlockVar(6), PowerVar<WeakPower>(1); OnUpgrade: Block +3
        only (9 block), Weak stays 1."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_defy(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.player.block == 9
        assert enemy.get_power_amount(PowerId.WEAK) == 1

    def test_debilitate_deals_damage_then_applies_power(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        combat.hand = [make_debilitate_card()]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert enemy.current_hp == REFERENCE_ENEMY_HP - DEBILITATE_DAMAGE
        assert enemy.get_power_amount(PowerId.DEBILITATE) == DEBILITATE_POWER_AMOUNT

    def test_upgraded_debilitate_values_match_reference(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        combat.hand = [make_debilitate_card(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert enemy.current_hp == REFERENCE_ENEMY_HP - DEBILITATE_UPGRADED_DAMAGE
        assert enemy.get_power_amount(PowerId.DEBILITATE) == DEBILITATE_UPGRADED_POWER_AMOUNT

    def test_demesne_applies_power_and_upgrade_only_changes_cost(self):
        combat = _make_combat()
        card = make_demesne(upgraded=True)
        combat.hand = [card]
        combat.energy = 2

        assert card.cost == 2
        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.DEMESNE) == 1

    def test_devour_life_applies_owner_power_amount(self):
        combat = _make_combat()
        combat.hand = [make_devour_life_card(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.DEVOUR_LIFE) == 2

    def test_lethality_applies_owner_power_amount(self):
        combat = _make_combat()
        combat.hand = [make_lethality_card(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.LETHALITY) == 75

    def test_parse_draws_reference_cards_and_is_ethereal(self):
        combat = _make_combat()
        drawn = [make_defend_necrobinder() for _ in range(PARSE_DRAW_COUNT)]
        card = make_parse()
        combat.hand = [card]
        combat.draw_pile = list(drawn)
        combat.energy = 1

        assert card.is_ethereal
        assert combat.play_card(0)

        assert combat.hand == drawn

    def test_upgraded_parse_draw_count_matches_reference(self):
        combat = _make_combat()
        drawn = [make_defend_necrobinder() for _ in range(PARSE_UPGRADED_DRAW_COUNT)]
        combat.hand = [make_parse(upgraded=True)]
        combat.draw_pile = list(drawn)
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.hand == drawn

    def test_scourge_applies_doom_then_draws_cards(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        first = make_defend_necrobinder()
        second = make_defend_necrobinder()
        combat.hand = [make_scourge(upgraded=True)]
        combat.draw_pile = [first, second]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.DOOM) == 16
        assert first in combat.hand
        assert second in combat.hand

    def test_shared_fate_lowers_owner_and_target_strength_and_exhausts(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        card = make_shared_fate(upgraded=True)
        combat.hand = [card]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == -2
        assert enemy.get_power_amount(PowerId.STRENGTH) == -3
        assert card in combat.exhaust_pile

    def test_squeeze_counts_other_osty_attack_cards_across_combat_piles(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.summon_osty(combat.player, 5)
        hand_osty_attack = make_poke()
        draw_osty_attack = make_unleash()
        discard_osty_attack = make_flatten()
        non_osty_attack = make_defend_necrobinder()
        combat.hand = [make_squeeze(), hand_osty_attack]
        combat.draw_pile = [draw_osty_attack, non_osty_attack]
        combat.discard_pile = [discard_osty_attack]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 60
        assert combat.count_powered_hits_by_dealer_this_turn(combat.get_osty(combat.player)) == 1

    def test_squeeze_is_playable_without_osty_but_has_no_effect(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        card = make_squeeze()
        combat.hand = [card]
        combat.energy = 3

        assert combat.can_play_card(card) is True
        assert combat.play_card(0, 0)
        assert enemy.current_hp == 100
        assert combat.energy == 0
        assert card in combat.discard_pile

    def test_high_five_is_not_playable_without_osty(self):
        combat = _make_combat()
        card = make_high_five()
        combat.hand = [card]
        combat.energy = 2

        assert combat.can_play_card(card) is False
        assert combat.play_card(0, 0) is False
        assert combat.energy == 2
        assert combat.hand == [card]

    def test_times_up_damage_scales_with_target_doom(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.apply_power_to(enemy, PowerId.DOOM, 7, applier=combat.player)
        combat.hand = [make_times_up()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 93

    def test_upgraded_times_up_adds_retain_only(self):
        card = make_times_up(upgraded=True)

        assert card.cost == 2
        assert card.base_damage == 0
        assert card.effect_vars["extra_damage"] == 1
        assert card.is_retain
        assert card.exhausts

    def test_blight_strike_applies_doom_equal_to_damage_dealt(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        combat.hand = [make_blight_strike()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 92
        assert enemy.get_power_amount(PowerId.DOOM) == 8

    def test_call_of_the_void_generates_ethereal_cards_next_turn_start(self):
        combat = _make_combat()
        combat.hand = [make_call_of_the_void()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.CALL_OF_THE_VOID) == 1

        combat.end_player_turn()
        generated = [card for card in combat.hand if card.is_ethereal]
        assert generated
        assert len({card.card_id for card in generated}) == len(generated)
        assert all(card.rarity.name not in {"BASIC", "ANCIENT"} for card in generated)

    def test_banshees_cry_hits_only_hittable_enemies(self):
        """Matches BansheesCry.cs: base cost 9, DamageVar(33, Move), AllEnemies."""
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_banshees_cry()]
        combat.energy = 9

        assert combat.play_card(0)

        assert blocked.current_hp == 100
        assert hittable.current_hp == 67

    def test_deathbringer_applies_doom_and_weak_to_all_enemies(self):
        combat = _make_combat()
        extra_enemy, extra_ai = create_shrinker_beetle(Rng(9091))
        combat.add_enemy(extra_enemy, extra_ai)
        combat.hand = [make_deathbringer()]
        combat.energy = 2

        assert combat.play_card(0)
        for enemy in combat.enemies:
            assert enemy.get_power_amount(PowerId.DOOM) == 21
            assert enemy.get_power_amount(PowerId.WEAK) == 1

    def test_deaths_door_repeats_after_owner_applies_doom_this_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.apply_power_to(enemy, PowerId.DOOM, 1, applier=combat.player)
        combat.hand = [make_deaths_door()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == DEATHS_DOOR_BLOCK_GAIN * (1 + DEATHS_DOOR_REPEAT)

    def test_deaths_door_factory_exposes_reference_repeat_value(self):
        card = make_deaths_door()

        assert card.effect_vars["repeat"] == DEATHS_DOOR_REPEAT

    def test_deaths_door_does_not_repeat_for_non_owner_doom_applier(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.apply_power_to(combat.player, PowerId.DOOM, 1, applier=enemy)
        combat.hand = [make_deaths_door()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == DEATHS_DOOR_BLOCK_GAIN

    def test_deaths_door_stops_repeated_block_after_combat_ends(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 5
        combat.apply_power_to(enemy, PowerId.DOOM, 1, applier=combat.player)
        combat.player.apply_power(PowerId.JUGGERNAUT, 5)
        combat.hand = [make_deaths_door()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.is_over
        assert combat.player.block == DEATHS_DOOR_BLOCK_GAIN

    def test_enfeebling_touch_applies_temporary_strength_loss_then_restores(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.apply_power(PowerId.STRENGTH, 3)
        combat.hand = [make_enfeebling_touch(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.ENFEEBLING_TOUCH) == 11
        assert enemy.get_power_amount(PowerId.STRENGTH) == -8

        fire_after_turn_end(CombatSide.ENEMY, combat)

        assert enemy.get_power_amount(PowerId.ENFEEBLING_TOUCH) == 0
        assert enemy.get_power_amount(PowerId.STRENGTH) == 3

    def test_negative_pulse_debuffs_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_negative_pulse()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.block == 5
        assert blocked.get_power_amount(PowerId.DOOM) == 0
        assert hittable.get_power_amount(PowerId.DOOM) == 7

    def test_deathbringer_debuffs_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_deathbringer()]
        combat.energy = 2

        assert combat.play_card(0)

        assert blocked.get_power_amount(PowerId.DOOM) == 0
        assert blocked.get_power_amount(PowerId.WEAK) == 0
        assert hittable.get_power_amount(PowerId.DOOM) == 21
        assert hittable.get_power_amount(PowerId.WEAK) == 1

    def test_high_five_hits_and_debuffs_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.summon_osty(combat.player, 5)
        combat.hand = [make_high_five()]
        combat.energy = 2

        assert combat.play_card(0)

        assert blocked.current_hp == 100
        assert blocked.get_power_amount(PowerId.VULNERABLE) == 0
        assert hittable.current_hp == 89
        assert hittable.get_power_amount(PowerId.VULNERABLE) == 2

    def test_end_of_days_doom_kills_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 20
        hittable.current_hp = hittable.max_hp = 20
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_end_of_days()]
        combat.energy = 3

        assert combat.play_card(0)

        assert blocked.is_alive
        assert blocked.get_power_amount(PowerId.DOOM) == 0
        assert hittable.is_dead

    def test_countdown_doom_uses_owner_applier_for_shroud(self):
        combat = _make_combat()
        player = combat.player
        enemy = combat.enemies[0]
        player.apply_power(PowerId.SHROUD, 4)
        player.apply_power(PowerId.COUNTDOWN, 5)

        fire_after_side_turn_start(CombatSide.PLAYER, combat)

        assert enemy.get_power_amount(PowerId.DOOM) == 5
        assert player.block == 4

    def test_countdown_random_target_uses_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        hittable, blocked = combat.enemies
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.apply_power_to(combat.player, PowerId.COUNTDOWN, 5)

        fire_after_side_turn_start(CombatSide.PLAYER, combat)

        assert blocked.get_power_amount(PowerId.DOOM) == 0
        assert hittable.get_power_amount(PowerId.DOOM) == 5

    def test_neurosurge_doom_uses_owner_applier_for_shroud(self):
        combat = _make_combat()
        player = combat.player
        player.apply_power(PowerId.SHROUD, 4)
        player.apply_power(PowerId.NEUROSURGE, 3)

        fire_after_side_turn_start(CombatSide.PLAYER, combat)

        assert player.get_power_amount(PowerId.DOOM) == 3
        assert player.block == 4

    def test_neurosurge_gains_energy_before_drawing_void(self):
        """Matches Neurosurge.cs: gain energy before Draw, so drawn Void removes one."""
        combat = _make_combat()
        combat.hand = [make_neurosurge()]
        combat.draw_pile = [make_void()]
        combat.energy = 0

        assert combat.play_card(0)

        assert combat.energy == 2
        assert combat.player.get_power_amount(PowerId.NEUROSURGE) == NEUROSURGE_POWER_AMOUNT

    def test_neurosurge_upgrade_keeps_power_amount_and_increases_energy(self):
        """Matches Neurosurge.cs: upgrade changes Energy only, not NeurosurgePower."""
        combat = _make_combat()
        combat.hand = [make_neurosurge(upgraded=True)]
        combat.draw_pile = []
        combat.energy = 0

        assert combat.play_card(0)

        assert combat.energy == NEUROSURGE_UPGRADED_ENERGY
        assert combat.player.get_power_amount(PowerId.NEUROSURGE) == NEUROSURGE_POWER_AMOUNT

    def test_reaper_form_doom_uses_owner_applier_for_shroud(self):
        combat = _make_combat()
        player = combat.player
        enemy = combat.enemies[0]
        player.apply_power(PowerId.SHROUD, 2)
        player.apply_power(PowerId.REAPER_FORM, 1)

        combat.deal_damage(dealer=player, target=enemy, amount=5, props=ValueProp.MOVE)

        assert enemy.get_power_amount(PowerId.DOOM) == 5
        assert player.block == 2

    def test_reaper_form_doom_counts_blocked_damage(self):
        combat = _make_combat()
        player = combat.player
        enemy = combat.enemies[0]
        enemy.block = 3
        player.apply_power(PowerId.REAPER_FORM, 2)

        combat.deal_damage(dealer=player, target=enemy, amount=5, props=ValueProp.MOVE)

        assert enemy.get_power_amount(PowerId.DOOM) == 10

    def test_reaper_form_doom_applies_from_owner_osty_damage(self):
        combat = _make_combat()
        player = combat.player
        enemy = combat.enemies[0]
        osty = combat.summon_osty(player, 5)
        assert osty is not None
        player.apply_power(PowerId.REAPER_FORM, 1)

        combat.deal_damage(dealer=osty, target=enemy, amount=4, props=ValueProp.MOVE)

        assert enemy.get_power_amount(PowerId.DOOM) == 4

    def test_necro_mastery_damage_hits_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        combat.apply_power_to(combat.player, PowerId.NECRO_MASTERY, 2)

        combat.deal_damage(dealer=blocked, target=osty, amount=3, props=ValueProp.MOVE)

        assert blocked.current_hp == 100
        assert hittable.current_hp == 94

    def test_necro_mastery_triggers_when_osty_is_killed_by_hit(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        combat.apply_power_to(combat.player, PowerId.NECRO_MASTERY, 2)

        combat.deal_damage(dealer=blocked, target=osty, amount=5, props=ValueProp.MOVE)

        assert osty.is_dead
        assert blocked.current_hp == 100
        assert hittable.current_hp == 90

    def test_necro_mastery_triggers_when_osty_is_directly_killed(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        combat.apply_power_to(combat.player, PowerId.NECRO_MASTERY, 2)

        assert combat.kill_osty(combat.player)

        assert osty.is_dead
        assert blocked.current_hp == 100
        assert hittable.current_hp == 90

    def test_oblivion_records_cards_before_triggering_doom(self):
        combat = _make_combat()
        player = combat.player
        enemy = combat.enemies[0]
        player.apply_power(PowerId.SHROUD, 2)
        combat.hand = [create_card(CardId.OBLIVION)]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.OBLIVION) == 3
        assert enemy.get_power_amount(PowerId.DOOM) == 0
        assert player.block == 0

        combat.hand = [make_soul()]
        combat.draw_pile = []

        assert combat.play_card(0)
        assert enemy.get_power_amount(PowerId.DOOM) == 3
        assert player.block == 2

    def test_oblivion_only_records_cards_from_applier_player(self):
        combat = _make_combat()
        player = combat.player
        enemy = combat.enemies[0]
        ally = Creature(max_hp=60, current_hp=60, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None

        combat.apply_power_to(enemy, PowerId.OBLIVION, 3, applier=player)
        ally_soul = make_soul()
        ally_soul.owner = ally
        ally_state.hand = [ally_soul]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1

        assert combat.play_card_from_creature(ally, 0)
        assert enemy.get_power_amount(PowerId.DOOM) == 0

        player_soul = make_soul()
        combat.hand = [player_soul]
        combat.energy = 1

        assert combat.play_card(0)
        assert enemy.get_power_amount(PowerId.DOOM) == 3

    def test_oblivion_is_removed_at_player_turn_end(self):
        combat = _make_combat()
        player = combat.player
        enemy = combat.enemies[0]

        combat.apply_power_to(enemy, PowerId.OBLIVION, 3, applier=player)

        fire_after_turn_end(CombatSide.PLAYER, combat)

        assert enemy.get_power_amount(PowerId.OBLIVION) == 0
        assert PowerId.OBLIVION not in enemy.powers

    def test_pagestorm_draws_extra_when_ethereal_card_is_drawn(self):
        combat = _make_combat()
        soul = make_soul()
        soul.keywords = frozenset(set(soul.keywords) | {"ethereal"})
        extra = make_defend_necrobinder()
        combat.hand = [make_pagestorm()]
        combat.draw_pile = [soul, extra]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.PAGESTORM) == 1

        combat.draw_cards(combat.player, 1)
        assert soul in combat.hand
        assert extra in combat.hand

    def test_pull_from_below_counts_ethereal_cards_played_this_combat(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        ethereal = make_defend_necrobinder()
        ethereal.keywords = frozenset(set(ethereal.keywords) | {"ethereal"})
        combat.hand = [ethereal]
        combat.energy = 1

        assert combat.play_card(0)

        combat.hand = [make_pull_from_below()]
        combat.energy = 1
        assert combat.play_card(0, 0)
        assert enemy.current_hp == 95

    def test_pull_from_below_counts_card_that_was_ethereal_when_played(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = REFERENCE_ENEMY_HP
        ethereal_by_hex = make_defend_necrobinder()
        combat.hand = [ethereal_by_hex]
        combat.energy = 1
        combat.apply_power_to(combat.player, PowerId.HEX, 1, applier=enemy)

        assert ethereal_by_hex.is_ethereal
        assert combat.play_card(0)

        combat._remove_power(combat.player, PowerId.HEX)
        assert not ethereal_by_hex.is_ethereal

        combat.hand = [make_pull_from_below()]
        combat.energy = 1
        assert combat.play_card(0, 0)
        assert enemy.current_hp == REFERENCE_ENEMY_HP - 5

    def test_sculpting_strike_attacks_then_makes_selected_hand_card_ethereal(self):
        """Matches SculptingStrike.cs: DamageVar(9, Move) attack, then a chosen
        non-Ethereal hand card gains Ethereal."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        target_card = make_defend_necrobinder()
        combat.hand = [make_sculpting_strike(), target_card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 9
        assert combat.pending_choice is None
        assert target_card.is_ethereal

    def test_sentry_mode_generates_sweeping_gaze_next_turn(self):
        combat = _make_combat()
        combat.hand = [make_sentry_mode()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.SENTRY_MODE) == SENTRY_MODE_POWER_AMOUNT

        combat.end_player_turn()
        assert any(card.card_id == CardId.SWEEPING_GAZE for card in combat.hand)

    def test_sentry_mode_plus_costs_one_and_keeps_power_amount(self):
        """Matches SentryMode.cs: upgrade changes cost only."""
        combat = _make_combat()
        combat.hand = [make_sentry_mode(upgraded=True)]
        combat.energy = 1

        assert combat.hand[0].cost == SENTRY_MODE_UPGRADED_COST
        assert combat.play_card(0)

        assert combat.energy == 0
        assert combat.player.get_power_amount(PowerId.SENTRY_MODE) == SENTRY_MODE_POWER_AMOUNT

    def test_unleash_uses_osty_current_hp_in_damage_formula(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_bodyguard(), make_unleash()]
        combat.energy = 2

        assert combat.play_card(0)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        osty.current_hp = 4
        starting_hp = enemy.current_hp

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 10

    def test_upgraded_invoke_applies_upgraded_summon_and_energy_next_turn(self):
        combat = _make_combat()
        combat.hand = [make_invoke(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.SUMMON_NEXT_TURN) == 3
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 3

    def test_poke_uses_osty_damage_and_records_osty_dealer(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        starting_hp = enemy.current_hp
        combat.hand = [make_poke(upgraded=True)]

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 9
        assert combat.count_powered_hits_by_dealer_this_turn(osty) == 1
        assert combat.count_powered_hits_by_dealer_this_turn(combat.player) == 0

    def test_fetch_uses_osty_damage_and_records_osty_dealer(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        starting_hp = enemy.current_hp
        combat.hand = [make_fetch(upgraded=True)]
        combat.draw_pile = [make_soul()]

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 6
        assert combat.count_powered_hits_by_dealer_this_turn(osty) == 1

    def test_friendship_lowers_strength_and_grants_friendship_power(self):
        combat = _make_combat()
        combat.hand = [make_friendship()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == -2
        assert combat.player.get_power_amount(PowerId.FRIENDSHIP) == 1

    def test_upgraded_friendship_lowers_strength_by_reduced_amount(self):
        combat = _make_combat()
        combat.hand = [make_friendship(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == -1
        assert combat.player.get_power_amount(PowerId.FRIENDSHIP) == 1

    def test_hang_applies_two_stacks_on_first_play(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_hang()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 90
        assert enemy.get_power_amount(PowerId.HANG) == 2

    def test_hang_damage_and_stacks_scale_with_existing_hang(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.apply_power_to(enemy, PowerId.HANG, 2, applier=combat.player)
        combat.hand = [make_hang()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 80
        assert enemy.get_power_amount(PowerId.HANG) == 4

    def test_hang_stack_gain_caps_at_999(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 10000
        combat.apply_power_to(enemy, PowerId.HANG, 998, applier=combat.player)
        combat.hand = [make_hang()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.HANG) == HANG_STACK_CAP

    def test_misery_copies_original_target_debuffs_to_other_hittable_enemies(self):
        combat = _make_combat(extra_enemies=2)
        target, other, blocked = combat.enemies
        target.current_hp = target.max_hp = 100
        other.current_hp = other.max_hp = 100
        blocked.current_hp = blocked.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.apply_power_to(target, PowerId.WEAK, 2, applier=combat.player)
        combat.apply_power_to(target, PowerId.DOOM, 5, applier=combat.player)
        combat.hand = [make_misery()]
        combat.energy = 0

        assert combat.play_card(0, 0)

        assert target.current_hp == 93
        assert other.get_power_amount(PowerId.WEAK) == 2
        assert other.get_power_amount(PowerId.DOOM) == 5
        assert blocked.get_power_amount(PowerId.WEAK) == 0
        assert blocked.get_power_amount(PowerId.DOOM) == 0

    def test_misery_does_not_copy_debuffs_added_by_its_own_damage(self):
        combat = _make_combat(extra_enemies=1)
        target, other = combat.enemies
        target.current_hp = target.max_hp = 100
        other.current_hp = other.max_hp = 100
        combat.apply_power_to(combat.player, PowerId.ENVENOM, 3)
        combat.hand = [make_misery()]
        combat.energy = 0

        assert combat.play_card(0, 0)

        assert target.get_power_amount(PowerId.POISON) == 3
        assert other.get_power_amount(PowerId.POISON) == 0

    def test_misery_copies_temporary_debuff_without_double_copying_internal_strength(self):
        combat = _make_combat(extra_enemies=1)
        target, other = combat.enemies
        target.current_hp = target.max_hp = 100
        other.current_hp = other.max_hp = 100
        combat.apply_power_to(target, PowerId.MANGLE, 3, applier=combat.player)
        combat.hand = [make_misery()]
        combat.energy = 0

        assert target.get_power_amount(PowerId.MANGLE) == 3
        assert target.get_power_amount(PowerId.STRENGTH) == -3

        assert combat.play_card(0, 0)

        assert other.get_power_amount(PowerId.MANGLE) == 3
        assert other.get_power_amount(PowerId.STRENGTH) == -3

    def test_sic_em_uses_osty_damage_and_power_values(self):
        """Matches SicEm.cs: OstyDamageVar(5) +1 upgraded (6 damage);
        PowerVar<SicEmPower>(3) +1 upgraded (4 stacks)."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        starting_hp = enemy.current_hp
        combat.hand = [make_sic_em(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 6
        assert combat.count_powered_hits_by_dealer_this_turn(osty) == 1
        assert enemy.get_power_amount(PowerId.SIC_EM) == 4

    def test_sic_em_applies_power_even_without_osty(self):
        """SicEm.cs OnPlay: the Osty DamageCmd is gated on
        Osty.CheckMissingWithAnim, but PowerCmd.Apply<SicEmPower> runs
        unconditionally — with no Osty the target still gets 3 stacks and
        takes no Osty damage."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        assert combat.get_osty(combat.player) is None
        starting_hp = enemy.current_hp
        combat.hand = [make_sic_em()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp
        assert enemy.get_power_amount(PowerId.SIC_EM) == 3

    def test_snap_grants_retain_without_osty(self):
        """Snap.cs OnPlay: after the (Osty-gated) attack, CardSelectCmd.FromHand
        filtered to non-Retain cards + CardCmd.ApplyKeyword(CardKeyword.Retain)
        runs unconditionally — no Osty required (Snap.cs:36-44)."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        assert combat.get_osty(combat.player) is None
        target_card = make_strike_necrobinder()
        other = make_defend_necrobinder()
        already_retain = make_squeeze()
        already_retain.keywords = frozenset(set(already_retain.keywords) | {"retain"})
        combat.hand = [make_snap(), target_card, other, already_retain]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        options = [option.card for option in combat.pending_choice.options]
        assert options == [target_card, other]

        assert combat.resolve_pending_choice(0)
        assert "retain" in target_card.keywords
        assert "retain" not in other.keywords
        # No Osty was summoned, so Snap's Osty attack dealt no damage.
        assert enemy.current_hp == enemy.max_hp

    def test_sic_em_summons_even_when_osty_damage_is_fully_blocked(self):
        combat = _make_combat()
        player = combat.player
        enemy = combat.enemies[0]
        enemy.block = 10
        combat.summon_osty(player, 5)
        osty = combat.get_osty(player)
        assert osty is not None
        combat.apply_power_to(enemy, PowerId.SIC_EM, 3, applier=player)
        starting_max_hp = osty.max_hp

        combat.deal_damage(dealer=osty, target=enemy, amount=2, props=ValueProp.MOVE)

        assert enemy.current_hp == enemy.max_hp
        assert osty.max_hp == starting_max_hp + 3

    def test_veilpiercer_makes_ethereal_cards_cost_zero_then_ticks_down_on_play(self):
        combat = _make_combat()
        ethereal = make_defend_necrobinder()
        ethereal.keywords = frozenset(set(ethereal.keywords) | {"ethereal"})
        combat.hand = [make_veilpiercer(), ethereal]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert combat.player.get_power_amount(PowerId.VEILPIERCER) == 1

        assert combat.can_play_card(ethereal) is True
        assert combat.play_card(0, 0)
        assert combat.player.get_power_amount(PowerId.VEILPIERCER) == 0

    def test_banshees_cry_cost_drops_for_ethereal_cards_played_this_combat(self):
        """Matches BansheesCry.cs: base cost 9, EnergyVar(2) — card cost drops by 2
        for each Ethereal card played this combat (BansheesCryPower semantics)."""
        combat = _make_combat()
        held = make_banshees_cry()
        later = make_banshees_cry()
        combat.hand = [make_fear(), held, make_fear()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert held.cost == 7

        assert combat.play_card(1, 0)
        assert held.cost == 5

        combat.move_card_to_creature_hand(combat.player, later)
        assert later.cost == 5
        held.end_of_turn_cleanup()
        later.end_of_turn_cleanup()
        assert held.cost == 5
        assert later.cost == 5

    def test_banshees_cry_cost_uses_ethereal_state_from_when_card_was_played(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        ethereal_by_hex = make_defend_necrobinder()
        combat.hand = [ethereal_by_hex]
        combat.energy = 1
        combat.apply_power_to(combat.player, PowerId.HEX, 1, applier=enemy)

        assert ethereal_by_hex.is_ethereal
        assert combat.play_card(0)

        combat._remove_power(combat.player, PowerId.HEX)
        assert not ethereal_by_hex.is_ethereal

        watcher = make_banshees_cry()
        combat.move_card_to_creature_hand(combat.player, watcher)
        assert watcher.cost == watcher.original_cost - watcher.effect_vars["energy"]

    def test_flatten_uses_osty_damage_and_makes_other_flatten_free_this_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None
        other_flatten = make_flatten(upgraded=True)
        combat.hand = [make_flatten(), other_flatten]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 88
        assert combat.count_powered_hits_by_dealer_this_turn(osty) == 1
        assert other_flatten.cost == 0

    def test_flatten_entering_after_osty_attack_is_free_this_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        combat.summon_osty(combat.player, 5)
        osty = combat.get_osty(combat.player)
        assert osty is not None

        combat.deal_damage(osty, enemy, 4, ValueProp.MOVE)
        flatten = make_flatten()
        combat.move_card_to_creature_hand(combat.player, flatten)

        assert flatten.cost == 0
