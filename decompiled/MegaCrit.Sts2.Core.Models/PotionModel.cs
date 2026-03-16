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

	public LocString Description => new LocString("potions", base.Id.Entry + ".description");

	public LocString SelectionScreenPrompt => new LocString("potions", base.Id.Entry + ".selectionScreenPrompt");

	public LocString StaticDescription => Description;

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

	public virtual bool CanBeGeneratedInCombat => true;

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

	public void RemoveBeforeUse()
	{
		Owner.RemoveUsedPotionInternal(this);
		HasBeenRemovedFromState = true;
	}

	public void EnqueueManualUse(Creature? target)
	{
		AssertMutable();
		this.BeforeUse?.Invoke();
		UsePotionAction action = new UsePotionAction(this, target, CombatManager.Instance.IsInProgress);
		IsQueued = true;
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
	}

	public async Task OnUseWrapper(PlayerChoiceContext choiceContext, Creature? target)
	{
		RemoveBeforeUse();
		CombatState combatState = Owner.Creature.CombatState;
		choiceContext.PushModel(this);
		await CombatManager.Instance.WaitForUnpause();
		await Hook.BeforePotionUsed(Owner.RunState, combatState, this, target);
		if (TestMode.IsOff && combatState != null)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
			Vector2 targetPosition = Vector2.Zero;
			if (TargetType.IsSingleTarget())
			{
				NCreature creatureNode2 = NCombatRoom.Instance.GetCreatureNode(target);
				targetPosition = creatureNode2.GetBottomOfHitbox();
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
					NCreature creatureNode3 = NCombatRoom.Instance.GetCreatureNode(item);
					targetPosition += creatureNode3.VfxSpawnPosition;
				}
				targetPosition /= (float)readOnlyList.Count;
			}
			NItemThrowVfx child = NItemThrowVfx.Create(creatureNode.VfxSpawnPosition, targetPosition, Image);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
			await Cmd.Wait(0.5f);
		}
		await OnUse(choiceContext, target);
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
