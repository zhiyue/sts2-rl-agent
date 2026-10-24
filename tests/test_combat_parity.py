"""Combat flow parity tests against decompiled turn/relic semantics."""

import pytest

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.silent import make_bullet_time, make_well_laid_plans
from sts2_env.cards.status import make_burn, make_dazed, make_debt, make_regret, make_void
from sts2_env.cards.registry import play_card_effect
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CardType, CombatSide, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.powers.base import PowerInstance
from sts2_env.run.run_state import PlayerState, RunState


class _ChoiceLastRng:
    def __init__(self):
        self.calls = 0

    def choice(self, seq):
        self.calls += 1
        return seq[-1]


class _ReverseShuffleRng:
    def __init__(self):
        self.calls = 0

    def shuffle(self, seq) -> None:
        self.calls += 1
        seq.reverse()


class _FixedIntRng:
    def __init__(self, value: int):
        self.calls = 0
        self.value = value

    def next_int(self, low: int, high: int) -> int:
        self.calls += 1
        return self.value


class _GenerationRng:
    def __init__(self):
        self.calls = 0

    def sample(self, seq, count: int):
        self.calls += 1
        return list(seq)[:count]

    def shuffle(self, seq) -> None:
        self.calls += 1

    def choice(self, seq):
        self.calls += 1
        return list(seq)[0]


class _PotionRng:
    def __init__(self):
        self.calls = 0

    def next_float(self, upper: float = 1.0) -> float:
        self.calls += 1
        return 0.9 * upper

    def choice(self, seq):
        self.calls += 1
        return list(seq)[0]


def _make_combat(
    relics: list[str] | None = None,
    gold: int = 0,
    *,
    character_id: str = "Ironclad",
    extra_enemies: int = 0,
) -> CombatState:
    rng = Rng(42)
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        relics=relics or [],
        gold=gold,
        character_id=character_id,
    )
    creature, ai = create_shrinker_beetle(rng)
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(100 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class _CannotHitPower(PowerInstance):
    def __init__(self):
        super().__init__(PowerId.COVERED, 1)

    def should_allow_hitting(self, owner, combat):
        return False


@pytest.mark.parametrize(
    "card_id",
    [
        CardId.LUMINESCE,
        CardId.SUPERCRITICAL,
        CardId.PRODUCTION,
        CardId.RESTLESSNESS,
        CardId.ALIGNMENT,
        CardId.WISP,
        CardId.BORROWED_TIME,
        CardId.TACTICIAN,
    ],
)
def test_gain_energy_card_effects_do_not_gain_after_combat_ending(card_id):
    combat = _make_combat()
    card = create_card(card_id)
    card.owner = combat.player
    combat.hand = []
    combat.draw_pile = [make_strike_ironclad()]
    combat.energy = 0
    combat.is_over = True

    play_card_effect(card, combat, None)

    assert combat.energy == 0


def test_gain_energy_card_effect_uses_card_owner_state():
    combat = _make_combat()
    ally = combat.add_ally_player(
        PlayerState(player_id=2, character_id="Silent", max_hp=70, current_hp=70)
    )
    ally_state = combat.combat_player_state_for(ally)
    assert ally_state is not None
    card = create_card(CardId.TACTICIAN)
    card.owner = ally
    combat.energy = 0
    ally_state.energy = 0

    play_card_effect(card, combat, None)

    assert combat.energy == 0
    assert ally_state.energy == 1


def test_is_owner_side_turn_uses_owner_side():
    combat = _make_combat()
    enemy = combat.enemies[0]

    combat.current_side = CombatSide.PLAYER
    assert combat.is_owner_side_turn(combat.player)
    assert not combat.is_owner_side_turn(enemy)

    combat.current_side = CombatSide.ENEMY
    assert not combat.is_owner_side_turn(combat.player)
    assert combat.is_owner_side_turn(enemy)


def test_gain_stars_does_not_gain_after_combat_ending():
    combat = _make_combat(character_id="Regent")
    combat.stars = 0
    combat.player.stars = 0
    combat.is_over = True

    combat.gain_stars(combat.player, 3)

    assert combat.stars == 0
    assert combat.player.stars == 0
    assert combat.count_stars_gained_this_turn(combat.player) == 0


def test_seeker_strike_stable_shuffles_draw_pile_before_choice():
    combat = _make_combat()
    combat.rng = _ReverseShuffleRng()
    card = create_card(CardId.SEEKER_STRIKE)
    card.owner = combat.player
    card.effect_vars["cards"] = 2
    combat.hand = [card]
    combat.draw_pile = [
        create_card(CardId.BASH),
        create_card(CardId.DEFEND_IRONCLAD),
        create_card(CardId.ANGER),
    ]
    combat.energy = 3

    assert combat.play_card(0, 0)

    options = [option.card.card_id for option in combat.pending_choice.options]
    assert options == [CardId.DEFEND_IRONCLAD, CardId.BASH]


class TestRelicTurnHooks:
    def test_anchor_grants_block_before_first_turn(self):
        combat = _make_combat(["Anchor"])
        assert combat.player.block == 10

    def test_anchor_block_triggers_after_block_gained_hooks(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=43,
            relics=["Anchor"],
            character_id="Ironclad",
        )
        enemy, ai = create_shrinker_beetle(Rng(43))
        combat.add_enemy(enemy, ai)
        start_hp = enemy.current_hp
        combat.player.apply_power(PowerId.JUGGERNAUT, 5)

        combat.start_combat()

        assert combat.player.block == 10
        assert enemy.current_hp == start_hp - 5

    def test_ring_of_the_snake_modifies_round_1_hand_draw(self):
        combat = _make_combat(["RingOfTheSnake"])
        assert len(combat.hand) == 7

    def test_lantern_adds_energy_after_round_1_reset(self):
        combat = _make_combat(["Lantern"])
        assert combat.energy == 4

    def test_ice_cream_uses_add_max_energy_instead_of_reset(self):
        combat = _make_combat(["IceCream"])
        combat.energy = 1
        combat.end_player_turn()
        assert combat.round_number == 2
        assert combat.energy == 4

    def test_runic_pyramid_prevents_hand_flush(self):
        combat = _make_combat(["RunicPyramid"])
        retained = combat.hand[0]
        combat.end_player_turn()
        assert retained in combat.hand

    def test_bookmark_reduces_cost_of_single_turn_retained_card(self):
        combat = _make_combat(["Bookmark"])
        retained = make_strike_ironclad()
        retained.keywords = frozenset({"retain"})
        combat.hand = [retained]

        combat.end_player_turn()

        assert retained in combat.hand
        assert retained.cost == 0

    def test_bookmark_ignores_non_retained_cards_kept_by_runic_pyramid(self):
        combat = _make_combat(["Bookmark", "RunicPyramid"])
        normal = make_strike_ironclad()
        retained = make_strike_ironclad()
        retained.keywords = frozenset({"retain"})
        combat.hand = [normal, retained]

        combat.end_player_turn()

        assert normal in combat.hand
        assert retained in combat.hand
        assert normal.cost == 1
        assert retained.cost == 0

    def test_bookmark_discount_expires_when_card_is_played(self):
        """C# Bookmark.cs:30 EnergyCost.AddUntilPlayed(-1): the -1 modifier
        expires on play, restoring the printed cost."""
        combat = _make_combat(["Bookmark"])
        retained = make_strike_ironclad()
        retained.keywords = frozenset({"retain"})
        combat.hand = [retained]
        combat.end_player_turn()
        assert retained.cost == 0

        combat.energy = 0
        assert combat.play_card(0)
        assert retained.cost == 1

    def test_bookmark_discounts_stack_until_played(self):
        """C# Bookmark.cs:30 AddUntilPlayed(-1) can stack while the card stays
        retained; playing it restores the full printed cost."""
        combat = _make_combat(["Bookmark"])
        retained = make_strike_ironclad()
        retained.keywords = frozenset({"retain"})
        combat.hand = [retained]

        combat.end_player_turn()
        combat.end_player_turn()

        assert retained.cost == 0

        combat.energy = 0
        assert combat.play_card(0)
        assert retained.cost == 1


class TestPowerTurnHooks:
    def test_generated_status_dispatches_power_and_card_hooks(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 50
        enemy.max_hp = 50
        rocket_punch = create_card(CardId.ROCKET_PUNCH)
        combat.hand = [rocket_punch]
        combat.apply_power_to(combat.player, PowerId.SMOKESTACK, 4)
        combat.apply_power_to(combat.player, PowerId.PILLAR_OF_CREATION, 3)

        combat.add_generated_card_to_creature_hand(combat.player, make_dazed())

        assert enemy.current_hp == 46
        assert combat.player.block == 3
        assert rocket_punch.cost == 0
        assert combat.count_generated_cards_this_combat(combat.player) == 1

    def test_combat_uses_run_shuffle_stream_for_initial_draw_pile(self):
        run_state = RunState(seed=8401, character_id="Ironclad")
        run_state.initialize_run()
        run_state.player.deck = create_ironclad_starter_deck()
        shuffle_rng = _ReverseShuffleRng()
        run_state.rng.shuffle = shuffle_rng
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=list(run_state.player.deck),
            rng_seed=1,
            player_state=run_state.player,
        )

        combat.start_combat()

        assert shuffle_rng.calls == 1

    def test_random_enemy_target_uses_run_combat_targets_stream(self):
        run_state = RunState(seed=8402, character_id="Ironclad")
        run_state.initialize_run()
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=1,
            player_state=run_state.player,
        )
        first, first_ai = create_shrinker_beetle(Rng(1))
        second, second_ai = create_shrinker_beetle(Rng(2))
        combat.add_enemy(first, first_ai)
        combat.add_enemy(second, second_ai)
        target_rng = _ChoiceLastRng()
        run_state.rng.combat_targets = target_rng
        card = create_card(CardId.RIP_AND_TEAR)

        target = combat._resolve_target(card, None)

        assert target is second
        assert target_rng.calls == 1

    def test_random_energy_cost_uses_run_combat_energy_costs_stream(self):
        run_state = RunState(seed=8403, character_id="Ironclad")
        run_state.initialize_run()
        energy_rng = _FixedIntRng(2)
        run_state.rng.combat_energy_costs = energy_rng
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=1,
            player_state=run_state.player,
        )
        card = make_strike_ironclad()
        card.add_enchantment("Slither", 1)
        combat.hand = []
        combat.draw_pile = [card]

        combat.draw_cards(combat.player, 1)

        assert card.cost == 2
        assert energy_rng.calls == 1

    def test_generated_cards_use_run_combat_card_generation_stream(self):
        run_state = RunState(seed=8404, character_id="Ironclad")
        run_state.initialize_run()
        generation_rng = _GenerationRng()
        run_state.rng.combat_card_generation = generation_rng
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=1,
            player_state=run_state.player,
        )

        combat.generate_card_to_hand(combat.player, card_type=CardType.ATTACK)

        assert generation_rng.calls == 1
        assert len(combat.hand) == 1

    def test_random_potions_use_run_combat_potion_generation_stream(self):
        run_state = RunState(seed=8405, character_id="Ironclad")
        run_state.initialize_run()
        potion_rng = _PotionRng()
        run_state.rng.combat_potion_generation = potion_rng
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=1,
            player_state=run_state.player,
        )

        potion = combat.procure_random_potion(combat.player, in_combat=True)

        assert potion is not None
        assert potion_rng.calls == 2

    def test_random_draw_insert_uses_run_shuffle_stream(self):
        run_state = RunState(seed=8406, character_id="Ironclad")
        run_state.initialize_run()
        shuffle_rng = _FixedIntRng(0)
        run_state.rng.shuffle = shuffle_rng
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=1,
            player_state=run_state.player,
        )

        combat.add_generated_card_to_creature_draw_pile(
            combat.player,
            make_dazed(),
            random_position=True,
        )

        assert shuffle_rng.calls == 1

    def test_retained_card_choice_uses_run_combat_card_selection_stream(self):
        run_state = RunState(seed=8407, character_id="Ironclad")
        run_state.initialize_run()
        selection_rng = _ChoiceLastRng()
        run_state.rng.combat_card_selection = selection_rng
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=1,
            player_state=run_state.player,
        )
        first = make_strike_ironclad()
        second = make_strike_ironclad()
        first.keywords = frozenset({"retain"})
        second.keywords = frozenset({"retain"})
        combat.hand = [first, second]

        combat.reduce_retained_card_cost(combat.player)

        assert selection_rng.calls == 1
        assert first.cost == 1
        assert second.cost == 0

    def test_random_attack_from_hand_uses_run_shuffle_stream(self):
        run_state = RunState(seed=8408, character_id="Ironclad")
        run_state.initialize_run()
        shuffle_rng = _ChoiceLastRng()
        run_state.rng.shuffle = shuffle_rng
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=1,
            player_state=run_state.player,
        )
        creature, ai = create_shrinker_beetle(Rng(8408))
        combat.add_enemy(creature, ai)
        first = make_strike_ironclad()
        second = create_card(CardId.BASH)
        combat.hand = [first, second]

        assert combat.auto_play_random_attack_from_hand(combat.player)

        assert shuffle_rng.calls == 1
        assert first in combat.hand
        assert second in combat.discard_pile

    def test_smokestack_damage_hits_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 50
        hittable.current_hp = hittable.max_hp = 50
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.apply_power_to(combat.player, PowerId.SMOKESTACK, 4)

        combat.add_generated_card_to_creature_hand(combat.player, make_dazed())

        assert blocked.current_hp == 50
        assert hittable.current_hp == 46

    def test_monster_generated_status_skips_player_generated_hooks_but_not_rocket_punch(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 50
        enemy.max_hp = 50
        rocket_punch = create_card(CardId.ROCKET_PUNCH)
        combat.hand = [rocket_punch]
        combat.apply_power_to(combat.player, PowerId.SMOKESTACK, 4)
        combat.apply_power_to(combat.player, PowerId.PILLAR_OF_CREATION, 3)

        combat.add_status_cards_to_discard(combat.player, "DAZED", 1)

        assert enemy.current_hp == 50
        assert combat.player.block == 0
        assert rocket_punch.cost == 0
        assert combat.count_generated_cards_this_combat(combat.player) == 0

    def test_trash_to_treasure_channels_when_player_generates_status(self):
        combat = _make_combat(character_id="Defect")
        combat.apply_power_to(combat.player, PowerId.TRASH_TO_TREASURE, 2)

        combat.add_generated_card_to_creature_hand(combat.player, make_dazed())

        assert combat.orb_queue is not None
        assert len(combat.orb_queue.orbs) == 2

    def test_transforming_combat_card_counts_as_player_generated(self):
        combat = _make_combat()
        combat.apply_power_to(combat.player, PowerId.PILLAR_OF_CREATION, 3)
        old_card = make_dazed()
        replacement = make_burn()
        combat.hand = [old_card]

        combat.transform_card(old_card, replacement)

        assert combat.hand == [replacement]
        assert combat.player.block == 3
        assert combat.count_generated_cards_this_combat(combat.player) == 1

    def test_retain_hand_power_retains_current_hand(self):
        combat = _make_combat()
        retained = combat.hand[0]
        combat.apply_power_to(combat.player, PowerId.RETAIN_HAND, 1)
        combat.end_player_turn()
        assert retained in combat.hand

    def test_automation_triggers_from_draw_pipeline(self):
        combat = _make_combat()
        combat.player.apply_power(PowerId.AUTOMATION, 2)
        combat.draw_pile = [make_strike_ironclad() for _ in range(20)]
        combat.hand.clear()

        initial_energy = combat.energy
        combat.draw_cards(combat.player, 10)
        assert combat.energy == initial_energy + 2

        combat.hand.clear()
        combat.draw_cards(combat.player, 10)

        assert combat.energy == initial_energy + 4

    def test_automation_keeps_separate_draw_counters_like_reference(self):
        combat = _make_combat()
        combat.player.apply_power(PowerId.AUTOMATION, 1)
        combat.draw_pile = [make_strike_ironclad() for _ in range(20)]
        combat.hand.clear()

        initial_energy = combat.energy
        combat.draw_cards(combat.player, 9)
        combat.player.apply_power(PowerId.AUTOMATION, 1)
        combat.hand.clear()

        combat.draw_cards(combat.player, 1)

        assert combat.energy == initial_energy + 1

    def test_hellraiser_auto_plays_drawn_strike_before_normal_draw_hooks(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        strike = make_strike_ironclad()
        strike.owner = combat.player
        combat.hand.clear()
        combat.draw_pile = [strike]
        combat.energy = 0
        combat.player.apply_power(PowerId.HELLRAISER, 1)
        start_hp = enemy.current_hp

        combat.draw_cards(combat.player, 1)

        assert strike not in combat.hand
        assert combat.count_cards_played_this_turn(combat.player) == 1
        assert enemy.current_hp == start_hp - 6

    def test_no_draw_blocks_non_hand_draw(self):
        combat = _make_combat()
        combat.discard_pile.extend(combat.hand)
        combat.hand.clear()
        combat.player.apply_power(PowerId.NO_DRAW, 1)

        combat.draw_cards(combat.player, 1)

        assert len(combat.hand) == 0

    def test_no_draw_does_not_block_opening_hand_draw(self):
        rng = Rng(42)
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=42,
        )
        combat.player.apply_power(PowerId.NO_DRAW, 1)
        creature, ai = create_shrinker_beetle(rng)
        combat.add_enemy(creature, ai)

        combat.start_combat()

        assert len(combat.hand) == 5

    def test_duplication_replays_full_card_play_hooks(self):
        combat = _make_combat()
        strike = make_strike_ironclad()
        combat.hand = [strike]
        combat.energy = 1
        combat.player.apply_power(PowerId.RAGE, 3)
        combat.player.apply_power(PowerId.DUPLICATION, 1)

        combat.play_card(0, 0)

        assert combat.player.block == 6


class TestCardCleanup:
    def test_bullet_time_temporary_cost_resets_at_end_of_turn(self):
        combat = _make_combat()
        bullet_time = make_bullet_time()
        strike = make_strike_ironclad()
        combat.hand = [bullet_time, strike]
        combat.energy = 3

        assert strike.cost == strike.original_cost == 1
        combat.play_card(0)
        assert strike.cost == 0

        combat.end_player_turn()
        assert strike.cost == 1

    def test_bullet_time_sets_star_costs_free_until_end_of_turn(self):
        combat = _make_combat()
        bullet_time = make_bullet_time()
        strike = make_strike_ironclad()
        strike.star_cost = 2
        combat.hand = [bullet_time, strike]
        combat.energy = 3

        combat.play_card(0)
        assert combat.modified_star_cost(combat.player, strike) == 0

        combat.end_player_turn()
        assert combat.modified_star_cost(combat.player, strike) == 2

    def test_well_laid_plans_applies_correct_power(self):
        combat = _make_combat()
        well_laid_plans = make_well_laid_plans()
        combat.hand = [well_laid_plans]
        combat.energy = 1

        combat.play_card(0)

        assert combat.player.has_power(PowerId.WELL_LAID_PLANS)

    def test_burn_triggers_turn_end_in_hand_damage(self):
        combat = _make_combat()
        burn = make_burn()
        combat.hand = [burn]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 2
        assert burn in combat.discard_pile

    def test_regret_uses_pre_flush_hand_size_for_damage(self):
        combat = _make_combat()
        regret = make_regret()
        filler = make_strike_ironclad()
        combat.hand = [regret, filler]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 2

    def test_debt_triggers_turn_end_gold_loss(self):
        combat = _make_combat(gold=20)
        debt = make_debt()
        combat.hand = [debt]

        combat.end_player_turn()

        assert combat.gold == 10

    def test_void_loses_energy_when_drawn(self):
        combat = _make_combat()
        combat.hand.clear()
        combat.draw_pile = [make_void()]
        combat.energy = 3

        combat.draw_cards(combat.player, 1)

        assert combat.energy == 2


class TestExtraTurn:
    def test_paels_eye_skips_enemy_turn_and_grants_extra_turn(self):
        combat = _make_combat(["PaelsEye"])
        enemy_ai = combat.enemy_ais[0]

        combat.end_player_turn()

        assert combat.current_side == CombatSide.PLAYER
        assert combat.round_number == 2
        assert enemy_ai.current_move.state_id == "SHRINKER_MOVE"

    def test_paels_eye_exhausts_cards_through_exhaust_hooks(self):
        combat = _make_combat(["PaelsEye", "CharonsAshes"])
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp

        combat.end_player_turn()

        assert enemy.current_hp == starting_hp - 15


class TestEnemyTurnStart:
    def test_enemy_block_clears_on_first_enemy_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.gain_block(7)

        combat.end_player_turn()

        assert enemy.block == 0
