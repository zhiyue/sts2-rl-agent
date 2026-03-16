using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Entities.Merchant;

public sealed class MerchantPotionEntry : MerchantEntry
{
	public PotionModel? Model { get; private set; }

	public override bool IsStocked => Model != null;

	public MerchantPotionEntry(PotionModel potion, Player player)
		: base(player)
	{
		potion.AssertMutable();
		Model = potion;
		CalcCost();
		SaveManager.Instance.MarkPotionAsSeen(Model);
	}

	public MerchantPotionEntry(Player player)
		: base(player)
	{
		FillSlot(Array.Empty<PotionModel>());
	}

	private void FillSlot(IEnumerable<PotionModel> blacklist)
	{
		Model = PotionFactory.CreateRandomPotionOutOfCombat(_player, _player.PlayerRng.Shops, blacklist).ToMutable();
		CalcCost();
		SaveManager.Instance.MarkPotionAsSeen(Model);
	}

	private static int GetCost(PotionRarity rarity)
	{
		return rarity switch
		{
			PotionRarity.Rare => 100, 
			PotionRarity.Uncommon => 75, 
			_ => 50, 
		};
	}

	public override void CalcCost()
	{
		if (Model == null)
		{
			throw new InvalidOperationException("There is no item to purchase.");
		}
		_cost = GetCost(Model.Rarity);
		if (TestMode.IsOff)
		{
			_cost = (int)Mathf.Round((float)_cost * _player.PlayerRng.Shops.NextFloat(0.95f, 1.05f));
		}
	}

	protected override async Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost)
	{
		if (Model == null)
		{
			throw new InvalidOperationException("There is no item to purchase.");
		}
		PotionProcureResult potionProcureResult = await PotionCmd.TryToProcure(Model, _player);
		if (!potionProcureResult.success)
		{
			InvokePurchaseFailed((potionProcureResult.failureReason == PotionProcureFailureReason.NotAllowed) ? PurchaseStatus.FailureForbidden : PurchaseStatus.FailureSpace);
			return (false, 0);
		}
		if (!ignoreCost)
		{
			await PlayerCmd.LoseGold(base.Cost, _player, GoldLossType.Spent);
		}
		_player.RunState.CurrentMapPointHistoryEntry?.GetEntry(_player.NetId).BoughtPotions.Add(Model.Id);
		RunManager.Instance.RewardSynchronizer.SyncLocalGoldLost(base.Cost);
		RunManager.Instance.RewardSynchronizer.SyncLocalObtainedPotion(Model);
		return (true, (!ignoreCost) ? base.Cost : 0);
	}

	protected override void ClearAfterPurchase()
	{
		Model = null;
	}

	protected override void RestockAfterPurchase(MerchantInventory? inventory)
	{
		HashSet<PotionModel> hashSet = inventory?.PotionEntries.Select((MerchantPotionEntry e) => e.Model?.CanonicalInstance).OfType<PotionModel>().ToHashSet() ?? new HashSet<PotionModel>();
		FillSlot(Array.Empty<PotionModel>());
	}
}
