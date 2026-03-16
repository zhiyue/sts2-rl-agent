using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Claw : CardModel
{
	private const string _increaseKey = "Increase";

	private decimal _extraDamageFromClawPlays;

	private decimal ExtraDamageFromClawPlays
	{
		get
		{
			return _extraDamageFromClawPlays;
		}
		set
		{
			AssertMutable();
			_extraDamageFromClawPlays = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(3m, ValueProp.Move),
		new DynamicVar("Increase", 2m)
	});

	public Claw()
		: base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitVfxNode((Creature t) => NScratchVfx.Create(t, goingRight: true))
			.Execute(choiceContext);
		IEnumerable<Claw> enumerable = base.Owner.PlayerCombatState.AllCards.OfType<Claw>();
		decimal baseValue = base.DynamicVars["Increase"].BaseValue;
		foreach (Claw item in enumerable)
		{
			item.BuffFromClawPlay(baseValue);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
		base.DynamicVars["Increase"].UpgradeValueBy(1m);
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.Damage.BaseValue += ExtraDamageFromClawPlays;
	}

	private void BuffFromClawPlay(decimal extraDamage)
	{
		base.DynamicVars.Damage.BaseValue += extraDamage;
		ExtraDamageFromClawPlays += extraDamage;
	}
}
