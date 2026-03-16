using System;

namespace MegaCrit.Sts2.Core.Saves;

public static class CorruptFileHandler
{
	public static string GenerateCorruptFilePath(string originalPath, ReadSaveStatus status)
	{
		int num = originalPath.LastIndexOf('/');
		if (num == -1)
		{
			num = originalPath.LastIndexOf('\\');
		}
		string text = ((num >= 0) ? originalPath.Substring(0, num) : "");
		string text2 = ((num >= 0) ? originalPath.Substring(num + 1) : originalPath);
		int num2 = text2.LastIndexOf('.');
		string value = ((num2 >= 0) ? text2.Substring(0, num2) : text2);
		string text3 = ((num2 >= 0) ? text2.Substring(num2) : "");
		long value2 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		string reasonChar = GetReasonChar(status);
		string text4 = $"{value}.{value2}.{reasonChar}.corrupt";
		if (string.IsNullOrEmpty(text))
		{
			return text4;
		}
		return text + "/" + text4;
	}

	private static string GetReasonChar(ReadSaveStatus status)
	{
		return status switch
		{
			ReadSaveStatus.JsonParseError => "JSN", 
			ReadSaveStatus.MissingSchemaVersion => "SCH", 
			ReadSaveStatus.FutureVersion => "FUT", 
			ReadSaveStatus.VersionTooOld => "OLD", 
			ReadSaveStatus.MigrationFailed => "MIG", 
			ReadSaveStatus.FileEmpty => "EMP", 
			ReadSaveStatus.FileAccessError => "ACC", 
			ReadSaveStatus.Unrecoverable => "UNR", 
			ReadSaveStatus.ValidationFailed => "VAL", 
			_ => "UNK", 
		};
	}
}
