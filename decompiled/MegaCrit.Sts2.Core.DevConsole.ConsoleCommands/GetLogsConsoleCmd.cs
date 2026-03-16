using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class GetLogsConsoleCmd : AbstractConsoleCmd
{
	private const string _dateTimeSpecifier = "yyyy_MM_dd_HH_mm_ss";

	private const long _maxLogFileBytes = 8388608L;

	private const long _maxFeedbackCrashDumpBytes = 52428800L;

	private const long _maxCoreDumpBytes = 209715200L;

	public override string CmdName => "getlogs";

	public override string Args => "<name:string>";

	public override string Description => "Gathers logs, automatically zips them to a file containing 'name', and opens the directory containing the zip file.";

	public override bool IsNetworked => false;

	public override bool DebugOnly => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		string extraName = "";
		if (args.Length != 0)
		{
			extraName = "-" + args[0];
		}
		string bugReportPath = GetBugReportPath(extraName);
		TaskHelper.RunSafely(GrabLogs(bugReportPath));
		return new CmdResult(success: true, "Zipping files to '" + bugReportPath + "'...");
	}

	public static string GetBugReportPath(string extraName = "")
	{
		string userDataDir = OS.GetUserDataDir();
		if (userDataDir == null)
		{
			throw new InvalidOperationException("Unable to open the user data directory.");
		}
		string value = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
		return Path.Combine(userDataDir, $"BugReport{extraName}-{value}.zip");
	}

	public static async Task GrabLogs(string bugReportPath)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			bugReportPath = bugReportPath.Replace('/', '\\');
		}
		bool consoleVisible = NDevConsole.Instance.Visible;
		if (consoleVisible)
		{
			NDevConsole.Instance.HideConsole();
			await NDevConsole.Instance.ToSignal(NDevConsole.Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
			await NDevConsole.Instance.ToSignal(NDevConsole.Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		Image image = NDevConsole.Instance.GetViewport().GetTexture().GetImage();
		if (consoleVisible)
		{
			NDevConsole.Instance.ShowConsole();
		}
		await using (FileStream outputStream = new FileStream(bugReportPath, FileMode.CreateNew))
		{
			ZipFiles(outputStream, image.SavePngToBuffer());
		}
		Error error = OS.ShellShowInFileManager(ProjectSettings.GlobalizePath(bugReportPath));
		if (error != Error.Ok)
		{
			Log.Error($"Error {error}: Cannot open OS file manager. Files zipped to '{bugReportPath}'");
		}
		else
		{
			Log.Info("Files zipped to '" + bugReportPath + "'");
		}
	}

	public static void ZipFiles(Stream outputStream, byte[] screenshotBytes)
	{
		if (RunManager.Instance.IsInProgress && RunManager.Instance.CombatReplayWriter.IsRecordingReplay)
		{
			RunManager.Instance.WriteReplay(stopRecording: false);
		}
		string baseDir = OS.GetExecutablePath().GetBaseDir();
		string text = ProjectSettings.GlobalizePath(UserDataPathProvider.GetAccountScopedBasePath(""));
		string text2 = ProjectSettings.GlobalizePath("user://");
		string path = Path.Combine(text2, "logs");
		string text3 = Path.Combine(baseDir, "release_info.json");
		string path2 = Path.Combine(text2, "sentry", "reports");
		using ZipArchive archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true);
		if (Directory.Exists(path))
		{
			foreach (string item in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
			{
				string relativePath = Path.GetRelativePath(text2, item);
				ArchiveLogFile(item, archive, relativePath, 8388608L);
			}
		}
		List<string> list = new List<string>();
		if (Directory.Exists(path2))
		{
			list.AddRange(Directory.GetFiles(path2));
		}
		if (OS.GetName() == "Windows")
		{
			string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
			string path3 = Path.Combine(folderPath, "CrashDumps");
			if (Directory.Exists(path3))
			{
				string exeName = Path.GetFileNameWithoutExtension(OS.GetExecutablePath());
				list.AddRange(from f in Directory.GetFiles(path3)
					where Path.GetFileName(f).StartsWith(exeName, StringComparison.OrdinalIgnoreCase)
					select f);
			}
		}
		if (list.Count > 0)
		{
			string text4 = list.OrderByDescending(File.GetLastWriteTime).First();
			DateTimeOffset dateTimeOffset = File.GetLastWriteTime(text4);
			string text5 = "yyyy-MM-dd_HH-mm-ss";
			ArchiveFile(text4, archive, "crashes/crash_" + dateTimeOffset.ToString(text5) + Path.GetExtension(text4));
		}
		TryCollectLinuxCoreDump(archive);
		foreach (string allSaveFile in GetAllSaveFiles(text))
		{
			string entryName = "saves/" + Path.GetRelativePath(text, allSaveFile);
			if (Path.GetExtension(allSaveFile).Equals(".json", StringComparison.OrdinalIgnoreCase))
			{
				ArchiveLogFile(allSaveFile, archive, entryName, 0L);
			}
			else
			{
				ArchiveFile(allSaveFile, archive, entryName);
			}
		}
		if (File.Exists(text3))
		{
			ArchiveFile(text3, archive, "release_info.json");
		}
		ArchiveBytes(screenshotBytes, archive, "screenshot.png");
	}

	public static void ZipFeedbackLogs(Stream outputStream, int profileId)
	{
		if (RunManager.Instance.IsInProgress && RunManager.Instance.CombatReplayWriter.IsRecordingReplay)
		{
			RunManager.Instance.WriteReplay(stopRecording: false);
		}
		string baseDir = OS.GetExecutablePath().GetBaseDir();
		string text = ProjectSettings.GlobalizePath(UserDataPathProvider.GetAccountScopedBasePath(""));
		string text2 = ProjectSettings.GlobalizePath("user://");
		string path = Path.Combine(text2, "logs");
		string text3 = Path.Combine(baseDir, "release_info.json");
		string path2 = Path.Combine(text2, "sentry", "reports");
		using ZipArchive archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true);
		if (Directory.Exists(path))
		{
			foreach (string item in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
			{
				string relativePath = Path.GetRelativePath(text2, item);
				ArchiveLogFile(item, archive, relativePath, 8388608L);
			}
		}
		List<string> list = new List<string>();
		if (Directory.Exists(path2))
		{
			list.AddRange(Directory.GetFiles(path2));
		}
		if (OS.GetName() == "Windows")
		{
			string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
			string path3 = Path.Combine(folderPath, "CrashDumps");
			if (Directory.Exists(path3))
			{
				string exeName = Path.GetFileNameWithoutExtension(OS.GetExecutablePath());
				list.AddRange(from f in Directory.GetFiles(path3)
					where Path.GetFileName(f).StartsWith(exeName, StringComparison.OrdinalIgnoreCase)
					select f);
			}
		}
		if (list.Count > 0)
		{
			string text4 = list.OrderByDescending(File.GetLastWriteTime).First();
			DateTimeOffset dateTimeOffset = File.GetLastWriteTime(text4);
			string text5 = "yyyy-MM-dd_HH-mm-ss";
			long length = new FileInfo(text4).Length;
			if (length <= 52428800)
			{
				ArchiveFile(text4, archive, "crashes/crash_" + dateTimeOffset.ToString(text5) + Path.GetExtension(text4));
			}
			else
			{
				Log.Warn($"Crash dump too large for feedback upload ({length / 1048576} MB), skipping: {text4}");
			}
		}
		TryCollectLinuxCoreDump(archive);
		int num = 7;
		List<string> list2 = new List<string>(num);
		CollectionsMarshal.SetCount(list2, num);
		Span<string> span = CollectionsMarshal.AsSpan(list2);
		int num2 = 0;
		span[num2] = ProfileSaveManager.ProfilePath;
		num2++;
		span[num2] = "settings.save";
		num2++;
		span[num2] = ProgressSaveManager.GetProgressPathForProfile(profileId);
		num2++;
		span[num2] = RunSaveManager.GetRunSavePath(profileId, "current_run.save");
		num2++;
		span[num2] = RunSaveManager.GetRunSavePath(profileId, "current_run_mp.save");
		num2++;
		span[num2] = PrefsSaveManager.GetPrefsPath(profileId);
		num2++;
		span[num2] = Path.Combine(UserDataPathProvider.GetProfileDir(profileId), "replays/latest.mcr");
		List<string> list3 = list2;
		string path4 = Path.Combine(text, RunHistorySaveManager.GetHistoryPath(profileId));
		if (Directory.Exists(path4))
		{
			string text6 = Directory.EnumerateFiles(path4, "*", SearchOption.TopDirectoryOnly).OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
			if (text6 != null)
			{
				list3.Add(Path.GetRelativePath(text, text6));
			}
		}
		foreach (string item2 in list3)
		{
			string text7 = Path.Combine(text, item2);
			if (File.Exists(text7))
			{
				string entryName = "saves/" + item2;
				if (Path.GetExtension(text7).Equals(".json", StringComparison.OrdinalIgnoreCase))
				{
					ArchiveLogFile(text7, archive, entryName, 0L);
				}
				else
				{
					ArchiveFile(text7, archive, entryName);
				}
			}
		}
		if (File.Exists(text3))
		{
			ArchiveFile(text3, archive, "release_info.json");
		}
	}

	private static void TryCollectLinuxCoreDump(ZipArchive archive)
	{
		if (OS.GetName() != "Linux")
		{
			return;
		}
		string fileName = Path.GetFileName(OS.GetExecutablePath());
		try
		{
			using Process process = new Process();
			process.StartInfo = new ProcessStartInfo
			{
				FileName = "coredumpctl",
				Arguments = "info -1 --no-pager " + fileName,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			process.Start();
			Task<string> task = process.StandardOutput.ReadToEndAsync();
			if (!task.Wait(10000))
			{
				process.Kill();
				process.WaitForExit(5000);
				Log.Warn("coredumpctl info timed out after 10s");
			}
			else
			{
				string result = task.Result;
				process.WaitForExit();
				if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(result))
				{
					ZipArchiveEntry zipArchiveEntry = archive.CreateEntry("crashes/coredump_info.txt");
					using StreamWriter streamWriter = new StreamWriter(zipArchiveEntry.Open());
					streamWriter.Write(result);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Warn("Could not collect coredumpctl info: " + ex.Message);
		}
		string text = Path.Combine(Path.GetTempPath(), $"sts2_coredump_{Guid.NewGuid()}.core");
		try
		{
			using Process process2 = new Process();
			process2.StartInfo = new ProcessStartInfo
			{
				FileName = "coredumpctl",
				Arguments = "dump -1 --no-pager -o \"" + text + "\" " + fileName,
				UseShellExecute = false
			};
			process2.Start();
			if (!process2.WaitForExit(10000))
			{
				Log.Warn("coredumpctl dump timed out after 10s, killing process");
				process2.Kill();
				process2.WaitForExit(5000);
			}
			else if (process2.ExitCode != 0)
			{
				Log.Warn($"coredumpctl dump exited with code {process2.ExitCode}");
			}
			if (File.Exists(text))
			{
				long length = new FileInfo(text).Length;
				if (length > 209715200)
				{
					Log.Warn($"Core dump is {length / 1048576} MB which exceeds the {200} MB limit, skipping");
				}
				else if (length > 0)
				{
					ArchiveFile(text, archive, "crashes/coredump.core");
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Warn("Could not collect core dump: " + ex2.Message);
		}
		finally
		{
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
		}
	}

	private static IEnumerable<string> GetAllSaveFiles(string accountBasePath)
	{
		if (!Directory.Exists(accountBasePath))
		{
			yield break;
		}
		foreach (string item in Directory.EnumerateFiles(accountBasePath, "*", SearchOption.AllDirectories))
		{
			yield return item;
		}
	}

	public static string ReadTailText(Stream stream, long maxBytes)
	{
		bool flag = false;
		if (maxBytes > 0 && stream.Length > maxBytes)
		{
			stream.Seek(stream.Length - maxBytes, SeekOrigin.Begin);
			while (stream.Position < stream.Length)
			{
				int num = stream.ReadByte();
				if (num == -1)
				{
					break;
				}
				if ((num & 0xC0) != 128)
				{
					stream.Seek(-1L, SeekOrigin.Current);
					break;
				}
			}
			flag = true;
		}
		using StreamReader streamReader = new StreamReader(stream, null, detectEncodingFromByteOrderMarks: true, -1, leaveOpen: true);
		string text = streamReader.ReadToEnd();
		if (flag)
		{
			int num2 = text.IndexOf('\n');
			if (num2 >= 0)
			{
				string text2 = text;
				int num3 = num2 + 1;
				text = text2.Substring(num3, text2.Length - num3);
			}
			text = $"[...truncated, showing last ~{maxBytes / 1048576} MB...]\n" + text;
		}
		return text;
	}

	private static void ArchiveLogFile(string file, ZipArchive archive, string entryName, long maxBytes)
	{
		entryName = entryName.Replace("\\", "/");
		ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(entryName);
		try
		{
			using FileStream stream = new FileStream(file, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
			string text = ReadTailText(stream, maxBytes);
			string value = LogSanitizer.Sanitize(text);
			using StreamWriter streamWriter = new StreamWriter(zipArchiveEntry.Open());
			streamWriter.Write(value);
		}
		catch (FileNotFoundException)
		{
			Log.Error("Could not find file for zipping: " + file);
		}
	}

	private static void ArchiveFile(string file, ZipArchive archive, string entryName)
	{
		entryName = entryName.Replace("\\", "/");
		ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(entryName);
		try
		{
			using FileStream fileStream = new FileStream(file, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
			using Stream destination = zipArchiveEntry.Open();
			fileStream.CopyTo(destination);
		}
		catch (FileNotFoundException)
		{
			Log.Error("Could not find file for zipping: " + file);
		}
	}

	private static void ArchiveBytes(byte[] bytes, ZipArchive archive, string entryName)
	{
		ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(entryName);
		using Stream stream = zipArchiveEntry.Open();
		stream.Write(bytes);
	}
}
