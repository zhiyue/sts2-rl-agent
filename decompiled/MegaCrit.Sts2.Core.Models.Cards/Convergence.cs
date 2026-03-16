using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Convergence : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new EnergyVar(1),
		new StarsVar(1)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		base.EnergyHoverTip,
		HoverTipFactory.FromKeyword(CardKeyword.Retain)
	});

	public Convergence()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await PowerCmd.Apply<RetainHandPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		await PowerCmd.Apply<EnergyNextTurnPower>(base.Owner.Creature, base.DynamicVars.Energy.BaseValue, base.Owner.Creature, this);
		await PowerCmd.Apply<StarNextTurnPower>(base.Owner.Creature, base.DynamicVars.Stars.BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Stars.UpgradeValueBy(1m);
	}
}
