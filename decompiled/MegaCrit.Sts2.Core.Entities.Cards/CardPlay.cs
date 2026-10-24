using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Represents a single instance of a card being played.
/// This is important for distinguishing between multiple plays of the same card during the same turn.
/// </summary>
public class CardPlay
{
	/// <summary>
	/// The card that was played.
	/// </summary>
	public required CardModel Card { get; init; }

	/// <summary>
	/// The creature that was targeted by the card.
	/// Null for un-targeted cards.
	/// </summary>
	public required Creature? Target { get; init; }

	/// <summary>
	/// The pile that the card should be put in after this play is over.
	/// </summary>
	public required PileType ResultPile { get; init; }

	/// <summary>
	/// Info about the resources used during this card play.
	/// </summary>
	public required ResourceInfo Resources { get; init; }

	/// <summary>
	/// Whether this card is being auto-played.
	/// False when the player plays the card manually from their hand.
	/// True when played automatically by an effect like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.MayhemPower" />.
	/// </summary>
	public required bool IsAutoPlay { get; init; }

	/// <summary>
	/// The index of the current card play relative to playCount. Usually 0, but when a card has an effect like Replay,
	/// the second play will have playIndex=1, the third will have playIndex=2, etc.
	/// </summary>
	public required int PlayIndex { get; init; }

	/// <summary>
	/// The total number of times this card is going to be played. Usually 1, but when a card has an effect like Replay,
	/// it will be higher (Replay 1 will have playCount=2, Replay 2 will have playCount=3, etc.).
	/// You can compare this with playCount to see if you're on the final play of the card.
	/// </summary>
	public required int PlayCount { get; init; }

	/// <summary>
	/// Is this the first card play in the series?
	/// Usually true, but when a card has an effect like Replay, this will only be true on the card's first play.
	/// For example, if a card has Replay 2, this will be true for its first CardPlay and false for the last two.
	/// </summary>
	public bool IsFirstInSeries => PlayIndex == 0;

	/// <summary>
	/// Is this the last card play in the series?
	/// Usually true, but when a card has an effect like Replay, this will only be true on the card's last play.
	/// For example, if a card has Replay 2, this will be false for its first two CardPlays and true for the third.
	/// </summary>
	public bool IsLastInSeries => PlayIndex == PlayCount - 1;
}
