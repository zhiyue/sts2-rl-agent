using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class AggressionPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Side)
		{
			return;
		}
		CardPile pile = PileType.Discard.GetPile(base.Owner.Player);
		IEnumerable<CardModel> source = pile.Cards.Where((CardModel c) => c.Type == CardType.Attack);
		IEnumerable<CardModel> enumerable = source.ToList().UnstableShuffle(base.Owner.Player.RunState.Rng.CombatCardSelection).Take(base.Amount);
		foreach (CardModel card in enumerable)
		{
			await CardPileCmd.Add(card, PileType.Hand);
			if (card.IsUpgradable)
			{
				CardCmd.Upgrade(card);
			}
		}
	}
}
