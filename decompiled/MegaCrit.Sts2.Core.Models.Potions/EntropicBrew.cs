using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class EntropicBrew : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.AnyTime;

	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
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
