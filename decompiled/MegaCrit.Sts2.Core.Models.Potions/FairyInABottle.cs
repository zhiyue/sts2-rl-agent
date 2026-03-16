using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class FairyInABottle : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.Automatic;

	public override TargetType TargetType => TargetType.Self;

	public override bool CanBeGeneratedInCombat => false;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		await CreatureCmd.Heal(target, (decimal)target.MaxHp * 0.3m);
	}

	public override bool ShouldDie(Creature creature)
	{
		if (creature != base.Owner.Creature)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		await OnUseWrapper(new ThrowingPlayerChoiceContext(), creature);
	}
}
