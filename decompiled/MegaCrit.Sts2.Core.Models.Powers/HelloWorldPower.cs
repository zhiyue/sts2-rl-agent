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

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class HelloWorldPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner.Player && base.AmountOnTurnStart >= 1)
		{
			Flash();
			IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(base.Owner.Player, from c in base.Owner.Player.Character.CardPool.GetUnlockedCards(base.Owner.Player.UnlockState, base.Owner.Player.RunState.CardMultiplayerConstraint)
				where c.Rarity == CardRarity.Common
				select c, base.AmountOnTurnStart, base.Owner.Player.RunState.Rng.CombatCardGeneration);
			await CardPileCmd.AddGeneratedCardsToCombat(distinctForCombat, PileType.Hand, addedByPlayer: true);
		}
	}
}
