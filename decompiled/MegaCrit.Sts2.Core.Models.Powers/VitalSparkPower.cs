using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class VitalSparkPower : PowerModel
{
	private class Data
	{
		public readonly HashSet<Player> playersTriggeredThisTurn = new HashSet<Player>();
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(1));

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && dealer != null && props.IsPoweredAttack() && !result.WasFullyBlocked)
		{
			Creature creature = dealer;
			if (dealer.Monster is Osty)
			{
				creature = dealer.PetOwner.Creature;
			}
			if (creature.Player != null && !GetInternalData<Data>().playersTriggeredThisTurn.Contains(creature.Player))
			{
				GetInternalData<Data>().playersTriggeredThisTurn.Add(creature.Player);
				Flash();
				await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, creature.Player);
			}
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != CombatSide.Enemy)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().playersTriggeredThisTurn.Clear();
		return Task.CompletedTask;
	}
}
