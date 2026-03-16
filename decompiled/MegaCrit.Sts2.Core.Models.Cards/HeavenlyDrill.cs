using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class HeavenlyDrill : CardModel
{
	protected override bool HasEnergyCostX => true;

	protected override bool ShouldGlowGoldInternal => base.Owner.PlayerCombatState.Energy >= base.DynamicVars.Energy.IntValue;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(8m, ValueProp.Move),
		new EnergyVar(4)
	});

	public HeavenlyDrill()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		int num = ResolveEnergyXValue();
		if (num >= base.DynamicVars.Energy.IntValue)
		{
			num *= 2;
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(num).FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_giant_horizontal_slash", null, "slash_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
