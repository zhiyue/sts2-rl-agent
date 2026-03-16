using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class ChooseABundleScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NChooseABundleSelectionScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NChooseABundleSelectionScreen");
		NChooseABundleSelectionScreen screen = AutoSlayer.GetCurrentScreen<NChooseABundleSelectionScreen>();
		List<NCardBundle> list = UiHelper.FindAll<NCardBundle>(screen);
		if (list.Count == 0)
		{
			AutoSlayLog.Warn("No bundles found in bundle selection screen");
			return;
		}
		NCardBundle nCardBundle = random.NextItem(list);
		AutoSlayLog.Action("Selecting card bundle");
		await UiHelper.Click(nCardBundle.Hitbox);
		await Task.Delay(500, ct);
		NConfirmButton nConfirmButton = UiHelper.FindFirst<NConfirmButton>(screen);
		if (nConfirmButton != null)
		{
			AutoSlayLog.Action("Confirming bundle selection");
			await UiHelper.Click(nConfirmButton);
			await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(), ct, TimeSpan.FromSeconds(10L), "Bundle selection screen did not close after confirmation");
		}
		AutoSlayLog.ExitScreen("NChooseABundleSelectionScreen");
	}
}
