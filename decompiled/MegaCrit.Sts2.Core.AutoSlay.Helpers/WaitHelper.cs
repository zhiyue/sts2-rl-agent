using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.AutoSlay.Helpers;

public static class WaitHelper
{
	public static async Task Until(Func<bool> condition, CancellationToken ct, TimeSpan? timeout = null, string? timeoutMessage = null)
	{
		TimeSpan actualTimeout = timeout ?? AutoSlayConfig.nodeWaitTimeout;
		using CancellationTokenSource timeoutCts = new CancellationTokenSource(actualTimeout);
		using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
		try
		{
			while (!condition())
			{
				AutoSlayer.CurrentWatchdog?.Check();
				await Task.Delay(AutoSlayConfig.pollingInterval, linkedCts.Token);
			}
		}
		catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
		{
			string message = timeoutMessage ?? $"Condition not met after {actualTimeout.TotalSeconds}s";
			throw new AutoSlayTimeoutException(message);
		}
	}

	public static async Task<T> ForNode<T>(Node root, string nodePath, CancellationToken ct, TimeSpan? timeout = null) where T : Node
	{
		try
		{
			await Until(() => root.HasNode(nodePath), ct, timeout, $"Node {nodePath} of type {typeof(T).Name} not found");
		}
		catch (TimeoutException)
		{
			DumpSceneTreeContext(root, nodePath, "not found");
			throw;
		}
		T node = root.GetNode<T>(nodePath);
		Control control = node as Control;
		if (control != null)
		{
			try
			{
				await Until(() => control.Visible, ct, timeout, "Node " + nodePath + " not visible");
			}
			catch (TimeoutException)
			{
				DumpSceneTreeContext(root, nodePath, "not visible");
				throw;
			}
			NButton nButton = node as NButton;
			if (nButton != null)
			{
				try
				{
					await Until(() => nButton.IsEnabled, ct, timeout, "Button " + nodePath + " not enabled");
				}
				catch (TimeoutException)
				{
					DumpSceneTreeContext(root, nodePath, "not enabled");
					throw;
				}
			}
		}
		return node;
	}

	private static void DumpSceneTreeContext(Node root, string nodePath, string reason)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(35, 1, stringBuilder2);
		handler.AppendLiteral("[AutoSlay] Scene tree dump (node ");
		handler.AppendFormatted(reason);
		handler.AppendLiteral("):");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, stringBuilder2);
		handler.AppendLiteral("  Looking for: ");
		handler.AppendFormatted(nodePath);
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(11, 2, stringBuilder2);
		handler.AppendLiteral("  Root: ");
		handler.AppendFormatted(root.Name);
		handler.AppendLiteral(" (");
		handler.AppendFormatted(root.GetType().Name);
		handler.AppendLiteral(")");
		stringBuilder5.AppendLine(ref handler);
		int num = nodePath.LastIndexOf('/');
		string text = ((num > 0) ? nodePath.Substring(0, num) : "");
		string text2;
		if (num <= 0)
		{
			text2 = nodePath;
		}
		else
		{
			int num2 = num + 1;
			text2 = nodePath.Substring(num2, nodePath.Length - num2);
		}
		string value = text2;
		Node node = (string.IsNullOrEmpty(text) ? root : root.GetNodeOrNull(text));
		if (node == null)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(32, 1, stringBuilder2);
			handler.AppendLiteral("  Parent path '");
			handler.AppendFormatted(text);
			handler.AppendLiteral("' does not exist!");
			stringBuilder6.AppendLine(ref handler);
			string[] array = text.Split('/');
			Node node2 = root;
			string text3 = "";
			string[] array2 = array;
			foreach (string text4 in array2)
			{
				if (!string.IsNullOrEmpty(text4))
				{
					text3 = text3 + ((text3.Length > 0) ? "/" : "") + text4;
					Node nodeOrNull = node2.GetNodeOrNull(text4);
					if (nodeOrNull == null)
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder7 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
						handler.AppendLiteral("  Path broken at: ");
						handler.AppendFormatted(text3);
						stringBuilder7.AppendLine(ref handler);
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder8 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, stringBuilder2);
						handler.AppendLiteral("  Children of ");
						handler.AppendFormatted(node2.Name);
						handler.AppendLiteral(":");
						stringBuilder8.AppendLine(ref handler);
						DumpChildren(node2, stringBuilder, 2, 2);
						break;
					}
					node2 = nodeOrNull;
				}
			}
		}
		else
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder9 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(13, 2, stringBuilder2);
			handler.AppendLiteral("  Parent: ");
			handler.AppendFormatted(node.Name);
			handler.AppendLiteral(" (");
			handler.AppendFormatted(node.GetType().Name);
			handler.AppendLiteral(")");
			stringBuilder9.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder10 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(21, 1, stringBuilder2);
			handler.AppendLiteral("  Looking for child: ");
			handler.AppendFormatted(value);
			stringBuilder10.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder11 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, stringBuilder2);
			handler.AppendLiteral("  Children of ");
			handler.AppendFormatted(node.Name);
			handler.AppendLiteral(":");
			stringBuilder11.AppendLine(ref handler);
			DumpChildren(node, stringBuilder, 2, 3);
		}
		AutoSlayLog.Info(stringBuilder.ToString().TrimEnd());
	}

	private static void DumpChildren(Node node, StringBuilder sb, int depth, int maxDepth, string indent = "    ")
	{
		foreach (Node child in node.GetChildren())
		{
			string text = $"{child.Name} ({child.GetType().Name})";
			if (child is Control control)
			{
				text += (control.Visible ? " [visible]" : " [hidden]");
			}
			if (child is NButton nButton)
			{
				text += (nButton.IsEnabled ? " [enabled]" : " [disabled]");
			}
			StringBuilder stringBuilder = sb;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 2, stringBuilder);
			handler.AppendFormatted(indent);
			handler.AppendFormatted(text);
			stringBuilder2.AppendLine(ref handler);
			if (depth < maxDepth)
			{
				DumpChildren(child, sb, depth + 1, maxDepth, indent + "  ");
			}
			else if (child.GetChildCount() > 0)
			{
				stringBuilder = sb;
				StringBuilder stringBuilder3 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(17, 2, stringBuilder);
				handler.AppendFormatted(indent);
				handler.AppendLiteral("  ... (");
				handler.AppendFormatted(child.GetChildCount());
				handler.AppendLiteral(" children)");
				stringBuilder3.AppendLine(ref handler);
			}
		}
	}

	public static async Task ForTask(Task task, CancellationToken ct, TimeSpan? timeout = null, string? timeoutMessage = null)
	{
		TimeSpan actualTimeout = timeout ?? AutoSlayConfig.nodeWaitTimeout;
		using CancellationTokenSource timeoutCts = new CancellationTokenSource(actualTimeout);
		using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
		_ = 1;
		try
		{
			while (!task.IsCompleted)
			{
				AutoSlayer.CurrentWatchdog?.Check();
				Task task2 = Task.Delay(AutoSlayConfig.pollingInterval, linkedCts.Token);
				await Task.WhenAny(task, task2);
			}
			await task;
		}
		catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
		{
			string message = timeoutMessage ?? $"Task not completed after {actualTimeout.TotalSeconds}s";
			throw new AutoSlayTimeoutException(message);
		}
	}

	public static async Task WithTimeout(Func<CancellationToken, Task> action, TimeSpan timeout, CancellationToken ct)
	{
		using CancellationTokenSource timeoutCts = new CancellationTokenSource();
		using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, ct);
		Task task = action(linkedCts.Token);
		Task task2 = Task.Delay(timeout, linkedCts.Token);
		Task completedTask = await Task.WhenAny(task, task2);
		await linkedCts.CancelAsync();
		if (completedTask == task || ct.IsCancellationRequested)
		{
			await task;
			return;
		}
		throw new AutoSlayTimeoutException($"Operation timed out after {timeout.TotalSeconds}s");
	}
}
