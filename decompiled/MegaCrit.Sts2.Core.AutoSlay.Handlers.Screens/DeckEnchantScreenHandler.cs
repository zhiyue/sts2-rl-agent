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

public class DeckEnchantScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NDeckEnchantSelectScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NDeckEnchantSelectScreen");
		NDeckEnchantSelectScreen screen = AutoSlayer.GetCurrentScreen<NDeckEnchantSelectScreen>();
		List<NGridCardHolder> cards = new List<NGridCardHolder>();
		await WaitHelper.Until(delegate
		{
			cards = UiHelper.FindAll<NGridCardHolder>(screen);
			return cards.Count > 0;
		}, ct, TimeSpan.FromSeconds(5L), "No cards found in enchant screen");
		await Task.Delay(300, ct);
		int maxSelections = Math.Min(cards.Count, 5);
		List<NGridCardHolder> selectedCards = new List<NGridCardHolder>();
		Control singlePreviewContainer;
		Control multiPreviewContainer;
		NConfirmButton nodeOrNull;
		for (int i = 0; i < maxSelections; i++)
		{
			singlePreviewContainer = screen.GetNodeOrNull<Control>("%EnchantSinglePreviewContainer");
			multiPreviewContainer = screen.GetNodeOrNull<Control>("%EnchantMultiPreviewContainer");
			nodeOrNull = screen.GetNodeOrNull<NConfirmButton>("Confirm");
			if ((singlePreviewContainer?.Visible ?? false) || (multiPreviewContainer?.Visible ?? false) || (nodeOrNull != null && nodeOrNull.IsEnabled))
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
			AutoSlayLog.Action($"Selecting card to enchant ({cards.Count} cards available)");
			nGridCardHolder.EmitSignal(NCardHolder.SignalName.Pressed, nGridCardHolder);
			await Task.Delay(200, ct);
		}
		singlePreviewContainer = screen.GetNodeOrNull<Control>("%EnchantSinglePreviewContainer");
		multiPreviewContainer = screen.GetNodeOrNull<Control>("%EnchantMultiPreviewContainer");
		nodeOrNull = screen.GetNodeOrNull<NConfirmButton>("Confirm");
		Control control = singlePreviewContainer;
		if (control == null || !control.Visible)
		{
			Control control2 = multiPreviewContainer;
			if ((control2 == null || !control2.Visible) && nodeOrNull != null && nodeOrNull.IsEnabled)
			{
				AutoSlayLog.Action("Clicking main confirm button to show preview");
				await UiHelper.Click(nodeOrNull);
				await Task.Delay(200, ct);
			}
		}
		Control visiblePreviewContainer = null;
		await WaitHelper.Until(delegate
		{
			singlePreviewContainer = screen.GetNodeOrNull<Control>("%EnchantSinglePreviewContainer");
			multiPreviewContainer = screen.GetNodeOrNull<Control>("%EnchantMultiPreviewContainer");
			Control control3 = singlePreviewContainer;
			if (control3 != null && control3.Visible)
			{
				visiblePreviewContainer = singlePreviewContainer;
				return true;
			}
			Control control4 = multiPreviewContainer;
			if (control4 != null && control4.Visible)
			{
				visiblePreviewContainer = multiPreviewContainer;
				return true;
			}
			return false;
		}, ct, TimeSpan.FromSeconds(5L), "Enchant preview container did not appear");
		await Task.Delay(500, ct);
		NConfirmButton confirmButton = visiblePreviewContainer?.GetNodeOrNull<NConfirmButton>("Confirm");
		if (confirmButton == null)
		{
			List<NConfirmButton> source = UiHelper.FindAll<NConfirmButton>(screen);
			confirmButton = source.FirstOrDefault((NConfirmButton b) => b.IsEnabled);
		}
		if (confirmButton != null)
		{
			await WaitHelper.Until(() => confirmButton.IsEnabled, ct, TimeSpan.FromSeconds(5L), "Enchant confirm button did not become enabled");
			AutoSlayLog.Action("Confirming enchant");
			await UiHelper.Click(confirmButton);
			await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(), ct, TimeSpan.FromSeconds(10L), "Enchant screen did not close after confirmation");
		}
		else
		{
			AutoSlayLog.Error("No confirm button found on enchant screen");
		}
		AutoSlayLog.ExitScreen("NDeckEnchantSelectScreen");
	}
}
