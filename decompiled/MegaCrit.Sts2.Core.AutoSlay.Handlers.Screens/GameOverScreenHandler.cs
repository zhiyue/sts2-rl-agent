using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class GameOverScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NGameOverScreen);

	public TimeSpan Timeout => TimeSpan.FromMinutes(2L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NGameOverScreen");
		NGameOverScreen screen = AutoSlayer.GetCurrentScreen<NGameOverScreen>();
		NGameOverContinueButton continueButton = UiHelper.FindFirst<NGameOverContinueButton>(screen);
		if (continueButton == null)
		{
			AutoSlayLog.Error("Continue button not found on game over screen");
			return;
		}
		await WaitHelper.Until(() => continueButton.IsEnabled, ct, TimeSpan.FromSeconds(30L), "Continue button did not become enabled");
		AutoSlayLog.Action("Clicking continue button");
		await UiHelper.Click(continueButton);
		NReturnToMainMenuButton mainMenuButton = null;
		int waitCycles = 0;
		await WaitHelper.Until(delegate
		{
			if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
			{
				return true;
			}
			mainMenuButton = UiHelper.FindFirst<NReturnToMainMenuButton>(screen);
			waitCycles++;
			if (waitCycles % 20 == 0)
			{
				bool value = mainMenuButton != null;
				bool value2 = mainMenuButton?.Visible ?? false;
				bool value3 = mainMenuButton?.IsEnabled ?? false;
				AutoSlayLog.Info($"Waiting for main menu button: found={value}, visible={value2}, enabled={value3}");
				AutoSlayer.CurrentWatchdog?.Reset("Waiting for game over summary animation");
			}
			NReturnToMainMenuButton nReturnToMainMenuButton = mainMenuButton;
			return nReturnToMainMenuButton != null && nReturnToMainMenuButton.Visible && mainMenuButton.IsEnabled;
		}, ct, TimeSpan.FromSeconds(90L), "Main menu button did not become enabled");
		if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
		{
			AutoSlayLog.Action("Game over screen closed automatically");
			AutoSlayLog.ExitScreen("NGameOverScreen");
			return;
		}
		AutoSlayLog.Action("Clicking main menu button");
		await UiHelper.Click(mainMenuButton);
		await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(), ct, TimeSpan.FromSeconds(30L), "Game over screen did not close");
		AutoSlayLog.ExitScreen("NGameOverScreen");
	}
}
