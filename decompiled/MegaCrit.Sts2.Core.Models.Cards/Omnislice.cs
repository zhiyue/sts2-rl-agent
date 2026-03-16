using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Omnislice : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(8m, ValueProp.Move));

	public Omnislice()
		: base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await using AttackContext context = await AttackCommand.CreateContextAsync(base.CombatState, this);
		List<DamageResult> list = (await CreatureCmd.Damage(choiceContext, cardPlay.Target, base.DynamicVars.Damage.BaseValue, ValueProp.Move, this)).ToList();
		context.AddHit(list);
		DamageResult damageResult = list.FirstOrDefault();
		if (damageResult != null)
		{
			List<Creature> list2 = (from e in base.CombatState.GetTeammatesOf(damageResult.Receiver).Except(new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(cardPlay.Target))
				where e.IsHittable
				select e).ToList();
			if (list2.Count != 0)
			{
				AttackContext attackContext = context;
				attackContext.AddHit(await CreatureCmd.Damage(choiceContext, list2, damageResult.TotalDamage + damageResult.OverkillDamage, ValueProp.Unpowered | ValueProp.Move, base.Owner.Creature, this));
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
