using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;

public class TreasureRoomHandler : IRoomHandler, IHandler
{
	private const string _roomPath = "/root/Game/RootSceneContainer/Run/RoomContainer/TreasureRoom";

	public RoomType[] HandledTypes => new RoomType[1] { RoomType.Treasure };

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for treasure room");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		NTreasureRoom room = await WaitHelper.ForNode<NTreasureRoom>(root, "/root/Game/RootSceneContainer/Run/RoomContainer/TreasureRoom", ct);
		NClickableControl node = room.GetNode<NClickableControl>("Chest");
		AutoSlayLog.Action("Opening chest");
		await UiHelper.Click(node);
		await Task.Delay(1000, ct);
		List<NTreasureRoomRelicHolder> list = UiHelper.FindAll<NTreasureRoomRelicHolder>(room);
		foreach (NTreasureRoomRelicHolder item in list)
		{
			if (item.IsEnabled && item.Visible)
			{
				AutoSlayLog.Action("Picking up relic");
				await UiHelper.Click(item);
				await Task.Delay(500, ct);
			}
		}
		NProceedButton proceedButton = room.ProceedButton;
		await WaitHelper.Until(() => proceedButton.IsEnabled, ct, TimeSpan.FromSeconds(5L), "Proceed button not enabled after picking relics");
		AutoSlayLog.Action("Clicking proceed");
		await UiHelper.Click(proceedButton);
	}
}
