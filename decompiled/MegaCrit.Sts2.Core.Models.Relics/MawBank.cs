using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MawBank : RelicModel
{
	private bool _hasItemBeenBought;

	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(12));

	public override bool ShouldFlashOnPlayer => false;

	public override bool IsUsedUp => HasItemBeenBought;

	[SavedProperty]
	public bool HasItemBeenBought
	{
		get
		{
			return _hasItemBeenBought;
		}
		set
		{
			AssertMutable();
			_hasItemBeenBought = value;
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (base.Owner.RunState.BaseRoom == room && !HasItemBeenBought)
		{
			Flash();
			await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
		}
	}

	public override Task AfterItemPurchased(Player player, MerchantEntry itemPurchased, int goldSpent)
	{
		if (player != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (HasItemBeenBought)
		{
			return Task.CompletedTask;
		}
		if (goldSpent <= 0)
		{
			return Task.CompletedTask;
		}
		Flash();
		HasItemBeenBought = true;
		return Task.CompletedTask;
	}
}
