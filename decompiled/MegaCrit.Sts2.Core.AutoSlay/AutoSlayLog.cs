using System;
using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.AutoSlay;

public static class AutoSlayLog
{
	private const string _prefix = "[AutoSlay]";

	private static StreamWriter? _fileWriter;

	private static readonly object _lock = new object();

	public static void OpenLogFile(string path)
	{
		lock (_lock)
		{
			_fileWriter?.Dispose();
			_fileWriter = new StreamWriter(path, append: false)
			{
				AutoFlush = true
			};
		}
	}

	public static void CloseLogFile()
	{
		lock (_lock)
		{
			_fileWriter?.Dispose();
			_fileWriter = null;
		}
	}

	public static void Info(string message)
	{
		string text = "[AutoSlay] " + message;
		Log.Info(text);
		WriteToFile("INFO", text);
	}

	public static void Warn(string message)
	{
		string text = "[AutoSlay] " + message;
		Log.Warn(text);
		WriteToFile("WARN", text);
	}

	public static void Error(string message)
	{
		string text = "[AutoSlay] " + message;
		Log.Error(text);
		WriteToFile("ERROR", text);
	}

	public static void Error(string message, Exception ex)
	{
		string text = $"{"[AutoSlay]"} {message}: {ex.Message}\n{ex.StackTrace}";
		Log.Error(text);
		WriteToFile("ERROR", text);
	}

	private static void WriteToFile(string level, string message)
	{
		lock (_lock)
		{
			_fileWriter?.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}");
		}
	}

	public static void RunStarted(string seed)
	{
		string text = Path.Combine(OS.GetUserDataDir(), "logs", "godot.log");
		Info("Starting run with seed=" + seed);
		Info("Godot log: " + text);
	}

	public static void RunCompleted(string seed)
	{
		Info("Run completed successfully with seed=" + seed);
	}

	public static void RunFailed(string seed, Exception ex)
	{
		Error("Run failed with seed=" + seed, ex);
	}

	public static void EnterRoom(RoomType type, int act, int floor)
	{
		Info($"Entering {type} room (Act {act + 1}, Floor {floor})");
	}

	public static void ExitRoom(RoomType type)
	{
		Info($"Finished {type} room");
	}

	public static void EnterScreen(string screenName)
	{
		Info("Handling screen: " + screenName);
	}

	public static void ExitScreen(string screenName)
	{
		Info("Finished screen: " + screenName);
	}

	public static void Action(string action)
	{
		Info("Action: " + action);
	}

	public static void StateSnapshot(RunState? runState)
	{
		if (runState == null)
		{
			Info("State: RunState is null");
			return;
		}
		Info($"State: Floor={runState.TotalFloor}, Room={runState.CurrentRoom?.RoomType}, Act={runState.CurrentActIndex + 1}");
	}
}
