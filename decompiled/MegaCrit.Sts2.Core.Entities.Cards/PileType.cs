namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Used for determining which Card Pile. Useful when flying cards around, knowing if a card is in your Hand or
/// in the Discard Pile and things like that.
/// </summary>
public enum PileType
{
	None,
	/// <summary>
	/// Where cards are drawn from.
	/// </summary>
	Draw,
	/// <summary>
	/// Where cards are manually played from.
	/// </summary>
	Hand,
	/// <summary>
	/// Where cards are discarded/flushed to.
	/// </summary>
	Discard,
	/// <summary>
	/// Where cards are exhausted to.
	/// </summary>
	Exhaust,
	/// <summary>
	/// A temporary pile that a card lives in while it's mid-play, so that it isn't counted towards your hand or your
	/// discard pile.
	/// </summary>
	Play,
	/// <summary>
	/// Where cards live between rooms.
	/// When a new combat starts, all cards in here are cloned into your draw pile, so modifications to cards in combat
	/// won't modify cards in here.
	/// </summary>
	Deck
}
