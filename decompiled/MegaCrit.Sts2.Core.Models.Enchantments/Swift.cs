using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Swift : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool ShowAmount => true;

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		if (base.Status == EnchantmentStatus.Normal)
		{
			await CardPileCmd.Draw(choiceContext, base.Amount, base.Card.Owner);
			base.Status = EnchantmentStatus.Disabled;
		}
	}
}
