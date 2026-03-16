using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SignalBoostPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return playCount;
		}
		if (card.Type != CardType.Power)
		{
			return playCount;
		}
		return playCount + 1;
	}

	public override async Task AfterModifyingCardPlayCount(CardModel card)
	{
		await PowerCmd.Decrement(this);
	}
}
