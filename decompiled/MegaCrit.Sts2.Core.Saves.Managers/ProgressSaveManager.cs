using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

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
		SerializableProgress serializableProgress = Progress.ToSerializable();
		serializableProgress.SchemaVersion = _migrationManager.GetLatestVersion<SerializableProgress>();
		string content = JsonSerializationUtility.ToJson(serializableProgress);
		_saveStore.WriteFile(GetProgressPathForProfile(_profileIdProvider.CurrentProfileId), content);
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

	public UnlockState GenerateUnlockState()
	{
		return new UnlockState(Progress);
	}

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

	public void UpdateWithRunData(SerializableRun serializableRun, bool victory)
	{
		bool flag = serializableRun.Players.Count == 1;
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
		else
		{
			orCreateCharacterStats.CurrentWinStreak = 0L;
			orCreateCharacterStats.TotalLosses++;
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
		HashSet<ModelId> hashSet = serializablePlayer.Deck.Select((SerializableCard c) => c.Id).ToHashSet();
		foreach (ModelId item in hashSet)
		{
			CardStats orCreateCardStats = Progress.GetOrCreateCardStats(item);
			if (victory)
			{
				orCreateCardStats.TimesWon++;
			}
			else
			{
				orCreateCardStats.TimesLost++;
			}
		}
		foreach (MapPointHistoryEntry item2 in list)
		{
			MapPointRoomHistoryEntry mapPointRoomHistoryEntry = item2.FirstRoomOfType(RoomType.Event);
			if (mapPointRoomHistoryEntry != null)
			{
				Progress.MarkEventAsSeen(mapPointRoomHistoryEntry.ModelId);
			}
			PlayerMapPointHistoryEntry entry = item2.GetEntry(serializablePlayer.NetId);
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
			if (item2.MapPointType == MapPointType.Ancient)
			{
				MapPointRoomHistoryEntry mapPointRoomHistoryEntry2 = item2.FirstRoomOfType(RoomType.Event);
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
		UpdateEpochsPostRun(serializablePlayer, serializableRun.Ascension, victory);
		SaveProgress();
	}

	private void UpdateEpochsPostRun(SerializablePlayer serializablePlayer, int ascension, bool victory)
	{
		TryObtainEpochPostRun(EpochModel.Get<NeowEpoch>(), serializablePlayer);
		PostRunUnlockCharacterEpochCheck(serializablePlayer);
		PostRunCharacterEpochChecks(serializablePlayer, ascension, victory);
		int num = ModelDb.AllCharacters.Count();
		if (victory && Progress.CharacterStats.Count >= num)
		{
			TryObtainEpochPostRun(EpochModel.Get<DailyRunEpoch>(), serializablePlayer);
		}
		if (Progress.CharacterStats.Count >= num)
		{
			TryObtainEpochPostRun(EpochModel.Get<OrobasEpoch>(), serializablePlayer);
		}
		if (ModelDb.AllAncients.All((AncientEventModel a) => Progress.AncientStats.ContainsKey(a.Id) || a is Darv))
		{
			TryObtainEpochPostRun(EpochModel.Get<DarvEpoch>(), serializablePlayer);
		}
	}

	private void PostRunUnlockCharacterEpochCheck(SerializablePlayer serializablePlayer)
	{
		CharacterModel byId = ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId);
		string text = ((byId is Silent) ? EpochModel.GetId<Regent1Epoch>() : ((byId is Regent) ? EpochModel.GetId<Necrobinder1Epoch>() : ((!(byId is Necrobinder)) ? null : EpochModel.GetId<Defect1Epoch>())));
		string text2 = text;
		if (text2 != null && TryObtainEpochPostRun(EpochModel.Get(text2), serializablePlayer))
		{
			Log.Info($"Epoch obtained for playing a run as {serializablePlayer.CharacterId}");
		}
	}

	private void PostRunCharacterEpochChecks(SerializablePlayer serializablePlayer, int ascension, bool victory)
	{
		if (victory)
		{
			CheckAscensionOneCompleted(serializablePlayer, ascension);
			string id = EpochModel.GetId<CustomAndSeedsEpoch>();
			if (Progress.Wins >= 3)
			{
				TryObtainEpochPostRun(EpochModel.Get(id), serializablePlayer);
			}
		}
	}

	private void CheckAscensionOneCompleted(SerializablePlayer serializablePlayer, int ascension)
	{
		if (ascension == 1)
		{
			CharacterModel byId = ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId);
			string text = ((byId is Ironclad) ? EpochModel.GetId<Ironclad7Epoch>() : ((byId is Silent) ? EpochModel.GetId<Silent7Epoch>() : ((byId is Regent) ? EpochModel.GetId<Regent7Epoch>() : ((byId is Defect) ? EpochModel.GetId<Defect7Epoch>() : ((!(byId is Necrobinder)) ? null : EpochModel.GetId<Necrobinder7Epoch>())))));
			string text2 = text;
			if (text2 != null)
			{
				TryObtainEpochPostRun(EpochModel.Get(text2), serializablePlayer);
			}
		}
	}

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

	private void ObtainCharUnlockEpoch(Player localPlayer, int act)
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

	private bool TryObtainEpochMidRun(EpochModel epoch, Player localPlayer)
	{
		if (!TryObtainEpochInternal(epoch))
		{
			return false;
		}
		localPlayer.DiscoveredEpochs.Add(epoch.Id);
		return true;
	}

	private bool TryObtainEpochPostRun(EpochModel epoch, SerializablePlayer serializablePlayer)
	{
		if (!TryObtainEpochInternal(epoch))
		{
			return false;
		}
		serializablePlayer.DiscoveredEpochs.Add(epoch.Id);
		return true;
	}

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
			SentryService.CaptureMessage(text);
		}
		return true;
	}

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
		foreach (SerializableEpoch epoch in Progress.Epochs)
		{
			bool flag = epoch.Id != EpochModel.Get<NeowEpoch>().Id;
			bool flag2 = flag;
			if (flag2)
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
			string id = queue.Dequeue();
			EpochModel[] timelineExpansion = EpochModel.Get(id).GetTimelineExpansion();
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
		foreach (var monstersWithSlot in room.Encounter.MonstersWithSlots)
		{
			MonsterModel item2 = monstersWithSlot.Item1;
			EnemyStats orCreateEnemyStats = Progress.GetOrCreateEnemyStats(item2.Id);
			bool flag = orCreateEnemyStats.FightStats.Count == 0;
			FightStats fightStats2 = orCreateEnemyStats.FightStats.FirstOrDefault((FightStats f) => f.Character == character.Id);
			if (fightStats2 == null)
			{
				Log.Info($"{character.Id} fought {item2.Id} for the first time and WON >:(");
				FightStats item3 = new FightStats
				{
					Character = character.Id,
					Wins = 1,
					Losses = 0
				};
				orCreateEnemyStats.FightStats.Add(item3);
				if (flag)
				{
					localPlayer.DiscoveredEnemies.Add(item2.Id);
				}
			}
			else
			{
				orCreateEnemyStats.IncrementWin(character.Id);
			}
		}
	}

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

	public bool SeenFtue(string ftueKey)
	{
		if (!Progress.EnableFtues)
		{
			return true;
		}
		return Progress.FtueCompleted.Contains(ftueKey);
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
