"""Reference parity tests for Colorless card effects."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.base import CardInstance
from sts2_env.cards.status import make_decay, make_slimed
from sts2_env.cards.colorless import (
    make_anointed,
    make_automation,
    make_beacon_of_hope,
    make_calamity_card,
    make_catastrophe,
    make_coordinate_card,
    make_dark_shackles,
    make_equilibrium,
    make_eternal_armor,
    make_finesse,
    make_fisticuffs,
    make_gang_up,
    make_gold_axe,
    make_hidden_gem,
    make_huddle_up,
    make_impatience,
    make_intercept_card,
    make_jackpot,
    make_knockdown,
    make_nostalgia_card,
    make_prep_time,
    make_production,
    make_prolong,
    make_prowess,
    make_rend,
    make_rolling_boulder,
    make_salvo,
    make_scrawl,
)
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CardRarity, CardType, CombatSide, PowerId, TargetType, ValueProp
from sts2_env.core.hooks import fire_after_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


CALAMITY_POWER_AMOUNT = 1
NOSTALGIA_POWER_AMOUNT = 1
AUTOMATION_DRAW_THRESHOLD = 10
AUTOMATION_ENERGY_GAIN = 1
KNOCKDOWN_BASE_DAMAGE = 10
KNOCKDOWN_BASE_MULTIPLIER = 2
KNOCKDOWN_PLUS_DAMAGE = 14
KNOCKDOWN_PLUS_MULTIPLIER = 3
PREP_TIME_BASE_VIGOR = 4
PREP_TIME_PLUS_VIGOR = 6
ROLLING_BOULDER_BASE_DAMAGE = 5
ROLLING_BOULDER_PLUS_DAMAGE = 10
ROLLING_BOULDER_INCREMENT = 5


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=4242,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(4242))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestColorlessCardEffectsReferenceParity:
    def test_anointed_moves_only_rare_draw_pile_cards_to_hand_and_exhausts(self):
        """Matches Anointed.cs OnPlay (lines 22-24): count = MaxCardsInHand -
        hand size; that many RANDOM rare draw-pile cards (TakeRandom, no
        replacement) fill the hand's free slots."""
        combat = _make_combat()
        rare = CardInstance(
            card_id=CardId.ALCHEMIZE,
            cost=1,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.RARE,
        )
        common = make_strike_ironclad()
        combat.hand = [make_anointed()]
        combat.draw_pile = [common, rare]
        combat.energy = 1

        assert combat.play_card(0)

        assert rare in combat.hand
        assert common in combat.draw_pile
        assert any(card.card_id == CardId.ANOINTED for card in combat.exhaust_pile)

    def test_anointed_only_fills_free_hand_slots(self):
        """Anointed.cs: `MaxCardsInHand - hand.Count` bounds the move — when the
        hand already has MaxHandSize-1 free slot, exactly one rare moves."""
        from sts2_env.core.constants import MAX_HAND_SIZE

        combat = _make_combat()
        rares = [
            CardInstance(
                card_id=CardId.ALCHEMIZE,
                cost=1,
                card_type=CardType.SKILL,
                target_type=TargetType.SELF,
                rarity=CardRarity.RARE,
            )
            for _ in range(3)
        ]
        filler = [make_strike_ironclad() for _ in range(MAX_HAND_SIZE - 2)]
        combat.hand = [make_anointed(), *filler]
        combat.draw_pile = list(rares)
        combat.energy = 1

        assert combat.play_card(0)
        # One free slot after the play (Anointed exhausts), so only one rare moves.
        assert len(combat.hand) == MAX_HAND_SIZE - 1
        assert sum(1 for c in combat.draw_pile if c.rarity == CardRarity.RARE) == 2

    def test_anointed_plus_adds_retain(self):
        """Matches Anointed.cs: upgraded Anointed adds Retain."""
        assert make_anointed(upgraded=True).is_retain

    def test_automation_plus_costs_zero_and_grants_energy_after_ten_owner_draws(self):
        """Matches Automation.cs / AutomationPower.cs: every 10 owner draws gains configured energy."""
        combat = _make_combat()
        drawn = [make_defend_ironclad() for _ in range(AUTOMATION_DRAW_THRESHOLD)]
        combat.hand = [make_automation(upgraded=True)]
        combat.draw_pile = list(drawn)
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.AUTOMATION) == AUTOMATION_ENERGY_GAIN
        assert combat.energy == 0

        combat.draw_cards(combat.player, AUTOMATION_DRAW_THRESHOLD)

        assert combat.energy == AUTOMATION_ENERGY_GAIN
        assert combat.hand == drawn

    def test_calamity_generates_owner_attack_after_owner_attack_only(self):
        combat = _make_combat()
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None

        ally_attack = make_strike_ironclad()
        ally_attack.owner = ally
        ally_state.hand = [ally_attack]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1

        owner_attack = make_strike_ironclad()
        combat.hand = [make_calamity_card(), owner_attack]
        combat.energy = 4

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.CALAMITY) == CALAMITY_POWER_AMOUNT

        assert combat.play_card_from_creature(ally, 0, 0)
        assert ally_state.hand == []

        assert combat.play_card(0, 0)
        assert len(combat.hand) == CALAMITY_POWER_AMOUNT
        assert all(card.card_type == CardType.ATTACK for card in combat.hand)
        assert all(card.rarity not in {CardRarity.BASIC, CardRarity.ANCIENT, CardRarity.EVENT} for card in combat.hand)

    def test_nostalgia_redirects_only_owner_qualifying_cards_to_draw_top(self):
        combat = _make_combat()
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None

        ally_attack = make_strike_ironclad()
        ally_attack.owner = ally
        ally_state.hand = [ally_attack]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1

        owner_attack = make_strike_ironclad()
        owner_skill = make_defend_ironclad()
        combat.hand = [make_nostalgia_card(), owner_attack, owner_skill]
        combat.draw_pile = []
        combat.discard_pile = []
        combat.energy = 4

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.NOSTALGIA) == NOSTALGIA_POWER_AMOUNT

        assert combat.play_card_from_creature(ally, 0, 0)
        assert ally_attack in ally_state.discard
        assert ally_attack not in combat.draw_pile

        assert combat.play_card(0, 0)
        assert combat.draw_pile == [owner_attack]
        assert owner_attack not in combat.discard_pile

        assert combat.play_card(0)
        assert owner_skill in combat.discard_pile
        assert combat.draw_pile == [owner_attack]

    def test_catastrophe_autoplays_two_draw_pile_cards_preferring_playable(self):
        combat = _make_combat()
        first = make_strike_ironclad()
        second = make_defend_ironclad()
        combat.hand = [make_catastrophe()]
        combat.draw_pile = [first, second]
        combat.energy = 2

        assert combat.play_card(0)

        assert first in combat.discard_pile
        assert second in combat.discard_pile
        assert combat.draw_pile == []

    def test_eternal_armor_applies_plating(self):
        combat = _make_combat()
        combat.hand = [make_eternal_armor()]
        combat.energy = 3

        assert combat.play_card(0)

        # EternalArmor.cs:20 defines PowerVar<PlatingPower>(9m) (OnUpgrade at
        # lines 32-35 adds 3 -> 12); v0.107.1 rebalanced the old value of 7.
        assert combat.player.get_power_amount(PowerId.PLATING) == 9

    def test_jackpot_deals_damage_and_adds_three_zero_cost_cards(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_jackpot()]
        combat.energy = 3

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 75
        assert len(combat.hand) == 3
        assert all(card.cost == 0 and not card.has_energy_cost_x for card in combat.hand)

    def test_upgraded_jackpot_upgrades_generated_cards(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_jackpot(upgraded=True)]
        combat.energy = 3

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 70
        assert len(combat.hand) == 3
        assert all(card.upgraded for card in combat.hand)

    def test_prep_time_applies_start_of_turn_vigor_power(self):
        combat = _make_combat()
        combat.hand = [make_prep_time()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.PREP_TIME) == PREP_TIME_BASE_VIGOR

        combat.end_player_turn()

        assert combat.player.get_power_amount(PowerId.VIGOR) == PREP_TIME_BASE_VIGOR

    def test_prep_time_plus_applies_upgraded_start_of_turn_vigor_power(self):
        """Matches PrepTime.cs: upgraded PrepTimePower is 6."""
        combat = _make_combat()
        combat.hand = [make_prep_time(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.PREP_TIME) == PREP_TIME_PLUS_VIGOR

        combat.end_player_turn()

        assert combat.player.get_power_amount(PowerId.VIGOR) == PREP_TIME_PLUS_VIGOR

    def test_prowess_grants_strength_and_dexterity(self):
        combat = _make_combat()
        combat.hand = [make_prowess()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 1

    def test_salvo_deals_damage_and_retains_remaining_hand(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        retained = make_strike_ironclad()
        combat.hand = [make_salvo(), retained]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 12
        assert combat.player.get_power_amount(PowerId.RETAIN_HAND) == 1

        combat.end_player_turn()

        assert retained in combat.hand

    def test_dark_shackles_applies_temporary_strength_loss_then_restores(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.apply_power(PowerId.STRENGTH, 5)
        combat.hand = [make_dark_shackles()]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.STRENGTH) == -4
        assert enemy.get_power_amount(PowerId.DARK_SHACKLES) == 9

        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert enemy.get_power_amount(PowerId.STRENGTH) == 5
        assert enemy.get_power_amount(PowerId.DARK_SHACKLES) == 0

    def test_equilibrium_gains_block_and_retains_remaining_hand(self):
        combat = _make_combat()
        retained = make_strike_ironclad()
        combat.hand = [make_equilibrium(), retained]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == 13
        assert combat.player.get_power_amount(PowerId.RETAIN_HAND) == 1

        combat.end_player_turn()
        assert retained in combat.hand

    def test_impatience_draws_only_when_no_attack_remains_in_hand(self):
        combat = _make_combat()
        skill_draw = make_defend_ironclad()
        combat.hand = [make_impatience()]
        combat.draw_pile = [skill_draw]
        combat.energy = 0

        assert combat.play_card(0)
        assert skill_draw in combat.hand

        combat = _make_combat()
        attack = make_strike_ironclad()
        skill_draw = make_defend_ironclad()
        combat.hand = [make_impatience(), attack]
        combat.draw_pile = [skill_draw]
        combat.energy = 0

        assert combat.play_card(0)
        assert skill_draw not in combat.hand
        assert combat.draw_pile[0] is skill_draw

    def test_impatience_plus_draws_three_when_no_attack_remains_in_hand(self):
        """Matches Impatience.cs: upgraded CardsVar draws 3 if the owner's hand has no Attack."""
        combat = _make_combat()
        drawn = [make_defend_ironclad(), make_defend_ironclad(), make_defend_ironclad()]
        combat.hand = [make_impatience(upgraded=True)]
        combat.draw_pile = list(drawn)
        combat.energy = 0

        assert combat.play_card(0)

        assert combat.hand == drawn
        assert combat.draw_pile == []

    def test_huddle_up_draws_for_owner_and_living_player_teammates(self):
        """Matches HuddleUp.cs OnPlay (lines 26-34): each living player teammate
        draws Cards(3 upgraded). CombatState.GetTeammatesOf (CombatState.cs:397)
        "includes the creature itself", so the caster draws too; dead allies
        (filtered by c.IsAlive) draw nothing."""
        combat = _make_combat()
        living_ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        dead_ally = combat.add_ally_player(
            PlayerState(player_id=3, character_id="Ironclad", max_hp=60, current_hp=0)
        )
        living_state = combat.combat_player_state_for(living_ally)
        dead_state = combat.combat_player_state_for(dead_ally)
        assert living_state is not None
        assert dead_state is not None
        drawn = [make_strike_ironclad(), make_defend_ironclad(), make_strike_ironclad()]
        living_state.draw = list(drawn)
        living_state.zone_map["draw"] = living_state.draw
        dead_draw = [make_defend_ironclad()]
        dead_state.draw = list(dead_draw)
        dead_state.zone_map["draw"] = dead_state.draw
        combat.hand = [make_huddle_up(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)

        assert living_state.hand == drawn
        assert living_state.draw == []
        assert dead_state.hand == []
        assert dead_state.draw == dead_draw
        # The caster is its own teammate (C# GetTeammatesOf includes the
        # creature itself), so the primary player draws 3 upgraded cards too.
        assert len(combat.hand) == 3

    def test_intercept_grants_block_and_covered_to_target_ally(self):
        combat = _make_combat()
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None

        card = make_intercept_card()
        card.owner = ally
        ally_state.hand = [card]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1

        assert combat.play_card_from_creature(ally, 0, 0)
        assert ally.block == 9
        assert combat.primary_player.block == 0
        assert combat.primary_player.get_power_amount(PowerId.COVERED) == 1
        intercept = ally.powers.get(PowerId.INTERCEPT)
        assert intercept is not None
        assert getattr(intercept, "_covered_creatures", []) == [combat.primary_player]

        enemy = combat.enemies[0]
        hp_before = combat.primary_player.current_hp
        combat.primary_player.block = 0
        combat.deal_damage(enemy, combat.primary_player, 10, ValueProp.MOVE)
        assert combat.primary_player.current_hp == hp_before

    def test_production_base_and_upgraded_both_exhaust(self):
        combat = _make_combat()
        base = make_production()
        upgraded = make_production(upgraded=True)
        combat.hand = [base, upgraded]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.energy == 2
        assert any(card.card_id == base.card_id for card in combat.exhaust_pile)

        # Production.cs keeps Exhaust on upgrade: CanonicalKeywords is
        # [CardKeyword.Exhaust] and OnUpgrade (lines 29-32) only bumps Energy
        # by 1 (EnergyVar(2) -> 3); the old premise that the upgrade removed
        # Exhaust no longer holds in v0.107.1.
        assert combat.play_card(0)
        assert combat.energy == 5
        assert any(card.card_id == upgraded.card_id for card in combat.exhaust_pile)

    def test_prolong_carries_current_block_to_next_player_turn(self):
        combat = _make_combat()
        combat.player.gain_block(11)
        combat.hand = [make_prolong()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.BLOCK_NEXT_TURN) == 11

        combat.end_player_turn()
        assert combat.player.block == 11

    def test_rend_scales_damage_with_non_duration_debuff_count(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 200
        enemy.current_hp = 200
        combat.apply_power_to(enemy, PowerId.WEAK, 1)
        combat.apply_power_to(enemy, PowerId.FRAIL, 1)
        combat.hand = [make_rend()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 175

    def test_rend_counts_negative_strength_but_ignores_temporary_debuffs(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 200
        enemy.current_hp = 200
        combat.apply_power_to(enemy, PowerId.STRENGTH, -2, applier=combat.player)
        combat.apply_power_to(enemy, PowerId.DARK_SHACKLES, 9, applier=combat.player)
        combat.hand = [make_rend()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 180

    def test_rend_uses_normal_attack_damage_modifiers(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 200
        enemy.current_hp = 200
        combat.player.apply_power(PowerId.STRENGTH, 3)
        combat.hand = [make_rend()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 182

    def test_gold_axe_scales_with_all_players_finished_card_plays(self):
        # GoldAxe.cs CanonicalVars (lines 20-21): the multiplier is
        # CombatManager.Instance.History.CardPlaysFinished.Count() — the
        # combat-wide history covering every player's finished plays, so the
        # ally's strike counts too (multiplayer semantics; single-player
        # behavior is unchanged because the only player is the owner).
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None

        ally_strike = make_strike_ironclad()
        ally_strike.owner = ally
        ally_state.hand = [ally_strike]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1

        combat.hand = [make_strike_ironclad(), make_strike_ironclad(), make_gold_axe()]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert combat.play_card(0, 0)
        assert combat.play_card_from_creature(ally, 0, 0)
        before_gold_axe = enemy.current_hp

        # 2 owner strikes + 1 ally strike finished before Gold Axe resolves;
        # Gold Axe's own in-flight play is not yet in History (C# appends
        # CardPlayFinishedEntry after the effect resolves).
        assert combat.play_card(0, 0)
        assert enemy.current_hp == before_gold_axe - 3
        assert make_gold_axe(upgraded=True).is_retain

    def test_fisticuffs_block_uses_normal_block_modifiers(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        combat.apply_power_to(combat.player, PowerId.DEXTERITY, 3)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 1)
        combat.hand = [make_fisticuffs()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 93
        assert combat.player.block == 7

    def test_fisticuffs_does_not_gain_block_after_combat_ending(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 7
        combat.hand = [make_fisticuffs()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.is_over
        assert combat.player.block == 0

    def test_beacon_of_hope_triggers_from_colorless_block_cards(self):
        combat = _make_combat()
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        combat.hand = [make_beacon_of_hope(), make_finesse()]
        combat.draw_pile = []
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.player.block == 4
        assert ally.block == 2

    def test_beacon_of_hope_triggers_from_fisticuffs_block(self):
        combat = _make_combat()
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        combat.hand = [make_beacon_of_hope(), make_fisticuffs()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.play_card(0, 0)
        assert enemy.current_hp == 93
        assert combat.player.block == 7
        assert ally.block == 3

    def test_coordinate_applies_temporary_strength_to_target_ally_then_expires(self):
        combat = _make_combat()
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_state = combat.combat_player_state_for(ally)
        assert ally_state is not None

        card = make_coordinate_card(upgraded=True)
        card.owner = ally
        ally_state.hand = [card]
        ally_state.zone_map["hand"] = ally_state.hand
        ally_state.energy = 1

        assert combat.play_card_from_creature(ally, 0, 0)
        assert combat.primary_player.get_power_amount(PowerId.COORDINATE) == 8
        assert combat.primary_player.get_power_amount(PowerId.STRENGTH) == 8

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert combat.primary_player.get_power_amount(PowerId.COORDINATE) == 0
        assert combat.primary_player.get_power_amount(PowerId.STRENGTH) == 0

    def test_knockdown_damage_and_multiplier_match_reference_values(self):
        """Matches Knockdown.cs: damage first, then apply KnockdownPower."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_knockdown(), make_knockdown(upgraded=True)]
        combat.energy = 6

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 100 - KNOCKDOWN_BASE_DAMAGE
        assert enemy.get_power_amount(PowerId.KNOCKDOWN) == KNOCKDOWN_BASE_MULTIPLIER

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 100 - KNOCKDOWN_BASE_DAMAGE - KNOCKDOWN_PLUS_DAMAGE
        assert enemy.get_power_amount(PowerId.KNOCKDOWN) == KNOCKDOWN_PLUS_MULTIPLIER

    def test_hidden_gem_does_not_apply_replay_to_curse_or_quest_cards(self):
        combat = _make_combat()
        curse = CardInstance(
            card_id=CardId.CURSE_OF_THE_BELL,
            cost=0,
            card_type=CardType.CURSE,
            target_type=TargetType.NONE,
            rarity=CardRarity.CURSE,
        )
        quest = CardInstance(
            card_id=CardId.SPOILS_MAP,
            cost=0,
            card_type=CardType.QUEST,
            target_type=TargetType.NONE,
            rarity=CardRarity.QUEST,
        )
        combat.hand = [make_hidden_gem()]
        combat.draw_pile = [curse, quest]
        combat.energy = 1

        assert combat.play_card(0)
        assert curse.base_replay_count == 0
        assert quest.base_replay_count == 0

    def test_hidden_gem_prefers_attack_skill_power_draw_cards_over_status_fallback(self):
        """Matches HiddenGem.cs: prefer playable Attack / Skill / Power cards when available."""
        combat = _make_combat()
        status = CardInstance(
            card_id=CardId.WOUND,
            cost=-1,
            card_type=CardType.STATUS,
            target_type=TargetType.NONE,
            rarity=CardRarity.STATUS,
            keywords=frozenset({"unplayable"}),
        )
        skill = make_defend_ironclad()
        combat.hand = [make_hidden_gem()]
        combat.draw_pile = [status, skill]
        combat.energy = 1

        assert combat.play_card(0)

        assert skill.base_replay_count == 2
        assert status.base_replay_count == 0

    def test_gang_up_counts_only_other_allied_powered_hits_on_same_target_this_turn(self):
        """Matches GangUp.cs: multiplier filters by receiver, powered attack, dealer side, and dealer identity."""
        combat = _make_combat()
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        other_enemy, other_ai = create_shrinker_beetle(Rng(4243))
        combat.add_enemy(other_enemy, other_ai)
        target = combat.enemies[0]
        target.current_hp = target.max_hp = 100
        combat.record_damage_event(ally, target, ValueProp.MOVE, 1)
        combat.record_damage_event(combat.player, target, ValueProp.MOVE, 1)
        combat.record_damage_event(ally, other_enemy, ValueProp.MOVE, 1)
        combat.record_damage_event(combat.enemies[1], target, ValueProp.MOVE, 1)
        combat.record_damage_event(ally, target, ValueProp.MOVE | ValueProp.UNPOWERED, 1)
        combat.hand = [make_gang_up()]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert target.current_hp == 90

    def test_rolling_boulder_deals_unpowered_start_turn_damage_then_increments(self):
        """Matches RollingBoulder.cs: start-turn unpowered damage increases by 5 each trigger."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_rolling_boulder()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ROLLING_BOULDER) == ROLLING_BOULDER_BASE_DAMAGE

        combat.end_player_turn()

        assert enemy.current_hp == 100 - ROLLING_BOULDER_BASE_DAMAGE
        assert combat.player.get_power_amount(PowerId.ROLLING_BOULDER) == (
            ROLLING_BOULDER_BASE_DAMAGE + ROLLING_BOULDER_INCREMENT
        )

    def test_rolling_boulder_plus_starts_at_ten_damage(self):
        """Matches RollingBoulder.cs: upgraded RollingBoulderPower starts at 10."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_rolling_boulder(upgraded=True)]
        combat.energy = 3

        assert combat.play_card(0)
        combat.end_player_turn()

        assert enemy.current_hp == 100 - ROLLING_BOULDER_PLUS_DAMAGE
        assert combat.player.get_power_amount(PowerId.ROLLING_BOULDER) == (
            ROLLING_BOULDER_PLUS_DAMAGE + ROLLING_BOULDER_INCREMENT
        )

    def test_scrawl_plus_retains_and_draws_until_hand_is_full(self):
        """Matches Scrawl.cs: upgraded Scrawl adds Retain and draws to the 10-card hand limit."""
        combat = _make_combat()
        kept = [make_defend_ironclad() for _ in range(8)]
        drawn = [make_strike_ironclad(), make_strike_ironclad(), make_strike_ironclad()]
        combat.hand = [make_scrawl(upgraded=True), *kept]
        combat.draw_pile = list(drawn)
        combat.energy = 1

        assert combat.play_card(0)

        assert make_scrawl(upgraded=True).is_retain
        assert combat.hand == [*kept, drawn[0], drawn[1]]
        assert combat.draw_pile == [drawn[2]]


def test_hidden_gem_grants_replay_to_eligible_draw_pile_card():
    """HiddenGem.cs OnPlay (lines 35-45): pick a random draw-pile card that is
    not Unplayable, not Curse/Quest (CardType 5/6), and has
    GetEnchantedReplayCount() < 1; grant it BaseReplayCount += Replay(2)."""
    combat = _make_combat()
    strike = make_strike_ironclad()
    decay = make_decay()
    combat.hand = [make_hidden_gem()]
    combat.draw_pile = [decay, strike]
    combat.energy = 1

    assert combat.play_card(0)

    # Curse (CardType.CURSE) is filtered out; the strike is the only eligible
    # candidate and receives +2 base replay.
    assert strike.base_replay_count == 2
    assert decay.base_replay_count == 0


def test_hidden_gem_prefers_attack_skill_power_over_other_types():
    """HiddenGem.cs lines 40-43: cards of CardType 1-3 (Attack/Skill/Power) are
    preferred; Status/others only serve as fallback when no preferred card
    exists."""
    combat = _make_combat()
    strike = make_strike_ironclad()
    slimed = make_slimed()
    combat.hand = [make_hidden_gem()]
    combat.draw_pile = [slimed, strike]
    combat.energy = 1

    assert combat.play_card(0)

    assert strike.base_replay_count == 2
    assert slimed.base_replay_count == 0


def test_hidden_gem_skips_cards_that_already_have_replay():
    """HiddenGem.cs filter `GetEnchantedReplayCount() < 1`: cards already
    carrying replay (>= 1) are ineligible; with no other candidates the play
    is a no-op."""
    combat = _make_combat()
    strike = make_strike_ironclad()
    strike.base_replay_count = 1
    decay = make_decay()
    combat.hand = [make_hidden_gem()]
    combat.draw_pile = [strike, decay]
    combat.energy = 1

    assert combat.play_card(0)

    assert strike.base_replay_count == 1
    assert decay.base_replay_count == 0


def test_hidden_gem_upgraded_grants_three_replay():
    """HiddenGem.cs OnUpgrade: Replay.UpgradeValueBy(1) -> 3 when upgraded."""
    combat = _make_combat()
    strike = make_strike_ironclad()
    combat.hand = [make_hidden_gem(upgraded=True)]
    combat.draw_pile = [strike]
    combat.energy = 1

    assert combat.play_card(0)

    assert strike.base_replay_count == 3
