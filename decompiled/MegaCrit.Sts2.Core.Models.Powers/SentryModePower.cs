using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SentryModePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<SweepingGaze>());

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner.Player)
		{
			for (int i = 0; i < base.Amount; i++)
			{
				CardModel card = combatState.CreateCard<SweepingGaze>(base.Owner.Player);
				await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
			}
		}
	}
}
