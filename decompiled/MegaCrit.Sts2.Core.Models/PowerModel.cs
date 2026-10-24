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
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

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

	/// <summary>
	/// NOTE: Unlike other models' DynamicDescription, this doesn't include any variables. Someone should probably do that
	/// but it's a bit larger of a refactor than I want to do right now
	/// </summary>
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

	public virtual PowerInstanceType InstanceType => PowerInstanceType.None;

	/// <summary>
	/// Should this power be visible?
	/// Don't make this virtual, use <see cref="P:MegaCrit.Sts2.Core.Models.PowerModel.IsVisibleInternal" /> instead if you need to override.
	/// </summary>
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

	/// <summary>
	/// Should this power be visible?
	/// Usually true, but overridden to false for powers that want to stay hidden and perform extra logic behind the
	/// scenes.
	/// </summary>
	protected virtual bool IsVisibleInternal => true;

	/// <summary>
	/// Should this power play VFX when applied/removed/flashed?
	/// Usually true for visible powers, but overridden to false for powers that don't want to be so "loud", like Osty's
	/// Die For You power.
	/// </summary>
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
		private set
		{
			SetAmount(value);
		}
	}

	/// <summary>
	/// The amount this power had at the beginning of the current turn.
	/// Updated at the very start of each turn before any hooks run.
	/// Useful for powers that should only trigger if they were present at turn start,
	/// preventing same-turn activation when applied mid-turn (e.g., via auto-play effects).
	/// </summary>
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

	/// <summary>
	/// The color to use for this power's Amount label.
	/// Usually red for debuffs and cream for all other powers, but can be overridden for special cases.
	/// </summary>
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

	/// <summary>
	/// This enables the behavior of duration-type powers (Vulnerable, Weak, etc.) ticking down at the end of the
	/// monster side turn, but skipping the first tick if a monster applied the power to the player.
	/// </summary>
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

	/// <summary>
	/// Get the Creature that this power is on.
	/// Will technically be null on a canonical power model, but we should never be checking that, so we leave this as
	/// non-nullable for convenience.
	/// </summary>
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

	/// <summary>
	/// The CombatState that this power's owner exists in.
	/// Will technically be null on a canonical power model, but we should never be checking that, so we leave this as
	/// non-nullable for convenience.
	/// </summary>
	public ICombatState CombatState => Owner.CombatState;

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

	/// <summary>
	/// Get the Creature that this power targets.
	/// For most powers, this is null. It is used on instanced powers that each target a player in multiplayer.
	/// </summary>
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

	/// <summary>
	/// "Public" data about this power.
	/// These data are visible everywhere else in the game. They can be displayed in localization entries, read/written
	/// by other callers, and if you clone this power, all these vars will be cloned along with it.
	/// If you have "private" data that should be invisible to the rest of the game, use <see cref="F:MegaCrit.Sts2.Core.Models.PowerModel._internalData" />.
	/// </summary>
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

	/// <summary>
	/// If set to true, this power's amount will automatically be multiplied by the player count when applied.
	/// </summary>
	public virtual bool ShouldScaleInMultiplayer => false;

	protected virtual IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public HoverTip DumbHoverTip => GetDumbHoverTip();

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
					string variable = ((Applier.Monster != null) ? Applier.Monster.Title.GetFormattedText() : ((Applier.Player == null) ? "" : PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, Applier.Player.NetId)));
					locString.Add("ApplierName", variable);
				}
				if (Target != null)
				{
					if (Target.IsMonster)
					{
						locString.Add("TargetName", Target.Monster.Title);
					}
					else
					{
						locString.Add("TargetName", PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, Target.Player.NetId));
					}
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

	/// <summary>
	/// Hook that allows this power to classify the enemy as primary or secondary.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Creatures.Creature.IsPrimaryEnemy" /> for detailed definitions.
	/// </summary>
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

	/// <summary>
	/// Should this power be removed from its owner due to an amount change?
	/// Most powers are removed when they go below 0.
	/// Powers that allow negative amounts (Strength, DexterityPower, etc.) should be removed when they reach exactly 0.
	/// </summary>
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

	/// <summary>
	/// If ShouldScaleInMultiplayer is true, this will be called before the power is applied to any enemies.
	/// Unless overridden, it applies a default scaling based on the number of players and
	/// <see cref="M:MegaCrit.Sts2.Core.Models.Singleton.MultiplayerScalingModel.GetMultiplayerScaling(MegaCrit.Sts2.Core.Models.EncounterModel,System.Int32)" />.
	/// Be sure to use the passed combat state rather than CombatState, as it will not be set in time.
	/// </summary>
	/// <param name="combatState">The combat state which will own the power.</param>
	/// <param name="applier">The creature applying the power.</param>
	/// <param name="amount">The amount of the power that is being applied.</param>
	/// <param name="target">The target receiving the power.</param>
	/// <param name="cardSource">The card applying the power, if any.</param>
	/// <returns>The modified amount to apply.</returns>
	public virtual decimal GetScaledAmountForMultiplayer(ICombatState combatState, Creature? applier, decimal amount, Creature target, CardModel? cardSource)
	{
		return amount * (decimal)combatState.Players.Count * MultiplayerScalingModel.GetMultiplayerScaling(combatState.Encounter, combatState.RunState.CurrentActIndex);
	}

	/// <summary>
	/// Initialize any internal data used by this power.
	/// </summary>
	/// <returns></returns>
	protected virtual object? InitInternalData()
	{
		return null;
	}

	/// <summary>
	/// "Private" data that lives behind-the-scenes to make this power work as expected.
	/// These data are isolated to this instance of the power. They cannot be displayed in localization entries, and
	/// they will be reset on clones of this power.
	/// If you want to set data that is meant to be cloned and displayed in localization, use <see cref="P:MegaCrit.Sts2.Core.Models.PowerModel.DynamicVars" />.
	/// </summary>
	protected T GetInternalData<T>()
	{
		return (T)_internalData;
	}

	public HoverTip GetDumbHoverTip(int? amountOverride = null)
	{
		LocString description = Description;
		AddDumbVariablesToDescription(description, amountOverride);
		return new HoverTip(this, description.GetFormattedText(), isSmart: false);
	}

	private void AddDumbVariablesToDescription(LocString description, int? amountOverride = null)
	{
		description.Add("Amount", amountOverride ?? Amount);
		description.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
		description.Add("energyPrefix", EnergyIconHelper.GetPrefix(this));
	}

	public void SetAmount(int amount, bool silent = false)
	{
		AssertMutable();
		amount = Math.Clamp(amount, -999999999, 999999999);
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

	/// <summary>
	/// Hook that runs just before this power is first applied to its owner.
	/// Does not run if the owner already has this power, and its amount was just changed.
	/// </summary>
	/// <param name="target">The creature to which this power is about to be applied.</param>
	/// <param name="amount">The amount of the power that is about to be applied.</param>
	/// <param name="applier">The creature that applied the power, if any.</param>
	/// <param name="cardSource">The card that applied the power, if any.</param>
	public virtual Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Hook that runs after this power is first applied to its owner.
	/// Does not run if the owner already has this power, and its amount was just changed.
	/// </summary>
	/// <param name="applier"></param>
	/// <param name="cardSource"></param>
	public virtual Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Hook that runs after this power is removed from its owner.
	/// <param name="oldOwner">The owner of this power before it was removed.</param>
	/// </summary>
	public virtual Task AfterRemoved(Creature oldOwner)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Should this power be removed after its owner dies?
	/// Usually true, but false for powers that do things like revive their owner.
	/// </summary>
	public virtual bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return true;
	}

	/// <summary>
	/// Should this power's owner's death trigger effects with the Fatal keyword like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Feed" />?
	/// Usually true, but false for <see cref="T:MegaCrit.Sts2.Core.Models.Powers.MinionPower" /> and a few other powers.
	/// </summary>
	public virtual bool ShouldOwnerDeathTriggerFatal()
	{
		return true;
	}
}
