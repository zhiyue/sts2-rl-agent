using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TrashToTreasurePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
	{
		if (addedByPlayer && card.Type == CardType.Status)
		{
			Flash();
			for (int i = 0; i < base.Amount; i++)
			{
				OrbModel orb = OrbModel.GetRandomOrb(base.Owner.Player.RunState.Rng.CombatOrbGeneration).ToMutable();
				await OrbCmd.Channel(new ThrowingPlayerChoiceContext(), orb, base.Owner.Player);
			}
		}
	}
}
