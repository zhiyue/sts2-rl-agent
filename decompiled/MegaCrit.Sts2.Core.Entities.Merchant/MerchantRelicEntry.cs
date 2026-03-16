using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Entities.Merchant;

public sealed class MerchantRelicEntry : MerchantEntry
{
	private static readonly HashSet<RelicModel> _baseBlacklist = new HashSet<RelicModel>
	{
		ModelDb.Relic<TheCourier>(),
		ModelDb.Relic<OldCoin>()
	};

	public RelicModel? Model { get; private set; }

	public override bool IsStocked => Model != null;

	public MerchantRelicEntry(RelicRarity rarity, Player player)
		: base(player)
	{
		FillSlot(rarity);
	}

	public MerchantRelicEntry(RelicModel relic, Player player)
		: base(player)
	{
		SetModel(relic);
	}

	private void FillSlot(RelicRarity rarity, IEnumerable<RelicModel>? blacklist = null)
	{
		if (blacklist == null)
		{
			blacklist = Array.Empty<RelicModel>();
		}
		SetModel(RelicFactory.PullNextRelicFromBack(_player, rarity, blacklist.Concat(_baseBlacklist)).ToMutable());
	}

	public override void CalcCost()
	{
		_cost = (int)Math.Round((float)Model.MerchantCost * _player.PlayerRng.Shops.NextFloat(0.85f, 1.15f));
	}

	private void SetModel(RelicModel model)
	{
		model.AssertMutable();
		Model = model;
		CalcCost();
		SaveManager.Instance.MarkRelicAsSeen(Model);
	}

	protected override async Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost)
	{
		if (!ignoreCost)
		{
			await PlayerCmd.LoseGold(base.Cost, _player, GoldLossType.Spent);
		}
		_player.RunState.CurrentMapPointHistoryEntry?.GetEntry(_player.NetId).BoughtRelics.Add(Model.Id);
		await RelicCmd.Obtain(Model, _player);
		RunManager.Instance.RewardSynchronizer.SyncLocalGoldLost(base.Cost);
		RunManager.Instance.RewardSynchronizer.SyncLocalObtainedRelic(Model);
		return (true, (!ignoreCost) ? base.Cost : 0);
	}

	protected override void ClearAfterPurchase()
	{
		Model = null;
	}

	protected override void RestockAfterPurchase(MerchantInventory? inventory)
	{
		HashSet<RelicModel> blacklist = inventory?.RelicEntries.Select((MerchantRelicEntry e) => e.Model?.CanonicalInstance).OfType<RelicModel>().ToHashSet() ?? new HashSet<RelicModel>();
		FillSlot(RelicFactory.RollRarity(_player), blacklist);
	}
}
