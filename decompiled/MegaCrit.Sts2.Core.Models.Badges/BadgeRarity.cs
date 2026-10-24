namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// The rarity used by Badges in the Run Summary screen.
/// The rarity is based on how hard they are to get and reward bonus points the harder they are.
/// Used for tiebreakers amongst players who have a tied score in the Leaderboards.
/// </summary>
public enum BadgeRarity
{
	None,
	Bronze,
	Silver,
	Gold
}
