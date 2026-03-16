using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.Saves.Validation;

public sealed class DeserializationContext
{
	private readonly List<ValidationError> _errors = new List<ValidationError>();

	private readonly Stack<string> _pathSegments = new Stack<string>();

	private string CurrentPath => string.Join(".", _pathSegments.Reverse());

	public IReadOnlyList<ValidationError> Errors => _errors;

	public bool HasFatal => _errors.Any((ValidationError e) => e.IsFatal);

	public int WarningCount => _errors.Count((ValidationError e) => !e.IsFatal);

	public int FatalCount => _errors.Count((ValidationError e) => e.IsFatal);

	public void PushPath(string segment)
	{
		_pathSegments.Push(segment);
	}

	public void PopPath()
	{
		_pathSegments.Pop();
	}

	public void Warn(string message)
	{
		_errors.Add(new ValidationError(ValidationSeverity.Warning, CurrentPath, message));
	}

	public void Fatal(string message)
	{
		_errors.Add(new ValidationError(ValidationSeverity.Fatal, CurrentPath, message));
	}
}
