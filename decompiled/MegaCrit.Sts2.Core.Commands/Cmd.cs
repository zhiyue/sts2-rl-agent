using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Commands;

public static class Cmd
{
	public static Task Wait(float seconds, bool ignoreCombatEnd = false)
	{
		return Wait(seconds, default(CancellationToken), ignoreCombatEnd);
	}

	public static async Task Wait(float seconds, CancellationToken cancelToken, bool ignoreCombatEnd = false)
	{
		if (!NonInteractiveMode.IsActive && !(seconds <= 0f) && (NGame.Instance == null || (SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant && (ignoreCombatEnd || !CombatManager.Instance.IsEnding))))
		{
			SceneTree sceneTree = (SceneTree)Engine.GetMainLoop();
			SceneTreeTimer timer = sceneTree.CreateTimer(seconds);
			await WaitInternal(timer, cancelToken);
		}
	}

	private static Task WaitInternal(SceneTreeTimer timer, CancellationToken cancellationToken)
	{
		TaskCompletionSource tcs = new TaskCompletionSource();
		timer.Timeout += Receive;
		if (cancellationToken.CanBeCanceled)
		{
			cancellationToken.Register(delegate
			{
				tcs.TrySetCanceled(cancellationToken);
			});
		}
		return tcs.Task;
		void Receive()
		{
			tcs.TrySetResult();
			timer.Timeout -= Receive;
		}
	}

	public static async Task CustomScaledWait(float fastSeconds, float standardSeconds, bool ignoreCombatEnd = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!NonInteractiveMode.IsActive && SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant && (ignoreCombatEnd || !CombatManager.Instance.IsEnding))
		{
			switch (SaveManager.Instance.PrefsSave.FastMode)
			{
			case FastModeType.Fast:
				await Wait(fastSeconds, cancellationToken, ignoreCombatEnd);
				break;
			case FastModeType.Normal:
				await Wait(standardSeconds, cancellationToken, ignoreCombatEnd);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case FastModeType.Instant:
				break;
			}
		}
	}
}
