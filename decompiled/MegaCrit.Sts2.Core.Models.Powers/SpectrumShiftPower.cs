using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SpectrumShiftPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner.Player)
		{
			List<CardModel> cards = CardFactory.GetDistinctForCombat(player, ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint), base.Amount, player.RunState.Rng.CombatCardGeneration).ToList();
			await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, addedByPlayer: true);
			Flash();
		}
	}
}
