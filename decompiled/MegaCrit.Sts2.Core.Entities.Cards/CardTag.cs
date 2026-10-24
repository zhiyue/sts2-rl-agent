namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Card Tags are behavior-less metadata that other models can use to look things up.
///
/// A card tag should NEVER automatically add behavior to a specific card, which means non-model areas of the codebase
/// should probably never reference individual ones. If you're here to try and add automatic behavior to a card, check
/// out CardKeyword instead.
///
/// The proper way to use Card Tags is for other models to look them up to power their own behavior.
///
/// For example, Perfected Strike ("Deal 6 damage. Deals 2 additional damage for ALL your cards containing 'Strike'.")
/// can look up all cards with the Strike keyword to determine how much extra damage to deal.
/// </summary>
public enum CardTag
{
	None,
	/// <summary>
	/// Makes this card a "Strike" card (<see cref="T:MegaCrit.Sts2.Core.Models.Cards.StrikeIronclad" />, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PommelStrike" />, etc).
	///
	/// Used by effects that apply to Strikes, generally in tandem with a Basic rarity check (like
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PandorasBox" />), but also sometimes without (like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" /> and
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Hellraiser" />).
	/// </summary>
	Strike,
	/// <summary>
	/// Makes this card a "Defend" card (<see cref="T:MegaCrit.Sts2.Core.Models.Cards.DefendIronclad" />, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.DefendSilent" />, etc).
	///
	/// Used by effects that apply to Defends, generally in tandem with a Basic rarity check (like
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PandorasBox" />), but also sometimes without (like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Fasten" />).
	/// </summary>
	Defend,
	/// <summary>
	/// Makes this card a "Minion" card (<see cref="T:MegaCrit.Sts2.Core.Models.Cards.MinionDiveBomb" />, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.MinionSacrifice" />, etc).
	///
	/// Used by effects that apply to Minion cards, like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.VitruvianMinion" />.
	/// </summary>
	Minion,
	/// <summary>
	/// Makes this card an "Osty Attack" card (<see cref="T:MegaCrit.Sts2.Core.Models.Cards.Poke" />, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Flatten" />, etc).
	/// </summary>
	OstyAttack,
	/// <summary>
	/// Makes this card a "Shiv" card (<see cref="F:MegaCrit.Sts2.Core.Entities.Cards.CardTag.Shiv" />).
	/// </summary>
	Shiv
}
