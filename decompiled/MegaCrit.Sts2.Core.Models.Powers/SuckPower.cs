using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SuckPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		if (command.Attacker != base.Owner || command.TargetSide == base.Owner.Side || !command.DamageProps.IsPoweredAttack())
		{
			return;
		}
		List<List<DamageResult>> list = command.Results.ToList();
		int num = 0;
		foreach (List<DamageResult> item in list)
		{
			List<DamageResult> list2 = item.Where((DamageResult r) => r.Receiver.IsPet).ToList();
			foreach (DamageResult petHit in list2)
			{
				item.RemoveAll((DamageResult r) => r.Receiver == petHit.Receiver.PetOwner?.Creature);
			}
			if (item.Any((DamageResult r) => r.UnblockedDamage > 0))
			{
				num++;
			}
		}
		if (num > 0)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, base.Amount * num, base.Owner, null);
		}
	}
}
