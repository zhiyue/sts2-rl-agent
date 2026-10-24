namespace MegaCrit.Sts2.Core.Saves.Validation;

/// <summary>
/// A single validation error found during save deserialization.
/// </summary>
/// <param name="Severity">Whether this error is fatal or a warning</param>
/// <param name="Path">Dot-separated field path where the error occurred (e.g. "Players[0].Deck[3].Id")</param>
/// <param name="Message">Human-readable description of the error</param>
public sealed record ValidationError(ValidationSeverity Severity, string Path, string Message)
{
	/// <summary>
	/// Whether this error is fatal (the save cannot be loaded).
	/// </summary>
	public bool IsFatal => Severity == ValidationSeverity.Fatal;
}
