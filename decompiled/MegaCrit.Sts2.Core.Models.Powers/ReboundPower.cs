using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ReboundPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return (pileType, position);
		}
		if (pileType != PileType.Discard)
		{
			return (pileType, position);
		}
		return (PileType.Draw, CardPilePosition.Top);
	}

	public override async Task AfterModifyingCardPlayResultPileOrPosition(CardModel card, PileType pileType, CardPilePosition position)
	{
		if (card.Owner.Creature == base.Owner)
		{
			Flash();
			await PowerCmd.Decrement(this);
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Remove(this);
		}
	}
}
