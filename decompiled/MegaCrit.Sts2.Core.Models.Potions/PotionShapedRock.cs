using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class PotionShapedRock : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Token;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AnyEnemy;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(15m, ValueProp.Unpowered));

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		await CreatureCmd.Damage(choiceContext, target, base.DynamicVars.Damage, base.Owner.Creature, null);
	}
}
