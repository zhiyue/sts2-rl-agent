using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class SneckoOil : PotionModel
{
	private int _testEnergyCostOverride = -1;

	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AnyPlayer;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(7));

	public int TestEnergyCostOverride
	{
		get
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

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		NCombatRoom.Instance?.PlaySplashVfx(target, new Color("6ec46f"));
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, target.Player);
		IEnumerable<CardModel> enumerable = PileType.Hand.GetPile(target.Player).Cards.Where((CardModel c) => !c.EnergyCost.CostsX);
		foreach (CardModel item in enumerable)
		{
			if (item.EnergyCost.GetWithModifiers(CostModifiers.None) >= 0)
			{
				item.EnergyCost.SetThisTurnOrUntilPlayed(NextEnergyCost());
				NCard.FindOnTable(item)?.PlayRandomizeCostAnim();
			}
		}
	}

	private int NextEnergyCost()
	{
		if (TestEnergyCostOverride >= 0)
		{
			return TestEnergyCostOverride;
		}
		return base.Owner.RunState.Rng.CombatEnergyCosts.NextInt(4);
	}
}
