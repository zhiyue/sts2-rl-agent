using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Parry : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromCard<SovereignBlade>(),
		HoverTipFactory.Static(StaticHoverTip.Block)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<ParryPower>(10m));

	public Parry()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<ParryPower>(choiceContext, base.Owner.Creature, base.DynamicVars["ParryPower"].BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["ParryPower"].UpgradeValueBy(4m);
	}
}
