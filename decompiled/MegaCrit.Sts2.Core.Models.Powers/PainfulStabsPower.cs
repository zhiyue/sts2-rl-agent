using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PainfulStabsPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Wound>();

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		return creature != base.Owner;
	}

	public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		if (command.Attacker != base.Owner || command.TargetSide == base.Owner.Side || !command.DamageProps.IsPoweredAttack())
		{
			return;
		}
		List<DamageResult> list = command.Results.SelectMany((List<DamageResult> r) => r).ToList();
		if (!list.Any((DamageResult r) => r.UnblockedDamage > 0))
		{
			return;
		}
		Dictionary<Creature, List<DamageResult>> damageResultsByCreature = new Dictionary<Creature, List<DamageResult>>();
		foreach (DamageResult item in list)
		{
			if (item.Receiver.IsPlayer)
			{
				if (!damageResultsByCreature.ContainsKey(item.Receiver))
				{
					damageResultsByCreature.Add(item.Receiver, new List<DamageResult>());
				}
				damageResultsByCreature[item.Receiver].Add(item);
			}
		}
		bool anyWoundApplied = false;
		foreach (Creature key in damageResultsByCreature.Keys)
		{
			int num = damageResultsByCreature[key].Count((DamageResult r) => r.UnblockedDamage > 0);
			anyWoundApplied = anyWoundApplied || num > 0;
			await CardPileCmd.AddToCombatAndPreview<Wound>(key, PileType.Discard, base.Amount * num, null);
		}
		if (anyWoundApplied)
		{
			Flash();
		}
	}
}
