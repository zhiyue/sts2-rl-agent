using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs;

public static class RunHistoryUtilities
{
	public static void CreateRunHistoryEntry(SerializableRun run, bool victory, bool isAbandoned, PlatformType platformType)
	{
		ModelId killedByEncounter = ModelId.none;
		ModelId killedByEvent = ModelId.none;
		MapPointHistoryEntry mapPointHistoryEntry = run.MapPointHistory.LastOrDefault()?.LastOrDefault();
		if (!victory && mapPointHistoryEntry != null)
		{
			RoomType roomType = mapPointHistoryEntry.Rooms.First().RoomType;
			if (roomType.IsCombatRoom())
			{
				killedByEncounter = mapPointHistoryEntry.Rooms.First().ModelId;
			}
			else if (roomType == RoomType.Event)
			{
				killedByEvent = mapPointHistoryEntry.Rooms.First().ModelId;
			}
		}
		List<RunHistoryPlayer> list = new List<RunHistoryPlayer>();
		foreach (SerializablePlayer player in run.Players)
		{
			RunHistoryPlayer item = new RunHistoryPlayer
			{
				Id = player.NetId,
				Character = player.CharacterId,
				Deck = player.Deck,
				Relics = player.Relics,
				Potions = player.Potions,
				MaxPotionSlotCount = player.MaxPotionSlotCount
			};
			list.Add(item);
		}
		RunHistory history = new RunHistory
		{
			BuildId = (ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? "NON-RELEASE-VERSION"),
			PlatformType = platformType,
			Players = list,
			GameMode = GetGameMode(run),
			Win = victory,
			KilledByEncounter = killedByEncounter,
			KilledByEvent = killedByEvent,
			WasAbandoned = isAbandoned,
			Seed = run.SerializableRng.Seed,
			StartTime = run.StartTime,
			RunTime = ((run.WinTime > 0) ? run.WinTime : run.RunTime),
			MapPointHistory = run.MapPointHistory,
			Ascension = run.Ascension,
			Acts = run.Acts.Select((SerializableActModel a) => a.Id).ToList(),
			Modifiers = run.Modifiers
		};
		SaveManager.Instance.SaveRunHistory(history);
		Log.Info("Created Run History entry!");
		if (RunManager.Instance.IsInProgress)
		{
			RunManager.Instance.History = history;
		}
	}

	private static GameMode GetGameMode(SerializableRun run)
	{
		if (run.DailyTime.HasValue)
		{
			return GameMode.Daily;
		}
		return GameMode.Standard;
	}
}
