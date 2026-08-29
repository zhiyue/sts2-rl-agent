"""Parity tests for event/ancient relic obtain, opening, and rest hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck, make_inflame
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.silent import make_backstab
from sts2_env.cards.status import make_clumsy, make_luminesce, make_wound
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CombatSide, PowerId
from sts2_env.core.hooks import fire_after_card_played, fire_before_turn_end, should_flush
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.reward_objects import AddCardsReward, RelicReward, RemoveCardReward, UpgradeCardsReward
from sts2_env.run.rest_site import generate_rest_site_options
from sts2_env.run.run_state import RunState


def _make_combat(relics: list[str] | None = None, *, seed: int = 881) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
        gold=50,
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicEventObtainOpeningRestHooksParity:
    def test_golden_pearl_grants_one_hundred_fifty_gold_on_obtain(self):
        run_state = RunState(seed=895, character_id="Ironclad")
        starting_gold = run_state.player.gold

        assert run_state.player.obtain_relic("GOLDEN_PEARL")
        assert run_state.player.gold == starting_gold + 150

    def test_looming_fruit_and_nutritious_oyster_gain_max_hp_on_obtain(self):
        run_state = RunState(seed=896, character_id="Ironclad")
        max_hp_before = run_state.player.max_hp
        hp_before = run_state.player.current_hp

        assert run_state.player.obtain_relic("LOOMING_FRUIT")
        assert run_state.player.max_hp == max_hp_before + 31
        assert run_state.player.current_hp == hp_before + 31

        assert run_state.player.obtain_relic("NUTRITIOUS_OYSTER")
        assert run_state.player.max_hp == max_hp_before + 42
        assert run_state.player.current_hp == hp_before + 42

    def test_ancient_relics_add_their_cards_to_deck_on_obtain(self):
        run_state = RunState(seed=897, character_id="Ironclad")

        assert run_state.player.obtain_relic("BLOOD_SOAKED_ROSE")
        assert run_state.player.obtain_relic("NEOWS_TORMENT")
        assert run_state.player.obtain_relic("PAELS_HORN")
        assert run_state.player.obtain_relic("TANXS_WHISTLE")

        added_ids = [card.card_id for card in run_state.player.deck]
        assert added_ids.count(CardId.ENTHRALLED) == 1
        assert added_ids.count(CardId.NEOWS_FURY) == 1
        assert added_ids.count(CardId.RELAX) == 2
        assert added_ids.count(CardId.WHISTLE) == 1

    def test_electric_shrymp_enchants_one_skill_with_imbued(self):
        run_state = RunState(seed=898, character_id="Ironclad")
        run_state.player.deck = create_ironclad_starter_deck()

        assert run_state.player.obtain_relic("ELECTRIC_SHRYMP")
        imbued_cards = [card for card in run_state.player.deck if card.enchantments.get("Imbued") == 1]

        assert len(imbued_cards) == 1
        assert imbued_cards[0].card_type.name == "SKILL"

    def test_gnarled_hammer_enchants_up_to_three_attacks_with_sharp(self):
        run_state = RunState(seed=899, character_id="Ironclad")
        run_state.player.deck = create_ironclad_starter_deck()

        assert run_state.player.obtain_relic("GNARLED_HAMMER")
        sharp_cards = [card for card in run_state.player.deck if card.enchantments.get("Sharp") == 3]

        assert len(sharp_cards) == 3
        assert all(card.card_type.name == "ATTACK" for card in sharp_cards)

    def test_iron_club_draws_after_every_four_owner_cards_played(self):
        combat = _make_combat(["IRON_CLUB"], seed=900)
        relic = next(relic for relic in combat.relics if relic.relic_id.name == "IRON_CLUB")
        draw = [make_strike_ironclad(), make_strike_ironclad()]
        combat.hand = []
        combat.draw_pile = draw
        combat.discard_pile = []

        for _ in range(3):
            card = make_strike_ironclad()
            card.owner = combat.player
            fire_after_card_played(card, combat)
        assert relic._cards_played == 3  # noqa: SLF001
        assert combat.hand == []

        drawn = draw[0]
        fourth = make_strike_ironclad()
        fourth.owner = combat.player
        fire_after_card_played(fourth, combat)

        assert relic._cards_played == 4  # noqa: SLF001
        assert drawn in combat.hand

        other_owner = combat.enemies[0]
        enemy_card = make_strike_ironclad()
        enemy_card.owner = other_owner
        fire_after_card_played(enemy_card, combat)
        assert relic._cards_played == 4  # noqa: SLF001

    def test_ringing_triangle_prevents_round_one_hand_flush_only(self):
        combat = _make_combat(["RINGING_TRIANGLE"], seed=901)

        combat.round_number = 1
        assert should_flush(combat, combat.player) is False

        combat.round_number = 2
        assert should_flush(combat, combat.player) is True

    def test_screaming_flagon_damages_hittable_enemies_when_hand_empty(self):
        combat = _make_combat(["SCREAMING_FLAGON"], seed=902)
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = []

        fire_before_turn_end(CombatSide.PLAYER, combat)

        assert enemy.current_hp == 80

    def test_sling_of_courage_grants_strength_only_in_elite_combat(self):
        elite = _make_combat(["SLING_OF_COURAGE"], seed=906)
        elite.is_elite = True
        relic = next(relic for relic in elite.relics if relic.relic_id.name == "SLING_OF_COURAGE")

        relic.before_combat_start(elite.player, elite)
        assert elite.player.get_power_amount(PowerId.STRENGTH) == 2

        regular = _make_combat(["SLING_OF_COURAGE"], seed=907)
        regular_relic = next(relic for relic in regular.relics if relic.relic_id.name == "SLING_OF_COURAGE")

        regular_relic.before_combat_start(regular.player, regular)
        assert regular.player.get_power_amount(PowerId.STRENGTH) == 0

    def test_very_hot_cocoa_grants_round_one_energy_only(self):
        combat = _make_combat(["VERY_HOT_COCOA"], seed=903)

        assert combat.energy == 7
        combat.end_player_turn()
        assert combat.round_number == 2
        assert combat.energy == 3

    def test_meat_cleaver_cook_option_is_visible_but_disabled_when_too_few_cards(self):
        run_state = RunState(seed=904, character_id="Ironclad")
        run_state.player.deck = [make_wound()]
        assert run_state.player.obtain_relic("MEAT_CLEAVER")

        options = generate_rest_site_options(run_state.player, run_state.player.relics)
        cook = next(option for option in options if option.option_id == "COOK")

        assert cook.enabled is False

    def test_meat_cleaver_cook_option_enabled_with_two_removable_cards(self):
        run_state = RunState(seed=905, character_id="Ironclad")
        run_state.player.deck = [make_strike_ironclad(), make_strike_ironclad()]
        assert run_state.player.obtain_relic("MEAT_CLEAVER")

        options = generate_rest_site_options(run_state.player, run_state.player.relics)
        cook = next(option for option in options if option.option_id == "COOK")

        assert cook.enabled is True

    def test_meat_cleaver_cook_can_remove_status_and_curse_cards(self):
        run_state = RunState(seed=906, character_id="Ironclad")
        run_state.enable_deck_choice_requests = True
        run_state.player.deck = [make_wound(), make_clumsy()]
        assert run_state.player.obtain_relic("MEAT_CLEAVER")
        cook = next(option for option in generate_rest_site_options(run_state.player) if option.option_id == "COOK")

        result = cook.execute(run_state.player)

        assert result == "Choose 2 cards to remove"
        assert run_state.pending_choice is not None
        assert [option.card.card_id for option in run_state.pending_choice.options] == [
            make_clumsy().card_id,
            make_wound().card_id,
        ]

    def test_fiddle_adds_two_to_opening_hand_draw(self):
        combat = _make_combat(["Fiddle"])
        assert len(combat.hand) == 7

    def test_fiddle_blocks_non_hand_draws_only_on_owner_turn(self):
        combat = _make_combat(["Fiddle"], seed=893)
        combat.hand = []
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad()]

        combat.current_side = CombatSide.PLAYER
        combat.draw_cards(combat.player, 1)
        assert len(combat.hand) == 0
        assert len(combat.draw_pile) == 2

        combat.current_side = CombatSide.ENEMY
        combat.draw_cards(combat.player, 1)
        assert len(combat.hand) == 1
        assert len(combat.draw_pile) == 1

    def test_jeweled_mask_moves_power_to_hand_and_makes_it_free(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=890,
            character_id="Ironclad",
            relics=["JeweledMask"],
        )
        power = make_inflame()
        combat.draw_pile = [make_strike_ironclad() for _ in range(6)] + [power]
        combat.hand = []
        combat.discard_pile = []
        combat.exhaust_pile = []

        combat._start_player_turn()  # noqa: SLF001

        assert power in combat.hand
        assert power not in combat.draw_pile
        assert combat.modified_card_cost(combat.player, power) == 0

    def test_pollinous_core_resets_turn_count_after_bonus_draw(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=891,
            character_id="Ironclad",
            relics=["PollinousCore"],
        )
        relic = next(relic for relic in combat.relics if relic.relic_id.name == "POLLINOUS_CORE")
        relic._turns_seen = 3  # noqa: SLF001
        combat.draw_pile = [make_strike_ironclad() for _ in range(8)]
        combat.hand = []
        combat.discard_pile = []
        combat.exhaust_pile = []

        combat._start_player_turn()  # noqa: SLF001

        assert len(combat.hand) == 7
        assert relic._turns_seen == 0  # noqa: SLF001

    def test_toasty_mittens_exhausts_chosen_hand_card_and_grants_strength(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=892,
            character_id="Ironclad",
            relics=["ToastyMittens"],
        )
        innate = make_backstab()
        non_innate = make_strike_ironclad()
        combat.draw_pile = [make_strike_ironclad() for _ in range(8)]
        combat.hand = [innate, non_innate]
        combat.discard_pile = []
        combat.exhaust_pile = []

        combat._start_player_turn()  # noqa: SLF001

        assert combat.pending_choice is not None
        target_index = next(
            index
            for index, option in enumerate(combat.pending_choice.options)
            if option.card is non_innate
        )
        assert combat.resolve_pending_choice(target_index)

        assert innate in combat.hand
        assert non_innate in combat.exhaust_pile
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1

    def test_toasty_mittens_prompts_again_on_later_turns(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=892,
            character_id="Ironclad",
            relics=["ToastyMittens"],
        )
        creature, ai = create_shrinker_beetle(Rng(892))
        combat.add_enemy(creature, ai)
        combat.start_combat()

        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1

        combat.end_player_turn()

        assert combat.pending_choice is not None

    def test_pomander_queues_single_upgrade_reward_when_followups_are_deferred(self):
        run_state = RunState(seed=882, character_id="Ironclad")
        run_state.defer_followup_rewards = True

        assert run_state.player.obtain_relic("POMANDER")
        assert len(run_state.pending_rewards) == 1
        reward = run_state.pending_rewards[0]
        assert isinstance(reward, UpgradeCardsReward)
        assert reward.count == 1

    def test_preserved_fog_queues_remove_five_and_folly_reward(self):
        run_state = RunState(seed=883, character_id="Ironclad")
        run_state.defer_followup_rewards = True

        assert run_state.player.obtain_relic("PRESERVED_FOG")
        assert len(run_state.pending_rewards) == 1
        reward = run_state.pending_rewards[0]
        assert isinstance(reward, RemoveCardReward)
        assert reward.count == 5
        assert reward.after_remove_card_names == ("Folly",)

    def test_pumpkin_candle_only_grants_energy_in_act_obtained(self):
        run_state = RunState(seed=884, character_id="Ironclad")
        run_state.current_act_index = 1
        run_state.player.deck = create_ironclad_starter_deck()
        assert run_state.player.obtain_relic("PUMPKIN_CANDLE")

        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=list(run_state.player.deck),
            rng_seed=884,
            character_id="Ironclad",
            player_state=run_state.player,
        )
        assert combat.max_energy == 4

        run_state.current_act_index = 2
        next_act_combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=list(run_state.player.deck),
            rng_seed=885,
            character_id="Ironclad",
            player_state=run_state.player,
        )
        assert next_act_combat.max_energy == 3

    def test_radiant_pearl_adds_luminesce_to_hand_on_round_one(self):
        combat = _make_combat(["RadiantPearl"], seed=885)
        assert any(card.card_id == CardId.LUMINESCE for card in combat.hand)

    def test_royal_poison_deals_four_unblockable_damage_on_round_one(self):
        combat = _make_combat(["RoyalPoison"], seed=886)
        assert combat.player.current_hp == 76

    def test_sai_grants_seven_block_each_player_turn(self):
        combat = _make_combat(["Sai"], seed=887)
        assert combat.player.block == 7
        combat.end_player_turn()
        assert combat.player.block == 7

    def test_sai_block_triggers_after_block_gained_hooks(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=894,
            character_id="Ironclad",
            relics=["Sai"],
        )
        enemy, ai = create_shrinker_beetle(Rng(894))
        combat.add_enemy(enemy, ai)
        start_hp = enemy.current_hp
        combat.player.apply_power(PowerId.JUGGERNAUT, 5)

        combat.start_combat()

        assert combat.player.block == 7
        assert enemy.current_hp == start_hp - 5

    def test_signet_ring_grants_eight_hundred_eighty_eight_gold_on_obtain(self):
        run_state = RunState(seed=888, character_id="Ironclad")
        starting_gold = run_state.player.gold

        assert run_state.player.obtain_relic("SIGNET_RING")
        assert run_state.player.gold == starting_gold + 888

    def test_small_capsule_queues_one_relic_reward(self):
        run_state = RunState(seed=889, character_id="Ironclad")

        assert run_state.player.obtain_relic("SMALL_CAPSULE")
        rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, RelicReward)]
        assert len(rewards) == 1
