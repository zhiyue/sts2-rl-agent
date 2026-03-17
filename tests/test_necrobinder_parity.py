"""Necrobinder parity tests for decompiled summon and card-state flows."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.necrobinder import (
    make_afterlife,
    make_bone_shards,
    make_bodyguard,
    make_death_march,
    create_necrobinder_starter_deck,
    make_capture_spirit,
    make_dirge,
    make_eidolon,
    make_grave_warden,
    make_high_five,
    make_legion_of_bone,
    make_necro_mastery,
    make_pull_aggro,
    make_protector,
    make_rattle,
    make_reanimate,
    make_right_hand_hand,
    make_sacrifice,
    make_seance,
    make_spur,
    make_transfigure,
    make_undeath,
)
from sts2_env.cards.status import make_soul
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CombatSide, PowerId, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=create_necrobinder_starter_deck(),
        rng_seed=42,
        character_id="Necrobinder",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestNecrobinderParity:
    def test_borrowed_time_applies_doom_and_gains_energy(self):
        """Matches BorrowedTime.cs: apply Doom to the owner, then gain energy immediately."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.BORROWED_TIME)]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.DOOM) == 3
        assert combat.energy == 1

    def test_countdown_card_applies_countdown_power(self):
        """Matches Countdown.cs: apply CountdownPower to the owner."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.COUNTDOWN_CARD)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.COUNTDOWN) == 6

    def test_danse_macabre_applies_danse_macabre_power(self):
        """Matches DanseMacabre.cs: apply DanseMacabrePower to the owner."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.DANSE_MACABRE)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.DANSE_MACABRE) == 3

    def test_death_march_scales_with_non_opening_draws_this_turn(self):
        """Matches DeathMarch.cs: damage = base + extra * non-opening draws this turn."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_death_march()]
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad()]
        combat.energy = 1
        combat.draw_cards(combat.player, 2)

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 14

    def test_capture_spirit_deals_unblockable_damage_and_adds_souls_to_draw(self):
        """Matches CaptureSpirit.cs: unblockable damage plus generated Souls to draw pile."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.block = 20
        combat.hand = [make_capture_spirit()]
        combat.energy = 1
        starting_hp = enemy.current_hp

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 3
        assert enemy.block == 20
        souls = [card for card in combat.draw_pile if card.card_id == CardId.SOUL]
        assert len(souls) == 3

    def test_sacrifice_kills_osty_and_gains_double_osty_hp_as_block(self):
        """Matches Sacrifice.cs: kill Osty, then gain block equal to twice its max HP."""
        combat = _make_combat()
        combat.summon_osty(combat.player, 5)
        combat.hand = [make_sacrifice()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert not combat.osty.is_alive
        assert combat.player.block == 10

    def test_transfigure_requests_choice_then_grants_replay_and_cost(self):
        """Matches Transfigure.cs: select a hand card, add replay, and increase non-X cost."""
        combat = _make_combat()
        target_card = make_strike_ironclad()
        combat.hand = [make_transfigure(), target_card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [target_card]

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None
        assert target_card.base_replay_count == 1
        assert target_card.cost == 2

    def test_ally_right_hand_hand_returns_to_ally_hand_using_ally_energy(self):
        combat = _make_combat()
        ally_state = PlayerState(player_id=2, character_id="Necrobinder", max_hp=45, current_hp=45)
        ally = combat.add_ally_player(ally_state)
        ally_combat_state = combat.combat_player_state_for(ally)
        assert ally_combat_state is not None
        combat.summon_osty(ally, 5)
        card = make_right_hand_hand()
        card.owner = ally
        ally_combat_state.hand = [card]
        ally_combat_state.zone_map["hand"] = ally_combat_state.hand
        ally_combat_state.energy = 2
        combat.energy = 0

        assert combat.play_card_from_creature(ally, 0, 0)
        assert card in ally_combat_state.hand
        assert card not in combat.hand

    def test_undeath_gains_block_and_clones_itself_to_discard(self):
        """Matches Undeath.cs: gain block and add a generated clone of itself to discard."""
        combat = _make_combat()
        combat.hand = [make_undeath()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.block == 7
        clones = [card for card in combat.discard_pile if card.card_id == CardId.UNDEATH]
        assert len(clones) == 2

    def test_bodyguard_summons_osty_at_card_summon_value(self):
        """Matches Bodyguard.cs: summon Osty using the card's summon value."""
        combat = _make_combat()
        combat.hand = [make_bodyguard()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 5

    def test_afterlife_summons_stronger_osty_and_exhausts_self(self):
        """Matches Afterlife.cs: summon Osty for 6 and exhaust the card."""
        combat = _make_combat()
        card = make_afterlife()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 6
        assert card in combat.exhaust_pile

    def test_grave_warden_gains_block_and_adds_soul_to_draw(self):
        """Matches GraveWarden.cs: gain block, then add Soul cards into draw pile."""
        combat = _make_combat()
        combat.hand = [make_grave_warden()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 8
        souls = [card for card in combat.draw_pile if card.card_id == CardId.SOUL]
        assert len(souls) == 1

    def test_pull_aggro_summons_osty_and_gains_block(self):
        """Matches PullAggro.cs: summon first, then gain block."""
        combat = _make_combat()
        combat.hand = [make_pull_aggro()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 4
        assert combat.player.block == 7

    def test_legion_of_bone_summons_for_all_alive_player_allies(self):
        """Matches LegionOfBone.cs: summon Osty for every alive player creature."""
        combat = _make_combat()
        ally = Creature(max_hp=45, current_hp=45, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        card = make_legion_of_bone()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 6
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None
        ally_osties = [
            creature
            for creature in combat.allies
            if getattr(creature, "is_osty", False) and getattr(creature, "pet_owner", None) is ally
        ]
        assert len(ally_osties) == 1
        assert ally_osties[0].max_hp == 6

    def test_reanimate_summons_large_osty_and_exhausts_self(self):
        """Matches Reanimate.cs: summon a large Osty and exhaust the card."""
        combat = _make_combat()
        card = make_reanimate()
        combat.hand = [card]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 20
        assert card in combat.exhaust_pile

    def test_spur_summons_and_heals_existing_osty(self):
        """Matches Spur.cs: summon more Osty HP, then heal Osty."""
        combat = _make_combat()
        combat.summon_osty(combat.player, 3)
        assert combat.osty is not None
        combat.osty.current_hp = 1
        combat.hand = [make_spur()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.osty.max_hp == 6
        assert combat.osty.current_hp == 6

    def test_necro_mastery_summons_osty_and_applies_power(self):
        """Matches NecroMastery.cs: summon Osty and apply NecroMasteryPower."""
        combat = _make_combat()
        combat.hand = [make_necro_mastery()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 5
        assert combat.player.get_power_amount(PowerId.NECRO_MASTERY) == 1

    def test_ally_sacrifice_kills_only_ally_osty_and_grants_ally_block(self):
        combat = _make_combat()
        ally_state = PlayerState(player_id=2, character_id="Necrobinder", max_hp=45, current_hp=45)
        ally = combat.add_ally_player(ally_state)
        ally_combat_state = combat.combat_player_state_for(ally)
        assert ally_combat_state is not None

        combat.summon_osty(combat.player, 5)
        combat.summon_osty(ally, 4)
        primary_osty = combat.get_osty(combat.player)
        ally_osty = combat.get_osty(ally)
        assert primary_osty is not None and ally_osty is not None

        card = make_sacrifice()
        card.owner = ally
        ally_combat_state.hand = [card]
        ally_combat_state.zone_map["hand"] = ally_combat_state.hand
        ally_combat_state.energy = 1

        assert combat.play_card_from_creature(ally, 0)
        assert primary_osty.is_alive
        assert not ally_osty.is_alive
        assert ally.block == 8
        assert combat.player.block == 0

    def test_ally_spur_heals_and_grows_only_ally_osty(self):
        combat = _make_combat()
        ally_state = PlayerState(player_id=2, character_id="Necrobinder", max_hp=45, current_hp=45)
        ally = combat.add_ally_player(ally_state)
        ally_combat_state = combat.combat_player_state_for(ally)
        assert ally_combat_state is not None

        combat.summon_osty(combat.player, 3)
        combat.summon_osty(ally, 3)
        primary_osty = combat.get_osty(combat.player)
        ally_osty = combat.get_osty(ally)
        assert primary_osty is not None and ally_osty is not None
        primary_osty.current_hp = 1
        ally_osty.current_hp = 1

        card = make_spur()
        card.owner = ally
        ally_combat_state.hand = [card]
        ally_combat_state.zone_map["hand"] = ally_combat_state.hand
        ally_combat_state.energy = 1

        assert combat.play_card_from_creature(ally, 0)
        assert primary_osty.max_hp == 3
        assert primary_osty.current_hp == 1
        assert ally_osty.max_hp == 6
        assert ally_osty.current_hp == 6

    def test_ally_necro_mastery_applies_power_and_summons_for_ally_owner(self):
        combat = _make_combat()
        ally_state = PlayerState(player_id=2, character_id="Necrobinder", max_hp=45, current_hp=45)
        ally = combat.add_ally_player(ally_state)
        ally_combat_state = combat.combat_player_state_for(ally)
        assert ally_combat_state is not None

        card = make_necro_mastery()
        card.owner = ally
        ally_combat_state.hand = [card]
        ally_combat_state.zone_map["hand"] = ally_combat_state.hand
        ally_combat_state.energy = 2

        assert combat.play_card_from_creature(ally, 0)
        ally_osty = combat.get_osty(ally)
        assert ally_osty is not None
        assert ally_osty.max_hp == 5
        assert ally.get_power_amount(PowerId.NECRO_MASTERY) == 1
        assert combat.player.get_power_amount(PowerId.NECRO_MASTERY) == 0

    def test_dirge_uses_x_energy_for_multiple_summons_and_souls(self):
        """Matches Dirge.cs: X-cost summons Osty X times and adds X Souls to draw."""
        combat = _make_combat()
        combat.hand = [make_dirge()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.osty is not None
        assert combat.osty.max_hp == 6
        souls = [card for card in combat.draw_pile if card.card_id == CardId.SOUL]
        assert len(souls) == 2

    def test_eidolon_exhausts_hand_and_grants_intangible_at_threshold(self):
        """Matches Eidolon.cs: exhaust the hand, then grant Intangible if 9+ cards were exhausted."""
        combat = _make_combat()
        combat.hand = [make_eidolon()] + [make_strike_ironclad() for _ in range(9)]
        combat.energy = 2

        assert combat.play_card(0)
        assert len(combat.hand) == 0
        assert len(combat.exhaust_pile) >= 9
        assert combat.player.get_power_amount(PowerId.INTANGIBLE) == 1

    def test_protector_scales_damage_with_owner_osty_max_hp(self):
        """Matches Protector.cs: damage is based on the owner's live Osty max HP."""
        combat = _make_combat()
        combat.summon_osty(combat.player, 5)
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_protector()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 15

    def test_bone_shards_hits_all_enemies_gains_block_and_kills_osty(self):
        """Matches BoneShards.cs: Osty attack all enemies, gain block, then kill Osty."""
        combat = _make_combat()
        combat.summon_osty(combat.player, 5)
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_bone_shards()]
        combat.energy = 1

        assert combat.play_card(0)
        assert enemy.current_hp == starting_hp - 9
        assert combat.player.block == 9
        assert combat.osty is not None
        assert not combat.osty.is_alive

    def test_rattle_uses_prior_osty_hits_this_turn_for_hit_count(self):
        """Matches Rattle.cs: hit count is 1 + prior Osty attacks this turn."""
        combat = _make_combat()
        combat.summon_osty(combat.player, 5)
        osty = combat.osty
        assert osty is not None
        enemy = combat.enemies[0]
        combat.deal_damage(dealer=osty, target=enemy, amount=4, props=ValueProp.MOVE)
        starting_hp = enemy.current_hp
        combat.hand = [make_rattle()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 14

    def test_ally_high_five_uses_ally_osty_when_primary_osty_is_missing(self):
        """Matches HighFive.cs owner semantics: allied Necrobinder can use its own Osty."""
        combat = _make_combat()
        ally_state = PlayerState(player_id=2, character_id="Necrobinder", max_hp=45, current_hp=45)
        ally = combat.add_ally_player(ally_state)
        ally_combat_state = combat.combat_player_state_for(ally)
        assert ally_combat_state is not None

        combat.summon_osty(ally, 4)
        ally_osty = combat.get_osty(ally)
        assert ally_osty is not None and ally_osty.is_alive
        assert combat.osty is None

        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        card = make_high_five()
        card.owner = ally
        ally_combat_state.hand = [card]
        ally_combat_state.zone_map["hand"] = ally_combat_state.hand
        ally_combat_state.energy = 2

        assert combat.can_play_card(card) is True
        assert combat.play_card_from_creature(ally, 0)
        assert enemy.current_hp == starting_hp - 11
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

    def test_seance_sorts_draw_pile_and_transforms_selected_card_into_upgraded_soul(self):
        """Matches Seance.cs: sorted simple-grid selection, then transform chosen card into Soul."""
        combat = _make_combat()
        source = make_strike_ironclad()
        other = make_undeath()
        seance = make_seance(upgraded=True)
        combat.hand = [seance]
        combat.draw_pile = [other, source]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        expected = sorted([other, source], key=lambda current: (current.rarity.value, current.card_id.value))
        assert [option.card for option in combat.pending_choice.options] == expected

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None
        transformed = expected[0]
        assert transformed not in combat.draw_pile
        assert any(card.card_id == make_soul(upgraded=True).card_id and card.upgraded for card in combat.draw_pile)

    def test_borrowed_time_applies_doom_and_gains_energy(self):
        """Matches BorrowedTime.cs: apply Doom to self, then gain energy."""
        combat = _make_combat()
        card = create_card(CardId.BORROWED_TIME)
        combat.hand = [card]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.DOOM) == card.effect_vars.get("doom", 3)
        assert combat.energy == card.effect_vars.get("energy", 1)

    def test_countdown_applies_countdown_power(self):
        """Matches Countdown.cs: apply CountdownPower with the configured amount."""
        combat = _make_combat()
        card = create_card(CardId.COUNTDOWN_CARD)
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.COUNTDOWN) == card.effect_vars.get("countdown", 6)

    def test_danse_macabre_applies_danse_macabre_power(self):
        """Matches DanseMacabre.cs: apply DanseMacabrePower with the configured amount."""
        combat = _make_combat()
        card = create_card(CardId.DANSE_MACABRE)
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.DANSE_MACABRE) == card.effect_vars.get("danse_macabre", 3)

    def test_death_march_scales_damage_with_non_hand_draws_this_turn(self):
        """Matches DeathMarch.cs: damage scales with this-turn draws that were not opening-hand draws."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        card = make_death_march()
        combat.hand = [card]
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad()]
        combat.energy = 1
        combat.draw_cards(combat.player, 2)
        starting_hp = enemy.current_hp

        assert combat.play_card(0, 0)
        expected_damage = card.effect_vars.get("calc_base", card.base_damage or 8) + 2 * card.effect_vars.get("extra_damage", 3)
        assert enemy.current_hp == starting_hp - expected_damage
