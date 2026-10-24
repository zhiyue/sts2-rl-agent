using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class KinglyPunch : CardModel
{
	private const string _increaseKey = "Increase";

	private decimal _extraDamage;

	private decimal ExtraDamage
	{
		get
		{
			return _extraDamage;
		}
		set
		{
			AssertMutable();
			_extraDamage = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(8m, ValueProp.Move),
		new DynamicVar("Increase", 4m)
	});

	public KinglyPunch()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}

	public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this)
		{
			return Task.CompletedTask;
		}
		decimal baseValue = base.DynamicVars["Increase"].BaseValue;
		base.DynamicVars.Damage.BaseValue += baseValue;
		ExtraDamage += baseValue;
		return Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars["Increase"].UpgradeValueBy(2m);
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.Damage.BaseValue += ExtraDamage;
	}
}
