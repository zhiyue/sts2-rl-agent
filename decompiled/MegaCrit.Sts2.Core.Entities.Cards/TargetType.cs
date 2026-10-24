namespace MegaCrit.Sts2.Core.Entities.Cards;

public enum TargetType
{
	None,
	/// <summary>Can only target self. No target selection.</summary>
	Self,
	/// <summary>Can target any enemy. Target selection is performed to determine the enemy.</summary>
	AnyEnemy,
	/// <summary>Targets all enemies. No target selection.</summary>
	AllEnemies,
	/// <summary>Targets all enemies. No target selection.</summary>
	RandomEnemy,
	/// <summary>
	/// Targets any player.
	/// In multiplayer, target selection is performed to determine the player.
	/// In singleplayer, no target selection is performed.
	/// </summary>
	AnyPlayer,
	/// <summary>
	/// Targets any ally (any player excluding itself).
	/// In multiplayer, target selection is performed to determine the player.
	/// You should not see this in singleplayer.
	/// </summary>
	AnyAlly,
	/// <summary>Targets all allies. No target selection.</summary>
	AllAllies,
	/// <summary>
	/// Target selection is performed, but no creature target is expected.
	/// Currently only used by <see cref="T:MegaCrit.Sts2.Core.Models.Potions.FoulPotion" /> to target the merchant, which is not a creature.
	/// </summary>
	TargetedNoCreature,
	/// <summary>Targets the local player's Osty</summary>
	Osty
}
