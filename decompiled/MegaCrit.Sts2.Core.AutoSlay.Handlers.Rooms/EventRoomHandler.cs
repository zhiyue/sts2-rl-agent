using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;

public class EventRoomHandler : IRoomHandler, IHandler
{
	private const string _roomPath = "/root/Game/RootSceneContainer/Run/RoomContainer/EventRoom";

	private const int _maxIterations = 50;

	public RoomType[] HandledTypes => new RoomType[1] { RoomType.Event };

	public TimeSpan Timeout => TimeSpan.FromMinutes(3L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for event room");
		Node eventRoom = await WaitForEventRoom(ct);
		if (await WaitForEventOptions(eventRoom, ct))
		{
			AutoSlayLog.Action("Event room completed");
			return;
		}
		int iterations = 0;
		while (iterations < 50)
		{
			ct.ThrowIfCancellationRequested();
			if (!GodotObject.IsInstanceValid(eventRoom) || !eventRoom.IsInsideTree())
			{
				RunState runState = RunManager.Instance.DebugOnlyGetState();
				if (runState != null && runState.CurrentRoomCount > 1)
				{
					AbstractRoom? baseRoom = runState.BaseRoom;
					if (baseRoom != null && baseRoom.RoomType == RoomType.Event)
					{
						AutoSlayLog.Action("Event triggered combat, handling combat first");
						await HandleEventCombat(ct);
						AutoSlayLog.Action("Combat finished, checking if event resumes");
						Node root = ((SceneTree)Engine.GetMainLoop()).Root;
						Node nodeOrNull = root.GetNodeOrNull("/root/Game/RootSceneContainer/Run/RoomContainer/EventRoom");
						if (nodeOrNull == null)
						{
							AutoSlayLog.Action("Event ended after combat (no event room)");
							break;
						}
						eventRoom = nodeOrNull;
						await Task.Delay(500, ct);
						List<NEventOptionButton> list = (from o in UiHelper.FindAll<NEventOptionButton>(eventRoom)
							where !o.Option.IsLocked
							select o).ToList();
						if (list.Count == 0)
						{
							AutoSlayLog.Action("Event finished after combat");
							break;
						}
						iterations++;
						continue;
					}
				}
				AutoSlayLog.Action("Event room no longer valid, exiting");
				break;
			}
			List<NEventOptionButton> list2 = (from o in UiHelper.FindAll<NEventOptionButton>(eventRoom)
				where !o.Option.IsLocked
				select o).ToList();
			if (list2.Count == 0)
			{
				break;
			}
			NEventOptionButton choice = random.NextItem(list2);
			AutoSlayLog.Action("Selecting event option: " + choice.Event.Id.Entry);
			await UiHelper.Click(choice);
			if (choice.Option.IsProceed)
			{
				AutoSlayLog.Action("Clicked proceed, exiting event");
				await WaitHelper.Until(() => !GodotObject.IsInstanceValid(eventRoom) || !eventRoom.IsInsideTree() || (NMapScreen.Instance?.IsOpen ?? false), ct, TimeSpan.FromSeconds(5L), "Event room did not close after clicking proceed");
				break;
			}
			await WaitHelper.Until(delegate
			{
				NOverlayStack? instance2 = NOverlayStack.Instance;
				if (instance2 != null && instance2.ScreenCount > 0)
				{
					return true;
				}
				NMapScreen? instance3 = NMapScreen.Instance;
				if (instance3 != null && instance3.IsOpen)
				{
					return true;
				}
				return !GodotObject.IsInstanceValid(eventRoom) || !eventRoom.IsInsideTree() || UiHelper.FindAll<NEventOptionButton>(eventRoom).Any((NEventOptionButton o) => !o.Option.IsLocked);
			}, ct, TimeSpan.FromSeconds(5L), "Event options did not reappear after choice");
			NOverlayStack? instance = NOverlayStack.Instance;
			if (instance != null && instance.ScreenCount > 0)
			{
				AutoSlayLog.Action("Overlay screen opened during event, deferring to drain loop");
				break;
			}
			iterations++;
		}
		if (iterations >= 50)
		{
			AutoSlayLog.Warn($"Event room hit iteration limit ({50})");
		}
		AutoSlayLog.Action("Event room completed");
	}

	private async Task HandleEventCombat(CancellationToken ct)
	{
		await WaitHelper.Until(() => CombatManager.Instance.IsInProgress, ct, AutoSlayConfig.nodeWaitTimeout, "Event combat not started");
		AutoSlayLog.Action("Event combat started, applying buffs and killing enemies");
		Creature player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState()).Creature;
		await PowerCmd.Apply<StrengthPower>(player, 100m, player, null);
		await PowerCmd.Apply<PlatingPower>(player, 100m, player, null);
		await PowerCmd.Apply<RegenPower>(player, 100m, player, null);
		int killAttempts = 0;
		int noEnemyWaitLoops = 0;
		while (CombatManager.Instance.IsInProgress && killAttempts < 20)
		{
			ct.ThrowIfCancellationRequested();
			CombatState combatState = CombatManager.Instance.DebugOnlyGetState();
			if (combatState == null)
			{
				AutoSlayLog.Info("Combat state became null, exiting kill loop");
				break;
			}
			foreach (Creature creature in combatState.Enemies)
			{
				List<PowerModel> list = creature.Powers.Where((PowerModel p) => p.ShouldStopCombatFromEnding()).ToList();
				foreach (PowerModel item in list)
				{
					AutoSlayLog.Info($"Removing blocking power {item.GetType().Name} from {creature}");
					await PowerCmd.Remove(item);
				}
			}
			List<Creature> list2 = combatState.Enemies.Where((Creature e) => e.IsAlive).ToList();
			if (list2.Count > 0)
			{
				AutoSlayLog.Action($"Killing {list2.Count} event combat enemies (attempt {killAttempts + 1})");
				await CreatureCmd.Kill(list2);
				killAttempts++;
				noEnemyWaitLoops = 0;
				await Task.Delay(1000, ct);
				await CombatManager.Instance.CheckWinCondition();
			}
			else
			{
				noEnemyWaitLoops++;
				if (noEnemyWaitLoops > 100)
				{
					AutoSlayLog.Info("Event combat still in progress but no enemies for 10s, breaking loop");
					break;
				}
				await Task.Delay(100, ct);
			}
		}
		if (CombatManager.Instance.IsInProgress)
		{
			await CombatManager.Instance.CheckWinCondition();
		}
		await WaitHelper.Until(() => !CombatManager.Instance.IsInProgress, ct, TimeSpan.FromSeconds(30L), "Event combat did not end");
		AutoSlayLog.Action("Event combat finished");
	}

	private async Task<Node> WaitForEventRoom(CancellationToken ct)
	{
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		return await WaitHelper.ForNode<Node>(root, "/root/Game/RootSceneContainer/Run/RoomContainer/EventRoom", ct);
	}

	private async Task<bool> WaitForEventOptions(Node eventRoom, CancellationToken ct)
	{
		NAncientEventLayout nAncientEventLayout = UiHelper.FindFirst<NAncientEventLayout>(eventRoom);
		if (nAncientEventLayout != null)
		{
			await HandleAncientEventDialogue(nAncientEventLayout, ct);
			return false;
		}
		NFakeMerchant nFakeMerchant = UiHelper.FindFirst<NFakeMerchant>(eventRoom);
		if (nFakeMerchant != null)
		{
			AutoSlayLog.Info("Detected custom event: FakeMerchant");
			await HandleFakeMerchantEvent(nFakeMerchant, ct);
			return true;
		}
		int waitCycles = 0;
		await WaitHelper.Until(delegate
		{
			waitCycles++;
			if (waitCycles % 50 == 0)
			{
				int childCount = eventRoom.GetChildCount();
				AutoSlayLog.Info($"Waiting for event options: {childCount} children in event room");
				AutoSlayer.CurrentWatchdog?.Reset("Waiting for event options to load");
			}
			return UiHelper.FindAll<NEventOptionButton>(eventRoom).Count > 0;
		}, ct, TimeSpan.FromSeconds(30L), "Event options not loaded");
		return false;
	}

	private async Task HandleFakeMerchantEvent(NFakeMerchant fakeMerchant, CancellationToken ct)
	{
		AutoSlayLog.Action("Handling FakeMerchant event");
		NProceedButton proceedButton = null;
		await WaitHelper.Until(delegate
		{
			proceedButton = UiHelper.FindFirst<NProceedButton>(fakeMerchant);
			return proceedButton != null && proceedButton.IsEnabled && proceedButton.Visible;
		}, ct, TimeSpan.FromSeconds(10L), "FakeMerchant proceed button not available");
		AutoSlayLog.Action("Clicking FakeMerchant proceed button");
		await UiHelper.Click(proceedButton);
	}

	private async Task HandleAncientEventDialogue(NAncientEventLayout ancientLayout, CancellationToken ct)
	{
		AutoSlayLog.Info("Detected Ancient event, clicking through dialogue");
		int clicks = 0;
		while (clicks < 50)
		{
			ct.ThrowIfCancellationRequested();
			if (!GodotObject.IsInstanceValid(ancientLayout))
			{
				break;
			}
			List<NEventOptionButton> list = (from b in UiHelper.FindAll<NEventOptionButton>(ancientLayout)
				where b.IsEnabled && !b.Option.IsLocked
				select b).ToList();
			if (list.Count > 0)
			{
				AutoSlayLog.Info($"Ancient dialogue finished, {list.Count} options available");
				break;
			}
			NButton nodeOrNull = ancientLayout.GetNodeOrNull<NButton>("%DialogueHitbox");
			if (nodeOrNull == null || !nodeOrNull.Visible || !nodeOrNull.IsEnabled)
			{
				await Task.Delay(100, ct);
				continue;
			}
			AutoSlayLog.Info($"Clicking Ancient dialogue (click {clicks + 1})");
			AutoSlayer.CurrentWatchdog?.Reset("Clicking Ancient event dialogue");
			nodeOrNull.EmitSignal(NClickableControl.SignalName.Released, nodeOrNull);
			clicks++;
			await Task.Delay(500, ct);
		}
		await WaitHelper.Until(() => UiHelper.FindAll<NEventOptionButton>(ancientLayout).Any((NEventOptionButton b) => b.IsEnabled && !b.Option.IsLocked), ct, TimeSpan.FromSeconds(10L), "Ancient event options did not become available after dialogue");
	}
}
