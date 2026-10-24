using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;

namespace MegaCrit.Sts2.Core.Models.Potions;

/// <summary>
/// Represents a potion that has been removed from the game. Mostly used for the run history.
/// </summary>
public sealed class DeprecatedPotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.None;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AnyEnemy;
}
