using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Entities.Merchant;

public sealed class MerchantCardRemovalEntry : MerchantEntry
{
	public bool Used { get; private set; }

	public override bool IsStocked => !Used;

	public MerchantCardRemovalEntry(Player player)
		: base(player)
	{
		CalcCost();
	}

	public override void CalcCost()
	{
		_cost = 75 + 25 * _player.ExtraFields.CardShopRemovalsUsed;
	}

	public int CalcPriceIncrease()
	{
		decimal d = 25m;
		return (int)Math.Round(d);
	}

	public async Task<bool> OnTryPurchaseWrapper(MerchantInventory? inventory, bool ignoreCost = false, bool cancelable = true)
	{
		if (!base.EnoughGold && !ignoreCost)
		{
			InvokePurchaseFailed(PurchaseStatus.FailureGold);
			return false;
		}
		var (success, goldSpent) = await OnTryPurchase(inventory, ignoreCost, cancelable);
		if (success)
		{
			await Hook.AfterItemPurchased(_player.RunState, _player, this, goldSpent);
			InvokePurchaseCompleted(this);
		}
		return success;
	}

	protected override async Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost)
	{
		return await OnTryPurchase(inventory, ignoreCost, cancelable: true);
	}

	private async Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost, bool cancelable)
	{
		if (Used)
		{
			return (false, 0);
		}
		int goldToSpend = ((!ignoreCost) ? base.Cost : 0);
		bool flag = await RunManager.Instance.OneOffSynchronizer.DoLocalMerchantCardRemoval(goldToSpend, cancelable);
		if (flag)
		{
			NRun.Instance?.MerchantRoom?.Inventory.OnCardRemovalUsed();
		}
		return (flag, goldToSpend);
	}

	protected override void ClearAfterPurchase()
	{
	}

	protected override void RestockAfterPurchase(MerchantInventory? inventory)
	{
	}

	public void SetUsed()
	{
		Used = true;
	}
}
