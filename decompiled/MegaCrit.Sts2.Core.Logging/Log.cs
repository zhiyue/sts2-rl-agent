using System;

namespace MegaCrit.Sts2.Core.Logging;

public static class Log
{
	private static readonly Logger _logger = new Logger(null, LogType.Generic);

	public static string Timestamp => DateTime.UtcNow.ToString("HH:mm:ss");

	public static event Action<LogLevel, string, int>? LogCallback;

	public static void InvokeGlobalLogCallback(LogLevel logLevel, string log, int skipFrames)
	{
		Log.LogCallback?.Invoke(logLevel, log, skipFrames);
	}

	public static void Load(string text, int skipFrames = 2)
	{
		_logger.Load(text, skipFrames);
	}

	public static void Debug(string text, int skipFrames = 2)
	{
		_logger.Debug(text, skipFrames);
	}

	public static void VeryDebug(string text, int skipFrames = 2)
	{
		_logger.VeryDebug(text, skipFrames);
	}

	public static void Info(string text, int skipFrames = 2)
	{
		_logger.Info(text, skipFrames);
	}

	public static void Warn(string text, int skipFrames = 2)
	{
		_logger.Warn(text, skipFrames);
	}

	public static void Error(string text, int skipFrames = 2)
	{
		_logger.Error(text, skipFrames);
	}

	public static void LogMessage(LogLevel level, LogType type, string text, int skipFrames = 1)
	{
		_logger.LogMessage(level, type, text, skipFrames);
	}
}
