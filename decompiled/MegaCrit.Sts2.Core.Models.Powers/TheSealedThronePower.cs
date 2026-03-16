using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TheSealedThronePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner.Player)
		{
			Flash();
			await PlayerCmd.GainStars(base.Amount, base.Owner.Player);
		}
	}
}
