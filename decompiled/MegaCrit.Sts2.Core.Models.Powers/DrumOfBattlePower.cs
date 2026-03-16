using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class DrumOfBattlePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public override async Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player.Creature != base.Owner)
		{
			return;
		}
		Flash();
		CardPile drawPile = PileType.Draw.GetPile(base.Owner.Player);
		for (int i = 0; i < base.Amount; i++)
		{
			await CardPileCmd.ShuffleIfNecessary(choiceContext, base.Owner.Player);
			CardModel cardModel = drawPile.Cards.FirstOrDefault();
			if (cardModel != null)
			{
				await CardCmd.Exhaust(choiceContext, cardModel);
			}
		}
	}
}
