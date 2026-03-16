using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockCloneCardsOnPlayPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner.Player)
		{
			CardModel card = cardPlay.Card.CreateClone();
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
		}
	}
}
