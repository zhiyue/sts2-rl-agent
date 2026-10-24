"""Factory upgrade parity checks for Colorless cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import (
    make_believe_in_you,
    make_bolas,
    make_calamity_card,
    make_catastrophe,
    make_coordinate_card,
    make_dark_shackles,
    make_dramatic_entrance,
    make_entropy,
    make_equilibrium,
    make_eternal_armor,
    make_fasten,
    make_finesse,
    make_flash_of_steel,
    make_intercept_card,
    make_jack_of_all_trades,
    make_jackpot,
    make_lift,
    make_mayhem_card,
    make_nostalgia_card,
    make_omnislice,
    make_panic_button,
    make_production,
    make_prolong,
    make_prowess,
    make_rally,
    make_rend,
    make_restlessness,
    make_salvo,
    make_seeker_strike,
    make_shockwave,
    make_splash,
    make_stratagem,
    make_tag_team,
    make_the_bomb_card,
    make_the_gambit,
    make_thrumming_hatchet,
    make_ultimate_defend,
    make_ultimate_strike,
)
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


def _make_combat(*, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=5150,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(5150))
    combat.add_enemy(creature, ai)
    for index in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(5151 + index))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


def _add_ally(combat: CombatState):
    return combat.add_ally_player(
        PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
    )


class TestColorlessFactoryUpgradeParity:
    def test_believe_in_you_factory_upgrade_grants_three_energy_to_target_ally(self):
        combat = _make_combat()
        ally = _add_ally(combat)
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None
        ally_state.energy = 0
        combat.hand = [make_believe_in_you(upgraded=True)]
        combat.energy = 0

        assert combat.play_card(0, 0)

        # BelieveInYou.cs:18 defines EnergyVar(2) and OnUpgrade (lines 31-34)
        # adds 1 -> 3; OnPlay grants the energy to cardPlay.Target.Player.
        assert ally_state.energy == 3

    def test_bolas_factory_upgrade_deals_four_and_returns_next_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        bolas = make_bolas(upgraded=True)
        combat.hand = [bolas]
        combat.draw_pile = []

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 96
        assert bolas in combat.discard_pile

        combat.end_player_turn()

        assert bolas in combat.hand

    def test_calamity_card_factory_upgrade_costs_two_and_applies_power(self):
        combat = _make_combat()
        card = make_calamity_card(upgraded=True)
        combat.hand = [card]
        combat.energy = 2

        assert card.cost == 2
        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.CALAMITY) == 1

    def test_catastrophe_factory_upgrade_autoplays_three_draw_cards(self):
        combat = _make_combat()
        first = make_strike_ironclad()
        second = make_defend_ironclad()
        third = make_strike_ironclad()
        combat.hand = [make_catastrophe(upgraded=True)]
        combat.draw_pile = [first, second, third]
        combat.energy = 2

        assert combat.play_card(0)

        assert combat.draw_pile == []
        assert first in combat.discard_pile
        assert second in combat.discard_pile
        assert third in combat.discard_pile

    def test_coordinate_card_factory_upgrade_applies_eight_coordinate_to_target_ally(self):
        combat = _make_combat()
        ally = _add_ally(combat)
        combat.hand = [make_coordinate_card(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert ally.get_power_amount(PowerId.COORDINATE) == 8
        assert ally.get_power_amount(PowerId.STRENGTH) == 8

    def test_dark_shackles_factory_upgrade_applies_fifteen_temporary_strength_loss(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_dark_shackles(upgraded=True)]

        assert combat.play_card(0, 0)

        assert enemy.get_power_amount(PowerId.DARK_SHACKLES) == 15
        assert enemy.get_power_amount(PowerId.STRENGTH) == -15

    def test_dramatic_entrance_factory_upgrade_hits_all_enemies_for_fifteen_and_exhausts(self):
        combat = _make_combat(extra_enemies=1)
        for enemy in combat.enemies:
            enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_dramatic_entrance(upgraded=True)]

        assert combat.play_card(0)

        assert [enemy.current_hp for enemy in combat.enemies] == [85, 85]
        assert any(card.card_id == CardId.DRAMATIC_ENTRANCE for card in combat.exhaust_pile)

    def test_entropy_factory_upgrade_adds_innate_and_keeps_one_transform_power(self):
        combat = _make_combat()
        card = make_entropy(upgraded=True)
        combat.hand = [card]
        combat.energy = 1

        assert card.is_innate
        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.ENTROPY) == 1

    def test_equilibrium_factory_upgrade_gains_sixteen_block_and_retains_hand(self):
        combat = _make_combat()
        retained = make_strike_ironclad()
        combat.hand = [make_equilibrium(upgraded=True), retained]
        combat.energy = 2

        assert combat.play_card(0)

        assert combat.player.block == 16
        assert combat.player.get_power_amount(PowerId.RETAIN_HAND) == 1

    def test_eternal_armor_factory_upgrade_applies_twelve_plating(self):
        combat = _make_combat()
        combat.hand = [make_eternal_armor(upgraded=True)]
        combat.energy = 3

        assert combat.play_card(0)

        # EternalArmor.cs:20 defines PowerVar<PlatingPower>(9m) and OnUpgrade
        # (lines 32-35) adds 3 -> 12.
        assert combat.player.get_power_amount(PowerId.PLATING) == 12

    def test_fasten_factory_upgrade_applies_six_extra_defend_block(self):
        combat = _make_combat()
        combat.hand = [make_fasten(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)

        # Fasten.cs:38 defines DynamicVar("ExtraBlock", 4) and OnUpgrade
        # (lines 50-53) adds 2 -> 6.
        assert combat.player.get_power_amount(PowerId.FASTEN) == 6

    def test_finesse_factory_upgrade_gains_seven_block_and_draws_one(self):
        combat = _make_combat()
        drawn = make_strike_ironclad()
        combat.hand = [make_finesse(upgraded=True)]
        combat.draw_pile = [drawn]

        assert combat.play_card(0)

        assert combat.player.block == 7
        assert combat.hand == [drawn]

    def test_flash_of_steel_factory_upgrade_deals_eight_and_draws_one(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        drawn = make_strike_ironclad()
        combat.hand = [make_flash_of_steel(upgraded=True)]
        combat.draw_pile = [drawn]

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 92
        assert combat.hand == [drawn]

    def test_intercept_card_factory_upgrade_grants_thirteen_block_and_covers_ally(self):
        combat = _make_combat()
        ally = _add_ally(combat)
        combat.hand = [make_intercept_card(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert combat.player.block == 13
        assert ally.get_power_amount(PowerId.COVERED) == 1

    def test_jackpot_factory_upgrade_deals_thirty_and_creates_three_upgraded_cards(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_jackpot(upgraded=True)]
        combat.energy = 3

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 70
        assert len(combat.hand) == 3
        assert all(card.upgraded and card.cost == 0 for card in combat.hand)

    def test_jack_of_all_trades_factory_upgrade_creates_two_colorless_cards(self):
        combat = _make_combat()
        combat.hand = [make_jack_of_all_trades(upgraded=True)]

        assert combat.play_card(0)

        assert len(combat.hand) == 2
        assert all(card.card_id != CardId.JACK_OF_ALL_TRADES for card in combat.hand)

    def test_lift_factory_upgrade_gives_target_ally_sixteen_block(self):
        combat = _make_combat()
        ally = _add_ally(combat)
        combat.hand = [make_lift(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert ally.block == 16

    def test_mayhem_card_factory_upgrade_costs_one_and_applies_power(self):
        combat = _make_combat()
        card = make_mayhem_card(upgraded=True)
        combat.hand = [card]
        combat.energy = 1

        assert card.cost == 1
        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.MAYHEM) == 1

    def test_nostalgia_card_factory_upgrade_costs_zero_and_applies_power(self):
        combat = _make_combat()
        card = make_nostalgia_card(upgraded=True)
        combat.hand = [card]
        combat.energy = 0

        assert card.cost == 0
        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.NOSTALGIA) == 1

    def test_omnislice_factory_upgrade_deals_eleven_and_splashes(self):
        combat = _make_combat(extra_enemies=1)
        primary, secondary = combat.enemies
        primary.current_hp = primary.max_hp = 100
        secondary.current_hp = secondary.max_hp = 100
        combat.hand = [make_omnislice(upgraded=True)]

        assert combat.play_card(0, 0)

        assert primary.current_hp == 89
        assert secondary.current_hp == 89

    def test_panic_button_factory_upgrade_gains_forty_block_and_no_block_two(self):
        combat = _make_combat()
        combat.hand = [make_panic_button(upgraded=True)]

        assert combat.play_card(0)

        assert combat.player.block == 40
        assert combat.player.get_power_amount(PowerId.NO_BLOCK) == 2

    def test_production_factory_upgrade_exhausts_and_gains_three_energy(self):
        combat = _make_combat()
        card = make_production(upgraded=True)
        combat.hand = [card]
        combat.energy = 0

        # Production.cs keeps Exhaust on upgrade: CanonicalKeywords is
        # [CardKeyword.Exhaust] and OnUpgrade (lines 29-32) only bumps Energy
        # by 1 (EnergyVar(2) -> 3).
        assert card.exhausts
        assert combat.play_card(0)

        assert combat.energy == 3
        assert card in combat.exhaust_pile
        assert card not in combat.discard_pile

    def test_prolong_factory_upgrade_does_not_exhaust_and_keeps_block_next_turn(self):
        combat = _make_combat()
        card = make_prolong(upgraded=True)
        combat.player.gain_block(12)
        combat.hand = [card]

        assert not card.exhausts
        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.BLOCK_NEXT_TURN) == 12
        assert card in combat.discard_pile

    def test_prowess_factory_upgrade_grants_two_strength_and_dexterity(self):
        combat = _make_combat()
        combat.hand = [make_prowess(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.STRENGTH) == 2
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 2

    def test_rally_factory_upgrade_grants_seventeen_block_to_owner_and_teammates(self):
        """Rally.cs OnUpgrade: Block 12 -> 17 for every living player teammate,
        including the caster (CombatState.GetTeammatesOf includes the creature
        itself, CombatState.cs:397)."""
        combat = _make_combat()
        ally = _add_ally(combat)
        combat.hand = [make_rally(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)

        assert combat.player.block == 17
        assert ally.block == 17

    def test_rend_factory_upgrade_uses_eighteen_plus_eight_per_debuff(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.apply_power_to(enemy, PowerId.WEAK, 1)
        combat.hand = [make_rend(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 74

    def test_restlessness_factory_upgrade_draws_three_and_gains_three_when_only_card(self):
        combat = _make_combat()
        drawn = [make_strike_ironclad(), make_defend_ironclad(), make_strike_ironclad()]
        combat.hand = [make_restlessness(upgraded=True)]
        combat.draw_pile = list(drawn)
        combat.energy = 0

        assert combat.play_card(0)

        assert combat.hand == drawn
        assert combat.energy == 3

    def test_salvo_factory_upgrade_deals_sixteen_and_retains_hand(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_salvo(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 84
        assert combat.player.get_power_amount(PowerId.RETAIN_HAND) == 1

    def test_seeker_strike_factory_upgrade_deals_twelve_and_reveals_three_draw_cards(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_seeker_strike(upgraded=True)]
        combat.draw_pile = [
            make_strike_ironclad(),
            make_defend_ironclad(),
            make_strike_ironclad(),
            make_defend_ironclad(),
        ]
        combat.energy = 1

        assert combat.play_card(0, 0)

        # SeekerStrike.cs:21-24 defines DamageVar(9) + CardsVar(3) and
        # OnUpgrade (lines 44-47) adds 3 -> 12 damage; the reveal count of 3
        # is unchanged.
        assert enemy.current_hp == 88
        assert combat.pending_choice is not None
        assert len(combat.pending_choice.options) == 3

    def test_shockwave_factory_upgrade_applies_five_weak_and_vulnerable(self):
        combat = _make_combat(extra_enemies=1)
        combat.hand = [make_shockwave(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)

        for enemy in combat.enemies:
            assert enemy.get_power_amount(PowerId.WEAK) == 5
            assert enemy.get_power_amount(PowerId.VULNERABLE) == 5

    def test_splash_factory_upgrade_marks_generated_choice_upgraded_and_free(self):
        combat = _make_combat()
        combat.hand = [make_splash(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.pending_choice is not None
        selected = combat.pending_choice.options[0].card
        assert selected.upgraded

        assert combat.resolve_pending_choice(0)

        assert selected in combat.hand
        assert selected.cost == 0
        assert selected.combat_vars["_turn_cost_override"] == 0

    def test_stratagem_factory_upgrade_costs_zero_and_applies_power(self):
        combat = _make_combat()
        card = make_stratagem(upgraded=True)
        combat.hand = [card]
        combat.energy = 0

        assert card.cost == 0
        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.STRATAGEM) == 1

    def test_tag_team_factory_upgrade_deals_fifteen_and_applies_tag_team(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_tag_team(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 85
        assert enemy.get_power_amount(PowerId.TAG_TEAM) == 1

    def test_the_bomb_card_factory_upgrade_sets_fifty_damage_countdown(self):
        combat = _make_combat()
        combat.hand = [make_the_bomb_card(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)

        bomb = combat.player.powers[PowerId.THE_BOMB]
        assert bomb.amount == 3
        assert bomb.damage == 50

    def test_the_gambit_factory_upgrade_gains_seventy_five_block(self):
        combat = _make_combat()
        combat.hand = [make_the_gambit(upgraded=True)]

        assert combat.play_card(0)

        assert combat.player.block == 75
        assert combat.player.get_power_amount(PowerId.THE_GAMBIT) == 1

    def test_thrumming_hatchet_factory_upgrade_deals_fourteen_and_returns_next_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        hatchet = make_thrumming_hatchet(upgraded=True)
        combat.hand = [hatchet]
        combat.draw_pile = []
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 86
        assert hatchet in combat.discard_pile

        combat.end_player_turn()

        assert hatchet in combat.hand

    def test_ultimate_defend_factory_upgrade_gains_fifteen_block(self):
        combat = _make_combat()
        combat.hand = [make_ultimate_defend(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.block == 15

    def test_ultimate_strike_factory_upgrade_deals_twenty(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_ultimate_strike(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 80
