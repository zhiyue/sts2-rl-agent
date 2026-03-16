using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class MapScreenHandler : IScreenHandler, IHandler
{
	private TaskCompletionSource? _roomEnteredTcs;

	public Type ScreenType => typeof(NMapScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NMapScreen");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		NRun runNode = root.GetNode<NRun>("/root/Game/RootSceneContainer/Run");
		await WaitHelper.Until(() => runNode.GlobalUi.MapScreen.IsVisibleInTree(), ct, AutoSlayConfig.mapScreenTimeout, "Map screen not visible");
		List<NMapPoint> source = UiHelper.FindAll<NMapPoint>(runNode.GlobalUi.MapScreen);
		RunState runState = RunManager.Instance.DebugOnlyGetState();
		NMapPoint nextRoom;
		if (runState.VisitedMapCoords.Count == 0)
		{
			AutoSlayLog.Action("Selecting first room");
			nextRoom = source.First((NMapPoint mp) => mp.Point.coord.row == 0);
		}
		else
		{
			IReadOnlyList<MapCoord> visitedMapCoords = runState.VisitedMapCoords;
			MapCoord lastCoord = visitedMapCoords[visitedMapCoords.Count - 1];
			NMapPoint nMapPoint = source.First((NMapPoint mp) => mp.Point.coord.Equals(lastCoord));
			MapPoint child = nMapPoint.Point.Children.First();
			nextRoom = source.First((NMapPoint mp) => mp.Point.coord.Equals(child.coord));
			AutoSlayLog.Action($"Selecting room at ({child.coord.row}, {child.coord.col})");
		}
		await WaitHelper.Until(() => nextRoom.IsEnabled, ct, TimeSpan.FromSeconds(10L), "Map point not enabled");
		_roomEnteredTcs = new TaskCompletionSource();
		RunManager.Instance.RoomEntered += OnRoomEntered;
		try
		{
			await UiHelper.Click(nextRoom);
			await WaitHelper.ForTask(_roomEnteredTcs.Task, ct, AutoSlayConfig.mapScreenTimeout, "Room not entered after map click");
		}
		finally
		{
			RunManager.Instance.RoomEntered -= OnRoomEntered;
			_roomEnteredTcs = null;
		}
		AutoSlayLog.ExitScreen("NMapScreen");
	}

	private void OnRoomEntered()
	{
		_roomEnteredTcs?.TrySetResult();
	}
}
