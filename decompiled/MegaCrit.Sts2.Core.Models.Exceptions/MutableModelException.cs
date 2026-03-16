using System;

namespace MegaCrit.Sts2.Core.Models.Exceptions;

public class MutableModelException : Exception
{
	public MutableModelException(Type t)
		: base($"Mutable model of type {t} used in incorrect place.")
	{
	}
}
