using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Odds;

public abstract class AbstractOdds(float initialValue, Rng rng)
{
	protected readonly Rng _rng = rng;

	public float CurrentValue { get; protected set; } = initialValue;

	/// <summary>
	/// Sets CurrentValue. Should be used only in instances where we're loading the player from another source, such as
	/// multiplayer host.
	/// </summary>
	/// <param name="newValue">The new value.</param>
	public void OverrideCurrentValue(float newValue)
	{
		CurrentValue = newValue;
	}
}
