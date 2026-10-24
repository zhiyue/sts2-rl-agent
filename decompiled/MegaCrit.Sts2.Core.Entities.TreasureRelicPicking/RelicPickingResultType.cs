namespace MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;

public enum RelicPickingResultType
{
	/// <summary>
	/// A single player voted for the relic, and that player received the relic.
	/// </summary>
	OnlyOnePlayerVoted,
	/// <summary>
	/// Multiple players voted for the relic. The details of the fight are in the fight member.
	/// </summary>
	FoughtOver,
	/// <summary>
	/// No player voted for the relic. The player that received it was a loser of a fight.
	/// </summary>
	ConsolationPrize,
	/// <summary>
	/// No player voted for the relic, and no player got the relic because they skipped over it.
	/// </summary>
	Skipped
}
