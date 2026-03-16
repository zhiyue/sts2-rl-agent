using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Rampage : CardModel
{
	private const string _increaseKey = "Increase";

	private decimal _extraDamageFromPlays;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(9m, ValueProp.Move),
		new DynamicVar("Increase", 5m)
	});

	private decimal ExtraDamageFromPlays
	{
		get
		{
			return _extraDamageFromPlays;
		}
		set
		{
			AssertMutable();
			_extraDamageFromPlays = value;
		}
	}

	public Rampage()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		base.DynamicVars.Damage.BaseValue += base.DynamicVars["Increase"].BaseValue;
		ExtraDamageFromPlays += base.DynamicVars["Increase"].BaseValue;
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.Damage.BaseValue += ExtraDamageFromPlays;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Increase"].UpgradeValueBy(4m);
	}
}
