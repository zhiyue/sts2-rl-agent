"""Focused regressions for combat-start and reward-modifying shop/event relics."""

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import make_inflame
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.reward_objects import CardReward
from sts2_env.run.run_state import RunState


def test_wing_charm_enchants_exactly_one_reward_card_with_swift() -> None:
    run_state = RunState(seed=701, character_id="Ironclad")
    assert run_state.player.obtain_relic("WING_CHARM")

    reward = CardReward(
        run_state.player.player_id,
        cards=[
            create_card(CardId.ANGER),
            create_card(CardId.SHRUG_IT_OFF),
            create_card(CardId.INFLAME),
        ],
    )
    reward.populate(run_state, None)

    swift_cards = [card for card in reward.cards if card.enchantments.get("Swift") == 1]
    assert len(swift_cards) == 1


def test_jeweled_mask_moves_a_power_from_draw_to_hand_for_free_on_round_one() -> None:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[
            make_inflame(),
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_strike_ironclad(),
        ],
        relics=["JEWELED_MASK"],
        rng_seed=702,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(702))
    combat.add_enemy(creature, ai)

    combat.start_combat()

    inflames = [card for card in combat.hand if card.card_id == CardId.INFLAME]
    assert len(inflames) == 1
    assert inflames[0].cost == 0
    assert len(combat.hand) == 6
    assert all(card.card_id != CardId.INFLAME for card in combat.draw_pile)


def test_toolbox_pauses_before_opening_draw_and_adds_selected_colorless_to_hand() -> None:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[make_strike_ironclad() for _ in range(6)],
        relics=["TOOLBOX"],
        rng_seed=703,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(703))
    combat.add_enemy(creature, ai)

    combat.start_combat()

    assert combat.pending_choice is not None
    assert len(combat.hand) == 0
    assert len(combat.pending_choice.options) == 3

    assert combat.resolve_pending_choice(0)

    assert combat.pending_choice is None
    assert len(combat.hand) == 6
    assert any(card.card_id not in {CardId.STRIKE_IRONCLAD} for card in combat.hand)


def test_choices_paradox_prompts_after_opening_draw_and_selected_card_retains() -> None:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[make_strike_ironclad() for _ in range(8)],
        relics=["CHOICES_PARADOX"],
        rng_seed=704,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(704))
    combat.add_enemy(creature, ai)

    combat.start_combat()

    assert combat.pending_choice is not None
    assert len(combat.hand) == 5
    assert len(combat.pending_choice.options) == 5
    assert all(option.card.is_retain for option in combat.pending_choice.options)

    selected_id = combat.pending_choice.options[0].card.card_id
    assert combat.resolve_pending_choice(0)

    retained_cards = [card for card in combat.hand if card.card_id == selected_id]
    assert retained_cards
    assert retained_cards[-1].is_retain
