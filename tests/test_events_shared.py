"""Focused tests for shared event parity improvements."""

import sts2_env.events.shared  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.silent import make_backstab
from sts2_env.cards.status import make_clumsy, make_decay, make_doubt, make_exterminate, make_greed, make_guilty, make_injury, make_lantern_key, make_metamorphosis, make_poor_sleep, make_regret, make_squash
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import RunState
from sts2_env.events.shared import BattlewornDummy, Bugslayer, ColorfulPhilosophers, ColossalFlower, Darv, DenseVegetation, DoorsOfLightAndDark, DrowningBeacon, GraveOfTheForgotten, HungryForMushrooms, InfestedAutomaton, LostWisp, Nonupeipe, Orobas, Pael, PunchOff, RoundTeaParty, SpiritGrafter, SunkenStatue, SunkenTreasury, Tanx, Tezcatara, TheLanternKey, ThisOrThat, Trial, TrashHeap, UnrestSite, Vakuu, Wellspring


def test_round_tea_party_pick_fight_is_multi_page_and_grants_relic():
    run_state = RunState(seed=7, character_id="Ironclad")
    run_state.initialize_run()
    event = RoundTeaParty()

    result = event.choose(run_state, "pick_fight")
    assert not result.finished
    assert result.next_options[0].option_id == "continue_fight"

    starting_hp = run_state.player.current_hp
    starting_relics = len(run_state.player.relics)
    result = event.choose(run_state, "continue_fight")
    assert result.finished
    assert run_state.player.current_hp == starting_hp - 11
    assert len(run_state.player.relics) == starting_relics + 1


def test_trial_accept_randomizes_variant_and_merchant_guilty_adds_rewards():
    run_state = RunState(seed=11, character_id="Ironclad")
    run_state.initialize_run()
    run_state.rng.niche.next_int = lambda low, high: 0  # merchant branch
    event = Trial()
    event.generate_initial_options(run_state)

    result = event.choose(run_state, "accept")
    assert not result.finished
    option_ids = {option.option_id for option in result.next_options}
    assert option_ids == {"merchant_guilty", "merchant_innocent"}

    starting_relics = len(run_state.player.relics)
    result = event.choose(run_state, "merchant_guilty")
    assert result.finished
    assert any(card.card_id == make_regret().card_id for card in run_state.player.deck)
    assert len(run_state.player.relics) == starting_relics + 2


def test_trial_nondescript_guilty_surfaces_card_rewards_through_run_manager():
    mgr = RunManager(seed=13, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    event = Trial()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    mgr.run_state.rng.niche.next_int = lambda low, high: 2  # nondescript branch

    first = mgr._do_event_choice({"option_id": "accept"})
    assert first["phase"] == RunManager.PHASE_EVENT

    second = mgr._do_event_choice({"option_id": "nondescript_guilty"})
    assert second["phase"] == RunManager.PHASE_CARD_REWARD
    assert any(card.card_id == make_doubt().card_id for card in mgr.run_state.player.deck)

    actions = mgr.get_available_actions()
    reward_actions = [action for action in actions if action["action"] == "pick_card"]
    assert len(reward_actions) == 3


def test_trial_merchant_innocent_uses_run_level_upgrade_reward_chain():
    mgr = RunManager(seed=41, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_EVENT
    event = Trial()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    mgr.run_state.rng.niche.next_int = lambda low, high: 0

    mgr._do_event_choice({"option_id": "accept"})
    result = mgr._do_event_choice({"option_id": "merchant_innocent"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD

    actions = mgr.get_available_actions()
    choose_actions = [action for action in actions if action["action"] == "choose"]
    assert choose_actions

    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE


def test_trial_double_down_ends_run():
    mgr = RunManager(seed=17, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    event = Trial()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)

    first = mgr._do_event_choice({"option_id": "reject"})
    assert first["phase"] == RunManager.PHASE_EVENT

    second = mgr._do_event_choice({"option_id": "double_down"})
    assert second["phase"] == RunManager.PHASE_RUN_OVER


def test_battleworn_dummy_and_trash_heap_surface_real_rewards():
    run_state = RunState(seed=19, character_id="Ironclad")
    run_state.initialize_run()

    dummy = BattlewornDummy()
    result = dummy.choose(run_state, "setting_1")
    rewards = result.rewards["reward_objects"]
    assert len(rewards) == 1
    assert rewards[0].reward_type.name == "POTION"

    heap = TrashHeap()
    result = heap.choose(run_state, "dive_in")
    rewards = result.rewards["reward_objects"]
    assert len(rewards) == 1
    assert rewards[0].reward_type.name == "RELIC"


def test_battleworn_dummy_setting_two_upgrades_two_random_cards():
    run_state = RunState(seed=191, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    dummy = BattlewornDummy()

    result = dummy.choose(run_state, "setting_2")

    assert result.finished
    assert sum(1 for card in run_state.player.deck if card.upgraded) >= 2


def test_doors_of_light_and_dark_light_and_dark_apply_real_effects():
    run_state = RunState(seed=192, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    event = DoorsOfLightAndDark()
    starting_deck = len(run_state.player.deck)

    light = event.choose(run_state, "light")
    assert light.finished
    assert sum(1 for card in run_state.player.deck if card.upgraded) >= 2

    dark = event.choose(run_state, "dark")
    assert dark.finished is False
    assert event.pending_choice is not None
    resolved = event.resolve_pending_choice(0)
    assert resolved.finished
    assert len(run_state.player.deck) == starting_deck - 1


def test_unrest_site_rest_adds_poor_sleep_curse():
    run_state = RunState(seed=23, character_id="Ironclad")
    run_state.initialize_run()
    event = UnrestSite()

    result = event.choose(run_state, "rest")
    assert result.finished
    assert any(card.card_id == make_poor_sleep().card_id for card in run_state.player.deck)


def test_bugslayer_adds_real_reward_cards_to_deck():
    run_state = RunState(seed=29, character_id="Ironclad")
    run_state.initialize_run()
    event = Bugslayer()

    exterminate = event.choose(run_state, "exterminate")
    assert exterminate.finished
    assert any(card.card_id == make_exterminate().card_id for card in run_state.player.deck)

    squash = event.choose(run_state, "squash")
    assert squash.finished
    assert any(card.card_id == make_squash().card_id for card in run_state.player.deck)


def test_dense_vegetation_spirit_grafter_and_wellspring_apply_real_state_changes():
    run_state = RunState(seed=61, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    starting_deck = len(run_state.player.deck)

    vegetation = DenseVegetation()
    result = vegetation.choose(run_state, "trudge_on")
    assert not result.finished
    result = vegetation.resolve_pending_choice(0)
    assert result.finished
    assert len(run_state.player.deck) == starting_deck - 1

    grafter = SpiritGrafter()
    healed_before = run_state.player.current_hp
    result = grafter.choose(run_state, "let_it_in")
    assert result.finished
    assert any(card.card_id == make_metamorphosis().card_id for card in run_state.player.deck)
    assert run_state.player.current_hp >= healed_before

    result = grafter.choose(run_state, "rejection")
    assert not result.finished
    hp_before = run_state.player.current_hp
    deck_before = len(run_state.player.deck)
    result = grafter.resolve_pending_choice(0)
    assert result.finished
    assert len(run_state.player.deck) == deck_before - 1
    assert run_state.player.current_hp == hp_before - 9

    spring = Wellspring()
    bottle = spring.choose(run_state, "bottle")
    rewards = bottle.rewards["reward_objects"]
    assert rewards and rewards[0].reward_type.name == "POTION"

    deck_before = len(run_state.player.deck)
    bathe = spring.choose(run_state, "bathe")
    assert not bathe.finished
    bathe = spring.resolve_pending_choice(0)
    assert bathe.finished
    assert len(run_state.player.deck) == deck_before
    assert any(card.card_id == make_guilty().card_id for card in run_state.player.deck)


def test_shared_events_apply_real_relic_potion_card_and_curse_effects():
    run_state = RunState(seed=62, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    start_relics = len(run_state.player.relics)

    beacon = DrowningBeacon()
    bottle = beacon.choose(run_state, "bottle")
    rewards = bottle.rewards["reward_objects"]
    assert rewards and rewards[0].reward_type.name == "POTION"
    climb = beacon.choose(run_state, "climb")
    assert climb.finished
    assert len(run_state.player.relics) == start_relics + 1

    forgotten = GraveOfTheForgotten()
    run_state.player.deck.append(make_backstab())
    result = forgotten.choose(run_state, "confront")
    assert not result.finished
    result = forgotten.resolve_pending_choice(0)
    assert result.finished
    assert any(card.card_id == make_decay().card_id for card in run_state.player.deck)
    accept = forgotten.choose(run_state, "accept")
    assert accept.finished

    mushrooms = HungryForMushrooms()
    before_relics = len(run_state.player.relics)
    mushrooms.choose(run_state, "big")
    mushrooms.choose(run_state, "fragrant")
    assert len(run_state.player.relics) >= before_relics + 2

    automaton = InfestedAutomaton()
    deck_before = len(run_state.player.deck)
    automaton.choose(run_state, "study")
    automaton.choose(run_state, "touch_core")
    assert len(run_state.player.deck) >= deck_before + 2

    wisp = LostWisp()
    before_relics = len(run_state.player.relics)
    wisp.choose(run_state, "claim")
    assert len(run_state.player.relics) == before_relics + 1
    assert any(card.card_id == make_decay().card_id for card in run_state.player.deck)

    statue = SunkenStatue()
    before_relics = len(run_state.player.relics)
    statue.choose(run_state, "grab_sword")
    assert len(run_state.player.relics) == before_relics + 1


def test_ancient_option_pool_events_obtain_real_relics():
    run_state = RunState(seed=63, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()

    nonu = Nonupeipe()
    options = nonu.generate_initial_options(run_state)
    before_relics = len(run_state.player.relics)
    result = nonu.choose(run_state, options[0].option_id)
    assert result.finished
    assert len(run_state.player.relics) == before_relics + 1

    tanx = Tanx()
    options = tanx.generate_initial_options(run_state)
    before_relics = len(run_state.player.relics)
    result = tanx.choose(run_state, options[0].option_id)
    assert result.finished
    assert len(run_state.player.relics) == before_relics + 1

    for event_cls in (Orobas, Pael, Tezcatara, Vakuu):
        event = event_cls()
        before_relics = len(run_state.player.relics)
        options = event.generate_initial_options(run_state)
        result = event.choose(run_state, options[0].option_id)
        assert result.finished
        assert len(run_state.player.relics) == before_relics + 1


def test_darv_uses_act_conditioned_boss_relic_pool():
    run_state = RunState(seed=64, character_id="Ironclad")
    run_state.initialize_run()
    darv = Darv()

    run_state.current_act_index = 1
    run_state.rng.up_front.choice = lambda seq: seq[-1]
    run_state.rng.up_front.shuffle = lambda seq: seq.reverse()
    run_state.rng.up_front.next_int = lambda low, high: 0
    options = darv.generate_initial_options(run_state)
    labels = {option.label for option in options}
    assert any("SOZU" in label or "Sozu" in label for label in labels)

    run_state.current_act_index = 2
    options = darv.generate_initial_options(run_state)
    labels = {option.label for option in options}
    assert any("VELVET" in label or "Velvet" in label for label in labels)


def test_shared_reward_and_curse_events_apply_real_state_changes():
    run_state = RunState(seed=65, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()

    philosophers = ColorfulPhilosophers()
    result = philosophers.choose(run_state, philosophers.generate_initial_options(run_state)[0].option_id)
    rewards = result.rewards["reward_objects"]
    assert len(rewards) == 3
    assert all(reward.reward_type.name == "CARD" for reward in rewards)

    flower = ColossalFlower()
    result = flower.choose(run_state, "reach_deeper")
    assert not result.finished
    result = flower.choose(run_state, "reach_deeper")
    assert not result.finished
    before_relics = len(run_state.player.relics)
    result = flower.choose(run_state, "pollinous_core")
    assert result.finished
    assert len(run_state.player.relics) == before_relics + 1

    treasury = SunkenTreasury()
    result = treasury.choose(run_state, "second_chest")
    assert result.finished
    assert any(card.card_id == make_greed().card_id for card in run_state.player.deck)

    this_or_that = ThisOrThat()
    before_relics = len(run_state.player.relics)
    result = this_or_that.choose(run_state, "ornate")
    assert result.finished
    assert len(run_state.player.relics) == before_relics + 1
    assert any(card.card_id == make_clumsy().card_id for card in run_state.player.deck)

    punch = PunchOff()
    nab = punch.choose(run_state, "nab")
    rewards = nab.rewards["reward_objects"]
    assert any(card.card_id == make_injury().card_id for card in run_state.player.deck)
    assert len(rewards) == 1 and rewards[0].reward_type.name == "RELIC"

    take_them = punch.choose(run_state, "take_them")
    assert not take_them.finished
    fight = punch.choose(run_state, "fight")
    rewards = fight.rewards["reward_objects"]
    assert [reward.reward_type.name for reward in rewards] == ["RELIC", "POTION"]

    lantern = TheLanternKey()
    keep = lantern.choose(run_state, "keep_key")
    assert not keep.finished
    fight = lantern.choose(run_state, "fight")
    rewards = fight.rewards["reward_objects"]
    assert len(rewards) == 1 and rewards[0].reward_type.name == "ADD_CARD"
    assert fight.event_combat_setup == "mysterious_knight"


def test_shared_event_combat_branches_expose_event_combat_setups():
    run_state = RunState(seed=67, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()

    vegetation = DenseVegetation()
    rest = vegetation.choose(run_state, "rest")
    assert not rest.finished
    fight = vegetation.choose(run_state, "fight")
    assert fight.finished
    assert fight.event_combat_setup == "dense_vegetation"

    punch = PunchOff()
    take_them = punch.choose(run_state, "take_them")
    assert not take_them.finished
    fight = punch.choose(run_state, "fight")
    assert fight.finished
    assert fight.event_combat_setup == "punch_off"


def test_run_manager_enters_combat_for_shared_event_combat_branches():
    mgr = RunManager(seed=68, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_EVENT
    event = DenseVegetation()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)

    first = mgr._do_event_choice({"option_id": "rest"})
    assert first["phase"] == RunManager.PHASE_EVENT

    second = mgr._do_event_choice({"option_id": "fight"})
    assert second["phase"] == RunManager.PHASE_COMBAT
