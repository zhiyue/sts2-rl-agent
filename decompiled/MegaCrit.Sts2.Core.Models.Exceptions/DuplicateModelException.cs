using System;

namespace MegaCrit.Sts2.Core.Models.Exceptions;

public class DuplicateModelException : Exception
{
	public DuplicateModelException(Type t)
		: base($"Trying to create a duplicate canonical model of type {t}. Don't call constructors on models! Use ModelDb instead.")
	{
	}
}
