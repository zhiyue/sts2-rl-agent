using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class BarricadePower : PowerModel
{
	private const string _applierNameKey = "ApplierName";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new StringVar("ApplierName"));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		Creature applier2 = base.Applier;
		if (applier2 != null && applier2.IsMonster)
		{
			((StringVar)base.DynamicVars["ApplierName"]).StringValue = base.Applier.Monster.Title.GetFormattedText();
		}
		return Task.CompletedTask;
	}

	public override bool ShouldClearBlock(Creature creature)
	{
		if (base.Owner != creature)
		{
			return true;
		}
		return false;
	}
}
