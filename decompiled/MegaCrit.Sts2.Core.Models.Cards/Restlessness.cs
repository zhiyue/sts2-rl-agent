using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Restlessness : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(2),
		new EnergyVar(2)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(base.EnergyHoverTip);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Retain);

	protected override bool ShouldGlowGoldInternal => IsOnlyCardInHand;

	private bool IsOnlyCardInHand => !PileType.Hand.GetPile(base.Owner).Cards.Except(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(this)).Any();

	public Restlessness()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (IsOnlyCardInHand)
		{
			for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
			{
				await CardPileCmd.Draw(choiceContext, base.Owner);
			}
			await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
		base.DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
