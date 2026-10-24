using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BelieveInYou : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(base.EnergyHoverTip);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(2));

	public BelieveInYou()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, cardPlay.Target.Player);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
