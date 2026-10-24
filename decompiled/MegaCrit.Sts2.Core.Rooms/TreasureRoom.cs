using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Rooms;

public class TreasureRoom : AbstractRoom
{
	private IRunState? _runState;

	public override RoomType RoomType => RoomType.Treasure;

	public override ModelId? ModelId => null;

	public TreasureRoom(int actIndex)
	{
		if ((actIndex < 0 || actIndex > 2) ? true : false)
		{
			throw new ArgumentOutOfRangeException("actIndex", "must be between 0 and 2");
		}
	}

	public override async Task EnterInternal(IRunState? runState, bool isRestoringRoomStackBase)
	{
		if (isRestoringRoomStackBase)
		{
			throw new InvalidOperationException("TreasureRoom does not support room stack reconstruction.");
		}
		if (runState != null)
		{
			await PreloadManager.LoadRoomTreasureAssets(runState.Act);
			NRun.Instance?.SetCurrentRoom(NTreasureRoom.Create(this, runState));
			await Hook.AfterRoomEntered(runState, this);
			_runState = runState;
		}
		RunManager.Instance.TreasureRoomRelicSynchronizer.BeginRelicPicking();
	}

	public override Task Exit(IRunState? runState)
	{
		RunManager.Instance.TreasureRoomRelicSynchronizer.OnRoomExited();
		return Task.CompletedTask;
	}

	public override Task Resume(AbstractRoom _, IRunState? runState)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Offer the normal treasure room rewards (gold, relics, etc.)
	/// </summary>
	/// <returns>The amount of gold given to the player.</returns>
	public Task<int> DoNormalRewards()
	{
		return RunManager.Instance.OneOffSynchronizer.DoLocalTreasureRoomRewards();
	}

	/// <summary>
	/// Offer extra rewards.
	/// Normally this does nothing but if extra rewards were added by a relic then this will show the rewards screen.
	/// Note that this only awaits the local player's reward task, and not everyone's.
	/// </summary>
	public async Task DoExtraRewardsIfNeeded()
	{
		Task localTask = null;
		List<RewardsSet> rewards = new List<RewardsSet>();
		foreach (Player player in _runState.Players)
		{
			List<RewardsSet> list = rewards;
			list.Add(await RewardsCmd.GenerateForRoomEnd(player, this));
		}
		foreach (RewardsSet item in rewards)
		{
			Task task = TaskHelper.RunSafely(item.Offer());
			if (LocalContext.IsMe(item.Player))
			{
				localTask = task;
			}
		}
		if (localTask == null)
		{
			throw new InvalidOperationException("Tried to do extra rewards, but the local player is not in the run state!");
		}
		await localTask;
	}
}
