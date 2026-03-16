using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class VenerableTeaSet : RelicModel
{
	private bool _gainEnergyInNextCombat;

	public override RelicRarity Rarity => RelicRarity.Common;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(2));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	[SavedProperty]
	public bool GainEnergyInNextCombat
	{
		get
		{
			return _gainEnergyInNextCombat;
		}
		set
		{
			AssertMutable();
			if (_gainEnergyInNextCombat != value)
			{
				_gainEnergyInNextCombat = value;
				base.Status = (_gainEnergyInNextCombat ? RelicStatus.Active : RelicStatus.Normal);
			}
		}
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is RestSiteRoom))
		{
			return Task.CompletedTask;
		}
		GainEnergyInNextCombat = true;
		return Task.CompletedTask;
	}

	public override Task AfterEnergyReset(Player player)
	{
		if (base.Owner != player)
		{
			return Task.CompletedTask;
		}
		if (!GainEnergyInNextCombat)
		{
			return Task.CompletedTask;
		}
		Flash();
		PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
		GainEnergyInNextCombat = false;
		return Task.CompletedTask;
	}
}
