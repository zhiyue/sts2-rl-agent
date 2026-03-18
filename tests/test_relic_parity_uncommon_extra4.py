"""Fourth batch of focused uncommon relic parity tests for trigger timing and combat math."""

import sts2_env.potions  # noqa: F401
import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CombatSide, PowerId, RoomType, ValueProp
from sts2_env.core.hooks import fire_after_turn_end, fire_before_side_turn_start, fire_before_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.run.rooms import RoomVisitContext
from sts2_env.run.run_state import RunState


def _with_owner(cards: list, owner):
    for card in cards:
        card.owner = owner
    return cards


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 901,
    enemies: int = 1,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    for i in range(enemies):
        if i == 0:
            creature, ai = create_shrinker_beetle(Rng(seed + i))
        else:
            creature, ai = create_twig_slime_s(Rng(seed + i))
        combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _make_defect_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 951,
) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=seed,
        character_id="Defect",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _strike_damage_vs_vulnerable(relics: list[str], *, seed: int) -> int:
    combat = _make_ironclad_combat(relics, seed=seed)
    enemy = combat.enemies[0]
    enemy.max_hp = 200
    enemy.current_hp = 200
    enemy.block = 0
    enemy.apply_power(PowerId.VULNERABLE, 1, applier=combat.player)
    combat.hand = _with_owner([make_strike_ironclad()], combat.player)
    combat.energy = 1
    hp_before = enemy.current_hp
    assert combat.play_card(0, 0)
    return hp_before - enemy.current_hp


class TestRelicParityUncommonExtra4:
    def test_parrying_shield_only_triggers_at_or_above_ten_block(self):
        """Matches ParryingShield.cs: after player turn, thresholded random-enemy hit for 6."""
        combat = _make_ironclad_combat(["ParryingShield"], seed=901)
        enemy = combat.enemies[0]
        enemy.max_hp = 160
        enemy.current_hp = 160

        combat.player.block = 9
        hp_before = enemy.current_hp
        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert enemy.current_hp == hp_before

        combat.player.block = 10
        hp_before = enemy.current_hp
        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert enemy.current_hp == hp_before - 6

    def test_pantograph_heals_only_on_boss_room_entry(self):
        """Matches Pantograph.cs: room-entry boss heal for living owner only."""
        run_state = RunState(seed=902, character_id="Ironclad")
        assert run_state.player.obtain_relic("PANTOGRAPH")
        relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "PANTOGRAPH")
        run_state.player.max_hp = 80
        run_state.player.current_hp = 40

        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.MONSTER))
        assert run_state.player.current_hp == 40

        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.BOSS))
        assert run_state.player.current_hp == 65

        run_state.player.current_hp = 0
        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.BOSS))
        assert run_state.player.current_hp == 0

    def test_paper_phrog_increases_damage_against_vulnerable_target(self):
        """Matches PaperPhrog.cs: powered attacks into Vulnerable get +0.25 vulnerable multiplier."""
        baseline = _strike_damage_vs_vulnerable([], seed=903)
        with_phrog = _strike_damage_vs_vulnerable(["PaperPhrog"], seed=903)

        assert baseline == 9
        assert with_phrog == 10

    def test_petrified_toad_procures_potion_shaped_rock_at_combat_start(self):
        """Matches PetrifiedToad.cs: before-combat-start-late tries to procure PotionShapedRock."""
        combat = _make_ironclad_combat(["PetrifiedToad"], seed=904)
        toad_potions = [p for p in combat.potions if p is not None and p.potion_id == "PotionShapedRock"]

        assert len(toad_potions) == 1

    def test_ripple_basin_requires_no_attacks_and_resets_each_turn(self):
        """Matches RippleBasin.cs: if no owner attacks this turn, gain 4 block at turn end."""
        combat = _make_ironclad_combat(["RippleBasin"], seed=905)
        combat.player.block = 0

        combat.hand = _with_owner([make_strike_ironclad()], combat.player)
        combat.energy = 1
        assert combat.play_card(0, 0)
        fire_before_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.block == 0

        fire_before_side_turn_start(CombatSide.PLAYER, combat)
        fire_before_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.block == 4

    def test_self_forming_clay_applies_block_next_turn_only_on_unblocked_damage(self):
        """Matches relic trigger contract: only unblocked damage applies the follow-up block power."""
        combat = _make_ironclad_combat(["SelfFormingClay"], seed=906)
        enemy = combat.enemies[0]

        combat.player.block = 0
        combat.deal_damage(enemy, combat.player, 5, ValueProp.UNPOWERED)
        assert combat.player.get_power_amount(PowerId.BLOCK_NEXT_TURN) == 3

        combat2 = _make_ironclad_combat(["SelfFormingClay"], seed=907)
        enemy2 = combat2.enemies[0]
        combat2.player.block = 99
        combat2.deal_damage(enemy2, combat2.player, 5, ValueProp.UNPOWERED)
        assert combat2.player.get_power_amount(PowerId.BLOCK_NEXT_TURN) == 0

    def test_twisted_funnel_applies_poison_only_on_round_one(self):
        """Matches TwistedFunnel.cs: before side turn start, round-1 poison all enemies."""
        combat = _make_ironclad_combat(["TwistedFunnel"], seed=908, enemies=2)
        for enemy in combat.enemies:
            assert enemy.get_power_amount(PowerId.POISON) == 4
            enemy.powers.pop(PowerId.POISON, None)

        combat.round_number = 2
        fire_before_side_turn_start(CombatSide.PLAYER, combat)
        for enemy in combat.enemies:
            assert enemy.get_power_amount(PowerId.POISON) == 0

    def test_gold_plated_cables_triggers_first_orb_passive_one_extra_time(self):
        """Matches GoldPlatedCables.cs: first orb gains +1 passive trigger count."""
        baseline = _make_defect_combat([], seed=909)
        with_cables = _make_defect_combat(["GoldPlatedCables"], seed=909)

        for combat in (baseline, with_cables):
            combat.player.block = 0
            combat.channel_orb(combat.player, "FROST")
            fire_before_turn_end(CombatSide.PLAYER, combat)
            assert combat.orb_queue is not None
            combat.orb_queue.trigger_before_turn_end(combat)

        assert baseline.player.block == 2
        assert with_cables.player.block == 4
