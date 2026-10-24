using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

[Flags]
public enum UnplayableReason
{
	None = 0,
	/// <summary>
	/// The card has the Unplayable keyword, or its UnplayableThisTurn flag is set to true.
	/// </summary>
	HasUnplayableKeyword = 2,
	/// <summary>
	/// Something like Normality is blocking the card from being played.
	/// </summary>
	BlockedByHook = 4,
	/// <summary>
	/// The card itself has a built-in reason that is blocking its play, like how Grand Finale can't be played unless
	/// your draw pile is empty.
	/// </summary>
	BlockedByCardLogic = 8,
	/// <summary>
	/// You don't have enough energy to play the card.
	/// </summary>
	EnergyCostTooHigh = 0x10,
	/// <summary>
	/// You don't have enough stars to play the card.
	/// </summary>
	StarCostTooHigh = 0x20,
	/// <summary>
	/// You have no living allies you can target.
	/// </summary>
	NoLivingAllies = 0x40
}
