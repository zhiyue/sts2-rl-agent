using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ToolsOfTheTradePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner.Player)
		{
			return count;
		}
		return count + (decimal)base.Amount;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player)
		{
			List<CardModel> list = (await CardSelectCmd.FromHandForDiscard(choiceContext, player, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, base.Amount), null, this)).ToList();
			if (list.Count != 0)
			{
				await CardCmd.Discard(choiceContext, list);
			}
		}
	}
}
