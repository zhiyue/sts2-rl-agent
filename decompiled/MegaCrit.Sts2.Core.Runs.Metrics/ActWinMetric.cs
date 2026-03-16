namespace MegaCrit.Sts2.Core.Runs.Metrics;

public struct ActWinMetric
{
	public readonly string act;

	public readonly bool win;

	public ActWinMetric(string actId, bool win)
	{
		act = actId;
		this.win = win;
	}
}
