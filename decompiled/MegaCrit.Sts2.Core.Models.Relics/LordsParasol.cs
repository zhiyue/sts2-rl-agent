using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LordsParasol : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is MerchantRoom merchantRoom))
		{
			return Task.CompletedTask;
		}
		TaskHelper.RunSafely(PurchaseEverything(merchantRoom.Inventory));
		return Task.CompletedTask;
	}

	private async Task PurchaseEverything(MerchantInventory inventory)
	{
		if (inventory.Player != base.Owner)
		{
			return;
		}
		bool uiBlocked = false;
		try
		{
			if (TestMode.IsOff)
			{
				NRun.Instance.GlobalUi.TopBar.Map.Disable();
				NRun.Instance.GlobalUi.TopBar.Deck.Disable();
				NMapScreen.Instance.SetTravelEnabled(enabled: false);
				uiBlocked = true;
				NMerchantRoom.Instance.BlockInput();
				await Cmd.Wait(0.75f);
				NMerchantRoom.Instance.Inventory.Open();
				NHotkeyManager.Instance.AddBlockingScreen(NMerchantRoom.Instance.Inventory);
				await Cmd.Wait(1f);
			}
			foreach (MerchantCardEntry characterCardEntry in inventory.CharacterCardEntries)
			{
				await characterCardEntry.OnTryPurchaseWrapper(inventory, ignoreCost: true);
				await Cmd.Wait(0.25f);
			}
			foreach (MerchantCardEntry colorlessCardEntry in inventory.ColorlessCardEntries)
			{
				await colorlessCardEntry.OnTryPurchaseWrapper(inventory, ignoreCost: true);
				await Cmd.Wait(0.25f);
			}
			foreach (MerchantRelicEntry relicEntry in inventory.RelicEntries)
			{
				NRun.Instance.GlobalUi.TopBar.Map.Enable();
				NRun.Instance.GlobalUi.TopBar.Deck.Enable();
				await relicEntry.OnTryPurchaseWrapper(inventory, ignoreCost: true);
				NRun.Instance.GlobalUi.TopBar.Deck.Disable();
				NRun.Instance.GlobalUi.TopBar.Map.Disable();
				await Cmd.Wait(0.25f);
			}
			foreach (MerchantPotionEntry potionEntry in inventory.PotionEntries)
			{
				await potionEntry.OnTryPurchaseWrapper(inventory, ignoreCost: true);
				await Cmd.Wait(0.25f);
			}
		}
		finally
		{
			if (uiBlocked)
			{
				NHotkeyManager.Instance.RemoveBlockingScreen(NMerchantRoom.Instance.Inventory);
				NMerchantRoom.Instance.UnblockInput();
				NRun.Instance.GlobalUi.TopBar.Map.Enable();
				NRun.Instance.GlobalUi.TopBar.Deck.Enable();
				NMapScreen.Instance.SetTravelEnabled(enabled: true);
			}
		}
		if (inventory.CardRemovalEntry != null)
		{
			NMapScreen.Instance.SetTravelEnabled(enabled: false);
			await inventory.CardRemovalEntry.OnTryPurchaseWrapper(inventory, ignoreCost: true, cancelable: false);
			NMapScreen.Instance.SetTravelEnabled(enabled: true);
		}
	}
}
