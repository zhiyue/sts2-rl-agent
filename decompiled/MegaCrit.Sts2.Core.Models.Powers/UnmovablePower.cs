using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class UnmovablePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (target.IsMonster)
		{
			return 1m;
		}
		if (!props.IsCardOrMonsterMove())
		{
			return 1m;
		}
		if (cardSource != null && cardSource.Owner.Creature != base.Owner)
		{
			return 1m;
		}
		int num = CombatManager.Instance.History.Entries.OfType<BlockGainedEntry>().Count((BlockGainedEntry e) => e.HappenedThisTurn(base.CombatState) && e.Actor == target && e.Props.IsCardOrMonsterMove() && e.CardPlay != cardPlay);
		if (num >= base.Amount)
		{
			return 1m;
		}
		return 2m;
	}
}
