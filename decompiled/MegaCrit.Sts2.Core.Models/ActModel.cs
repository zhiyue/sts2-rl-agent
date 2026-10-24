using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models;

public abstract class ActModel : AbstractModel
{
	protected RoomSet _rooms;

	private IEnumerable<EncounterModel>? _allEncounters;

	private IEnumerable<EncounterModel>? _allWeakEncounters;

	private IEnumerable<EncounterModel>? _allRegularEncounters;

	private IEnumerable<EncounterModel>? _allEliteEncounters;

	private IEnumerable<EncounterModel>? _allBossEncounters;

	private IEnumerable<MonsterModel>? _allMonsters;

	/// <summary>
	/// The subset of shared Ancients that will be available in a mutable act instance.
	/// Will differ from run to run.
	/// </summary>
	private List<AncientEventModel>? _sharedAncientSubset;

	private ActModel _canonicalInstance;

	public LocString Title => new LocString("acts", base.Id.Entry + ".title");

	protected string FilePathIdentifier => base.Id.Entry.ToLowerInvariant();

	public string RestSiteBackgroundPath => SceneHelper.GetScenePath("rest_site/" + FilePathIdentifier + "_rest_site");

	public string MapTopBgPath => ImageHelper.GetImagePath($"packed/map/map_bgs/{FilePathIdentifier}/map_top_{FilePathIdentifier}.png");

	public Texture2D MapTopBg => PreloadManager.Cache.GetCompressedTexture2D(MapTopBgPath);

	public string MapMidBgPath => ImageHelper.GetImagePath($"packed/map/map_bgs/{FilePathIdentifier}/map_middle_{FilePathIdentifier}.png");

	public Texture2D MapMidBg => PreloadManager.Cache.GetCompressedTexture2D(MapMidBgPath);

	public string MapBotBgPath => ImageHelper.GetImagePath($"packed/map/map_bgs/{FilePathIdentifier}/map_bottom_{FilePathIdentifier}.png");

	public Texture2D MapBotBg => PreloadManager.Cache.GetCompressedTexture2D(MapBotBgPath);

	/// <summary>
	/// The index where the act can appear when the list of acts is generated.
	/// 0 =&gt; first act, 1 =&gt; second act, 2 =&gt; third act.
	/// Returns negative if this act should never be organically shown to the player.
	/// </summary>
	public abstract int Index { get; }

	/// <summary>
	/// Whether this is the default unlocked act.
	/// For example, Overgrowth is unlocked by default, but Underdocks is not.
	/// </summary>
	public abstract bool IsDefault { get; }

	/// <summary>
	/// Color of the dots on the map after you've traveled through them.
	/// Also affects the color of the Boss and Ancient nodes.
	/// </summary>
	public abstract Color MapTraveledColor { get; }

	/// <summary>
	/// Color of the dots on the map for paths you haven't traveled.
	/// Also affects the color of the Boss node before you get adjacent to it.
	/// </summary>
	public abstract Color MapUntraveledColor { get; }

	public abstract Color MapBgColor { get; }

	public IEnumerable<string> AssetPaths
	{
		get
		{
			List<string> obj = new List<string> { BackgroundScenePath, MapBotBgPath, MapMidBgPath, MapTopBgPath };
			IEnumerable<string> collection;
			if (!_rooms.HasAncient)
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				collection = enumerable;
			}
			else
			{
				collection = _rooms.Ancient.MapNodeAssetPaths;
			}
			obj.AddRange(collection);
			obj.AddRange(_rooms.Boss.MapNodeAssetPaths);
			IEnumerable<string> collection2;
			if (!_rooms.HasSecondBoss)
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				collection2 = enumerable;
			}
			else
			{
				collection2 = _rooms.SecondBoss.MapNodeAssetPaths;
			}
			obj.AddRange(collection2);
			return new _003C_003Ez__ReadOnlyList<string>(obj);
		}
	}

	public abstract string[] BgMusicOptions { get; }

	public abstract string[] MusicBankPaths { get; }

	public abstract string AmbientSfx { get; }

	protected virtual int NumberOfWeakEncounters => 3;

	/// <summary>
	/// The number of rooms we have for an act, not including any external modifications (ie multiplayer)
	/// NOTE: this excludes the boss room or ancient room
	/// </summary>
	protected abstract int BaseNumberOfRooms { get; }

	/// <summary>
	/// All the monster encounters in this act.
	/// Do not put conditional checks in here.
	/// </summary>
	public IEnumerable<EncounterModel> AllEncounters => _allEncounters ?? (_allEncounters = GenerateAllEncounters());

	/// <summary>
	/// All the weak monster encounters in this act.
	/// </summary>
	public IEnumerable<EncounterModel> AllWeakEncounters => _allWeakEncounters ?? (_allWeakEncounters = AllEncounters.Where((EncounterModel e) => e != null && e.RoomType == RoomType.Monster && e.IsWeak));

	/// <summary>
	/// All the regular (non-weak) monster encounters in this act.
	/// </summary>
	public IEnumerable<EncounterModel> AllRegularEncounters => _allRegularEncounters ?? (_allRegularEncounters = AllEncounters.Where((EncounterModel e) => e != null && e.RoomType == RoomType.Monster && !e.IsWeak));

	/// <summary>
	/// All the elite monster encounters in this act.
	/// </summary>
	public IEnumerable<EncounterModel> AllEliteEncounters => _allEliteEncounters ?? (_allEliteEncounters = AllEncounters.Where((EncounterModel e) => e.RoomType == RoomType.Elite));

	/// <summary>
	/// All the boss encounters in this act.
	/// </summary>
	public IEnumerable<EncounterModel> AllBossEncounters => _allBossEncounters ?? (_allBossEncounters = AllEncounters.Where((EncounterModel e) => e.RoomType == RoomType.Boss));

	/// <summary>
	/// All the monsters you can encounter in this act.
	/// </summary>
	public IEnumerable<MonsterModel> AllMonsters => _allMonsters ?? (_allMonsters = AllEncounters.SelectMany((EncounterModel e) => e.AllPossibleMonsters).Distinct());

	public Achievement DefeatedAllEnemiesAchievement => Enum.Parse<Achievement>("Defeat" + base.Id.Entry.Capitalize() + "Enemies");

	/// <summary>
	/// The path for this act's Treasure Chest Spine animation resource.
	/// </summary>
	public virtual string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_" + FilePathIdentifier + "_skel_data.tres";

	/// <summary>
	/// The normal (non-stroke) skin name for this act's chest.
	/// </summary>
	public abstract string ChestSpineSkinNameNormal { get; }

	/// <summary>
	/// The stroke-outline skin name for this act's chest.
	/// </summary>
	public abstract string ChestSpineSkinNameStroke { get; }

	public virtual MegaSkeletonDataResource ChestSpineResource => new MegaSkeletonDataResource(PreloadManager.Cache.GetAsset<Resource>(ChestSpineResourcePath));

	public abstract string ChestOpenSfx { get; }

	/// <summary>
	/// Overridden by ActModel to define what order bosses should be encountered in.
	/// </summary>
	public abstract IEnumerable<EncounterModel> BossDiscoveryOrder { get; }

	/// <summary>
	/// All the Ancients that are available in this act.
	/// Ignores Unlocks/Epoch state.
	/// Does not include shared Ancients.
	/// </summary>
	public abstract IEnumerable<AncientEventModel> AllAncients { get; }

	/// <summary>
	/// All the events that are available in this act (ignores Unlocks/Epoch state).
	/// Does not include shared events.
	/// </summary>
	public abstract IEnumerable<EventModel> AllEvents { get; }

	public ActModel CanonicalInstance
	{
		get
		{
			if (!base.IsMutable)
			{
				return this;
			}
			return _canonicalInstance;
		}
		private set
		{
			AssertMutable();
			_canonicalInstance = value;
		}
	}

	public override bool ShouldReceiveCombatHooks => false;

	/// <summary>
	/// The boss encounter that has been rolled for this mutable instance of the act.
	/// </summary>
	public EncounterModel BossEncounter => _rooms.Boss;

	/// <summary>
	/// The second boss encounter for Double Boss mode (Ascension 10+), if set.
	/// </summary>
	public EncounterModel? SecondBossEncounter => _rooms.SecondBoss;

	/// <summary>
	/// Whether this act has a second boss (Double Boss ascension mode).
	/// </summary>
	public bool HasSecondBoss => _rooms.HasSecondBoss;

	/// <summary>
	/// The Ancient event that has been rolled for this mutable instance of the act.
	/// </summary>
	public AncientEventModel Ancient => _rooms.Ancient;

	public string BackgroundScenePath => SceneHelper.GetScenePath($"backgrounds/{FilePathIdentifier}/{FilePathIdentifier}_background");

	public Control CreateRestSiteBackground()
	{
		return PreloadManager.Cache.GetScene(RestSiteBackgroundPath).Instantiate<Control>(PackedScene.GenEditState.Disabled);
	}

	/// <summary>
	/// Returns the number of rooms we have for an act accounting for if we are in multiplayer or not.
	/// NOTE: this excludes the boss room or ancient room
	/// </summary>
	/// <returns></returns>
	public int GetNumberOfRooms(bool isMultiplayer)
	{
		int num = BaseNumberOfRooms;
		if (isMultiplayer)
		{
			num--;
		}
		return num;
	}

	/// <summary>
	/// Returns the number of floors for this act, which includes the boss floor and ancient floor
	/// </summary>
	/// <param name="isMultiplayer"></param>
	/// <returns></returns>
	public int GetNumberOfFloors(bool isMultiplayer)
	{
		return GetNumberOfRooms(isMultiplayer) + 2;
	}

	/// <summary>
	/// Generates every encounter that is in this act.
	/// Overriden in subclasses, but should only be called once by <see cref="P:MegaCrit.Sts2.Core.Models.ActModel.AllEncounters" /> so it can be cached.
	/// </summary>
	public abstract IEnumerable<EncounterModel> GenerateAllEncounters();

	/// <summary>
	/// Returns true if the act is unlocked given the passed unlock state, false otherwise.
	/// </summary>
	public abstract bool IsUnlocked(UnlockState unlockState);

	protected override void DeepCloneFields()
	{
		_rooms = new RoomSet();
	}

	/// <summary>
	/// Returns every Ancient in this act that the player has unlocked.
	/// </summary>
	public abstract IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState state);

	protected string GetFullLayerPath(string layerName)
	{
		return $"res://scenes/backgrounds/{FilePathIdentifier}/layers/{FilePathIdentifier}_{layerName}.tscn";
	}

	/// <summary>
	/// Set the subset of shared Ancients that will be available in a mutable act instance.
	/// Generally called in <see cref="M:MegaCrit.Sts2.Core.Runs.RunManager.GenerateRooms" />, and will differ from run to run.
	/// </summary>
	public void SetSharedAncientSubset(List<AncientEventModel> sharedAncientSubset)
	{
		AssertMutable();
		_sharedAncientSubset = new List<AncientEventModel>();
		_sharedAncientSubset.AddRange(sharedAncientSubset);
	}

	public IEnumerable<string> GetAllBackgroundLayerPaths()
	{
		string backgroundsPath = "res://scenes/backgrounds/" + FilePathIdentifier + "/layers";
		using DirAccess dirAccess = DirAccess.Open(backgroundsPath);
		if (dirAccess == null)
		{
			return Array.Empty<string>();
		}
		return (from path in dirAccess.GetFiles()
			where path.EndsWith(".tscn")
			select backgroundsPath + "/" + path).ToArray();
	}

	public void GenerateRooms(Rng rng, UnlockState unlockState, bool isMultiplayer = false)
	{
		AssertMutable();
		List<EventModel> list = AllEvents.Concat(ModelDb.AllSharedEvents).ToList();
		if (!unlockState.IsEpochRevealed<Event1Epoch>())
		{
			list.RemoveAll((EventModel e) => Event1Epoch.Events.Any((EventModel ev) => ev.Id == e.Id));
		}
		if (!unlockState.IsEpochRevealed<Event2Epoch>())
		{
			list.RemoveAll((EventModel e) => Event2Epoch.Events.Any((EventModel ev) => ev.Id == e.Id));
		}
		if (!unlockState.IsEpochRevealed<Event3Epoch>())
		{
			list.RemoveAll((EventModel e) => Event3Epoch.Events.Any((EventModel ev) => ev.Id == e.Id));
		}
		_rooms.events.AddRange(list.UnstableShuffle(rng));
		GrabBag<EncounterModel> grabBag = new GrabBag<EncounterModel>();
		for (int num = 0; num < NumberOfWeakEncounters; num++)
		{
			if (!grabBag.Any())
			{
				foreach (EncounterModel allWeakEncounter in AllWeakEncounters)
				{
					grabBag.Add(allWeakEncounter, 1.0);
				}
			}
			AddWithoutRepeatingTags(_rooms.normalEncounters, grabBag, rng);
		}
		GrabBag<EncounterModel> grabBag2 = new GrabBag<EncounterModel>();
		for (int num2 = NumberOfWeakEncounters; num2 < GetNumberOfRooms(isMultiplayer); num2++)
		{
			if (!grabBag2.Any())
			{
				foreach (EncounterModel allRegularEncounter in AllRegularEncounters)
				{
					grabBag2.Add(allRegularEncounter, 1.0);
				}
			}
			AddWithoutRepeatingTags(_rooms.normalEncounters, grabBag2, rng);
		}
		GrabBag<EncounterModel> grabBag3 = new GrabBag<EncounterModel>();
		for (int num3 = 0; num3 < 15; num3++)
		{
			if (!grabBag3.Any())
			{
				foreach (EncounterModel allEliteEncounter in AllEliteEncounters)
				{
					grabBag3.Add(allEliteEncounter, 1.0);
				}
			}
			AddWithoutRepeatingTags(_rooms.eliteEncounters, grabBag3, rng);
		}
		_rooms.Boss = rng.NextItem(AllBossEncounters);
		_rooms.Ancient = rng.NextItem(GetUnlockedAncients(unlockState).Concat(_sharedAncientSubset ?? new List<AncientEventModel>()));
	}

	/// <summary>
	/// Called after a load. Re-generates specific rooms if necessary.
	/// </summary>
	public void ValidateRoomsAfterLoad(Rng rng)
	{
		if (_rooms.Boss is DeprecatedEncounter)
		{
			_rooms.Boss = rng.NextItem(AllBossEncounters.Where((EncounterModel e) => e.Id != _rooms.SecondBoss?.Id));
		}
		if (_rooms.SecondBoss is DeprecatedEncounter)
		{
			EncounterModel secondBoss = rng.NextItem(AllBossEncounters.Where((EncounterModel e) => e.Id != _rooms.Boss.Id));
			_rooms.SecondBoss = secondBoss;
		}
	}

	public void ApplyDiscoveryOrderModifications(UnlockState unlockState)
	{
		foreach (EncounterModel item in BossDiscoveryOrder)
		{
			if (!unlockState.HasSeenEncounter(item))
			{
				_rooms.Boss = item;
				break;
			}
		}
		ApplyActDiscoveryOrderModifications(unlockState);
	}

	protected abstract void ApplyActDiscoveryOrderModifications(UnlockState unlockState);

	private static void AddWithoutRepeatingTags(ICollection<EncounterModel> encounters, GrabBag<EncounterModel> grabBag, Rng rng)
	{
		EncounterModel encounterModel = grabBag.GrabAndRemove(rng, (EncounterModel e) => !e.SharesTagsWith(encounters.LastOrDefault()) && e != encounters.LastOrDefault());
		if (encounterModel == null)
		{
			encounterModel = grabBag.GrabAndRemove(rng);
		}
		if (encounterModel != null)
		{
			encounters.Add(encounterModel);
		}
	}

	public EventModel PullAncient()
	{
		return _rooms.Ancient;
	}

	public EventModel PullNextEvent(RunState runState)
	{
		_rooms.EnsureNextEventIsValid(runState);
		EventModel eventModel = Hook.ModifyNextEvent(runState, _rooms.NextEvent);
		runState.AddVisitedEvent(eventModel);
		return eventModel;
	}

	public EncounterModel PullNextEncounter(RoomType roomType)
	{
		return roomType switch
		{
			RoomType.Monster => _rooms.NextNormalEncounter, 
			RoomType.Elite => _rooms.NextEliteEncounter, 
			RoomType.Boss => _rooms.NextBossEncounter, 
			_ => throw new ArgumentOutOfRangeException("roomType", roomType, null), 
		};
	}

	public void MarkRoomVisited(RoomType roomType)
	{
		_rooms.MarkVisited(roomType);
	}

	public BackgroundAssets GenerateBackgroundAssets(Rng rng)
	{
		return new BackgroundAssets(FilePathIdentifier, rng);
	}

	public void SetBossEncounter(EncounterModel encounter)
	{
		AssertMutable();
		if (encounter.RoomType != RoomType.Boss)
		{
			throw new ArgumentException("The encounter must be a boss.");
		}
		_rooms.Boss = encounter;
	}

	public void SetSecondBossEncounter(EncounterModel? encounter)
	{
		AssertMutable();
		if (encounter != null && encounter.RoomType != RoomType.Boss)
		{
			throw new ArgumentException("The encounter must be a boss.");
		}
		_rooms.SecondBoss = encounter;
	}

	public void RemoveEventFromSet(EventModel eventModel)
	{
		eventModel.AssertCanonical();
		_rooms.events.Remove(eventModel);
	}

	public ActModel ToMutable()
	{
		AssertCanonical();
		ActModel actModel = (ActModel)MutableClone();
		actModel.CanonicalInstance = this;
		return actModel;
	}

	public SerializableActModel ToSave()
	{
		AssertMutable();
		return new SerializableActModel
		{
			Id = base.Id,
			SerializableRooms = _rooms.ToSave()
		};
	}

	public static ActModel FromSave(SerializableActModel save)
	{
		ActModel actModel = ModelDb.GetById<ActModel>(save.Id).ToMutable();
		actModel._rooms = RoomSet.FromSave(save.SerializableRooms);
		return actModel;
	}

	public abstract MapPointTypeCounts GetMapPointTypes(Rng mapRng);

	/// <summary>
	/// Creates the map for this act.
	/// </summary>
	public ActMap CreateMap(RunState runState, bool replaceTreasureWithElites)
	{
		return StandardActMap.CreateFor(runState, replaceTreasureWithElites);
	}

	/// <summary>
	/// Get a list of random ActModels.
	/// The ActModel at index 0 will be Act 1, index 1 will be Act 2, etc.
	/// This list is not _truly_ random; a given ActModel will only be rolled for the act index it belongs in.
	/// For example, Overgrowth may or may not be at index 0, but it will never be at index 1 or 2.
	/// </summary>
	/// <param name="rng">Rng to use for randomly rolling alternate acts.</param>
	/// <param name="unlockState">The object to use for checking unlock state.</param>
	/// <param name="isMultiplayer">Set to true if we are in a multiplayer run. Disables checking local progress save
	/// for alt acts.</param>
	/// <returns>Randomized list of acts.</returns>
	public static IEnumerable<ActModel> GetRandomList(Rng rng, UnlockState unlockState, bool isMultiplayer)
	{
		IReadOnlyList<IReadOnlyList<ActModel>> actsByIndex = ModelDb.ActsByIndex;
		List<ActModel> list = new List<ActModel>();
		for (int i = 0; i < actsByIndex.Count; i++)
		{
			ActModel actModel = null;
			List<ActModel> list2 = new List<ActModel>();
			foreach (ActModel item in actsByIndex[i])
			{
				if (item.IsUnlocked(unlockState))
				{
					if (!item.IsDefault && !isMultiplayer && !SaveManager.Instance.Progress.DiscoveredActs.Contains(item.Id) && TestMode.IsOff)
					{
						actModel = item;
						break;
					}
					list2.Add(item);
				}
			}
			if (actModel == null)
			{
				actModel = rng.NextItem(list2) ?? throw new InvalidOperationException($"No unlocked acts for index {i}!");
			}
			list.Add(actModel);
		}
		return list;
	}

	/// <summary>
	/// Get the default list of ActModels.
	/// Act 1 (index 0) is <see cref="T:MegaCrit.Sts2.Core.Models.Acts.Overgrowth" />.
	/// Act 2 is <see cref="T:MegaCrit.Sts2.Core.Models.Acts.Hive" />.
	/// Act 3 is <see cref="T:MegaCrit.Sts2.Core.Models.Acts.Glory" />.
	/// </summary>
	/// <returns></returns>
	public static IReadOnlyList<ActModel> GetDefaultList()
	{
		IReadOnlyList<IReadOnlyList<ActModel>> actsByIndex = ModelDb.ActsByIndex;
		List<ActModel> list = new List<ActModel>();
		for (int i = 0; i < actsByIndex.Count; i++)
		{
			ActModel actModel = null;
			foreach (ActModel item in actsByIndex[i])
			{
				if (item.IsDefault)
				{
					actModel = item;
					break;
				}
			}
			if (actModel == null)
			{
				throw new InvalidOperationException($"No default act for index {i}!");
			}
			list.Add(actModel);
		}
		return list;
	}
}
