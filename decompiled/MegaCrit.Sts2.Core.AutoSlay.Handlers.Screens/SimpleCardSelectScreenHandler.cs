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

public class SimpleCardSelectScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NSimpleCardSelectScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NSimpleCardSelectScreen");
		NSimpleCardSelectScreen screen = AutoSlayer.GetCurrentScreen<NSimpleCardSelectScreen>();
		NCardGrid grid = UiHelper.FindFirst<NCardGrid>(screen);
		if (grid == null)
		{
			AutoSlayLog.Error("Card grid not found in simple card select screen");
			return;
		}
		NConfirmButton confirmButton = screen.GetNodeOrNull<NConfirmButton>("%Confirm");
		HashSet<NGridCardHolder> selectedCards = new HashSet<NGridCardHolder>();
		for (int i = 0; i < 10; i++)
		{
			ct.ThrowIfCancellationRequested();
			if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
			{
				AutoSlayLog.Info("Screen auto-closed after selections");
				break;
			}
			if (confirmButton != null && confirmButton.IsEnabled)
			{
				AutoSlayLog.Action("Clicking confirm button");
				await UiHelper.Click(confirmButton);
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
			AutoSlayLog.Action($"Selecting card ({selectedCards.Count})");
			grid.EmitSignal(NCardGrid.SignalName.HolderPressed, nGridCardHolder);
			await Task.Delay(300, ct);
		}
		await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(), ct, TimeSpan.FromSeconds(5L), "Simple card select screen did not close");
		AutoSlayLog.ExitScreen("NSimpleCardSelectScreen");
	}
}
