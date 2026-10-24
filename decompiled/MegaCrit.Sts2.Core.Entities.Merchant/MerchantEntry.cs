using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Entities.Merchant;

/// <summary>
/// Represents purchasable entry in the merchant shop.
/// Wrapped in an NMerchant_Node that manages the UI interactions.
/// </summary>
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
		if (!IsStocked)
		{
			InvokePurchaseFailed(PurchaseStatus.FailureOutOfStock);
			return false;
		}
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

	/// <summary>
	/// Subclass-specific logic for purchasing the item in the entry.
	/// </summary>
	/// <param name="inventory">Inventory that the entry is being purchased in.</param>
	/// <param name="ignoreCost">Whether or not the cost should be ignored</param>
	/// <returns>Tuple containing whether or not the purchase was successful and the amount of gold spent.</returns>
	protected abstract Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost);

	/// <summary>
	/// Subclass-specific logic for clearing the entry after a purchase.
	/// </summary>
	protected abstract void ClearAfterPurchase();

	/// <summary>
	/// Subclass-specific logic for restocking the entry after a purchase if the player has an effect like
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.TheCourier" />.
	/// </summary>
	protected abstract void RestockAfterPurchase(MerchantInventory? inventory);
}
