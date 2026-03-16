using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Slither : EnchantmentModel
{
	private int _testEnergyCostOverride = -1;

	public int TestEnergyCostOverride
	{
		get
		{
			return _testEnergyCostOverride;
		}
		set
		{
			if (TestMode.IsOff)
			{
				throw new InvalidOperationException("Only set this value in test mode.");
			}
			AssertMutable();
			_testEnergyCostOverride = value;
		}
	}

	public override bool CanEnchant(CardModel card)
	{
		if (base.CanEnchant(card) && !card.Keywords.Contains(CardKeyword.Unplayable))
		{
			return !card.EnergyCost.CostsX;
		}
		return false;
	}

	public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != base.Card)
		{
			return Task.CompletedTask;
		}
		if (base.Card.Pile.Type != PileType.Hand)
		{
			return Task.CompletedTask;
		}
		base.Card.EnergyCost.SetThisCombat(NextEnergyCost());
		NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
		return Task.CompletedTask;
	}

	private int NextEnergyCost()
	{
		if (TestEnergyCostOverride >= 0)
		{
			return TestEnergyCostOverride;
		}
		return base.Card.Owner.RunState.Rng.CombatEnergyCosts.NextInt(4);
	}
}
