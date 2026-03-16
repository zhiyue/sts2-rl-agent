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

	public IEnumerable<ModelId> AllPotionIds => _allPotionIds ?? (_allPotionIds = AllPotions.Select((PotionModel p) => p.Id).ToHashSet());

	protected abstract IEnumerable<PotionModel> GenerateAllPotions();

	public virtual IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		return AllPotions;
	}
}
