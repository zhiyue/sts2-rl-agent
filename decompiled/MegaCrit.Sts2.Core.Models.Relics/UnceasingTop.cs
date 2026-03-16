using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class UnceasingTop : RelicModel
{
	public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override async Task AfterHandEmptied(PlayerChoiceContext choiceContext, Player player)
	{
		if (CombatManager.Instance.IsPlayPhase && player == base.Owner)
		{
			Flash();
			await CardPileCmd.Draw(choiceContext, player);
		}
	}
}
