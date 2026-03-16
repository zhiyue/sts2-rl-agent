using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Pendulum : RelicModel
{
	public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

	public override RelicRarity Rarity => RelicRarity.Common;

	public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner)
		{
			Flash();
			await CardPileCmd.Draw(choiceContext, player);
		}
	}
}
