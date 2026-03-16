using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DelicateFrond : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override async Task BeforeCombatStart()
	{
		Flash();
		while (base.Owner.HasOpenPotionSlots)
		{
			PotionModel potion = PotionFactory.CreateRandomPotionOutOfCombat(base.Owner, base.Owner.RunState.Rng.CombatPotionGeneration).ToMutable();
			if (!(await PotionCmd.TryToProcure(potion, base.Owner)).success)
			{
				break;
			}
		}
	}
}
