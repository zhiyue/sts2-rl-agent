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

	public HashSet<ModelId> AllRelicIds => _allRelicIds ?? (_allRelicIds = AllRelics.Select((RelicModel c) => c.Id).ToHashSet());

	public override bool ShouldReceiveCombatHooks => false;

	protected abstract IEnumerable<RelicModel> GenerateAllRelics();

	public virtual IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		return AllRelics;
	}
}
