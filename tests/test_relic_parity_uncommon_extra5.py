"""Fifth batch of focused uncommon relic parity tests for uncovered hook behavior."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.regent import create_regent_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CombatSide, PowerId, RoomType
from sts2_env.core.hooks import fire_before_side_turn_start
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
    seed: int = 1001,
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


def _make_regent_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1051,
) -> CombatState:
    combat = CombatState(
        player_hp=78,
        player_max_hp=78,
        deck=create_regent_starter_deck(),
        rng_seed=seed,
        character_id="Regent",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _damage_from_single_card(card: object, relics: list[str] | None = None, *, seed: int = 1091) -> int:
    combat = _make_ironclad_combat(relics, seed=seed)
    enemy = combat.enemies[0]
    enemy.max_hp = 250
    enemy.current_hp = 250
    combat.hand = _with_owner([card], combat.player)
    combat.energy = getattr(card, "cost", 1)
    hp_before = enemy.current_hp
    assert combat.play_card(0, 0)
    return hp_before - enemy.current_hp


class TestRelicParityUncommonExtra5:
    def test_bellows_upgrades_opening_hand_only_on_round_one(self):
        """Matches Bellows.cs: round-1 player turn start upgrades all cards currently in hand."""
        combat = _make_ironclad_combat(["Bellows"], seed=1001)
        assert combat.round_number == 1
        assert combat.hand
        assert all(card.upgraded for card in combat.hand)

        combat.end_player_turn()
        assert combat.round_number == 2
        assert any(not card.upgraded for card in combat.hand)

    def test_bowler_hat_grants_floor_twenty_percent_bonus_without_recursive_chaining(self):
        """Matches BowlerHat.cs: gain floor(20%) bonus gold after each non-bonus gain."""
        run_state = RunState(seed=1002, character_id="Ironclad")
        run_state.player.gold = 0
        assert run_state.player.obtain_relic("BOWLER_HAT")

        run_state.player.gain_gold(11)
        assert run_state.player.gold == 13

        run_state.player.gain_gold(1)
        assert run_state.player.gold == 14

    def test_eternal_feather_heals_at_rest_site_by_deck_size_quanta(self):
        """Matches EternalFeather.cs: rest-site heal is 3 * floor(deck_size / 5)."""
        run_state = RunState(seed=1003, character_id="Ironclad")
        run_state.player.deck = create_ironclad_starter_deck()
        assert run_state.player.obtain_relic("ETERNAL_FEATHER")
        relic = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "ETERNAL_FEATHER")

        run_state.player.current_hp = 40
        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.MONSTER))
        assert run_state.player.current_hp == 40

        relic.after_room_entered(run_state.player, RoomVisitContext(RoomType.REST_SITE))
        assert run_state.player.current_hp == 46

    def test_funerary_mask_adds_three_souls_to_draw_on_round_one(self):
        """Matches FuneraryMask.cs: on round 1 start, add 3 Soul cards to draw pile at random positions."""
        combat = _make_ironclad_combat(["FuneraryMask"], seed=1004)
        souls = [card for card in combat.draw_pile if card.card_id == CardId.SOUL]

        assert len(souls) == 3
        assert all(card.owner is combat.player for card in souls)

    def test_galactic_dust_gains_block_every_ten_stars_spent_with_rollover(self):
        """Matches GalacticDust.cs: thresholded spend counter grants 10 unpowered block per 10 stars."""
        combat = _make_regent_combat(["GalacticDust"], seed=1005)
        combat.player.block = 0
        combat.gain_stars(combat.player, 23)

        assert combat.spend_stars(combat.player, 9) == 9
        assert combat.player.block == 0

        assert combat.spend_stars(combat.player, 1) == 1
        assert combat.player.block == 10

        assert combat.spend_stars(combat.player, 13) == 13
        assert combat.player.block == 20

    def test_miniature_cannon_boosts_only_upgraded_attack_damage(self):
        """Matches MiniatureCannon.cs: powered upgraded attacks from owner gain +3 additive damage."""
        base_normal = _damage_from_single_card(create_card(CardId.STRIKE_IRONCLAD), [], seed=1006)
        with_cannon_normal = _damage_from_single_card(create_card(CardId.STRIKE_IRONCLAD), ["MiniatureCannon"], seed=1006)
        base_upgraded = _damage_from_single_card(create_card(CardId.STRIKE_IRONCLAD, upgraded=True), [], seed=1007)
        with_cannon_upgraded = _damage_from_single_card(
            create_card(CardId.STRIKE_IRONCLAD, upgraded=True),
            ["MiniatureCannon"],
            seed=1007,
        )

        assert base_normal == 6
        assert with_cannon_normal == base_normal
        assert base_upgraded == 9
        assert with_cannon_upgraded == base_upgraded + 3

    def test_red_mask_applies_round_one_weak_to_all_enemies_only(self):
        """Matches RedMask.cs: before side turn start on round 1, apply Weak(1) to all enemies."""
        combat = _make_ironclad_combat(["RedMask"], seed=1008, enemies=2)
        assert all(enemy.get_power_amount(PowerId.WEAK) == 1 for enemy in combat.enemies)

        for enemy in combat.enemies:
            enemy.powers.pop(PowerId.WEAK, None)
        combat.round_number = 2
        fire_before_side_turn_start(CombatSide.PLAYER, combat)
        assert all(enemy.get_power_amount(PowerId.WEAK) == 0 for enemy in combat.enemies)
