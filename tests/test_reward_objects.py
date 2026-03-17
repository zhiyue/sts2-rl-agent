"""Tests for reward objects and rewards-set assembly."""

from sts2_env.core.enums import RoomType
from sts2_env.run.reward_objects import CardReward, RewardsSet
from sts2_env.run.rooms import CombatRoom
from sts2_env.run.run_state import RunState


def test_rewards_set_merges_combat_room_extra_rewards_for_player():
    run_state = RunState(seed=42, character_id="Ironclad")
    room = CombatRoom(room_type=RoomType.MONSTER)
    extra = CardReward(run_state.player.player_id, context="regular")
    room.add_extra_reward(run_state.player.player_id, extra)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
    generated = rewards.generate_without_offering(run_state)

    assert any(reward is extra for reward in generated)
