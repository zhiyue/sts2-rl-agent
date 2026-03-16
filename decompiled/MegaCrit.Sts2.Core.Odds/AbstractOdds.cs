using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Odds;

public abstract class AbstractOdds(float initialValue, Rng rng)
{
	protected readonly Rng _rng = rng;

	public float CurrentValue { get; protected set; } = initialValue;

	public void OverrideCurrentValue(float newValue)
	{
		CurrentValue = newValue;
	}
}
