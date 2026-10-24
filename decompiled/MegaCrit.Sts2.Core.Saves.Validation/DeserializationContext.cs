using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.Saves.Validation;

/// <summary>
/// Collects validation errors during save deserialization.
/// Threaded through FromSerializable methods to accumulate structured errors with path tracking.
/// </summary>
public sealed class DeserializationContext
{
	private readonly List<ValidationError> _errors = new List<ValidationError>();

	private readonly Stack<string> _pathSegments = new Stack<string>();

	private string CurrentPath => string.Join(".", _pathSegments.Reverse());

	/// <summary>
	/// All validation errors collected so far, in insertion order.
	/// </summary>
	public IReadOnlyList<ValidationError> Errors => _errors;

	/// <summary>
	/// Whether any fatal errors have been recorded.
	/// </summary>
	public bool HasFatal => _errors.Any((ValidationError e) => e.IsFatal);

	/// <summary>
	/// Number of warning-severity errors.
	/// </summary>
	public int WarningCount => _errors.Count((ValidationError e) => !e.IsFatal);

	/// <summary>
	/// Number of fatal-severity errors.
	/// </summary>
	public int FatalCount => _errors.Count((ValidationError e) => e.IsFatal);

	/// <summary>
	/// Pushes a path segment onto the stack (e.g. "Players[0]", "Deck[3]", "Id").
	/// </summary>
	public void PushPath(string segment)
	{
		_pathSegments.Push(segment);
	}

	/// <summary>
	/// Pops the most recent path segment from the stack.
	/// </summary>
	public void PopPath()
	{
		_pathSegments.Pop();
	}

	/// <summary>
	/// Records a warning at the current path. Warnings indicate degraded data but the save is still loadable.
	/// </summary>
	public void Warn(string message)
	{
		_errors.Add(new ValidationError(ValidationSeverity.Warning, CurrentPath, message));
	}

	/// <summary>
	/// Records a fatal error at the current path. Fatal errors mean the save cannot be loaded.
	/// </summary>
	public void Fatal(string message)
	{
		_errors.Add(new ValidationError(ValidationSeverity.Fatal, CurrentPath, message));
	}
}
