namespace MegaCrit.Sts2.Core.Runs.Metrics;

public struct EncounterMetric
{
	public readonly string id;

	public readonly int damage;

	public readonly int turns;

	public EncounterMetric(string id, int damage, int turns)
	{
		this.id = id;
		this.damage = damage;
		this.turns = turns;
	}
}
