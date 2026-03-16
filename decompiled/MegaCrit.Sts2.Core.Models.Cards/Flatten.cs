using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Flatten : CardModel
{
	protected override bool ShouldGlowGoldInternal => HasOstyAttackedThisTurn;

	protected override bool ShouldGlowRedInternal => base.Owner.IsOstyMissing;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.OstyAttack };

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new OstyDamageVar(12m, ValueProp.Move));

	private bool HasOstyAttackedThisTurn => CombatManager.Instance.History.Entries.OfType<CreatureAttackedEntry>().Any((CreatureAttackedEntry e) => e.Actor == base.Owner.Osty && e.HappenedThisTurn(base.CombatState));

	public Flatten()
		: base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		if (!Osty.CheckMissingWithAnim(base.Owner))
		{
			await DamageCmd.Attack(base.DynamicVars.OstyDamage.BaseValue).FromOsty(base.Owner.Osty, this).Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.OstyDamage.UpgradeValueBy(4m);
	}

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card != this)
		{
			return Task.CompletedTask;
		}
		if (!HasOstyAttackedThisTurn)
		{
			return Task.CompletedTask;
		}
		ReduceCost();
		return Task.CompletedTask;
	}

	public override Task AfterAttack(AttackCommand command)
	{
		if (command.Attacker == null)
		{
			return Task.CompletedTask;
		}
		if (command.Attacker != base.Owner.Osty)
		{
			return Task.CompletedTask;
		}
		ReduceCost();
		return Task.CompletedTask;
	}

	private void ReduceCost()
	{
		base.EnergyCost.SetThisTurn(0);
	}
}
