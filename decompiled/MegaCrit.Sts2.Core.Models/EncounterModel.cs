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

public abstract class EncounterModel : AbstractModel
{
	private const string _locTable = "encounters";

	private Rng? _rng;

	private IReadOnlyList<(MonsterModel, string?)>? _monstersWithSlots;

	private EncounterModel _canonicalInstance;

	private BackgroundAssets? _backgroundAssets;

	public override bool ShouldReceiveCombatHooks => false;

	protected Rng Rng => _rng;

	public abstract RoomType RoomType { get; }

	public virtual bool IsWeak => false;

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

	public LocString? CustomRewardDescription => LocString.GetIfExists("encounters", base.Id.Entry + ".customRewardDescription");

	public virtual bool IsDebugEncounter => false;

	public virtual IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

	public bool HaveMonstersBeenGenerated => _monstersWithSlots != null;

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

	public virtual IEnumerable<string>? ExtraAssetPaths => null;

	public virtual float GetCameraScaling()
	{
		return 1f;
	}

	public virtual Vector2 GetCameraOffset()
	{
		return Vector2.Zero;
	}

	public string GetNextSlot(CombatState combatState)
	{
		return Slots.FirstOrDefault((string s) => combatState.Enemies.All((Creature c) => c.SlotName != s), string.Empty);
	}

	protected abstract IReadOnlyList<(MonsterModel, string?)> GenerateMonsters();

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
		if (ExtraAssetPaths != null)
		{
			hashSet.UnionWith(ExtraAssetPaths);
		}
		foreach (var monstersWithSlot in MonstersWithSlots)
		{
			MonsterModel item = monstersWithSlot.Item1;
			hashSet.UnionWith(item.AssetPaths);
		}
		return hashSet;
	}

	public void DebugRandomizeRng()
	{
		AssertMutable();
		_rng = new Rng((uint)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
	}

	public LocString GetLossMessageFor(CharacterModel character)
	{
		LocString locString = L10NLookup(base.Id.Entry + ".loss");
		character.AddDetailsTo(locString);
		locString.Add("encounter", Title);
		return locString;
	}

	public virtual Dictionary<string, string> SaveCustomState()
	{
		return new Dictionary<string, string>();
	}

	public virtual void LoadCustomState(Dictionary<string, string> state)
	{
	}

	private static LocString L10NLookup(string key)
	{
		return new LocString("encounters", key);
	}
}
