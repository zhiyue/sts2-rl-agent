using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class MasterPlannerPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Type != CardType.Skill)
		{
			return Task.CompletedTask;
		}
		Flash();
		CardCmd.ApplyKeyword(cardPlay.Card, CardKeyword.Sly);
		return Task.CompletedTask;
	}
}
