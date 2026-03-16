using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class EchoFormPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return playCount;
		}
		int num = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => e.Actor == base.Owner && e.CardPlay.IsFirstInSeries && e.HappenedThisTurn(base.CombatState));
		if (num >= base.Amount)
		{
			return playCount;
		}
		return playCount + 1;
	}

	public override Task AfterModifyingCardPlayCount(CardModel card)
	{
		Flash();
		return Task.CompletedTask;
	}
}
