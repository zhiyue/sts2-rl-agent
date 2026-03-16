using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class CrystalSphereScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NCrystalSphereScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(120L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NCrystalSphereScreen");
		NCrystalSphereScreen screen = AutoSlayer.GetCurrentScreen<NCrystalSphereScreen>();
		await Task.Delay(1000, ct);
		NProceedButton proceedButton = screen.GetNodeOrNull<NProceedButton>("%ProceedButton");
		if (proceedButton?.IsEnabled ?? false)
		{
			AutoSlayLog.Action("Clicking Crystal Sphere proceed button");
			await UiHelper.Click(proceedButton);
			await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree() || (NMapScreen.Instance?.IsVisibleInTree() ?? false), ct, TimeSpan.FromSeconds(10L), "Crystal Sphere screen did not close after clicking proceed");
			if (GodotObject.IsInstanceValid(screen) && screen.IsVisibleInTree())
			{
				NMapScreen instance = NMapScreen.Instance;
				if (instance != null && instance.IsVisibleInTree())
				{
					AutoSlayLog.Info("Map opened, manually removing Crystal Sphere screen from overlay stack");
					NOverlayStack.Instance?.Remove(screen);
					await Task.Delay(100, ct);
				}
			}
			AutoSlayLog.ExitScreen("NCrystalSphereScreen");
			return;
		}
		int maxClicks = 20;
		int clicks = 0;
		int lastClickableCount = int.MaxValue;
		int noProgressCount = 0;
		while (clicks < maxClicks)
		{
			ct.ThrowIfCancellationRequested();
			if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
			{
				break;
			}
			IOverlayScreen overlayScreen = NOverlayStack.Instance?.Peek();
			if (overlayScreen != null && overlayScreen != screen)
			{
				AutoSlayLog.Info("Child screen appeared (rewards), returning to drain loop");
				AutoSlayLog.ExitScreen("NCrystalSphereScreen");
				return;
			}
			proceedButton = screen.GetNodeOrNull<NProceedButton>("%ProceedButton");
			if (proceedButton?.IsEnabled ?? false)
			{
				break;
			}
			Control nodeOrNull = screen.GetNodeOrNull<Control>("%Cells");
			if (nodeOrNull == null)
			{
				await Task.Delay(100, ct);
				continue;
			}
			List<NCrystalSphereCell> list = (from c in UiHelper.FindAll<NCrystalSphereCell>(nodeOrNull)
				where c.Visible && c.Entity.IsHidden
				select c).ToList();
			if (list.Count == 0)
			{
				AutoSlayLog.Info("No more clickable cells, waiting for rewards or proceed");
				break;
			}
			if (list.Count >= lastClickableCount)
			{
				noProgressCount++;
				if (noProgressCount > 5)
				{
					AutoSlayLog.Info($"No progress after {noProgressCount} clicks, divinations likely exhausted");
					break;
				}
			}
			else
			{
				noProgressCount = 0;
			}
			lastClickableCount = list.Count;
			NCrystalSphereCell nCrystalSphereCell = random.NextItem(list);
			AutoSlayLog.Info($"Clicking crystal sphere cell at ({nCrystalSphereCell.Entity.X}, {nCrystalSphereCell.Entity.Y}), click {clicks + 1}, {list.Count} clickable");
			nCrystalSphereCell.EmitSignal(NClickableControl.SignalName.Released, nCrystalSphereCell);
			await Task.Delay(500, ct);
			clicks++;
		}
		await WaitHelper.Until(delegate
		{
			proceedButton = screen.GetNodeOrNull<NProceedButton>("%ProceedButton");
			NProceedButton nProceedButton = proceedButton;
			if (nProceedButton != null && nProceedButton.IsEnabled)
			{
				return true;
			}
			if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
			{
				return true;
			}
			IOverlayScreen overlayScreen3 = NOverlayStack.Instance?.Peek();
			return overlayScreen3 != null && overlayScreen3 != screen;
		}, ct, TimeSpan.FromSeconds(15L), "Crystal Sphere: neither proceed button nor rewards screen appeared");
		IOverlayScreen overlayScreen2 = NOverlayStack.Instance?.Peek();
		if (overlayScreen2 != null && overlayScreen2 != screen)
		{
			AutoSlayLog.Info("Rewards screen appeared, returning to drain loop");
			AutoSlayLog.ExitScreen("NCrystalSphereScreen");
			return;
		}
		if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
		{
			AutoSlayLog.ExitScreen("NCrystalSphereScreen");
			return;
		}
		proceedButton = screen.GetNodeOrNull<NProceedButton>("%ProceedButton");
		if (proceedButton != null && proceedButton.IsEnabled)
		{
			AutoSlayLog.Action("Clicking Crystal Sphere proceed button");
			await UiHelper.Click(proceedButton);
			await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree() || (NMapScreen.Instance?.IsVisibleInTree() ?? false), ct, TimeSpan.FromSeconds(10L), "Crystal Sphere screen did not close after clicking proceed");
			if (GodotObject.IsInstanceValid(screen) && screen.IsVisibleInTree())
			{
				NMapScreen instance2 = NMapScreen.Instance;
				if (instance2 != null && instance2.IsVisibleInTree())
				{
					AutoSlayLog.Info("Map opened, manually removing Crystal Sphere screen from overlay stack");
					NOverlayStack.Instance?.Remove(screen);
					await Task.Delay(100, ct);
				}
			}
		}
		AutoSlayLog.ExitScreen("NCrystalSphereScreen");
	}
}
