using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class MayhemPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player)
		{
			await CardPileCmd.AutoPlayFromDrawPile(choiceContext, base.Owner.Player, base.Amount, CardPilePosition.Top, forceExhaust: false);
		}
	}
}
