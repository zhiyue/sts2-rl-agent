using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class WellLaidPlansPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner.Player || !Hook.ShouldFlush(player.Creature.CombatState, player))
		{
			return;
		}
		List<CardModel> list = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 0, base.Amount), context: choiceContext, player: base.Owner.Player, filter: RetainFilter, source: this)).ToList();
		if (list.Count == 0)
		{
			return;
		}
		foreach (CardModel item in list)
		{
			item.GiveSingleTurnRetain();
		}
	}

	private bool RetainFilter(CardModel card)
	{
		return !card.ShouldRetainThisTurn;
	}
}
