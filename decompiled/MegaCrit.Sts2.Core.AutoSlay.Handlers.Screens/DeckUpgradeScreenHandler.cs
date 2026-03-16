using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class DeckUpgradeScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NDeckUpgradeSelectScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NDeckUpgradeSelectScreen");
		NDeckUpgradeSelectScreen screen = AutoSlayer.GetCurrentScreen<NDeckUpgradeSelectScreen>();
		List<NGridCardHolder> cards = UiHelper.FindAll<NGridCardHolder>(screen);
		if (cards.Count == 0)
		{
			AutoSlayLog.Warn("No cards found in upgrade screen");
			return;
		}
		int maxSelections = Math.Min(cards.Count, 5);
		for (int i = 0; i < maxSelections; i++)
		{
			if (!GodotObject.IsInstanceValid(screen))
			{
				break;
			}
			if (!screen.IsVisibleInTree())
			{
				break;
			}
			Control nodeOrNull = screen.GetNodeOrNull<Control>("%UpgradeSinglePreviewContainer");
			Control nodeOrNull2 = screen.GetNodeOrNull<Control>("%UpgradeMultiPreviewContainer");
			if ((nodeOrNull != null && nodeOrNull.Visible) || (nodeOrNull2 != null && nodeOrNull2.Visible))
			{
				break;
			}
			NGridCardHolder nGridCardHolder = random.NextItem(cards);
			AutoSlayLog.Action("Selecting card to upgrade");
			nGridCardHolder.EmitSignal(NCardHolder.SignalName.Pressed, nGridCardHolder);
			cards.Remove(nGridCardHolder);
			await Task.Delay(300, ct);
		}
		Control visiblePreview = null;
		await WaitHelper.Until(delegate
		{
			Control nodeOrNull3 = screen.GetNodeOrNull<Control>("%UpgradeSinglePreviewContainer");
			Control nodeOrNull4 = screen.GetNodeOrNull<Control>("%UpgradeMultiPreviewContainer");
			if (nodeOrNull3 != null && nodeOrNull3.Visible)
			{
				visiblePreview = nodeOrNull3;
				return true;
			}
			if (nodeOrNull4 != null && nodeOrNull4.Visible)
			{
				visiblePreview = nodeOrNull4;
				return true;
			}
			return !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree();
		}, ct, TimeSpan.FromSeconds(5L), "Upgrade preview did not appear");
		if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
		{
			AutoSlayLog.ExitScreen("NDeckUpgradeSelectScreen");
			return;
		}
		NConfirmButton confirmButton = visiblePreview?.GetNodeOrNull<NConfirmButton>("Confirm");
		if (confirmButton == null)
		{
			AutoSlayLog.Error("Preview confirm button not found");
			AutoSlayLog.ExitScreen("NDeckUpgradeSelectScreen");
			return;
		}
		await WaitHelper.Until(() => confirmButton.IsEnabled, ct, TimeSpan.FromSeconds(5L), "Upgrade confirm button did not become enabled");
		AutoSlayLog.Action("Confirming upgrade");
		await UiHelper.Click(confirmButton);
		await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(), ct, TimeSpan.FromSeconds(10L), "Upgrade screen did not close after confirmation");
		AutoSlayLog.ExitScreen("NDeckUpgradeSelectScreen");
	}
}
