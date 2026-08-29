"""Parity tests for rare/shop relic owner scope, costs, and generation hooks."""

from types import SimpleNamespace

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import (
    make_boost_away,
    create_defect_starter_deck,
    make_beam_cell,
    make_charge_battery,
)
from sts2_env.cards.ironclad import create_ironclad_starter_deck, make_barricade, make_bash, make_inflame, make_whirlwind
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.status import make_shiv
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CardRarity, CardTag, CombatSide, PowerId
from sts2_env.core.hooks import (
    fire_after_card_exhausted,
    fire_after_card_discarded,
    fire_after_card_played,
    fire_after_side_turn_start,
    fire_after_turn_end,
    fire_before_card_played,
    fire_before_side_turn_start,
    fire_before_turn_end,
)
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState, RunState


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1900,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _make_defect_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 1910,
) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=seed,
        character_id="Defect",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _combat_relic(combat: CombatState, relic_name: str):
    return next(relic for relic in combat.relics if relic.relic_id.name == relic_name)


class TestRelicRareShopOwnerScopeCostAndGenerationParity:
    def test_unsettling_lamp_doubles_first_card_debuff_once(self):
        """Matches UnsettlingLamp.cs: first debuff card doubles debuff amounts once."""
        combat = _make_ironclad_combat(["UnsettlingLamp"], seed=1900)
        enemy = combat.enemies[0]
        combat.hand = [make_bash()]
        combat.energy = 10

        assert combat.play_card(0, 0)

        assert enemy.get_power_amount(PowerId.VULNERABLE) == 4

        enemy.powers.pop(PowerId.VULNERABLE, None)
        combat.hand = [make_bash()]
        combat.energy = 10

        assert combat.play_card(0, 0)

        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

    def test_unsettling_lamp_treats_negative_strength_as_debuff(self):
        combat = _make_ironclad_combat(["UnsettlingLamp"], seed=1900)
        enemy = combat.enemies[0]
        source = make_bash()
        source.owner = combat.player

        combat.apply_power_to(enemy, PowerId.STRENGTH, -1, applier=combat.player, source=source)

        assert enemy.get_power_amount(PowerId.STRENGTH) == -2

    def test_unsettling_lamp_does_not_double_temporary_power_internal_strength_twice(self):
        combat = _make_ironclad_combat(["UnsettlingLamp"], seed=1900)
        enemy = combat.enemies[0]
        source = make_bash()
        source.owner = combat.player

        combat.apply_power_to(enemy, PowerId.SHACKLING_POTION, 2, applier=combat.player, source=source)

        assert enemy.get_power_amount(PowerId.SHACKLING_POTION) == 4
        assert enemy.get_power_amount(PowerId.STRENGTH) == -4

    def test_unsettling_lamp_ignores_debuff_absorbed_by_artifact(self):
        """Matches v0.111.0 UnsettlingLamp.cs: debuffs absorbed by Artifact don't trigger the relic."""
        combat = _make_ironclad_combat(["UnsettlingLamp"], seed=1900)
        enemy = combat.enemies[0]
        source = make_bash()
        source.owner = combat.player
        combat.apply_power_to(enemy, PowerId.ARTIFACT, 1, applier=combat.player)

        combat.apply_power_to(enemy, PowerId.STRENGTH, -1, applier=combat.player, source=source)

        assert enemy.get_power_amount(PowerId.ARTIFACT) == 0
        assert enemy.get_power_amount(PowerId.STRENGTH) == 0

        combat.apply_power_to(enemy, PowerId.STRENGTH, -1, applier=combat.player, source=source)

        assert enemy.get_power_amount(PowerId.STRENGTH) == -2

    def test_intimidating_helmet_grants_four_block_when_playing_cards_costing_two_or_more(self):
        """Matches IntimidatingHelmet.cs: cards spending at least 2 energy grant 4 block before play."""
        combat = _make_ironclad_combat(["IntimidatingHelmet"], seed=1901)
        bash = make_bash()
        bash.owner = combat.player
        combat.hand = [bash]
        combat.energy = 2
        combat.player.block = 0

        assert combat.play_card(0, 0)
        assert combat.player.block == 4

    def test_intimidating_helmet_uses_actual_energy_spent_for_x_cards(self):
        """Matches IntimidatingHelmet.cs: cardPlay.Resources.EnergyValue drives the block trigger."""
        combat = _make_ironclad_combat(["IntimidatingHelmet"], seed=1919)
        whirlwind = make_whirlwind()
        whirlwind.owner = combat.player
        combat.hand = [whirlwind]
        combat.energy = 2
        combat.player.block = 0

        assert combat.play_card(0)
        assert combat.player.block == 4

    def test_intimidating_helmet_block_triggers_after_block_gained_hooks(self):
        combat = _make_ironclad_combat(["IntimidatingHelmet"], seed=1925)
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        bash = make_bash()
        bash.owner = combat.player
        combat.player.apply_power(PowerId.JUGGERNAUT, 5)
        combat.hand = [bash]
        combat.energy = 2
        combat.player.block = 0

        assert combat.play_card(0, 0)

        assert combat.player.block == 4
        assert enemy.current_hp == start_hp - 13

    def test_ivory_tile_refunds_one_energy_after_three_cost_card_is_played(self):
        """Matches IvoryTile.cs: cards spending at least 3 energy refund 1 energy."""
        combat = _make_ironclad_combat(["IvoryTile"], seed=1902)
        bash = make_bash()
        bash.owner = combat.player
        bash.set_temporary_cost_for_turn(3)
        combat.hand = [bash]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert combat.energy == 1

    def test_cloak_clasp_gains_block_equal_to_cards_in_hand_before_turn_end(self):
        """Matches CloakClasp.cs: before turn end, gain block equal to hand size."""
        combat = _make_ironclad_combat(["CloakClasp"], seed=1903)
        relic = _combat_relic(combat, "CLOAK_CLASP")
        combat.hand = [
            make_strike_ironclad(),
            make_defend_ironclad(),
            make_bash(),
            make_inflame(),
        ]
        combat.player.block = 0

        relic.before_turn_end(combat.player, CombatSide.PLAYER, combat)
        assert combat.player.block == 4

    def test_cloak_clasp_block_triggers_after_block_gained_hooks(self):
        combat = _make_ironclad_combat(["CloakClasp"], seed=1926)
        relic = _combat_relic(combat, "CLOAK_CLASP")
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        combat.hand = [make_strike_ironclad(), make_defend_ironclad()]
        combat.player.apply_power(PowerId.JUGGERNAUT, 5)
        combat.player.block = 0

        relic.before_turn_end(combat.player, CombatSide.PLAYER, combat)

        assert combat.player.block == 2
        assert enemy.current_hp == start_hp - 5

    def test_meat_on_the_bone_heals_twelve_after_victory_when_below_half_hp(self):
        """Matches MeatOnTheBone.cs: if combat ends at or below 50% HP, heal 12."""
        combat = _make_ironclad_combat(["MeatOnTheBone"], seed=1904)
        enemy = combat.enemies[0]
        enemy.current_hp = 6
        enemy.max_hp = 6
        strike = make_strike_ironclad()
        strike.owner = combat.player
        combat.hand = [strike]
        combat.energy = 1
        combat.player.current_hp = 40

        assert combat.play_card(0, 0)
        assert combat.is_over and combat.player_won
        assert combat.player.current_hp == 52

    def test_meat_on_the_bone_heals_before_burning_blood_victory_heal(self):
        """Matches Hook.AfterCombatVictory: early victory hooks run before normal victory hooks."""
        combat = _make_ironclad_combat(["BurningBlood", "MeatOnTheBone"], seed=1923)
        enemy = combat.enemies[0]
        enemy.current_hp = 6
        enemy.max_hp = 6
        strike = make_strike_ironclad()
        strike.owner = combat.player
        combat.hand = [strike]
        combat.energy = 1
        combat.player.current_hp = 39

        assert combat.play_card(0, 0)
        assert combat.player.current_hp == 57

    def test_ruined_helmet_doubles_first_positive_strength_received_only_once(self):
        """Matches RuinedHelmet.cs: first positive Strength amount received in combat is doubled."""
        combat = _make_ironclad_combat(["RuinedHelmet"], seed=1924)

        combat.player.apply_power(PowerId.STRENGTH, 2)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 4

        combat.player.apply_power(PowerId.STRENGTH, 2)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 6

    def test_vexing_puzzlebox_uses_combat_card_pool(self):
        """Matches VexingPuzzlebox.cs: generated card comes from CardFactory.GetDistinctForCombat."""
        run = RunState(seed=10, character_id="Ironclad")
        run.player.deck = create_ironclad_starter_deck()
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=run.player.deck,
            rng_seed=10,
            character_id="Ironclad",
            relics=["VexingPuzzlebox"],
            player_state=run.player,
        )
        creature, ai = create_shrinker_beetle(Rng(10))
        combat.add_enemy(creature, ai)

        combat.start_combat()

        generated = combat.hand[-1]
        assert generated.card_id != CardId.BASH
        # v0.111.0: SetToFreeThisTurn instead of EnergyCost.SetThisCombat(0).
        assert generated.cost == 0

        base_cost = generated.original_cost
        combat.end_player_turn()

        assert generated.cost == base_cost

    def test_power_cell_moves_two_zero_cost_cards_from_draw_pile_to_hand_on_round_one(self):
        """Matches PowerCell.cs: round 1 moves up to 2 random zero-cost cards from draw to hand."""
        combat = _make_defect_combat(["PowerCell"], seed=1905)
        relic = _combat_relic(combat, "POWER_CELL")
        boost_away = make_boost_away()
        beam_cell = make_beam_cell()
        charge_battery = make_charge_battery()
        for card in (boost_away, beam_cell, charge_battery):
            card.owner = combat.player
        combat.hand = []
        combat.draw_pile = [boost_away, beam_cell, charge_battery]

        relic.before_side_turn_start(combat.player, CombatSide.PLAYER, combat)

        assert {card.card_id for card in combat.hand} == {CardId.BOOST_AWAY, CardId.BEAM_CELL}
        assert [card.card_id for card in combat.draw_pile] == [CardId.CHARGE_BATTERY]

    def test_orange_dough_uses_combat_colorless_card_pool(self):
        """Matches OrangeDough.cs: colorless cards come from CardFactory.GetDistinctForCombat."""
        run = RunState(seed=17, character_id="Ironclad")
        run.player.deck = create_ironclad_starter_deck()
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=run.player.deck,
            rng_seed=17,
            character_id="Ironclad",
            relics=["OrangeDough"],
            player_state=run.player,
        )
        creature, ai = create_shrinker_beetle(Rng(17))
        combat.add_enemy(creature, ai)

        combat.start_combat()

        assert CardId.HAND_OF_GREED not in {card.card_id for card in combat.hand[-2:]}

    def test_rainbow_ring_only_activates_once_per_turn(self):
        """Matches RainbowRing.cs: first Attack+Skill+Power trio per turn grants exactly 1 Str and 1 Dex."""
        combat = _make_ironclad_combat(["RainbowRing"], seed=1906)
        enemy = combat.enemies[0]
        enemy.max_hp = 500
        enemy.current_hp = 500
        cards = [
            make_strike_ironclad(),
            make_defend_ironclad(),
            make_barricade(),
            make_strike_ironclad(),
            make_defend_ironclad(),
            make_barricade(),
        ]
        for card in cards:
            card.owner = combat.player
        combat.hand = cards
        combat.energy = 10

        assert combat.play_card(0, 0)
        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.play_card(0, 0)
        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 1

    def test_attack_count_relics_ignore_cards_played_by_other_players(self):
        """Matches owner checks in Kunai.cs, Shuriken.cs, RainbowRing.cs, and RazorTooth.cs."""
        combat = _make_ironclad_combat(["Kunai", "Shuriken", "RainbowRing", "RazorTooth"], seed=1909)
        enemy = combat.enemies[0]
        enemy.max_hp = 500
        enemy.current_hp = 500
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None
        cards = [
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_defend_ironclad(),
            make_barricade(),
        ]
        ally_state.hand = cards
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 10

        assert combat.play_card_from_creature(ally, 0, 0)
        assert combat.play_card_from_creature(ally, 0, 0)
        assert combat.play_card_from_creature(ally, 0, 0)
        assert combat.play_card_from_creature(ally, 0)
        assert combat.play_card_from_creature(ally, 0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 0
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 0
        assert all(card.upgraded is False for card in cards)

    def test_cost_relics_ignore_cards_played_by_other_players(self):
        """Matches owner checks in IntimidatingHelmet.cs and IvoryTile.cs."""
        combat = _make_ironclad_combat(["IntimidatingHelmet", "IvoryTile"], seed=1910)
        enemy = combat.enemies[0]
        enemy.max_hp = 500
        enemy.current_hp = 500
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None
        bash = make_bash()
        bash.set_temporary_cost_for_turn(3)
        ally_state.hand = [bash]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 3
        combat.energy = 0
        combat.player.block = 0

        assert combat.play_card_from_creature(ally, 0, 0)
        assert combat.player.block == 0
        assert combat.energy == 0

    def test_helical_dart_grants_temporary_dexterity_until_turn_end(self):
        combat = _make_defect_combat(["HelicalDart"], seed=1932)
        enemy = combat.enemies[0]
        enemy.max_hp = 200
        enemy.current_hp = 200
        combat.hand = [make_shiv(), make_shiv()]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert combat.player.get_power_amount(PowerId.HELICAL_DART) == 1
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 1

        assert combat.play_card(0, 0)
        assert combat.player.get_power_amount(PowerId.HELICAL_DART) == 2
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 2

        fire_after_turn_end(CombatSide.PLAYER, combat)

        assert combat.player.get_power_amount(PowerId.HELICAL_DART) == 0
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 0

    def test_shop_event_card_play_relics_ignore_other_players_cards(self):
        """Matches owner checks on shop/event/ancient card-play relic hooks."""
        combat = _make_ironclad_combat(
            [
                "BrilliantScarf",
                "DaughterOfTheWind",
                "IronClub",
                "LostWisp",
                "MusicBox",
                "PaelsEye",
                "VelvetChoker",
            ],
            seed=1911,
        )
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        enemy = combat.enemies[0]
        enemy.max_hp = 300
        enemy.current_hp = 300
        marker = make_strike_ironclad()
        marker.owner = combat.player
        combat.hand = []
        combat.draw_pile = [marker]
        combat.player.block = 0

        ally_attack = make_strike_ironclad()
        ally_attack.owner = ally
        ally_skill = make_defend_ironclad()
        ally_skill.owner = ally
        ally_power = make_barricade()
        ally_power.owner = ally
        owner_attack = make_strike_ironclad()
        owner_attack.owner = combat.player

        fire_before_card_played(ally_attack, combat)
        for card in [ally_attack, ally_skill, ally_power, ally_attack, ally_skill, ally_power]:
            fire_after_card_played(card, combat)

        assert combat.player.block == 0
        assert enemy.current_hp == 300
        assert combat.hand == []
        assert marker not in combat.hand
        assert _combat_relic(combat, "BRILLIANT_SCARF")._cards_this_turn == 0
        assert _combat_relic(combat, "VELVET_CHOKER").should_play(combat.player, owner_attack, combat) is None
        assert _combat_relic(combat, "PAELS_EYE").should_take_extra_turn(combat.player, combat) is True

    def test_daughter_of_the_wind_block_triggers_after_block_gained_hooks(self):
        combat = _make_ironclad_combat(["DaughterOfTheWind"], seed=1927)
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        strike = make_strike_ironclad()
        strike.owner = combat.player
        combat.player.apply_power(PowerId.JUGGERNAUT, 5)
        combat.hand = [strike]
        combat.energy = 1
        combat.player.block = 0

        assert combat.play_card(0, 0)

        assert combat.player.block == 1
        assert enemy.current_hp == start_hp - 11

    def test_diamond_diadem_grants_block_and_blur_only_on_turn_one(self):
        combat = _make_ironclad_combat(["DiamondDiadem"], seed=1912)

        assert combat.player.block == 20
        assert combat.player.get_power_amount(PowerId.BLUR) == 1

        combat.end_player_turn()
        assert combat.player.get_power_amount(PowerId.BLUR) == 0
        block_after_round_two_start = combat.player.block

        fire_after_side_turn_start(CombatSide.PLAYER, combat)
        assert combat.player.block == block_after_round_two_start
        assert combat.player.get_power_amount(PowerId.BLUR) == 0

    def test_brilliant_scarf_makes_fifth_owner_card_energy_and_star_cost_free(self):
        combat = _make_ironclad_combat(["BrilliantScarf"], seed=1918)
        player = combat.player
        scarf = _combat_relic(combat, "BRILLIANT_SCARF")
        for _ in range(4):
            card = make_strike_ironclad()
            card.owner = player
            fire_after_card_played(card, combat)

        fifth = make_bash()
        fifth.owner = player
        fifth.star_cost = 2
        combat.hand = [fifth]

        assert scarf._cards_this_turn == 4
        assert combat.modified_card_cost(player, fifth) == 0
        assert combat.modified_star_cost(player, fifth) == 0

    def test_brilliant_scarf_only_modifies_hand_or_play_cards(self):
        combat = _make_ironclad_combat(["BrilliantScarf"], seed=1920)
        player = combat.player
        for _ in range(4):
            card = make_strike_ironclad()
            card.owner = player
            fire_after_card_played(card, combat)

        fifth = make_bash()
        fifth.owner = player
        fifth.star_cost = 2
        combat.discard_pile = [fifth]

        assert combat.modified_card_cost(player, fifth) == 2
        assert combat.modified_star_cost(player, fifth) == 2

    def test_brilliant_scarf_actual_fifth_play_spends_no_energy(self):
        combat = _make_ironclad_combat(["BrilliantScarf"], seed=1921)
        player = combat.player
        for _ in range(4):
            card = make_strike_ironclad()
            card.owner = player
            fire_after_card_played(card, combat)

        fifth = make_bash()
        fifth.owner = player
        combat.hand = [fifth]
        combat.energy = 5

        assert combat.play_card(0, 0)
        assert fifth.energy_spent == 0
        assert combat.energy == 5

    def test_music_box_locks_outer_attack_before_nested_attack_hooks(self):
        combat = _make_ironclad_combat(["MusicBox"], seed=1919)
        outer = make_bash()
        outer.owner = combat.player
        nested = make_strike_ironclad()
        nested.owner = combat.player
        combat.hand = []

        fire_before_card_played(outer, combat)
        fire_before_card_played(nested, combat)
        fire_after_card_played(nested, combat)
        assert combat.hand == []

        fire_after_card_played(outer, combat)

        assert len(combat.hand) == 1
        assert combat.hand[0].card_id == CardId.BASH
        assert "ethereal" in combat.hand[0].keywords

    def test_throwing_axe_ignores_other_players_first_card(self):
        """Matches ThrowingAxe.cs: the first doubled card must belong to the relic owner."""
        combat = _make_ironclad_combat(["ThrowingAxe"], seed=1912)
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None
        enemy = combat.enemies[0]
        enemy.max_hp = 300
        enemy.current_hp = 300

        ally_strike = make_strike_ironclad()
        ally_state.hand = [ally_strike]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        assert combat.play_card_from_creature(ally, 0, 0)
        assert enemy.current_hp == 294

        owner_strike = make_strike_ironclad()
        owner_strike.owner = combat.player
        combat.hand = [owner_strike]
        combat.energy = 1
        assert combat.play_card(0, 0)
        assert enemy.current_hp == 282

    def test_chemical_x_and_spiked_gauntlets_ignore_other_players_cards(self):
        """Matches owner checks in ChemicalX.cs and SpikedGauntlets.cs."""
        combat = _make_ironclad_combat(["ChemicalX", "SpikedGauntlets"], seed=1913)
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None
        chemical_x = _combat_relic(combat, "CHEMICAL_X")

        ally_x_card = SimpleNamespace(
            owner=ally,
            has_energy_cost_x=True,
            combat_vars={},
            energy_spent=3,
        )
        assert chemical_x.modify_x_value(combat.player, 3, ally_x_card) == 3
        chemical_x.before_card_played(combat.player, ally_x_card, combat)
        assert ally_x_card.energy_spent == 3

        owner_x_card = SimpleNamespace(owner=combat.player)
        assert chemical_x.modify_x_value(combat.player, 3, owner_x_card) == 5

        ally_power = make_inflame()
        ally_state.hand = [ally_power]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1
        assert combat.play_card_from_creature(ally, 0)
        assert ally_state.energy == 0
        assert ally_power.energy_spent == 1

    def test_owner_scoped_exhaust_relics_ignore_other_players_cards(self):
        """Matches owner checks in CharonsAshes.cs, BurningSticks.cs, and ForgottenSoul.cs."""
        combat = _make_ironclad_combat(["CharonsAshes", "BurningSticks", "ForgottenSoul"], seed=1914)
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        enemy = combat.enemies[0]
        enemy.max_hp = 300
        enemy.current_hp = 300
        combat.hand = []

        ally_skill = make_defend_ironclad()
        ally_skill.owner = ally
        fire_after_card_exhausted(ally_skill, combat)

        assert enemy.current_hp == 300
        assert combat.hand == []

        owner_skill = make_defend_ironclad()
        owner_skill.owner = combat.player
        fire_after_card_exhausted(owner_skill, combat)

        assert enemy.current_hp == 296
        assert len(combat.hand) == 1

    def test_tough_bandages_ignores_other_players_discards(self):
        """Matches ToughBandages.cs: only owner-card discards grant block."""
        combat = _make_ironclad_combat(["ToughBandages"], seed=1915)
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        combat.player.block = 0

        ally_card = make_strike_ironclad()
        ally_card.owner = ally
        fire_after_card_discarded(ally_card, combat)
        assert combat.player.block == 0

        owner_card = make_strike_ironclad()
        owner_card.owner = combat.player
        fire_after_card_discarded(owner_card, combat)
        assert combat.player.block == 3

    def test_razor_tooth_upgrades_played_attack_or_skill_card_in_combat(self):
        """Matches RazorTooth.cs: playing an upgradable Attack or Skill upgrades that card in combat."""
        combat = _make_ironclad_combat(["RazorTooth"], seed=1907)
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        strike = make_strike_ironclad()
        strike.owner = combat.player
        combat.hand = [strike]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert strike.upgraded is True

    def test_ghost_seed_makes_basic_strikes_and_defends_ethereal_in_combat(self):
        """Matches GhostSeed.cs: basic Strike/Defend cards become Ethereal when combat starts."""
        combat = _make_ironclad_combat(["GhostSeed"], seed=1908)
        all_cards = combat.hand + combat.draw_pile + combat.discard_pile + combat.exhaust_pile
        basics = [
            card for card in all_cards
            if card.rarity == CardRarity.BASIC and (CardTag.STRIKE in card.tags or CardTag.DEFEND in card.tags)
        ]
        bashes = [card for card in all_cards if card.card_id == CardId.BASH]

        assert basics
        assert all(card.is_ethereal for card in basics)
        assert bashes
        assert all(not card.is_ethereal for card in bashes)

        generated = make_strike_ironclad()
        combat.move_card_to_creature_hand(combat.player, generated)
        assert generated.is_ethereal
