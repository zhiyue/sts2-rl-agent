using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models.Badges;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Runs;

/// <summary>
/// Helper functions for calculating score, badges, and the score for daily leaderboards (Also called score, very confusing!)
/// </summary>
public static class ScoreUtility
{
	/// <summary>
	/// A sentinel value for clients in a multiplayer run to submit when uploading a daily run score.
	/// For more information on why this is needed, see <see cref="M:MegaCrit.Sts2.Core.Daily.DailyRunUtility.ShouldUploadScore(MegaCrit.Sts2.Core.Leaderboard.ILeaderboardHandle,System.Collections.Generic.IReadOnlyList{System.UInt64},System.Threading.CancellationToken)" />.
	/// </summary>
	public const int clientScore = -999999999;

	/// <summary>
	/// Calculates the score for a given <see cref="T:MegaCrit.Sts2.Core.Runs.IRunState" />. Only called in the Architect room.
	/// </summary>
	/// <param name="runState">The run that was just completed.</param>
	/// <param name="won">True if the run ended in a victory.</param>
	/// <returns>The final score for the run.</returns>
	public static int CalculateScore(IRunState runState, bool won)
	{
		return CalculateScore(runState.MapPointHistory, runState.AscensionLevel, won, runState.Players.Count);
	}

	/// <summary>
	/// Calculates the score for a given <see cref="T:MegaCrit.Sts2.Core.Saves.SerializableRun" />.
	/// </summary>
	/// <param name="run">A serializable version of the run that was just completed.</param>
	/// <param name="won">True if the run ended in a victory.</param>
	/// <returns>The final score for the run.</returns>
	public static int CalculateScore(SerializableRun run, bool won)
	{
		return CalculateScore(run.MapPointHistory, run.Ascension, won, run.Players.Count);
	}

	private static int CalculateScore(IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> history, int ascension, bool won, int playerCount)
	{
		int num = 0;
		num += GetScoreForFloor(history);
		num += GetScoreForGoldGained(history, playerCount);
		num += GetScoreForElitesKilled(GetElitesKilledCount(history));
		num += GetScoreForBossesSlain(GetBossesSlainCount(history, won));
		return (int)((double)num * (1.0 + (double)ascension * 0.1));
	}

	/// <summary>
	/// Helper function to calculate score based on total floors climbed.
	/// Useful as we display the score on the score line as well.
	/// </summary>
	public static int GetScoreForFloor(IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> history)
	{
		int num = 0;
		int count = history.Count;
		for (int i = 0; i < count; i++)
		{
			num += history[i].Count * 10 * (i + 1);
		}
		return num;
	}

	/// <summary>
	/// Counts the total number of elites the player killed during a run.
	/// </summary>
	public static int GetElitesKilledCount(IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> history)
	{
		List<MapPointRoomHistoryEntry> list = history.SelectMany((IReadOnlyList<MapPointHistoryEntry> actEntries) => actEntries).SelectMany((MapPointHistoryEntry e) => e.Rooms).ToList();
		int num = list.Count((MapPointRoomHistoryEntry r) => r.RoomType == RoomType.Elite);
		if (list.Count > 0 && list.Last().RoomType == RoomType.Elite)
		{
			num--;
		}
		return num;
	}

	/// <summary>
	/// Helper function to calculate score based on Elites killed.
	/// Useful as we display the score on the score line as well.
	/// </summary>
	public static int GetScoreForElitesKilled(int elitesKilled)
	{
		return elitesKilled * 50;
	}

	/// <summary>
	/// Counts the total number of bosses the player killed during a run.
	/// </summary>
	public static int GetBossesSlainCount(IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> history, bool won)
	{
		int num = 0;
		foreach (IReadOnlyList<MapPointHistoryEntry> item in history)
		{
			foreach (MapPointHistoryEntry item2 in item)
			{
				foreach (MapPointRoomHistoryEntry room in item2.Rooms)
				{
					bool flag = !won && item == history.Last() && item2 == item.Last() && room == item2.Rooms.Last();
					if (room.RoomType == RoomType.Boss && !flag)
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	/// <summary>
	/// Helper function to calculate score based on total bosses slain.
	/// Useful as we display the score on the score line as well.
	/// </summary>
	public static int GetScoreForBossesSlain(int bossCount)
	{
		return bossCount * 100;
	}

	public static int GetScoreForGoldGained(IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> history, int playerCount)
	{
		return history.SelectMany((IReadOnlyList<MapPointHistoryEntry> actEntries) => actEntries).SelectMany((MapPointHistoryEntry e) => e.PlayerStats).Sum((PlayerMapPointHistoryEntry p) => p.GoldGained) / (100 * playerCount);
	}

	/// <summary>
	/// Returns a list of badges that this run has obtained.
	/// </summary>
	public static List<Badge> GetBadges(SerializableRun run, ulong playerId, bool won)
	{
		List<Badge> list = new List<Badge>();
		foreach (Badge item in BadgePool.CreateAll(run, playerId, won))
		{
			if ((!item.RequiresWin || won) && (!item.MultiplayerOnly || run.Players.Count != 1) && item.IsObtained())
			{
				list.Add(item);
			}
		}
		return list;
	}

	/// <summary>
	/// Encodes the current run into a "daily upload score".
	/// This is greatly differentiated from the score used to unlock epochs. Cursed naming.
	/// </summary>
	public static int CalculateDailyScore(SerializableRun run, ulong localPlayerNetId, bool isVictory)
	{
		int num = ((!isVictory) ? 1 : 2);
		int num2 = Math.Clamp(run.FloorReached, 0, 99);
		int num3 = Math.Clamp(GetBadges(run, localPlayerNetId, isVictory).Count, 0, 99);
		int num4 = (int)Math.Clamp(isVictory ? run.WinTime : run.RunTime, 0L, 9999L);
		return num * 100000000 + num2 * 1000000 + num3 * 10000 + (9999 - num4);
	}

	/// <summary>
	/// Decodes our fancy encoded int into a struct.
	/// </summary>
	public static DecodedDailyScore DecodeDailyScore(int encodedScore)
	{
		int num = encodedScore / 100000000;
		int num2 = encodedScore / 1000000 % 100;
		int num3 = encodedScore / 10000 % 100;
		int num4 = 9999 - encodedScore % 10000;
		bool flag = (uint)(num - 1) <= 2u;
		bool isValid = flag && num2 >= 0 && num2 <= 99 && num3 >= 0 && num3 <= 99 && num4 >= 0 && num4 <= 9999;
		return new DecodedDailyScore
		{
			isValid = isValid,
			victory = num,
			floors = num2,
			badges = num3,
			runTime = num4
		};
	}
}
