using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ConfusedPower : PowerModel
{
	private int _testEnergyCostOverride = -1;

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Single;

	public int TestEnergyCostOverride
	{
		private get
		{
			return _testEnergyCostOverride;
		}
		set
		{
			TestMode.AssertOn();
			AssertMutable();
			_testEnergyCostOverride = value;
		}
	}

	public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		if (card.EnergyCost.Canonical < 0)
		{
			return Task.CompletedTask;
		}
		int cost = NextEnergyCost();
		card.EnergyCost.SetThisCombat(cost);
		NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
		return Task.CompletedTask;
	}

	private int NextEnergyCost()
	{
		if (TestEnergyCostOverride >= 0)
		{
			return TestEnergyCostOverride;
		}
		return base.Owner.Player.RunState.Rng.CombatEnergyCosts.NextInt(4);
	}
}
