"""Regression tests for foundational parity helpers in CombatState."""

import sts2_env.powers  # noqa: F401
import sts2_env.potions  # noqa: F401

from sts2_env.cards.base import CardInstance
from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.defect import (
    make_charge_battery,
    make_compact,
    make_hologram,
    make_lightning_rod,
    make_signal_boost,
    make_white_noise,
)
from sts2_env.cards.colorless import (
    make_believe_in_you,
    make_gang_up,
    make_gold_axe,
    make_hand_of_greed,
    make_hidden_gem,
    make_lift,
    make_mimic,
    make_mind_blast,
    make_omnislice,
    make_restlessness,
    make_volley,
)
from sts2_env.cards.colorless import make_fisticuffs
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad import (
    make_armaments,
    make_brand,
    make_thunderclap,
    make_burning_pact,
    make_demonic_shield,
    make_dismantle,
    make_feed,
    make_headbutt,
    make_juggling,
    make_primal_force,
    make_stoke,
    make_tear_asunder,
    make_true_grit,
)
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.ironclad_basic import make_bash, make_defend_ironclad
from sts2_env.cards.necrobinder import (
    make_afterlife,
    make_bodyguard,
    make_capture_spirit,
    make_cleanse,
    make_drain_power,
    make_dredge,
    make_dirge,
    make_end_of_days,
    make_grave_warden,
    make_graveblast,
    make_necro_mastery,
    make_pull_aggro,
    make_right_hand_hand,
    make_reave,
    make_reanimate,
    make_sacrifice,
    make_sculpting_strike,
    make_seance,
    make_severance,
    make_spur,
    make_undeath,
    make_legion_of_bone,
)
from sts2_env.cards.necrobinder import create_necrobinder_starter_deck
from sts2_env.cards.regent import (
    create_regent_starter_deck,
    make_begone,
    make_beat_into_shape,
    make_bulwark,
    make_charge,
    make_collision_course,
    make_convergence,
    make_conqueror,
    make_decisions_decisions,
    make_bundle_of_joy,
    make_crash_landing,
    make_cosmic_indifference,
    make_gather_light,
    make_glow,
    make_guards,
    make_hidden_cache,
    make_manifest_authority,
    make_make_it_so,
    make_photon_cut,
    make_quasar,
    make_refine_blade,
    make_resonance,
    make_seeking_edge,
    make_solar_strike,
    make_spoils_of_battle,
    make_summon_forth,
    make_the_smith,
    make_venerate,
    make_heirloom_hammer,
    make_void_form,
    make_wrought_in_war,
)
from sts2_env.cards.silent import (
    make_accelerant,
    make_dodge_and_roll,
    make_hand_trick,
    make_master_planner,
    make_nightmare,
    make_shadow_step,
    make_survivor,
    make_the_hunt,
    make_tools_of_the_trade,
)
from sts2_env.cards.status import (
    make_apotheosis,
    make_beckon,
    make_brightest_flame,
    make_distraction,
    make_dual_wield,
    make_enlightenment,
    make_metamorphosis,
    make_stack,
    make_wish,
)
from sts2_env.cards.status import (
    make_frantic_escape,
    make_fuel,
    make_shiv,
    make_minion_dive_bomb,
    make_minion_sacrifice,
    make_minion_strike,
    make_slimed,
    make_soul,
    make_sovereign_blade,
)
from sts2_env.characters.all import get_character
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CardRarity, CardType, CombatSide, PowerId, TargetType, ValueProp
from sts2_env.gym_env.action_space import get_action_mask
from sts2_env.core.rng import Rng
from sts2_env.monsters.act3 import create_soul_nexus
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.potions.base import create_potion
from sts2_env.powers.base import PowerInstance
from sts2_env.powers.monster import CrabRagePower, DoorRevivalPower, IllusionPower, RavenousPower
from sts2_env.powers.monster import DoorRevivalPower, SurprisePower


def _make_combat(deck, character_id: str) -> CombatState:
    rng = Rng(42)
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=deck,
        rng_seed=42,
        character_id=character_id,
    )
    creature, ai = create_shrinker_beetle(rng)
    combat.add_enemy(creature, ai)
    return combat


class TestGeneratedCards:
    def test_hello_world_generates_common_card_into_hand(self):
        combat = _make_combat(create_defect_starter_deck(), "Defect")
        combat.player.apply_power(PowerId.HELLO_WORLD, 1)

        combat.start_combat()

        defect_pool = set(get_character("Defect").card_pool)
        generated = [card for card in combat.hand if card.rarity == CardRarity.COMMON]
        assert len(combat.hand) == 6
        assert generated
        assert all(card.card_id in defect_pool for card in generated)

    def test_creative_ai_generates_power_card_into_hand(self):
        combat = _make_combat(create_defect_starter_deck(), "Defect")
        combat.player.apply_power(PowerId.CREATIVE_AI, 1)

        combat.start_combat()

        generated = [card for card in combat.hand if card.card_type == CardType.POWER]
        assert generated
        assert all(card.card_id in set(get_character("Defect").card_pool) for card in generated)

    def test_channel_orb_evoke_on_overflow_and_trigger_first_passive(self):
        combat = _make_combat(create_defect_starter_deck(), "Defect")
        combat.orb_queue.capacity = 1
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp

        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "FROST")

        assert enemy.current_hp == starting_hp - 8
        assert len(combat.orb_queue.orbs) == 1

        starting_block = combat.player.block
        combat.trigger_first_orb_passive(combat.player)
        assert combat.player.block == starting_block + 2

    def test_generate_ethereal_cards_uses_character_pool(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        combat.hand.clear()

        combat.generate_ethereal_cards(combat.player, 2)

        assert len(combat.hand) == 2
        assert all(card.is_ethereal for card in combat.hand)
        assert len({card.card_id for card in combat.hand}) == 2

    def test_philosophers_stone_buffs_enemies_added_mid_combat(self):
        from sts2_env.relics.registry import create_relic_by_name

        rng = Rng(42)
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=42,
            character_id="Ironclad",
            relics=[create_relic_by_name("PHILOSOPHERS_STONE")],
        )
        first_enemy, first_ai = create_shrinker_beetle(rng)
        combat.add_enemy(first_enemy, first_ai)
        combat.start_combat()

        spawned_enemy, spawned_ai = create_shrinker_beetle(rng)
        combat.add_enemy(spawned_enemy, spawned_ai)

        assert first_enemy.get_power_amount(PowerId.STRENGTH) == 1
        assert spawned_enemy.get_power_amount(PowerId.STRENGTH) == 1

    def test_philosophers_stone_buffs_enemy_spawned_by_real_monster_move(self):
        from sts2_env.relics.registry import create_relic_by_name

        rng = Rng(42)
        soul_nexus, soul_nexus_ai = create_soul_nexus(rng)
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=42,
            character_id="Ironclad",
            relics=[create_relic_by_name("PHILOSOPHERS_STONE")],
        )
        combat.add_enemy(soul_nexus, soul_nexus_ai)
        combat.start_combat()

        soul_nexus_ai.current_move.perform(combat)
        soul_nexus_ai.on_move_performed()

        assert len(combat.enemies) == 2
        spawned_enemy = combat.enemies[1]
        assert spawned_enemy.get_power_amount(PowerId.STRENGTH) == 1

    def test_add_ally_player_runtime_triggers_after_creature_added_hook(self):
        class AddedWatcher(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.VIGOR, 1)
                self.seen: list[Creature] = []

            def after_creature_added_to_combat(
                self,
                owner: Creature,
                creature: Creature,
                combat: CombatState,
            ) -> None:
                self.seen.append(creature)

        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        watcher = AddedWatcher()
        combat.player.powers[watcher.power_id] = watcher
        combat.start_combat()

        ally = Creature(max_hp=20, current_hp=20, side=CombatSide.PLAYER, is_player=True, monster_id="ALLY")
        added_ally = combat.add_ally_player(ally)

        assert added_ally is ally
        assert ally in combat.allies
        assert watcher.seen == [ally]

    def test_add_ally_player_before_combat_start_does_not_trigger_runtime_hook(self):
        class AddedWatcher(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.VIGOR, 1)
                self.seen: list[Creature] = []

            def after_creature_added_to_combat(
                self,
                owner: Creature,
                creature: Creature,
                combat: CombatState,
            ) -> None:
                self.seen.append(creature)

        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        watcher = AddedWatcher()
        combat.player.powers[watcher.power_id] = watcher
        ally = Creature(max_hp=20, current_hp=20, side=CombatSide.PLAYER, is_player=True, monster_id="ALLY")

        added_ally = combat.add_ally_player(ally)
        assert added_ally is ally
        assert watcher.seen == []

        combat.start_combat()
        assert combat.add_ally_player(ally) is ally
        assert watcher.seen == []


class TestAutoPlayFromDraw:
    def test_auto_play_from_draw_plays_top_card_and_fires_hooks(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.start_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp

        combat.hand.clear()
        combat.discard_pile.clear()
        combat.draw_pile = [make_strike_ironclad()]
        combat.energy = 0
        combat.player.apply_power(PowerId.RAGE, 3)

        combat.auto_play_from_draw(combat.player, 1)

        assert enemy.current_hp == starting_hp - 6
        assert combat.player.block == 3
        assert combat.energy == 0
        assert not combat.draw_pile
        assert len(combat.discard_pile) == 1


class TestOstySummon:
    def test_summon_osty_creates_and_scales_pet(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")

        combat.summon_osty(combat.player, 5)

        assert combat.osty is not None
        assert combat.osty in combat.allies
        assert combat.osty.is_pet
        assert combat.osty.is_osty
        assert combat.osty.pet_owner is combat.player
        assert combat.osty.owner is combat.player
        assert combat.osty.current_hp == 5
        assert combat.osty.max_hp == 5
        assert combat.osty.has_power(PowerId.DIE_FOR_YOU)

        combat.summon_osty(combat.player, 3)
        assert combat.osty.current_hp == 8
        assert combat.osty.max_hp == 8

    def test_summon_osty_revives_dead_pet(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        combat.summon_osty(combat.player, 5)
        assert combat.osty is not None

        combat.osty.current_hp = 0
        combat.summon_osty(combat.player, 4)

        assert combat.osty.current_hp == 4
        assert combat.osty.max_hp == 4
        assert combat.osty.is_alive

    def test_summon_osty_uses_summon_and_revive_hooks(self):
        class SummonWatcher(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.VIGOR, 1)
                self.summoned_amounts: list[int] = []
                self.revives: int = 0

            def modify_summon_amount(
                self,
                owner: Creature,
                summoner: Creature,
                amount: int,
                source: object | None,
                combat: CombatState,
            ) -> int:
                return amount + 2

            def after_summon(
                self,
                owner: Creature,
                summoner: Creature,
                amount: int,
                combat: CombatState,
            ) -> None:
                self.summoned_amounts.append(amount)

            def after_osty_revived(self, owner: Creature, osty: Creature, combat: CombatState) -> None:
                self.revives += 1

        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        watcher = SummonWatcher()
        combat.player.powers[watcher.power_id] = watcher

        combat.summon_osty(combat.player, 5)
        first_osty = combat.osty
        assert first_osty is not None
        assert first_osty.max_hp == 7
        assert watcher.summoned_amounts == [7]
        assert watcher.revives == 0

        combat.kill_osty(combat.player)
        combat.summon_osty(combat.player, 4)

        assert combat.osty is first_osty
        assert combat.osty.max_hp == 6
        assert combat.osty.current_hp == 6
        assert watcher.summoned_amounts == [7, 6]
        assert watcher.revives == 1


class TestMonsterDeathPrevention:
    def test_surprise_power_blocks_combat_end_when_last_enemy_dies(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        enemy.powers[PowerId.SURPRISE] = SurprisePower()
        combat.start_combat()

        assert combat.kill_creature(enemy)
        assert enemy.current_hp == 0
        assert combat.is_over is False
        assert combat.player_won is False

    def test_door_revival_power_marks_enemy_half_dead_and_blocks_combat_end(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        power = DoorRevivalPower()
        enemy.powers[PowerId.DOOR_REVIVAL] = power
        combat.start_combat()

        assert combat.kill_creature(enemy)
        assert enemy.current_hp == 0
        assert power.is_half_dead is True
        assert combat.is_over is False
        assert combat.player_won is False


class TestMonsterDeathBroadcast:
    def test_ravenous_power_gains_strength_when_ally_dies(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        owner = combat.enemies[0]
        ally, ally_ai = create_twig_slime_s(Rng(99))
        combat.add_enemy(ally, ally_ai)
        owner.powers[PowerId.RAVENOUS] = RavenousPower(3)
        combat.start_combat()

        assert combat.kill_creature(ally)
        assert owner.get_power_amount(PowerId.STRENGTH) == 3

    def test_crab_rage_triggers_once_when_ally_dies(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        owner = combat.enemies[0]
        ally, ally_ai = create_twig_slime_s(Rng(100))
        combat.add_enemy(ally, ally_ai)
        owner.powers[PowerId.CRAB_RAGE] = CrabRagePower()
        combat.start_combat()

        assert combat.kill_creature(ally)
        assert owner.get_power_amount(PowerId.STRENGTH) == 5
        assert owner.block == 99
        assert PowerId.CRAB_RAGE not in owner.powers


class TestUntargetableReviveStates:
    def test_reviving_illusion_enemy_is_not_hittable_or_targetable(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        power = IllusionPower()
        power.is_reviving = True
        enemy.powers[PowerId.ILLUSION] = power
        combat.start_combat()

        strike = make_headbutt()
        strike.owner = combat.player

        assert enemy not in combat.hittable_enemies
        assert combat._resolve_target(strike, 0) is None  # noqa: SLF001

    def test_half_dead_enemy_ignores_damage(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        power = DoorRevivalPower()
        power.is_half_dead = True
        enemy.powers[PowerId.DOOR_REVIVAL] = power
        combat.start_combat()

        hp_before = enemy.current_hp
        result = combat.deal_damage(combat.player, enemy, 10, ValueProp.MOVE)
        assert result
        assert result[0].hp_lost == 0
        assert enemy.current_hp == hp_before

    def test_summon_osty_respects_zero_modified_amount(self):
        class NullSummon(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.ACCURACY, 1)
                self.after_summon_calls = 0

            def modify_summon_amount(
                self,
                owner: Creature,
                summoner: Creature,
                amount: int,
                source: object | None,
                combat: CombatState,
            ) -> int:
                return 0

            def after_summon(
                self,
                owner: Creature,
                summoner: Creature,
                amount: int,
                combat: CombatState,
            ) -> None:
                self.after_summon_calls += 1

        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        blocker = NullSummon()
        combat.player.powers[blocker.power_id] = blocker

        combat.summon_osty(combat.player, 5)

        assert combat.osty is None
        assert blocker.after_summon_calls == 0

    def test_fresh_osty_creation_fires_add_hook_before_final_stats(self):
        class OstyWatcher(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.VIGOR, 1)
                self.add_snapshot: tuple[int, int, bool] | None = None
                self.summon_snapshot: tuple[int, int, bool] | None = None

            def after_creature_added_to_combat(
                self,
                owner: Creature,
                creature: Creature,
                combat: CombatState,
            ) -> None:
                if getattr(creature, "is_osty", False):
                    self.add_snapshot = (
                        creature.max_hp,
                        creature.current_hp,
                        creature.has_power(PowerId.DIE_FOR_YOU),
                    )

            def after_summon(
                self,
                owner: Creature,
                summoner: Creature,
                amount: int,
                combat: CombatState,
            ) -> None:
                if combat.osty is not None:
                    self.summon_snapshot = (
                        combat.osty.max_hp,
                        combat.osty.current_hp,
                        combat.osty.has_power(PowerId.DIE_FOR_YOU),
                    )

        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        watcher = OstyWatcher()
        combat.player.powers[watcher.power_id] = watcher

        combat.summon_osty(combat.player, 5)

        assert watcher.add_snapshot == (1, 1, False)
        assert watcher.summon_snapshot == (5, 5, True)


class TestDamageModifierParity:
    def test_vigor_consumes_after_the_tracked_attack(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.hand = [make_strike_ironclad(), make_strike_ironclad()]
        combat.energy = 2
        enemy = combat.enemies[0]
        combat.player.apply_power(PowerId.VIGOR, 5)

        start_hp = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - 11
        assert combat.player.get_power_amount(PowerId.VIGOR) == 0

        second_start = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == second_start - 6

    def test_vigor_applies_to_all_targets_in_a_single_attack_command(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=42,
            character_id="Ironclad",
        )
        rng = Rng(42)
        for _ in range(2):
            creature, ai = create_shrinker_beetle(rng)
            combat.add_enemy(creature, ai)
        combat.hand = [make_thunderclap()]
        combat.energy = 1
        combat.player.apply_power(PowerId.VIGOR, 5)

        starts = [enemy.current_hp for enemy in combat.enemies]
        assert combat.play_card(0)
        assert [enemy.current_hp for enemy in combat.enemies] == [start - 9 for start in starts]
        assert combat.player.get_power_amount(PowerId.VIGOR) == 0

    def test_accuracy_only_buffs_shivs(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.hand = [make_shiv(), make_strike_ironclad()]
        combat.energy = 2
        enemy = combat.enemies[0]
        combat.player.apply_power(PowerId.ACCURACY, 3)

        shiv_damage = combat.hand[0].base_damage or 0
        strike_damage = combat.hand[1].base_damage or 0
        start_hp = enemy.current_hp

        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - (shiv_damage + 3)

        second_start = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == second_start - strike_damage

    def test_double_damage_requires_a_card_source(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        combat.player.apply_power(PowerId.DOUBLE_DAMAGE, 1)

        start_hp = enemy.current_hp
        combat.deal_damage(dealer=combat.player, target=enemy, amount=6, props=ValueProp.MOVE)
        assert enemy.current_hp == start_hp - 6

        combat.hand = [make_strike_ironclad()]
        combat.energy = 1
        second_start = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == second_start - 12

    def test_thorns_retaliation_uses_full_damage_pipeline(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.hand = [make_strike_ironclad()]
        combat.energy = 1
        enemy = combat.enemies[0]
        enemy.apply_power(PowerId.THORNS, 3)
        combat.player.apply_power(PowerId.INTANGIBLE, 1)

        enemy_start = enemy.current_hp
        player_start = combat.player.current_hp

        assert combat.play_card(0, 0)
        assert enemy.current_hp == enemy_start - 6
        assert combat.player.current_hp == player_start - 1

    def test_omnislice_splash_still_triggers_thorns(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=42,
            character_id="Ironclad",
        )
        rng = Rng(42)
        for _ in range(2):
            creature, ai = create_shrinker_beetle(rng)
            combat.add_enemy(creature, ai)
        primary, secondary = combat.enemies
        secondary.apply_power(PowerId.THORNS, 3)
        combat.hand = [make_omnislice()]
        combat.energy = 0

        primary_start = primary.current_hp
        secondary_start = secondary.current_hp
        player_start = combat.player.current_hp

        assert combat.play_card(0, 0)
        assert primary.current_hp == primary_start - 8
        assert secondary.current_hp == secondary_start - 8
        assert combat.player.current_hp == player_start - 3

    def test_omnislice_splash_uses_overkill_damage(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=42,
            character_id="Ironclad",
        )
        rng = Rng(42)
        for _ in range(2):
            creature, ai = create_shrinker_beetle(rng)
            combat.add_enemy(creature, ai)
        primary, secondary = combat.enemies
        primary.current_hp = 2
        combat.hand = [make_omnislice()]
        combat.energy = 0

        secondary_start = secondary.current_hp

        assert combat.play_card(0, 0)
        assert primary.is_dead
        assert secondary.current_hp == secondary_start - 8

    def test_bone_flute_triggers_when_osty_deals_attack_damage(self):
        from sts2_env.relics.registry import create_relic_by_name

        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_necrobinder_starter_deck(),
            rng_seed=42,
            character_id="Necrobinder",
            relics=[create_relic_by_name("BONE_FLUTE")],
        )
        rng = Rng(42)
        enemy, ai = create_shrinker_beetle(rng)
        combat.add_enemy(enemy, ai)
        combat.summon_osty(combat.player, 5)
        assert combat.osty is not None

        start_block = combat.player.block
        combat.deal_damage(dealer=combat.osty, target=enemy, amount=4, props=ValueProp.MOVE)

        assert combat.player.block == start_block + 2

    def test_bone_flute_triggers_once_per_attack_command(self):
        from sts2_env.relics.registry import create_relic_by_name

        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_necrobinder_starter_deck(),
            rng_seed=42,
            character_id="Necrobinder",
            relics=[create_relic_by_name("BONE_FLUTE")],
        )
        rng = Rng(42)
        for _ in range(2):
            enemy, ai = create_shrinker_beetle(rng)
            combat.add_enemy(enemy, ai)
        combat.summon_osty(combat.player, 5)
        assert combat.osty is not None

        start_block = combat.player.block
        combat.deal_damage(
            dealer=combat.osty,
            amount=4,
            props=ValueProp.MOVE,
            targets=list(combat.alive_enemies),
        )

        assert combat.player.block == start_block + 2

    def test_flame_barrier_only_triggers_on_powered_damage_and_expires_on_enemy_turn_end(self):
        from sts2_env.core.hooks import fire_after_turn_end

        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        combat.player.apply_power(PowerId.FLAME_BARRIER, 4)

        enemy_start = enemy.current_hp
        combat.deal_damage(dealer=enemy, target=combat.player, amount=6, props=ValueProp.UNPOWERED)
        assert enemy.current_hp == enemy_start

        combat.deal_damage(dealer=enemy, target=combat.player, amount=6, props=ValueProp.MOVE)
        assert enemy.current_hp == enemy_start - 4
        assert combat.player.has_power(PowerId.FLAME_BARRIER)

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.has_power(PowerId.FLAME_BARRIER)

        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert not combat.player.has_power(PowerId.FLAME_BARRIER)

    def test_calamity_generates_non_basic_attacks_for_the_owner(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.hand = [make_strike_ironclad()]
        combat.energy = 1
        combat.player.apply_power(PowerId.CALAMITY, 2)

        assert combat.play_card(0, 0)
        assert len(combat.hand) == 2
        assert all(card.card_type == CardType.ATTACK for card in combat.hand)
        assert all(card.rarity not in {CardRarity.BASIC, CardRarity.ANCIENT, CardRarity.EVENT} for card in combat.hand)

    def test_aggression_retrieves_and_upgrades_random_attacks(self):
        from sts2_env.core.hooks import fire_before_side_turn_start

        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        attack_a = make_strike_ironclad()
        attack_b = make_strike_ironclad()
        skill = make_armaments()
        combat.discard_pile = [attack_a, attack_b, skill]
        combat.player.apply_power(PowerId.AGGRESSION, 2)

        fire_before_side_turn_start(CombatSide.PLAYER, combat)

        assert attack_a in combat.hand
        assert attack_b in combat.hand
        assert skill in combat.discard_pile
        assert attack_a.upgraded is True
        assert attack_b.upgraded is True

    def test_phantom_blades_only_buffs_the_first_shiv_played(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.hand = [make_shiv(), make_shiv()]
        combat.energy = 2
        enemy = combat.enemies[0]
        combat.player.apply_power(PowerId.PHANTOM_BLADES, 3)

        start_hp = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - 7

        second_start = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == second_start - 4


class TestResolvedCardStubs:
    def test_venerate_grants_stars(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        combat.hand = [make_venerate()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.stars == 2
        assert combat.player.stars == 2

    def test_solar_strike_deals_damage_and_grants_stars(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        combat.hand = [make_solar_strike()]
        combat.energy = 1
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp

        assert combat.play_card(0, 0)

        assert enemy.current_hp == starting_hp - 8
        assert combat.stars == 1

    def test_bodyguard_summons_osty(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        combat.hand = [make_bodyguard()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 5

    def test_afterlife_summons_stronger_osty(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        combat.hand = [make_afterlife()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 6

    def test_grave_warden_adds_soul_to_draw_pile(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        combat.hand = [make_grave_warden()]
        combat.energy = 1

        assert combat.play_card(0)
        assert any(card.card_id.name == "SOUL" for card in combat.draw_pile)

    def test_gather_light_gains_block_and_star(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        combat.hand = [make_gather_light()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 7
        assert combat.stars == 1

    def test_glow_gains_star_and_draws_cards(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        combat.draw_pile = [make_venerate(), make_gather_light()]
        combat.hand = [make_glow()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.stars == 1
        assert len(combat.hand) == 2


class TestPendingChoiceFlow:
    def test_wish_suspends_card_resolution_until_choice(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        wish = make_wish()
        strike = make_strike_ironclad()
        combat.hand = [wish]
        combat.draw_pile = [strike]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.num_options == 1
        assert wish in combat.play_pile

        mask = get_action_mask(combat)
        assert mask[0] == 0
        assert mask[1] == 1

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None
        assert strike in combat.hand
        assert wish in combat.exhaust_pile

    def test_hologram_uses_pending_discard_choice(self):
        combat = _make_combat(create_defect_starter_deck(), "Defect")
        hologram = make_hologram()
        strike = make_strike_ironclad()
        combat.hand = [hologram]
        combat.discard_pile = [strike]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert hologram in combat.play_pile

        assert combat.resolve_pending_choice(0)
        assert strike in combat.hand

    def test_secret_cards_filter_correct_types(self):
        combat = _make_combat(create_defect_starter_deck(), "Silent")
        technique = CardInstance(
            card_id=CardId.SECRET_TECHNIQUE,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
        )
        weapon = CardInstance(
            card_id=CardId.SECRET_WEAPON,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
        )
        attack = make_strike_ironclad()
        skill = make_gather_light()

        combat.hand = [technique]
        combat.draw_pile = [attack, skill]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert all(option.card.card_type == CardType.SKILL for option in combat.pending_choice.options)
        assert combat.resolve_pending_choice(0)
        assert skill in combat.hand

        combat.hand = [weapon]
        combat.energy = 1
        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert all(option.card.card_type == CardType.ATTACK for option in combat.pending_choice.options)

    def test_dual_wield_copies_selected_card(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        dual_wield = make_dual_wield()
        strike = make_strike_ironclad()
        combat.hand = [dual_wield, strike]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)

        strikes = [card for card in combat.hand if card.card_id == strike.card_id]
        assert len(strikes) == 2

    def test_graveblast_uses_same_choice_mechanism_on_discard(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        graveblast = make_graveblast()
        strike = make_strike_ironclad()
        combat.hand = [graveblast]
        combat.discard_pile = [strike]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert strike in combat.hand

    def test_seeker_strike_reveals_random_draw_subset_for_choice(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        seeker_strike = CardInstance(
            card_id=CardId.SEEKER_STRIKE,
            cost=1,
            card_type=CardType.ATTACK,
            target_type=TargetType.ANY_ENEMY,
            rarity=CardRarity.UNCOMMON,
            base_damage=6,
            effect_vars={"cards": 2},
        )
        card_a = make_strike_ironclad()
        card_b = make_gather_light()
        card_c = make_venerate()
        combat.hand = [seeker_strike]
        combat.draw_pile = [card_a, card_b, card_c]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.num_options == 2
        assert combat.resolve_pending_choice(0)

    def test_discovery_uses_generated_choice_and_zeroes_cost(self):
        combat = _make_combat(create_defect_starter_deck(), "Defect")
        discovery = CardInstance(
            card_id=CardId.DISCOVERY,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.UNCOMMON,
        )
        combat.hand = [discovery]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.num_options == 3
        assert combat.resolve_pending_choice(0)
        assert any(card.cost == 0 for card in combat.hand)

    def test_anointed_moves_all_rare_draw_cards_to_hand(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        anointed = CardInstance(
            card_id=CardId.ANOINTED,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.RARE,
        )
        rare_a = CardInstance(
            card_id=CardId.OFFERING,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.RARE,
        )
        rare_b = CardInstance(
            card_id=CardId.NIGHTMARE,
            cost=3,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.RARE,
        )
        common = make_strike_ironclad()
        combat.hand = [anointed]
        combat.draw_pile = [rare_a, common, rare_b]
        combat.energy = 1

        assert combat.play_card(0)
        assert rare_a in combat.hand
        assert rare_b in combat.hand
        assert common in combat.draw_pile

    def test_begone_transforms_selected_hand_card(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        begone = make_begone()
        strike = make_strike_ironclad()
        combat.hand = [begone, strike]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert any(card.card_id == make_minion_dive_bomb().card_id for card in combat.hand)

    def test_seance_requires_confirm_for_multi_select_transform(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        seance = make_seance()
        seance.effect_vars["cards"] = 2
        card_a = make_strike_ironclad()
        card_b = make_gather_light()
        combat.hand = [seance]
        combat.draw_pile = [card_a, card_b]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi

        mask_before = get_action_mask(combat)
        assert mask_before[0] == 0
        assert mask_before[1] == 1

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is not None
        assert 0 in combat.pending_choice.selected_indices

        mask_mid = get_action_mask(combat)
        assert mask_mid[0] == 0

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is not None
        assert combat.pending_choice.selected_indices == {0, 1}

        mask_after = get_action_mask(combat)
        assert mask_after[0] == 1

        assert combat.resolve_pending_choice(None)
        assert combat.pending_choice is None
        assert any(card.card_id == make_soul().card_id for card in combat.draw_pile)

    def test_compact_transforms_status_cards_into_fuel(self):
        combat = _make_combat(create_defect_starter_deck(), "Defect")
        compact = make_compact()
        slimed = make_slimed()
        strike = make_strike_ironclad()
        combat.hand = [compact, slimed, strike]
        combat.energy = 1

        assert combat.play_card(0)
        assert any(card.card_id == make_fuel().card_id for card in combat.hand)
        assert any(card.card_id == strike.card_id for card in combat.hand)

    def test_photon_cut_puts_selected_hand_card_back_on_draw_pile_top(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        photon_cut = CardInstance(
            card_id=CardId.PHOTON_CUT,
            cost=1,
            card_type=CardType.ATTACK,
            target_type=TargetType.ANY_ENEMY,
            rarity=CardRarity.COMMON,
            base_damage=10,
            effect_vars={"cards": 1},
        )
        keep = make_strike_ironclad()
        combat.hand = [photon_cut, keep]
        combat.draw_pile = [make_gather_light()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.draw_pile[0].card_id == keep.card_id

    def test_shining_strike_returns_self_to_draw_pile_top(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        shining_strike = CardInstance(
            card_id=CardId.SHINING_STRIKE,
            cost=1,
            card_type=CardType.ATTACK,
            target_type=TargetType.ANY_ENEMY,
            rarity=CardRarity.UNCOMMON,
            base_damage=8,
            effect_vars={"stars": 2},
        )
        combat.hand = [shining_strike]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.stars == 2
        assert shining_strike in combat.draw_pile
        assert shining_strike not in combat.discard_pile

    def test_spoils_of_battle_creates_and_forges_sovereign_blade(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        spoils = CardInstance(
            card_id=CardId.SPOILS_OF_BATTLE,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.COMMON,
            effect_vars={"forge": 10},
        )
        combat.hand = [spoils]
        combat.energy = 1

        assert combat.play_card(0)
        blades = [card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE]
        assert blades
        assert blades[0].base_damage == 20

    def test_wrought_in_war_forges_existing_sovereign_blades(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        blade = make_sovereign_blade()
        card = CardInstance(
            card_id=CardId.WROUGHT_IN_WAR,
            cost=1,
            card_type=CardType.ATTACK,
            target_type=TargetType.ANY_ENEMY,
            rarity=CardRarity.COMMON,
            base_damage=7,
            effect_vars={"forge": 5},
        )
        combat.hand = [card, blade]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert blade.base_damage == 15

    def test_big_bang_draws_gains_stars_energy_and_forge(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        big_bang = CardInstance(
            card_id=CardId.BIG_BANG,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.RARE,
            effect_vars={"cards": 1, "energy": 1, "stars": 1, "forge": 5},
        )
        combat.hand = [big_bang]
        combat.draw_pile = [make_gather_light()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.stars == 1
        assert combat.energy == 1
        assert any(card.card_id == CardId.SOVEREIGN_BLADE for card in combat.hand)

    def test_manifest_authority_generates_colorless_card(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        manifest = make_manifest_authority()
        combat.hand = [manifest]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 7
        assert len(combat.hand) == 1

    def test_refine_blade_adds_energy_next_turn_and_forges(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_refine_blade()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.has_power(PowerId.ENERGY_NEXT_TURN)
        assert any(c.card_id == CardId.SOVEREIGN_BLADE for c in combat.hand)

    def test_bulwark_forges_after_block(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_bulwark()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == 13
        assert any(c.card_id == CardId.SOVEREIGN_BLADE for c in combat.hand)

    def test_conqueror_applies_power_and_forges(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_conqueror()
        combat.hand = [card]
        combat.energy = 1
        enemy = combat.enemies[0]

        assert combat.play_card(0, 0)
        assert enemy.has_power(PowerId.CONQUEROR)
        assert any(c.card_id == CardId.SOVEREIGN_BLADE for c in combat.hand)

    def test_seeking_edge_forges_and_changes_sovereign_blade_to_aoe(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        enemy2, ai2 = create_shrinker_beetle(Rng(99))
        combat.add_enemy(enemy2, ai2)
        seeking_edge = make_seeking_edge()
        combat.hand = [seeking_edge]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.has_power(PowerId.SEEKING_EDGE)

        blade = next(card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE)
        hp0 = combat.enemies[0].current_hp
        hp1 = combat.enemies[1].current_hp
        combat.hand = [blade]
        combat.energy = blade.cost

        assert combat.play_card(0, 0)
        assert combat.enemies[0].current_hp < hp0
        assert combat.enemies[1].current_hp < hp1

    def test_the_smith_spends_stars_and_forges(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        smith = make_the_smith()
        combat.hand = [smith]
        combat.energy = 1
        combat.gain_stars(combat.player, 4)

        assert combat.play_card(0)
        assert combat.stars == 0
        blade = next(card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE)
        assert blade.base_damage == 40

    def test_photon_cut_factory_and_effect_request_choice(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        photon = make_photon_cut()
        keep = make_strike_ironclad()
        combat.hand = [photon, keep]
        combat.draw_pile = [make_gather_light()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None

    def test_bundle_of_joy_generates_colorless_cards(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_bundle_of_joy()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert len(combat.hand) == 3

    def test_crash_landing_fills_hand_with_debris(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_crash_landing()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        debris_count = sum(1 for c in combat.hand if c.card_id == CardId.DEBRIS)
        assert debris_count == 10

    def test_charge_transforms_exact_number_of_draw_cards(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_charge()
        card_a = make_strike_ironclad()
        card_b = make_gather_light()
        card_c = make_venerate()
        combat.hand = [card]
        combat.draw_pile = [card_a, card_b, card_c]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi
        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)

        minion_strikes = [c for c in combat.draw_pile if c.card_id == make_minion_strike().card_id]
        assert len(minion_strikes) == 2

    def test_guards_can_transform_any_number_of_hand_cards(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_guards()
        a = make_strike_ironclad()
        b = make_gather_light()
        combat.hand = [card, a, b]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi
        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)

        sacrifices = [c for c in combat.hand if c.card_id == make_minion_sacrifice().card_id]
        assert len(sacrifices) == 2

    def test_distraction_generates_free_skill_in_hand(self):
        combat = _make_combat(create_defect_starter_deck(), "Silent")
        distraction = make_distraction()
        combat.hand = [distraction]
        combat.energy = 1

        assert combat.play_card(0)
        assert any(card.card_type == CardType.SKILL and card.cost == 0 for card in combat.hand)

    def test_white_noise_generates_free_power_in_hand(self):
        combat = _make_combat(create_defect_starter_deck(), "Defect")
        white_noise = make_white_noise()
        combat.hand = [white_noise]
        combat.energy = 1

        assert combat.play_card(0)
        assert any(card.card_type == CardType.POWER and card.cost == 0 for card in combat.hand)

    def test_metamorphosis_adds_zero_cost_attacks_to_draw_pile(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        metamorphosis = make_metamorphosis()
        combat.hand = [metamorphosis]
        combat.energy = 2

        assert combat.play_card(0)
        generated = [card for card in combat.draw_pile if card.cost == 0 and card.card_type == CardType.ATTACK]
        assert len(generated) >= 3

    def test_jack_of_all_trades_generates_colorless_card(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        jack = CardInstance(
            card_id=CardId.JACK_OF_ALL_TRADES,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.UNCOMMON,
            effect_vars={"cards": 1},
        )
        combat.hand = [jack]
        combat.energy = 0

        assert combat.play_card(0)
        assert any(card.card_id != CardId.JACK_OF_ALL_TRADES for card in combat.hand)

    def test_hand_of_greed_grants_gold_on_kill(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_hand_of_greed()
        combat.hand = [card]
        combat.energy = 2
        combat.enemies[0].current_hp = 15
        starting_gold = combat.gold

        assert combat.play_card(0, 0)
        assert combat.gold == starting_gold + 20

    def test_hand_of_greed_and_feed_respect_fatal_eligibility(self):
        greed_combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        greed_enemy = greed_combat.enemies[0]
        greed_enemy.apply_power(PowerId.MINION, 1)
        greed_enemy.current_hp = 10

        hand_of_greed = make_hand_of_greed()
        greed_combat.hand = [hand_of_greed]
        greed_combat.energy = 2
        starting_gold = greed_combat.gold

        assert greed_combat.play_card(0, 0)
        assert greed_combat.gold == starting_gold

        feed_combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        feed_enemy = feed_combat.enemies[0]
        feed_enemy.apply_power(PowerId.MINION, 1)
        feed_enemy.current_hp = 8

        feed = make_feed()
        feed_combat.hand = [feed]
        feed_combat.energy = 1
        starting_max_hp = feed_combat.player.max_hp

        assert feed_combat.play_card(0, 0)
        assert feed_combat.player.max_hp == starting_max_hp

    def test_hidden_gem_adds_replay_to_draw_pile_card(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        hidden_gem = make_hidden_gem()
        strike = make_strike_ironclad()
        combat.hand = [hidden_gem]
        combat.draw_pile = [strike]
        combat.energy = 1

        assert combat.play_card(0)
        assert strike.base_replay_count == 2

    def test_survivor_discards_selected_hand_card(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Silent")
        card = make_survivor()
        discard_me = make_strike_ironclad()
        combat.hand = [card, discard_me]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert discard_me in combat.discard_pile

    def test_hand_trick_marks_selected_skill_as_sly_for_turn(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Silent")
        card = make_hand_trick()
        target_skill = make_survivor()
        combat.hand = [card, target_skill]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert target_skill.combat_vars.get("sly_this_turn") == 1

    def test_nightmare_sets_selected_card_on_power(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Silent")
        card = make_nightmare()
        target_card = make_strike_ironclad()
        combat.hand = [card, target_card]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        power = combat.player.powers.get(PowerId.NIGHTMARE)
        assert power is not None
        selected_card = getattr(power, "selected_card", None)
        assert selected_card is not None
        assert selected_card is not target_card
        assert selected_card.card_id == target_card.card_id

    def test_the_hunt_grants_power_and_extra_reward_on_kill(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Silent")
        card = make_the_hunt()
        combat.hand = [card]
        combat.energy = 1
        combat.enemies[0].current_hp = 5

        assert combat.play_card(0, 0)
        assert combat.player.has_power(PowerId.THE_HUNT)
        assert combat.extra_card_rewards == 1

    def test_the_hunt_does_not_trigger_fatal_reward_on_minion_kill(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Silent")
        card = make_the_hunt()
        combat.hand = [card]
        combat.energy = 1
        combat.enemies[0].current_hp = 5
        combat.enemies[0].apply_power(PowerId.MINION, 1)

        assert combat.play_card(0, 0)
        assert combat.extra_card_rewards == 0
        assert not combat.player.has_power(PowerId.THE_HUNT)

    def test_cards_apply_specific_powers_instead_of_generic_placeholders(self):
        silent = _make_combat(create_ironclad_starter_deck(), "Silent")
        silent.hand = [make_dodge_and_roll(), make_accelerant(), make_tools_of_the_trade()]
        silent.energy = 3
        assert silent.play_card(0)
        assert silent.player.has_power(PowerId.BLOCK_NEXT_TURN)
        assert silent.play_card(0)
        assert silent.player.has_power(PowerId.ACCELERANT)
        assert silent.play_card(0)
        assert silent.player.has_power(PowerId.TOOLS_OF_THE_TRADE)

        defect = _make_combat(create_defect_starter_deck(), "Defect")
        defect.hand = [make_charge_battery(), make_lightning_rod(), make_signal_boost()]
        defect.energy = 3
        assert defect.play_card(0)
        assert defect.player.has_power(PowerId.ENERGY_NEXT_TURN)
        assert defect.play_card(0)
        assert defect.player.has_power(PowerId.LIGHTNING_ROD)
        assert defect.play_card(0)
        assert defect.player.has_power(PowerId.SIGNAL_BOOST)

    def test_extra_card_rewards_and_combat_damage_history_persist_across_turns(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Silent")
        combat.start_combat()
        combat.extra_card_rewards = 1
        combat.record_damage_event(
            combat.enemies[0],
            combat.player,
            ValueProp.MOVE,
            unblocked_damage=4,
        )

        combat.end_player_turn()

        assert combat.extra_card_rewards == 1
        assert combat.count_unblocked_hits_received_this_combat(combat.player) >= 1

    def test_hammer_time_replicates_forge_to_other_living_players(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        ally = Creature(max_hp=50, current_hp=50, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        combat.apply_power_to(combat.player, PowerId.HAMMER_TIME, 1)

        combat.forge(combat.player, 5)

        player_blades = [card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE]
        ally_blades = [
            card for card in combat._zones_for_creature(ally)["hand"]  # noqa: SLF001
            if card.card_id == CardId.SOVEREIGN_BLADE
        ]
        assert len(player_blades) == 1
        assert len(ally_blades) == 1
        assert player_blades[0].base_damage == 15
        assert ally_blades[0].base_damage == 15

    def test_frantic_escape_increases_matching_sandpit_and_its_own_cost(self):
        from sts2_env.powers.remaining_c import SandpitPower

        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        sandpit = SandpitPower(4)
        sandpit.target = combat.player
        enemy.powers[PowerId.SANDPIT] = sandpit

        card = make_frantic_escape()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert sandpit.amount == 5
        assert card.cost == 2

    def test_right_hand_hand_returns_to_hand_when_enough_energy_remains(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        combat.summon_osty(combat.player, 5)
        card = make_right_hand_hand()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert card in combat.hand

    def test_make_it_so_returns_from_discard_every_third_skill_played(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        watcher = make_make_it_so()
        skill_a = make_gather_light()
        skill_b = make_venerate()
        skill_c = make_venerate()
        combat.discard_pile = [watcher]
        combat.hand = [skill_a, skill_b, skill_c]
        combat.energy = 3

        assert combat.play_card(0)
        assert watcher in combat.discard_pile
        assert combat.play_card(0)
        assert watcher in combat.discard_pile
        assert combat.play_card(0)
        assert watcher in combat.hand

    def test_master_planner_applies_real_power(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Silent")
        card = make_master_planner()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.has_power(PowerId.MASTER_PLANNER)


class TestDynamicColorlessParity:
    def test_believe_in_you_grants_energy_to_selected_ally(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        ally = Creature(max_hp=30, current_hp=30, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        card = make_believe_in_you()
        combat.hand = [card]
        combat.energy = 0

        assert combat.play_card(0, 0)
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None
        assert ally_state.energy == 3
        assert combat.energy == 0

    def test_lift_grants_block_to_selected_ally(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        ally = Creature(max_hp=30, current_hp=30, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        card = make_lift()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert ally.block == 11
        assert combat.player.block == 0

    def test_restlessness_only_triggers_when_it_is_the_only_card_in_hand(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_restlessness()
        combat.hand = [card]
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad(), make_strike_ironclad()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.energy == 2
        assert len(combat.hand) == 2

        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_restlessness()
        combat.hand = [card, make_strike_ironclad()]
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad(), make_strike_ironclad()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.energy == 0
        assert len(combat.hand) == 1

    def test_gang_up_scales_with_allied_powered_attacks_on_target_this_turn(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        ally = Creature(max_hp=30, current_hp=30, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        enemy = combat.enemies[0]
        enemy.current_hp = 40
        combat.record_damage_event(ally, enemy, ValueProp.MOVE, 5)
        combat.record_damage_event(ally, enemy, ValueProp.MOVE, 5)
        card = make_gang_up()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 40 - 15

    def test_mind_blast_scales_with_draw_pile_size(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        enemy.current_hp = 40
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad(), make_strike_ironclad(), make_strike_ironclad()]
        card = make_mind_blast()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 40 - 4

    def test_gold_axe_scales_with_cards_played_this_combat(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        enemy.current_hp = 40
        played_a = make_strike_ironclad()
        played_b = make_strike_ironclad()
        played_a.owner = combat.player
        played_b.owner = combat.player
        combat._played_cards_combat.extend([played_a, played_b])
        card = make_gold_axe()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 40 - 2

    def test_mimic_gains_block_equal_to_target_ally_block(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        ally = Creature(max_hp=30, current_hp=30, side=CombatSide.PLAYER, is_player=True)
        ally.gain_block(7)
        combat.add_ally_player(ally)
        card = make_mimic()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.player.block == 7

    def test_volley_uses_energy_spent_for_hit_count(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        enemy.current_hp = 40
        card = make_volley()
        combat.hand = [card]
        combat.energy = 3

        assert combat.play_card(0)
        assert enemy.current_hp == 40 - 30


class TestStatusParity:
    def test_apotheosis_upgrades_all_other_cards_in_all_piles(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        combat.hand = [make_apotheosis(), strike]
        combat.draw_pile = [defend]
        combat.energy = 2

        assert combat.play_card(0)
        assert strike.upgraded is True
        assert defend.upgraded is True

    def test_enlightenment_reduces_costs_to_one_this_turn_only_when_unupgraded(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        expensive = make_bash()
        combat.hand = [make_enlightenment(), expensive]
        combat.energy = 2

        assert combat.play_card(0)
        assert expensive.cost == 1
        combat.end_player_turn()
        assert expensive.cost == expensive.original_cost

    def test_enlightenment_upgraded_reduces_costs_to_one_for_combat(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        expensive = make_bash()
        combat.hand = [make_enlightenment(upgraded=True), expensive]
        combat.energy = 2

        assert combat.play_card(0)
        assert expensive.cost == 1
        combat.end_player_turn()
        assert expensive.cost == 1

    def test_stack_block_scales_with_discard_size(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.discard_pile = [make_strike_ironclad(), make_strike_ironclad(), make_bash()]
        combat.hand = [make_stack()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 3

        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.discard_pile = [make_strike_ironclad(), make_strike_ironclad(), make_bash()]
        combat.hand = [make_stack(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 6

    def test_brightest_flame_draws_gains_energy_and_loses_max_hp(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.hand = [make_brightest_flame()]
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad(), make_bash()]
        combat.energy = 0
        starting_max_hp = combat.player.max_hp
        starting_hp = combat.player.current_hp

        assert combat.play_card(0)
        assert combat.energy == 2
        assert len(combat.hand) == 2
        assert combat.player.max_hp == starting_max_hp - 1
        assert combat.player.current_hp == min(starting_hp, combat.player.max_hp)

    def test_beckon_triggers_at_turn_end_in_hand_not_on_draw(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        beckon = make_beckon()
        combat.hand = []
        combat.draw_pile = [beckon]
        starting_hp = combat.player.current_hp

        combat._draw_cards(1)
        assert combat.player.current_hp == starting_hp
        assert beckon in combat.hand

        combat.end_player_turn()
        assert combat.player.current_hp == starting_hp - 6

    def test_transform_relic_updates_persistent_and_combat_relic_state(self):
        from sts2_env.relics.base import RelicId
        from sts2_env.relics.registry import create_relic_by_name
        from sts2_env.run.run_state import RunState

        run_state = RunState(seed=42, character_id="Ironclad")
        run_state.initialize_run()
        run_state.player.obtain_relic("SWORD_OF_STONE")
        combat = CombatState(
            player_hp=run_state.player.current_hp,
            player_max_hp=run_state.player.max_hp,
            deck=list(run_state.player.deck),
            rng_seed=42,
            character_id="Ironclad",
            player_state=run_state.player,
        )

        stone = create_relic_by_name("SWORD_OF_STONE")
        combat.current_player_state.relics = [stone]
        combat.player.transform_relic(stone, RelicId.SWORD_OF_JADE)

        assert run_state.player.relics[0] == "SWORD_OF_JADE"
        assert run_state.player.relic_objects[0].relic_id.name == "SWORD_OF_JADE"
        assert combat.current_player_state.relics[0].relic_id.name == "SWORD_OF_JADE"

    def test_shadow_step_discards_hand_and_applies_shadow_step_power(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Silent")
        card = make_shadow_step()
        a = make_strike_ironclad()
        b = make_gather_light()
        combat.hand = [card, a, b]
        combat.energy = 1

        assert combat.play_card(0)
        assert len(combat.hand) == 0
        assert a in combat.discard_pile and b in combat.discard_pile
        assert combat.player.has_power(PowerId.SHADOW_STEP)

    def test_armaments_upgrades_selected_or_all_hand_cards(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_armaments()
        target_card = make_strike_ironclad()
        combat.hand = [card, target_card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert target_card.upgraded

        upgraded = make_armaments(upgraded=True)
        a = make_strike_ironclad()
        b = make_gather_light()
        combat.hand = [upgraded, a, b]
        combat.energy = 1
        assert combat.play_card(0)
        assert a.upgraded and b.upgraded

    def test_headbutt_puts_selected_discard_card_on_top(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_headbutt()
        chosen = make_strike_ironclad()
        combat.hand = [card]
        combat.discard_pile = [chosen]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.draw_pile[0] is chosen

    def test_true_grit_exhausts_random_or_selected_card(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_true_grit()
        target_card = make_strike_ironclad()
        combat.hand = [card, target_card]
        combat.energy = 1
        assert combat.play_card(0)
        assert target_card in combat.exhaust_pile

        upgraded = make_true_grit(upgraded=True)
        target_card2 = make_strike_ironclad()
        combat.hand = [upgraded, target_card2]
        combat.energy = 1
        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert target_card2 in combat.exhaust_pile

    def test_burning_pact_exhausts_selected_card_and_draws(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_burning_pact()
        target_card = make_strike_ironclad()
        combat.hand = [card, target_card]
        combat.draw_pile = [make_gather_light(), make_venerate()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert target_card in combat.exhaust_pile
        assert len(combat.hand) >= 2

    def test_brand_exhausts_random_hand_card_after_buff(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_brand()
        target_card = make_strike_ironclad()
        combat.hand = [card, target_card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert target_card in combat.exhaust_pile
        assert combat.player.has_power(PowerId.STRENGTH)

    def test_primal_force_transforms_attacks_in_hand_into_giant_rock(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_primal_force()
        attack = make_strike_ironclad()
        skill = make_survivor()
        combat.hand = [card, attack, skill]
        combat.energy = 0

        assert combat.play_card(0)
        assert any(c.card_id == CardId.GIANT_ROCK for c in combat.hand)
        assert any(c.card_id == skill.card_id for c in combat.hand)

    def test_stoke_exhausts_hand_then_draws_same_count(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_stoke()
        a = make_strike_ironclad()
        b = make_gather_light()
        combat.hand = [card, a, b]
        combat.draw_pile = [make_venerate(), make_gather_light(), make_solar_strike()]
        combat.energy = 1

        assert combat.play_card(0)
        assert len(combat.hand) == 2
        assert a in combat.exhaust_pile and b in combat.exhaust_pile

    def test_juggling_applies_juggling_power(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_juggling()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.has_power(PowerId.JUGGLING)

    def test_demonic_shield_targets_ally_and_grants_block_equal_to_owner_block(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        ally = Creature(max_hp=50, current_hp=50, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        combat.player.gain_block(12)
        card = make_demonic_shield()
        combat.hand = [card]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert ally.block == 12

    def test_fisticuffs_gains_block_equal_to_unblocked_damage(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        enemy.block = 3
        card = make_fisticuffs()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.player.block == 4

    def test_dismantle_hits_twice_only_if_target_vulnerable(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        enemy = combat.enemies[0]
        card = make_dismantle()
        combat.hand = [card]
        combat.energy = 1

        hp = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == hp - 8

        enemy.current_hp = enemy.max_hp
        enemy.apply_power(PowerId.VULNERABLE, 1)
        combat.hand = [make_dismantle()]
        combat.energy = 1
        hp = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == hp - 24

    def test_tear_asunder_uses_player_unblocked_hits_received(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = make_tear_asunder()
        enemy = combat.enemies[0]
        combat.player.lose_hp(1)
        combat.record_damage_event(enemy, combat.player, ValueProp.MOVE, 1)
        combat.record_damage_event(enemy, combat.player, ValueProp.MOVE, 1)
        combat.hand = [card]
        combat.energy = 2

        hp = enemy.current_hp
        assert combat.play_card(0, 0)
        assert enemy.current_hp == hp - 15

    def test_purity_exhausts_selected_hand_cards(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        purity = CardInstance(
            card_id=CardId.PURITY,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.UNCOMMON,
            keywords=frozenset({"retain", "exhaust"}),
            effect_vars={"cards": 3},
        )
        a = make_strike_ironclad()
        b = make_gather_light()
        combat.hand = [purity, a, b]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)
        assert a in combat.exhaust_pile and b in combat.exhaust_pile

    def test_thinking_ahead_puts_selected_card_back_on_top(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = CardInstance(
            card_id=CardId.THINKING_AHEAD,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.UNCOMMON,
            keywords=frozenset({"exhaust"}),
            effect_vars={"cards": 2},
        )
        keep = make_strike_ironclad()
        combat.hand = [card]
        combat.draw_pile = [keep, make_gather_light()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.draw_pile[0].card_id == keep.card_id

    def test_splash_generates_off_class_attack_choice(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        card = CardInstance(
            card_id=CardId.SPLASH,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.UNCOMMON,
        )
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert all(option.card.card_type == CardType.ATTACK for option in combat.pending_choice.options)
        assert combat.resolve_pending_choice(0)
        assert len(combat.hand) == 1
        assert combat.hand[0].cost == 0

    def test_alchemize_adds_random_potion_to_combat_slots(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.potions = [None, None, None]
        combat.max_potion_slots = 3
        card = CardInstance(
            card_id=CardId.ALCHEMIZE,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.RARE,
            keywords=frozenset({"exhaust"}),
        )
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert len(combat.held_potions()) == 1

    def test_entropic_brew_fills_empty_potion_slots(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        combat.potions = [None, None, None]
        combat.max_potion_slots = 3
        potion = create_potion("EntropicBrew")

        potion.use(combat, combat.player)

        assert len(combat.held_potions()) == 3

    def test_attack_potion_requests_choice_of_attack_cards(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        potion = create_potion("AttackPotion")

        potion.use(combat, combat.player)

        assert combat.pending_choice is not None
        assert all(option.card.card_type == CardType.ATTACK for option in combat.pending_choice.options)
        assert combat.resolve_pending_choice(0)
        assert len(combat.hand) == 1
        assert combat.hand[0].cost == 0

    def test_colorless_potion_requests_choice_of_colorless_cards(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        potion = create_potion("ColorlessPotion")

        potion.use(combat, combat.player)

        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert len(combat.hand) == 1
        assert combat.hand[0].cost == 0

    def test_cunning_potion_adds_upgraded_shivs(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        potion = create_potion("CunningPotion")

        potion.use(combat, combat.player)

        shivs = [card for card in combat.hand if card.card_id == CardId.SHIV]
        assert len(shivs) == 3
        assert all(card.upgraded for card in shivs)

    def test_liquid_memories_selects_discard_card_and_zeroes_cost(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        strike = make_strike_ironclad()
        combat.discard_pile = [strike]
        potion = create_potion("LiquidMemories")

        potion.use(combat, combat.player)

        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert strike in combat.hand
        assert strike.cost == 0

    def test_ashwater_exhausts_selected_hand_cards(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        a = make_strike_ironclad()
        b = make_gather_light()
        combat.hand = [a, b]
        potion = create_potion("Ashwater")

        potion.use(combat, combat.player)

        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)
        assert a in combat.exhaust_pile and b in combat.exhaust_pile

    def test_gamblers_brew_discards_selected_cards_and_redraws(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        a = make_strike_ironclad()
        b = make_gather_light()
        combat.hand = [a, b]
        combat.draw_pile = [make_venerate(), make_gather_light()]
        potion = create_potion("GamblersBrew")

        potion.use(combat, combat.player)

        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)
        assert a in combat.discard_pile and b in combat.discard_pile
        assert len(combat.hand) == 2

    def test_touch_of_insanity_sets_selected_card_to_zero_for_combat(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        target_card = make_strike_ironclad()
        target_card.set_combat_cost(2)
        combat.hand = [target_card]
        potion = create_potion("TouchOfInsanity")

        potion.use(combat, combat.player)

        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert target_card.cost == 0

    def test_cosmic_concoction_adds_three_upgraded_colorless_cards(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        potion = create_potion("CosmicConcoction")

        potion.use(combat, combat.player)

        assert len(combat.hand) == 3
        assert all(card.upgraded for card in combat.hand)

    def test_orobic_acid_adds_attack_skill_power_at_zero_cost(self):
        combat = _make_combat(create_ironclad_starter_deck(), "Ironclad")
        potion = create_potion("OrobicAcid")

        potion.use(combat, combat.player)

        assert len(combat.hand) == 3
        assert {card.card_type for card in combat.hand} == {CardType.ATTACK, CardType.SKILL, CardType.POWER}
        assert all(card.cost == 0 for card in combat.hand)

    def test_capture_spirit_deals_unblockable_damage_and_adds_souls(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        enemy = combat.enemies[0]
        enemy.block = 20
        card = make_capture_spirit()
        combat.hand = [card]
        combat.energy = 1
        starting_hp = enemy.current_hp

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 3
        assert enemy.block == 20
        souls = [c for c in combat.draw_pile if c.card_id == CardId.SOUL]
        assert len(souls) == 3

    def test_cleanse_summons_osty_and_exhausts_selected_draw_card(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_cleanse()
        victim = make_strike_ironclad()
        combat.hand = [card]
        combat.draw_pile = [victim]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 3
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert victim in combat.exhaust_pile

    def test_dredge_moves_selected_discard_cards_to_hand(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_dredge()
        a = make_strike_ironclad()
        b = make_gather_light()
        c = make_venerate()
        combat.hand = [card]
        combat.discard_pile = [a, b, c]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi
        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(2)
        assert combat.resolve_pending_choice(None)
        assert a in combat.hand and b in combat.hand and c in combat.hand

    def test_undeath_clones_itself_to_discard(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_undeath()
        combat.hand = [card]
        combat.energy = 0

        assert combat.play_card(0)
        clones = [c for c in combat.discard_pile if c.card_id == CardId.UNDEATH]
        assert len(clones) == 2

    def test_dirge_uses_x_energy_for_multiple_summons_and_souls(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_dirge()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 6
        souls = [c for c in combat.draw_pile if c.card_id == CardId.SOUL]
        assert len(souls) == 2

    def test_reanimate_summons_large_osty(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_reanimate()
        combat.hand = [card]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 20

    def test_sacrifice_kills_osty_and_gains_double_max_hp_block(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        combat.summon_osty(combat.player, 5)
        card = make_sacrifice()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert not combat.osty.is_alive
        assert combat.player.block == 10

    def test_end_of_days_immediately_kills_doomed_enemies(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_end_of_days()
        combat.hand = [card]
        combat.energy = 3
        combat.enemies[0].current_hp = 20

        assert combat.play_card(0)
        assert combat.enemies[0].is_dead

    def test_necro_mastery_summons_osty_and_applies_power(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_necro_mastery()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 5
        assert combat.player.has_power(PowerId.NECRO_MASTERY)

    def test_pull_aggro_summons_and_gains_block(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_pull_aggro()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == 7
        assert combat.osty is not None
        assert combat.osty.max_hp == 4

    def test_legion_of_bone_summons_large_osty(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_legion_of_bone()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 6

    def test_severance_adds_souls_to_three_piles(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_severance()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert any(c.card_id == CardId.SOUL for c in combat.draw_pile)
        assert any(c.card_id == CardId.SOUL for c in combat.discard_pile)
        assert any(c.card_id == CardId.SOUL for c in combat.hand)

    def test_spur_summons_and_heals_osty(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_spur()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 3
        assert combat.osty.current_hp == 3

    def test_sculpting_strike_applies_ethereal_to_selected_hand_card(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_sculpting_strike()
        target_card = make_strike_ironclad()
        combat.hand = [card, target_card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert target_card.is_ethereal

    def test_eidolon_exhausts_hand_and_grants_intangible_when_threshold_met(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = CardInstance(
            card_id=CardId.EIDOLON,
            cost=2,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.RARE,
        )
        combat.hand = [card] + [make_strike_ironclad() for _ in range(9)]
        combat.energy = 2

        assert combat.play_card(0)
        assert len(combat.hand) == 0
        assert len(combat.exhaust_pile) >= 9
        assert combat.player.has_power(PowerId.INTANGIBLE)

    def test_transfigure_grants_replay_and_increases_cost(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = CardInstance(
            card_id=CardId.TRANSFIGURE,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.RARE,
            keywords=frozenset({"exhaust"}),
        )
        target_card = make_strike_ironclad()
        combat.hand = [card, target_card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert target_card.base_replay_count == 1
        assert target_card.cost == 2

    def test_drain_power_upgrades_random_discard_cards(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_drain_power()
        a = make_strike_ironclad()
        b = make_gather_light()
        combat.hand = [card]
        combat.discard_pile = [a, b]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert any(c.upgraded for c in combat.discard_pile)

    def test_reave_adds_soul_to_draw_pile(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = make_reave()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert any(c.card_id == CardId.SOUL for c in combat.draw_pile)

    def test_collision_course_adds_debris_to_hand(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_collision_course()
        combat.hand = [card]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert any(c.card_id == CardId.DEBRIS for c in combat.hand)

    def test_cosmic_indifference_moves_selected_discard_to_draw_top(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_cosmic_indifference()
        target = make_strike_ironclad()
        combat.hand = [card]
        combat.discard_pile = [target]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.draw_pile[0].card_id == target.card_id

    def test_glimmer_puts_selected_hand_card_back_on_draw_pile(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = CardInstance(
            card_id=CardId.GLIMMER,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.UNCOMMON,
            effect_vars={"cards": 3},
        )
        keep = make_strike_ironclad()
        combat.hand = [card, keep]
        combat.draw_pile = [make_gather_light(), make_venerate(), make_solar_strike()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.draw_pile[0].card_id == keep.card_id

    def test_any_ally_card_is_unplayable_without_other_player_allies(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = CardInstance(
            card_id=CardId.LARGESSE,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.ANY_ALLY,
            rarity=CardRarity.UNCOMMON,
        )
        combat.hand = [card]
        combat.energy = 0

        assert not combat.can_play_card(card)

    def test_glimpse_beyond_has_no_effect_without_player_allies(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        card = CardInstance(
            card_id=CardId.GLIMPSE_BEYOND,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.ALL_ALLIES,
            rarity=CardRarity.RARE,
            effect_vars={"cards": 3},
        )
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert not any(c.card_id == CardId.SOUL for c in combat.draw_pile)

    def test_largesse_targets_ally_player_hand(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        ally = Creature(max_hp=50, current_hp=50, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        card = CardInstance(
            card_id=CardId.LARGESSE,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.ANY_ALLY,
            rarity=CardRarity.UNCOMMON,
        )
        combat.hand = [card]
        combat.energy = 0

        assert combat.can_play_card(card)
        assert combat.play_card(0, 0)
        assert len(combat._ally_player_zones[ally]["hand"]) == 1

    def test_glimpse_beyond_adds_souls_to_ally_player_draw(self):
        combat = _make_combat(create_necrobinder_starter_deck(), "Necrobinder")
        ally = Creature(max_hp=50, current_hp=50, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        card = CardInstance(
            card_id=CardId.GLIMPSE_BEYOND,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.ALL_ALLIES,
            rarity=CardRarity.RARE,
            effect_vars={"cards": 3},
        )
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert len(combat._ally_player_zones[ally]["draw"]) == 3

    def test_convergence_applies_energy_and_star_next_turn(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_convergence()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.has_power(PowerId.RETAIN_HAND)
        assert combat.player.has_power(PowerId.ENERGY_NEXT_TURN)
        assert combat.player.has_power(PowerId.STAR_NEXT_TURN)

    def test_hidden_cache_gains_star_and_star_next_turn(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_hidden_cache()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.stars == 1
        assert combat.player.has_power(PowerId.STAR_NEXT_TURN)

    def test_resonance_gains_strength_and_reduces_enemy_strength(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_resonance()
        combat.hand = [card]
        combat.energy = 1
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1
        assert combat.enemies[0].get_power_amount(PowerId.STRENGTH) == -1

    def test_summon_forth_brings_sovereign_blade_back_to_hand(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        blade = make_sovereign_blade()
        combat.discard_pile = [blade]
        card = make_summon_forth()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert blade in combat.hand

    def test_decisions_decisions_auto_plays_selected_skill_three_times(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_decisions_decisions()
        skill = make_gather_light()
        combat.hand = [card, skill]
        combat.draw_pile = [make_venerate(), make_solar_strike(), make_gather_light()]
        combat.energy = 0
        combat.gain_stars(combat.player, 6)

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.player.block >= 21

    def test_quasar_generates_upgraded_colorless_choice(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_quasar(upgraded=True)
        combat.hand = [card]
        combat.energy = 1
        combat.gain_stars(combat.player, 2)

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert len(combat.hand) == 1
        assert combat.hand[0].upgraded

    def test_heirloom_hammer_copies_selected_colorless_card(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        hammer = make_heirloom_hammer()
        gem = make_hidden_gem()
        combat.hand = [hammer, gem]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        copies = [c for c in combat.hand if c.card_id == gem.card_id]
        assert len(copies) == 2

    def test_void_form_ends_turn_after_play(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        card = make_void_form()
        combat.hand = [card]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.round_number == 2
        assert combat.current_side == CombatSide.PLAYER

    def test_beat_into_shape_uses_prior_hits_for_forge_amount(self):
        combat = _make_combat(create_regent_starter_deck(), "Regent")
        enemy = combat.enemies[0]

        first = make_strike_ironclad()
        beat = make_beat_into_shape()
        combat.hand = [first, beat]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert combat.play_card(0, 0)
        blade = next(card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE)
        assert blade.base_damage == 20
