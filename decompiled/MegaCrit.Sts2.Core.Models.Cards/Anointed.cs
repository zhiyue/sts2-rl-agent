using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Anointed : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public Anointed()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<CardModel> cards = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Rarity == CardRarity.Rare).ToList();
		await CardPileCmd.Add(cards, PileType.Hand);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Retain);
	}
}
