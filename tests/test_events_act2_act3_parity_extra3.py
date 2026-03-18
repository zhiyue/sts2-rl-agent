"""Focused parity coverage for remaining Act 2 / Act 3 events."""

import sts2_env.events.act2  # noqa: F401
import sts2_env.events.act3  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_spore_mind
from sts2_env.core.enums import CardId, ValueProp
from sts2_env.events.act2 import (
    CrystalSphere,
    EndlessConveyor,
    FakeMerchant,
    FieldOfManSizedHoles,
    WelcomeToWongos,
)
from sts2_env.events.act3 import TheArchitect
from sts2_env.potions.base import create_potion
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import RunState


def _make_run_state(seed: int) -> RunState:
    run_state = RunState(seed=seed, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    return run_state


def test_the_architect_is_pool_disabled_and_has_single_proceed_option():
    run_state = _make_run_state(901)
    event = TheArchitect()

    assert event.is_allowed(run_state) is False
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["proceed"]

    result = event.choose(run_state, "proceed")
    assert result.finished
    assert "architect" in result.description.lower()
    assert result.event_combat_setup == "the_architect"
    assert result.post_combat_phase == "RUN_OVER"


def test_run_manager_enters_the_architect_combat_and_ends_run_after_victory():
    mgr = RunManager(seed=901, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    event = TheArchitect()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)

    result = mgr._do_event_choice({"option_id": "proceed"})
    assert result["phase"] == RunManager.PHASE_COMBAT

    combat = mgr._combat  # noqa: SLF001
    enemy = combat.enemies[0]
    combat.deal_damage(combat.player, enemy, 99999, ValueProp.UNPOWERED)
    resolved = mgr._resolve_combat_end()  # noqa: SLF001
    assert resolved["phase"] == RunManager.PHASE_RUN_OVER
    assert mgr.run_state.is_over is True
    assert mgr.run_state.player_won is True


def test_crystal_sphere_thresholds_and_choices_apply_cost_or_debt():
    blocked_state = _make_run_state(902)
    event = CrystalSphere()
    assert event.is_allowed(blocked_state) is False

    blocked_state.current_act_index = 1
    blocked_state.player.gold = 99
    assert event.is_allowed(blocked_state) is False

    run_state = _make_run_state(903)
    run_state.current_act_index = 1
    run_state.player.gold = 180
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["pay", "debt"]
    assert 51 <= event._cost <= 100  # noqa: SLF001

    gold_before = run_state.player.gold
    pay = event.choose(run_state, "pay")
    assert pay.finished
    assert run_state.player.gold == gold_before - event._cost  # noqa: SLF001

    debt_state = _make_run_state(904)
    debt_state.current_act_index = 1
    debt_state.player.gold = 200
    debt_event = CrystalSphere()
    debt_event.generate_initial_options(debt_state)
    deck_before = len(debt_state.player.deck)

    debt = debt_event.choose(debt_state, "debt")
    assert debt.finished
    assert len(debt_state.player.deck) == deck_before + 1
    assert debt_state.player.deck[-1].card_id == CardId.DEBT


def test_endless_conveyor_observe_and_targeted_dishes_apply_expected_effects():
    observe_state = _make_run_state(905)
    observe_state.player.gold = 200
    observe = EndlessConveyor()
    observe.generate_initial_options(observe_state)
    observed = observe.choose(observe_state, "observe")
    assert observed.finished
    assert any(card.upgraded for card in observe_state.player.deck)

    run_state = _make_run_state(906)
    run_state.player.gold = 200
    event = EndlessConveyor()

    event._current_dish = "golden_fysh"  # noqa: SLF001
    gold_before = run_state.player.gold
    golden = event.choose(run_state, "grab")
    assert not golden.finished
    assert run_state.player.gold == gold_before + 75

    event._current_dish = "caviar"  # noqa: SLF001
    run_state.player.gold = 35
    max_hp_before = run_state.player.max_hp
    caviar = event.choose(run_state, "grab")
    assert not caviar.finished
    assert run_state.player.max_hp == max_hp_before + 4
    assert run_state.player.gold == 0
    assert [option.option_id for option in caviar.next_options] == ["grab", "leave"]
    assert [option.enabled for option in caviar.next_options] == [False, True]

    event._current_dish = "seapunk_salad"  # noqa: SLF001
    run_state.player.gold = 100
    deck_before = len(run_state.player.deck)
    seapunk = event.choose(run_state, "grab")
    assert not seapunk.finished
    assert len(run_state.player.deck) == deck_before + 1
    assert run_state.player.deck[-1].card_id == CardId.FEEDING_FRENZY_CARD

    event._current_dish = "fried_eel"  # noqa: SLF001
    deck_before = len(run_state.player.deck)
    eel = event.choose(run_state, "grab")
    assert not eel.finished
    assert len(run_state.player.deck) == deck_before + 1


def test_fake_merchant_gating_options_and_choices_reflect_foul_potion_and_gold():
    blocked_state = _make_run_state(907)
    blocked_state.current_act_index = 1
    blocked_state.player.gold = 99
    event = FakeMerchant()
    assert event.is_allowed(blocked_state) is False

    option_state = _make_run_state(908)
    option_state.current_act_index = 1
    option_state.player.gold = 120
    options = event.generate_initial_options(option_state)
    assert [option.option_id for option in options] == ["buy", "leave"]

    foul_state = _make_run_state(909)
    foul_state.current_act_index = 1
    foul_state.player.gold = 0
    foul_state.player.add_potion(create_potion("FoulPotion"))
    assert event.is_allowed(foul_state)
    foul_options = event.generate_initial_options(foul_state)
    assert [option.option_id for option in foul_options] == ["buy", "throw_foul", "leave"]

    gold_before = option_state.player.gold
    buy = event.choose(option_state, "buy")
    assert not buy.finished
    assert buy.next_options[-1].option_id == "leave"
    purchase = event.choose(option_state, buy.next_options[0].option_id)
    assert purchase.finished is False
    assert option_state.player.gold == gold_before - 50
    assert len(event._inventories[id(option_state)]) == 5  # noqa: SLF001
    assert any(option.option_id == "leave" for option in purchase.next_options)

    throw_foul = event.choose(foul_state, "throw_foul")
    assert throw_foul.finished
    assert throw_foul.event_combat_setup == "fake_merchant"
    assert len(throw_foul.rewards["reward_objects"]) == 7
    assert all(p.potion_id != "FoulPotion" for p in foul_state.player.held_potions())


def test_fake_merchant_throw_foul_only_rewards_current_stock():
    run_state = _make_run_state(919)
    run_state.current_act_index = 1
    run_state.player.gold = 200
    run_state.player.add_potion(create_potion("FoulPotion"))
    event = FakeMerchant()
    event.generate_initial_options(run_state)

    buy = event.choose(run_state, "buy")
    assert not buy.finished
    event.choose(run_state, buy.next_options[0].option_id)

    throw_foul = event.choose(run_state, "throw_foul")
    rewards = throw_foul.rewards["reward_objects"]
    assert len(rewards) == 6


def test_run_manager_enters_combat_for_fake_merchant_throw_foul():
    mgr = RunManager(seed=909, character_id="Ironclad")
    mgr.run_state.current_act_index = 1
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr.run_state.player.gold = 0
    mgr.run_state.player.add_potion(create_potion("FoulPotion"))
    mgr._phase = RunManager.PHASE_EVENT
    event = FakeMerchant()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)

    result = mgr._do_event_choice({"option_id": "throw_foul"})
    assert result["phase"] == RunManager.PHASE_COMBAT


def test_welcome_to_wongos_thresholds_purchase_effects_and_leave_downgrade():
    threshold_state = _make_run_state(910)
    threshold_state.current_act_index = 1
    event = WelcomeToWongos()

    threshold_state.player.gold = 100
    options = event.generate_initial_options(threshold_state)
    assert [option.option_id for option in options] == [
        "bargain_bin",
        "featured",
        "mystery",
        "leave",
    ]
    assert [option.enabled for option in options] == [True, False, False, True]

    threshold_state.player.gold = 200
    options = event.generate_initial_options(threshold_state)
    assert [option.option_id for option in options] == [
        "bargain_bin",
        "featured",
        "mystery",
        "leave",
    ]
    assert [option.enabled for option in options] == [True, True, False, True]

    threshold_state.player.gold = 300
    options = event.generate_initial_options(threshold_state)
    assert [option.option_id for option in options] == [
        "bargain_bin",
        "featured",
        "mystery",
        "leave",
    ]
    assert [option.enabled for option in options] == [True, True, True, True]

    bargain_state = _make_run_state(911)
    bargain_state.current_act_index = 1
    bargain_state.player.gold = 400
    bargain_event = WelcomeToWongos()
    relics_before = len(bargain_state.player.relics)
    gold_before = bargain_state.player.gold
    points_before = bargain_state.player.wongo_points
    bargain = bargain_event.choose(bargain_state, "bargain_bin")
    assert bargain.finished
    assert bargain_state.player.gold == gold_before - 100
    assert len(bargain_state.player.relics) == relics_before + 1
    assert bargain_state.player.wongo_points == points_before + 32

    featured_state = _make_run_state(912)
    featured_state.current_act_index = 1
    featured_state.player.gold = 400
    featured_event = WelcomeToWongos()
    featured_event.generate_initial_options(featured_state)
    featured_id = featured_event._featured_relic_id
    relics_before = len(featured_state.player.relics)
    gold_before = featured_state.player.gold
    points_before = featured_state.player.wongo_points
    featured = featured_event.choose(featured_state, "featured")
    assert featured.finished
    assert featured_state.player.gold == gold_before - 200
    assert len(featured_state.player.relics) == relics_before + 1
    assert featured_state.player.relics[-1] == featured_id
    assert featured_state.player.wongo_points == points_before + 16

    mystery_state = _make_run_state(913)
    mystery_state.current_act_index = 1
    mystery_state.player.gold = 400
    mystery_event = WelcomeToWongos()
    relics_before = len(mystery_state.player.relics)
    gold_before = mystery_state.player.gold
    points_before = mystery_state.player.wongo_points
    mystery = mystery_event.choose(mystery_state, "mystery")
    assert mystery.finished
    assert mystery_state.player.gold == gold_before - 300
    assert len(mystery_state.player.relics) == relics_before + 1
    assert "WONGOS_MYSTERY_TICKET" in mystery_state.player.relics
    assert mystery_state.player.wongo_points == points_before + 8

    badge_state = _make_run_state(9131)
    badge_state.current_act_index = 1
    badge_state.player.gold = 400
    badge_state.player.wongo_points = 1992
    badge_event = WelcomeToWongos()
    badge = badge_event.choose(badge_state, "mystery")
    assert badge.finished
    assert badge_state.player.wongo_points == 2000
    assert "WONGO_CUSTOMER_APPRECIATION_BADGE" in badge_state.player.relics
    assert not badge.rewards

    mgr = RunManager(seed=9132, character_id="Ironclad")
    mgr.run_state.current_act_index = 1
    mgr.run_state.player.gold = 400
    mgr.run_state.player.wongo_points = 1992
    mgr._phase = RunManager.PHASE_EVENT
    mgr._event_model = WelcomeToWongos()
    mgr._event_options = mgr._event_model.generate_initial_options(mgr.run_state)
    result = mgr._do_event_choice({"option_id": "mystery"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    reward_ids = [getattr(mgr._current_reward, "relic_id", None)]
    reward_ids.extend(reward.relic_id for reward in mgr._pending_rewards if hasattr(reward, "relic_id"))
    assert "WONGOS_MYSTERY_TICKET" in reward_ids
    assert "WONGO_CUSTOMER_APPRECIATION_BADGE" in reward_ids

    leave_state = _make_run_state(914)
    leave_state.current_act_index = 1
    leave_state.player.gold = 300
    leave_state.player.deck = [
        create_card(CardId.BASH, upgraded=True),
        create_card(CardId.STRIKE_IRONCLAD),
    ]
    leave_event = WelcomeToWongos()
    leave = leave_event.choose(leave_state, "leave")
    assert leave.finished
    assert leave_state.player.deck[0].upgraded is False


def test_field_of_man_sized_holes_gate_resist_and_enter_behaviors():
    blocked_state = _make_run_state(915)
    blocked_state.player.deck = [make_spore_mind()]
    event = FieldOfManSizedHoles()
    assert event.is_allowed(blocked_state) is False

    resist_state = _make_run_state(916)
    deck_before = len(resist_state.player.deck)
    resist = event.choose(resist_state, "resist")
    assert resist.finished
    assert len(resist_state.player.deck) == deck_before - 1
    assert any(card.card_id == CardId.NORMALITY for card in resist_state.player.deck)

    enter_state = _make_run_state(917)
    enter = event.choose(enter_state, "enter")
    assert not enter.finished
    assert event.pending_choice is not None
    target = event.pending_choice.options[0].card
    resolved = event.resolve_pending_choice(0)
    assert resolved.finished
    assert target.enchantments.get("PerfectFit") == 1
