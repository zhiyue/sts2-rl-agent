using System;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.AutoSlay.Helpers;

public class Watchdog
{
	private DateTime _lastProgressTime;

	private DateTime _lastLogTime;

	private string _lastActivity = "Starting";

	public Watchdog()
	{
		Reset("Initialized");
	}

	public void Reset(string activity)
	{
		_lastProgressTime = DateTime.UtcNow;
		_lastActivity = activity;
	}

	public void Check()
	{
		TimeSpan timeSpan = DateTime.UtcNow - _lastProgressTime;
		if (timeSpan > AutoSlayConfig.watchdogLogInterval && DateTime.UtcNow - _lastLogTime > AutoSlayConfig.watchdogLogInterval)
		{
			_lastLogTime = DateTime.UtcNow;
			AutoSlayLog.Info($"[Watchdog] No progress for {timeSpan.TotalSeconds:F1}s (last: {_lastActivity})");
		}
		if (timeSpan > AutoSlayConfig.watchdogTimeout)
		{
			string value = DumpState();
			AutoSlayLog.Error($"[Watchdog] Stuck detected! No progress for {timeSpan.TotalSeconds:F1}s\nLast activity: {_lastActivity}\n{value}");
			throw new AutoSlayTimeoutException($"Watchdog timeout: No progress for {timeSpan.TotalSeconds:F1}s. Last activity: {_lastActivity}");
		}
	}

	public static string DumpState()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("=== AutoSlay State Dump ===");
		try
		{
			RunState runState = RunManager.Instance.DebugOnlyGetState();
			if (runState != null)
			{
				stringBuilder.AppendLine("Run State:");
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(24, 3, stringBuilder2);
				handler.AppendLiteral("  Floor: ");
				handler.AppendFormatted(runState.TotalFloor);
				handler.AppendLiteral(" (Act ");
				handler.AppendFormatted(runState.CurrentActIndex + 1);
				handler.AppendLiteral(", Floor ");
				handler.AppendFormatted(runState.ActFloor);
				handler.AppendLiteral(")");
				stringBuilder3.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder2);
				handler.AppendLiteral("  Room: ");
				handler.AppendFormatted(runState.CurrentRoom?.RoomType.ToString() ?? "null");
				stringBuilder4.AppendLine(ref handler);
			}
			else
			{
				stringBuilder.AppendLine("Run State: Not initialized");
			}
			if (NOverlayStack.Instance != null)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder2);
				handler.AppendLiteral("Overlay Stack: ");
				handler.AppendFormatted(NOverlayStack.Instance.ScreenCount);
				handler.AppendLiteral(" screens");
				stringBuilder5.AppendLine(ref handler);
				IOverlayScreen overlayScreen = NOverlayStack.Instance.Peek();
				if (overlayScreen != null)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder6 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
					handler.AppendLiteral("  Current: ");
					handler.AppendFormatted(overlayScreen.GetType().Name);
					stringBuilder6.AppendLine(ref handler);
				}
			}
			Node instance = NGame.Instance;
			if (instance != null)
			{
				stringBuilder.AppendLine("Scene Container:");
				Node nodeOrNull = instance.GetNodeOrNull("RootSceneContainer");
				if (nodeOrNull != null)
				{
					foreach (Node child in nodeOrNull.GetChildren())
					{
						string value = ((child is Control control) ? (control.Visible ? "visible" : "hidden") : "");
						StringBuilder stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder7 = stringBuilder2;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(10, 3, stringBuilder2);
						handler.AppendLiteral("  - ");
						handler.AppendFormatted(child.Name);
						handler.AppendLiteral(" (");
						handler.AppendFormatted(child.GetType().Name);
						handler.AppendLiteral(") [");
						handler.AppendFormatted(value);
						handler.AppendLiteral("]");
						stringBuilder7.AppendLine(ref handler);
					}
				}
			}
		}
		catch (Exception ex)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(21, 1, stringBuilder2);
			handler.AppendLiteral("Error dumping state: ");
			handler.AppendFormatted(ex.Message);
			stringBuilder8.AppendLine(ref handler);
		}
		stringBuilder.AppendLine("=== End State Dump ===");
		return stringBuilder.ToString();
	}
}
