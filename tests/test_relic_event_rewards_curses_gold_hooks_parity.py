"""Focused parity tests for additional event/uncommon relic hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.necrobinder import create_necrobinder_starter_deck, make_end_of_days
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CombatSide, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.potions.base import create_potion
from sts2_env.run.reward_objects import AddCardsReward, RelicReward, UpgradeCardsReward
from sts2_env.run.run_state import RunState


class _FailingSampleRng:
    def sample(self, values, count):
        raise AssertionError("wrong RNG stream")


class _FirstSampleRng:
    def sample(self, values, count):
        return list(values)[:count]


def _make_necrobinder_combat(relics: list[str] | None = None, *, seed: int = 771) -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=create_necrobinder_starter_deck(),
        rng_seed=seed,
        character_id="Necrobinder",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicEventRewardsCursesGoldHooksParity:
    def test_book_repair_knife_heals_three_per_enemy_that_dies_to_doom(self):
        combat = _make_necrobinder_combat(["BookRepairKnife"], seed=771)
        enemy = combat.enemies[0]
        enemy.current_hp = 20
        enemy.max_hp = 20
        combat.player.current_hp = 40
        combat.hand = [make_end_of_days()]
        combat.energy = 3

        assert combat.play_card(0)
        assert enemy.is_dead
        assert combat.player.current_hp == 43

    def test_reptile_trinket_grants_temporary_strength_after_potion_use(self):
        combat = _make_necrobinder_combat(["ReptileTrinket"], seed=772)
        combat.potions = [create_potion("FirePotion"), None, None]

        assert combat.use_potion(0, target_index=0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 3

        combat.end_player_turn()
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 0

    def test_sand_castle_queues_upgrade_reward_when_followups_are_deferred(self):
        run_state = RunState(seed=773, character_id="Ironclad")
        run_state.defer_followup_rewards = True

        assert run_state.player.obtain_relic("SAND_CASTLE")
        assert len(run_state.pending_rewards) == 1
        reward = run_state.pending_rewards[0]
        assert isinstance(reward, UpgradeCardsReward)
        assert reward.count == 6

    def test_seal_of_gold_spends_three_gold_for_one_energy_each_player_turn(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=774,
            character_id="Ironclad",
            relics=["SealOfGold"],
            gold=20,
        )
        creature, ai = create_shrinker_beetle(Rng(774))
        combat.add_enemy(creature, ai)
        combat.start_combat()

        assert combat.gold == 17
        assert combat.energy == 4

        combat.end_player_turn()
        assert combat.gold == 14
        assert combat.energy == 4

    def test_distinguished_cape_adds_random_curses_and_apparitions_without_hp_loss(self):
        run_state = RunState(seed=777, character_id="Ironclad")
        run_state.rng.niche = _FirstSampleRng()
        start_max_hp = run_state.player.max_hp
        starting_deck_size = len(run_state.player.deck)

        assert run_state.player.obtain_relic("DISTINGUISHED_CAPE")

        assert run_state.player.max_hp == start_max_hp
        added_cards = [card.card_id.name for card in run_state.player.deck[starting_deck_size:]]
        assert added_cards.count("APPARITION") == 3
        assert len([name for name in added_cards if name != "APPARITION"]) == 2

    def test_sere_talon_loses_nine_max_hp_and_queues_only_three_wishes(self):
        run_state = RunState(seed=775, character_id="Ironclad")
        run_state.defer_followup_rewards = True
        run_state.rng.niche = _FailingSampleRng()
        start_max_hp = run_state.player.max_hp

        assert run_state.player.obtain_relic("SERE_TALON")
        assert run_state.player.max_hp == start_max_hp - 9
        assert len(run_state.pending_rewards) == 1
        assert isinstance(run_state.pending_rewards[0], AddCardsReward)
        added_cards = [card.card_id.name for card in run_state.pending_rewards[0].cards]
        assert added_cards.count("WISH") == 3
        assert len(added_cards) == 3

    def test_sere_talon_immediate_adds_only_three_wishes_after_max_hp_loss(self):
        run_state = RunState(seed=775, character_id="Ironclad")
        run_state.rng.niche = _FailingSampleRng()
        start_max_hp = run_state.player.max_hp
        deck_size_before = len(run_state.player.deck)

        assert run_state.player.obtain_relic("SERE_TALON")

        assert run_state.player.max_hp == start_max_hp - 9
        added_cards = [card.card_id.name for card in run_state.player.deck[deck_size_before:]]
        assert added_cards.count("WISH") == 3
        assert len(added_cards) == 3

    def test_small_capsule_queues_one_relic_reward(self):
        run_state = RunState(seed=776, character_id="Ironclad")

        assert run_state.player.obtain_relic("SMALL_CAPSULE")
        rewards = [reward for reward in run_state.pending_rewards if isinstance(reward, RelicReward)]
        assert len(rewards) == 1
