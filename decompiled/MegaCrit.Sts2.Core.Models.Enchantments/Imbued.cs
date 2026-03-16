using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Imbued : EnchantmentModel
{
	public override bool ShouldStartAtBottomOfDrawPile => true;

	public override bool ShowAmount => false;

	public override bool CanEnchantCardType(CardType cardType)
	{
		return cardType == CardType.Skill;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Card.Owner && base.Card.CombatState.RoundNumber == 1)
		{
			await CardCmd.AutoPlay(choiceContext, base.Card, null);
		}
	}
}
