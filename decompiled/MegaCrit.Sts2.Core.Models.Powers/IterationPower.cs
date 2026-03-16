using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class IterationPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner.Creature == base.Owner && card.Type == CardType.Status)
		{
			int num = CombatManager.Instance.History.Entries.OfType<CardDrawnEntry>().Count((CardDrawnEntry e) => e.HappenedThisTurn(base.CombatState) && e.Actor == base.Owner && e.Card.Type == CardType.Status);
			if (num <= 1)
			{
				Flash();
				await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player);
			}
		}
	}
}
