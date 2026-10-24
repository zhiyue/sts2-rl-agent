using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
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
		int count = CardPile.MaxCardsInHand - PileType.Hand.GetPile(base.Owner).Cards.Count;
		List<CardModel> cards = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Rarity == CardRarity.Rare).TakeRandom(count, base.Owner.RunState.Rng.CombatCardSelection).ToList();
		await CardPileCmd.Add(cards, PileType.Hand);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Retain);
	}
}
