using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Catastrophe : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public Catastrophe()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
		{
			CardModel cardModel = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => !c.Keywords.Contains(CardKeyword.Unplayable)).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
				.FirstOrDefault();
			if (cardModel == null)
			{
				cardModel = PileType.Draw.GetPile(base.Owner).Cards.ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle).FirstOrDefault();
			}
			if (cardModel != null)
			{
				await CardCmd.AutoPlay(choiceContext, cardModel, null);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
