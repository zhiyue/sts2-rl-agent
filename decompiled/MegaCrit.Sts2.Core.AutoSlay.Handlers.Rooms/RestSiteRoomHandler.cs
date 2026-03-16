using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;

public class RestSiteRoomHandler : IRoomHandler, IHandler
{
	private const string _roomPath = "/root/Game/RootSceneContainer/Run/RoomContainer/RestSiteRoom";

	public RoomType[] HandledTypes => new RoomType[1] { RoomType.RestSite };

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for rest site room");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		NRestSiteRoom room = await WaitHelper.ForNode<NRestSiteRoom>(root, "/root/Game/RootSceneContainer/Run/RoomContainer/RestSiteRoom", ct);
		List<NRestSiteButton> list = (from b in UiHelper.FindAll<NRestSiteButton>(room)
			where b.Option.IsEnabled
			select b).ToList();
		if (list.Count == 0)
		{
			AutoSlayLog.Warn("No clickable rest site buttons found");
			return;
		}
		NRestSiteButton nRestSiteButton = random.NextItem(list);
		AutoSlayLog.Action("Selecting rest site option: " + nRestSiteButton.Option.GetType().Name);
		await UiHelper.Click(nRestSiteButton);
		NProceedButton proceedButton = room.ProceedButton;
		await WaitHelper.Until(delegate
		{
			if (!proceedButton.IsEnabled)
			{
				NOverlayStack? instance2 = NOverlayStack.Instance;
				if (instance2 == null)
				{
					return false;
				}
				return instance2.ScreenCount > 0;
			}
			return true;
		}, ct, TimeSpan.FromSeconds(10L), "Rest site option did not respond");
		NOverlayStack? instance = NOverlayStack.Instance;
		if (instance != null && instance.ScreenCount > 0)
		{
			AutoSlayLog.Action("Overlay screen detected, deferring proceed to drain loop");
			return;
		}
		AutoSlayLog.Action("Clicking proceed");
		await UiHelper.Click(proceedButton);
	}
}
