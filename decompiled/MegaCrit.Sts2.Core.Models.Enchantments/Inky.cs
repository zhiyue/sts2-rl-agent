using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Inky : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool ShowAmount => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(1m, ValueProp.Move),
		new PowerVar<WeakPower>(1m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<WeakPower>());

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		IReadOnlyList<Creature> targets;
		if (base.Card.TargetType != TargetType.AllEnemies)
		{
			IReadOnlyList<Creature> readOnlyList = new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(cardPlay.Target);
			targets = readOnlyList;
		}
		else
		{
			targets = base.Card.CombatState.HittableEnemies;
		}
		await PowerCmd.Apply<WeakPower>(choiceContext, targets, base.DynamicVars.Weak.BaseValue, base.Card.Owner.Creature, base.Card);
	}

	public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		return base.DynamicVars.Damage.BaseValue;
	}
}
