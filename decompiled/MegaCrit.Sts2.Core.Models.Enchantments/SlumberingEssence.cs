using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class SlumberingEssence : EnchantmentModel
{
	public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Card.Owner)
		{
			return Task.CompletedTask;
		}
		CardPile? pile = base.Card.Pile;
		if (pile == null || pile.Type != PileType.Hand)
		{
			return Task.CompletedTask;
		}
		base.Card.EnergyCost.AddUntilPlayed(-1);
		return Task.CompletedTask;
	}
}
