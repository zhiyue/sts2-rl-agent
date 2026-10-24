using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// How long an <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier" /> should last.
/// </summary>
[Flags]
public enum LocalCostModifierExpiration
{
	/// <summary>
	/// This modifier should wear off when combat ends.
	/// Used for effects like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Enlightenment" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Cards.KinglyKick" />.
	/// </summary>
	EndOfCombat = 0,
	/// <summary>
	/// This modifier should wear off when the turn ends.
	/// Used for effects like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.BulletTime" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Pinpoint" />.
	/// </summary>
	EndOfTurn = 2,
	/// <summary>
	/// This modifier should wear off when the card is played.
	/// Used for effects like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Eidolon" />.
	/// </summary>
	WhenPlayed = 4
}
