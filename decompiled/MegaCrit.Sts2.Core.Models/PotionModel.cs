using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models;

public abstract class PotionModel : AbstractModel
{
	public const string locTable = "potions";

	private Player? _owner;

	private DynamicVarSet? _dynamicVars;

	private PotionModel _canonicalInstance;

	public LocString Title => new LocString("potions", base.Id.Entry + ".title");

	private LocString Description => new LocString("potions", base.Id.Entry + ".description");

	public LocString SelectionScreenPrompt => new LocString("potions", base.Id.Entry + ".selectionScreenPrompt");

	public LocString DynamicDescription
	{
		get
		{
			LocString description = Description;
			DynamicVars.AddTo(description);
			string prefix = EnergyIconHelper.GetPrefix(this);
			description.Add("energyPrefix", EnergyIconHelper.GetPrefix(this));
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

	private string PackedImagePath => ImageHelper.GetImagePath("atlases/potion_atlas.sprites/" + base.Id.Entry.ToLowerInvariant() + ".tres");

	private string PackedOutlinePath => ImageHelper.GetImagePath("atlases/potion_outline_atlas.sprites/" + base.Id.Entry.ToLowerInvariant() + ".tres");

	public string ImagePath => PackedImagePath;

	public Texture2D Image => ResourceLoader.Load<Texture2D>(PackedImagePath, null, ResourceLoader.CacheMode.Reuse);

	public string? OutlinePath
	{
		get
		{
			if (!ResourceLoader.Exists(PackedOutlinePath))
			{
				return null;
			}
			return PackedOutlinePath;
		}
	}

	public Texture2D? Outline
	{
		get
		{
			if (OutlinePath == null)
			{
				return null;
			}
			return ResourceLoader.Load<Texture2D>(OutlinePath, null, ResourceLoader.CacheMode.Reuse);
		}
	}

	public abstract PotionRarity Rarity { get; }

	public abstract PotionUsage Usage { get; }

	public abstract TargetType TargetType { get; }

	public PotionPoolModel Pool => ModelDb.AllPotionPools.First((PotionPoolModel p) => p.AllPotionIds.Contains(base.Id));

	/// <summary>
	/// Get the player that owns this relic.
	/// Will technically be null on a canonical relic model, but we should never be checking that, so we leave this as
	/// non-nullable for convenience.
	/// </summary>
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
				throw new InvalidOperationException("Cannot move potion " + base.Id.Entry + " from one owner to another");
			}
			_owner = value;
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

	protected virtual IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public bool IsQueued { get; private set; }

	/// <summary>
	/// Whether or not using this potion can heal the player or their pets (like Fruit Juice), or do other restricted
	/// actions (like Fairy in a Bottle's resurrection).
	///
	/// Used primarily to filter potions out of random in-combat generation effects, to avoid making annoying
	/// "optimal play" behavior.
	/// </summary>
	public virtual bool CanBeGeneratedInCombat => true;

	/// <summary>
	/// Whether or not this potion passes its custom usability check.
	/// By default, there are no custom checks, so this always returns true.
	/// Subclasses can override this to add custom checks.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Potions.FoulPotion" /> makes sure that we're either in combat, at the Merchant, or in the
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Events.FakeMerchant" /> event.
	/// </example>
	public virtual bool PassesCustomUsabilityCheck => true;

	public HoverTip HoverTip
	{
		get
		{
			HoverTip result = new HoverTip(Title, DynamicDescription);
			result.SetCanonicalModel(CanonicalInstance);
			return result;
		}
	}

	public IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTip }.Concat(ExtraHoverTips);

	public virtual IEnumerable<IHoverTip> ExtraHoverTips => Array.Empty<IHoverTip>();

	public PotionModel CanonicalInstance
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

	public override bool ShouldReceiveCombatHooks => true;

	/// <summary>
	/// Set to true when this potion is removed from a player's potion belt.
	/// </summary>
	public bool HasBeenRemovedFromState { get; private set; }

	public event Action? BeforeUse;

	public PotionModel ToMutable()
	{
		AssertCanonical();
		PotionModel potionModel = (PotionModel)MutableClone();
		potionModel.CanonicalInstance = this;
		return potionModel;
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		HasBeenRemovedFromState = false;
		this.BeforeUse = null;
	}

	public void Discard()
	{
		Owner.DiscardPotionInternal(this);
		HasBeenRemovedFromState = true;
	}

	/// <summary>
	/// Remove this potion as part of using it.
	/// </summary>
	public void RemoveBeforeUse()
	{
		Owner.RemoveUsedPotionInternal(this);
		HasBeenRemovedFromState = true;
	}

	public void EnqueueManualUse(Creature? target)
	{
		AssertMutable();
		this.BeforeUse?.Invoke();
		if (target == null && IsValidTarget(Owner.Creature))
		{
			target = Owner.Creature;
		}
		UsePotionAction action = new UsePotionAction(this, target, CombatManager.Instance.IsInProgress);
		IsQueued = true;
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
	}

	/// <summary>
	/// Returns true if target is valid for this potion.
	/// NOTE: This operates differently than cards! Do not try to unify this with CardModel.IsValidTarget unless you
	/// change UI targeting; namely, CardModel's TargetType.Self does not pass a target, whereas potions do.
	/// </summary>
	public bool IsValidTarget(Creature? target)
	{
		if (target == null)
		{
			if (TargetType == TargetType.TargetedNoCreature)
			{
				return true;
			}
			return !TargetType.IsSingleTarget();
		}
		if (!target.IsAlive)
		{
			return false;
		}
		if (TargetType == TargetType.AnyEnemy)
		{
			return target.Side != Owner.Creature.Side;
		}
		if (TargetType == TargetType.AnyAlly)
		{
			if (target.Side == Owner.Creature.Side)
			{
				return target != Owner.Creature;
			}
			return false;
		}
		if (TargetType == TargetType.AnyPlayer)
		{
			return target.IsPlayer;
		}
		if (TargetType == TargetType.Self)
		{
			return target == Owner.Creature;
		}
		return false;
	}

	public async Task OnUseWrapper(PlayerChoiceContext choiceContext, Creature? target)
	{
		RemoveBeforeUse();
		ICombatState combatState = Owner.Creature.CombatState;
		choiceContext.PushModel(this);
		await CombatManager.Instance.WaitForUnpause();
		await Hook.BeforePotionUsed(Owner.RunState, combatState, this, target);
		if (TestMode.IsOff && combatState != null)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(Owner.Creature);
			Vector2 targetPosition = Vector2.Zero;
			if (TargetType.IsSingleTarget())
			{
				targetPosition = (NCombatRoom.Instance?.GetCreatureNode(target))?.GetBottomOfHitbox() ?? Vector2.Zero;
			}
			else
			{
				IReadOnlyList<Creature> readOnlyList = ((TargetType != TargetType.AllEnemies) ? (from c in combatState.GetCreaturesOnSide(CombatSide.Player)
					where c.IsHittable
					select c).ToList() : (from c in combatState.GetCreaturesOnSide(CombatSide.Enemy)
					where c.IsHittable
					select c).ToList());
				foreach (Creature item in readOnlyList)
				{
					targetPosition += (NCombatRoom.Instance?.GetCreatureNode(item))?.VfxSpawnPosition ?? Vector2.Zero;
				}
				targetPosition /= (float)readOnlyList.Count;
			}
			Vector2 sourcePosition = nCreature?.VfxSpawnPosition ?? Vector2.Zero;
			NItemThrowVfx child = NItemThrowVfx.Create(sourcePosition, targetPosition, Image);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
			await Cmd.Wait(0.5f);
		}
		CombatManager.Instance.BeginCardOrPotionEffect(Owner);
		try
		{
			await OnUse(choiceContext, target);
		}
		finally
		{
			CombatManager.Instance.EndCardOrPotionEffect(Owner);
		}
		InvokeExecutionFinished();
		if (combatState != null && CombatManager.Instance.IsInProgress)
		{
			CombatManager.Instance.History.PotionUsed(combatState, this, target);
		}
		await Hook.AfterPotionUsed(Owner.RunState, combatState, this, target);
		Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(Owner.NetId).PotionUsed.Add(base.Id);
		await CombatManager.Instance.CheckForEmptyHand(choiceContext, Owner);
		choiceContext.PopModel(this);
	}

	public void AfterUsageCanceled()
	{
		IsQueued = false;
	}

	protected virtual Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		return Task.CompletedTask;
	}

	public SerializablePotion ToSerializable(int slotIndex)
	{
		AssertMutable();
		return new SerializablePotion
		{
			Id = base.Id,
			SlotIndex = slotIndex
		};
	}

	public static PotionModel FromSerializable(SerializablePotion save)
	{
		return SaveUtil.PotionOrDeprecated(save.Id).ToMutable();
	}

	/// <summary>
	/// Ensure that the target is valid (non-null) for a targeted potion.
	/// </summary>
	/// <exception cref="T:System.ArgumentNullException">Thrown if the target is null.</exception>
	protected static void AssertValidForTargetedPotion([NotNull] Creature? target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target", "Target must be present for targeted potions.");
		}
	}

	public bool CanThrowAtAlly()
	{
		if (TargetType == TargetType.AnyPlayer && Owner.RunState.Players.Count > 1)
		{
			return CombatManager.Instance.IsInProgress;
		}
		return false;
	}
}
