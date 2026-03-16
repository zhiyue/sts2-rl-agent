using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class DeckTransformScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NDeckTransformSelectScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NDeckTransformSelectScreen");
		NDeckTransformSelectScreen screen = AutoSlayer.GetCurrentScreen<NDeckTransformSelectScreen>();
		NCardGrid grid = UiHelper.FindFirst<NCardGrid>(screen);
		if (grid == null)
		{
			AutoSlayLog.Error("Card grid not found in transform screen");
			return;
		}
		Control previewContainer = screen.GetNodeOrNull<Control>("%PreviewContainer");
		NConfirmButton mainConfirmButton = screen.GetNodeOrNull<NConfirmButton>("Confirm");
		HashSet<NGridCardHolder> selectedCards = new HashSet<NGridCardHolder>();
		for (int i = 0; i < 10; i++)
		{
			ct.ThrowIfCancellationRequested();
			if (previewContainer?.Visible ?? false)
			{
				AutoSlayLog.Info("Preview container appeared after selecting cards");
				break;
			}
			if (mainConfirmButton != null && mainConfirmButton.IsEnabled)
			{
				AutoSlayLog.Action("Clicking main confirm button");
				await UiHelper.Click(mainConfirmButton);
				await Task.Delay(300, ct);
				await WaitHelper.Until(() => previewContainer?.Visible ?? false, ct, TimeSpan.FromSeconds(5L), "Preview container did not appear after confirm");
				break;
			}
			List<NGridCardHolder> list = (from c in UiHelper.FindAll<NGridCardHolder>(screen)
				where !selectedCards.Contains(c)
				select c).ToList();
			if (list.Count == 0)
			{
				AutoSlayLog.Warn("No more cards available to select");
				break;
			}
			NGridCardHolder nGridCardHolder = random.NextItem(list);
			selectedCards.Add(nGridCardHolder);
			AutoSlayLog.Action($"Selecting card to transform ({selectedCards.Count})");
			grid.EmitSignal(NCardGrid.SignalName.HolderPressed, nGridCardHolder);
			await Task.Delay(300, ct);
		}
		await WaitHelper.Until(() => previewContainer?.Visible ?? false, ct, TimeSpan.FromSeconds(5L), "Preview container did not appear");
		await Task.Delay(500, ct);
		NConfirmButton previewConfirmButton = previewContainer?.GetNodeOrNull<NConfirmButton>("Confirm");
		if (previewConfirmButton == null)
		{
			AutoSlayLog.Error("Preview confirm button not found");
			return;
		}
		await WaitHelper.Until(() => previewConfirmButton.IsEnabled, ct, TimeSpan.FromSeconds(5L), "Preview confirm button did not become enabled");
		AutoSlayLog.Action("Confirming transform");
		await UiHelper.Click(previewConfirmButton);
		await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(), ct, TimeSpan.FromSeconds(10L), "Transform screen did not close after confirmation");
		AutoSlayLog.ExitScreen("NDeckTransformSelectScreen");
	}
}
