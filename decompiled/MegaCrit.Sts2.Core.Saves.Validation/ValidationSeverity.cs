namespace MegaCrit.Sts2.Core.Saves.Validation;

/// <summary>
/// Severity levels for save validation errors.
/// </summary>
public enum ValidationSeverity
{
	/// <summary>
	/// The save loaded but data was degraded (e.g. deprecated content replaced with fallback).
	/// </summary>
	Warning,
	/// <summary>
	/// The save cannot be loaded due to invalid or missing data.
	/// </summary>
	Fatal
}
