using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Bellows : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (player.Creature.CombatState.RoundNumber > 1)
		{
			return Task.CompletedTask;
		}
		Flash();
		CardCmd.Upgrade(PileType.Hand.GetPile(base.Owner).Cards, CardPreviewStyle.HorizontalLayout);
		return Task.CompletedTask;
	}
}
