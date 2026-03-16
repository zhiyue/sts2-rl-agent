using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public class CardPlay
{
	public required CardModel Card { get; init; }

	public required Creature? Target { get; init; }

	public required PileType ResultPile { get; init; }

	public required ResourceInfo Resources { get; init; }

	public required bool IsAutoPlay { get; init; }

	public required int PlayIndex { get; init; }

	public required int PlayCount { get; init; }

	public bool IsFirstInSeries => PlayIndex == 0;

	public bool IsLastInSeries => PlayIndex == PlayCount - 1;
}
