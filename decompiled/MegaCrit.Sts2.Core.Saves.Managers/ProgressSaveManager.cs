using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.Migrations;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Saves.Validation;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;
using Sentry;

namespace MegaCrit.Sts2.Core.Saves.Managers;

public class ProgressSaveManager
{
	public const string fileName = "progress.save";

	private readonly ISaveStore _saveStore;

	private readonly MigrationManager _migrationManager;

	private readonly IProfileIdProvider _profileIdProvider;

	private static HashSet<ModelId>? _eliteEncounters;

	public ProgressState Progress { get; set; } = ProgressState.CreateDefault();

	public ProgressSaveManager(int profileId, ISaveStore saveStore, MigrationManager migrationManager)
		: this(saveStore, migrationManager, new StaticProfileIdProvider(profileId))
	{
	}

	public ProgressSaveManager(ISaveStore saveStore, MigrationManager migrationManager, IProfileIdProvider profileIdProvider)
	{
		_saveStore = saveStore;
		_migrationManager = migrationManager;
		_profileIdProvider = profileIdProvider;
	}

	public static string GetProgressPathForProfile(int profileId)
	{
		return Path.Combine(UserDataPathProvider.GetProfileDir(profileId), UserDataPathProvider.SavesDir, "progress.save");
	}

	public void SaveProgress()
	{
		try
		{
			SerializableProgress serializableProgress = Progress.ToSerializable();
			serializableProgress.SchemaVersion = _migrationManager.GetLatestVersion<SerializableProgress>();
			string content = JsonSerializationUtility.ToJson(serializableProgress);
			_saveStore.WriteFile(GetProgressPathForProfile(_profileIdProvider.CurrentProfileId), content);
		}
		catch (Exception ex)
		{
			Log.Error($"Failed to save progress: {ex}");
			SentryService.CaptureException(ex);
		}
	}

	public ReadSaveResult<SerializableProgress> LoadProgress()
	{
		ReadSaveResult<SerializableProgress> readSaveResult = _migrationManager.LoadSave<SerializableProgress>(GetProgressPathForProfile(_profileIdProvider.CurrentProfileId));
		if (!readSaveResult.Success || readSaveResult.SaveData == null)
		{
			Progress = ProgressState.CreateDefault();
			Ironclad ironclad = ModelDb.Character<Ironclad>();
			foreach (CardModel item in ironclad.StartingDeck)
			{
				Progress.MarkCardAsSeen(item.Id);
			}
			foreach (RelicModel startingRelic in ironclad.StartingRelics)
			{
				Progress.MarkRelicAsSeen(startingRelic.Id);
			}
			SaveProgress();
		}
		else
		{
			DeserializationContext deserializationContext = new DeserializationContext();
			Progress = ProgressState.FromSerializable(readSaveResult.SaveData, deserializationContext);
			foreach (ValidationError error in deserializationContext.Errors)
			{
				Log.Warn($"Progress parse: {error}");
			}
		}
		return readSaveResult;
	}

	/// <summary>
	/// Generate an unlock state from the current progress save.
	/// </summary>
	public UnlockState GenerateUnlockState()
	{
		return new UnlockState(Progress);
	}

	/// <summary>
	/// Called after a loss. Marks the player as having lost to an encounter (not a monster).
	/// </summary>
	private void IncrementEncounterLoss(ModelId characterId, ModelId encounterId)
	{
		EncounterStats orCreateEncounterStats = Progress.GetOrCreateEncounterStats(encounterId);
		FightStats fightStats = orCreateEncounterStats.FightStats.FirstOrDefault((FightStats f) => f.Character == characterId);
		if (fightStats == null)
		{
			Log.Info($"{characterId} fought {encounterId} for the first time and LOST :(");
			FightStats item = new FightStats
			{
				Character = characterId,
				Wins = 0,
				Losses = 1
			};
			orCreateEncounterStats.FightStats.Add(item);
		}
		else
		{
			orCreateEncounterStats.IncrementLoss(characterId);
		}
	}

	/// <summary>
	/// Called after a loss. Marks the player as having lost to a specific monster in an encounter.
	/// </summary>
	private void IncrementEnemyFightLoss(ModelId characterId, ModelId monster)
	{
		EnemyStats orCreateEnemyStats = Progress.GetOrCreateEnemyStats(monster);
		FightStats fightStats = orCreateEnemyStats.FightStats.FirstOrDefault((FightStats f) => f.Character == characterId);
		if (fightStats == null)
		{
			Log.Info($"{characterId} fought {monster} for the first time and LOST >:(");
			FightStats item = new FightStats
			{
				Character = characterId,
				Wins = 0,
				Losses = 1
			};
			orCreateEnemyStats.FightStats.Add(item);
		}
		else
		{
			orCreateEnemyStats.IncrementLoss(characterId);
		}
	}

	public void MarkPotionAsSeen(PotionModel potion)
	{
		if (Progress.MarkPotionAsSeen(potion.Id) && LocalContext.IsMine(potion))
		{
			potion.Owner.DiscoveredPotions.Add(potion.Id);
		}
	}

	public void MarkCardAsSeen(CardModel card)
	{
		if (Progress.MarkCardAsSeen(card.Id) && LocalContext.IsMine(card))
		{
			card.Owner.DiscoveredCards.Add(card.Id);
		}
	}

	public void MarkRelicAsSeen(RelicModel relic)
	{
		if (Progress.MarkRelicAsSeen(relic.Id) && LocalContext.IsMine(relic))
		{
			relic.Owner.DiscoveredRelics.Add(relic.Id);
		}
	}

	/// <summary>
	/// Called whenever a run is completed. This could be Death, Victory, or Abandon.
	/// </summary>
	public void UpdateWithRunData(SerializableRun serializableRun, bool victory)
	{
		bool flag = serializableRun.Players.Count == 1;
		bool flag2 = serializableRun.GameMode == GameMode.Daily;
		bool flag3 = serializableRun.GameMode == GameMode.Custom;
		SerializablePlayer serializablePlayer;
		if (flag)
		{
			serializablePlayer = serializableRun.Players.First();
		}
		else
		{
			ulong playerId = PlatformUtil.GetLocalPlayerId(serializableRun.PlatformType);
			serializablePlayer = serializableRun.Players.FirstOrDefault((SerializablePlayer p) => p.NetId == playerId);
			if (serializablePlayer == null)
			{
				Log.Warn($"Local player with net id {playerId} not found in run! Progress will not be updated");
				return;
			}
		}
		for (int num = 0; num < serializableRun.MapPointHistory.Count; num++)
		{
			if (num >= serializableRun.Acts.Count)
			{
				Log.Warn($"There are {serializableRun.MapPointHistory.Count} acts in the map point history, but {serializableRun.Acts.Count} acts in the act array! This is unexpected");
			}
			else
			{
				ModelId id = serializableRun.Acts[num].Id;
				Progress.MarkActAsSeen(id);
			}
		}
		List<MapPointHistoryEntry> list = serializableRun.MapPointHistory.SelectMany((List<MapPointHistoryEntry> act) => act).ToList();
		Progress.TotalPlaytime += ((serializableRun.WinTime > 0) ? serializableRun.WinTime : serializableRun.RunTime);
		Progress.WongoPoints += serializablePlayer.ExtraFields.WongoPoints;
		Progress.TestSubjectKills += serializableRun.ExtraFields.TestSubjectKills;
		Progress.FloorsClimbed += list.Count;
		CharacterStats orCreateCharacterStats = Progress.GetOrCreateCharacterStats(serializablePlayer.CharacterId);
		orCreateCharacterStats.Playtime += ((serializableRun.WinTime > 0) ? serializableRun.WinTime : serializableRun.RunTime);
		if (victory)
		{
			int num2 = ScoreUtility.CalculateScore(serializableRun, victory);
			Progress.ArchitectDamage += num2;
			if (!flag2 && !flag3)
			{
				if (flag)
				{
					IncrementSingleplayerAscension(serializableRun, orCreateCharacterStats);
				}
				else
				{
					IncrementMultiplayerAscension(serializableRun);
				}
				orCreateCharacterStats.TotalWins++;
				orCreateCharacterStats.CurrentWinStreak++;
				orCreateCharacterStats.BestWinStreak = Math.Max(orCreateCharacterStats.BestWinStreak, orCreateCharacterStats.CurrentWinStreak);
				if (orCreateCharacterStats.FastestWinTime < 0 || orCreateCharacterStats.FastestWinTime > serializableRun.RunTime)
				{
					orCreateCharacterStats.FastestWinTime = serializableRun.RunTime;
				}
			}
		}
		else
		{
			if (!flag2 && !flag3)
			{
				orCreateCharacterStats.CurrentWinStreak = 0L;
				orCreateCharacterStats.TotalLosses++;
			}
			MapPointHistoryEntry mapPointHistoryEntry = serializableRun.MapPointHistory.LastOrDefault()?.LastOrDefault();
			if (mapPointHistoryEntry != null)
			{
				RoomType roomType = mapPointHistoryEntry.Rooms.Last().RoomType;
				if (roomType.IsCombatRoom())
				{
					ModelId modelId = mapPointHistoryEntry.Rooms.Last().ModelId;
					ModelId characterId = serializablePlayer.CharacterId;
					IncrementEncounterLoss(characterId, modelId);
					foreach (ModelId monsterId in mapPointHistoryEntry.Rooms.Last().MonsterIds)
					{
						IncrementEnemyFightLoss(characterId, monsterId);
					}
				}
			}
		}
		foreach (MapPointHistoryEntry item in list)
		{
			MapPointRoomHistoryEntry mapPointRoomHistoryEntry = item.FirstRoomOfType(RoomType.Event);
			if (mapPointRoomHistoryEntry != null)
			{
				Progress.MarkEventAsSeen(mapPointRoomHistoryEntry.ModelId);
			}
		}
		if (!flag2 && !flag3)
		{
			HashSet<ModelId> hashSet = serializablePlayer.Deck.Select((SerializableCard c) => c.Id).ToHashSet();
			foreach (ModelId item2 in hashSet)
			{
				CardStats orCreateCardStats = Progress.GetOrCreateCardStats(item2);
				if (victory)
				{
					orCreateCardStats.TimesWon++;
				}
				else
				{
					orCreateCardStats.TimesLost++;
				}
			}
			foreach (MapPointHistoryEntry item3 in list)
			{
				PlayerMapPointHistoryEntry entry = item3.GetEntry(serializablePlayer.NetId);
				foreach (CardChoiceHistoryEntry cardChoice in entry.CardChoices)
				{
					CardStats orCreateCardStats2 = Progress.GetOrCreateCardStats(cardChoice.Card.Id);
					if (cardChoice.wasPicked)
					{
						orCreateCardStats2.TimesPicked++;
					}
					else
					{
						orCreateCardStats2.TimesSkipped++;
					}
				}
				if (item3.MapPointType == MapPointType.Ancient)
				{
					MapPointRoomHistoryEntry mapPointRoomHistoryEntry2 = item3.FirstRoomOfType(RoomType.Event);
					AncientStats orCreateAncientStats = Progress.GetOrCreateAncientStats(mapPointRoomHistoryEntry2.ModelId);
					if (victory)
					{
						orCreateAncientStats.IncrementWin(serializablePlayer.CharacterId);
					}
					else
					{
						orCreateAncientStats.IncrementLoss(serializablePlayer.CharacterId);
					}
				}
			}
		}
		UpdateEpochsPostRun(serializablePlayer, serializableRun, victory);
		SaveProgress();
	}

	/// <summary>
	/// Check for Epoch-related unlocks at the end of each run.
	/// Singleplayer only.
	/// </summary>
	private void UpdateEpochsPostRun(SerializablePlayer serializablePlayer, SerializableRun serializableRun, bool victory)
	{
		TryObtainEpochPostRun(EpochModel.Get<NeowEpoch>(), serializablePlayer, serializableRun);
		PostRunUnlockCharacterEpochCheck(serializablePlayer, serializableRun);
		PostRunCharacterEpochChecks(serializablePlayer, serializableRun, victory);
		int num = ModelDb.AllCharacters.Count();
		if (victory && Progress.CharacterStats.Count >= num)
		{
			TryObtainEpochPostRun(EpochModel.Get<DailyRunEpoch>(), serializablePlayer, serializableRun);
		}
		if (Progress.CharacterStats.Count >= num)
		{
			TryObtainEpochPostRun(EpochModel.Get<OrobasEpoch>(), serializablePlayer, serializableRun);
		}
		if (ModelDb.AllAncients.All((AncientEventModel a) => Progress.AncientStats.ContainsKey(a.Id) || a is Darv))
		{
			TryObtainEpochPostRun(EpochModel.Get<DarvEpoch>(), serializablePlayer, serializableRun);
		}
	}

	/// <summary>
	/// If you complete a run with a character for the first time,
	/// there's generally an unlock for another character.
	/// </summary>
	private void PostRunUnlockCharacterEpochCheck(SerializablePlayer serializablePlayer, SerializableRun serializableRun)
	{
		CharacterModel byId = ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId);
		string text = ((byId is Silent) ? EpochModel.GetId<Regent1Epoch>() : ((byId is Regent) ? EpochModel.GetId<Necrobinder1Epoch>() : ((!(byId is Necrobinder)) ? null : EpochModel.GetId<Defect1Epoch>())));
		string text2 = text;
		if (text2 != null && TryObtainEpochPostRun(EpochModel.Get(text2), serializablePlayer, serializableRun))
		{
			Log.Info($"Epoch obtained for playing a run as {serializablePlayer.CharacterId}");
		}
	}

	private void PostRunCharacterEpochChecks(SerializablePlayer serializablePlayer, SerializableRun serializableRun, bool victory)
	{
		if (victory)
		{
			CheckAscensionOneCompleted(serializablePlayer, serializableRun);
			string id = EpochModel.GetId<CustomAndSeedsEpoch>();
			if (Progress.Wins >= 3)
			{
				TryObtainEpochPostRun(EpochModel.Get(id), serializablePlayer, serializableRun);
			}
		}
	}

	/// <summary>
	/// When the player completes Ascension 1 they unlock Character Epoch 7.
	/// This unlock grants the player the means to Kill the Architect and 3 cards.
	/// </summary>
	private void CheckAscensionOneCompleted(SerializablePlayer serializablePlayer, SerializableRun serializableRun)
	{
		if (serializableRun.Ascension == 1)
		{
			CharacterModel byId = ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId);
			string text = ((byId is Ironclad) ? EpochModel.GetId<Ironclad7Epoch>() : ((byId is Silent) ? EpochModel.GetId<Silent7Epoch>() : ((byId is Regent) ? EpochModel.GetId<Regent7Epoch>() : ((byId is Defect) ? EpochModel.GetId<Defect7Epoch>() : ((!(byId is Necrobinder)) ? null : EpochModel.GetId<Necrobinder7Epoch>())))));
			string text2 = text;
			if (text2 != null)
			{
				TryObtainEpochPostRun(EpochModel.Get(text2), serializablePlayer, serializableRun);
			}
		}
	}

	/// <summary>
	/// Checks for Character5Epoch for all characters.
	/// Requires defeating 15 Elites in the game with the character.
	/// </summary>
	private void CheckFifteenElitesDefeatedEpoch(Player localPlayer)
	{
		CharacterModel character = localPlayer.Character;
		EpochModel epochModel;
		if (!(character is Ironclad))
		{
			if (!(character is Silent))
			{
				if (!(character is Regent))
				{
					if (!(character is Defect))
					{
						if (!(character is Necrobinder))
						{
							if (!(character is Deprived))
							{
								throw new ArgumentOutOfRangeException("character", character, null);
							}
							epochModel = null;
						}
						else
						{
							epochModel = EpochModel.Get(EpochModel.GetId<Necrobinder5Epoch>());
						}
					}
					else
					{
						epochModel = EpochModel.Get(EpochModel.GetId<Defect5Epoch>());
					}
				}
				else
				{
					epochModel = EpochModel.Get(EpochModel.GetId<Regent5Epoch>());
				}
			}
			else
			{
				epochModel = EpochModel.Get(EpochModel.GetId<Silent5Epoch>());
			}
		}
		else
		{
			epochModel = EpochModel.Get(EpochModel.GetId<Ironclad5Epoch>());
		}
		EpochModel epochModel2 = epochModel;
		if (epochModel2 == null)
		{
			return;
		}
		HashSet<ModelId> eliteEncounters = GetEliteEncounters();
		int num = 0;
		foreach (EncounterStats value in Progress.EncounterStats.Values)
		{
			if (!eliteEncounters.Contains(value.Id))
			{
				continue;
			}
			foreach (FightStats fightStat in value.FightStats)
			{
				if (fightStat.Character == character.Id)
				{
					num += fightStat.Wins;
					break;
				}
			}
		}
		Log.Info($"Elites Defeated: {num}/{eliteEncounters.Count}");
		if (num >= 15)
		{
			TryObtainEpochMidRun(epochModel2, localPlayer);
		}
	}

	/// <summary>
	/// Checks for Character6Epoch for all characters.
	/// Requires defeating 15 Bosses in the game with the character.
	/// </summary>
	private void CheckFifteenBossesDefeatedEpoch(Player localPlayer)
	{
		CharacterModel character = localPlayer.Character;
		EpochModel epochModel;
		if (!(character is Ironclad))
		{
			if (!(character is Silent))
			{
				if (!(character is Regent))
				{
					if (!(character is Defect))
					{
						if (!(character is Necrobinder))
						{
							if (!(character is Deprived))
							{
								throw new ArgumentOutOfRangeException("character", character, null);
							}
							epochModel = null;
						}
						else
						{
							epochModel = EpochModel.Get(EpochModel.GetId<Necrobinder6Epoch>());
						}
					}
					else
					{
						epochModel = EpochModel.Get(EpochModel.GetId<Defect6Epoch>());
					}
				}
				else
				{
					epochModel = EpochModel.Get(EpochModel.GetId<Regent6Epoch>());
				}
			}
			else
			{
				epochModel = EpochModel.Get(EpochModel.GetId<Silent6Epoch>());
			}
		}
		else
		{
			epochModel = EpochModel.Get(EpochModel.GetId<Ironclad6Epoch>());
		}
		EpochModel epochModel2 = epochModel;
		if (epochModel2 == null)
		{
			return;
		}
		HashSet<ModelId> hashSet = ModelDb.Acts.SelectMany((ActModel a) => a.AllBossEncounters.Select((EncounterModel e) => e.Id)).ToHashSet();
		int num = 0;
		foreach (EncounterStats value in Progress.EncounterStats.Values)
		{
			if (!hashSet.Contains(value.Id))
			{
				continue;
			}
			foreach (FightStats fightStat in value.FightStats)
			{
				if (fightStat.Character == character.Id)
				{
					num += fightStat.Wins;
					break;
				}
			}
		}
		if (num >= 15)
		{
			TryObtainEpochMidRun(epochModel2, localPlayer);
		}
	}

	/// <summary>
	/// Helper function to obtain character unlocks 1, 2, and 3.
	/// Requires the player to defeat a boss in an act for the first time (for each character!)
	/// </summary>
	private void ObtainCharUnlockEpoch(Player localPlayer, int act)
	{
		if (localPlayer.Character.IsPlayable)
		{
			string text = localPlayer.Character.Id.Entry.ToUpperInvariant();
			EpochModel epochModel = null;
			switch (act)
			{
			case 0:
				epochModel = EpochModel.Get(text + "2_EPOCH");
				break;
			case 1:
				epochModel = EpochModel.Get(text + "3_EPOCH");
				break;
			case 2:
				epochModel = EpochModel.Get(text + "4_EPOCH");
				break;
			case 3:
				Log.Error($"Act {act + 1} is not yet implemented.");
				break;
			default:
				Log.Error($"Unsupported Act: {act}");
				break;
			}
			if (epochModel == null)
			{
				Log.Error("EpochModel was not found :(");
			}
			else if (TryObtainEpochMidRun(epochModel, localPlayer))
			{
				Log.Info($"Epoch obtained for completing Act {act + 1}");
			}
		}
	}

	/// <summary>
	/// Called mid-run (like when you kill a boss that unlocks an Epoch).
	/// If this Epoch's slot is available, we obtain it and return true.
	/// We return a bool so callers can log or show VFX accordingly.
	/// </summary>
	/// <param name="epoch">Epoch to try to obtain.</param>
	/// <param name="localPlayer">The local player's Player instance.</param>
	/// <returns>
	/// True if you Obtained a new Epoch, false if not (you already have it OR the slot is unavailable).
	/// </returns>
	private bool TryObtainEpochMidRun(EpochModel epoch, Player localPlayer)
	{
		if (localPlayer.RunState.GameMode.AreAchievementsAndEpochsLocked())
		{
			return false;
		}
		if (!TryObtainEpochInternal(epoch))
		{
			return false;
		}
		localPlayer.DiscoveredEpochs.Add(epoch.Id);
		return true;
	}

	/// <summary>
	/// Called post-run (like after abandoning your first run, which unlocks the Silent).
	/// If this Epoch's slot is available, we obtain it and return true.
	/// We return a bool so callers can log or show VFX accordingly.
	/// </summary>
	/// <param name="epoch">Epoch to try to obtain.</param>
	/// <param name="serializablePlayer">The serializable version of the local player's Player instance.</param>
	/// <param name="serializableRun">The serializable version of the run.</param>
	/// <returns>
	/// True if you Obtained a new Epoch, false if not (you already have it OR the slot is unavailable).
	/// </returns>
	private bool TryObtainEpochPostRun(EpochModel epoch, SerializablePlayer serializablePlayer, SerializableRun serializableRun)
	{
		if (serializableRun.GameMode.AreAchievementsAndEpochsLocked())
		{
			return false;
		}
		if (!TryObtainEpochInternal(epoch))
		{
			return false;
		}
		serializablePlayer.DiscoveredEpochs.Add(epoch.Id);
		return true;
	}

	/// <summary>
	/// WARNING: This should only be called by <see cref="M:MegaCrit.Sts2.Core.Saves.Managers.ProgressSaveManager.TryObtainEpochMidRun(MegaCrit.Sts2.Core.Timeline.EpochModel,MegaCrit.Sts2.Core.Entities.Players.Player)" /> and
	/// <see cref="M:MegaCrit.Sts2.Core.Saves.Managers.ProgressSaveManager.TryObtainEpochPostRun(MegaCrit.Sts2.Core.Timeline.EpochModel,MegaCrit.Sts2.Core.Saves.Runs.SerializablePlayer,MegaCrit.Sts2.Core.Saves.SerializableRun)" />.
	/// If this Epoch's slot is available, we obtain it and return true.
	/// We return a bool so callers can log or show VFX accordingly.
	/// </summary>
	/// <param name="epoch">Epoch to try to obtain.</param>
	/// <returns>
	/// True if you Obtained a new Epoch, false if not (you already have it OR the slot is unavailable).
	/// </returns>
	private bool TryObtainEpochInternal(EpochModel epoch)
	{
		if (Progress.IsEpochObtained(epoch.Id))
		{
			Log.Info("Player already has Epoch: " + epoch.Id);
			return false;
		}
		Progress.ObtainEpoch(epoch.Id);
		NGame.Instance?.AddChildSafely(NGainEpochVfx.Create(epoch));
		if (GetRevealableEpochs().All((SerializableEpoch e) => e.Id != epoch.Id))
		{
			string text = "Epoch " + epoch.Id + " was obtained, but is not yet revealable by the player!";
			Log.Warn(text);
			SentryService.CaptureMessage(text, SentryLevel.Info, delegate(Scope scope)
			{
				string text2 = JsonSerializer.Serialize(Progress.Epochs, JsonSerializationUtility.GetTypeInfo<List<SerializableEpoch>>());
				scope.AddCompressedAttachment(text2, "epochs.txt.gz");
			});
		}
		return true;
	}

	/// <summary>
	/// Returns the set of epochs that the player has both obtained and may reveal.
	/// In certain (buggy) scenarios, the player may end up obtaining epochs that are still hidden. An example scenario
	/// (though this may be fixed in the future) is:
	///  - The player joins a multiplayer daily, which allows them to play a character they have not unlocked
	///  - The player obtains an epoch for that character which they can't see on the timeline
	///
	/// This method skips those sorts of epochs. However, it still returns epochs that are in ObtainedNoSlot state but
	/// are about to be revealed by another epoch the player has obtained.
	/// </summary>
	public IEnumerable<SerializableEpoch> GetRevealableEpochs()
	{
		HashSet<string> satisfiedEpochIds = new HashSet<string>(from e in Progress.Epochs.Where(delegate(SerializableEpoch e)
			{
				EpochState state2 = e.State;
				return (uint)(state2 - 3) <= 1u;
			})
			select e.Id);
		HashSet<string> reachableSet = new HashSet<string>();
		Queue<string> queue = new Queue<string>();
		string id = EpochModel.Get<NeowEpoch>().Id;
		reachableSet.Add(id);
		queue.Enqueue(id);
		foreach (SerializableEpoch epoch in Progress.Epochs)
		{
			bool flag = epoch.Id == id;
			bool flag2 = flag;
			if (!flag2)
			{
				EpochState state = epoch.State;
				bool flag3 = ((state == EpochState.None || (uint)(state - 2) <= 1u) ? true : false);
				flag2 = flag3;
			}
			if (!flag2)
			{
				reachableSet.Add(epoch.Id);
				queue.Enqueue(epoch.Id);
			}
		}
		while (queue.Count > 0)
		{
			string id2 = queue.Dequeue();
			EpochModel[] timelineExpansion = EpochModel.Get(id2).GetTimelineExpansion();
			foreach (EpochModel epochModel in timelineExpansion)
			{
				if (!reachableSet.Contains(epochModel.Id))
				{
					reachableSet.Add(epochModel.Id);
					if (satisfiedEpochIds.Contains(epochModel.Id))
					{
						queue.Enqueue(epochModel.Id);
					}
				}
			}
		}
		return Progress.Epochs.Where((SerializableEpoch e) => satisfiedEpochIds.Contains(e.Id) && reachableSet.Contains(e.Id));
	}

	/// <summary>
	/// Lots of checks and update of the progress save file after the end of each combat.
	/// </summary>
	public void UpdateAfterCombatWon(Player localPlayer, CombatRoom room)
	{
		CombatState combatState = room.CombatState;
		IRunState runState = combatState.RunState;
		CharacterModel character = localPlayer.Character;
		ModelId id = combatState.Encounter.Id;
		EncounterStats orCreateEncounterStats = Progress.GetOrCreateEncounterStats(id);
		FightStats fightStats = orCreateEncounterStats.FightStats.FirstOrDefault((FightStats f) => f.Character == character.Id);
		if (fightStats == null)
		{
			Log.Info($"{character.Id} fought {id} for the first time and WON >:)");
			FightStats item = new FightStats
			{
				Character = character.Id,
				Wins = 1,
				Losses = 0
			};
			orCreateEncounterStats.FightStats.Add(item);
		}
		else
		{
			orCreateEncounterStats.IncrementWin(character.Id);
		}
		if (room.RoomType == RoomType.Boss)
		{
			ObtainCharUnlockEpoch(localPlayer, runState.CurrentActIndex);
			CheckFifteenBossesDefeatedEpoch(localPlayer);
		}
		else if (room.RoomType == RoomType.Elite)
		{
			CheckFifteenElitesDefeatedEpoch(localPlayer);
		}
		foreach (MonsterModel spawnedEnemy in room.Encounter.SpawnedEnemies)
		{
			EnemyStats orCreateEnemyStats = Progress.GetOrCreateEnemyStats(spawnedEnemy.Id);
			bool flag = orCreateEnemyStats.FightStats.Count == 0;
			FightStats fightStats2 = orCreateEnemyStats.FightStats.FirstOrDefault((FightStats f) => f.Character == character.Id);
			if (fightStats2 == null)
			{
				Log.Info($"{character.Id} fought {spawnedEnemy.Id} for the first time and WON >:(");
				FightStats item2 = new FightStats
				{
					Character = character.Id,
					Wins = 1,
					Losses = 0
				};
				orCreateEnemyStats.FightStats.Add(item2);
				if (flag)
				{
					localPlayer.DiscoveredEnemies.Add(spawnedEnemy.Id);
				}
			}
			else
			{
				orCreateEnemyStats.IncrementWin(character.Id);
			}
		}
	}

	/// <summary>
	/// If the player wins a singleplayer run, this logic checks if they're playing on Max Ascension.
	/// If so, increment it by 1.
	/// </summary>
	private static void IncrementSingleplayerAscension(SerializableRun run, CharacterStats charStats)
	{
		if (run.Ascension == charStats.MaxAscension)
		{
			if (charStats.MaxAscension < 10)
			{
				charStats.MaxAscension++;
				charStats.PreferredAscension = charStats.MaxAscension;
			}
		}
		else
		{
			Log.Info($"Not playing on max singleplayer ascension ({charStats.MaxAscension})");
		}
	}

	/// <summary>
	/// If the player wins a multiplayer run, this logic checks if they're playing on Max Ascension.
	/// If so, increment it by 1.
	/// </summary>
	private void IncrementMultiplayerAscension(SerializableRun run)
	{
		if (run.Ascension == Progress.MaxMultiplayerAscension)
		{
			if (Progress.MaxMultiplayerAscension < 10)
			{
				Progress.MaxMultiplayerAscension++;
				Progress.PreferredMultiplayerAscension = Progress.MaxMultiplayerAscension;
			}
		}
		else
		{
			Log.Info($"Not playing on max multiplayer ascension ({Progress.MaxMultiplayerAscension})");
		}
	}

	/// <summary>
	/// Returns true if all ftues are disabled OR if the given ftue key exists (seen by the player before).
	/// Is also disabled if there's no game (test mode)
	/// </summary>
	public bool SeenFtue(string ftueKey)
	{
		if (!Progress.EnableFtues)
		{
			return true;
		}
		return Progress.FtueCompleted.Contains(ftueKey);
	}

	/// <summary>
	/// Checks if a one-time popup has been seen before or not.
	/// This is only used by the "Ascension Unlocked" popups for now.
	/// </summary>
	public bool SeenPopup(string popupKey)
	{
		if (TestMode.IsOn)
		{
			return true;
		}
		return Progress.FtueCompleted.Contains(popupKey);
	}

	public void MarkFtueAsComplete(string ftueId)
	{
		if (Progress.MarkFtueAsComplete(ftueId))
		{
			Log.Info("Player has seen ftue " + ftueId + "!");
			SaveProgress();
		}
	}

	public void SetFtuesEnabled(bool enabled)
	{
		if (Progress.EnableFtues != enabled)
		{
			Log.Info($"Player has set FTUEs enabled: {enabled}");
			Progress.EnableFtues = enabled;
			SaveProgress();
		}
	}

	public void ResetFtues()
	{
		Log.Info("Player has reset FTUEs to enabled");
		Progress.ResetFtues();
		SaveProgress();
	}

	private static HashSet<ModelId> GetEliteEncounters()
	{
		return _eliteEncounters ?? (_eliteEncounters = (from e in ModelDb.AllEncounters
			where e.RoomType == RoomType.Elite
			select e.Id).Distinct().ToHashSet());
	}
}
