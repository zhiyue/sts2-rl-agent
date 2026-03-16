using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SuckPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	public override async Task AfterAttack(AttackCommand command)
	{
		if (command.Attacker != base.Owner || command.TargetSide == base.Owner.Side || !command.DamageProps.IsPoweredAttack())
		{
			return;
		}
		List<DamageResult> list = command.Results.ToList();
		List<DamageResult> list2 = list.Where((DamageResult r) => r.Receiver.IsPet).ToList();
		foreach (DamageResult petHit in list2)
		{
			list.RemoveAll((DamageResult r) => r.Receiver == petHit.Receiver.PetOwner?.Creature);
		}
		int num = list.Count((DamageResult r) => r.UnblockedDamage > 0);
		if (num > 0)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(base.Owner, base.Amount * num, base.Owner, null);
		}
	}
}
