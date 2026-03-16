using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class DeckCardSelectScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NDeckCardSelectScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NDeckCardSelectScreen");
		NDeckCardSelectScreen screen = AutoSlayer.GetCurrentScreen<NDeckCardSelectScreen>();
		List<NGridCardHolder> cards = new List<NGridCardHolder>();
		await WaitHelper.Until(delegate
		{
			cards = UiHelper.FindAll<NGridCardHolder>(screen);
			return cards.Count > 0;
		}, ct, TimeSpan.FromSeconds(5L), "No cards found in card select screen");
		await Task.Delay(300, ct);
		int maxSelections = Math.Min(cards.Count, 5);
		List<NGridCardHolder> selectedCards = new List<NGridCardHolder>();
		Control previewContainer;
		NConfirmButton nodeOrNull;
		for (int i = 0; i < maxSelections; i++)
		{
			previewContainer = screen.GetNodeOrNull<Control>("%PreviewContainer");
			nodeOrNull = screen.GetNodeOrNull<NConfirmButton>("%Confirm");
			if ((previewContainer?.Visible ?? false) || (nodeOrNull != null && nodeOrNull.IsEnabled))
			{
				break;
			}
			List<NGridCardHolder> list = cards.Where((NGridCardHolder c) => !selectedCards.Contains(c)).ToList();
			if (list.Count == 0)
			{
				break;
			}
			NGridCardHolder nGridCardHolder = random.NextItem(list);
			selectedCards.Add(nGridCardHolder);
			AutoSlayLog.Action($"Selecting card {i + 1} ({cards.Count} cards available)");
			nGridCardHolder.EmitSignal(NCardHolder.SignalName.Pressed, nGridCardHolder);
			await Task.Delay(200, ct);
		}
		previewContainer = screen.GetNodeOrNull<Control>("%PreviewContainer");
		nodeOrNull = screen.GetNodeOrNull<NConfirmButton>("%Confirm");
		Control control = previewContainer;
		if ((control == null || !control.Visible) && nodeOrNull != null && nodeOrNull.IsEnabled)
		{
			AutoSlayLog.Action("Clicking main confirm button to show preview");
			await UiHelper.Click(nodeOrNull);
			await Task.Delay(200, ct);
		}
		await WaitHelper.Until(delegate
		{
			previewContainer = screen.GetNodeOrNull<Control>("%PreviewContainer");
			return previewContainer?.Visible ?? false;
		}, ct, TimeSpan.FromSeconds(5L), "Preview container did not appear");
		await Task.Delay(500, ct);
		NConfirmButton confirmButton = previewContainer?.GetNodeOrNull<NConfirmButton>("%PreviewConfirm");
		if (confirmButton == null)
		{
			List<NConfirmButton> source = UiHelper.FindAll<NConfirmButton>(screen);
			confirmButton = source.FirstOrDefault((NConfirmButton b) => b.IsEnabled);
		}
		if (confirmButton != null)
		{
			await WaitHelper.Until(() => confirmButton.IsEnabled, ct, TimeSpan.FromSeconds(5L), "Card select confirm button did not become enabled");
			AutoSlayLog.Action("Confirming selection");
			await UiHelper.Click(confirmButton);
			await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(), ct, TimeSpan.FromSeconds(10L), "Card select screen did not close after confirmation");
		}
		else
		{
			AutoSlayLog.Error("No confirm button found on card select screen");
		}
		AutoSlayLog.ExitScreen("NDeckCardSelectScreen");
	}
}
