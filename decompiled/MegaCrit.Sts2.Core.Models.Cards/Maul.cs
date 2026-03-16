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

public sealed class Maul : CardModel
{
	private const string _increaseKey = "Increase";

	private decimal _extraDamageFromMaulPlays;

	private decimal ExtraDamageFromMaulPlays
	{
		get
		{
			return _extraDamageFromMaulPlays;
		}
		set
		{
			AssertMutable();
			_extraDamageFromMaulPlays = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(5m, ValueProp.Move),
		new DynamicVar("Increase", 1m)
	});

	public Maul()
		: base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitVfxNode((Creature t) => NScratchVfx.Create(t, goingRight: true))
			.Execute(choiceContext);
		IEnumerable<Maul> enumerable = base.Owner.PlayerCombatState.AllCards.OfType<Maul>();
		decimal baseValue = base.DynamicVars["Increase"].BaseValue;
		foreach (Maul item in enumerable)
		{
			item.BuffFromMaulPlay(baseValue);
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
		base.DynamicVars.Damage.BaseValue += ExtraDamageFromMaulPlays;
	}

	private void BuffFromMaulPlay(decimal extraDamage)
	{
		base.DynamicVars.Damage.BaseValue += extraDamage;
		ExtraDamageFromMaulPlays += extraDamage;
	}
}
