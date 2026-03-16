using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Rooms;

public class TreasureRoom : AbstractRoom
{
	private Player? _player;

	public override RoomType RoomType => RoomType.Treasure;

	public override ModelId? ModelId => null;

	public TreasureRoom(int actIndex)
	{
		if ((actIndex < 0 || actIndex > 2) ? true : false)
		{
			throw new ArgumentOutOfRangeException("actIndex", "must be between 0 and 2");
		}
	}

	public override async Task Enter(IRunState? runState, bool isRestoringRoomStackBase)
	{
		if (isRestoringRoomStackBase)
		{
			throw new InvalidOperationException("TreasureRoom does not support room stack reconstruction.");
		}
		_player = LocalContext.GetMe(runState);
		if (runState != null)
		{
			await PreloadManager.LoadRoomTreasureAssets(runState.Act);
			NRun.Instance?.SetCurrentRoom(NTreasureRoom.Create(this, runState));
			await Hook.AfterRoomEntered(runState, this);
		}
		RunManager.Instance.TreasureRoomRelicSynchronizer.BeginRelicPicking();
	}

	public override Task Exit(IRunState? runState)
	{
		return Task.CompletedTask;
	}

	public override Task Resume(AbstractRoom _, IRunState? runState)
	{
		throw new NotImplementedException();
	}

	public Task<int> DoNormalRewards()
	{
		return RunManager.Instance.OneOffSynchronizer.DoLocalTreasureRoomRewards();
	}

	public Task DoExtraRewardsIfNeeded()
	{
		return RewardsCmd.OfferForRoomEnd(_player, this);
	}
}
