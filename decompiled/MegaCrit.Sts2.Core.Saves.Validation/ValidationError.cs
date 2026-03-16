namespace MegaCrit.Sts2.Core.Saves.Validation;

public sealed record ValidationError(ValidationSeverity Severity, string Path, string Message)
{
	public bool IsFatal => Severity == ValidationSeverity.Fatal;
}
