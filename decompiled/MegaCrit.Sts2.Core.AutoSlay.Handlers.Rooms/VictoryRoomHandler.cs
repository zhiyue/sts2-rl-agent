using System;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;

public class VictoryRoomHandler : IHandler
{
	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for victory room");
		await WaitHelper.Until(() => NCombatRoom.Instance != null, ct, AutoSlayConfig.nodeWaitTimeout, "Victory room not ready");
		NCombatRoom instance = NCombatRoom.Instance;
		NProceedButton proceedButton = instance.ProceedButton;
		await WaitHelper.Until(() => proceedButton.IsEnabled, ct, TimeSpan.FromSeconds(10L), "Proceed button not enabled");
		AutoSlayLog.Action("Clicking proceed on victory");
		await UiHelper.Click(proceedButton);
		AutoSlayLog.Action("Victory room completed");
	}
}
