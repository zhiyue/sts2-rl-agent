using System;
using System.Diagnostics;
using System.IO;
using Godot;

namespace MegaCrit.Sts2.Core.Logging;

public class EditorLogPrinter : ILogPrinter
{
	private static string GetColorCode(LogLevel level)
	{
		return level switch
		{
			LogLevel.VeryDebug => "#B180D3", 
			LogLevel.Debug => "#B180D3", 
			LogLevel.Info => "#76FF56", 
			LogLevel.Warn => "#FFCB3D", 
			LogLevel.Error => "#FF4747", 
			LogLevel.Load => "#87CEEB", 
			_ => throw new ArgumentOutOfRangeException("level", level, null), 
		};
	}

	private static bool LogFullStack(LogLevel level)
	{
		return level switch
		{
			LogLevel.VeryDebug => false, 
			LogLevel.Debug => false, 
			LogLevel.Info => false, 
			LogLevel.Warn => false, 
			LogLevel.Error => true, 
			LogLevel.Load => false, 
			_ => throw new ArgumentOutOfRangeException("level", level, null), 
		};
	}

	public void Print(LogLevel level, string text, int skipFrames)
	{
		skipFrames++;
		string value = level.ToString().ToUpperInvariant();
		string colorCode = GetColorCode(level);
		string text2 = $"[b][color={colorCode}][{value}][/color][/b] {text}";
		string text3 = "";
		if (level != LogLevel.Warn)
		{
			if (LogFullStack(level))
			{
				StackTrace value2 = new StackTrace(skipFrames, fNeedFileInfo: true);
				text3 = $"\n{value2}";
			}
			else
			{
				StackFrame stackFrame = new StackFrame(skipFrames, needFileInfo: true);
				DiagnosticMethodInfo diagnosticMethodInfo = DiagnosticMethodInfo.Create(stackFrame);
				int fileLineNumber = stackFrame.GetFileLineNumber();
				string fileName = stackFrame.GetFileName();
				string fileName2 = Path.GetFileName(fileName);
				text3 = $" line {fileLineNumber}:{diagnosticMethodInfo?.Name}() in {fileName2}";
			}
		}
		if (level == LogLevel.Error)
		{
			string text4 = $"[{value}] {text}{text3}";
			GD.PrintErr(text4);
			GD.PushError(text4);
		}
		if (level == LogLevel.Warn)
		{
			GD.PrintRich(text2 ?? "");
		}
		else
		{
			GD.PrintRich(text2 + "[color=#606060]" + text3 + "[/color]");
		}
	}
}
