using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class ChooseARelicScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NChooseARelicSelection);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NChooseARelicSelection");
		NChooseARelicSelection currentScreen = AutoSlayer.GetCurrentScreen<NChooseARelicSelection>();
		List<NClickableControl> list = UiHelper.FindAll<NClickableControl>(currentScreen);
		if (list.Count == 0)
		{
			AutoSlayLog.Warn("No clickable elements found in relic selection screen");
			return;
		}
		NClickableControl button = random.NextItem(list);
		AutoSlayLog.Action("Selecting relic");
		await UiHelper.Click(button);
		AutoSlayLog.ExitScreen("NChooseARelicSelection");
	}
}
