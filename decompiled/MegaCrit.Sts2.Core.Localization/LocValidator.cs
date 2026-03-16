using SmartFormat.Core.Parsing;

namespace MegaCrit.Sts2.Core.Localization;

public static class LocValidator
{
	public static bool ValidateFormatString(string text, out string? errorMessage)
	{
		try
		{
			Parser parser = new Parser();
			parser.ParseFormat(text);
			errorMessage = null;
			return true;
		}
		catch (ParsingErrors parsingErrors)
		{
			errorMessage = parsingErrors.Message;
			return false;
		}
	}
}
