using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models;

public abstract class RelicModel : AbstractModel
{
	private static readonly StringName _isUsed = new StringName("is_used");

	private static readonly StringName _pulse = new StringName("pulse");

	private static readonly StringName _isWaxStr = new StringName("is_wax");

	protected const string _locTable = "relics";

	private string? _resolvedBigIconPath;

	private Player? _owner;

	private bool _isWax;

	private bool _isMelted;

	private DynamicVarSet? _dynamicVars;

	private int _floorAddedToDeck;

	private RelicStatus _status;

	private RelicModel? _canonicalInstance;

	public virtual LocString Title
	{
		get
		{
			LocString locString = new LocString("relics", base.Id.Entry + ".title");
			if (IsWax)
			{
				LocString waxRelicPrefix = ToyBox.WaxRelicPrefix;
				waxRelicPrefix.Add("Title", locString);
				locString = waxRelicPrefix;
			}
			return locString;
		}
	}

	public LocString Flavor => new LocString("relics", base.Id.Entry + ".flavor");

	protected LocString EventDescription => LocString.GetIfExists("relics", base.Id.Entry + ".eventDescription") ?? Description;

	public LocString Description => new LocString("relics", base.Id.Entry + ".description");

	protected LocString SelectionScreenPrompt
	{
		get
		{
			LocString locString = new LocString("relics", base.Id.Entry + ".selectionScreenPrompt");
			DynamicVars.AddTo(locString);
			return locString;
		}
	}

	public LocString DynamicEventDescription
	{
		get
		{
			LocString eventDescription = EventDescription;
			DynamicVars.AddTo(eventDescription);
			eventDescription.Add("energyPrefix", EnergyIconHelper.GetPrefix(this));
			eventDescription.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
			return eventDescription;
		}
	}

	public LocString DynamicDescription
	{
		get
		{
			LocString description = Description;
			DynamicVars.AddTo(description);
			string prefix = EnergyIconHelper.GetPrefix(this);
			description.Add("energyPrefix", prefix);
			description.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
			foreach (KeyValuePair<string, object> variable in description.Variables)
			{
				if (variable.Value is EnergyVar energyVar)
				{
					energyVar.ColorPrefix = prefix;
				}
			}
			return description;
		}
	}

	protected LocString? AdditionalRestSiteHealText
	{
		get
		{
			LocString ifExists = LocString.GetIfExists("relics", base.Id.Entry + ".additionalRestSiteHealText");
			if (ifExists != null)
			{
				DynamicVars.AddTo(ifExists);
			}
			return ifExists;
		}
	}

	protected virtual string IconBaseName => base.Id.Entry.ToLowerInvariant();

	public virtual string PackedIconPath => ImageHelper.GetImagePath("atlases/relic_atlas.sprites/" + IconBaseName + ".tres");

	protected virtual string PackedIconOutlinePath => ImageHelper.GetImagePath("atlases/relic_outline_atlas.sprites/" + IconBaseName + ".tres");

	protected virtual string BigIconPath => ImageHelper.GetImagePath("relics/" + IconBaseName + ".png");

	private string BigBetaIconPath => ImageHelper.GetImagePath("relics/beta/" + IconBaseName + ".png");

	private static string MissingIconPath => ImageHelper.GetImagePath("powers/missing_power.png");

	public string IconPath => PackedIconPath;

	public Texture2D Icon => ResourceLoader.Load<Texture2D>(PackedIconPath, null, ResourceLoader.CacheMode.Reuse);

	public Texture2D IconOutline => ResourceLoader.Load<Texture2D>(PackedIconOutlinePath, null, ResourceLoader.CacheMode.Reuse);

	public Texture2D BigIcon => PreloadManager.Cache.GetTexture2D(ResolvedBigIconPath);

	private string ResolvedBigIconPath
	{
		get
		{
			if (_resolvedBigIconPath != null)
			{
				return _resolvedBigIconPath;
			}
			if (ResourceLoader.Exists(BigIconPath))
			{
				_resolvedBigIconPath = BigIconPath;
			}
			else if (ResourceLoader.Exists(BigBetaIconPath))
			{
				_resolvedBigIconPath = BigBetaIconPath;
			}
			else
			{
				_resolvedBigIconPath = MissingIconPath;
			}
			return _resolvedBigIconPath;
		}
	}

	public abstract RelicRarity Rarity { get; }

	public RelicPoolModel Pool => ModelDb.AllRelicPools.First((RelicPoolModel p) => p.AllRelicIds.Contains(base.Id));

	public bool IsTradable
	{
		get
		{
			if (IsUsedUp)
			{
				return false;
			}
			if (HasUponPickupEffect)
			{
				return false;
			}
			if (IsMelted)
			{
				return false;
			}
			if (SpawnsPets)
			{
				return false;
			}
			RelicRarity rarity = Rarity;
			if ((rarity == RelicRarity.Starter || (uint)(rarity - 6) <= 1u) ? true : false)
			{
				return false;
			}
			return true;
		}
	}

	public Player Owner
	{
		get
		{
			AssertMutable();
			return _owner;
		}
		set
		{
			AssertMutable();
			if (_owner != null && _owner != value)
			{
				throw new InvalidOperationException("Cannot move relic from " + base.Id.Entry + " one owner to another");
			}
			_owner = value;
		}
	}

	public virtual bool IsUsedUp => false;

	public virtual bool HasUponPickupEffect => false;

	public virtual bool SpawnsPets => false;

	public virtual bool IsStackable => false;

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public bool IsWax
	{
		get
		{
			return _isWax;
		}
		set
		{
			AssertMutable();
			_isWax = value;
		}
	}

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public bool IsMelted
	{
		get
		{
			return _isMelted;
		}
		set
		{
			AssertMutable();
			_isMelted = value;
		}
	}

	public virtual bool AddsPet => false;

	public int StackCount { get; private set; } = 1;

	public DynamicVarSet DynamicVars
	{
		get
		{
			if (_dynamicVars != null)
			{
				return _dynamicVars;
			}
			_dynamicVars = new DynamicVarSet(CanonicalVars);
			_dynamicVars.InitializeWithOwner(this);
			return _dynamicVars;
		}
	}

	protected virtual IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public virtual int MerchantCost => Rarity switch
	{
		RelicRarity.Common => 200, 
		RelicRarity.Uncommon => 250, 
		RelicRarity.Rare => 300, 
		RelicRarity.Shop => 225, 
		RelicRarity.Ancient => 999, 
		RelicRarity.Starter => 999, 
		RelicRarity.Event => 999, 
		RelicRarity.None => 1, 
		_ => throw new InvalidOperationException($"Relic {base.Id} has invalid merchant rarity {Rarity}."), 
	};

	public int FloorAddedToDeck
	{
		get
		{
			return _floorAddedToDeck;
		}
		set
		{
			AssertMutable();
			_floorAddedToDeck = value;
		}
	}

	public RelicStatus Status
	{
		get
		{
			return _status;
		}
		set
		{
			AssertMutable();
			if (_status != value)
			{
				_status = value;
				this.StatusChanged?.Invoke();
			}
		}
	}

	public virtual bool ShowCounter => false;

	public virtual int DisplayAmount => 0;

	public virtual string FlashSfx => "event:/sfx/ui/relic_activate_general";

	public virtual bool ShouldFlashOnPlayer => true;

	public HoverTip HoverTip
	{
		get
		{
			LocString locString = DynamicDescription;
			if (IsMelted)
			{
				locString = new LocString("gameplay_ui", "RELIC_IS_MELTED");
				locString.Add("description", DynamicDescription);
			}
			else if (IsUsedUp && base.IsMutable)
			{
				locString = new LocString("gameplay_ui", "RELIC_USED_UP");
				locString.Add("description", DynamicDescription);
			}
			HoverTip result = new HoverTip(Title, locString);
			result.SetCanonicalModel(CanonicalInstance);
			return result;
		}
	}

	protected virtual IEnumerable<IHoverTip> ExtraHoverTips => Array.Empty<IHoverTip>();

	public IEnumerable<IHoverTip> HoverTipsExcludingRelic => ExtraHoverTips;

	public IEnumerable<IHoverTip> HoverTips
	{
		get
		{
			int num = 1;
			List<IHoverTip> list = new List<IHoverTip>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<IHoverTip> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = HoverTip;
			List<IHoverTip> list2 = list;
			list2.AddRange(ExtraHoverTips);
			return list2;
		}
	}

	public RelicModel CanonicalInstance
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

	public bool HasBeenRemovedFromState { get; private set; }

	public override bool ShouldReceiveCombatHooks => true;

	public event Action<RelicModel, IEnumerable<Creature>>? Flashed;

	public event Action? DisplayAmountChanged;

	public event Action? StatusChanged;

	public virtual bool IsAllowed(IRunState runState)
	{
		return true;
	}

	protected static bool IsBeforeAct3TreasureChest(IRunState runState)
	{
		int num = ((runState.Players.Count > 1) ? 38 : 41);
		return runState.TotalFloor < num;
	}

	public void Flash()
	{
		Flash(new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(Owner.Creature));
	}

	public void Flash(IEnumerable<Creature> targets)
	{
		this.Flashed?.Invoke(this, targets);
	}

	protected void InvokeDisplayAmountChanged()
	{
		this.DisplayAmountChanged?.Invoke();
	}

	public void UpdateTexture(TextureRect texture)
	{
		ShaderMaterial shaderMaterial = (ShaderMaterial)texture.Material;
		shaderMaterial.SetShaderParameter(_isWaxStr, IsWax ? 1 : 0);
		if (IsMelted)
		{
			texture.SelfModulate = Colors.DarkRed;
		}
		if (!RunManager.Instance.IsInProgress || IsMelted)
		{
			shaderMaterial.SetShaderParameter(_pulse, 0);
			shaderMaterial.SetShaderParameter(_isUsed, 0);
			return;
		}
		switch (Status)
		{
		case RelicStatus.Normal:
			shaderMaterial.SetShaderParameter(_pulse, 0);
			shaderMaterial.SetShaderParameter(_isUsed, 0);
			break;
		case RelicStatus.Active:
			shaderMaterial.SetShaderParameter(_pulse, 1);
			shaderMaterial.SetShaderParameter(_isUsed, 0);
			break;
		case RelicStatus.Disabled:
			shaderMaterial.SetShaderParameter(_pulse, 0);
			shaderMaterial.SetShaderParameter(_isUsed, 1);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public RelicModel ToMutable()
	{
		AssertCanonical();
		return (RelicModel)MutableClone();
	}

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_dynamicVars = DynamicVars.Clone(this);
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		if (_canonicalInstance == null)
		{
			CanonicalInstance = ModelDb.GetById<RelicModel>(base.Id);
		}
		HasBeenRemovedFromState = false;
		this.Flashed = null;
		this.DisplayAmountChanged = null;
		this.StatusChanged = null;
	}

	public void RemoveInternal()
	{
		HasBeenRemovedFromState = true;
	}

	public void IncrementStackCount()
	{
		AssertMutable();
		if (!IsStackable)
		{
			throw new InvalidOperationException($"Cannot increment stack count on {base.Id} because it is not a stackable relic.");
		}
		StackCount++;
	}

	public virtual Task AfterObtained()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterRemoved()
	{
		return Task.CompletedTask;
	}

	public SerializableRelic ToSerializable()
	{
		AssertMutable();
		return new SerializableRelic
		{
			Id = base.Id,
			Props = SavedProperties.From(this),
			FloorAddedToDeck = FloorAddedToDeck
		};
	}

	public static RelicModel FromSerializable(SerializableRelic save)
	{
		RelicModel relicModel = SaveUtil.RelicOrDeprecated(save.Id).ToMutable();
		save.Props?.Fill(relicModel);
		if (save.FloorAddedToDeck.HasValue)
		{
			relicModel.FloorAddedToDeck = save.FloorAddedToDeck.Value;
		}
		return relicModel;
	}

	protected void RelicIconChanged()
	{
		_resolvedBigIconPath = null;
	}

	protected static LocString L10NLookup(string entryName)
	{
		return new LocString("relics", entryName);
	}
}
