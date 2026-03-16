using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SpiritOfAshPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
		HoverTipFactory.Static(StaticHoverTip.Block)
	});

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Keywords.Contains(CardKeyword.Ethereal))
		{
			await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
		}
	}
}
