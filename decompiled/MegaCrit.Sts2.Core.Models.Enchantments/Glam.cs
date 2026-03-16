using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Glam : EnchantmentModel
{
	private const string _timesKey = "Times";

	private bool _usedThisCombat;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Times", 1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, base.DynamicVars["Times"]));

	private bool UsedThisCombat
	{
		get
		{
			return _usedThisCombat;
		}
		set
		{
			AssertMutable();
			_usedThisCombat = value;
		}
	}

	public override int EnchantPlayCount(int originalPlayCount)
	{
		if (UsedThisCombat)
		{
			return originalPlayCount;
		}
		return originalPlayCount + base.DynamicVars["Times"].IntValue;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (UsedThisCombat)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card != base.Card)
		{
			return Task.CompletedTask;
		}
		UsedThisCombat = true;
		base.Status = EnchantmentStatus.Disabled;
		return Task.CompletedTask;
	}
}
