using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Runs.Metrics;

public static class MetricUtilities
{
	private const string _metricsRunDataEndpoint = "https://sts2-metric-uploads.herokuapp.com/record_data/";

	private const string _metricsAchievementDataEndpoint = "https://sts2-metric-uploads.herokuapp.com/record_achievement/";

	private const string _metricsEpochDataEndpoint = "https://sts2-metric-uploads.herokuapp.com/record_epoch/";

	private const string _metricsSettingsDataEndpoint = "https://sts2-metric-uploads.herokuapp.com/record_settings/";

	private const int _runLengthThreshold = 5;

	public static void UploadRunMetrics(SerializableRun run, bool isVictory, ulong localPlayerId)
	{
		try
		{
			UploadRunMetricsInternal(run, isVictory, localPlayerId);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to upload run metrics: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private static bool ShouldUploadMetrics()
	{
		if (ReleaseInfoManager.Instance.ReleaseInfo == null)
		{
			Log.Info("Skipping metrics upload, this is a debug build.");
			return false;
		}
		if (!SaveManager.Instance.PrefsSave.UploadData)
		{
			Log.Info("Skipping metrics upload, user has upload data unset in settings");
			return false;
		}
		if (OS.HasFeature("editor"))
		{
			Log.Info("Skipping metrics upload since we're in the editor.");
			return false;
		}
		return true;
	}

	private static void UploadRunMetricsInternal(SerializableRun run, bool isVictory, ulong localPlayerId)
	{
		if (!ShouldUploadMetrics())
		{
			return;
		}
		if (RunManager.Instance.IsAbandoned)
		{
			Log.Info("Skipping metrics upload, run was abandoned.");
		}
		else if (ModManager.LoadedMods.Count > 0)
		{
			Log.Info("Skipping metrics upload, we're running modded.");
			ModManager.CallMetricsHooks(run, isVictory, localPlayerId);
		}
		else
		{
			if (run.Modifiers.Count != 0)
			{
				return;
			}
			List<MapPointHistoryEntry> list = run.MapPointHistory.SelectMany((List<MapPointHistoryEntry> logs) => logs).ToList();
			if (list.Count < 5)
			{
				return;
			}
			string language = LocManager.Instance.Language;
			LocManager.Instance.SetLanguage("eng");
			try
			{
				ModelId killedByEncounter = ModelId.none;
				MapPointHistoryEntry mapPointHistoryEntry = run.MapPointHistory.LastOrDefault()?.LastOrDefault();
				if (!isVictory && mapPointHistoryEntry != null && mapPointHistoryEntry.Rooms.Last().RoomType.IsCombatRoom())
				{
					killedByEncounter = mapPointHistoryEntry.Rooms.Last().ModelId;
				}
				SerializablePlayer localPlayer = run.Players.First((SerializablePlayer p) => p.NetId == localPlayerId);
				List<EncounterMetric> encounters = (from e in list
					where e.Rooms.Last().RoomType.IsCombatRoom()
					select new EncounterMetric(e.Rooms.Last().ModelId.Entry, int.Min(e.GetEntry(localPlayerId).DamageTaken, localPlayer.MaxHp), e.Rooms.Last().TurnsTaken + 1)).ToList();
				List<EventChoiceMetric> eventChoices = (from e in list
					where e.Rooms.First().RoomType == RoomType.Event && e.GetEntry(localPlayerId).EventChoices.Count > 0
					select new EventChoiceMetric(e, localPlayerId)).ToList();
				List<CardChoiceMetric> cardChoices = (from e in list
					where e.GetEntry(localPlayerId).CardChoices.Count > 0
					select new CardChoiceMetric(e.GetEntry(localPlayerId).CardChoices)).ToList();
				List<AncientMetric> ancientChoices = (from e in list
					where e.MapPointType == MapPointType.Ancient
					where e.GetEntry(localPlayerId).AncientChoices.Count > 0
					select new AncientMetric(e, e.GetEntry(localPlayerId))).ToList();
				List<ActWinMetric> list2 = new List<ActWinMetric>();
				for (int num = 0; num < run.MapPointHistory.Count; num++)
				{
					bool win = num < run.MapPointHistory.Count - 1 || isVictory;
					list2.Add(new ActWinMetric(run.Acts[num].Id.Entry, win));
				}
				ProgressState progress = SaveManager.Instance.Progress;
				RunMetrics run2 = new RunMetrics
				{
					Ascension = run.Ascension,
					TotalPlaytime = progress.TotalPlaytime,
					TotalWinRate = (float)progress.Wins / (float)progress.NumberOfRuns,
					BuildId = (ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? "NON-RELEASE-VERSION"),
					PlayerId = progress.UniqueId,
					Character = localPlayer.CharacterId,
					NumPlayers = run.Players.Count,
					Team = ((run.Players.Count > 1) ? run.Players.Select((SerializablePlayer p) => p.CharacterId).ToList() : new List<ModelId>()),
					Win = isVictory,
					FloorReached = list.Count,
					KilledByEncounter = killedByEncounter,
					Deck = localPlayer.Deck.Select((SerializableCard c) => c.Id),
					Relics = localPlayer.Relics.Select((SerializableRelic r) => r.Id),
					RunPlaytime = ((run.WinTime > 0) ? run.WinTime : run.RunTime),
					Encounters = encounters,
					CardChoices = cardChoices,
					EventChoices = eventChoices,
					AncientChoices = ancientChoices,
					ActWins = list2,
					CampfireUpgrades = (from c in list.Where((MapPointHistoryEntry e) => e.MapPointType == MapPointType.RestSite).SelectMany((MapPointHistoryEntry e) => e.GetEntry(localPlayerId).UpgradedCards)
						select c.Entry).ToList(),
					RelicBuys = (from r in list.SelectMany((MapPointHistoryEntry e) => e.GetEntry(localPlayerId).BoughtRelics)
						select r.Entry).ToList(),
					PotionBuys = (from p in list.SelectMany((MapPointHistoryEntry e) => e.GetEntry(localPlayerId).BoughtPotions)
						select p.Entry).ToList(),
					ColorlessBuys = (from c in list.SelectMany((MapPointHistoryEntry e) => e.GetEntry(localPlayerId).BoughtColorless)
						select c.Entry).ToList(),
					PotionDiscards = (from p in list.SelectMany((MapPointHistoryEntry e) => e.GetEntry(localPlayerId).PotionDiscarded)
						select p.Entry).ToList()
				};
				UploadRunMetrics(run2);
			}
			finally
			{
				LocManager.Instance.SetLanguage(language);
			}
		}
	}

	private static void UploadRunMetrics(RunMetrics run)
	{
		Log.Info("Uploading run metrics...");
		string json = JsonSerializer.Serialize(run, MetricsSerializerContext.Default.RunMetrics);
		TaskHelper.RunSafely(PutRequest("https://sts2-metric-uploads.herokuapp.com/record_data/", json, "Run metrics"));
	}

	public static void UploadAchievementMetric(Achievement achievement)
	{
		if (ShouldUploadMetrics())
		{
			AchievementMetric value = new AchievementMetric
			{
				BuildId = (ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? "NON-RELEASE-VERSION"),
				Achievement = achievement.ToString(),
				TotalRuns = SaveManager.Instance.Progress.NumberOfRuns,
				TotalPlaytime = SaveManager.Instance.Progress.TotalPlaytime,
				TotalAchievements = SaveManager.Instance.Progress.UnlockedAchievements.Count
			};
			string json = JsonSerializer.Serialize(value, MetricsSerializerContext.Default.AchievementMetric);
			Log.Info($"Uploading achievement metric for achievement {achievement}");
			TaskHelper.RunSafely(PutRequest("https://sts2-metric-uploads.herokuapp.com/record_achievement/", json, $"Achievement {achievement}"));
		}
	}

	public static void UploadEpochMetric(string epochId)
	{
		if (ShouldUploadMetrics())
		{
			EpochMetric value = new EpochMetric
			{
				BuildId = (ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? "NON-RELEASE-VERSION"),
				Epoch = epochId.Replace("_EPOCH", ""),
				TotalRuns = SaveManager.Instance.Progress.NumberOfRuns,
				TotalPlaytime = SaveManager.Instance.Progress.TotalPlaytime,
				TotalEpochs = SaveManager.Instance.Progress.Epochs.Count
			};
			string json = JsonSerializer.Serialize(value, MetricsSerializerContext.Default.EpochMetric);
			Log.Info("Uploading epoch metric for epoch " + epochId);
			TaskHelper.RunSafely(PutRequest("https://sts2-metric-uploads.herokuapp.com/record_epoch/", json, "Epoch " + epochId));
		}
	}

	public static void UploadSettingsMetric()
	{
		if (ShouldUploadMetrics())
		{
			long num = (long)OS.GetMemoryInfo()["physical"];
			SettingsDataMetric value = new SettingsDataMetric
			{
				BuildId = (ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? "NON-RELEASE-VERSION"),
				Os = OS.GetName(),
				Platform = "Steam",
				SystemRam = ((num == -1) ? (-1) : ((int)Math.Round((double)num / 1073741824.0))),
				LanguageCode = SaveManager.Instance.SettingsSave.Language,
				FastModeType = SaveManager.Instance.PrefsSave.FastMode,
				Screenshake = SaveManager.Instance.PrefsSave.ScreenShakeOptionIndex,
				ShowRunTimer = SaveManager.Instance.PrefsSave.ShowRunTimer,
				ShowCardIndices = SaveManager.Instance.PrefsSave.ShowCardIndices,
				DisplayCount = DisplayServer.GetScreenCount(),
				DisplayResolution = SaveManager.Instance.SettingsSave.WindowSize,
				Fullscreen = SaveManager.Instance.SettingsSave.Fullscreen,
				AspectRatio = SaveManager.Instance.SettingsSave.AspectRatioSetting,
				ResizeWindows = SaveManager.Instance.SettingsSave.ResizeWindows,
				VSync = SaveManager.Instance.SettingsSave.VSync,
				FpsLimit = SaveManager.Instance.SettingsSave.FpsLimit,
				Msaa = SaveManager.Instance.SettingsSave.Msaa
			};
			string json = JsonSerializer.Serialize(value, MetricsSerializerContext.Default.SettingsDataMetric);
			Log.Info("Completed 5 runs! Uploading settings metrics");
			TaskHelper.RunSafely(PutRequest("https://sts2-metric-uploads.herokuapp.com/record_settings/", json, "Settings data metrics"));
		}
	}

	private static async Task PutRequest(string url, string json, string context)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(json);
		using System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
		ByteArrayContent byteArrayContent = new ByteArrayContent(bytes);
		byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
		HttpResponseMessage httpResponseMessage;
		try
		{
			httpResponseMessage = await client.PutAsync(url, byteArrayContent);
		}
		catch (HttpRequestException ex)
		{
			Log.Warn("Metrics upload for '" + context + "' failed due to network error: " + ex.Message);
			return;
		}
		if (httpResponseMessage.IsSuccessStatusCode)
		{
			Log.Info("Metric for '" + context + "' successfully uploaded!");
			return;
		}
		Log.Warn($"Metrics upload request for '{context}' failed with status code: {httpResponseMessage.StatusCode}");
		Log.Info("Response body: " + await httpResponseMessage.Content.ReadAsStringAsync());
	}
}
