using System.Collections.Generic;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Runs;

public static class ScoreUtility
{
	public const int clientScore = -99999;

	public static int CalculateScore(IRunState runState, bool won)
	{
		return CalculateScore(runState.MapPointHistory, runState.AscensionLevel, won);
	}

	public static int CalculateScore(SerializableRun run, bool won)
	{
		return CalculateScore(run.MapPointHistory, run.Ascension, won);
	}

	private static int CalculateScore(IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> history, int ascension, bool won)
	{
		int num = 0;
		int count = history.Count;
		for (int i = 0; i < count; i++)
		{
			num += history[i].Count * 10 * (i + 1);
		}
		if (won)
		{
			num += 300;
		}
		else if (count <= 2)
		{
			if (count > 1)
			{
				num += 100;
			}
		}
		else
		{
			num += 200;
		}
		return (int)((double)num * (1.0 + (double)ascension * 0.1));
	}
}
