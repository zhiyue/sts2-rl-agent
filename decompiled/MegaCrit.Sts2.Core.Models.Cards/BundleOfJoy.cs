using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BundleOfJoy : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public BundleOfJoy()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(base.Owner, ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(base.Owner.UnlockState, base.RunState.CardMultiplayerConstraint), base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatCardGeneration);
		foreach (CardModel item in distinctForCombat)
		{
			await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Hand, addedByPlayer: true);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
