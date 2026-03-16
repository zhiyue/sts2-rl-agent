using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

public class RewardsScreenHandler : IScreenHandler, IHandler
{
	public Type ScreenType => typeof(NRewardsScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NRewardsScreen");
		NRewardsScreen screen = AutoSlayer.GetCurrentScreen<NRewardsScreen>();
		HashSet<NRewardButton> attemptedButtons = new HashSet<NRewardButton>();
		while (true)
		{
			ct.ThrowIfCancellationRequested();
			bool hasPotionSlots = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState())?.HasOpenPotionSlots ?? false;
			NRewardButton nRewardButton = UiHelper.FindAll<NRewardButton>(screen).FirstOrDefault((NRewardButton b) => b.IsEnabled && !attemptedButtons.Contains(b) && (!(b.Reward is PotionReward) || hasPotionSlots));
			if (nRewardButton == null)
			{
				break;
			}
			attemptedButtons.Add(nRewardButton);
			AutoSlayLog.Action("Clicking reward button: " + (nRewardButton.Reward?.GetType().Name ?? "unknown"));
			await UiHelper.Click(nRewardButton);
			await Task.Delay(500, ct);
			IOverlayScreen overlayScreen = NOverlayStack.Instance?.Peek();
			if (overlayScreen != null && overlayScreen != screen)
			{
				AutoSlayLog.Action("Child screen opened, returning to drain loop");
				AutoSlayLog.ExitScreen("NRewardsScreen");
				return;
			}
		}
		NProceedButton nProceedButton = UiHelper.FindFirst<NProceedButton>(screen);
		if (nProceedButton != null)
		{
			AutoSlayLog.Action("Clicking proceed");
			await UiHelper.Click(nProceedButton);
			await WaitHelper.Until(() => !GodotObject.IsInstanceValid(screen) || NOverlayStack.Instance?.Peek() != screen || (NMapScreen.Instance?.IsOpen ?? false), ct, TimeSpan.FromSeconds(10L), "Rewards screen did not close or map did not open after clicking proceed");
		}
		AutoSlayLog.ExitScreen("NRewardsScreen");
	}
}
