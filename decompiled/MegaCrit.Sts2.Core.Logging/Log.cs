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

	/// <summary>
	/// Prints to stdout. It should be used when loading operations are happening.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="skipFrames"></param>
	public static void Load(string text, int skipFrames = 2)
	{
		_logger.Load(text, skipFrames);
	}

	/// <summary>
	/// Prints to stdout. Debug information which is useful for debugging. It should be used for verbose text.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="skipFrames"></param>
	public static void Debug(string text, int skipFrames = 2)
	{
		_logger.Debug(text, skipFrames);
	}

	/// <summary>
	/// Prints to stdout. Debug information which is useful for debugging. It should be used for verbose text.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="skipFrames"></param>
	public static void VeryDebug(string text, int skipFrames = 2)
	{
		_logger.VeryDebug(text, skipFrames);
	}

	/// <summary>
	/// Prints to stdout. It should be used for general information which is useful for debugging.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="skipFrames"></param>
	public static void Info(string text, int skipFrames = 2)
	{
		_logger.Info(text, skipFrames);
	}

	/// <summary>
	/// Prints to stderr without a stacktrace. It should be used for non-critical issues which we should be aware of
	/// or could indicate an issue.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="skipFrames"></param>
	public static void Warn(string text, int skipFrames = 2)
	{
		_logger.Warn(text, skipFrames);
	}

	/// <summary>
	/// Prints a stacktrace to stderr. It should be used for critical issue which should also not block
	/// the continuation of the game.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="skipFrames"></param>
	public static void Error(string text, int skipFrames = 2)
	{
		_logger.Error(text, skipFrames);
	}

	public static void LogMessage(LogLevel level, LogType type, string text, int skipFrames = 1)
	{
		_logger.LogMessage(level, type, text, skipFrames);
	}
}
