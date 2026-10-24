using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class StratagemPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner.Player)
		{
			return;
		}
		Flash();
		foreach (CardModel item in await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(base.Owner.Player), base.Owner.Player, new CardSelectorPrefs(base.SelectionScreenPrompt, base.Amount)))
		{
			await CardPileCmd.Add(item, PileType.Hand);
		}
	}
}
