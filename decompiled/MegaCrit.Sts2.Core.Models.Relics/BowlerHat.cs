using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BowlerHat : RelicModel
{
	private const string _goldIncreaseKey = "GoldIncrease";

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool IsAllowedInShops => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("GoldIncrease", 1.25m));

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override decimal ModifyGoldGained(Player player, decimal amount)
	{
		if (player != base.Owner)
		{
			return amount;
		}
		return amount * base.DynamicVars["GoldIncrease"].BaseValue;
	}

	public override Task AfterModifyingGoldGained(Player player, decimal amount)
	{
		Flash();
		return Task.CompletedTask;
	}
}
