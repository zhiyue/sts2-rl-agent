"""Parity tests for content added in game v0.107.1 (build 23811903).

Ground truth: decompiled MegaCrit.Sts2.Core.Models.Cards/{NotYet,Scare,Wither}.cs
and the Ironclad/Silent/Status card pools. Each assertion cites the C# construct
it mirrors so failures point at the divergent line.
"""

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import make_not_yet
from sts2_env.cards.silent import make_scare
from sts2_env.cards.status import make_wither, wither_fake_upgrade, wither_turn_end_in_hand
from sts2_env.core.card_pools import IRONCLAD_CARD_POOL, SILENT_CARD_POOL, STATUS_CARD_POOL
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CardRarity, CardType, PowerId, TargetType
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat(extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=[],
        rng_seed=42,
        relics=[],
        gold=0,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        creature, ai = create_shrinker_beetle(Rng(100 + i))
        combat.add_enemy(creature, ai)
    return combat


class TestNotYetCardParity:
    """C# NotYet: cost 2, Skill, Rare, Self, Exhaust, HealVar(10), OnUpgrade Heal +3."""

    def test_static_metadata_matches_decompiled_csharp(self):
        for upgraded, expected_heal in ((False, 10), (True, 13)):
            card = create_card(CardId.NOT_YET, upgraded=upgraded)
            assert card.cost == 2
            assert card.card_type is CardType.SKILL
            assert card.rarity is CardRarity.RARE
            assert card.target_type is TargetType.SELF
            assert card.keywords == frozenset({"exhaust"})
            assert card.effect_vars["heal"] == expected_heal
            assert card.upgraded is upgraded

    def test_play_heals_owner_by_canonical_amount(self):
        combat = _make_combat()
        combat.player.current_hp = 50
        combat.energy = 2
        combat.hand = [make_not_yet()]

        combat.play_card(0)

        assert combat.player.current_hp == 60

    def test_play_exhausts_card(self):
        combat = _make_combat()
        combat.energy = 2
        not_yet = make_not_yet()
        combat.hand = [not_yet]

        combat.play_card(0)

        assert not_yet in combat.exhaust_pile


class TestScareCardParity:
    """C# Scare: cost 0, Skill, Uncommon, AllEnemies, Exhaust, 1 Weak to all;
    OnUpgrade removes Exhaust only."""

    def test_static_metadata_matches_decompiled_csharp(self):
        base = make_scare()
        assert base.cost == 0
        assert base.card_type is CardType.SKILL
        assert base.rarity is CardRarity.UNCOMMON
        assert base.target_type is TargetType.ALL_ENEMIES
        assert base.keywords == frozenset({"exhaust"})

        upgraded = make_scare(upgraded=True)
        assert upgraded.keywords == frozenset()
        assert upgraded.cost == 0

    def test_play_applies_one_weak_to_all_enemies(self):
        combat = _make_combat(extra_enemies=1)
        combat.hand = [make_scare()]

        combat.play_card(0)

        for enemy in combat.hittable_enemies:
            assert enemy.has_power(PowerId.WEAK)
            assert enemy.get_power_amount(PowerId.WEAK) == 1

    def test_upgraded_still_applies_weak(self):
        combat = _make_combat(extra_enemies=1)
        combat.hand = [make_scare(upgraded=True)]

        combat.play_card(0)

        for enemy in combat.hittable_enemies:
            assert enemy.get_power_amount(PowerId.WEAK) == 1


class TestWitherCardParity:
    """C# Wither: cost -1, Status, Unplayable, MaxUpgradeLevel 0,
    DamageVar(3, Unpowered|Move) at end of turn in hand; FakeUpgrade: +3 damage."""

    def test_static_metadata_matches_decompiled_csharp(self):
        card = make_wither()
        assert card.cost == -1
        assert card.card_type is CardType.STATUS
        assert card.rarity is CardRarity.STATUS
        assert card.target_type is TargetType.NONE
        assert card.keywords == frozenset({"unplayable"})
        assert card.effect_vars["damage"] == 3

    def test_cannot_be_upgraded(self):
        card = create_card(CardId.WITHER, upgraded=True)
        assert card.upgraded is False
        assert card.effect_vars["damage"] == 3

    def test_turn_end_in_hand_deals_three_unpowered_damage(self):
        combat = _make_combat()
        combat.hand = [make_wither()]

        combat.end_player_turn()

        assert combat.player.current_hp == 77

    def test_fake_upgrade_increases_damage_by_three(self):
        combat = _make_combat()
        wither = make_wither()
        combat.hand = [wither]
        combat.end_player_turn()
        assert combat.player.current_hp == 77

        wither_fake_upgrade(wither)
        assert wither.effect_vars["damage"] == 6

        # The end-of-turn hook wiring is proven above; invoke the hook directly
        # so the beetle's own round-3 attack does not pollute the HP assertion.
        # Fake-upgraded damage is 3+3=6 (C# DynamicVars.Damage.UpgradeValueBy(3m)).
        wither_turn_end_in_hand(wither, combat, 1)
        assert combat.player.current_hp == 71


class TestNewCardPoolMembership:
    """C# pools: NotYet in IroncladCardPool, Scare in SilentCardPool,
    Wither in StatusCardPool."""

    def test_not_yet_in_ironclad_pool(self):
        assert CardId.NOT_YET in IRONCLAD_CARD_POOL

    def test_scare_in_silent_pool(self):
        assert CardId.SCARE in SILENT_CARD_POOL

    def test_wither_in_status_pool(self):
        assert CardId.WITHER in STATUS_CARD_POOL
