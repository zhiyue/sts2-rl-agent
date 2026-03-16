using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class DeadlyEvents : ModifierModel
{
	protected override void AfterRunCreated(RunState runState)
	{
		foreach (Player player in runState.Players)
		{
			player.RelicGrabBag.Remove<JuzuBracelet>();
		}
		runState.SharedRelicGrabBag.Remove<JuzuBracelet>();
		runState.Odds.UnknownMapPoint.EliteOdds = 0.1f;
		runState.Odds.UnknownMapPoint.SetBaseOdds(RoomType.Elite, 0.1f);
	}

	protected override void AfterRunLoaded(RunState runState)
	{
		runState.Odds.UnknownMapPoint.SetBaseOdds(RoomType.Elite, 0.1f);
	}

	public override float ModifyOddsIncreaseForUnrolledRoomType(RoomType roomType, float oddsIncrease)
	{
		if (roomType != RoomType.Treasure)
		{
			return oddsIncrease;
		}
		return oddsIncrease * 2f;
	}
}
