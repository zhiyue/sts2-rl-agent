using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Misery : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(7m, ValueProp.Move));

	public Misery()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		List<PowerModel> originalDebuffs = (from p in cardPlay.Target.Powers
			where p.TypeForCurrentAmount == PowerType.Debuff
			select (PowerModel)p.ClonePreservingMutability()).ToList();
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		foreach (Creature enemy in base.CombatState.HittableEnemies)
		{
			if (enemy == cardPlay.Target)
			{
				continue;
			}
			foreach (PowerModel item in originalDebuffs)
			{
				PowerModel powerModel = PowerCmd.FindExistingInstanceForStacking(item, enemy, item.Applier);
				if (powerModel != null)
				{
					DoHackyThingsForSpecificPowers(powerModel);
					await PowerCmd.ModifyAmount(choiceContext, powerModel, item.Amount, item.Applier, this);
				}
				else
				{
					PowerModel power = (PowerModel)item.ClonePreservingMutability();
					DoHackyThingsForSpecificPowers(power);
					await PowerCmd.Apply(choiceContext, power, enemy, item.Amount, item.Applier, this);
				}
			}
		}
	}

	private static void DoHackyThingsForSpecificPowers(PowerModel power)
	{
		if (power is ITemporaryPower temporaryPower)
		{
			temporaryPower.IgnoreNextInstance();
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		AddKeyword(CardKeyword.Retain);
	}
}
