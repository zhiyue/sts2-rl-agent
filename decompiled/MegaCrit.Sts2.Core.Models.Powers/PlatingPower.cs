using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PlatingPower : PowerModel
{
	private const string _decrementKey = "Decrement";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldScaleInMultiplayer => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Decrement", 1m));

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (base.Owner.Side == CombatSide.Enemy)
		{
			base.DynamicVars["Decrement"].BaseValue = base.Owner.CombatState.RunState.Players.Count;
		}
		return Task.CompletedTask;
	}

	/// <summary>
	/// We want enemies that start with Plating to also start combat with block.
	/// </summary>
	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side != CombatSide.Player)
		{
			return Task.CompletedTask;
		}
		if (base.Owner.IsPlayer)
		{
			return Task.CompletedTask;
		}
		if (combatState.RoundNumber > 1)
		{
			return Task.CompletedTask;
		}
		return CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
	}

	/// <summary>
	/// We do this in early so that it triggers before end-of-turn damage effects.
	/// </summary>
	public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			Flash();
			await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(base.Owner) && (base.Owner.Player == null || base.Owner.Player.PlayerCombatState.TurnNumber != 1) && (base.Owner.Side != CombatSide.Enemy || combatState.RoundNumber != 1))
		{
			if (base.Owner.Side == CombatSide.Enemy)
			{
				await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -base.DynamicVars["Decrement"].BaseValue, null, null);
			}
			else
			{
				await PowerCmd.Decrement(this);
			}
		}
	}

	public override decimal GetScaledAmountForMultiplayer(ICombatState combatState, Creature? applier, decimal amount, Creature target, CardModel? cardSource)
	{
		return (decimal)((combatState.Players.Count - 1) * 2 + 1) * amount;
	}
}
