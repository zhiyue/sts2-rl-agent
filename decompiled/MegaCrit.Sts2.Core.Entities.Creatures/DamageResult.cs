using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Entities.Creatures;

public class DamageResult
{
	public Creature Receiver { get; }

	public ValueProp Props { get; }

	public int BlockedDamage { get; set; }

	public int UnblockedDamage { get; init; }

	public int OverkillDamage { get; init; }

	public int TotalDamage => BlockedDamage + UnblockedDamage;

	public bool WasBlockBroken { get; set; }

	public bool WasFullyBlocked { get; set; }

	public bool WasTargetKilled { get; init; }

	public DamageResult(Creature receiver, ValueProp props)
	{
		Receiver = receiver;
		Props = props;
	}
}
