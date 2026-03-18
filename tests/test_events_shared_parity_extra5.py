"""Focused parity tests for additional shared events coverage (extra5)."""

import sts2_env.events.shared  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_decay, make_doubt, make_greed
from sts2_env.core.enums import CardId, CardType, TargetType
from sts2_env.events.shared import (
    ColossalFlower,
    Darv,
    DenseVegetation,
    DoorsOfLightAndDark,
    Nonupeipe,
    Pael,
    PunchOff,
    SpiritGrafter,
    Tanx,
    Tezcatara,
    TheLanternKey,
    TinkerTime,
    Vakuu,
)
from sts2_env.relics.base import RelicId
from sts2_env.run.reward_objects import AddCardsReward
from sts2_env.run.run_state import RunState


def _make_run_state(seed: int = 601, character_id: str = "Ironclad") -> RunState:
    run_state = RunState(seed=seed, character_id=character_id)
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    return run_state


def _count_card(deck, card_id: CardId) -> int:
    return sum(1 for card in deck if card.card_id == card_id)


def test_colossal_flower_progression_scales_gold_damage_and_final_relic():
    shallow_state = _make_run_state(601)
    shallow_state.player.current_hp = 18
    assert ColossalFlower().is_allowed(shallow_state) is False

    run_state = _make_run_state(602)
    event = ColossalFlower()
    start_hp = run_state.player.current_hp
    start_gold = run_state.player.gold

    first = event.choose(run_state, "reach_deeper")
    assert first.finished is False
    extract = event.choose(run_state, "extract")
    assert extract.finished
    assert run_state.player.current_hp == start_hp - 5
    assert run_state.player.gold == start_gold + 75

    core_state = _make_run_state(603)
    core_event = ColossalFlower()
    hp_before_core = core_state.player.current_hp
    relics_before_core = len(core_state.player.relics)
    core_event.choose(core_state, "reach_deeper")
    core_event.choose(core_state, "reach_deeper")
    core = core_event.choose(core_state, "pollinous_core")

    assert core.finished
    assert core_state.player.current_hp == hp_before_core - 18
    assert len(core_state.player.relics) == relics_before_core + 1
    assert core_state.player.relics[-1] == RelicId.POLLINOUS_CORE.name


def test_darv_includes_dusty_tome_branch_and_awards_selected_boss_relic():
    run_state = _make_run_state(604)
    run_state.current_act_index = 1
    run_state.rng.up_front.shuffle = lambda seq: None
    run_state.rng.up_front.choice = lambda seq: seq[-1]
    run_state.rng.up_front.next_int = lambda low, high: 1

    event = Darv()
    options = event.generate_initial_options(run_state)
    assert len(options) == 3
    assert any(relic_id == RelicId.DUSTY_TOME.name for relic_id, _ in event._choices.values())

    dusty_option = next(
        option for option in options if event._choices[option.option_id][0] == RelicId.DUSTY_TOME.name
    )
    dusty_relic_id, dusty_attrs = event._choices[dusty_option.option_id]
    relics_before = len(run_state.player.relics)
    result = event.choose(run_state, dusty_option.option_id)

    assert result.finished
    assert len(run_state.player.relics) == relics_before + 1
    assert run_state.player.relics[-1] == dusty_relic_id
    assert run_state.player.relic_objects[-1]._ancient_card_id == dusty_attrs["_ancient_card_id"]
    assert any(
        card.card_id.name == dusty_attrs["_ancient_card_id"] and card.upgraded
        for card in run_state.player.deck
    )


def test_dense_vegetation_trudge_and_rest_paths_mutate_hp_deck_and_combat_setup():
    trudge_state = _make_run_state(605)
    trudge_event = DenseVegetation()
    hp_before = trudge_state.player.current_hp
    deck_before = len(trudge_state.player.deck)

    trudge = trudge_event.choose(trudge_state, "trudge_on")
    assert trudge.finished is False
    removed = trudge_event.pending_choice.options[0].card
    trudge_final = trudge_event.resolve_pending_choice(0)

    assert trudge_final.finished
    assert trudge_state.player.current_hp == hp_before - 11
    assert len(trudge_state.player.deck) == deck_before - 1
    assert all(card.instance_id != removed.instance_id for card in trudge_state.player.deck)

    rest_state = _make_run_state(606)
    rest_state.player.current_hp = 20
    rest_event = DenseVegetation()
    heal_amount = int(rest_state.player.max_hp * 0.3)

    rest = rest_event.choose(rest_state, "rest")
    assert rest.finished is False
    assert rest_state.player.current_hp == 20 + heal_amount
    fight = rest_event.choose(rest_state, "fight")
    assert fight.finished
    assert fight.event_combat_setup == "dense_vegetation"


def test_doors_of_light_and_dark_upgrades_two_and_removes_one_card():
    run_state = _make_run_state(607)
    event = DoorsOfLightAndDark()
    deck_before = len(run_state.player.deck)

    light = event.choose(run_state, "light")
    assert light.finished
    assert sum(1 for card in run_state.player.deck if card.upgraded) >= 2

    dark = event.choose(run_state, "dark")
    assert dark.finished is False
    dark_final = event.resolve_pending_choice(0)
    assert dark_final.finished
    assert len(run_state.player.deck) == deck_before - 1


def test_nonupeipe_and_tanx_conditional_relics_are_gated_by_enchantable_deck():
    nonu_state = _make_run_state(608)
    nonu_state.rng.up_front.shuffle = lambda seq: seq.reverse()
    nonu = Nonupeipe()
    nonu.generate_initial_options(nonu_state)
    assert RelicId.BEAUTIFUL_BRACELET.name in nonu._choices.values()

    bracelet_option = next(
        option_id
        for option_id, relic_id in nonu._choices.items()
        if relic_id == RelicId.BEAUTIFUL_BRACELET.name
    )
    nonu_before = len(nonu_state.player.relics)
    nonu.choose(nonu_state, bracelet_option)
    assert len(nonu_state.player.relics) == nonu_before + 1
    assert nonu_state.player.relics[-1] == RelicId.BEAUTIFUL_BRACELET.name

    nonu_excluded_state = _make_run_state(609)
    nonu_excluded_state.player.deck = [make_decay(), make_doubt(), make_greed(), make_decay()]
    nonu_excluded_state.rng.up_front.shuffle = lambda seq: seq.reverse()
    nonu_excluded = Nonupeipe()
    nonu_excluded.generate_initial_options(nonu_excluded_state)
    assert RelicId.BEAUTIFUL_BRACELET.name not in nonu_excluded._choices.values()

    tanx_state = _make_run_state(610)
    tanx_state.rng.up_front.shuffle = lambda seq: seq.reverse()
    tanx = Tanx()
    tanx.generate_initial_options(tanx_state)
    assert RelicId.TRI_BOOMERANG.name in tanx._choices.values()

    boomerang_option = next(
        option_id
        for option_id, relic_id in tanx._choices.items()
        if relic_id == RelicId.TRI_BOOMERANG.name
    )
    tanx_before = len(tanx_state.player.relics)
    tanx.choose(tanx_state, boomerang_option)
    assert len(tanx_state.player.relics) == tanx_before + 1
    assert tanx_state.player.relics[-1] == RelicId.TRI_BOOMERANG.name

    tanx_excluded_state = _make_run_state(611)
    tanx_excluded_state.player.deck = [make_decay(), make_doubt(), make_greed(), make_decay()]
    tanx_excluded_state.rng.up_front.shuffle = lambda seq: seq.reverse()
    tanx_excluded = Tanx()
    tanx_excluded.generate_initial_options(tanx_excluded_state)
    assert RelicId.TRI_BOOMERANG.name not in tanx_excluded._choices.values()


def test_pael_pool_conditions_and_legion_lockout_follow_deck_and_relic_state():
    run_state = _make_run_state(612)
    run_state.rng.up_front.choice = lambda seq: seq[-1]
    pael = Pael()
    pael.generate_initial_options(run_state)

    assert pael._choices["relic_2"] == RelicId.PAELS_TOOTH.name
    assert pael._choices["relic_3"] == RelicId.PAELS_LEGION.name
    relics_before = len(run_state.player.relics)
    pael.choose(run_state, "relic_3")
    assert len(run_state.player.relics) == relics_before + 1
    assert run_state.player.relics[-1] == RelicId.PAELS_LEGION.name

    locked_state = _make_run_state(613)
    locked_state.player.relics.append(RelicId.PAELS_LEGION.name)
    locked_state.player.deck = [make_decay(), make_doubt(), make_greed(), make_decay()]
    locked_state.rng.up_front.choice = lambda seq: seq[-1]
    locked_pael = Pael()
    locked_pael.generate_initial_options(locked_state)

    assert locked_pael._choices["relic_2"] == RelicId.PAELS_GROWTH.name
    assert locked_pael._choices["relic_3"] == RelicId.PAELS_BLOOD.name

    byrdpip_state = _make_run_state(6141)
    byrdpip_state.player.obtain_relic(RelicId.BYRDPIP.name)
    byrdpip_state.rng.up_front.choice = lambda seq: seq[-1]
    byrdpip_pael = Pael()
    byrdpip_pael.generate_initial_options(byrdpip_state)
    assert byrdpip_pael._choices["relic_3"] == RelicId.PAELS_BLOOD.name


def test_punch_off_and_spirit_grafter_apply_curses_rewards_and_card_removal_damage():
    punch_state = _make_run_state(614)
    punch = PunchOff()
    injury_before = _count_card(punch_state.player.deck, CardId.INJURY)

    nab = punch.choose(punch_state, "nab")
    assert nab.finished
    assert _count_card(punch_state.player.deck, CardId.INJURY) == injury_before + 1
    assert [reward.reward_type.name for reward in nab.rewards["reward_objects"]] == ["RELIC"]

    take_them = punch.choose(punch_state, "take_them")
    assert take_them.finished is False
    fight = punch.choose(punch_state, "fight")
    assert fight.finished
    assert fight.event_combat_setup == "punch_off"
    assert [reward.reward_type.name for reward in fight.rewards["reward_objects"]] == ["RELIC", "POTION"]

    grafter_state = _make_run_state(615)
    grafter_state.player.current_hp = 20
    grafter = SpiritGrafter()
    metamorphosis_before = _count_card(grafter_state.player.deck, CardId.METAMORPHOSIS)

    let_it_in = grafter.choose(grafter_state, "let_it_in")
    assert let_it_in.finished
    assert grafter_state.player.current_hp == 45
    assert _count_card(grafter_state.player.deck, CardId.METAMORPHOSIS) == metamorphosis_before + 1

    deck_before = len(grafter_state.player.deck)
    hp_before = grafter_state.player.current_hp
    rejection = grafter.choose(grafter_state, "rejection")
    assert rejection.finished is False
    rejection_final = grafter.resolve_pending_choice(0)
    assert rejection_final.finished
    assert len(grafter_state.player.deck) == deck_before - 1
    assert grafter_state.player.current_hp == hp_before - 9


def test_tezcatara_and_vakuu_relic_selection_tracks_pool_rolls():
    tez_state = _make_run_state(616)
    tez_state.rng.up_front.choice = lambda seq: seq[-1]
    tezcatara = Tezcatara()
    tezcatara.generate_initial_options(tez_state)

    assert tezcatara._choices["comfort_1"] == RelicId.YUMMY_COOKIE.name
    assert tezcatara._choices["comfort_2"] == RelicId.TOASTY_MITTENS.name
    assert tezcatara._choices["comfort_3"] == RelicId.TOY_BOX.name
    tez_relics_before = len(tez_state.player.relics)
    tezcatara.choose(tez_state, "comfort_2")
    assert len(tez_state.player.relics) == tez_relics_before + 1
    assert tez_state.player.relics[-1] == RelicId.TOASTY_MITTENS.name

    vakuu_state = _make_run_state(617)
    vakuu_state.rng.up_front.shuffle = lambda seq: seq.reverse()
    vakuu = Vakuu()
    vakuu.generate_initial_options(vakuu_state)

    assert vakuu._choices["relic_1"] == RelicId.FIDDLE.name
    assert vakuu._choices["relic_2"] == RelicId.DISTINGUISHED_CAPE.name
    assert vakuu._choices["relic_3"] == RelicId.JEWELED_MASK.name
    vakuu_relics_before = len(vakuu_state.player.relics)
    vakuu.choose(vakuu_state, "relic_3")
    assert len(vakuu_state.player.relics) == vakuu_relics_before + 1
    assert vakuu_state.player.relics[-1] == RelicId.JEWELED_MASK.name


def test_lantern_key_and_tinker_time_yield_expected_reward_and_card_configuration():
    return_state = _make_run_state(618)
    return_event = TheLanternKey()
    gold_before = return_state.player.gold
    returned = return_event.choose(return_state, "return_key")
    assert returned.finished
    assert return_state.player.gold == gold_before + 100

    key_state = _make_run_state(619)
    key_event = TheLanternKey()
    keep = key_event.choose(key_state, "keep_key")
    assert keep.finished is False
    fight = key_event.choose(key_state, "fight")
    assert fight.finished
    assert fight.event_combat_setup == "mysterious_knight"
    reward = fight.rewards["reward_objects"][0]
    assert isinstance(reward, AddCardsReward)
    assert reward.cards[0].card_id == CardId.LANTERN_KEY

    skill_state = _make_run_state(620)
    skill_state.rng.up_front.shuffle = lambda seq: None
    skill_event = TinkerTime()
    assert [option.option_id for option in skill_event.generate_initial_options(skill_state)] == ["choose_card_type"]
    skill_first = skill_event.choose(skill_state, "choose_card_type")
    assert skill_first.finished is False
    assert "skill" in {option.option_id for option in skill_first.next_options}
    skill_second = skill_event.choose(skill_state, "skill")
    assert skill_second.finished is False
    skill_rider = skill_second.next_options[0].option_id
    skill_final = skill_event.choose(skill_state, skill_rider)
    assert skill_final.finished
    skill_card = skill_state.player.deck[-1]
    assert skill_card.card_id == CardId.MAD_SCIENCE
    assert skill_card.card_type == CardType.SKILL
    assert skill_card.target_type == TargetType.SELF
    assert skill_card.base_block == 8
    assert skill_card.base_damage is None
    assert skill_card.effect_vars["rider"] in {4, 5, 6}

    power_state = _make_run_state(621)
    power_state.rng.up_front.shuffle = lambda seq: seq.reverse()
    power_event = TinkerTime()
    assert [option.option_id for option in power_event.generate_initial_options(power_state)] == ["choose_card_type"]
    power_first = power_event.choose(power_state, "choose_card_type")
    assert power_first.finished is False
    assert "power" in {option.option_id for option in power_first.next_options}
    power_second = power_event.choose(power_state, "power")
    assert power_second.finished is False
    power_rider = power_second.next_options[0].option_id
    power_final = power_event.choose(power_state, power_rider)
    assert power_final.finished
    power_card = power_state.player.deck[-1]
    assert power_card.card_id == CardId.MAD_SCIENCE
    assert power_card.card_type == CardType.POWER
    assert power_card.target_type == TargetType.SELF
    assert power_card.base_damage is None
    assert power_card.base_block is None
    assert power_card.effect_vars["rider"] in {7, 8, 9}
