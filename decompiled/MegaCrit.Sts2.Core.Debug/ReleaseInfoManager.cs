using System;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Debug;

public class ReleaseInfoManager
{
	private static ReleaseInfoManager? _instance;

	private const string _releaseInfoFileName = "release_info.json";

	public static ReleaseInfoManager Instance => _instance ?? (_instance = new ReleaseInfoManager());

	public ReleaseInfo? ReleaseInfo { get; }

	private ReleaseInfoManager()
	{
		ReleaseInfo = LoadConfig();
	}

	private ReleaseInfo? LoadConfig()
	{
		string[] possibleReleaseInfoPaths = GetPossibleReleaseInfoPaths();
		string[] array = possibleReleaseInfoPaths;
		foreach (string path in array)
		{
			string text = ProjectSettings.GlobalizePath(path);
			if (!Godot.FileAccess.FileExists(text))
			{
				continue;
			}
			Log.Info("Found release_info.json at: " + text);
			using Godot.FileAccess fileAccess = Godot.FileAccess.Open(text, Godot.FileAccess.ModeFlags.Read);
			if (fileAccess == null)
			{
				Log.Error("Failed to open file: " + text);
				continue;
			}
			try
			{
				string asText = fileAccess.GetAsText();
				return JsonSerializer.Deserialize(asText, ReleaseInfoJsonSerializerContext.Default.ReleaseInfo);
			}
			catch (JsonException ex)
			{
				Log.Error("Failed to deserialize release_info.json: " + ex.Message);
			}
			catch (Exception ex2)
			{
				Log.Error("Unexpected error reading release_info.json: " + ex2.Message);
			}
		}
		Log.Info("File `release_info.json` not found in any of the expected locations.");
		return null;
	}

	private static string[] GetPossibleReleaseInfoPaths()
	{
		string executablePath = OS.GetExecutablePath();
		string path = Path.GetDirectoryName(executablePath) ?? string.Empty;
		if (OS.GetName() == "macOS")
		{
			string path2 = Path.Combine(path, "..", "Resources");
			return new string[2]
			{
				Path.Combine(path2, "release_info.json"),
				Path.Combine(path, "release_info.json")
			};
		}
		return new string[1] { Path.Combine(path, "release_info.json") };
	}
}
