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
		CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
		List<CardModel> cards = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Type == CardType.Skill).ToList();
		CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, base.Owner, prefs)).FirstOrDefault();
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
