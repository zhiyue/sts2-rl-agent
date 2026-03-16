using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Entities.Merchant;

public abstract class MerchantEntry
{
	protected readonly Player _player;

	protected int _cost;

	public int Cost
	{
		get
		{
			decimal num = _cost;
			if (_player.RunState.CurrentRoom is MerchantRoom)
			{
				num = Hook.ModifyMerchantPrice(_player.RunState, _player, this, _cost);
			}
			return (int)num;
		}
	}

	public bool EnoughGold => Cost <= _player.Gold;

	public abstract bool IsStocked { get; }

	public event Action<PurchaseStatus, MerchantEntry>? PurchaseCompleted;

	public event Action<PurchaseStatus>? PurchaseFailed;

	public event Action? EntryUpdated;

	public void InvokePurchaseCompleted(MerchantEntry entry)
	{
		this.PurchaseCompleted?.Invoke(PurchaseStatus.Success, entry);
	}

	public void InvokePurchaseFailed(PurchaseStatus status)
	{
		this.PurchaseFailed?.Invoke(status);
	}

	protected MerchantEntry(Player player)
	{
		_player = player;
	}

	protected virtual void UpdateEntry()
	{
	}

	public void OnMerchantInventoryUpdated()
	{
		UpdateEntry();
		this.EntryUpdated?.Invoke();
	}

	public abstract void CalcCost();

	public async Task<bool> OnTryPurchaseWrapper(MerchantInventory? inventory, bool ignoreCost = false)
	{
		if (!EnoughGold && !ignoreCost)
		{
			InvokePurchaseFailed(PurchaseStatus.FailureGold);
			return false;
		}
		var (success, goldSpent) = await OnTryPurchase(inventory, ignoreCost);
		if (success)
		{
			if (_player.RunState.CurrentRoom is MerchantRoom && Hook.ShouldRefillMerchantEntry(_player.RunState, this, _player))
			{
				RestockAfterPurchase(inventory);
			}
			else
			{
				ClearAfterPurchase();
			}
			await Hook.AfterItemPurchased(_player.RunState, _player, this, goldSpent);
			InvokePurchaseCompleted(this);
		}
		return success;
	}

	protected abstract Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost);

	protected abstract void ClearAfterPurchase();

	protected abstract void RestockAfterPurchase(MerchantInventory? inventory);
}
