using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Pinpoint : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(17m, ValueProp.Move));

	public Pinpoint()
		: base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(5m);
	}

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card != this)
		{
			return Task.CompletedTask;
		}
		if (base.IsClone)
		{
			return Task.CompletedTask;
		}
		int amount = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.CardPlay.Card.Type == CardType.Skill && e.CardPlay.Card.Owner == base.Owner && e.HappenedThisTurn(base.CombatState));
		ReduceCostBy(amount);
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Type != CardType.Skill)
		{
			return Task.CompletedTask;
		}
		ReduceCostBy(1);
		return Task.CompletedTask;
	}

	private void ReduceCostBy(int amount)
	{
		base.EnergyCost.AddThisTurn(-amount);
	}
}
