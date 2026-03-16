using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Entrench : CardModel
{
	public override CardPoolModel VisualCardPool => ModelDb.CardPool<IroncladCardPool>();

	public override bool GainsBlock => true;

	public Entrench()
		: base(2, CardType.Skill, CardRarity.Event, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.Owner.Creature.Block, ValueProp.Unpowered | ValueProp.Move, cardPlay);
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
