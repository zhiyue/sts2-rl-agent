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

public sealed class FakeVenerableTeaSet : RelicModel
{
	private bool _gainEnergyInNextCombat;

	public override RelicRarity Rarity => RelicRarity.Event;

	public override int MerchantCost => 50;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(1));

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

	public override async Task AfterEnergyReset(Player player)
	{
		if (base.Owner == player && GainEnergyInNextCombat)
		{
			Flash();
			await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
			GainEnergyInNextCombat = false;
		}
	}
}
