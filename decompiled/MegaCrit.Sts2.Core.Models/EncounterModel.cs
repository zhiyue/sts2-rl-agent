using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models;

/// <summary>
/// An encounter represents a single combat encounter. It can be a weak encounter, normal enemies, elites,
/// boss fight, or enemies you fight during an event. Encounters may be made up of one or more monsters and
/// the enemies the player fights may not always be the same due to randomization of that encounter.
/// </summary>
public abstract class EncounterModel : AbstractModel
{
	private const string _locTable = "encounters";

	private Rng? _rng;

	private IReadOnlyList<(MonsterModel, string?)>? _monstersWithSlots;

	private List<MonsterModel>? _spawnedEnemies;

	private EncounterModel _canonicalInstance;

	private BackgroundAssets? _backgroundAssets;

	public override bool ShouldReceiveCombatHooks => false;

	/// <summary>
	/// A per-encounter RNG that we can use to do random rolls in the encounter (for things like monster HP)
	/// independently of the run's centralized RNG.
	/// This is safe to do in encounters because we don't need to keep track of a given encounter's RNG state once it's
	/// over.
	/// The private backing field for this can be null, but by the time this is being accessed by subclasses, it should
	/// always be set. If it's not, you're doing something wrong.
	/// </summary>
	protected Rng Rng => _rng;

	public abstract RoomType RoomType { get; }

	/// <summary>
	/// Is this one of the weak encounters that we start acts with?
	/// </summary>
	public virtual bool IsWeak => false;

	/// <summary>
	/// Should this encounter give combat rewards when it's over?
	/// Usually true, but many event-specific encounters set this to false.
	/// </summary>
	public virtual bool ShouldGiveRewards => true;

	public virtual int MinGoldReward
	{
		get
		{
			double num = RoomType switch
			{
				RoomType.Monster => 10, 
				RoomType.Elite => 35, 
				RoomType.Boss => 100, 
				_ => 0, 
			};
			if (AscensionHelper.HasAscension(AscensionLevel.Poverty))
			{
				num *= AscensionHelper.PovertyAscensionGoldMultiplier;
			}
			return (int)num;
		}
	}

	public virtual int MaxGoldReward
	{
		get
		{
			double num = RoomType switch
			{
				RoomType.Monster => 20, 
				RoomType.Elite => 45, 
				RoomType.Boss => 100, 
				_ => 0, 
			};
			if (AscensionHelper.HasAscension(AscensionLevel.Poverty))
			{
				num *= AscensionHelper.PovertyAscensionGoldMultiplier;
			}
			return (int)num;
		}
	}

	/// <summary>
	/// The description for this encounter's custom reward.
	/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Monsters.ThievingHopper" /> uses a <see cref="T:MegaCrit.Sts2.Core.Rewards.SpecialCardReward" /> to give you your stolen card
	/// back, and we use this field to specify the "Take your stolen card back" text for the reward button.
	/// </summary>
	public LocString? CustomRewardDescription => LocString.GetIfExists("encounters", base.Id.Entry + ".customRewardDescription");

	/// <summary>
	/// Is this an encounter that's used for debugging, testing, etc.?
	/// </summary>
	public virtual bool IsDebugEncounter => false;

	public virtual IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

	/// <summary>
	/// Have this encounter's monsters been generated yet?
	/// Used for delayed-start combats (like combat-style events that can transition to combats) to make sure we don't
	/// try to re-generated already generated monsters when the actual combat starts.
	/// </summary>
	public bool HaveMonstersBeenGenerated => _monstersWithSlots != null;

	/// <summary>
	/// Returns a list of (mutable) monsters in this encounter, along with the slot they should be placed in.
	/// This should only be used when generating a real encounter, not when generating the hypothetical monsters that could
	/// be in the encounter. For that, use AllPossibleMonsters.
	/// </summary>
	public IReadOnlyList<(MonsterModel, string?)> MonstersWithSlots
	{
		get
		{
			AssertMutable();
			if (_monstersWithSlots == null)
			{
				throw new InvalidOperationException("GenerateMonstersWithSlots must be called before using this property!");
			}
			return _monstersWithSlots;
		}
	}

	/// <summary>
	/// A list of all the monsters that were present in this encounter.
	/// This counts monsters that were initially spawned in the encounter, as well as monsters that were summoned during
	/// it.
	/// </summary>
	public IReadOnlyList<MonsterModel> SpawnedEnemies
	{
		get
		{
			AssertMutable();
			return _spawnedEnemies ?? new List<MonsterModel>();
		}
	}

	public abstract IEnumerable<MonsterModel> AllPossibleMonsters { get; }

	public EncounterModel CanonicalInstance
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

	public virtual bool HasScene => false;

	public virtual IReadOnlyList<string> Slots => Array.Empty<string>();

	/// <summary>
	/// Should the players be fully centered in the encounter?
	/// Usually this is false, because we want to leave padding in the center for card plays.
	/// However, in certain situations (like "surrounded" combats), we want to fully center the players and just live
	/// with them being temporarily covered by played cards.
	/// </summary>
	public virtual bool FullyCenterPlayers => false;

	private string ScenePath => SceneHelper.GetScenePath("encounters/" + base.Id.Entry.ToLowerInvariant());

	protected virtual bool HasCustomBackground => false;

	public virtual string CustomBgm => "";

	public bool HasBgm => CustomBgm != "";

	public virtual string AmbientSfx => "";

	public bool HasAmbientSfx => AmbientSfx != "";

	public virtual string BossNodePath => $"res://animations/map/{base.Id.Entry.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}_node_skel_data.tres";

	public virtual MegaSkeletonDataResource? BossNodeSpineResource
	{
		get
		{
			if (!ResourceLoader.Exists(BossNodePath))
			{
				return null;
			}
			return new MegaSkeletonDataResource(PreloadManager.Cache.GetAsset<Resource>(BossNodePath));
		}
	}

	public LocString Title => L10NLookup(base.Id.Entry + ".title");

	/// <summary>
	/// Used for pre-loading boss map assets
	/// </summary>
	public IEnumerable<string> MapNodeAssetPaths
	{
		get
		{
			if (BossNodeSpineResource != null)
			{
				return new global::_003C_003Ez__ReadOnlySingleElementList<string>(BossNodePath);
			}
			return new global::_003C_003Ez__ReadOnlyArray<string>(new string[2]
			{
				BossNodePath + ".png",
				BossNodePath + "_outline.png"
			});
		}
	}

	public virtual IEnumerable<string> ExtraAssetPaths => Array.Empty<string>();

	public virtual float GetCameraScaling()
	{
		return 1f;
	}

	public virtual Vector2 GetCameraOffset()
	{
		return Vector2.Zero;
	}

	public string GetNextSlot(ICombatState combatState)
	{
		return Slots.FirstOrDefault((string s) => combatState.Enemies.All((Creature c) => c.SlotName != s), string.Empty);
	}

	protected abstract IReadOnlyList<(MonsterModel, string?)> GenerateMonsters();

	/// <summary>
	/// Generate the monsters that the players should fight in this encounter.
	/// </summary>
	/// <param name="runState">
	/// The <see cref="T:MegaCrit.Sts2.Core.Runs.IRunState" /> that this encounter exists in.
	/// Used for things like generating a deterministic RNG, etc.
	/// </param>
	public void GenerateMonstersWithSlots(IRunState runState)
	{
		AssertMutable();
		if (_monstersWithSlots != null)
		{
			throw new InvalidOperationException("Monsters have already been generated for this encounter.");
		}
		if (_rng == null)
		{
			uint seed = (uint)((int)runState.Rng.Seed + runState.TotalFloor + StringHelper.GetDeterministicHashCode(base.Id.Entry));
			_rng = new Rng(seed);
		}
		_monstersWithSlots = GenerateMonsters();
		foreach (var monstersWithSlot in _monstersWithSlots)
		{
			MonsterModel item = monstersWithSlot.Item1;
			item.AssertMutable();
		}
	}

	public bool SharesTagsWith(EncounterModel? other)
	{
		if (other != null)
		{
			return Tags.Intersect(other.Tags).Any();
		}
		return false;
	}

	public NCombatBackground CreateBackground(ActModel parentAct, Rng rng)
	{
		return NCombatBackground.Create(GetBackgroundAssets(parentAct, rng));
	}

	private BackgroundAssets GetBackgroundAssets(ActModel parentAct, Rng rng)
	{
		AssertMutable();
		if (_backgroundAssets == null)
		{
			if (HasCustomBackground)
			{
				_backgroundAssets = CreateBackgroundAssetsForCustom(rng);
			}
			else
			{
				_backgroundAssets = parentAct.GenerateBackgroundAssets(rng);
			}
		}
		return _backgroundAssets;
	}

	private BackgroundAssets CreateBackgroundAssetsForCustom(Rng rng)
	{
		return new BackgroundAssets(base.Id.Entry.ToLowerInvariant(), rng);
	}

	public Control CreateScene()
	{
		return PreloadManager.Cache.GetScene(ScenePath).Instantiate<Control>(PackedScene.GenEditState.Disabled);
	}

	public EncounterModel ToMutable()
	{
		AssertCanonical();
		EncounterModel encounterModel = (EncounterModel)MutableClone();
		encounterModel.CanonicalInstance = this;
		return encounterModel;
	}

	public IEnumerable<string> GetAssetPaths(IRunState runState)
	{
		HashSet<string> hashSet = new HashSet<string>();
		hashSet.UnionWith(GetBackgroundAssets(runState.Act, NCombatRoom.GenerateBackgroundRngForCurrentPoint(runState)).AssetPaths);
		if (HasScene)
		{
			hashSet.Add(ScenePath);
		}
		hashSet.UnionWith(ExtraAssetPaths);
		foreach (var monstersWithSlot in MonstersWithSlots)
		{
			MonsterModel item = monstersWithSlot.Item1;
			hashSet.UnionWith(item.AssetPaths);
		}
		return hashSet;
	}

	/// <summary>
	/// Randomize the per-encounter RNG.
	/// Should only be used for debugging (like in the `encounter` console command) to keep from
	/// rolling the same
	/// </summary>
	public void DebugRandomizeRng()
	{
		AssertMutable();
		_rng = new Rng((uint)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
	}

	/// <summary>
	/// Get the message to show when the specified character loses in this encounter.
	/// </summary>
	/// <param name="character">Character that lost.</param>
	public LocString GetLossMessageFor(CharacterModel character)
	{
		LocString locString = L10NLookup(base.Id.Entry + ".loss");
		character.AddDetailsTo(locString);
		locString.Add("encounter", Title);
		return locString;
	}

	/// <summary>
	/// The proportion (0-1) of gold rewards the players receive when this encounter's combat ends.
	/// Defaults to the fraction of spawned enemies that were defeated rather than escaped.
	/// Override for encounters with bespoke escape/reward rules (e.g. <see cref="T:MegaCrit.Sts2.Core.Models.Encounters.GremlinMercNormal" />).
	/// </summary>
	public virtual float CalculateGoldProportion(CombatState combatState)
	{
		return 1f - (float)combatState.EscapedCreatures.Count / (float)SpawnedEnemies.Count;
	}

	/// <summary>
	/// Override to persist encounter-specific state when saving a pre-finished combat room.
	/// </summary>
	public virtual Dictionary<string, string> SaveCustomState()
	{
		return new Dictionary<string, string>();
	}

	/// <summary>
	/// Override to restore encounter-specific state when loading a pre-finished combat room.
	/// </summary>
	public virtual void LoadCustomState(Dictionary<string, string> state)
	{
	}

	private static LocString L10NLookup(string key)
	{
		return new LocString("encounters", key);
	}

	/// <summary>
	/// Called when a creature is spawned during combat.
	/// If it's an enemy, it's added to the SpawnedEnemies list.
	/// </summary>
	public void OnCreatureSpawned(Creature creature)
	{
		AssertMutable();
		if (creature.Side != CombatSide.Enemy)
		{
			return;
		}
		MonsterModel monsterModel = creature.Monster?.CanonicalInstance;
		if (monsterModel != null && (_spawnedEnemies == null || !_spawnedEnemies.Contains(monsterModel)))
		{
			if (_spawnedEnemies == null)
			{
				_spawnedEnemies = new List<MonsterModel>();
			}
			_spawnedEnemies.Add(monsterModel);
		}
	}
}
