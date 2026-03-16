using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class DeprecatedPotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.None;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AnyEnemy;
}
