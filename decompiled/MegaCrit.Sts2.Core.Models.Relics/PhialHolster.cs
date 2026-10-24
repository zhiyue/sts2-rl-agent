using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PhialHolster : RelicModel
{
	private const string _potionSlotsKey = "PotionSlots";

	private const string _potionCountKey = "Potions";

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("PotionSlots", 1m),
		new DynamicVar("Potions", 2m)
	});

	public override async Task AfterObtained()
	{
		await PlayerCmd.GainMaxPotionCount(base.DynamicVars["PotionSlots"].IntValue, base.Owner);
		List<PotionModel> list = PotionFactory.CreateRandomPotionsOutOfCombat(base.Owner, base.DynamicVars["Potions"].IntValue, base.Owner.RunState.Rng.CombatPotionGeneration);
		foreach (PotionModel item in list)
		{
			await PotionCmd.TryToProcure(item.ToMutable(), base.Owner);
		}
	}
}
