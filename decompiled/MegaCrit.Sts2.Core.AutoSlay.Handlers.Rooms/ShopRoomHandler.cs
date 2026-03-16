using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;

public class ShopRoomHandler : IRoomHandler, IHandler
{
	private const string _roomPath = "/root/Game/RootSceneContainer/Run/RoomContainer/MerchantRoom";

	public RoomType[] HandledTypes => new RoomType[1] { RoomType.Shop };

	public TimeSpan Timeout => TimeSpan.FromSeconds(120L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for shop room");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		NMerchantRoom room = await WaitHelper.ForNode<NMerchantRoom>(root, "/root/Game/RootSceneContainer/Run/RoomContainer/MerchantRoom", ct);
		AutoSlayLog.Action("Opening merchant inventory");
		room.OpenInventory();
		await Task.Delay(500, ct);
		int maxAttempts = 50;
		int attempts = 0;
		while (attempts < maxAttempts)
		{
			ct.ThrowIfCancellationRequested();
			attempts++;
			List<NMerchantSlot> list = (from slot in room.Inventory.GetAllSlots()
				where !(slot is NMerchantCardRemoval)
				where slot.Entry.IsStocked && slot.Entry.EnoughGold
				select slot).ToList();
			if (list.Count == 0)
			{
				AutoSlayLog.Action("No more affordable items to buy");
				break;
			}
			NMerchantSlot nMerchantSlot = random.NextItem(list);
			AutoSlayLog.Action($"Buying item (cost: {nMerchantSlot.Entry.Cost})");
			await nMerchantSlot.Entry.OnTryPurchaseWrapper(room.Inventory.Inventory);
			await Task.Delay(300, ct);
		}
		NBackButton nBackButton = UiHelper.FindFirst<NBackButton>(room);
		if (nBackButton != null)
		{
			AutoSlayLog.Action("Closing inventory");
			await UiHelper.Click(nBackButton);
			await Task.Delay(300, ct);
		}
		NProceedButton proceedButton = room.ProceedButton;
		AutoSlayLog.Action("Clicking proceed");
		await UiHelper.Click(proceedButton);
	}
}
