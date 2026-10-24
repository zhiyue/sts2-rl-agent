using SmartFormat.Core.Parsing;

namespace MegaCrit.Sts2.Core.Localization;

/// <summary>
/// Validates SmartFormat syntax in localization strings.
/// Used to catch format string errors early during override file loading.
/// </summary>
public static class LocValidator
{
	/// <summary>
	/// Validates a SmartFormat format string for syntax errors.
	/// </summary>
	/// <param name="text">The format string to validate</param>
	/// <param name="errorMessage">The error message if validation fails, or null if valid</param>
	/// <returns>True if the format string is syntactically valid, false otherwise</returns>
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
