using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class CursedRun : ModifierModel
{
	public override async Task AfterActEntered()
	{
		foreach (Player player in base.RunState.Players)
		{
			CardModel canonicalCard = base.RunState.Rng.Niche.NextItem(from c in ModelDb.CardPool<CurseCardPool>().GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
				where c.CanBeGeneratedByModifiers
				select c);
			CardModel card = player.RunState.CreateCard(canonicalCard, player);
			CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
			if (LocalContext.IsMe(player))
			{
				CardCmd.PreviewCardPileAdd(result);
			}
		}
	}
}
