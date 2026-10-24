using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SkittishPower : PowerModel
{
	private class Data
	{
		/// <summary>
		/// Has the owner gained block from Skittish this turn?
		/// Skittish should only trigger once per turn, so we can skip when this is true.
		/// </summary>
		public bool hasGainedBlockThisTurn;
	}

	private const string _extendSfx = "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_extend";

	private const string _retractSfx = "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_retract";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldScaleInMultiplayer => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public bool HasGainedBlockThisTurn
	{
		get
		{
			return GetInternalData<Data>().hasGainedBlockThisTurn;
		}
		private set
		{
			AssertMutable();
			GetInternalData<Data>().hasGainedBlockThisTurn = value;
		}
	}

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		if (!HasGainedBlockThisTurn && command.DamageProps.HasFlag(ValueProp.Move) && command.ModelSource is CardModel)
		{
			DamageResult damageResult = command.Results.SelectMany((List<DamageResult> r) => r).FirstOrDefault((DamageResult r) => r.Receiver == base.Owner);
			if (damageResult != null && damageResult.UnblockedDamage != 0)
			{
				HasGainedBlockThisTurn = true;
				SfxCmd.Play("event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_retract");
				await CreatureCmd.TriggerAnim(base.Owner, "BlockStart", 0.3f);
				await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != base.Owner.Side)
		{
			if (HasGainedBlockThisTurn)
			{
				SfxCmd.Play("event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_extend");
				await CreatureCmd.TriggerAnim(base.Owner, "BlockEnd", 0.15f);
			}
			HasGainedBlockThisTurn = false;
		}
	}

	public override decimal GetScaledAmountForMultiplayer(ICombatState combatState, Creature? applier, decimal amount, Creature target, CardModel? cardSource)
	{
		return amount * (1m + (decimal)(combatState.Players.Count - 1) * 0.5m);
	}
}
