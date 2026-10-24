using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

/// <summary>
/// Placeholder event if we can't find the card we are looking for (such as in the case that it's been deprecated)
/// </summary>
public sealed class DeprecatedCard : CardModel
{
	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	/// <summary>
	/// Placeholder event if we can't find the card we are looking for (such as in the case that it's been deprecated)
	/// </summary>
	public DeprecatedCard()
		: base(0, CardType.Status, CardRarity.Status, TargetType.None, shouldShowInCardLibrary: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, 1m, base.Owner);
		if (base.DeckVersion != null)
		{
			await CardPileCmd.RemoveFromDeck(base.DeckVersion);
			base.DeckVersion = null;
		}
	}
}
