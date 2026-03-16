using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class ChooseACardScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NChooseACardSelectionScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NChooseACardSelectionScreen");
		NChooseACardSelectionScreen currentScreen = AutoSlayer.GetCurrentScreen<NChooseACardSelectionScreen>();
		List<NCardHolder> list = UiHelper.FindAll<NCardHolder>(currentScreen);
		if (list.Count == 0)
		{
			AutoSlayLog.Warn("No card holders found in choose-a-card screen");
			return;
		}
		NCardHolder nCardHolder = random.NextItem(list);
		AutoSlayLog.Action("Selecting card");
		nCardHolder.EmitSignal(NCardHolder.SignalName.Pressed, nCardHolder);
		await Task.Delay(100, ct);
		AutoSlayLog.ExitScreen("NChooseACardSelectionScreen");
	}
}
