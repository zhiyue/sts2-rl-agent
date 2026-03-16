using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class CardRewardScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NCardRewardSelectionScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NCardRewardSelectionScreen");
		NCardRewardSelectionScreen screen = AutoSlayer.GetCurrentScreen<NCardRewardSelectionScreen>();
		await Task.Delay(400, ct);
		List<NCardHolder> list = UiHelper.FindAll<NCardHolder>(screen);
		if (list.Count == 0)
		{
			AutoSlayLog.Warn("No card holders found in card reward screen");
			return;
		}
		NCardHolder nCardHolder = random.NextItem(list);
		AutoSlayLog.Action("Selecting card reward");
		nCardHolder.EmitSignal(NCardHolder.SignalName.Pressed, nCardHolder);
		await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(), ct, TimeSpan.FromSeconds(10L), "Card reward screen did not close after selection");
		AutoSlayLog.ExitScreen("NCardRewardSelectionScreen");
	}
}
