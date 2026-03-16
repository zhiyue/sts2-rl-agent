using System;

namespace MegaCrit.Sts2.Core.Models.Exceptions;

public class CanonicalModelException : Exception
{
	public CanonicalModelException(Type t)
		: base($"Canonical model of type {t} used in incorrect place.")
	{
	}
}
