using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class CallingBell : RelicModel
{
	private const string _relicsKey = "Relics";

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Relics", 3m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<CurseOfTheBell>();

	public override async Task AfterObtained()
	{
		await CardPileCmd.AddCurseToDeck<CurseOfTheBell>(base.Owner);
		await Cmd.Wait(0.75f);
		await RewardsCmd.OfferCustom(base.Owner, GenerateRewards());
	}

	private List<Reward> GenerateRewards()
	{
		int num2;
		Span<Reward> span;
		int num;
		if (TestMode.IsOn)
		{
			num = 3;
			List<Reward> list = new List<Reward>(num);
			CollectionsMarshal.SetCount(list, num);
			span = CollectionsMarshal.AsSpan(list);
			num2 = 0;
			span[num2] = new RelicReward(ModelDb.Relic<Anchor>().ToMutable(), base.Owner);
			num2++;
			span[num2] = new RelicReward(ModelDb.Relic<GremlinHorn>().ToMutable(), base.Owner);
			num2++;
			span[num2] = new RelicReward(ModelDb.Relic<MummifiedHand>().ToMutable(), base.Owner);
			return list;
		}
		num2 = 3;
		List<Reward> list2 = new List<Reward>(num2);
		CollectionsMarshal.SetCount(list2, num2);
		span = CollectionsMarshal.AsSpan(list2);
		num = 0;
		span[num] = new RelicReward(RelicRarity.Common, base.Owner);
		num++;
		span[num] = new RelicReward(RelicRarity.Uncommon, base.Owner);
		num++;
		span[num] = new RelicReward(RelicRarity.Rare, base.Owner);
		return list2;
	}
}
