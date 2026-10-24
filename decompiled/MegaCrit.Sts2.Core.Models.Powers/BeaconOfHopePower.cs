using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class BeaconOfHopePower : PowerModel
{
	private bool _hasAlreadyBeenGivenBlock;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	/// <summary>
	/// Prevents Beacon of Hope from triggering if it already has triggered for this instance of block.
	/// This is to prevent Beacon of Hope infinitely triggering against another player Beacon of Hope.
	/// </summary>
	private bool HasAlreadyBeenGivenBlock
	{
		get
		{
			return _hasAlreadyBeenGivenBlock;
		}
		set
		{
			AssertMutable();
			_hasAlreadyBeenGivenBlock = value;
		}
	}

	public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		if (amount < 1m || creature != base.Owner || base.CombatState.CurrentSide != base.Owner.Side || HasAlreadyBeenGivenBlock)
		{
			return;
		}
		decimal amountToGive = amount * 0.5m;
		if (amountToGive < 1m)
		{
			return;
		}
		IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner)
			where c != null && c.IsAlive && c.IsPlayer && c.Player.Creature != base.Owner
			select c;
		HasAlreadyBeenGivenBlock = true;
		foreach (Creature item in enumerable)
		{
			await CreatureCmd.GainBlock(item, amountToGive, ValueProp.Unpowered, null);
		}
		HasAlreadyBeenGivenBlock = false;
	}
}
