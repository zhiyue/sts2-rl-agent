using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class JeweledMask : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber <= 1)
		{
			IReadOnlyList<CardModel> cards = PileType.Draw.GetPile(player).Cards;
			List<CardModel> list = cards.Where((CardModel c) => c.Type == CardType.Power).ToList();
			if (list.Count != 0)
			{
				CardModel cardModel = player.RunState.Rng.CombatCardSelection.NextItem(list);
				Flash();
				cardModel.SetToFreeThisTurn();
				await CardPileCmd.Add(cardModel, PileType.Hand);
			}
		}
	}
}
