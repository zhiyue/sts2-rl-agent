using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Entities.Creatures;

/// <summary>
/// A class containing a bunch of meta-info about an instance of damage that was dealt to a creature.
/// See comments on individual properties for more details and examples.
/// </summary>
public class DamageResult
{
	/// <summary>
	/// The creature that received the damage.
	/// </summary>
	public Creature Receiver { get; }

	/// <summary>
	/// The value props of the damage that was dealt.
	/// </summary>
	public ValueProp Props { get; }

	/// <summary>
	/// The amount of damage that was blocked by the target.
	/// </summary>
	/// <example>
	/// X deals 10 damage to Y, who has 15 Block. BlockedDamage = 10.
	/// X deals 20 damage to Y, who has 15 Block. BlockedDamage = 15.
	/// X deals 20 damage to Y, who has 20 Block. BlockedDamage = 20.
	/// X deals 20 damage to Y, who has 0 Block.  BlockedDamage = 20.
	/// </example>
	public int BlockedDamage { get; set; }

	/// <summary>
	/// The amount of damage that the target received after blocking all that it could.
	/// </summary>
	/// <example>
	/// X deals 10 damage to Y, who has 15 Block. UnblockedDamage = 0.
	/// X deals 20 damage to Y, who has 15 Block. UnblockedDamage = 5.
	/// X deals 20 damage to Y, who has 20 Block. UnblockedDamage = 0.
	/// X deals 20 damage to Y, who has 0 Block.  UnblockedDamage = 20.
	/// </example>
	public int UnblockedDamage { get; init; }

	/// <summary>
	/// If the target was killed, this will be the amount of extra damage that the target received after their HP
	/// dropped to 0.
	/// </summary>
	/// <example>
	/// X deals 10 damage to Y, who has 15 HP. OverkillDamage = 0.
	/// X deals 10 damage to Y, who has 10 HP. OverkillDamage = 0.
	/// X deals 10 damage to Y, who has 5 HP.  OverkillDamage = 5.
	/// </example>
	public int OverkillDamage { get; init; }

	/// <summary>
	/// The total amount of damage that the target received, including both blocked and unblocked.
	/// </summary>
	/// <example>
	/// X deals 10 damage to Y, who has 15 Block. TotalDamage = 10.
	/// X deals 20 damage to Y, who has 15 Block. TotalDamage = 20.
	/// X deals 20 damage to Y, who has 20 Block. TotalDamage = 20.
	/// X deals 20 damage to Y, who has 0 Block.  TotalDamage = 20.
	/// </example>
	public int TotalDamage => BlockedDamage + UnblockedDamage;

	/// <summary>
	/// Whether the creature's block was broken by this instance of damage.
	/// Note: If possible, you should prefer using the AfterBlockBroken hook instead of checking this, as non-damage
	/// sources like Expose can remove block.
	/// </summary>
	/// <example>
	/// X deals 10 damage to Y, who has 15 Block. WasBlockBroken = false.
	/// X deals 20 damage to Y, who has 15 Block. WasBlockBroken = true.
	/// X deals 20 damage to Y, who has 20 Block. WasBlockBroken = true.
	/// X deals 20 damage to Y, who has 0 Block.  WasBlockBroken = false.
	/// </example>
	public bool WasBlockBroken { get; set; }

	/// <summary>
	/// Whether the entirety of this instance of damage was blocked.
	/// </summary>
	/// <example>
	/// X deals 10 damage to Y, who has 15 Block. WasFullyBlocked = true.
	/// X deals 20 damage to Y, who has 15 Block. WasFullyBlocked = false.
	/// X deals 20 damage to Y, who has 20 Block. WasFullyBlocked = true.
	/// X deals 20 damage to Y, who has 0 Block.  WasFullyBlocked = false.
	/// </example>
	public bool WasFullyBlocked { get; set; }

	/// <summary>
	/// Was the creature that took damage killed by it?
	/// Note: This will return true even if the creature was resurrected afterwards (Fairy in a Bottle, etc.).
	/// </summary>
	/// <example>
	/// X deals 10 damage to Y, who has 15 HP. WasTargetKilled = false.
	/// X deals 20 damage to Y, who has 15 HP. WasTargetKilled = true.
	/// X deals 20 damage to Y, who has 20 HP. WasTargetKilled = true.
	/// X deals 20 damage to Y, who has 20 HP and Fairy in a Bottle. WasTargetKilled = true.
	/// </example>
	public bool WasTargetKilled { get; init; }

	public DamageResult(Creature receiver, ValueProp props)
	{
		Receiver = receiver;
		Props = props;
	}
}
