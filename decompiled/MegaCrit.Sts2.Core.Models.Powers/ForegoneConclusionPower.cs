using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ForegoneConclusionPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == base.Owner.Player)
		{
			await CardPileCmd.ShuffleIfNecessary(choiceContext, base.Owner.Player);
			await CardPileCmd.Add(await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(base.Owner.Player), base.Owner.Player, new CardSelectorPrefs(base.SelectionScreenPrompt, base.Amount)), PileType.Hand);
			await PowerCmd.Remove(this);
		}
	}
}
