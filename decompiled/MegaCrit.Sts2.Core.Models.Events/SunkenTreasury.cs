using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class SunkenTreasury : EventModel
{
	private const string _smallChestGoldKey = "SmallChestGold";

	private const string _largeChestGoldKey = "LargeChestGold";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("SmallChestGold", 60m),
		new DynamicVar("LargeChestGold", 333m)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, FirstChest, "SUNKEN_TREASURY.pages.INITIAL.options.FIRST_CHEST"),
			new EventOption(this, SecondChest, "SUNKEN_TREASURY.pages.INITIAL.options.SECOND_CHEST", HoverTipFactory.FromCardWithCardHoverTips<Greed>())
		});
	}

	public override void CalculateVars()
	{
		base.DynamicVars["SmallChestGold"].BaseValue += (decimal)(base.Rng.NextInt(16) - 8);
		base.DynamicVars["LargeChestGold"].BaseValue += (decimal)(base.Rng.NextInt(61) - 30);
	}

	private async Task FirstChest()
	{
		await PlayerCmd.GainGold(base.DynamicVars["SmallChestGold"].BaseValue, base.Owner);
		SetEventFinished(L10NLookup("SUNKEN_TREASURY.pages.FIRST_CHEST.description"));
	}

	private async Task SecondChest()
	{
		await PlayerCmd.GainGold(base.DynamicVars["LargeChestGold"].BaseValue, base.Owner);
		await CardPileCmd.AddCurseToDeck<Greed>(base.Owner);
		LocString locString = L10NLookup("SUNKEN_TREASURY.pages.SECOND_CHEST.description");
		locString.Add("Monologue", new LocString("characters", base.Owner.Character.Id.Entry + ".goldMonologue"));
		SetEventFinished(locString);
	}
}
