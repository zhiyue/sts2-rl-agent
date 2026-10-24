using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// An enum representing the types of sources that should be included when calculating a card's keywords.
/// This mirrors <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.CostModifiers" />; see that type for the analogous local-vs-global distinction on energy cost.
/// </summary>
[Flags]
public enum KeywordSources
{
	/// <summary>
	/// No sources at all. This returns an empty set.
	/// </summary>
	None = 0,
	/// <summary>
	/// Include keywords that have been applied directly to the card.
	/// These live locally on the card itself (canonical keywords plus local <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.AddKeyword(MegaCrit.Sts2.Core.Entities.Cards.CardKeyword)" /> minus
	/// local <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.RemoveKeyword(MegaCrit.Sts2.Core.Entities.Cards.CardKeyword)" />), and persist regardless of changes to other models in the combat
	/// state. <see cref="T:MegaCrit.Sts2.Core.Models.Relics.MusicBox" /> applies Ethereal this way.
	/// </summary>
	Local = 2,
	/// <summary>
	/// Include keywords that are persistently granted by other models in the combat state, and that wear off when that
	/// source changes or leaves. These are never stored on the card; they are computed on demand.
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Powers.HexPower" /> grants Ethereal this way for as long as it exists.
	/// </summary>
	Global = 4,
	/// <summary>
	/// Include all sources.
	/// </summary>
	All = -1
}
