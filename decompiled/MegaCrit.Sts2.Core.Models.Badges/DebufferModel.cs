using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Keeps track of how many debuffs a player has applied.
/// </summary>
public class DebufferModel : BadgeModel
{
	public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power.GetTypeForAmount(amount) == PowerType.Debuff && !(power is ITemporaryPower) && applier != null && applier.Player != null && applier != power.Owner)
		{
			applier.Player.ExtraFields.DebuffsApplied++;
		}
		return Task.CompletedTask;
	}
}
