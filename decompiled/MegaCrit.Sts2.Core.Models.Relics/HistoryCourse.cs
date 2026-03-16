using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class HistoryCourse : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner || base.Owner.Creature.CombatState.RoundNumber == 1)
		{
			return;
		}
		CardModel cardModel = CombatManager.Instance.History.CardPlaysFinished.LastOrDefault(delegate(CardPlayFinishedEntry e)
		{
			bool flag = e.CardPlay.Card.Owner == base.Owner && e.RoundNumber == base.Owner.Creature.CombatState.RoundNumber - 1;
			bool flag2 = flag;
			if (flag2)
			{
				CardType type = e.CardPlay.Card.Type;
				bool flag3 = (uint)(type - 1) <= 1u;
				flag2 = flag3;
			}
			return flag2 && !e.CardPlay.Card.IsDupe;
		})?.CardPlay.Card;
		if (cardModel != null)
		{
			Flash();
			await CardCmd.AutoPlay(choiceContext, cardModel.CreateDupe(), null);
		}
	}
}
