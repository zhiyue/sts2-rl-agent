using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models;

public abstract class PowerModel : AbstractModel
{
	public const string locTable = "powers";

	protected static readonly Color _normalAmountLabelColor = StsColors.cream;

	protected static readonly Color _debuffAmountLabelColor = StsColors.red;

	private string? _resolvedBigIconPath;

	private int _amount;

	private int _amountOnTurnStart;

	private bool _skipNextDurationTick;

	private Creature? _owner;

	private Creature? _applier;

	private Creature? _target;

	private DynamicVarSet? _dynamicVars;

	private object? _internalData;

	private PowerModel _canonicalInstance;

	public virtual LocString Title => new LocString("powers", base.Id.Entry + ".title");

	public virtual LocString Description => new LocString("powers", base.Id.Entry + ".description");

	public LocString SmartDescription
	{
		get
		{
			if (!HasSmartDescription)
			{
				return Description;
			}
			return new LocString("powers", SmartDescriptionLocKey);
		}
	}

	public bool HasSmartDescription => LocString.Exists("powers", SmartDescriptionLocKey);

	public LocString RemoteDescription
	{
		get
		{
			if (!HasRemoteDescription)
			{
				return Description;
			}
			return new LocString("powers", RemoteDescriptionLocKey);
		}
	}

	public bool HasRemoteDescription => LocString.Exists("powers", RemoteDescriptionLocKey);

	protected virtual string RemoteDescriptionLocKey => base.Id.Entry + ".remoteDescription";

	protected virtual string SmartDescriptionLocKey => base.Id.Entry + ".smartDescription";

	protected LocString SelectionScreenPrompt
	{
		get
		{
			LocString locString = new LocString("powers", base.Id.Entry + ".selectionScreenPrompt");
			if (!locString.Exists())
			{
				throw new InvalidOperationException($"No selection screen prompt for {base.Id}.");
			}
			DynamicVars.AddTo(locString);
			locString.Add("Amount", Amount);
			return locString;
		}
	}

	public string PackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/" + base.Id.Entry.ToLowerInvariant() + ".tres");

	private string BigIconPath => ImageHelper.GetImagePath("powers/" + base.Id.Entry.ToLowerInvariant() + ".png");

	private string BigBetaIconPath => ImageHelper.GetImagePath("powers/beta/" + base.Id.Entry.ToLowerInvariant() + ".png");

	private static string MissingIconPath => ImageHelper.GetImagePath("powers/missing_power.png");

	public string IconPath => PackedIconPath;

	public Texture2D Icon => ResourceLoader.Load<Texture2D>(PackedIconPath, null, ResourceLoader.CacheMode.Reuse);

	public Texture2D BigIcon => PreloadManager.Cache.GetTexture2D(ResolvedBigIconPath);

	public string ResolvedBigIconPath
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

	public abstract PowerType Type { get; }

	public virtual bool IsInstanced => false;

	public bool IsVisible
	{
		get
		{
			if (Target == null || LocalContext.IsMe(Target) || Target.IsEnemy)
			{
				return IsVisibleInternal;
			}
			return false;
		}
	}

	protected virtual bool IsVisibleInternal => true;

	public virtual bool ShouldPlayVfx
	{
		get
		{
			Creature owner = Owner;
			if (owner != null && owner.IsAlive && CombatManager.Instance.IsInProgress)
			{
				return IsVisible;
			}
			return false;
		}
	}

	public int Amount
	{
		get
		{
			return _amount;
		}
		set
		{
			SetAmount(value);
		}
	}

	public int AmountOnTurnStart
	{
		get
		{
			return _amountOnTurnStart;
		}
		set
		{
			AssertMutable();
			_amountOnTurnStart = value;
		}
	}

	public virtual int DisplayAmount => Amount;

	public virtual Color AmountLabelColor
	{
		get
		{
			if (GetTypeForAmount(Amount) != PowerType.Debuff)
			{
				return _normalAmountLabelColor;
			}
			return _debuffAmountLabelColor;
		}
	}

	public abstract PowerStackType StackType { get; }

	public virtual bool AllowNegative => false;

	public PowerType TypeForCurrentAmount => GetTypeForAmount(Amount);

	public bool SkipNextDurationTick
	{
		get
		{
			return _skipNextDurationTick;
		}
		set
		{
			AssertMutable();
			_skipNextDurationTick = value;
		}
	}

	public Creature Owner
	{
		get
		{
			AssertMutable();
			return _owner;
		}
		private set
		{
			AssertMutable();
			if (_owner != null && _owner != value)
			{
				throw new InvalidOperationException("Cannot move power " + base.Id.Entry + " from one owner to another");
			}
			_owner = value;
		}
	}

	public CombatState CombatState => Owner.CombatState;

	public Creature? Applier
	{
		get
		{
			return _applier;
		}
		set
		{
			AssertMutable();
			_applier = value;
		}
	}

	public Creature? Target
	{
		get
		{
			return _target;
		}
		set
		{
			AssertMutable();
			_target = value;
		}
	}

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

	public virtual bool ShouldScaleInMultiplayer => false;

	protected virtual IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public HoverTip DumbHoverTip
	{
		get
		{
			LocString description = Description;
			AddDumbVariablesToDescription(description);
			return new HoverTip(this, description.GetFormattedText(), isSmart: false);
		}
	}

	protected virtual IEnumerable<IHoverTip> ExtraHoverTips => Array.Empty<IHoverTip>();

	public IEnumerable<IHoverTip> HoverTips
	{
		get
		{
			List<IHoverTip> list = new List<IHoverTip>();
			if (!IsVisible)
			{
				return list;
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = HasSmartDescription && base.IsMutable;
			if (flag)
			{
				LocString locString = SmartDescription;
				if (Applier != null && !LocalContext.IsMe(Applier) && HasRemoteDescription)
				{
					locString = RemoteDescription;
				}
				locString.Add("Amount", Amount);
				locString.Add("OnPlayer", Owner.IsPlayer);
				locString.Add("IsMultiplayer", Owner.CombatState.Players.Count > 1);
				locString.Add("PlayerCount", Owner.CombatState.Players.Count);
				locString.Add("OwnerName", Owner.IsPlayer ? Owner.Player.Character.Title : Owner.Monster.Title);
				if (Applier != null)
				{
					locString.Add("ApplierName", Applier.IsPlayer ? Applier.Player.Character.Title : Applier.Monster.Title);
				}
				if (Target != null)
				{
					locString.Add("TargetName", Target.IsPlayer ? Target.Player.Character.Title : Target.Monster.Title);
				}
				AddDumbVariablesToDescription(locString);
				DynamicVars.AddTo(locString);
				stringBuilder.Append(locString.GetFormattedText());
			}
			else
			{
				LocString description = Description;
				AddDumbVariablesToDescription(description);
				stringBuilder.Append(description.GetFormattedText());
			}
			list.Add(new HoverTip(this, stringBuilder.ToString(), flag));
			list.AddRange(ExtraHoverTips);
			return list;
		}
	}

	private PowerModel CanonicalInstance
	{
		get
		{
			if (!base.IsMutable)
			{
				return this;
			}
			return _canonicalInstance;
		}
		set
		{
			AssertMutable();
			_canonicalInstance = value;
		}
	}

	public override bool ShouldReceiveCombatHooks => true;

	public virtual bool OwnerIsSecondaryEnemy => false;

	public event Action? PulsingStarted;

	public event Action? PulsingStopped;

	public event Action<PowerModel>? Flashed;

	public event Action? DisplayAmountChanged;

	public event Action? Removed;

	public void StartPulsing()
	{
		this.PulsingStarted?.Invoke();
	}

	public void StopPulsing()
	{
		this.PulsingStopped?.Invoke();
	}

	protected void Flash()
	{
		this.Flashed?.Invoke(this);
	}

	protected void InvokeDisplayAmountChanged()
	{
		this.DisplayAmountChanged?.Invoke();
	}

	public PowerType GetTypeForAmount(decimal customAmount)
	{
		if (StackType.Equals(PowerStackType.Counter) && AllowNegative && customAmount < 0m)
		{
			return PowerType.Debuff;
		}
		if (!AllowNegative && Type.Equals(PowerType.Debuff) && customAmount < 0m)
		{
			return PowerType.Buff;
		}
		return Type;
	}

	public bool ShouldRemoveDueToAmount()
	{
		if (AllowNegative || Amount > 0)
		{
			if (AllowNegative)
			{
				return Amount == 0;
			}
			return false;
		}
		return true;
	}

	protected virtual object? InitInternalData()
	{
		return null;
	}

	protected T GetInternalData<T>()
	{
		return (T)_internalData;
	}

	private void AddDumbVariablesToDescription(LocString description)
	{
		description.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
		description.Add("energyPrefix", EnergyIconHelper.GetPrefix(this));
	}

	public void SetAmount(int amount, bool silent = false)
	{
		AssertMutable();
		int num = amount - _amount;
		if (num != 0)
		{
			_amount = amount;
			this.DisplayAmountChanged?.Invoke();
			Owner.InvokePowerModified(this, num, silent);
		}
	}

	public PowerModel ToMutable(int initialAmount = 0)
	{
		AssertCanonical();
		PowerModel powerModel = (PowerModel)MutableClone();
		powerModel.CanonicalInstance = this;
		powerModel.Amount = initialAmount;
		return powerModel;
	}

	public void ApplyInternal(Creature owner, decimal amount, bool silent = false)
	{
		if (!(amount == 0m))
		{
			AssertMutable();
			Owner = owner;
			SetAmount((int)amount, silent);
			Owner.ApplyPowerInternal(this);
		}
	}

	public void RemoveInternal()
	{
		AssertMutable();
		this.Removed?.Invoke();
		Owner.RemovePowerInternal(this);
	}

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_dynamicVars = DynamicVars.Clone(this);
		_internalData = InitInternalData();
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		this.Flashed = null;
		this.DisplayAmountChanged = null;
		this.Removed = null;
		this.PulsingStarted = null;
		this.PulsingStopped = null;
		_owner = null;
	}

	public virtual Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterRemoved(Creature oldOwner)
	{
		return Task.CompletedTask;
	}

	public virtual bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return true;
	}

	public virtual bool ShouldOwnerDeathTriggerFatal()
	{
		return true;
	}
}
