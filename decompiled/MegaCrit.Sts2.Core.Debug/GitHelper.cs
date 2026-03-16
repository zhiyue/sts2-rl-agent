using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Debug;

public static class GitHelper
{
	public static Task<string?>? ShortCommitIdTask { get; private set; }

	public static string? ShortCommitId { get; private set; }

	public static async Task Initialize()
	{
		ShortCommitIdTask = Task.Run((Func<string>)GetCommitId);
		ShortCommitId = await ShortCommitIdTask;
	}

	private static string? GetCommitId()
	{
		if (!OS.HasFeature("editor"))
		{
			return null;
		}
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = "git",
			Arguments = "rev-parse --short HEAD",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true
		};
		using Process process = Process.Start(startInfo);
		if (process == null)
		{
			Log.Error("Error: Unable to start git process to get the git commit id.");
			return null;
		}
		string text = process.StandardOutput.ReadToEnd();
		process.WaitForExit();
		return text.Trim();
	}
}
