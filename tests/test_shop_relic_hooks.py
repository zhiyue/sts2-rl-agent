"""Regression tests for merchant relic hook plumbing."""

from sts2_env.run.run_manager import RunManager
from sts2_env.run.shop import generate_shop_inventory
from sts2_env.run.run_state import RunState


def test_membership_card_halves_shop_prices():
    base = RunState(seed=301, character_id="Ironclad")
    discounted = RunState(seed=301, character_id="Ironclad")
    assert discounted.player.obtain_relic("MEMBERSHIP_CARD")

    base_inv = generate_shop_inventory(base)
    discounted_inv = generate_shop_inventory(discounted)

    assert discounted_inv.cards[0].price == round(base_inv.cards[0].price * 0.5)
    assert discounted_inv.removal_cost == round(base_inv.removal_cost * 0.5)


def test_the_courier_refills_bought_shop_card_slot():
    mgr = RunManager(seed=302, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("THE_COURIER")
    mgr.run_state.player.gold = 9999
    mgr._enter_shop()

    assert mgr._shop_inventory is not None
    original_entry = mgr._shop_inventory.cards[0]
    original_card_id = original_entry.card_id

    result = mgr._do_shop_action({"action": "buy_card", "index": 0})

    assert "Bought card" in result["description"]
    refilled_entry = mgr._shop_inventory.cards[0]
    assert refilled_entry.price < 999999
    assert refilled_entry.card_id
    assert len(mgr.run_state.player.deck) > 0
    assert refilled_entry is not original_entry or refilled_entry.card_id != original_card_id


def test_molten_egg_and_fresnel_lens_modify_merchant_cards():
    run_state = RunState(seed=303, character_id="Ironclad")
    assert run_state.player.obtain_relic("MOLTEN_EGG")
    assert run_state.player.obtain_relic("FRESNEL_LENS")

    inv = generate_shop_inventory(run_state)

    attack_entries = [entry for entry in inv.cards if entry.card_type == "Attack" and entry.card is not None]
    assert attack_entries
    assert all(entry.card.upgraded for entry in attack_entries)
    assert all(entry.card.enchantments.get("Nimble") == 2 for entry in attack_entries)


def test_buying_modified_merchant_card_preserves_modifications_in_deck():
    mgr = RunManager(seed=304, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("MOLTEN_EGG")
    assert mgr.run_state.player.obtain_relic("FRESNEL_LENS")
    mgr.run_state.player.gold = 9999
    mgr._enter_shop()

    assert mgr._shop_inventory is not None
    attack_index = next(i for i, entry in enumerate(mgr._shop_inventory.cards) if entry.card_type == "Attack")
    mgr._do_shop_action({"action": "buy_card", "index": attack_index})

    added = mgr.run_state.player.deck[-1]
    assert added.upgraded
    assert added.enchantments.get("Nimble") == 2
