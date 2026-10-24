using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models;

public abstract class RelicPoolModel : AbstractModel, IPoolModel
{
	private IEnumerable<RelicModel>? _relics;

	private HashSet<ModelId>? _allRelicIds;

	public abstract string EnergyColorName { get; }

	public virtual Color LabOutlineColor => StsColors.halfTransparentBlack;

	/// <summary>
	/// Get every relic in this pool (ignores Unlocks/Epoch state).
	/// </summary>
	public IEnumerable<RelicModel> AllRelics
	{
		get
		{
			if (_relics == null)
			{
				_relics = GenerateAllRelics();
				_relics = ModHelper.ConcatModelsFromMods(this, _relics);
			}
			return _relics;
		}
	}

	/// <summary>
	/// Get the ID of every relic in this pool (ignores Unlocks/Epoch state).
	/// </summary>
	public HashSet<ModelId> AllRelicIds => _allRelicIds ?? (_allRelicIds = AllRelics.Select((RelicModel c) => c.Id).ToHashSet());

	public override bool ShouldReceiveCombatHooks => false;

	/// <summary>
	/// Generates every relic in this pool (ignores Unlocks/Epoch state).
	/// Overridden in subclasses, but should only be called once by <see cref="P:MegaCrit.Sts2.Core.Models.RelicPoolModel.AllRelics" /> so it can be cached.
	/// </summary>
	protected abstract IEnumerable<RelicModel> GenerateAllRelics();

	/// <summary>
	/// Returns every relic in this pool that the player has unlocked.
	/// By default, this is just AllRelics, but can be overriden in subclasses to remove relics that should be locked
	/// under certain conditions.
	/// </summary>
	public virtual IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		return AllRelics;
	}
}
