using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class HelloWorld : CardModel
{
	public override CardPoolModel VisualCardPool => ModelDb.CardPool<DefectCardPool>();

	public HelloWorld()
		: base(1, CardType.Power, CardRarity.Event, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await PowerCmd.Apply<HelloWorldPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Innate);
	}
}
