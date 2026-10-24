using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class SecretTechnique : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public SecretTechnique()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel cardModel = (await CardSelectCmd.FromCombatPile(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1), context: choiceContext, pile: PileType.Draw.GetPile(base.Owner), player: base.Owner, filter: (CardModel c) => c.Type == CardType.Skill)).FirstOrDefault();
		if (cardModel != null)
		{
			await CardPileCmd.Add(cardModel, PileType.Hand);
		}
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
