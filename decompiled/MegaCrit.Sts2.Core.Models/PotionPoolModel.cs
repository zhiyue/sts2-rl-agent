using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models;

public abstract class PotionPoolModel : AbstractModel, IPoolModel
{
	private IEnumerable<PotionModel>? _allPotions;

	private HashSet<ModelId>? _allPotionIds;

	public abstract string EnergyColorName { get; }

	public override bool ShouldReceiveCombatHooks => false;

	public virtual Color LabOutlineColor => StsColors.halfTransparentBlack;

	/// <summary>
	/// Get every potion in this pool (ignores Unlocks/Epoch state).
	/// </summary>
	public IEnumerable<PotionModel> AllPotions
	{
		get
		{
			if (_allPotions == null)
			{
				_allPotions = GenerateAllPotions();
				_allPotions = ModHelper.ConcatModelsFromMods(this, _allPotions);
			}
			return _allPotions;
		}
	}

	/// <summary>
	/// Get the ID of every potion in this pool (ignores Unlocks/Epoch state).
	/// </summary>
	public IEnumerable<ModelId> AllPotionIds => _allPotionIds ?? (_allPotionIds = AllPotions.Select((PotionModel p) => p.Id).ToHashSet());

	/// <summary>
	/// Generates every potion in this pool (ignores Unlocks/Epoch state).
	/// Overriden in subclasses, but should only be called once by <see cref="P:MegaCrit.Sts2.Core.Models.PotionPoolModel.AllPotions" /> so it can be cached.
	/// </summary>
	protected abstract IEnumerable<PotionModel> GenerateAllPotions();

	/// <summary>
	/// Returns every potion in this pool that the player has unlocked.
	/// By default, this is just AllPotions, but can be overriden in subclasses to remove potions that should be locked
	/// under certain conditions.
	/// </summary>
	public virtual IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		return AllPotions;
	}
}
