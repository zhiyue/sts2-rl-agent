namespace MegaCrit.Sts2.Core.Localization;

/// <summary>
/// Represents a validation error found in a localization override file.
/// </summary>
public record LocValidationError(string FilePath, string Key, string ErrorMessage);
