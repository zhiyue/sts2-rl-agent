using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class VexingPuzzlebox : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner && base.Owner.Creature.CombatState.RoundNumber == 1)
		{
			Flash();
			CardModel cardModel = CardFactory.GetDistinctForCombat(base.Owner, base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint), 1, base.Owner.RunState.Rng.CombatCardGeneration).First();
			cardModel.EnergyCost.SetThisCombat(0);
			await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
		}
	}
}
