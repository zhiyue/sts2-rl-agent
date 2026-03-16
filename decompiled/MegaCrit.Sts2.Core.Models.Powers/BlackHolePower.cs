using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class BlackHolePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Resources.StarsSpent > 0 && cardPlay.Card.Owner == base.Owner.Player && cardPlay.IsLastInSeries)
		{
			await DealDamageToAllEnemies();
		}
	}

	public override async Task AfterStarsGained(int amount, Player gainer)
	{
		if (amount > 0 && gainer == base.Owner.Player)
		{
			await DealDamageToAllEnemies();
		}
	}

	private async Task DealDamageToAllEnemies()
	{
		Flash();
		await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), base.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner, null);
	}
}
