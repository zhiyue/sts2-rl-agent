using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class TeaMaster : EventModel
{
	private const string _boneTeaCostKey = "BoneTeaCost";

	private const string _emberTeaCostKey = "EmberTeaCost";

	private const int _boneTeaCost = 50;

	private const int _emberTeaCost = 150;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[5]
	{
		new DynamicVar("BoneTeaCost", 50m),
		new DynamicVar("EmberTeaCost", 150m),
		new StringVar("BoneTeaDescription", ModelDb.Relic<BoneTea>().DynamicDescription.GetFormattedText()),
		new StringVar("EmberTeaDescription", ModelDb.Relic<EmberTea>().DynamicDescription.GetFormattedText()),
		new StringVar("TeaOfDiscourtesyDescription", ModelDb.Relic<TeaOfDiscourtesy>().DynamicDescription.GetFormattedText())
	});

	public override bool IsAllowed(RunState runState)
	{
		if (runState.CurrentActIndex < 2)
		{
			return runState.Players.All((Player p) => p.Gold >= 150);
		}
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = new List<EventOption>();
		if ((decimal)base.Owner.Gold >= base.DynamicVars["BoneTeaCost"].BaseValue)
		{
			list.Add(new EventOption(this, BoneTea, "TEA_MASTER.pages.INITIAL.options.BONE_TEA", HoverTipFactory.FromRelicExcludingItself<BoneTea>()));
		}
		else
		{
			list.Add(new EventOption(this, null, "TEA_MASTER.pages.INITIAL.options.BONE_TEA_LOCKED"));
		}
		if ((decimal)base.Owner.Gold >= base.DynamicVars["EmberTeaCost"].BaseValue)
		{
			list.Add(new EventOption(this, EmberTea, "TEA_MASTER.pages.INITIAL.options.EMBER_TEA", HoverTipFactory.FromRelicExcludingItself<EmberTea>()));
		}
		else
		{
			list.Add(new EventOption(this, null, "TEA_MASTER.pages.INITIAL.options.EMBER_TEA_LOCKED"));
		}
		list.Add(new EventOption(this, TeaOfDiscourtesy, "TEA_MASTER.pages.INITIAL.options.TEA_OF_DISCOURTESY", HoverTipFactory.FromRelicExcludingItself<TeaOfDiscourtesy>()));
		return list;
	}

	private async Task BoneTea()
	{
		await PlayerCmd.LoseGold(base.DynamicVars["BoneTeaCost"].BaseValue, base.Owner, GoldLossType.Spent);
		await RelicCmd.Obtain<BoneTea>(base.Owner);
		SetEventFinished(L10NLookup("TEA_MASTER.pages.DONE.description"));
	}

	private async Task EmberTea()
	{
		await PlayerCmd.LoseGold(base.DynamicVars["EmberTeaCost"].BaseValue, base.Owner, GoldLossType.Spent);
		await RelicCmd.Obtain<EmberTea>(base.Owner);
		SetEventFinished(L10NLookup("TEA_MASTER.pages.DONE.description"));
	}

	private async Task TeaOfDiscourtesy()
	{
		await RelicCmd.Obtain<TeaOfDiscourtesy>(base.Owner);
		SetEventFinished(L10NLookup("TEA_MASTER.pages.TEA_OF_DISCOURTESY.description"));
	}
}
