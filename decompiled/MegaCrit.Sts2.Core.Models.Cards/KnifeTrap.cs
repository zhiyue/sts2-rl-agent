using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class KnifeTrap : CardModel
{
	private const string _calculatedShivsKey = "CalculatedShivs";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Shiv>(base.IsUpgraded));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("CalculatedShivs").WithMultiplier((CardModel card, Creature? _) => PileType.Exhaust.GetPile(card.Owner).Cards.Count((CardModel c) => c.Tags.Contains(CardTag.Shiv)))
	});

	public KnifeTrap()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		IEnumerable<CardModel> enumerable = PileType.Exhaust.GetPile(base.Owner).Cards.Where((CardModel c) => c.Tags.Contains(CardTag.Shiv)).ToList();
		bool flag = true;
		foreach (CardModel item in enumerable)
		{
			if (base.IsUpgraded)
			{
				CardCmd.Upgrade(item, CardPreviewStyle.None);
			}
			await CardCmd.AutoPlay(choiceContext, item, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !flag);
			flag = false;
		}
	}
}
