using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class CountdownPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(base.Owner))
		{
			Flash();
			Creature creature = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
			if (creature != null)
			{
				await PowerCmd.Apply<DoomPower>(new ThrowingPlayerChoiceContext(), creature, base.Amount, base.Owner, null);
			}
		}
	}
}
