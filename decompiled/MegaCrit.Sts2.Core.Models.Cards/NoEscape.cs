using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class NoEscape : CardModel
{
	private const string _calculatedDoomKey = "CalculatedDoom";

	private const string _doomThresholdKey = "DoomThreshold";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new DynamicVar("DoomThreshold", 10m),
		new CalculationBaseVar(10m),
		new CalculationExtraVar(5m),
		new CalculatedVar("CalculatedDoom").WithMultiplier(delegate(CardModel card, Creature? target)
		{
			int num = target?.GetPowerAmount<DoomPower>() ?? 0;
			decimal baseValue = card.DynamicVars["DoomThreshold"].BaseValue;
			return Math.Floor((decimal)num / baseValue);
		})
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	public NoEscape()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await PowerCmd.Apply<DoomPower>(cardPlay.Target, ((CalculatedVar)base.DynamicVars["CalculatedDoom"]).Calculate(cardPlay.Target), base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.CalculationBase.UpgradeValueBy(5m);
	}
}
